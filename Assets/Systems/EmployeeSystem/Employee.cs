using UnityEngine;
using System;
using System.Collections.Generic;
using TechMogul.Data;
using TechMogul.Traits;

namespace TechMogul.Systems
{
    [System.Serializable]
    public class Employee
    {
        public string employeeId;
        public string employeeName;
        public RoleSO role;
        
        [Header("Traits")]
        public string majorTraitId;
        public List<string> minorTraitIds = new List<string>();
        
        [Header("Skills")]
        public float devSkill;
        public float designSkill;
        public float marketingSkill;
        
        [Header("Skill History")]
        public List<SkillSnapshot> skillHistory = new List<SkillSnapshot>();
        
        [Header("Well-being")]
        public float morale;
        public float burnout;
        public float stress;
        public float fatigue;
        
        [Header("Work Performance")]
        public float productivity;
        public float qualityContribution;
        
        [Header("Work")]
        public float monthlySalary;
        public EmployeeAssignment currentAssignment;
        public bool isAvailable;
        
        [Header("Experience")]
        public int daysSinceHired;
        public int totalProjectsCompleted;
        public int totalContractsCompleted;
        
        public Employee(RoleSO roleTemplate, string name, float minSkill, float maxSkill)
        {
            var generator = new EmployeeGenerator();
            var generatedData = generator.GenerateEmployee(roleTemplate, minSkill, maxSkill);
            
            InitializeFromGenerated(roleTemplate, name, generatedData);
        }
        
        public Employee(RoleSO roleTemplate, string name, GeneratedEmployeeData generatedData)
        {
            InitializeFromGenerated(roleTemplate, name, generatedData);
        }
        
        void InitializeFromGenerated(RoleSO roleTemplate, string name, GeneratedEmployeeData generatedData)
        {
            employeeId = Guid.NewGuid().ToString();
            employeeName = name;
            role = roleTemplate;
            
            devSkill = generatedData.devSkill;
            designSkill = generatedData.designSkill;
            marketingSkill = generatedData.marketingSkill;
            morale = generatedData.morale;
            burnout = generatedData.burnout;
            stress = generatedData.stress;
            fatigue = generatedData.fatigue;
            productivity = generatedData.productivity;
            qualityContribution = generatedData.qualityContribution;
            monthlySalary = generatedData.monthlySalary;
            
            majorTraitId = generatedData.majorTraitId;
            minorTraitIds = generatedData.minorTraitIds != null ? generatedData.minorTraitIds : new List<string>();
            
            currentAssignment = EmployeeAssignment.Idle();
            isAvailable = true;
            daysSinceHired = 0;
            totalProjectsCompleted = 0;
            totalContractsCompleted = 0;
            
            RecordSkillSnapshot();
        }
        
        public float GetSigningBonus()
        {
            return monthlySalary * 0.5f;
        }
        
        public void RecordSkillSnapshot()
        {
            skillHistory.Add(new SkillSnapshot
            {
                day = daysSinceHired,
                devSkill = devSkill,
                designSkill = designSkill,
                marketingSkill = marketingSkill
            });
            
            if (skillHistory.Count > 100)
            {
                skillHistory.RemoveAt(0);
            }
        }
        
        public void AssignToWork(EmployeeAssignment assignment)
        {
            currentAssignment = assignment;
            isAvailable = false;
        }
        
        public void CompleteAssignment()
        {
            currentAssignment = EmployeeAssignment.Idle();
            isAvailable = true;
        }
        
        public void AddBurnout(float amount)
        {
            burnout = Mathf.Clamp(burnout + amount, 0, 100);
            
            if (burnout > 80)
            {
                morale = Mathf.Max(morale - 5f, 0);
            }
        }
        
        public void RecoverBurnout(float amount)
        {
            burnout = Mathf.Max(burnout - amount, 0);
        }
        
        public void ChangeMorale(float amount)
        {
            morale = Mathf.Clamp(morale + amount, 0, 100);
        }
        
        public void ImproveSkill(SkillType skillType, float amount)
        {
            bool skillChanged = false;
            
            switch (skillType)
            {
                case SkillType.Development:
                    float oldDev = devSkill;
                    devSkill = Mathf.Min(devSkill + amount, 100);
                    skillChanged = oldDev != devSkill;
                    break;
                case SkillType.Design:
                    float oldDesign = designSkill;
                    designSkill = Mathf.Min(designSkill + amount, 100);
                    skillChanged = oldDesign != designSkill;
                    break;
                case SkillType.Marketing:
                    float oldMarketing = marketingSkill;
                    marketingSkill = Mathf.Min(marketingSkill + amount, 100);
                    skillChanged = oldMarketing != marketingSkill;
                    break;
            }
            
            if (skillChanged)
            {
                RecordSkillSnapshot();
            }
        }
        
        public float GetSkill(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Development => devSkill,
                SkillType.Design => designSkill,
                SkillType.Marketing => marketingSkill,
                _ => 0
            };
        }
        
        public float GetAverageSkill()
        {
            return (devSkill + designSkill + marketingSkill) / 3f;
        }
        
        public float GetEffectiveSkill(SkillType skillType)
        {
            float baseSkill = GetSkill(skillType);
            float moraleModifier = (morale - 50f) / 100f;
            float burnoutModifier = -burnout / 200f;
            
            float effectiveSkill = baseSkill * (1f + moraleModifier + burnoutModifier);
            return Mathf.Clamp(effectiveSkill, 0, 100);
        }
        
        public string GetTraitSummary()
        {
            if (string.IsNullOrEmpty(majorTraitId))
            {
                return "No traits";
            }
            
            string summary = majorTraitId;
            if (minorTraitIds != null && minorTraitIds.Count > 0)
            {
                summary += $" + {minorTraitIds.Count} minors";
            }
            return summary;
        }
    }
    
    public enum SkillType
    {
        Development,
        Design,
        Marketing
    }
    
    [System.Serializable]
    public class SkillSnapshot
    {
        public int day;
        public float devSkill;
        public float designSkill;
        public float marketingSkill;
    }
}
