using UnityEngine;
using System;
using System.Collections.Generic;
using TechMogul.Data;

namespace TechMogul.Systems
{
    [System.Serializable]
    public class Employee
    {
        public string employeeId;
        public string employeeName;
        public RoleSO role;
        
        [Header("Skills")]
        public float devSkill;
        public float designSkill;
        public float marketingSkill;
        
        [Header("Skill History")]
        public List<SkillSnapshot> skillHistory = new List<SkillSnapshot>();
        
        [Header("Well-being")]
        public float morale;
        public float burnout;
        
        [Header("Work")]
        public float monthlySalary;
        public string currentAssignment;
        public bool isAvailable;
        public bool isFired;
        public bool needsFinalPayment;
        
        [Header("Experience")]
        public int daysSinceHired;
        public int totalProjectsCompleted;
        public int totalContractsCompleted;
        
        public Employee(RoleSO roleTemplate, string name)
        {
            employeeId = Guid.NewGuid().ToString();
            employeeName = name;
            role = roleTemplate;
            
            // Get reputation system to determine skill caps
            var reputationSystem = UnityEngine.Object.FindFirstObjectByType<ReputationSystem>();
            float maxSkill = 20f; // Default to very low if no reputation system
            float minSkill = 0f;
            
            if (reputationSystem != null)
            {
                maxSkill = reputationSystem.GetEmployeeQualityMultiplier();
                minSkill = reputationSystem.GetEmployeeMinSkill();
                UnityEngine.Debug.Log($"[Employee Gen] Reputation: {reputationSystem.CurrentReputation:F0}, Min: {minSkill:F0}, Max: {maxSkill:F0}");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[Employee Gen] ReputationSystem not found! Using default low caps (0-20)");
            }
            
            // Generate skills based on role specialization
            // Primary skill: Use full reputation range
            // Secondary skills: 40-70% of primary skill
            
            float primarySkillValue = UnityEngine.Random.Range(minSkill, maxSkill);
            float secondarySkillMultiplier = UnityEngine.Random.Range(0.4f, 0.7f); // Secondary skills are 40-70% of primary
            float tertiarySkillMultiplier = UnityEngine.Random.Range(0.3f, 0.6f); // Tertiary even lower
            
            // Assign skills based on role
            switch (roleTemplate.roleName)
            {
                case "Developer":
                    devSkill = primarySkillValue * 1.3f; // Boost primary skill
                    designSkill = primarySkillValue * secondarySkillMultiplier;
                    marketingSkill = primarySkillValue * tertiarySkillMultiplier;
                    break;
                    
                case "Designer":
                    devSkill = primarySkillValue * tertiarySkillMultiplier;
                    designSkill = primarySkillValue * 1.3f; // Boost primary skill
                    marketingSkill = primarySkillValue * secondarySkillMultiplier;
                    break;
                    
                case "Marketer":
                    devSkill = primarySkillValue * tertiarySkillMultiplier;
                    designSkill = primarySkillValue * secondarySkillMultiplier;
                    marketingSkill = primarySkillValue * 1.3f; // Boost primary skill
                    break;
                    
                default:
                    // Fallback: balanced skills
                    devSkill = primarySkillValue;
                    designSkill = primarySkillValue * secondarySkillMultiplier;
                    marketingSkill = primarySkillValue * tertiarySkillMultiplier;
                    break;
            }
            
            // Clamp primary skill to max (with 1.3× boost allowance)
            float boostedMax = maxSkill * 1.3f;
            
            // Round and clamp all skills
            devSkill = Mathf.Round(Mathf.Clamp(devSkill, 1f, boostedMax));
            designSkill = Mathf.Round(Mathf.Clamp(designSkill, 1f, maxSkill));
            marketingSkill = Mathf.Round(Mathf.Clamp(marketingSkill, 1f, maxSkill));
            
            // Ensure all skills are at least 1
            devSkill = Mathf.Max(devSkill, 1f);
            designSkill = Mathf.Max(designSkill, 1f);
            marketingSkill = Mathf.Max(marketingSkill, 1f);
            
            UnityEngine.Debug.Log($"[Employee Gen] {roleTemplate.roleName} created: Dev {devSkill}, Design {designSkill}, Marketing {marketingSkill}");
            
            morale = UnityEngine.Random.Range(70f, 90f);
            burnout = UnityEngine.Random.Range(0f, 20f);
            
            // Calculate salary based on skills (higher skills = higher salary)
            monthlySalary = CalculateSalaryFromSkills(roleTemplate);
            
            currentAssignment = "None";
            isAvailable = true;
            isFired = false;
            needsFinalPayment = false;
            daysSinceHired = 0;
            totalProjectsCompleted = 0;
            totalContractsCompleted = 0;
            
            RecordSkillSnapshot();
        }
        
        public float GetSigningBonus()
        {
            // Signing bonus is 2 weeks salary (half a month)
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
        
        float GenerateWeightedSkill(float min, float max, float rarityCurve)
        {
            // Use power curve to make higher values rarer
            // rarityCurve of 2.5 means: 50% chance gives 18% of range, 90% gives 66% of range
            float random = UnityEngine.Random.value; // 0-1
            float weighted = Mathf.Pow(random, rarityCurve); // Apply power curve
            
            // Map to skill range
            float skill = min + (weighted * (max - min));
            
            // Clamp and return
            return Mathf.Clamp(skill, min, max);
        }
        
        float CalculateSalaryFromSkills(RoleSO roleTemplate)
        {
            // Calculate average skill level
            float averageSkill = (devSkill + designSkill + marketingSkill) / 3f;
            
            // Salary scales with skill but flattens at high levels
            // Low skills (0-20) = $500-$1,200/month
            // Mid skills (40-60) = $1,800-$3,200/month
            // High skills (80-100) = $3,200-$5,500/month
            
            // Use logarithmic scaling to prevent exponential growth at high skills
            // This keeps elite employees affordable
            float baseSalaryPerSkill = 50f;
            
            // Apply diminishing returns at high skill levels
            // Linear up to 60, then logarithmic curve flattens growth
            float adjustedSkill = averageSkill;
            if (averageSkill > 60f)
            {
                float excess = averageSkill - 60f;
                // Logarithmic scaling: each 10 points above 60 is worth progressively less
                adjustedSkill = 60f + (excess * 0.65f); // 35% diminishing returns above 60
            }
            
            float calculatedSalary = adjustedSkill * baseSalaryPerSkill;
            
            // Add role specialization bonus (smaller bonus for balance)
            float primarySkill = Mathf.Max(devSkill, Mathf.Max(designSkill, marketingSkill));
            float specializationBonus = (primarySkill / 100f) * calculatedSalary * 0.12f;
            calculatedSalary += specializationBonus;
            
            // Add small random variance (±8%) for variety
            float variance = calculatedSalary * 0.08f;
            float finalSalary = calculatedSalary + UnityEngine.Random.Range(-variance, variance);
            
            // Ensure minimum viable salary
            finalSalary = Mathf.Max(finalSalary, 500f);
            
            // Round to nearest 50 for cleaner numbers
            return Mathf.Round(finalSalary / 50f) * 50f;
        }
        
        public void AssignToWork(string assignmentName)
        {
            currentAssignment = assignmentName;
            isAvailable = false;
        }
        
        public void CompleteAssignment()
        {
            currentAssignment = "None";
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
