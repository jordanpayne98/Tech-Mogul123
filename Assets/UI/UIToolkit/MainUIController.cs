using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using TechMogul.Core;
using TechMogul.Systems;
using TechMogul.UI.Components;

namespace TechMogul.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainUIController : MonoBehaviour
    {
        [Header("Hire Dialog")]
        [SerializeField] private HireDialogController hireDialog;
        
        [Header("Panel Controllers")]
        [SerializeField] private ProductPanelController productPanelController;
        [SerializeField] private ContractPanelController contractPanelController;
        
        [Header("Standalone Dialogs")]
        [SerializeField] private GameObject employeeDetailDialogGO;
        
        private UIDocument uiDocument;
        private VisualElement root;
        
        // Header elements
        private Label cashValue;
        private Label dateValue;
        private Label speedValue;
        private Label reputationValue;
        
        // Employee detail dialog
        private VisualElement employeeDetailDialog;
        private VisualElement employeeDetailContent;
        private Button closeEmployeeDetailBtn;
        private Button fireEmployeeBtn;
        
        // Time control buttons
        private Button playPauseBtn;
        private Button fastBtn;
        private Button fasterBtn;
        private Button fastestBtn;
        
        // Game menu buttons
        private Button newGameBtn;
        private Button saveGameBtn;
        private Button loadGameBtn;
        
        private bool isPaused = true;
        
        // Panels
        private VisualElement employeePanel;
        private VisualElement productPanel;
        private VisualElement contractPanel;
        private VisualElement marketPanel;
        
        // Sidebar buttons
        private Button employeesBtn;
        private Button productsBtn;
        private Button contractsBtn;
        private Button marketBtn;
        
        // Sidebar stats
        private Label employeeCount;
        private Label productCount;
        
        // Employee panel elements
        private DataTable employeeTable;
        private VisualElement employeeTableContainer;
        private VisualElement employeeEmptyState;
        private Button hireBtn;
        
        private Button filterAllRoles;
        private Button filterDeveloper;
        private Button filterDesigner;
        private Button filterMarketer;
        private Button filterAllStatus;
        private Button filterAvailable;
        private Button filterBusy;
        
        private string currentRoleFilter = "All";
        private string currentStatusFilter = "All";
        
        private Employee selectedEmployee;
        
        // Market panel elements
        private Label playerMarketShare;
        private Label rivalMarketShare;
        private Label unclaimedMarketShare;
        private ScrollView rivalList;
        
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
            
            root = uiDocument.rootVisualElement;
            
            CacheReferences();
            BindButtons();
            SubscribeToEvents();
            
            ShowPanel(employeePanel, employeesBtn);
            
            // Initialize panel controllers
            if (productPanelController != null && productPanel != null)
            {
                productPanelController.Initialize(productPanel);
            }
            
            if (contractPanelController != null && contractPanel != null)
            {
                contractPanelController.Initialize(contractPanel);
            }
            
            RefreshEmployeeList();
            
            // Initialize reputation display
            InitializeReputation();
            
            // Initialize market panel if RivalSystem is already ready
            if (RivalSystem.Instance != null && RivalSystem.Instance.Rivals.Count > 0)
            {
                PopulateMarketPanel();
            }
            
            Debug.Log("MainUIController initialized with new layout");
        }
        
        void OnDisable()
        {
            UnsubscribeFromEvents();
            
            if (employeeTable != null)
            {
                employeeTable.SaveColumnState("EmployeeTable");
            }
        }
        
        void CacheReferences()
        {
            // Header elements
            cashValue = root.Q<Label>("cash-value");
            dateValue = root.Q<Label>("date-value");
            speedValue = root.Q<Label>("speed-value");
            reputationValue = root.Q<Label>("reputation-value");
            
            // Time control buttons
            playPauseBtn = root.Q<Button>("play-pause-btn");
            fastBtn = root.Q<Button>("fast-btn");
            fasterBtn = root.Q<Button>("faster-btn");
            fastestBtn = root.Q<Button>("fastest-btn");
            
            // Game menu buttons (optional - may not exist in UXML)
            newGameBtn = root.Q<Button>("new-game-btn");
            saveGameBtn = root.Q<Button>("save-game-btn");
            loadGameBtn = root.Q<Button>("load-game-btn");
            
            // Panels
            employeePanel = root.Q<VisualElement>("employee-panel");
            productPanel = root.Q<VisualElement>("product-panel");
            contractPanel = root.Q<VisualElement>("contract-panel");
            marketPanel = root.Q<VisualElement>("market-panel");
            
            // Sidebar buttons
            employeesBtn = root.Q<Button>("employees-btn");
            productsBtn = root.Q<Button>("products-btn");
            contractsBtn = root.Q<Button>("contracts-btn");
            marketBtn = root.Q<Button>("market-btn");
            
            // Sidebar stats
            employeeCount = root.Q<Label>("employee-count");
            productCount = root.Q<Label>("product-count");
            
            // Employee panel elements
            employeeTableContainer = root.Q<VisualElement>("employee-table-container");
            employeeEmptyState = root.Q<VisualElement>("empty-state");
            hireBtn = root.Q<Button>("hire-btn");
            
            filterAllRoles = root.Q<Button>("filter-all-roles");
            filterDeveloper = root.Q<Button>("filter-developer");
            filterDesigner = root.Q<Button>("filter-designer");
            filterMarketer = root.Q<Button>("filter-marketer");
            filterAllStatus = root.Q<Button>("filter-all-status");
            filterAvailable = root.Q<Button>("filter-available");
            filterBusy = root.Q<Button>("filter-busy");
            
            // Get Employee detail dialog from standalone GameObject
            if (employeeDetailDialogGO != null)
            {
                var dialogDoc = employeeDetailDialogGO.GetComponent<UIDocument>();
                if (dialogDoc != null && dialogDoc.rootVisualElement != null)
                {
                    employeeDetailDialog = dialogDoc.rootVisualElement.Q<VisualElement>("overlay");
                    if (employeeDetailDialog != null)
                    {
                        employeeDetailDialog.style.display = DisplayStyle.None;
                        employeeDetailContent = employeeDetailDialog.Q<VisualElement>("employee-detail-content");
                        closeEmployeeDetailBtn = employeeDetailDialog.Q<Button>("close-employee-detail-btn");
                        fireEmployeeBtn = employeeDetailDialog.Q<Button>("fire-employee-btn");
                    }
                    else
                    {
                        Debug.LogError("Employee Detail Dialog overlay element not found!");
                    }
                }
                else
                {
                    Debug.LogError("EmployeeDetailDialog GameObject is missing UIDocument component or rootVisualElement!");
                }
            }
            else
            {
                Debug.LogWarning("EmployeeDetailDialog GameObject not assigned in MainUIController!");
            }
            
            // Market panel elements
            playerMarketShare = root.Q<Label>("player-market-share");
            rivalMarketShare = root.Q<Label>("rival-market-share");
            unclaimedMarketShare = root.Q<Label>("unclaimed-market-share");
            rivalList = root.Q<ScrollView>("rival-list");
            
            LogMissingElements();
        }
        
        void LogMissingElements()
        {
            if (cashValue == null) Debug.LogWarning("cash-value Label not found");
            if (dateValue == null) Debug.LogWarning("date-value Label not found");
            if (speedValue == null) Debug.LogWarning("speed-value Label not found");
            if (playPauseBtn == null) Debug.LogWarning("play-pause-btn Button not found");
            if (employeePanel == null) Debug.LogWarning("employee-panel not found");
            if (employeesBtn == null) Debug.LogWarning("employees-btn not found");
        }
        
        void BindButtons()
        {
            // Time controls
            if (playPauseBtn != null) playPauseBtn.clicked += TogglePlayPause;
            if (fastBtn != null) fastBtn.clicked += () => ChangeSpeed(TimeSpeed.Fast);
            if (fasterBtn != null) fasterBtn.clicked += () => ChangeSpeed(TimeSpeed.Faster);
            if (fastestBtn != null) fastestBtn.clicked += () => ChangeSpeed(TimeSpeed.Fastest);
            
            // Game menu controls
            if (newGameBtn != null) newGameBtn.clicked += OnNewGameClicked;
            if (saveGameBtn != null) saveGameBtn.clicked += OnSaveGameClicked;
            if (loadGameBtn != null) loadGameBtn.clicked += OnLoadGameClicked;
            
            // Sidebar navigation
            if (employeesBtn != null) employeesBtn.clicked += () => ShowPanel(employeePanel, employeesBtn);
            if (productsBtn != null) productsBtn.clicked += () => ShowPanel(productPanel, productsBtn);
            if (contractsBtn != null) contractsBtn.clicked += () => ShowPanel(contractPanel, contractsBtn);
            if (marketBtn != null) marketBtn.clicked += () => ShowPanel(marketPanel, marketBtn);
            
            if (hireBtn != null) hireBtn.clicked += OpenHireDialog;
            
            if (filterAllRoles != null) filterAllRoles.clicked += () => SetRoleFilter("All");
            if (filterDeveloper != null) filterDeveloper.clicked += () => SetRoleFilter("Developer");
            if (filterDesigner != null) filterDesigner.clicked += () => SetRoleFilter("Designer");
            if (filterMarketer != null) filterMarketer.clicked += () => SetRoleFilter("Marketer");
            if (filterAllStatus != null) filterAllStatus.clicked += () => SetStatusFilter("All");
            if (filterAvailable != null) filterAvailable.clicked += () => SetStatusFilter("Available");
            if (filterBusy != null) filterBusy.clicked += () => SetStatusFilter("Busy");
            
            // Employee detail dialog
            if (closeEmployeeDetailBtn != null) closeEmployeeDetailBtn.clicked += CloseEmployeeDetailDialog;
            
            InitializeEmployeeTable();
        }
        
        void InitializeEmployeeTable()
        {
            if (employeeTableContainer == null)
            {
                Debug.LogWarning("Employee table container not found");
                return;
            }
            
            employeeTable = new DataTable(employeeTableContainer);
            
            employeeTable.AddColumn(new DataTableColumn("role", "Role", 150f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(100f)
                .SetMaxWidth(250f)
                .SetFormatter(emp => ((Employee)emp).role.roleName)
                .SetSortValueGetter(emp => ((Employee)emp).role.roleName));
            
            employeeTable.AddColumn(new DataTableColumn("name", "Name", 200f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(150f)
                .SetMaxWidth(350f)
                .SetFormatter(emp => ((Employee)emp).employeeName)
                .SetSortValueGetter(emp => ((Employee)emp).employeeName));
            
            employeeTable.AddColumn(new DataTableColumn("dev", "Dev", 80f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(60f)
                .SetMaxWidth(120f)
                .SetFormatter(emp => ((Employee)emp).devSkill.ToString("F0"))
                .SetSortValueGetter(emp => ((Employee)emp).devSkill));
            
            employeeTable.AddColumn(new DataTableColumn("design", "Design", 80f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(60f)
                .SetMaxWidth(120f)
                .SetFormatter(emp => ((Employee)emp).designSkill.ToString("F0"))
                .SetSortValueGetter(emp => ((Employee)emp).designSkill));
            
            employeeTable.AddColumn(new DataTableColumn("marketing", "Marketing", 80f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(60f)
                .SetMaxWidth(120f)
                .SetFormatter(emp => ((Employee)emp).marketingSkill.ToString("F0"))
                .SetSortValueGetter(emp => ((Employee)emp).marketingSkill));
            
            employeeTable.AddColumn(new DataTableColumn("morale", "Morale", 100f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(80f)
                .SetMaxWidth(150f)
                .SetFormatter(emp => ((Employee)emp).morale.ToString("F0") + "%")
                .SetSortValueGetter(emp => ((Employee)emp).morale));
            
            employeeTable.AddColumn(new DataTableColumn("salary", "Salary", 120f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(100f)
                .SetMaxWidth(180f)
                .SetFormatter(emp => "$" + ((Employee)emp).monthlySalary.ToString("N0"))
                .SetSortValueGetter(emp => ((Employee)emp).monthlySalary));
            
            employeeTable.AddColumn(new DataTableColumn("status", "Status", 100f)
                .SetAlignment(TextAnchor.MiddleCenter)
                .SetMinWidth(80f)
                .SetMaxWidth(150f)
                .SetFormatter(emp => 
                {
                    var e = (Employee)emp;
                    return e.isFired ? "Fired" : (e.isAvailable ? "Available" : "Busy");
                })
                .SetSortValueGetter(emp => 
                {
                    var e = (Employee)emp;
                    return e.isFired ? 2 : (e.isAvailable ? 0 : 1);
                }));
            
            // Set row click callback to show employee details
            employeeTable.OnRowClicked = (rowData) =>
            {
                Debug.Log($"[MainUI] Row clicked! Data type: {rowData?.GetType().Name}");
                if (rowData is Employee employee)
                {
                    Debug.Log($"[MainUI] Employee clicked: {employee.employeeName}");
                    OnEmployeeRowClicked(employee);
                }
                else
                {
                    Debug.LogWarning($"[MainUI] Row data is not an Employee! Type: {rowData?.GetType().Name}");
                }
            };
            
            employeeTable.Build();
            employeeTable.LoadColumnState("EmployeeTable");
        }
        
        void SetRoleFilter(string roleFilter)
        {
            currentRoleFilter = roleFilter;
            
            filterAllRoles?.RemoveFromClassList("filter-btn-active");
            filterDeveloper?.RemoveFromClassList("filter-btn-active");
            filterDesigner?.RemoveFromClassList("filter-btn-active");
            filterMarketer?.RemoveFromClassList("filter-btn-active");
            
            Button activeBtn = roleFilter switch
            {
                "Developer" => filterDeveloper,
                "Designer" => filterDesigner,
                "Marketer" => filterMarketer,
                _ => filterAllRoles
            };
            
            activeBtn?.AddToClassList("filter-btn-active");
            RefreshEmployeeList();
        }
        
        void SetStatusFilter(string statusFilter)
        {
            currentStatusFilter = statusFilter;
            
            filterAllStatus?.RemoveFromClassList("filter-btn-active");
            filterAvailable?.RemoveFromClassList("filter-btn-active");
            filterBusy?.RemoveFromClassList("filter-btn-active");
            
            Button activeBtn = statusFilter switch
            {
                "Available" => filterAvailable,
                "Busy" => filterBusy,
                _ => filterAllStatus
            };
            
            activeBtn?.AddToClassList("filter-btn-active");
            RefreshEmployeeList();
        }
        
        void TogglePlayPause()
        {
            if (isPaused)
            {
                ChangeSpeed(TimeSpeed.Normal);
            }
            else
            {
                ChangeSpeed(TimeSpeed.Paused);
            }
        }
        
        void OpenHireDialog()
        {
            Debug.Log("OpenHireDialog called");
            if (hireDialog != null)
            {
                Debug.Log("HireDialog is assigned, calling Show()");
                hireDialog.Show();
            }
            else
            {
                Debug.LogError("HireDialog is NULL - not assigned in MainUIController Inspector!");
            }
        }
        
        void ChangeSpeed(TimeSpeed speed)
        {
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = speed });
        }
        
        void ShowPanel(VisualElement panel, Button activeBtn)
        {
            if (panel == null || activeBtn == null) return;
            
            // Hide all panels
            employeePanel?.AddToClassList("hidden");
            productPanel?.AddToClassList("hidden");
            contractPanel?.AddToClassList("hidden");
            marketPanel?.AddToClassList("hidden");
            
            // Remove active class from all buttons
            employeesBtn?.RemoveFromClassList("sidebar-btn-active");
            productsBtn?.RemoveFromClassList("sidebar-btn-active");
            contractsBtn?.RemoveFromClassList("sidebar-btn-active");
            marketBtn?.RemoveFromClassList("sidebar-btn-active");
            
            // Show selected panel and highlight button
            panel.RemoveFromClassList("hidden");
            activeBtn.AddToClassList("sidebar-btn-active");
        }
        
        void SubscribeToEvents()
        {
            EventBus.Subscribe<OnCashChangedEvent>(UpdateCash);
            EventBus.Subscribe<OnDayTickEvent>(UpdateDate);
            EventBus.Subscribe<OnDayTickEvent>(evt => RefreshEmployeeListIfVisible());
            EventBus.Subscribe<OnSpeedChangedEvent>(UpdateSpeed);
            EventBus.Subscribe<OnReputationChangedEvent>(UpdateReputation);
            EventBus.Subscribe<OnEmployeeHiredEvent>(HandleEmployeeHired);
            EventBus.Subscribe<OnEmployeeFiredEvent>(HandleEmployeeFired);
            EventBus.Subscribe<TechMogul.Products.OnProductStartedEvent>(evt => UpdateSidebarStats());
            EventBus.Subscribe<TechMogul.Products.OnProductReleasedEvent>(evt => UpdateSidebarStats());
            EventBus.Subscribe<OnRivalsInitializedEvent>(evt => PopulateMarketPanel());
        }
        
        void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<OnCashChangedEvent>(UpdateCash);
            EventBus.Unsubscribe<OnDayTickEvent>(UpdateDate);
            EventBus.Unsubscribe<OnDayTickEvent>(evt => RefreshEmployeeListIfVisible());
            EventBus.Unsubscribe<OnSpeedChangedEvent>(UpdateSpeed);
            EventBus.Unsubscribe<OnReputationChangedEvent>(UpdateReputation);
            EventBus.Unsubscribe<OnEmployeeHiredEvent>(HandleEmployeeHired);
            EventBus.Unsubscribe<OnEmployeeFiredEvent>(HandleEmployeeFired);
            EventBus.Unsubscribe<TechMogul.Products.OnProductStartedEvent>(evt => UpdateSidebarStats());
            EventBus.Unsubscribe<TechMogul.Products.OnProductReleasedEvent>(evt => UpdateSidebarStats());
            EventBus.Unsubscribe<OnRivalsInitializedEvent>(evt => PopulateMarketPanel());
        }
        
        void UpdateCash(OnCashChangedEvent evt)
        {
            if (cashValue != null)
            {
                cashValue.text = $"${evt.NewCash:N0}";
            }
        }
        
        void UpdateDate(OnDayTickEvent evt)
        {
            if (dateValue != null)
            {
                string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
                                   "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                string monthStr = months[evt.CurrentDate.Month - 1];
                dateValue.text = $"{monthStr} {evt.CurrentDate.Day}, {evt.CurrentDate.Year}";
            }
        }
        
        void UpdateReputation(OnReputationChangedEvent evt)
        {
            if (reputationValue != null)
            {
                var reputationSystem = FindFirstObjectByType<ReputationSystem>();
                float maxRep = reputationSystem != null ? reputationSystem.MaxReputation : 100f;
                
                string stars = new string('★', evt.starRating) + new string('☆', 5 - evt.starRating);
                reputationValue.text = $"{stars} ({evt.newReputation:F0}/{maxRep:F0})";
            }
        }
        
        void InitializeReputation()
        {
            var reputationSystem = FindFirstObjectByType<ReputationSystem>();
            if (reputationSystem != null && reputationValue != null)
            {
                int stars = reputationSystem.StarRating;
                float reputation = reputationSystem.CurrentReputation;
                float maxRep = reputationSystem.MaxReputation;
                string starDisplay = new string('★', stars) + new string('☆', 5 - stars);
                reputationValue.text = $"{starDisplay} ({reputation:F0}/{maxRep:F0})";
            }
        }
        
        void UpdateSpeed(OnSpeedChangedEvent evt)
        {
            isPaused = evt.NewSpeed == TimeSpeed.Paused;
            
            if (speedValue != null)
            {
                string speedText = evt.NewSpeed switch
                {
                    TimeSpeed.Paused => "PAUSED",
                    TimeSpeed.Normal => "1x",
                    TimeSpeed.Fast => "2x",
                    TimeSpeed.Faster => "4x",
                    TimeSpeed.Fastest => "8x",
                    _ => "PAUSED"
                };
                
                speedValue.text = speedText;
            }
            
            // Update play/pause button appearance
            if (playPauseBtn != null)
            {
                if (isPaused)
                {
                    playPauseBtn.text = "▶";
                }
                else
                {
                    playPauseBtn.text = "| |";
                }
            }
            
            // Remove active class from all speed buttons
            playPauseBtn?.RemoveFromClassList("time-btn-active");
            fastBtn?.RemoveFromClassList("time-btn-active");
            fasterBtn?.RemoveFromClassList("time-btn-active");
            fastestBtn?.RemoveFromClassList("time-btn-active");
            
            // Highlight active speed button
            Button activeBtn = evt.NewSpeed switch
            {
                TimeSpeed.Normal => playPauseBtn,
                TimeSpeed.Fast => fastBtn,
                TimeSpeed.Faster => fasterBtn,
                TimeSpeed.Fastest => fastestBtn,
                _ => null
            };
            
            activeBtn?.AddToClassList("time-btn-active");
        }
        
        void HandleEmployeeHired(OnEmployeeHiredEvent evt)
        {
            RefreshEmployeeList();
            UpdateSidebarStats();
        }
        
        void HandleEmployeeFired(OnEmployeeFiredEvent evt)
        {
            RefreshEmployeeList();
            UpdateSidebarStats();
        }
        
        void UpdateSidebarStats()
        {
            var employeeSystem = FindObjectOfType<EmployeeSystem>();
            var productSystem = FindObjectOfType<TechMogul.Products.ProductSystem>();
            
            if (employeeCount != null && employeeSystem != null)
            {
                int count = employeeSystem.Employees.Count(e => !e.isFired);
                employeeCount.text = count.ToString();
            }
            
            if (productCount != null && productSystem != null)
            {
                int count = productSystem.Products.Count(p => p.state == TechMogul.Products.ProductState.Released);
                productCount.text = count.ToString();
            }
        }
        
        void RefreshEmployeeList()
        {
            if (EmployeeSystem.Instance == null) return;
            
            var employees = EmployeeSystem.Instance.Employees;
            
            var filteredEmployees = employees.Where(e => PassesFilter(e)).ToList();
            
            int count = filteredEmployees.Count;
            
            if (employeeCount != null)
            {
                employeeCount.text = employees.Count.ToString();
            }
            
            if (employeeEmptyState != null)
            {
                if (count == 0)
                {
                    employeeEmptyState.RemoveFromClassList("hidden");
                    
                    if (employees.Count > 0 && (currentRoleFilter != "All" || currentStatusFilter != "All"))
                    {
                        var emptyText = employeeEmptyState.Q<Label>("empty-text");
                        var emptySubtext = employeeEmptyState.Q<Label>("empty-subtext");
                        if (emptyText != null) emptyText.text = "No employees match filters";
                        if (emptySubtext != null) emptySubtext.text = "Try changing the filter settings";
                    }
                }
                else
                {
                    employeeEmptyState.AddToClassList("hidden");
                }
            }
            
            if (employeeTable != null && employeeTableContainer != null)
            {
                if (count == 0)
                {
                    employeeTableContainer.AddToClassList("hidden");
                }
                else
                {
                    employeeTableContainer.RemoveFromClassList("hidden");
                    employeeTable.SetData(filteredEmployees);
                }
            }
            
            // If an employee is selected, refresh their details
            if (selectedEmployee != null)
            {
                ShowEmployeeDetails(selectedEmployee);
            }
        }
        
        void RefreshEmployeeListIfVisible()
        {
            // Only refresh if employee panel is visible (not hidden)
            if (employeePanel != null && !employeePanel.ClassListContains("hidden"))
            {
                RefreshEmployeeList();
            }
        }
        
        bool PassesFilter(Employee employee)
        {
            bool roleMatch = currentRoleFilter == "All" || 
                            employee.role.roleName.Contains(currentRoleFilter, System.StringComparison.OrdinalIgnoreCase);
            
            bool statusMatch = currentStatusFilter == "All" ||
                              (currentStatusFilter == "Available" && employee.isAvailable) ||
                              (currentStatusFilter == "Busy" && !employee.isAvailable);
            
            return roleMatch && statusMatch;
        }
        
        void OnEmployeeRowClicked(Employee employee)
        {
            Debug.Log($"[MainUI] OnEmployeeRowClicked called for: {employee.employeeName}");
            selectedEmployee = employee;
            ShowEmployeeDetails(employee);
        }
        
        void ShowEmployeeDetails(Employee employee)
        {
            if (employeeDetailDialog == null || employeeDetailContent == null)
            {
                Debug.LogError("[MainUI] Employee detail dialog not found!");
                return;
            }
            
            Debug.Log($"[MainUI] Showing employee details dialog for {employee.employeeName}");
            
            employeeDetailContent.Clear();
            
            var container = new VisualElement();
            container.style.paddingTop = 15;
            container.style.paddingBottom = 15;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            
            AddDetailHeader(container, "EMPLOYEE DETAILS");
            
            AddDetailField(container, "Name", employee.employeeName);
            AddDetailField(container, "Role", employee.role.roleName);
            
            AddDetailSeparator(container);
            AddDetailHeader(container, "SKILLS");
            
            AddSkillBar(container, "Development", employee.devSkill);
            AddSkillBar(container, "Design", employee.designSkill);
            AddSkillBar(container, "Marketing", employee.marketingSkill);
            
            float avgSkill = employee.GetAverageSkill();
            AddDetailField(container, "Average", avgSkill.ToString("F1"));
            
            AddDetailSeparator(container);
            AddDetailHeader(container, "WELL-BEING");
            
            AddProgressBar(container, "Morale", employee.morale, GetMoraleColor(employee.morale));
            AddProgressBar(container, "Burnout", employee.burnout, new Color(0.95f, 0.26f, 0.21f));
            
            AddDetailSeparator(container);
            AddDetailHeader(container, "SKILL PROGRESSION");
            
            if (employee.skillHistory.Count > 1)
            {
                AddSkillProgression(container, employee);
            }
            else
            {
                var noDataLabel = new Label("Track skill improvements over time");
                noDataLabel.style.fontSize = 13;
                noDataLabel.style.color = new Color(0.59f, 0.59f, 0.63f);
                noDataLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                noDataLabel.style.marginTop = 20;
                noDataLabel.style.marginBottom = 20;
                noDataLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                container.Add(noDataLabel);
            }
            
            AddDetailSeparator(container);
            AddDetailHeader(container, "EMPLOYMENT");
            
            AddDetailField(container, "Salary", "$" + employee.monthlySalary.ToString("N0") + "/month");
            AddDetailField(container, "Status", employee.isAvailable ? "Available" : "Busy");
            AddDetailField(container, "Assignment", employee.currentAssignment);
            AddDetailField(container, "Days Hired", employee.daysSinceHired.ToString());
            
            employeeDetailContent.Add(container);
            
            // Wire up fire button
            if (fireEmployeeBtn != null)
            {
                fireEmployeeBtn.clicked += () => RequestFireEmployee(employee);
                fireEmployeeBtn.SetEnabled(employee.isAvailable);
                
                if (!employee.isAvailable)
                {
                    fireEmployeeBtn.tooltip = "Cannot fire employee currently assigned to work";
                }
            }
            
            // Show the dialog
            if (employeeDetailDialog != null)
            {
                employeeDetailDialog.style.display = DisplayStyle.Flex;
            }
            
            Debug.Log($"[MainUI] Employee details dialog shown for {employee.employeeName}");
        }
        
        void CloseEmployeeDetailDialog()
        {
            if (employeeDetailDialog != null)
            {
                employeeDetailDialog.style.display = DisplayStyle.None;
            }
            
            // Clear the fire button callback
            if (fireEmployeeBtn != null)
            {
                fireEmployeeBtn.clicked -= () => RequestFireEmployee(selectedEmployee);
            }
            
            // Clear the selected employee to prevent auto-reopening
            selectedEmployee = null;
        }
        
        void RequestFireEmployee(Employee employee)
        {
            if (!employee.isAvailable)
            {
                Debug.LogWarning($"Cannot fire {employee.employeeName} - currently assigned to {employee.currentAssignment}");
                return;
            }
            
            if (ShowFireConfirmation(employee))
            {
                EventBus.Publish(new RequestFireEmployeeEvent { EmployeeId = employee.employeeId });
                CloseEmployeeDetailDialog();
            }
        }
        
        bool ShowFireConfirmation(Employee employee)
        {
            return UnityEditor.EditorUtility.DisplayDialog(
                "Fire Employee",
                $"Are you sure you want to fire {employee.employeeName}?\n\n" +
                $"Role: {employee.role.roleName}\n" +
                $"Skills: Dev {employee.devSkill:F0}, Design {employee.designSkill:F0}, Marketing {employee.marketingSkill:F0}\n" +
                $"Salary: ${employee.monthlySalary:N0}/month\n\n" +
                $"NOTE: They will receive their full salary (${employee.monthlySalary:N0}) at the end of this month.\n" +
                "This action cannot be undone.",
                "Fire Employee",
                "Cancel"
            );
        }
        
        void AddDetailHeader(VisualElement container, string text)
        {
            var header = new Label(text);
            header.style.fontSize = 15;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.color = new Color(0.65f, 0.65f, 0.7f);
            header.style.marginTop = 28;
            header.style.marginBottom = 20;
            header.style.unityTextAlign = TextAnchor.MiddleLeft;
            header.style.letterSpacing = 1;
            container.Add(header);
        }
        
        void AddDetailField(VisualElement container, string label, string value)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 0;
            row.style.paddingBottom = 16;
            row.style.paddingTop = 16;
            row.style.minHeight = 20;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            
            var labelElement = new Label(label);
            labelElement.style.fontSize = 15;
            labelElement.style.color = new Color(0.7f, 0.7f, 0.75f);
            labelElement.style.flexShrink = 0;
            labelElement.style.marginRight = 20;
            labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            
            var valueElement = new Label(value);
            valueElement.style.fontSize = 15;
            valueElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueElement.style.color = new Color(0.9f, 0.9f, 0.95f);
            valueElement.style.unityTextAlign = TextAnchor.MiddleRight;
            valueElement.style.flexShrink = 1;
            valueElement.style.whiteSpace = WhiteSpace.Normal;
            
            row.Add(labelElement);
            row.Add(valueElement);
            container.Add(row);
        }
        
        void AddDetailSeparator(VisualElement container)
        {
            var separator = new VisualElement();
            separator.style.height = 1;
            separator.style.backgroundColor = new Color(1, 1, 1, 0.15f);
            separator.style.marginTop = 32;
            separator.style.marginBottom = 32;
            container.Add(separator);
        }
        
        void AddSkillBar(VisualElement container, string skillName, float skillValue)
        {
            var row = new VisualElement();
            row.style.marginBottom = 0;
            row.style.paddingBottom = 20;
            row.style.paddingTop = 16;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            
            var labelRow = new VisualElement();
            labelRow.style.flexDirection = FlexDirection.Row;
            labelRow.style.justifyContent = Justify.SpaceBetween;
            labelRow.style.alignItems = Align.Center;
            labelRow.style.marginBottom = 12;
            labelRow.style.minHeight = 20;
            
            var label = new Label(skillName);
            label.style.fontSize = 15;
            label.style.color = new Color(0.75f, 0.75f, 0.8f);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            
            var value = new Label(skillValue.ToString("F0"));
            value.style.fontSize = 15;
            value.style.unityFontStyleAndWeight = FontStyle.Bold;
            value.style.color = new Color(0.3f, 0.7f, 0.95f);
            value.style.unityTextAlign = TextAnchor.MiddleRight;
            
            labelRow.Add(label);
            labelRow.Add(value);
            
            var barContainer = new VisualElement();
            barContainer.style.height = 10;
            barContainer.style.backgroundColor = new Color(0, 0, 0, 0.4f);
            barContainer.style.borderTopLeftRadius = 5;
            barContainer.style.borderTopRightRadius = 5;
            barContainer.style.borderBottomLeftRadius = 5;
            barContainer.style.borderBottomRightRadius = 5;
            barContainer.style.marginBottom = 4;
            
            var barFill = new VisualElement();
            barFill.style.height = 10;
            barFill.style.width = Length.Percent(skillValue);
            barFill.style.backgroundColor = GetSkillColor(skillValue);
            barFill.style.borderTopLeftRadius = 5;
            barFill.style.borderTopRightRadius = 5;
            barFill.style.borderBottomLeftRadius = 5;
            barFill.style.borderBottomRightRadius = 5;
            
            barContainer.Add(barFill);
            
            row.Add(labelRow);
            row.Add(barContainer);
            container.Add(row);
        }
        
        void AddProgressBar(VisualElement container, string label, float value, Color barColor)
        {
            var row = new VisualElement();
            row.style.marginBottom = 0;
            row.style.paddingBottom = 20;
            row.style.paddingTop = 16;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            
            var labelRow = new VisualElement();
            labelRow.style.flexDirection = FlexDirection.Row;
            labelRow.style.justifyContent = Justify.SpaceBetween;
            labelRow.style.alignItems = Align.Center;
            labelRow.style.marginBottom = 12;
            labelRow.style.minHeight = 20;
            
            var labelElement = new Label(label);
            labelElement.style.fontSize = 15;
            labelElement.style.color = new Color(0.75f, 0.75f, 0.8f);
            labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            
            var valueElement = new Label(value.ToString("F0") + "%");
            valueElement.style.fontSize = 15;
            valueElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueElement.style.color = barColor;
            valueElement.style.unityTextAlign = TextAnchor.MiddleRight;
            
            labelRow.Add(labelElement);
            labelRow.Add(valueElement);
            
            var barContainer = new VisualElement();
            barContainer.style.height = 10;
            barContainer.style.backgroundColor = new Color(0, 0, 0, 0.4f);
            barContainer.style.borderTopLeftRadius = 5;
            barContainer.style.borderTopRightRadius = 5;
            barContainer.style.borderBottomLeftRadius = 5;
            barContainer.style.borderBottomRightRadius = 5;
            barContainer.style.marginBottom = 4;
            
            var barFill = new VisualElement();
            barFill.style.height = 10;
            barFill.style.width = Length.Percent(value);
            barFill.style.backgroundColor = barColor;
            barFill.style.borderTopLeftRadius = 5;
            barFill.style.borderTopRightRadius = 5;
            barFill.style.borderBottomLeftRadius = 5;
            barFill.style.borderBottomRightRadius = 5;
            
            barContainer.Add(barFill);
            
            row.Add(labelRow);
            row.Add(barContainer);
            container.Add(row);
        }
        
        Color GetSkillColor(float skill)
        {
            if (skill >= 75)
                return new Color(0.18f, 0.8f, 0.44f);
            else if (skill >= 50)
                return new Color(0.2f, 0.6f, 0.86f);
            else if (skill >= 25)
                return new Color(0.95f, 0.61f, 0.07f);
            else
                return new Color(0.95f, 0.26f, 0.21f);
        }
        
        Color GetMoraleColor(float morale)
        {
            if (morale >= 70)
                return new Color(0.18f, 0.8f, 0.44f);
            else if (morale >= 40)
                return new Color(0.95f, 0.61f, 0.07f);
            else
                return new Color(0.95f, 0.26f, 0.21f);
        }
        
        void AddSkillProgression(VisualElement container, Employee employee)
        {
            var history = employee.skillHistory;
            if (history.Count < 2) return;
            
            var first = history[0];
            var last = history[history.Count - 1];
            
            AddSkillChange(container, "Development", first.devSkill, last.devSkill);
            AddSkillChange(container, "Design", first.designSkill, last.designSkill);
            AddSkillChange(container, "Marketing", first.marketingSkill, last.marketingSkill);
            
            var timeLabel = new Label($"Tracked over {last.day - first.day} days");
            timeLabel.style.fontSize = 10;
            timeLabel.style.color = new Color(0.59f, 0.59f, 0.63f);
            timeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            timeLabel.style.marginTop = 8;
            timeLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            container.Add(timeLabel);
            
            AddSkillChart(container, employee);
        }
        
        void AddSkillChange(VisualElement container, string skillName, float startValue, float currentValue)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginBottom = 6;
            row.style.paddingLeft = 4;
            row.style.paddingRight = 4;
            
            var label = new Label(skillName);
            label.style.fontSize = 11;
            label.style.color = new Color(0.71f, 0.71f, 0.75f);
            
            float change = currentValue - startValue;
            string changeText;
            Color changeColor;
            
            if (change > 0)
            {
                changeText = $"{startValue:F0} → {currentValue:F0} (+{change:F1})";
                changeColor = new Color(0.18f, 0.8f, 0.44f);
            }
            else if (change < 0)
            {
                changeText = $"{startValue:F0} → {currentValue:F0} ({change:F1})";
                changeColor = new Color(0.95f, 0.26f, 0.21f);
            }
            else
            {
                changeText = $"{currentValue:F0} (No change)";
                changeColor = new Color(0.71f, 0.71f, 0.75f);
            }
            
            var value = new Label(changeText);
            value.style.fontSize = 11;
            value.style.unityFontStyleAndWeight = FontStyle.Bold;
            value.style.color = changeColor;
            
            row.Add(label);
            row.Add(value);
            container.Add(row);
        }
        
        void AddSkillChart(VisualElement container, Employee employee)
        {
            var chartContainer = new VisualElement();
            chartContainer.style.marginTop = 10;
            chartContainer.style.marginBottom = 5;
            chartContainer.style.height = 60;
            chartContainer.style.backgroundColor = new Color(0, 0, 0, 0.3f);
            chartContainer.style.borderTopLeftRadius = 4;
            chartContainer.style.borderTopRightRadius = 4;
            chartContainer.style.borderBottomLeftRadius = 4;
            chartContainer.style.borderBottomRightRadius = 4;
            chartContainer.style.borderLeftWidth = 1;
            chartContainer.style.borderRightWidth = 1;
            chartContainer.style.borderTopWidth = 1;
            chartContainer.style.borderBottomWidth = 1;
            chartContainer.style.borderLeftColor = new Color(1, 1, 1, 0.1f);
            chartContainer.style.borderRightColor = new Color(1, 1, 1, 0.1f);
            chartContainer.style.borderTopColor = new Color(1, 1, 1, 0.1f);
            chartContainer.style.borderBottomColor = new Color(1, 1, 1, 0.1f);
            chartContainer.style.paddingTop = 5;
            chartContainer.style.paddingBottom = 5;
            chartContainer.style.paddingLeft = 5;
            chartContainer.style.paddingRight = 5;
            chartContainer.style.flexDirection = FlexDirection.Row;
            chartContainer.style.alignItems = Align.FlexEnd;
            
            var history = employee.skillHistory;
            int maxPoints = Mathf.Min(history.Count, 10);
            int step = Mathf.Max(1, history.Count / maxPoints);
            
            for (int i = 0; i < history.Count; i += step)
            {
                var snapshot = history[i];
                float avgSkill = (snapshot.devSkill + snapshot.designSkill + snapshot.marketingSkill) / 3f;
                
                var bar = new VisualElement();
                bar.style.width = Length.Percent(100f / maxPoints);
                bar.style.height = Length.Percent(avgSkill);
                bar.style.backgroundColor = GetSkillColor(avgSkill);
                bar.style.marginLeft = 1;
                bar.style.marginRight = 1;
                
                chartContainer.Add(bar);
            }
            
            container.Add(chartContainer);
        }
        
        void PopulateMarketPanel()
        {
            if (rivalList == null || RivalSystem.Instance == null)
            {
                Debug.LogWarning("Market panel elements or RivalSystem not ready");
                return;
            }
            
            rivalList.Clear();
            
            var rivals = RivalSystem.Instance.Rivals;
            float totalRivalShare = RivalSystem.Instance.GetTotalRivalMarketShare();
            float playerShare = 0f;
            float unclaimedShare = 100f - totalRivalShare - playerShare;
            
            if (playerMarketShare != null)
                playerMarketShare.text = $"{playerShare:F1}%";
            
            if (rivalMarketShare != null)
                rivalMarketShare.text = $"{totalRivalShare:F1}%";
            
            if (unclaimedMarketShare != null)
                unclaimedMarketShare.text = $"{unclaimedShare:F1}%";
            
            foreach (var rival in rivals)
            {
                var rivalCard = CreateRivalCard(rival);
                rivalList.Add(rivalCard);
            }
            
            Debug.Log($"Populated market panel with {rivals.Count} rivals");
        }
        
        VisualElement CreateRivalCard(RivalCompanyData rival)
        {
            var card = new VisualElement();
            card.AddToClassList("rival-card");
            
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 8;
            
            var nameLabel = new Label(rival.Name);
            nameLabel.style.fontSize = 16;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = new Color(0.9f, 0.9f, 0.95f);
            
            var industryLabel = new Label(rival.Industry);
            industryLabel.style.fontSize = 12;
            industryLabel.style.color = new Color(0.5f, 0.7f, 0.9f);
            industryLabel.style.backgroundColor = new Color(0.1f, 0.2f, 0.3f, 0.5f);
            industryLabel.style.paddingTop = 2;
            industryLabel.style.paddingBottom = 2;
            industryLabel.style.paddingLeft = 6;
            industryLabel.style.paddingRight = 6;
            industryLabel.style.borderTopLeftRadius = 3;
            industryLabel.style.borderTopRightRadius = 3;
            industryLabel.style.borderBottomLeftRadius = 3;
            industryLabel.style.borderBottomRightRadius = 3;
            
            header.Add(nameLabel);
            header.Add(industryLabel);
            
            var description = new Label(rival.Description);
            description.style.fontSize = 13;
            description.style.color = new Color(0.7f, 0.7f, 0.75f);
            description.style.marginBottom = 10;
            description.style.whiteSpace = WhiteSpace.Normal;
            
            var statsRow = new VisualElement();
            statsRow.style.flexDirection = FlexDirection.Row;
            statsRow.style.justifyContent = Justify.SpaceBetween;
            
            var marketShareStat = CreateStatElement("Market Share", $"{rival.MarketShare:F1}%");
            var employeeStat = CreateStatElement("Employees", rival.EmployeeCount.ToString());
            
            statsRow.Add(marketShareStat);
            statsRow.Add(employeeStat);
            
            card.Add(header);
            card.Add(description);
            card.Add(statsRow);
            
            return card;
        }
        
        VisualElement CreateStatElement(string label, string value)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.alignItems = Align.Center;
            
            var labelElement = new Label(label);
            labelElement.style.fontSize = 11;
            labelElement.style.color = new Color(0.5f, 0.5f, 0.55f);
            labelElement.style.marginBottom = 2;
            
            var valueElement = new Label(value);
            valueElement.style.fontSize = 14;
            valueElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueElement.style.color = new Color(0.3f, 0.7f, 0.95f);
            
            container.Add(labelElement);
            container.Add(valueElement);
            
            return container;
        }
        
        void OnNewGameClicked()
        {
            Debug.Log("New Game button clicked");
            EventBus.Publish(new RequestStartNewGameEvent());
        }
        
        void OnSaveGameClicked()
        {
            Debug.Log("Save Game button clicked");
            EventBus.Publish(new RequestSaveGameEvent());
        }
        
        void OnLoadGameClicked()
        {
            Debug.Log("Load Game button clicked");
            EventBus.Publish(new RequestLoadGameEvent());
        }
    }
}
