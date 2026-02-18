using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Products;
using TechMogul.Traits;

namespace TechMogul.Systems
{
    public class WorkSimulation
    {
        private const float BASE_STRESS_RATE = 2f;
        private const float BASE_FATIGUE_RATE = 3f;
        private const float STRESS_RECOVERY_RATE = 5f;
        private const float FATIGUE_RECOVERY_RATE = 8f;
        private const float MORALE_DECLINE_FROM_STRESS = 0.5f;
        private const float BURNOUT_FROM_STRESS_THRESHOLD = 70f;
        private const float BURNOUT_INCREASE_RATE = 1f;
        
        public static DailyWorkResult ProcessDailyWork(Employee employee, ProductData product)
        {
            if (employee == null || product == null)
            {
                return new DailyWorkResult();
            }
            
            Dictionary<StatType, float> traitModifiers = TraitIntegration.EvaluateEmployeeTraits(employee, 1, 1.0f);
            
            float roleFit = CalculateRoleFit(employee, product.category);
            float moraleMultiplier = Mathf.Clamp01(employee.morale / 100f);
            float fatigueMultiplier = 1f - Mathf.Clamp01(employee.fatigue / 200f);
            
            float baseProductivity = employee.productivity * roleFit * moraleMultiplier * fatigueMultiplier;
            float traitProductivity = TraitIntegration.ApplyTraitModifier(traitModifiers, StatType.Productivity, baseProductivity);
            
            float varianceMultiplier = 1.0f;
            if (TraitSystem.Instance != null)
            {
                varianceMultiplier = TraitSystem.Instance.RollProductivityVariance(traitModifiers);
            }
            
            float effectiveProductivity = traitProductivity * varianceMultiplier;
            
            float dailyProgress = effectiveProductivity / 10f;
            
            float baseBugRate = 1f - Mathf.Clamp01(employee.qualityContribution / 100f);
            float traitBugRate = TraitIntegration.ApplyTraitModifier(traitModifiers, StatType.BugRate, baseBugRate);
            float phaseBugMultiplier = GetPhaseBugMultiplier(product.currentPhase);
            float stressBugMultiplier = 1f + Mathf.Clamp01(employee.stress / 200f);
            
            float bugsAdded = dailyProgress * traitBugRate * phaseBugMultiplier * stressBugMultiplier;
            
            float baseQuality = employee.qualityContribution / 100f;
            float traitQuality = TraitIntegration.ApplyTraitModifier(traitModifiers, StatType.Quality, baseQuality);
            float qualityAdded = dailyProgress * traitQuality;
            
            return new DailyWorkResult
            {
                progressAdded = dailyProgress,
                bugsAdded = bugsAdded,
                qualityAdded = qualityAdded,
                roleFit = roleFit,
                effectiveProductivity = effectiveProductivity
            };
        }
        
        public static void UpdateEmployeeStress(Employee employee, bool isWorking, int activeAssignments)
        {
            Dictionary<StatType, float> traitModifiers = TraitIntegration.EvaluateEmployeeTraits(employee, 1, 1.0f);
            
            if (isWorking)
            {
                float workloadFactor = Mathf.Max(1f, activeAssignments);
                float burnoutMultiplier = 1f + (employee.burnout / 100f);
                float dailyStress = BASE_STRESS_RATE * workloadFactor * burnoutMultiplier;
                
                employee.stress = Mathf.Min(100f, employee.stress + dailyStress);
                
                float dailyFatigue = BASE_FATIGUE_RATE * workloadFactor;
                employee.fatigue = Mathf.Min(100f, employee.fatigue + dailyFatigue);
            }
            else
            {
                float baseRecovery = STRESS_RECOVERY_RATE;
                float traitRecovery = TraitIntegration.ApplyTraitModifier(traitModifiers, StatType.StressRecoveryRate, baseRecovery);
                
                employee.stress = Mathf.Max(0f, employee.stress - traitRecovery);
                employee.fatigue = Mathf.Max(0f, employee.fatigue - FATIGUE_RECOVERY_RATE);
            }
        }
        
        public static void UpdateEmployeeMorale(Employee employee)
        {
            Dictionary<StatType, float> traitModifiers = TraitIntegration.EvaluateEmployeeTraits(employee, 1, 1.0f);
            
            float moraleModifier = TraitIntegration.GetTraitModifier(traitModifiers, StatType.Morale);
            
            if (employee.stress > BURNOUT_FROM_STRESS_THRESHOLD)
            {
                float moraleDecline = MORALE_DECLINE_FROM_STRESS * (employee.stress / 100f);
                employee.morale = Mathf.Max(0f, employee.morale - moraleDecline + moraleModifier);
            }
            else if (employee.stress < 30f && employee.fatigue < 30f)
            {
                employee.morale = Mathf.Min(100f, employee.morale + 0.2f + moraleModifier);
            }
        }
        
        public static void UpdateEmployeeBurnout(Employee employee)
        {
            Dictionary<StatType, float> traitModifiers = TraitIntegration.EvaluateEmployeeTraits(employee, 1, 1.0f);
            
            if (employee.stress > BURNOUT_FROM_STRESS_THRESHOLD)
            {
                float baseBurnoutRate = BURNOUT_INCREASE_RATE * (employee.stress / 100f);
                float traitBurnoutRate = TraitIntegration.ApplyTraitModifier(traitModifiers, StatType.BurnoutRate, baseBurnoutRate);
                
                employee.burnout = Mathf.Min(100f, employee.burnout + traitBurnoutRate);
            }
            else if (employee.stress < 20f)
            {
                employee.burnout = Mathf.Max(0f, employee.burnout - 0.1f);
            }
        }
        
        private static float CalculateRoleFit(Employee employee, TechMogul.Data.ProductCategorySO category)
        {
            if (category == null) return 0.5f;
            
            float weightedSkill = 
                (employee.devSkill * category.devSkillWeight) +
                (employee.designSkill * category.designSkillWeight) +
                (employee.marketingSkill * category.marketingSkillWeight);
            
            return Mathf.Clamp01(weightedSkill / 100f);
        }
        
        private static float GetPhaseBugMultiplier(TechMogul.Products.ProjectPhase phase)
        {
            switch (phase)
            {
                case TechMogul.Products.ProjectPhase.Implementation:
                    return 1.5f;
                case TechMogul.Products.ProjectPhase.BugFix:
                    return 0.5f;
                case TechMogul.Products.ProjectPhase.Polish:
                    return 0.3f;
                default:
                    return 1.0f;
            }
        }
    }
    
    public class DailyWorkResult
    {
        public float progressAdded;
        public float bugsAdded;
        public float qualityAdded;
        public float roleFit;
        public float effectiveProductivity;
    }
}
