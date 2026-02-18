using System.Collections.Generic;
using TechMogul.Systems;

namespace TechMogul.Traits
{
    public static class TraitIntegration
    {
        public static EmployeeConditionContext BuildContext(Employee employee, int teamSize = 1, float deadlineRemainingPct = 1.0f)
        {
            return new EmployeeConditionContext
            {
                morale = employee.morale / 100f,
                stress = employee.stress / 100f,
                deadlineRemainingPct = deadlineRemainingPct,
                currentPhase = ProjectPhase.Implementation,
                multitaskingState = teamSize > 1 ? MultitaskingState.Multi : MultitaskingState.Single,
                teamSize = teamSize,
                usesNewTech = false,
                usesOldTech = false,
                projectDayIndex = 0,
                isProjectLead = false,
                pairedWithHigherSkilled = false,
                companyRevenueTrend = false,
                recentlyPromoted = false,
                tenureYears = employee.daysSinceHired / 365f,
                projectComplexity = 3
            };
        }
        
        public static Dictionary<StatType, float> EvaluateEmployeeTraits(Employee employee, int teamSize = 1, float deadlineRemainingPct = 1.0f)
        {
            if (TraitSystem.Instance == null)
            {
                return new Dictionary<StatType, float>();
            }
            
            EmployeeConditionContext context = BuildContext(employee, teamSize, deadlineRemainingPct);
            
            return TraitSystem.Instance.EvaluateEmployeeTraits(
                employee.employeeId,
                employee.majorTraitId,
                employee.minorTraitIds,
                context
            );
        }
        
        public static float ApplyTraitModifier(Dictionary<StatType, float> traitModifiers, StatType stat, float baseValue)
        {
            if (traitModifiers.TryGetValue(stat, out float modifier))
            {
                return baseValue * (1f + modifier);
            }
            return baseValue;
        }
        
        public static float GetTraitModifier(Dictionary<StatType, float> traitModifiers, StatType stat)
        {
            if (traitModifiers.TryGetValue(stat, out float modifier))
            {
                return modifier;
            }
            return 0f;
        }
    }
}
