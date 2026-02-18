using UnityEngine;
using TMPro;
using System.Collections;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    public class NotificationManager : UIController
    {
        [Header("References")]
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;
        
        [Header("Settings")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private int maxNotifications = 5;
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnGameStartedEvent>(evt => 
                ShowNotification("Game Started! Good luck building your tech empire."));
            
            Subscribe<OnBankruptcyEvent>(evt => 
                ShowNotification("Bankruptcy! Your company has run out of cash.", NotificationType.Error));
            
            Subscribe<OnInsufficientCashEvent>(evt => 
                ShowNotification($"Insufficient cash! Need ${evt.Required:N0}, have ${evt.Available:N0}", NotificationType.Warning));
            
            Subscribe<OnMonthTickEvent>(evt => 
                ShowNotification($"Month {evt.Month}, {evt.Year} - New month started", NotificationType.Info));
            
            Subscribe<TechMogul.Systems.OnEmployeeHiredEvent>(evt => 
                ShowNotification($"Hired {evt.Name} as {evt.Role.roleName}", NotificationType.Success));
            
            Subscribe<TechMogul.Systems.OnEmployeeFiredEvent>(evt => 
                ShowNotification($"Fired {evt.Name}", NotificationType.Warning));
        }
        
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            if (notificationPrefab == null || notificationContainer == null)
            {
                Debug.LogWarning("NotificationManager: Missing prefab or container reference");
                return;
            }
            
            // Limit number of simultaneous notifications
            if (notificationContainer.childCount >= maxNotifications)
            {
                Destroy(notificationContainer.GetChild(0).gameObject);
            }
            
            var notification = Instantiate(notificationPrefab, notificationContainer);
            
            // Find text component and set message
            var text = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = message;
            }
            
            // Auto-destroy after duration
            Destroy(notification, displayDuration);
        }
        
        #if UNITY_EDITOR
        // Test methods (accessible via context menu in Inspector)
        [ContextMenu("Test Notification - Info")]
        void TestNotificationInfo()
        {
            ShowNotification("This is a test INFO notification!", NotificationType.Info);
        }
        
        [ContextMenu("Test Notification - Success")]
        void TestNotificationSuccess()
        {
            ShowNotification("Product released successfully! Revenue: $5,000", NotificationType.Success);
        }
        
        [ContextMenu("Test Notification - Warning")]
        void TestNotificationWarning()
        {
            ShowNotification("Cash is running low! $2,000 remaining", NotificationType.Warning);
        }
        
        [ContextMenu("Test Notification - Error")]
        void TestNotificationError()
        {
            ShowNotification("Insufficient funds! Cannot hire employee.", NotificationType.Error);
        }
        
        [ContextMenu("Test Multiple Notifications")]
        void TestMultipleNotifications()
        {
            StartCoroutine(TestMultipleNotificationsCoroutine());
        }
        
        IEnumerator TestMultipleNotificationsCoroutine()
        {
            ShowNotification("First notification", NotificationType.Info);
            yield return new WaitForSeconds(0.5f);
            ShowNotification("Second notification", NotificationType.Success);
            yield return new WaitForSeconds(0.5f);
            ShowNotification("Third notification", NotificationType.Warning);
            yield return new WaitForSeconds(0.5f);
            ShowNotification("Fourth notification", NotificationType.Error);
        }
        #endif
    }
    
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
