using System;
using System.Collections.Generic;
using TechMogul.Systems;
using TechMogul.Data;

namespace TechMogul.Core.Save
{
    [Serializable]
    public class SerializableEmployee
    {
        public string employeeId;
        public string employeeName;
        public string roleId;
        
        public string majorTraitId;
        public List<string> minorTraitIds;
        
        public float devSkill;
        public float designSkill;
        public float marketingSkill;
        
        public List<SkillSnapshot> skillHistory;
        
        public float morale;
        public float burnout;
        public float stress;
        public float fatigue;
        public float productivity;
        public float qualityContribution;
        
        public float monthlySalary;
        public string assignmentType;
        public string assignmentId;
        public string assignmentDisplayName;
        public bool isAvailable;
        
        public int daysSinceHired;
        public int totalProjectsCompleted;
        public int totalContractsCompleted;
        
        public static SerializableEmployee FromEmployee(Employee employee, IDefinitionResolver resolver)
        {
            if (resolver == null)
            {
                UnityEngine.Debug.LogError("IDefinitionResolver is null in SerializableEmployee.FromEmployee");
                return null;
            }
            
            string roleId = string.Empty;
            if (employee.role != null)
            {
                roleId = resolver.GetId(employee.role);
            }
            
            return new SerializableEmployee
            {
                employeeId = employee.employeeId,
                employeeName = employee.employeeName,
                roleId = roleId,
                
                majorTraitId = employee.majorTraitId ?? string.Empty,
                minorTraitIds = employee.minorTraitIds != null ? new List<string>(employee.minorTraitIds) : new List<string>(),
                
                devSkill = employee.devSkill,
                designSkill = employee.designSkill,
                marketingSkill = employee.marketingSkill,
                
                skillHistory = employee.skillHistory != null ? new List<SkillSnapshot>(employee.skillHistory) : new List<SkillSnapshot>(),
                
                morale = employee.morale,
                burnout = employee.burnout,
                stress = employee.stress,
                fatigue = employee.fatigue,
                productivity = employee.productivity,
                qualityContribution = employee.qualityContribution,
                
                monthlySalary = employee.monthlySalary,
                assignmentType = employee.currentAssignment?.assignmentType.ToString() ?? AssignmentType.Idle.ToString(),
                assignmentId = employee.currentAssignment?.assignmentId ?? string.Empty,
                assignmentDisplayName = employee.currentAssignment?.displayName ?? "Idle",
                isAvailable = employee.isAvailable,
                
                daysSinceHired = employee.daysSinceHired,
                totalProjectsCompleted = employee.totalProjectsCompleted,
                totalContractsCompleted = employee.totalContractsCompleted
            };
        }
        
        public Employee ToEmployee(IDefinitionResolver resolver)
        {
            if (resolver == null)
            {
                UnityEngine.Debug.LogError("IDefinitionResolver is null in SerializableEmployee.ToEmployee");
                return null;
            }
            
            RoleSO role = null;
            if (!string.IsNullOrEmpty(roleId))
            {
                role = resolver.Resolve<RoleSO>(roleId);
                if (role == null)
                {
                    UnityEngine.Debug.LogWarning($"Failed to resolve role with ID: {roleId}");
                }
            }
            
            var generatedData = new GeneratedEmployeeData
            {
                devSkill = this.devSkill,
                designSkill = this.designSkill,
                marketingSkill = this.marketingSkill,
                morale = this.morale,
                burnout = this.burnout,
                stress = this.stress,
                fatigue = this.fatigue,
                productivity = this.productivity,
                qualityContribution = this.qualityContribution,
                monthlySalary = this.monthlySalary,
                majorTraitId = this.majorTraitId ?? string.Empty,
                minorTraitIds = this.minorTraitIds != null ? new List<string>(this.minorTraitIds) : new List<string>()
            };
            
            Employee employee = new Employee(role, employeeName, generatedData)
            {
                employeeId = this.employeeId,
                skillHistory = this.skillHistory != null ? new List<SkillSnapshot>(this.skillHistory) : new List<SkillSnapshot>(),
                currentAssignment = DeserializeAssignment(),
                isAvailable = this.isAvailable,
                daysSinceHired = this.daysSinceHired,
                totalProjectsCompleted = this.totalProjectsCompleted,
                totalContractsCompleted = this.totalContractsCompleted
            };
            
            return employee;
        }
        
        private EmployeeAssignment DeserializeAssignment()
        {
            if (string.IsNullOrEmpty(assignmentType) || !Enum.TryParse<AssignmentType>(assignmentType, out var type))
            {
                return EmployeeAssignment.Idle();
            }
            
            return new EmployeeAssignment
            {
                assignmentType = type,
                assignmentId = assignmentId ?? string.Empty,
                displayName = assignmentDisplayName ?? "Idle"
            };
        }
    }
}
