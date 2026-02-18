using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Traits
{
    [Serializable]
    public class ConditionDef
    {
        public ConditionType type;
        public ComparisonOp op;
        public float value;
        public ProjectPhase phaseValue;
        public MultitaskingState multitaskingValue;
        public EventCategory eventCategory;
        public bool boolValue;
        public List<ConditionDef> compositeConditions = new List<ConditionDef>();
        
        public bool Evaluate(EmployeeConditionContext context)
        {
            switch (type)
            {
                case ConditionType.MoraleCompare:
                    return CompareFloat(context.morale, op, value);
                
                case ConditionType.StressCompare:
                    return CompareFloat(context.stress, op, value);
                
                case ConditionType.DeadlineRemainingPct:
                    return CompareFloat(context.deadlineRemainingPct, op, value);
                
                case ConditionType.ProjectPhaseEquals:
                    return context.currentPhase == phaseValue;
                
                case ConditionType.MultitaskingState:
                    return context.multitaskingState == multitaskingValue;
                
                case ConditionType.TeamSizeCompare:
                    return CompareFloat(context.teamSize, op, value);
                
                case ConditionType.UsesNewTech:
                    return context.usesNewTech;
                
                case ConditionType.UsesOldTech:
                    return context.usesOldTech;
                
                case ConditionType.ProjectDayIndex:
                    return CompareFloat(context.projectDayIndex, op, value);
                
                case ConditionType.IsProjectLead:
                    return context.isProjectLead == boolValue;
                
                case ConditionType.PairedWithHigherSkilledTeammate:
                    return context.pairedWithHigherSkilled == boolValue;
                
                case ConditionType.CompanyRevenueTrend:
                    return context.companyRevenueTrend;
                
                case ConditionType.PromotionState:
                    return context.recentlyPromoted;
                
                case ConditionType.TenureYearsCompare:
                    return CompareFloat(context.tenureYears, op, value);
                
                case ConditionType.ProjectComplexity:
                    return CompareFloat(context.projectComplexity, op, value);
                
                case ConditionType.All:
                    return EvaluateAll(context);
                
                case ConditionType.Any:
                    return EvaluateAny(context);
                
                default:
                    return false;
            }
        }
        
        bool CompareFloat(float a, ComparisonOp operation, float b)
        {
            switch (operation)
            {
                case ComparisonOp.LessThan:
                    return a < b;
                case ComparisonOp.LessThanOrEqual:
                    return a <= b;
                case ComparisonOp.GreaterThan:
                    return a > b;
                case ComparisonOp.GreaterThanOrEqual:
                    return a >= b;
                case ComparisonOp.Equal:
                    return Mathf.Approximately(a, b);
                default:
                    return false;
            }
        }
        
        bool EvaluateAll(EmployeeConditionContext context)
        {
            for (int i = 0; i < compositeConditions.Count; i++)
            {
                if (!compositeConditions[i].Evaluate(context))
                {
                    return false;
                }
            }
            return compositeConditions.Count > 0;
        }
        
        bool EvaluateAny(EmployeeConditionContext context)
        {
            for (int i = 0; i < compositeConditions.Count; i++)
            {
                if (compositeConditions[i].Evaluate(context))
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    public struct EmployeeConditionContext
    {
        public float morale;
        public float stress;
        public float deadlineRemainingPct;
        public ProjectPhase currentPhase;
        public MultitaskingState multitaskingState;
        public int teamSize;
        public bool usesNewTech;
        public bool usesOldTech;
        public int projectDayIndex;
        public bool isProjectLead;
        public bool pairedWithHigherSkilled;
        public bool companyRevenueTrend;
        public bool recentlyPromoted;
        public float tenureYears;
        public int projectComplexity;
    }
}
