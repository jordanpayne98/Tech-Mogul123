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
    public class MainUIController : UIController
    {
        [Header("Hire Dialog")]
        [SerializeField] private HireDialogController hireDialog;
        
        [Header("Panel Controllers")]
        [SerializeField] private ProductPanelController productPanelController;
        [SerializeField] private ContractPanelController contractPanelController;
        [SerializeField] private IndustryOverviewController industryOverviewController;
        
        [Header("Standalone Dialogs")]
        [SerializeField] private GameObject employeeDetailDialogGO;
        
        private UIDocument uiDocument;
        private VisualElement root;
        
        // Header elements
        private Label cashValue;
        private Label dateValue;
        private Label speedValue;
        private Label reputationValue;
        private Label eraValue;
        
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
        private VisualElement technologyPanel;
        
        // Sidebar buttons
        private Button employeesBtn;
        private Button productsBtn;
        private Button contractsBtn;
        private Button marketBtn;
        private Button technologyBtn;
        
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
        
        protected override void Awake()
        {
            base.Awake();
            uiDocument = GetComponent<UIDocument>();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
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
            
            if (RivalSystem.Instance != null && RivalSystem.Instance.AllCompanies.Count > 0)
            {
                PopulateMarketPanel();
            }
            
            Debug.Log("MainUIController initialized with new layout");
        }
        
        protected override void OnDisable()
        {
            if (employeeTable != null)
            {
                employeeTable.SaveColumnState("EmployeeTable");
            }
            
            base.OnDisable();
        }
        
        void CacheReferences()
        {
            // Header elements
            cashValue = root.Q<Label>("cash-value");
            dateValue = root.Q<Label>("date-value");
            speedValue = root.Q<Label>("speed-value");
            reputationValue = root.Q<Label>("reputation-value");
            eraValue = root.Q<Label>("era-value");
            
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
            technologyPanel = root.Q<VisualElement>("technology-panel");
            
            Debug.Log($"Technology panel found: {technologyPanel != null}");
            if (technologyPanel != null)
            {
                Debug.Log($"Technology panel classes: {string.Join(", ", technologyPanel.GetClasses())}");
            }
            
            // Sidebar buttons
            employeesBtn = root.Q<Button>("employees-btn");
            productsBtn = root.Q<Button>("products-btn");
            contractsBtn = root.Q<Button>("contracts-btn");
            marketBtn = root.Q<Button>("market-btn");
            technologyBtn = root.Q<Button>("technology-btn");
            
            // Sidebar stats
            employeeCount = root.Q<Label>("employee-count");
            productCount = root.Q<Label>("product-count");
            
            // Employee panel elements
            employeeTableContainer = root.Q<VisualElement>("employee-table-container");
            employeeEmptyState = root.Q<VisualElement>("empty-state");
            hireBtn = root.Q<Button>("hire-btn");
            
            if (hireBtn == null)
            {
                Debug.LogWarning("hire-btn not found, trying employeePanel query...");
                if (employeePanel != null)
                {
                    hireBtn = employeePanel.Q<Button>("hire-btn");
                }
            }
            
            Debug.Log($"hireBtn found: {hireBtn != null}");
            
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
            if (technologyBtn != null) technologyBtn.clicked += () => ShowPanel(technologyPanel, technologyBtn);
            
            if (hireBtn != null) hireBtn.clicked += OpenHireDialog;
            else Debug.LogError("hireBtn is NULL - button not found in UXML!");
            
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
                    return e.isAvailable ? "Available" : "Busy";
                })
                .SetSortValueGetter(emp => 
                {
                    var e = (Employee)emp;
                    return e.isAvailable ? 0 : 1;
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
            if (panel == null || activeBtn == null)
            {
                Debug.LogWarning($"ShowPanel: panel is null: {panel == null}, activeBtn is null: {activeBtn == null}");
                return;
            }
            
            Debug.Log($"ShowPanel called for: {panel.name}");
            
            // Hide all panels
            employeePanel?.AddToClassList("hidden");
            productPanel?.AddToClassList("hidden");
            contractPanel?.AddToClassList("hidden");
            marketPanel?.AddToClassList("hidden");
            technologyPanel?.AddToClassList("hidden");
            
            // Remove active class from all buttons
            employeesBtn?.RemoveFromClassList("sidebar-btn-active");
            productsBtn?.RemoveFromClassList("sidebar-btn-active");
            contractsBtn?.RemoveFromClassList("sidebar-btn-active");
            marketBtn?.RemoveFromClassList("sidebar-btn-active");
            technologyBtn?.RemoveFromClassList("sidebar-btn-active");
            
            // Show selected panel and highlight button
            panel.RemoveFromClassList("hidden");
            activeBtn.AddToClassList("sidebar-btn-active");
            
            Debug.Log($"Panel {panel.name} classes after showing: {string.Join(", ", panel.GetClasses())}");
            Debug.Log($"Panel display style: {panel.style.display}");
            
            // Refresh IndustryOverview when market panel shown
            if (panel == marketPanel && industryOverviewController != null)
            {
                industryOverviewController.ForceRefresh();
            }
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnCashChangedEvent>(UpdateCash);
            Subscribe<OnDayTickEvent>(UpdateDate);
            Subscribe<OnDayTickEvent>(evt => RefreshEmployeeListIfVisible());
            Subscribe<OnSpeedChangedEvent>(UpdateSpeed);
            Subscribe<OnReputationChangedEvent>(UpdateReputation);
            Subscribe<OnEmployeeHiredEvent>(HandleEmployeeHired);
            Subscribe<OnEmployeeFiredEvent>(HandleEmployeeFired);
            Subscribe<TechMogul.Products.OnProductStartedEvent>(evt => UpdateSidebarStats());
            Subscribe<TechMogul.Products.OnProductReleasedEvent>(evt => UpdateSidebarStats());
            Subscribe<OnRivalsInitializedEvent>(evt => PopulateMarketPanel());
            Subscribe<OnEraChangedEvent>(UpdateEra);
            Subscribe<OnGameStartedEvent>(InitializeEra);
            Subscribe<OnTechnologiesUpdatedEvent>(UpdateTechnologyPanel);
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
        
        void UpdateEra(OnEraChangedEvent evt)
        {
            if (eraValue != null && evt.NewEra != null)
            {
                eraValue.text = evt.NewEra.eraName;
                
                string trendIndicator = GetMarketTrendIndicator(evt.NewEra.baseMarketSizeMultiplier);
                eraValue.tooltip = $"{evt.NewEra.description}\n\nMarket Trend: {trendIndicator}";
            }
        }
        
        void InitializeEra(OnGameStartedEvent evt)
        {
            EraSystem eraSystem = ServiceLocator.Instance.Get<EraSystem>();
            if (eraSystem != null && eraSystem.CurrentEra != null && eraValue != null)
            {
                eraValue.text = eraSystem.CurrentEra.eraName;
                
                string trendIndicator = GetMarketTrendIndicator(eraSystem.CurrentEra.baseMarketSizeMultiplier);
                eraValue.tooltip = $"{eraSystem.CurrentEra.description}\n\nMarket Trend: {trendIndicator}";
            }
        }
        
        string GetMarketTrendIndicator(float multiplier)
        {
            if (multiplier >= 2.5f) return "🔥 Explosive Growth";
            if (multiplier >= 1.5f) return "📈 Strong Growth";
            if (multiplier >= 1.0f) return "➡️ Steady";
            if (multiplier >= 0.5f) return "📉 Declining";
            return "💤 Stagnant";
        }
        
        void UpdateTechnologyPanel(OnTechnologiesUpdatedEvent evt)
        {
            if (technologyPanel == null)
            {
                return;
            }
            
            ScrollView techList = technologyPanel.Q<ScrollView>("technology-list");
            if (techList == null)
            {
                return;
            }
            
            techList.Clear();
            
            TechnologySystem techSystem = ServiceLocator.Instance.Get<TechnologySystem>();
            if (techSystem == null)
            {
                return;
            }
            
            foreach (var kvp in evt.AdoptionRates)
            {
                string techId = kvp.Key;
                float adoption = kvp.Value;
                TechAdoptionPhase phase = evt.Phases.ContainsKey(techId) ? evt.Phases[techId] : TechAdoptionPhase.Locked;
                
                TechnologySO tech = techSystem.GetTechnology(techId);
                if (tech == null)
                {
                    continue;
                }
                
                VisualElement techItem = CreateTechnologyItem(tech, adoption, phase);
                techList.Add(techItem);
            }
        }
        
        VisualElement CreateTechnologyItem(TechnologySO tech, float adoption, TechAdoptionPhase phase)
        {
            VisualElement item = new VisualElement();
            item.AddToClassList("tech-item");
            item.style.flexDirection = FlexDirection.Row;
            item.style.justifyContent = Justify.SpaceBetween;
            item.style.alignItems = Align.Center;
            item.style.paddingLeft = 12;
            item.style.paddingRight = 12;
            item.style.paddingTop = 12;
            item.style.paddingBottom = 12;
            item.style.marginBottom = new StyleLength(8);
            item.style.backgroundColor = new Color(0.08f, 0.08f, 0.09f, 0.9f);
            item.style.borderTopLeftRadius = 8;
            item.style.borderTopRightRadius = 8;
            item.style.borderBottomLeftRadius = 8;
            item.style.borderBottomRightRadius = 8;
            
            VisualElement leftSection = new VisualElement();
            leftSection.style.flexGrow = 1;
            
            Label nameLabel = new Label(tech.techName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = new Color(0.9f, 0.9f, 0.96f);
            
            Label categoryLabel = new Label($"{tech.category} • {tech.researchYear}");
            categoryLabel.style.fontSize = 12;
            categoryLabel.style.color = new Color(0.7f, 0.7f, 0.75f);
            categoryLabel.style.marginTop = 2;
            
            leftSection.Add(nameLabel);
            leftSection.Add(categoryLabel);
            
            VisualElement rightSection = new VisualElement();
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;
            
            Label adoptionLabel = new Label($"{adoption:P0}");
            adoptionLabel.style.fontSize = 18;
            adoptionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            adoptionLabel.style.marginRight = 12;
            adoptionLabel.style.color = GetAdoptionColor(adoption);
            
            Label phaseLabel = new Label(GetPhaseText(phase));
            phaseLabel.style.fontSize = 12;
            phaseLabel.style.paddingLeft = 4;
            phaseLabel.style.paddingRight = 4;
            phaseLabel.style.paddingTop = 4;
            phaseLabel.style.paddingBottom = 4;
            phaseLabel.style.borderTopLeftRadius = 4;
            phaseLabel.style.borderTopRightRadius = 4;
            phaseLabel.style.borderBottomLeftRadius = 4;
            phaseLabel.style.borderBottomRightRadius = 4;
            phaseLabel.style.backgroundColor = GetPhaseColor(phase);
            phaseLabel.style.color = Color.white;
            
            rightSection.Add(adoptionLabel);
            rightSection.Add(phaseLabel);
            
            item.Add(leftSection);
            item.Add(rightSection);
            
            item.tooltip = $"{tech.description}\n\nInnovation: +{tech.innovationBonus:P0} | Bug Risk: {tech.bugRiskMultiplier:F2}x | Market Relevance: {tech.marketRelevanceMultiplier:F2}x";
            
            return item;
        }
        
        Color GetAdoptionColor(float adoption)
        {
            if (adoption >= 0.75f) return new Color(0.2f, 0.8f, 0.3f);
            if (adoption >= 0.40f) return new Color(0.3f, 0.7f, 0.9f);
            if (adoption >= 0.15f) return new Color(0.9f, 0.7f, 0.3f);
            return new Color(0.6f, 0.6f, 0.65f);
        }
        
        Color GetPhaseColor(TechAdoptionPhase phase)
        {
            return phase switch
            {
                TechAdoptionPhase.Mandatory => new Color(0.15f, 0.65f, 0.2f),
                TechAdoptionPhase.Mainstream => new Color(0.2f, 0.6f, 0.75f),
                TechAdoptionPhase.Growth => new Color(0.75f, 0.6f, 0.2f),
                TechAdoptionPhase.EarlyAdoption => new Color(0.75f, 0.5f, 0.2f),
                TechAdoptionPhase.Research => new Color(0.6f, 0.4f, 0.7f),
                _ => new Color(0.4f, 0.4f, 0.45f)
            };
        }
        
        string GetPhaseText(TechAdoptionPhase phase)
        {
            return phase switch
            {
                TechAdoptionPhase.Mandatory => "Mandatory",
                TechAdoptionPhase.Mainstream => "Mainstream",
                TechAdoptionPhase.Growth => "Growth",
                TechAdoptionPhase.EarlyAdoption => "Early",
                TechAdoptionPhase.Research => "Research",
                _ => "Locked"
            };
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
            var employeeSystem = FindFirstObjectByType<EmployeeSystem>();
            var productSystem = FindFirstObjectByType<TechMogul.Products.ProductSystem>();
            
            if (employeeCount != null && employeeSystem != null)
            {
                int count = employeeSystem.Employees.Count;
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
            AddDetailHeader(container, "TRAITS");
            
            AddTraitSection(container, employee);
            
            AddDetailSeparator(container);
            AddDetailHeader(container, "WELL-BEING");
            
            AddProgressBar(container, "Morale", employee.morale, GetMoraleColor(employee.morale));
            AddProgressBar(container, "Burnout", employee.burnout, new Color(0.95f, 0.26f, 0.21f));
            AddProgressBar(container, "Stress", employee.stress, new Color(0.95f, 0.6f, 0.21f));
            AddProgressBar(container, "Fatigue", employee.fatigue, new Color(0.6f, 0.6f, 0.8f));
            
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
            AddDetailField(container, "Assignment", employee.currentAssignment?.displayName ?? "Idle");
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
            
            Debug.Log($"Firing employee: {employee.employeeName}");
            EventBus.Publish(new RequestFireEmployeeEvent { EmployeeId = employee.employeeId });
            CloseEmployeeDetailDialog();
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
        
        void AddTraitSection(VisualElement container, Employee employee)
        {
            if (TechMogul.Traits.TraitSystem.Instance == null)
            {
                var noTraitLabel = new Label("Trait system not initialized");
                noTraitLabel.style.fontSize = 13;
                noTraitLabel.style.color = new Color(0.59f, 0.59f, 0.63f);
                noTraitLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                container.Add(noTraitLabel);
                return;
            }
            
            var traitSystem = TechMogul.Traits.TraitSystem.Instance;
            
            // Get traits
            TechMogul.Traits.TraitDefinitionSO majorTrait = null;
            List<TechMogul.Traits.TraitDefinitionSO> minorTraits = new List<TechMogul.Traits.TraitDefinitionSO>();
            
            if (!string.IsNullOrEmpty(employee.majorTraitId))
            {
                majorTrait = traitSystem.GetTrait(employee.majorTraitId);
            }
            
            if (employee.minorTraitIds != null)
            {
                for (int i = 0; i < employee.minorTraitIds.Count; i++)
                {
                    var minorTrait = traitSystem.GetTrait(employee.minorTraitIds[i]);
                    if (minorTrait != null)
                    {
                        minorTraits.Add(minorTrait);
                    }
                }
            }
            
            // Display major trait
            if (majorTrait != null)
            {
                var majorContainer = new VisualElement();
                majorContainer.style.marginBottom = 8;
                majorContainer.style.paddingTop = 8;
                majorContainer.style.paddingBottom = 8;
                majorContainer.style.paddingLeft = 10;
                majorContainer.style.paddingRight = 10;
                majorContainer.style.backgroundColor = new Color(0.2f, 0.15f, 0.3f, 0.4f);
                majorContainer.style.borderTopLeftRadius = 4;
                majorContainer.style.borderTopRightRadius = 4;
                majorContainer.style.borderBottomLeftRadius = 4;
                majorContainer.style.borderBottomRightRadius = 4;
                
                var majorLabel = new Label(majorTrait.displayName);
                majorLabel.style.fontSize = 14;
                majorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                majorLabel.style.color = new Color(0.8f, 0.6f, 1.0f);
                majorLabel.style.marginBottom = 3;
                
                var majorDesc = new Label(majorTrait.description);
                majorDesc.style.fontSize = 11;
                majorDesc.style.color = new Color(0.71f, 0.71f, 0.75f);
                majorDesc.style.whiteSpace = WhiteSpace.Normal;
                
                majorContainer.Add(majorLabel);
                majorContainer.Add(majorDesc);
                container.Add(majorContainer);
            }
            else
            {
                var noMajorLabel = new Label("No major trait");
                noMajorLabel.style.fontSize = 12;
                noMajorLabel.style.color = new Color(0.59f, 0.59f, 0.63f);
                noMajorLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                container.Add(noMajorLabel);
            }
            
            // Display minor traits
            if (minorTraits.Count > 0)
            {
                var minorsContainer = new VisualElement();
                minorsContainer.style.marginTop = 8;
                
                for (int i = 0; i < minorTraits.Count; i++)
                {
                    var minorTrait = minorTraits[i];
                    
                    var minorContainer = new VisualElement();
                    minorContainer.style.marginBottom = 6;
                    minorContainer.style.paddingLeft = 4;
                    
                    var minorRow = new VisualElement();
                    minorRow.style.flexDirection = FlexDirection.Row;
                    minorRow.style.marginBottom = 2;
                    
                    var bullet = new Label("•");
                    bullet.style.fontSize = 12;
                    bullet.style.color = new Color(0.5f, 0.7f, 0.9f);
                    bullet.style.marginRight = 6;
                    bullet.style.unityTextAlign = TextAnchor.UpperLeft;
                    bullet.style.flexShrink = 0;
                    
                    var minorLabel = new Label(minorTrait.displayName);
                    minorLabel.style.fontSize = 12;
                    minorLabel.style.color = new Color(0.71f, 0.71f, 0.75f);
                    minorLabel.style.whiteSpace = WhiteSpace.Normal;
                    minorLabel.style.flexShrink = 1;
                    minorLabel.style.flexGrow = 1;
                    
                    minorRow.Add(bullet);
                    minorRow.Add(minorLabel);
                    minorContainer.Add(minorRow);
                    
                    if (!string.IsNullOrEmpty(minorTrait.description))
                    {
                        var minorDesc = new Label(minorTrait.description);
                        minorDesc.style.fontSize = 10;
                        minorDesc.style.color = new Color(0.59f, 0.59f, 0.63f);
                        minorDesc.style.whiteSpace = WhiteSpace.Normal;
                        minorDesc.style.marginLeft = 18;
                        minorDesc.style.marginTop = 2;
                        minorContainer.Add(minorDesc);
                    }
                    
                    minorsContainer.Add(minorContainer);
                }
                
                container.Add(minorsContainer);
            }
            
            // Display active synergies
            var tagCounts = traitSystem.CountTraitTags(employee.majorTraitId, employee.minorTraitIds);
            bool hasSynergies = false;
            
            foreach (var kvp in tagCounts)
            {
                if (kvp.Value >= 2)
                {
                    hasSynergies = true;
                    break;
                }
            }
            
            if (hasSynergies)
            {
                var synergyHeader = new Label("Active Synergies:");
                synergyHeader.style.fontSize = 12;
                synergyHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                synergyHeader.style.color = new Color(0.4f, 0.8f, 0.4f);
                synergyHeader.style.marginTop = 10;
                synergyHeader.style.marginBottom = 4;
                container.Add(synergyHeader);
                
                foreach (var kvp in tagCounts)
                {
                    if (kvp.Value >= 2)
                    {
                        string tier = kvp.Value >= 3 ? "Tier 2" : "Tier 1";
                        var synergyLabel = new Label($"• {kvp.Key} ({tier})");
                        synergyLabel.style.fontSize = 11;
                        synergyLabel.style.color = new Color(0.5f, 0.9f, 0.5f);
                        synergyLabel.style.marginBottom = 2;
                        container.Add(synergyLabel);
                    }
                }
            }
        }
        
        void PopulateMarketPanel()
        {
            if (rivalList == null || RivalSystem.Instance == null)
            {
                Debug.LogWarning("Market panel elements or RivalSystem not ready");
                return;
            }
            
            rivalList.Clear();
            
            var companies = RivalSystem.Instance.AllCompanies;
            
            if (companies.Count == 0)
            {
                Debug.LogWarning("No companies in market yet");
                return;
            }
            
            foreach (var company in companies)
            {
                if (company.IsPlayerCompany)
                    continue;
                    
                var rivalCard = CreateRivalCard(company);
                rivalList.Add(rivalCard);
            }
            
            Debug.Log($"Populated market panel with {companies.Count - 1} rivals");
        }
        
        VisualElement CreateRivalCard(RivalCompany rival)
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
            
            var typeLabel = new Label(rival.Type.ToString());
            typeLabel.style.fontSize = 12;
            typeLabel.style.color = new Color(0.5f, 0.7f, 0.9f);
            typeLabel.style.backgroundColor = new Color(0.1f, 0.2f, 0.3f, 0.5f);
            typeLabel.style.paddingTop = 2;
            typeLabel.style.paddingBottom = 2;
            typeLabel.style.paddingLeft = 6;
            typeLabel.style.paddingRight = 6;
            typeLabel.style.borderTopLeftRadius = 3;
            typeLabel.style.borderTopRightRadius = 3;
            typeLabel.style.borderBottomLeftRadius = 3;
            typeLabel.style.borderBottomRightRadius = 3;
            
            header.Add(nameLabel);
            header.Add(typeLabel);
            
            var statsRow = new VisualElement();
            statsRow.style.flexDirection = FlexDirection.Row;
            statsRow.style.justifyContent = Justify.SpaceBetween;
            statsRow.style.marginTop = 8;
            
            var cashStat = CreateStatElement("Cash", $"${rival.Cash / 1000000f:F1}M");
            var revenueStat = CreateStatElement("Revenue", $"${rival.Revenue / 1000000f:F1}M");
            var categoryStat = CreateStatElement("Categories", rival.CategoryPositions.Count.ToString());
            var strategyStat = CreateStatElement("Strategy", rival.StrategyState.ToString());
            
            statsRow.Add(cashStat);
            statsRow.Add(revenueStat);
            statsRow.Add(categoryStat);
            statsRow.Add(strategyStat);
            
            card.Add(header);
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
            EventBus.Publish(new RequestOpenSaveDialogEvent());
        }
        
        void OnLoadGameClicked()
        {
            Debug.Log("Load Game button clicked");
            EventBus.Publish(new RequestOpenLoadDialogEvent());
        }
    }
}
