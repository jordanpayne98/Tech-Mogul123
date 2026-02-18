using UnityEngine;
using UnityEngine.UIElements;
using TechMogul.Core;
using TechMogul.Data;
using TechMogul.Systems;
using TechMogul.Contracts;

namespace TechMogul.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class HireDialogController : UIController
    {
        [Header("Role Templates")]
        [SerializeField] private RoleSO developerRole;
        [SerializeField] private RoleSO designerRole;
        [SerializeField] private RoleSO marketerRole;
        
        private TimeSpeed previousTimeSpeed;
        private bool wasTimePaused;
        
        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement overlay;
        private VisualElement candidatePanel;
        private VisualElement placeholder;
        
        // Role buttons
        private Button developerBtn;
        private Button designerBtn;
        private Button marketerBtn;
        
        // Candidate info
        private Label candidateName;
        private Label candidateRole;
        private Label devSkillValue;
        private Label designSkillValue;
        private Label marketingSkillValue;
        private VisualElement devSkillBar;
        private VisualElement designSkillBar;
        private VisualElement marketingSkillBar;
        private Label salaryValue;
        private Label signingBonusValue;
        
        // Action buttons
        private Button closeBtn;
        private Button generateBtn;
        private Button hireConfirmBtn;
        
        private RoleSO selectedRole;
        private Employee currentCandidate;
        private ReputationSystem reputationSystem;
        private IRng _rng;
        
        protected override void Awake()
        {
            base.Awake();
            uiDocument = GetComponent<UIDocument>();
            reputationSystem = ServiceLocator.Instance?.Get<ReputationSystem>();
            _rng = new UnityRng();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        
        void Start()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("UIDocument or root visual element is null in Start");
                return;
            }
            
            root = uiDocument.rootVisualElement;
            
            CacheReferences();
            BindButtons();
            
            Hide();
        }
        
        protected override void SubscribeToEvents()
        {
        }
        
        void CacheReferences()
        {
            overlay = root.Q<VisualElement>("overlay");
            Debug.Log($"HireDialog CacheReferences - overlay found: {overlay != null}");
            Debug.Log($"HireDialog CacheReferences - root has {root.childCount} children");
            
            candidatePanel = root.Q<VisualElement>("candidate-panel");
            placeholder = root.Q<VisualElement>("placeholder");
            
            // Role buttons
            developerBtn = root.Q<Button>("developer-btn");
            designerBtn = root.Q<Button>("designer-btn");
            marketerBtn = root.Q<Button>("marketer-btn");
            
            // Candidate info
            candidateName = root.Q<Label>("candidate-name");
            candidateRole = root.Q<Label>("candidate-role");
            devSkillValue = root.Q<Label>("dev-skill-value");
            designSkillValue = root.Q<Label>("design-skill-value");
            marketingSkillValue = root.Q<Label>("marketing-skill-value");
            devSkillBar = root.Q<VisualElement>("dev-skill-bar");
            designSkillBar = root.Q<VisualElement>("design-skill-bar");
            marketingSkillBar = root.Q<VisualElement>("marketing-skill-bar");
            salaryValue = root.Q<Label>("salary-value");
            signingBonusValue = root.Q<Label>("signing-bonus-value");
            
            // Action buttons
            closeBtn = root.Q<Button>("close-btn");
            generateBtn = root.Q<Button>("generate-btn");
            hireConfirmBtn = root.Q<Button>("hire-confirm-btn");
        }
        
        void BindButtons()
        {
            if (closeBtn != null) closeBtn.clicked += Hide;
            if (overlay != null) overlay.RegisterCallback<ClickEvent>(OnOverlayClick);
            
            if (developerBtn != null) developerBtn.clicked += () => SelectRole(developerRole, developerBtn);
            if (designerBtn != null) designerBtn.clicked += () => SelectRole(designerRole, designerBtn);
            if (marketerBtn != null) marketerBtn.clicked += () => SelectRole(marketerRole, marketerBtn);
            
            if (generateBtn != null) generateBtn.clicked += GenerateNewCandidate;
            if (hireConfirmBtn != null) hireConfirmBtn.clicked += HireCandidate;
        }
        
        void OnOverlayClick(ClickEvent evt)
        {
            // Close dialog if clicking on overlay (not the dialog itself)
            if (evt.target == overlay)
            {
                Hide();
            }
        }
        
        public void Show()
        {
            // Ensure _rng is initialized
            if (_rng == null)
            {
                _rng = new UnityRng();
            }
            
            // Ensure references are initialized
            if (overlay == null && uiDocument != null && uiDocument.rootVisualElement != null)
            {
                root = uiDocument.rootVisualElement;
                CacheReferences();
                BindButtons();
            }
            
            Debug.Log($"HireDialog.Show() - overlay is {(overlay != null ? "NOT NULL" : "NULL")}");
            if (overlay != null)
            {
                PauseTime();
                overlay.style.display = DisplayStyle.Flex;
                Debug.Log("HireDialog overlay display set to Flex");
                ResetDialog();
            }
            else
            {
                Debug.LogError("HireDialog overlay is NULL! Cannot show dialog.");
            }
        }
        
        public void Hide()
        {
            if (overlay != null)
            {
                overlay.style.display = DisplayStyle.None;
                ResumeTime();
            }
        }
        
        void ResetDialog()
        {
            selectedRole = null;
            currentCandidate = null;
            
            // Clear role selection
            developerBtn?.RemoveFromClassList("role-btn-selected");
            designerBtn?.RemoveFromClassList("role-btn-selected");
            marketerBtn?.RemoveFromClassList("role-btn-selected");
            
            // Show placeholder, hide candidate panel
            candidatePanel?.AddToClassList("hidden");
            placeholder?.RemoveFromClassList("hidden");
        }
        
        void SelectRole(RoleSO role, Button button)
        {
            if (role == null)
            {
                Debug.LogError("Role is null!");
                return;
            }
            
            selectedRole = role;
            
            // Update button selection
            developerBtn?.RemoveFromClassList("role-btn-selected");
            designerBtn?.RemoveFromClassList("role-btn-selected");
            marketerBtn?.RemoveFromClassList("role-btn-selected");
            button?.AddToClassList("role-btn-selected");
            
            // Generate first candidate
            GenerateNewCandidate();
        }
        
        void GenerateNewCandidate()
        {
            if (selectedRole == null)
            {
                Debug.LogWarning("No role selected");
                return;
            }
            
            // Generate random name
            string randomName = GenerateRandomName();
            
            // Get skill caps from reputation system
            float maxSkill = 20f;
            float minSkill = 0f;
            
            if (reputationSystem != null)
            {
                maxSkill = reputationSystem.GetEmployeeQualityMultiplier();
                minSkill = reputationSystem.GetEmployeeMinSkill();
            }
            
            // Create candidate employee
            currentCandidate = new Employee(selectedRole, randomName, minSkill, maxSkill);
            
            // Update UI
            DisplayCandidate();
        }
        
        void DisplayCandidate()
        {
            if (currentCandidate == null) return;
            
            // Show candidate panel, hide placeholder
            candidatePanel?.RemoveFromClassList("hidden");
            placeholder?.AddToClassList("hidden");
            
            // Update text
            if (candidateName != null) candidateName.text = currentCandidate.employeeName;
            if (candidateRole != null) candidateRole.text = currentCandidate.role.roleName;
            
            // Update skills
            UpdateSkillDisplay(devSkillValue, devSkillBar, currentCandidate.devSkill);
            UpdateSkillDisplay(designSkillValue, designSkillBar, currentCandidate.designSkill);
            UpdateSkillDisplay(marketingSkillValue, marketingSkillBar, currentCandidate.marketingSkill);
            
            float signingBonus = currentCandidate.GetSigningBonus();
            
            if (salaryValue != null) salaryValue.text = $"${currentCandidate.monthlySalary:N0}/month";
            if (signingBonusValue != null) signingBonusValue.text = $"${signingBonus:N0}";
            if (hireConfirmBtn != null) hireConfirmBtn.text = $"Hire (Pay ${signingBonus:N0} now)";
        }
        
        void UpdateSkillDisplay(Label valueLabel, VisualElement barElement, float skillValue)
        {
            if (valueLabel != null) valueLabel.text = Mathf.RoundToInt(skillValue).ToString();
            
            if (barElement != null)
            {
                barElement.style.width = Length.Percent(skillValue);
            }
        }
        
        void HireCandidate()
        {
            if (currentCandidate == null)
            {
                Debug.LogWarning("No candidate to hire");
                return;
            }
            
            EventBus.Publish(new RequestHireEmployeeEvent 
            { 
                RoleTemplate = selectedRole,
                EmployeeName = currentCandidate.employeeName
            });
            
            Debug.Log($"Hiring {currentCandidate.employeeName} as {selectedRole.roleName}");
            
            GenerateNewCandidate();
        }
        
        string GenerateRandomName()
        {
            string[] firstNames = { 
                "Alex", "Jordan", "Taylor", "Morgan", "Casey", "Riley", "Avery", "Quinn",
                "Blake", "Drew", "Sage", "River", "Skyler", "Phoenix", "Rowan", "Dakota",
                "Sam", "Cameron", "Charlie", "Jamie", "Kai", "Jesse", "Finley", "Hayden",
                "Parker", "Reese", "Peyton", "Logan", "Sawyer", "Bailey", "Harper", "Emery",
                "Adrian", "Alexis", "Angel", "Ari", "Ash", "Aspen", "August", "Aubrey",
                "Blake", "Blue", "Brook", "Cairo", "Carson", "Casey", "Cleo", "Denver",
                "Dylan", "Eden", "Ellis", "Evan", "Ezra", "Gray", "Harley", "Hunter",
                "Indie", "Jaden", "Jules", "Justice", "Kendall", "Lane", "London", "Lynn",
                "Marley", "Max", "Milan", "Nico", "Nova", "Ocean", "Onyx", "Orion",
                "Rain", "Raven", "Remy", "Robin", "Rory", "Royal", "Ryan", "Salem"
            };
            string[] lastNames = { 
                "Smith", "Johnson", "Chen", "Patel", "Garcia", "Kim", "Martinez", "Lee",
                "Brown", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "White",
                "Harris", "Martin", "Thompson", "Davis", "Rodriguez", "Lewis", "Walker", "Hall",
                "Allen", "Young", "King", "Wright", "Lopez", "Hill", "Scott", "Green",
                "Adams", "Baker", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner",
                "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart", "Morris",
                "Nguyen", "Murphy", "Rivera", "Cook", "Rogers", "Morgan", "Peterson", "Cooper",
                "Reed", "Bailey", "Bell", "Gomez", "Kelly", "Howard", "Ward", "Cox",
                "Diaz", "Richardson", "Wood", "Watson", "Brooks", "Bennett", "Gray", "James",
                "Reyes", "Cruz", "Hughes", "Price", "Myers", "Long", "Foster", "Sanders"
            };
            
            string firstName = firstNames[_rng.Range(0, firstNames.Length)];
            string lastName = lastNames[_rng.Range(0, lastNames.Length)];
            return $"{firstName} {lastName}";
        }
        
        void PauseTime()
        {
            var timeSystem = FindFirstObjectByType<TimeSystem>();
            if (timeSystem != null)
            {
                previousTimeSpeed = timeSystem.CurrentSpeed;
                wasTimePaused = (previousTimeSpeed == TimeSpeed.Paused);
                
                if (!wasTimePaused)
                {
                    EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Paused });
                }
            }
        }
        
        void ResumeTime()
        {
            if (!wasTimePaused)
            {
                EventBus.Publish(new RequestChangeSpeedEvent { Speed = previousTimeSpeed });
            }
        }
    }
}
