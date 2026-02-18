using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Core.Save;
using TechMogul.Data;
using TechMogul.Contracts;
using TechMogul.Traits;

namespace TechMogul.Systems
{
    public class EmployeeSystem : GameSystem
    {
        public static EmployeeSystem Instance { get; private set; }
        
        [Header("Settings")]
        [SerializeField] private float dailyBurnoutRecovery = 1f;
        [SerializeField] private float dailyMoraleChange = 0.5f;
        [SerializeField] private float burnoutThreshold = 80f;
        [SerializeField] private float lowMoraleThreshold = 30f;
        
        [Header("Name Generation")]
        [SerializeField] private string[] firstNames = { 
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
        [SerializeField] private string[] lastNames = { 
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
        
        private List<Employee> _employees = new List<Employee>();
        private List<(string employeeName, float salary)> _pendingSeverancePayments = new List<(string, float)>();
        private int _employeeCounter = 0;
        private ReputationSystem _reputationSystem;
        private TraitSystem _traitSystem;
        private IRng _rng;
        
        public IReadOnlyList<Employee> Employees => _employees.AsReadOnly();
        public IReadOnlyList<(string employeeName, float salary)> PendingSeverancePayments => _pendingSeverancePayments.AsReadOnly();
        
        protected override void Awake()
        {
            base.Awake();
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            _reputationSystem = ServiceLocator.Instance.Get<ReputationSystem>();
            _traitSystem = ServiceLocator.Instance.Get<TraitSystem>();
            _rng = new UnityRng();
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnDayTickEvent>(HandleDayTick);
            Subscribe<OnMonthTickEvent>(HandleMonthTick);
            Subscribe<RequestHireEmployeeEvent>(HandleHireRequest);
            Subscribe<RequestFireEmployeeEvent>(HandleFireRequest);
            Subscribe<RequestAssignEmployeeEvent>(HandleAssignEmployee);
            Subscribe<RequestUnassignEmployeeEvent>(HandleUnassignEmployee);
            Subscribe<RequestAddSkillXPEvent>(HandleAddSkillXP);
            Subscribe<RequestAddBurnoutEvent>(HandleAddBurnout);
            Subscribe<RequestChangeMoraleEvent>(HandleChangeMorale);
            Subscribe<RequestLoadEmployeesEvent>(HandleLoadEmployees);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            _employees.Clear();
            _pendingSeverancePayments.Clear();
            _employeeCounter = 0;
            Debug.Log("EmployeeSystem reset for new game");
        }
        
        void HandleLoadEmployees(RequestLoadEmployeesEvent evt)
        {
            _employees.Clear();
            
            IDefinitionResolver resolver = ServiceLocator.Instance.Get<IDefinitionResolver>();
            if (resolver == null)
            {
                Debug.LogError("IDefinitionResolver not found. Cannot load employees.");
                return;
            }
            
            foreach (var serializedEmployee in evt.Employees)
            {
                Employee employee = serializedEmployee.ToEmployee(resolver);
                if (employee != null)
                {
                    _employees.Add(employee);
                }
            }
            
            _employeeCounter = evt.Employees.Count;
            
            _pendingSeverancePayments.Clear();
            if (evt.PendingSeverancePayments != null)
            {
                foreach (var severance in evt.PendingSeverancePayments)
                {
                    _pendingSeverancePayments.Add((severance.employeeName, severance.salary));
                }
            }
            
            Debug.Log($"Loaded {_employees.Count} employees and {_pendingSeverancePayments.Count} pending severance payments");
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
            foreach (var employee in _employees)
            {
                employee.daysSinceHired++;
                
                if (employee.isAvailable)
                {
                    WorkSimulation.UpdateEmployeeStress(employee, false, 0);
                    employee.RecoverBurnout(dailyBurnoutRecovery);
                }
                
                WorkSimulation.UpdateEmployeeMorale(employee);
                WorkSimulation.UpdateEmployeeBurnout(employee);
                
                if (employee.daysSinceHired % 30 == 0)
                {
                    employee.RecordSkillSnapshot();
                }
            }
        }
        
        void HandleMonthTick(OnMonthTickEvent evt)
        {
            PaySalaries();
        }
        
        void UpdateMorale(Employee employee)
        {
            float moraleChange = 0;
            
            if (employee.burnout > burnoutThreshold)
            {
                moraleChange = -dailyMoraleChange * 2f;
            }
            else if (employee.isAvailable)
            {
                moraleChange = dailyMoraleChange * 0.5f;
            }
            else
            {
                moraleChange = -dailyMoraleChange * 0.25f;
            }
            
            employee.ChangeMorale(moraleChange);
            
            if (employee.morale < lowMoraleThreshold)
            {
                EventBus.Publish(new OnEmployeeLowMoraleEvent { Employee = employee });
            }
        }
        
        void PaySalaries()
        {
            float totalSalaries = 0f;
            
            // Pay active employees
            foreach (var employee in _employees)
            {
                totalSalaries += employee.monthlySalary;
            }
            
            // Pay severance to fired employees
            foreach (var (employeeName, salary) in _pendingSeverancePayments)
            {
                totalSalaries += salary;
                Debug.Log($"💰 Final severance payment of ${salary:N0} to {employeeName}");
            }
            
            _pendingSeverancePayments.Clear();
            
            if (totalSalaries > 0)
            {
                EventBus.Publish(new RequestDeductCashEvent { Amount = totalSalaries });
                Debug.Log($"💵 Paid salaries: ${totalSalaries:N0} to {_employees.Count} active employees");
            }
        }
        
        void HandleHireRequest(RequestHireEmployeeEvent evt)
        {
            if (evt.RoleTemplate == null)
            {
                Debug.LogError("Cannot hire employee: RoleTemplate is null");
                return;
            }
            
            string employeeName = evt.EmployeeName;
            if (string.IsNullOrEmpty(employeeName))
            {
                employeeName = GenerateRandomName();
            }
            
            float maxSkill = 20f;
            float minSkill = 0f;
            
            if (_reputationSystem != null)
            {
                maxSkill = _reputationSystem.GetEmployeeQualityMultiplier();
                minSkill = _reputationSystem.GetEmployeeMinSkill();
            }
            
            var generator = new EmployeeGenerator(_rng);
            if (_traitSystem != null && _traitSystem.Database != null)
            {
                generator.SetTraitDatabase(_traitSystem.Database);
            }
            
            var generatedData = generator.GenerateEmployee(evt.RoleTemplate, minSkill, maxSkill);
            var newEmployee = new Employee(evt.RoleTemplate, employeeName, generatedData);
            
            float signingBonus = newEmployee.GetSigningBonus();
            
            EventBus.Publish(new RequestDeductCashEvent { Amount = signingBonus });
            
            _employees.Add(newEmployee);
            _employeeCounter++;
            
            EventBus.Publish(new OnEmployeeHiredEvent 
            { 
                Employee = newEmployee,
                Name = employeeName,
                Role = evt.RoleTemplate
            });
            
            Debug.Log($"✅ Hired {employeeName} as {evt.RoleTemplate.roleName}");
            Debug.Log($"   Monthly Salary: ${newEmployee.monthlySalary:N0}/month");
            Debug.Log($"   Signing Bonus: ${signingBonus:N0} (2 weeks)");
            Debug.Log($"   Skills: Dev {newEmployee.devSkill:F0}, Design {newEmployee.designSkill:F0}, Marketing {newEmployee.marketingSkill:F0}");
            if (!string.IsNullOrEmpty(newEmployee.majorTraitId))
            {
                Debug.Log($"   Traits: {newEmployee.majorTraitId} + {newEmployee.minorTraitIds.Count} minors");
            }
            Debug.Log($"   Total Employees: {_employees.Count}");
        }
        
        void HandleFireRequest(RequestFireEmployeeEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                Debug.LogWarning($"Cannot fire employee: Employee with ID {evt.EmployeeId} not found");
                return;
            }
            
            if (!employee.isAvailable)
            {
                Debug.LogWarning($"Cannot fire {employee.employeeName}: Currently assigned to {employee.currentAssignment.displayName}");
                return;
            }
            
            string employeeName = employee.employeeName;
            float finalSalary = employee.monthlySalary;
            
            _pendingSeverancePayments.Add((employeeName, finalSalary));
            _employees.Remove(employee);
            
            EventBus.Publish(new OnEmployeeFiredEvent 
            { 
                EmployeeId = employee.employeeId,
                Name = employeeName 
            });
            
            Debug.Log($"🔥 Fired {employeeName}");
            Debug.Log($"   Final salary of ${finalSalary:N0} will be paid at end of month");
            Debug.Log($"   Removed from roster immediately");
        }
        
        void HandleAssignEmployee(RequestAssignEmployeeEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                Debug.LogWarning($"Cannot assign employee: Employee with ID {evt.EmployeeId} not found");
                return;
            }
            
            if (!employee.isAvailable)
            {
                Debug.LogWarning($"Cannot assign {employee.employeeName}: Already assigned to {employee.currentAssignment.displayName}");
                return;
            }
            
            employee.AssignToWork(evt.Assignment);
            
            Debug.Log($"📋 Assigned {employee.employeeName} to {evt.Assignment.displayName}");
        }
        
        void HandleUnassignEmployee(RequestUnassignEmployeeEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                Debug.LogWarning($"Cannot unassign employee: Employee with ID {evt.EmployeeId} not found");
                return;
            }
            
            string previousAssignment = employee.currentAssignment.displayName;
            employee.CompleteAssignment();
            
            Debug.Log($"✅ Unassigned {employee.employeeName} from {previousAssignment}");
        }
        
        void HandleAddSkillXP(RequestAddSkillXPEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                return;
            }
            
            if (evt.DevXP > 0)
            {
                employee.ImproveSkill(SkillType.Development, evt.DevXP);
            }
            
            if (evt.DesignXP > 0)
            {
                employee.ImproveSkill(SkillType.Design, evt.DesignXP);
            }
            
            if (evt.MarketingXP > 0)
            {
                employee.ImproveSkill(SkillType.Marketing, evt.MarketingXP);
            }
            
            Debug.Log($"📈 {employee.employeeName} gained XP - Dev: {evt.DevXP:F1}, Design: {evt.DesignXP:F1}, Marketing: {evt.MarketingXP:F1}");
        }
        
        void HandleAddBurnout(RequestAddBurnoutEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                return;
            }
            
            employee.AddBurnout(evt.Amount);
        }
        
        void HandleChangeMorale(RequestChangeMoraleEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                return;
            }
            
            employee.morale = Mathf.Clamp(employee.morale + evt.Amount, 0, 100);
        }
        
        public Employee GetEmployee(string employeeId)
        {
            return _employees.FirstOrDefault(e => e.employeeId == employeeId);
        }
        
        public List<Employee> GetAllEmployees()
        {
            return new List<Employee>(_employees);
        }
        
        public List<Employee> GetAvailableEmployees()
        {
            return _employees.Where(e => e.isAvailable).ToList();
        }
        
        public List<Employee> GetEmployeesByRole(RoleSO role)
        {
            return _employees.Where(e => e.role == role).ToList();
        }
        
        string GenerateRandomName()
        {
            string firstName = firstNames[_rng.Range(0, firstNames.Length)];
            string lastName = lastNames[_rng.Range(0, lastNames.Length)];
            return $"{firstName} {lastName}";
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Hire Random Developer")]
        void TestHireDeveloper()
        {
            Debug.Log("Attempting to hire developer...");
            
            // Use AssetDatabase to find ScriptableObject assets (Editor only)
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RoleSO");
            Debug.Log($"Found {guids.Length} RoleSO assets in project");
            
            RoleSO devRole = null;
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                RoleSO role = UnityEditor.AssetDatabase.LoadAssetAtPath<RoleSO>(path);
                
                Debug.Log($"  - {role.name}: roleName='{role.roleName}'");
                
                if (role.roleName != null && role.roleName.ToLower().Contains("developer"))
                {
                    devRole = role;
                }
            }
            
            if (devRole != null)
            {
                Debug.Log($"✅ Found Developer role: {devRole.name}");
                EventBus.Publish(new RequestHireEmployeeEvent { RoleTemplate = devRole });
            }
            else
            {
                Debug.LogError("❌ No Developer role found! Make sure DeveloperRole.asset has roleName='Developer'");
            }
        }
        
        [ContextMenu("Hire Random Designer")]
        void TestHireDesigner()
        {
            Debug.Log("Attempting to hire designer...");
            
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RoleSO");
            RoleSO designerRole = null;
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                RoleSO role = UnityEditor.AssetDatabase.LoadAssetAtPath<RoleSO>(path);
                
                if (role.roleName != null && role.roleName.ToLower().Contains("designer"))
                {
                    designerRole = role;
                }
            }
            
            if (designerRole != null)
            {
                Debug.Log($"✅ Found Designer role: {designerRole.name}");
                EventBus.Publish(new RequestHireEmployeeEvent { RoleTemplate = designerRole });
            }
            else
            {
                Debug.LogError("❌ No Designer role found!");
            }
        }
        
        [ContextMenu("Hire Random Marketer")]
        void TestHireMarketer()
        {
            Debug.Log("Attempting to hire marketer...");
            
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RoleSO");
            RoleSO marketerRole = null;
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                RoleSO role = UnityEditor.AssetDatabase.LoadAssetAtPath<RoleSO>(path);
                
                if (role.roleName != null && role.roleName.ToLower().Contains("market"))
                {
                    marketerRole = role;
                }
            }
            
            if (marketerRole != null)
            {
                Debug.Log($"✅ Found Marketer role: {marketerRole.name}");
                EventBus.Publish(new RequestHireEmployeeEvent { RoleTemplate = marketerRole });
            }
            else
            {
                Debug.LogError("❌ No Marketer role found!");
            }
        }
        
        [ContextMenu("Log All Employees")]
        void LogAllEmployees()
        {
            Debug.Log($"=== EMPLOYEE ROSTER ===");
            Debug.Log($"Total Employees: {_employees.Count}");
            
            if (_employees.Count == 0)
            {
                Debug.Log("No employees hired yet.");
                return;
            }
            
            foreach (var emp in _employees)
            {
                Debug.Log($"\n👤 {emp.employeeName} ({emp.role.roleName})");
                Debug.Log($"   Skills: Dev {emp.devSkill:F0}, Design {emp.designSkill:F0}, Marketing {emp.marketingSkill:F0}");
                Debug.Log($"   Morale: {emp.morale:F0}, Burnout: {emp.burnout:F0}");
                Debug.Log($"   Salary: ${emp.monthlySalary:N0}/month");
                Debug.Log($"   Status: {(emp.isAvailable ? "Available" : $"Working on {emp.currentAssignment}")}");
            }
        }
        #endif
    }
    
    // Events
    public class RequestHireEmployeeEvent
    {
        public RoleSO RoleTemplate;
        public string EmployeeName;
    }
    
    public class RequestFireEmployeeEvent
    {
        public string EmployeeId;
    }
    
    public class OnEmployeeHiredEvent
    {
        public Employee Employee;
        public string Name;
        public RoleSO Role;
    }
    
    public class OnEmployeeFiredEvent
    {
        public string EmployeeId;
        public string Name;
    }
    
    public class OnEmployeeLowMoraleEvent
    {
        public Employee Employee;
    }
    
    public class RequestAssignEmployeeEvent
    {
        public string EmployeeId;
        public EmployeeAssignment Assignment;
    }
    
    public class RequestUnassignEmployeeEvent
    {
        public string EmployeeId;
    }
    
    public class RequestAddSkillXPEvent
    {
        public string EmployeeId;
        public float DevXP;
        public float DesignXP;
        public float MarketingXP;
    }
    
    public class RequestAddBurnoutEvent
    {
        public string EmployeeId;
        public float Amount;
    }
    
    public class RequestChangeMoraleEvent
    {
        public string EmployeeId;
        public float Amount;
    }
}
