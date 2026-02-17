using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Data;

namespace TechMogul.Systems
{
    public class EmployeeSystem : MonoBehaviour
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
            "Blake", "Drew", "Sage", "River", "Skyler", "Phoenix", "Rowan", "Dakota"
        };
        [SerializeField] private string[] lastNames = { 
            "Smith", "Johnson", "Chen", "Patel", "Garcia", "Kim", "Martinez", "Lee",
            "Brown", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "White"
        };
        
        private List<Employee> _employees = new List<Employee>();
        private int _employeeCounter = 0;
        
        public IReadOnlyList<Employee> Employees => _employees.AsReadOnly();
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        void OnEnable()
        {
            SubscribeToEvents();
        }
        
        void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        void SubscribeToEvents()
        {
            EventBus.Subscribe<OnDayTickEvent>(HandleDayTick);
            EventBus.Subscribe<OnMonthTickEvent>(HandleMonthTick);
            EventBus.Subscribe<RequestHireEmployeeEvent>(HandleHireRequest);
            EventBus.Subscribe<RequestFireEmployeeEvent>(HandleFireRequest);
            EventBus.Subscribe<RequestAssignEmployeeEvent>(HandleAssignEmployee);
            EventBus.Subscribe<RequestUnassignEmployeeEvent>(HandleUnassignEmployee);
            EventBus.Subscribe<RequestAddSkillXPEvent>(HandleAddSkillXP);
            EventBus.Subscribe<RequestAddBurnoutEvent>(HandleAddBurnout);
            EventBus.Subscribe<RequestLoadEmployeesEvent>(HandleLoadEmployees);
            EventBus.Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<OnDayTickEvent>(HandleDayTick);
            EventBus.Unsubscribe<OnMonthTickEvent>(HandleMonthTick);
            EventBus.Unsubscribe<RequestHireEmployeeEvent>(HandleHireRequest);
            EventBus.Unsubscribe<RequestFireEmployeeEvent>(HandleFireRequest);
            EventBus.Unsubscribe<RequestAssignEmployeeEvent>(HandleAssignEmployee);
            EventBus.Unsubscribe<RequestUnassignEmployeeEvent>(HandleUnassignEmployee);
            EventBus.Unsubscribe<RequestAddSkillXPEvent>(HandleAddSkillXP);
            EventBus.Unsubscribe<RequestAddBurnoutEvent>(HandleAddBurnout);
            EventBus.Unsubscribe<RequestLoadEmployeesEvent>(HandleLoadEmployees);
            EventBus.Unsubscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            _employees.Clear();
            _employeeCounter = 0;
            Debug.Log("EmployeeSystem reset for new game");
        }
        
        void HandleLoadEmployees(RequestLoadEmployeesEvent evt)
        {
            _employees.Clear();
            
            foreach (var serializedEmployee in evt.Employees)
            {
                Employee employee = serializedEmployee.ToEmployee();
                _employees.Add(employee);
            }
            
            _employeeCounter = evt.Employees.Count;
            
            Debug.Log($"Loaded {_employees.Count} employees");
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
            foreach (var employee in _employees)
            {
                employee.daysSinceHired++;
                
                if (employee.isAvailable)
                {
                    employee.RecoverBurnout(dailyBurnoutRecovery);
                }
                
                UpdateMorale(employee);
                
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
            if (_employees.Count == 0) return;
            
            float totalSalaries = 0f;
            var employeesToRemove = new List<Employee>();
            
            foreach (var employee in _employees)
            {
                if (employee.isFired)
                {
                    if (employee.needsFinalPayment)
                    {
                        totalSalaries += employee.monthlySalary;
                        employeesToRemove.Add(employee);
                        Debug.Log($"💰 Final payment of ${employee.monthlySalary:N0} to {employee.employeeName}");
                    }
                }
                else
                {
                    totalSalaries += employee.monthlySalary;
                }
            }
            
            foreach (var employee in employeesToRemove)
            {
                _employees.Remove(employee);
                Debug.Log($"👋 {employee.employeeName} removed from payroll after final payment");
            }
            
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
            
            var newEmployee = new Employee(evt.RoleTemplate, employeeName);
            
            newEmployee.devSkill = evt.DevSkill;
            newEmployee.designSkill = evt.DesignSkill;
            newEmployee.marketingSkill = evt.MarketingSkill;
            newEmployee.morale = evt.Morale;
            newEmployee.burnout = evt.Burnout;
            newEmployee.monthlySalary = evt.MonthlySalary;
            
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
                Debug.LogWarning($"Cannot fire {employee.employeeName}: Currently assigned to {employee.currentAssignment}");
                return;
            }
            
            employee.isFired = true;
            employee.needsFinalPayment = true;
            employee.isAvailable = false;
            
            EventBus.Publish(new OnEmployeeFiredEvent 
            { 
                EmployeeId = employee.employeeId,
                Name = employee.employeeName 
            });
            
            Debug.Log($"🔥 Fired {employee.employeeName}");
            Debug.Log($"   Final salary of ${employee.monthlySalary:N0} will be paid at end of month");
            Debug.Log($"   They will be removed from roster after payment");
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
                Debug.LogWarning($"Cannot assign {employee.employeeName}: Already assigned to {employee.currentAssignment}");
                return;
            }
            
            employee.AssignToWork(evt.AssignmentName);
            
            Debug.Log($"📋 Assigned {employee.employeeName} to {evt.AssignmentName}");
        }
        
        void HandleUnassignEmployee(RequestUnassignEmployeeEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                Debug.LogWarning($"Cannot unassign employee: Employee with ID {evt.EmployeeId} not found");
                return;
            }
            
            string previousAssignment = employee.currentAssignment;
            employee.CompleteAssignment();
            
            Debug.Log($"✅ Unassigned {employee.employeeName} from {previousAssignment}");
        }
        
        void HandleAddSkillXP(RequestAddSkillXPEvent evt)
        {
            var employee = _employees.FirstOrDefault(e => e.employeeId == evt.EmployeeId);
            
            if (employee == null)
            {
                Debug.LogWarning($"Cannot add skill XP: Employee with ID {evt.EmployeeId} not found");
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
                Debug.LogWarning($"Cannot add burnout: Employee with ID {evt.EmployeeId} not found");
                return;
            }
            
            employee.AddBurnout(evt.Amount);
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
            string firstName = firstNames[Random.Range(0, firstNames.Length)];
            string lastName = lastNames[Random.Range(0, lastNames.Length)];
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
        public float DevSkill;
        public float DesignSkill;
        public float MarketingSkill;
        public float Morale;
        public float Burnout;
        public float MonthlySalary;
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
        public string AssignmentName;
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
}
