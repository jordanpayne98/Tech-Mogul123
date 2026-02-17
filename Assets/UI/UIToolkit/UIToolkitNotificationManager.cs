using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIToolkitNotificationManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxNotifications = 50;
        [SerializeField] private bool showMonthNotifications = false;
        
        private UIDocument uiDocument;
        
        // Dropdown elements
        private Button notificationPanelBtn;
        private Label notificationBadge;
        private Label notificationSummaryTitle;
        private Label notificationSummaryText;
        private VisualElement notificationDropdown;
        private ScrollView notificationList;
        private VisualElement notificationEmptyState;
        private Button clearAllBtn;
        
        private List<NotificationData> notifications = new List<NotificationData>();
        private bool isDropdownOpen = false;
        private GameDate currentGameDate;
        private string expandedNotificationId = null;
        
        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        void OnEnable()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("UIDocument or root visual element is null");
                return;
            }
            
            CacheReferences();
            BindEvents();
            SubscribeToGameEvents();
            
            UpdateUI();
        }
        
        void OnDisable()
        {
            UnsubscribeFromGameEvents();
        }
        
        void CacheReferences()
        {
            var root = uiDocument.rootVisualElement;
            
            notificationPanelBtn = root.Q<Button>("notification-panel-btn");
            notificationBadge = root.Q<Label>("notification-badge");
            notificationSummaryTitle = root.Q<Label>("notification-summary-title");
            notificationSummaryText = root.Q<Label>("notification-summary-text");
            notificationDropdown = root.Q<VisualElement>("notification-dropdown");
            notificationList = root.Q<ScrollView>("notification-list");
            notificationEmptyState = root.Q<VisualElement>("notification-empty-state");
            clearAllBtn = root.Q<Button>("notification-clear-btn");
        }
        
        void BindEvents()
        {
            if (notificationPanelBtn != null)
            {
                notificationPanelBtn.clicked += ToggleDropdown;
            }
            
            if (clearAllBtn != null)
            {
                clearAllBtn.clicked += ClearAllNotifications;
            }
            
            // Close dropdown when clicking outside
            uiDocument.rootVisualElement.RegisterCallback<ClickEvent>(evt =>
            {
                if (isDropdownOpen && notificationDropdown != null)
                {
                    // Check if click is outside dropdown and button
                    if (!notificationDropdown.worldBound.Contains(evt.position) &&
                        !notificationPanelBtn.worldBound.Contains(evt.position))
                    {
                        CloseDropdown();
                    }
                }
            });
        }
        
        void SubscribeToGameEvents()
        {
            EventBus.Subscribe<OnGameStartedEvent>(HandleGameStarted);
            EventBus.Subscribe<OnBankruptcyEvent>(HandleBankruptcy);
            EventBus.Subscribe<OnInsufficientCashEvent>(HandleInsufficientCash);
            EventBus.Subscribe<OnDayTickEvent>(HandleDayTick);
            EventBus.Subscribe<OnMonthlyReportEvent>(HandleMonthlyReport);
            
            if (showMonthNotifications)
            {
                EventBus.Subscribe<OnMonthTickEvent>(HandleMonthTick);
            }
            
            EventBus.Subscribe<OnEmployeeHiredEvent>(HandleEmployeeHired);
            EventBus.Subscribe<OnEmployeeFiredEvent>(HandleEmployeeFired);
        }
        
        void UnsubscribeFromGameEvents()
        {
            EventBus.Unsubscribe<OnGameStartedEvent>(HandleGameStarted);
            EventBus.Unsubscribe<OnBankruptcyEvent>(HandleBankruptcy);
            EventBus.Unsubscribe<OnInsufficientCashEvent>(HandleInsufficientCash);
            EventBus.Unsubscribe<OnDayTickEvent>(HandleDayTick);
            EventBus.Unsubscribe<OnMonthlyReportEvent>(HandleMonthlyReport);
            
            if (showMonthNotifications)
            {
                EventBus.Unsubscribe<OnMonthTickEvent>(HandleMonthTick);
            }
            
            EventBus.Unsubscribe<OnEmployeeHiredEvent>(HandleEmployeeHired);
            EventBus.Unsubscribe<OnEmployeeFiredEvent>(HandleEmployeeFired);
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
            currentGameDate = evt.CurrentDate;
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            // Clear all old notifications when starting a new game
            notifications.Clear();
            CloseDropdown();
            
            // Add fresh "Game Started" notification
            AddNotification("Game Started! Build your tech empire.", NotificationType.Info);
        }
        
        void HandleBankruptcy(OnBankruptcyEvent evt)
        {
            AddNotification("Bankruptcy! Your company ran out of cash.", NotificationType.Error);
        }
        
        void HandleInsufficientCash(OnInsufficientCashEvent evt)
        {
            AddNotification($"Insufficient cash! Need ${evt.Required:N0}", NotificationType.Warning);
        }
        
        void HandleMonthTick(OnMonthTickEvent evt)
        {
            AddNotification($"Month {evt.Month}, {evt.Year}", NotificationType.Info);
        }
        
        void HandleEmployeeHired(OnEmployeeHiredEvent evt)
        {
            AddNotification($"Hired {evt.Name} as {evt.Role.roleName}", NotificationType.Success);
        }
        
        void HandleEmployeeFired(OnEmployeeFiredEvent evt)
        {
            AddNotification($"Fired {evt.Name}", NotificationType.Warning);
        }
        
        void HandleMonthlyReport(OnMonthlyReportEvent evt)
        {
            string[] monthNames = { 
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December" 
            };
            
            string monthName = evt.Month >= 1 && evt.Month <= 12 
                ? monthNames[evt.Month - 1] 
                : "Unknown";
            
            string message = $"{monthName} {evt.Year} Report";
            
            AddExpandableNotification(message, NotificationType.Info, evt.Report);
        }
        
        public void AddNotification(string message, NotificationType type)
        {
            GameDate notificationDate = currentGameDate ?? new GameDate { Year = 2024, Month = 1, Day = 1 };
            
            var notification = new NotificationData
            {
                Id = System.Guid.NewGuid().ToString(),
                Message = message,
                Type = type,
                GameDate = new GameDate 
                { 
                    Year = notificationDate.Year,
                    Month = notificationDate.Month,
                    Day = notificationDate.Day
                },
                IsExpandable = false,
                MonthlyReport = null
            };
            
            notifications.Insert(0, notification);
            
            if (notifications.Count > maxNotifications)
            {
                notifications.RemoveAt(notifications.Count - 1);
            }
            
            UpdateUI();
        }
        
        public void AddExpandableNotification(string message, NotificationType type, MonthlyReport report)
        {
            GameDate notificationDate = currentGameDate ?? new GameDate { Year = 2024, Month = 1, Day = 1 };
            
            var notification = new NotificationData
            {
                Id = System.Guid.NewGuid().ToString(),
                Message = message,
                Type = type,
                GameDate = new GameDate 
                { 
                    Year = notificationDate.Year,
                    Month = notificationDate.Month,
                    Day = notificationDate.Day
                },
                IsExpandable = true,
                MonthlyReport = report
            };
            
            notifications.Insert(0, notification);
            
            if (notifications.Count > maxNotifications)
            {
                notifications.RemoveAt(notifications.Count - 1);
            }
            
            UpdateUI();
        }
        
        void ToggleDropdown()
        {
            if (isDropdownOpen)
            {
                CloseDropdown();
            }
            else
            {
                OpenDropdown();
            }
        }
        
        void OpenDropdown()
        {
            isDropdownOpen = true;
            if (notificationDropdown != null)
            {
                notificationDropdown.AddToClassList("visible");
            }
        }
        
        void CloseDropdown()
        {
            isDropdownOpen = false;
            if (notificationDropdown != null)
            {
                notificationDropdown.RemoveFromClassList("visible");
            }
        }
        
        void ClearAllNotifications()
        {
            notifications.Clear();
            UpdateUI();
            CloseDropdown();
        }
        
        void UpdateUI()
        {
            UpdateBadge();
            UpdateSummary();
            UpdateDropdownList();
        }
        
        void UpdateBadge()
        {
            if (notificationBadge == null) return;
            
            int count = notifications.Count;
            
            if (count > 0)
            {
                notificationBadge.text = count > 99 ? "99+" : count.ToString();
                notificationBadge.RemoveFromClassList("hidden");
            }
            else
            {
                notificationBadge.AddToClassList("hidden");
            }
        }
        
        void UpdateSummary()
        {
            if (notificationSummaryTitle == null || notificationSummaryText == null) return;
            
            if (notifications.Count > 0)
            {
                var latest = notifications[0];
                notificationSummaryTitle.text = GetSummaryTitle(latest.Type);
                notificationSummaryText.text = latest.Message;
            }
            else
            {
                notificationSummaryTitle.text = "No Notifications";
                notificationSummaryTitle.AddToClassList("notification-summary-empty");
                notificationSummaryText.text = "";
            }
        }
        
        string GetSummaryTitle(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => "Success!",
                NotificationType.Warning => "Warning!",
                NotificationType.Error => "Error!",
                NotificationType.Info => "Info",
                _ => "Notification"
            };
        }
        
        void UpdateDropdownList()
        {
            if (notificationList == null) return;
            
            // Clear existing items
            notificationList.Clear();
            
            if (notifications.Count == 0)
            {
                notificationEmptyState?.RemoveFromClassList("hidden");
            }
            else
            {
                notificationEmptyState?.AddToClassList("hidden");
                
                // Add notification items
                foreach (var notification in notifications)
                {
                    var item = CreateNotificationListItem(notification);
                    notificationList.Add(item);
                }
            }
        }
        
        VisualElement CreateNotificationListItem(NotificationData notification)
        {
            var container = new VisualElement();
            container.AddToClassList("notification-list-item");
            
            if (notification.IsExpandable)
            {
                container.AddToClassList("expandable");
            }
            
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("notification-item-header");
            
            var icon = new Label(GetIconForType(notification.Type));
            icon.AddToClassList("notification-item-icon");
            icon.AddToClassList(notification.Type.ToString().ToLower());
            headerContainer.Add(icon);
            
            var content = new VisualElement();
            content.AddToClassList("notification-item-content");
            
            var message = new Label(notification.Message);
            message.AddToClassList("notification-item-message");
            content.Add(message);
            
            var time = new Label(FormatGameDate(notification.GameDate));
            time.AddToClassList("notification-item-time");
            content.Add(time);
            
            headerContainer.Add(content);
            
            if (notification.IsExpandable)
            {
                var expandIcon = new Label("▼");
                expandIcon.AddToClassList("notification-expand-icon");
                headerContainer.Add(expandIcon);
                
                headerContainer.RegisterCallback<ClickEvent>(evt =>
                {
                    ToggleNotificationExpansion(notification.Id, container, expandIcon);
                    evt.StopPropagation();
                });
            }
            
            container.Add(headerContainer);
            
            if (notification.IsExpandable && notification.MonthlyReport != null)
            {
                var details = CreateMonthlyReportDetails(notification.MonthlyReport);
                details.AddToClassList("notification-details");
                details.AddToClassList("hidden");
                container.Add(details);
            }
            
            return container;
        }
        
        VisualElement CreateMonthlyReportDetails(MonthlyReport report)
        {
            var details = new VisualElement();
            details.AddToClassList("monthly-report-details");
            
            AddReportRow(details, "Contracts Completed:", report.ContractsCompleted.ToString(), "success");
            if (report.ContractsFailed > 0)
            {
                AddReportRow(details, "Contracts Failed:", report.ContractsFailed.ToString(), "warning");
            }
            AddReportRow(details, "Products Released:", report.ProductsReleased.ToString(), "info");
            AddReportRow(details, "Average Morale:", $"{report.AverageMorale:F0}%", GetMoraleClass(report.AverageMorale));
            
            var separator = new VisualElement();
            separator.AddToClassList("report-separator");
            details.Add(separator);
            
            AddReportRow(details, "Money Earned:", $"${report.MoneyEarned:N0}", "positive");
            AddReportRow(details, "Money Spent:", $"${report.MoneySpent:N0}", "negative");
            
            var profitClass = report.Profit >= 0 ? "positive" : "negative";
            var profitLabel = report.Profit >= 0 ? "Profit:" : "Loss:";
            AddReportRow(details, profitLabel, $"${Mathf.Abs(report.Profit):N0}", profitClass);
            
            return details;
        }
        
        void AddReportRow(VisualElement parent, string label, string value, string valueClass = "")
        {
            var row = new VisualElement();
            row.AddToClassList("report-row");
            
            var labelElement = new Label(label);
            labelElement.AddToClassList("report-label");
            row.Add(labelElement);
            
            var valueElement = new Label(value);
            valueElement.AddToClassList("report-value");
            if (!string.IsNullOrEmpty(valueClass))
            {
                valueElement.AddToClassList(valueClass);
            }
            row.Add(valueElement);
            
            parent.Add(row);
        }
        
        string GetMoraleClass(float morale)
        {
            if (morale >= 80) return "success";
            if (morale >= 60) return "info";
            if (morale >= 40) return "warning";
            return "negative";
        }
        
        void ToggleNotificationExpansion(string notificationId, VisualElement container, Label expandIcon)
        {
            bool isExpanding = expandedNotificationId != notificationId;
            
            if (expandedNotificationId != null && expandedNotificationId != notificationId)
            {
                CollapseAllNotifications();
            }
            
            if (isExpanding)
            {
                expandedNotificationId = notificationId;
                var details = container.Q<VisualElement>(className: "notification-details");
                if (details != null)
                {
                    details.RemoveFromClassList("hidden");
                }
                expandIcon.text = "▲";
            }
            else
            {
                expandedNotificationId = null;
                var details = container.Q<VisualElement>(className: "notification-details");
                if (details != null)
                {
                    details.AddToClassList("hidden");
                }
                expandIcon.text = "▼";
            }
        }
        
        void CollapseAllNotifications()
        {
            expandedNotificationId = null;
            if (notificationList != null)
            {
                var items = notificationList.Query<VisualElement>(className: "notification-list-item").ToList();
                foreach (var item in items)
                {
                    var details = item.Q<VisualElement>(className: "notification-details");
                    if (details != null)
                    {
                        details.AddToClassList("hidden");
                    }
                    
                    var expandIcon = item.Q<Label>(className: "notification-expand-icon");
                    if (expandIcon != null)
                    {
                        expandIcon.text = "▼";
                    }
                }
            }
        }
        
        string GetIconForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => "ℹ",
                NotificationType.Success => "✓",
                NotificationType.Warning => "⚠",
                NotificationType.Error => "✕",
                _ => "•"
            };
        }
        
        string FormatGameDate(GameDate date)
        {
            if (date == null) return "Unknown";
            
            string[] monthNames = { 
                "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" 
            };
            
            string monthStr = date.Month >= 1 && date.Month <= 12 
                ? monthNames[date.Month - 1] 
                : "???";
            
            return $"{monthStr} {date.Day}, {date.Year}";
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Test Notification - Info")]
        void TestNotificationInfo()
        {
            AddNotification("This is a test INFO notification!", NotificationType.Info);
        }
        
        [ContextMenu("Test Notification - Success")]
        void TestNotificationSuccess()
        {
            AddNotification("Product released successfully! Revenue: $5,000", NotificationType.Success);
        }
        
        [ContextMenu("Test Notification - Warning")]
        void TestNotificationWarning()
        {
            AddNotification("Cash is running low! $2,000 remaining", NotificationType.Warning);
        }
        
        [ContextMenu("Test Notification - Error")]
        void TestNotificationError()
        {
            AddNotification("Insufficient funds! Cannot hire employee.", NotificationType.Error);
        }
        
        [ContextMenu("Test Multiple Notifications")]
        void TestMultipleNotifications()
        {
            AddNotification("First notification", NotificationType.Info);
            AddNotification("Second notification", NotificationType.Success);
            AddNotification("Third notification", NotificationType.Warning);
            AddNotification("Fourth notification", NotificationType.Error);
            AddNotification("Fifth notification with a much longer message to test wrapping", NotificationType.Info);
        }
        #endif
        
        private class NotificationData
        {
            public string Id;
            public string Message;
            public NotificationType Type;
            public GameDate GameDate;
            public bool IsExpandable;
            public MonthlyReport MonthlyReport;
        }
    }
}
