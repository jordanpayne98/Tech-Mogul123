using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Data;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class GoalEvaluator
    {
        private readonly EmployeeSystem _employeeSystem;
        
        public GoalEvaluator(EmployeeSystem employeeSystem)
        {
            _employeeSystem = employeeSystem;
        }
        
        public bool EvaluateGoal(ContractData contract, GoalDefinition goal, ContractTracking tracking)
        {
            switch (goal.type)
            {
                // Skill-Based Goals
                case GoalType.ReachDevQuality:
                    return tracking.finalQuality >= goal.GetAdjustedTargetValue(contract.difficulty);
                    
                case GoalType.ReachDesignQuality:
                    return tracking.avgDesignSkill >= goal.GetAdjustedTargetValue(contract.difficulty);
                    
                case GoalType.ReachMarketingQuality:
                    return tracking.avgMarketingSkill >= goal.GetAdjustedTargetValue(contract.difficulty);
                    
                case GoalType.NoSkillBelowThreshold:
                    return tracking.lowestEffectiveSkill >= goal.GetAdjustedThreshold(contract.difficulty);
                    
                case GoalType.BalancedSkillDistribution:
                    float ratio = tracking.lowestSkill / tracking.highestSkill;
                    return ratio >= (goal.percentageTarget / 100f);
                    
                case GoalType.OverperformDevRequirement:
                    return tracking.avgDevSkill >= contract.requiredDevSkill * (1f + goal.percentageTarget / 100f);
                    
                case GoalType.DeliverHighQuality:
                    return tracking.finalQuality >= goal.GetAdjustedTargetValue(contract.difficulty);
                    
                case GoalType.LowSkillDeficit:
                    float deficit = CalculateSkillDeficit(contract, tracking);
                    return deficit <= goal.percentageTarget;
                    
                case GoalType.MaintainAvgEffectiveSkill:
                    return tracking.avgEffectiveSkill >= goal.GetAdjustedTargetValue(contract.difficulty);
                    
                case GoalType.NoLowMoraleDuringProject:
                    return tracking.lowestMorale >= goal.GetAdjustedThreshold(contract.difficulty);
                
                // Deadline & Time Goals
                case GoalType.FinishDaysEarly:
                    int daysUsed = contract.completionDay - contract.startDay;
                    int daysEarly = contract.totalDays - daysUsed;
                    return daysEarly >= goal.GetAdjustedIntegerTarget(contract.difficulty);
                    
                case GoalType.FinishPercentFaster:
                    float timeUsed = (float)(contract.completionDay - contract.startDay) / contract.totalDays;
                    return timeUsed <= (1f - goal.percentageTarget / 100f);
                    
                case GoalType.NotExceedTimePercent:
                    float percentUsed = (float)(contract.completionDay - contract.startDay) / contract.totalDays;
                    return percentUsed <= (goal.percentageTarget / 100f);
                    
                case GoalType.FinishExactDeadline:
                    return (contract.completionDay - contract.startDay) == contract.totalDays;
                    
                case GoalType.FinishFirstHalf:
                    return (contract.completionDay - contract.startDay) <= (contract.totalDays / 2);
                    
                case GoalType.ConsistentProgress:
                    return tracking.longestStallDays < goal.GetAdjustedIntegerTarget(contract.difficulty);
                    
                case GoalType.NoDaysZeroProgress:
                    return tracking.zeroDaysCount == 0;
                    
                case GoalType.ReachProgressMilestone:
                    return tracking.reachedMilestone;
                    
                case GoalType.NoOvertimeBurnout:
                    return tracking.maxBurnout <= goal.GetAdjustedThreshold(contract.difficulty);
                    
                case GoalType.NoBurnoutSpikes:
                    return tracking.maxBurnout <= goal.GetAdjustedThreshold(contract.difficulty);
                
                // Team Composition Goals
                case GoalType.UseMaxEmployees:
                    return tracking.maxTeamSize <= goal.GetAdjustedIntegerTarget(contract.difficulty);
                    
                case GoalType.UseMinEmployees:
                    return tracking.maxTeamSize >= goal.GetAdjustedIntegerTarget(contract.difficulty);
                    
                case GoalType.IncludeDesigner:
                    return tracking.hadDesigner;
                    
                case GoalType.IncludeMarketer:
                    return tracking.hadMarketer;
                    
                case GoalType.NotExceedEmployees:
                    return tracking.maxTeamSize <= goal.GetAdjustedIntegerTarget(contract.difficulty);
                    
                case GoalType.UseSingleSpecialist:
                    return tracking.maxTeamSize == 1;
                    
                case GoalType.UseLowSkillOnly:
                    return tracking.highestSkill <= goal.GetAdjustedThreshold(contract.difficulty);
                    
                case GoalType.UseHighSkillEmployee:
                    return tracking.highestSkill >= goal.GetAdjustedThreshold(contract.difficulty);
                    
                case GoalType.NoTeamChanges:
                    return tracking.teamChangeCount == 0;
                    
                case GoalType.NoReassignment:
                    return tracking.reassignmentCount == 0;
                
                // Morale & Wellbeing Goals
                case GoalType.MaintainAvgMorale:
                    return tracking.avgMorale >= goal.GetAdjustedThreshold(contract.difficulty);
                    
                case GoalType.FinishLowBurnout:
                    return tracking.finalBurnout <= goal.GetAdjustedThreshold(contract.difficulty);
                    
                case GoalType.IncreaseMorale:
                    return tracking.moraleChange > 0;
                    
                case GoalType.NoMoraleBelowThreshold:
                    return tracking.lowestMorale >= goal.GetAdjustedThreshold(contract.difficulty);
                    
                case GoalType.RecoverBurnout:
                    return tracking.burnoutRecovered >= goal.percentageTarget;
                    
                case GoalType.ZeroBurnoutSpikes:
                    return tracking.burnoutSpikeCount == 0;
                    
                case GoalType.ImproveEmployeeSkill:
                    return tracking.skillImproved;
                    
                case GoalType.FinishHigherMorale:
                    return tracking.moraleChange > 0;
                    
                case GoalType.NoNegativeMoraleEvents:
                    return tracking.negativeMoraleEvents == 0;
                    
                case GoalType.LimitBurnoutGrowth:
                    return tracking.totalBurnoutGrowth <= goal.percentageTarget;
                
                // Financial / Efficiency Goals
                case GoalType.UnderLabourCost:
                    return tracking.labourCost <= tracking.projectedLabourCost;
                    
                case GoalType.HighProductivity:
                    return tracking.avgProductivity >= (100f + goal.percentageTarget);
                    
                case GoalType.AvoidOverstaffing:
                    return !tracking.wasOverstaffed;
                    
                case GoalType.HighEfficiency:
                    return tracking.avgDailyProgress >= tracking.baselineProgress * (1f + goal.percentageTarget / 100f);
                    
                case GoalType.LowProductivityVariance:
                    return tracking.productivityVariance <= goal.percentageTarget;
                    
                case GoalType.NoProductivityDrop:
                    return tracking.minProductivity >= (goal.percentageTarget);
                    
                case GoalType.LowWastedTime:
                    return tracking.wastedDays <= goal.GetAdjustedIntegerTarget(contract.difficulty);
                    
                case GoalType.NoBurnoutMultiplier:
                    return !tracking.hadBurnoutMultiplier;
                    
                case GoalType.ExceedSkillCoverage:
                    return tracking.skillCoveragePercent >= (100f + goal.percentageTarget);
                    
                case GoalType.SteadyProductivity:
                    return tracking.productivityVariance <= goal.percentageTarget;
                
                // Advanced / Reputation Goals
                case GoalType.ClientSatisfaction:
                    return tracking.finalQuality >= goal.GetAdjustedTargetValue(contract.difficulty);
                    
                case GoalType.NoPenalties:
                    return tracking.penaltyCount == 0;
                    
                case GoalType.ImpressClient:
                    return tracking.finalQuality >= 95f;
                    
                case GoalType.NoOptionalGoalFailures:
                    return tracking.optionalGoalFailures == 0;
                    
                case GoalType.ContractStreak:
                    return tracking.currentStreak >= goal.GetAdjustedIntegerTarget(contract.difficulty);
                    
                case GoalType.NoReassignmentAdvanced:
                    return tracking.reassignmentCount == 0;
                    
                case GoalType.UnderQualifiedTeam:
                    return tracking.avgSkill < contract.requiredDevSkill * 0.8f && contract.progress >= 100f;
                    
                case GoalType.Overdeliver:
                    return tracking.rawQuality > 100f;
                    
                case GoalType.RecordCompletionTime:
                    return tracking.completionTime <= tracking.recordTime;
                    
                case GoalType.MaintainCompanyMorale:
                    return tracking.companyAvgMorale >= goal.GetAdjustedThreshold(contract.difficulty);
                
                default:
                    Debug.LogWarning($"Unknown goal type: {goal.type}");
                    return false;
            }
        }
        
        float CalculateSkillDeficit(ContractData contract, ContractTracking tracking)
        {
            float devDeficit = Mathf.Max(0, contract.requiredDevSkill - tracking.avgDevSkill);
            float designDeficit = Mathf.Max(0, contract.requiredDesignSkill - tracking.avgDesignSkill);
            float marketingDeficit = Mathf.Max(0, contract.requiredMarketingSkill - tracking.avgMarketingSkill);
            
            float totalRequired = contract.requiredDevSkill + contract.requiredDesignSkill + contract.requiredMarketingSkill;
            float totalDeficit = devDeficit + designDeficit + marketingDeficit;
            
            return (totalDeficit / totalRequired) * 100f;
        }
    }
    
    public class ContractTracking
    {
        // Skill metrics
        public float finalQuality;
        public float avgDevSkill;
        public float avgDesignSkill;
        public float avgMarketingSkill;
        public float avgEffectiveSkill;
        public float lowestEffectiveSkill;
        public float lowestSkill;
        public float highestSkill;
        public float avgSkill;
        public float rawQuality;
        
        // Time metrics
        public int completionTime;
        public int longestStallDays;
        public int zeroDaysCount;
        public bool reachedMilestone;
        public int wastedDays;
        public int recordTime;
        
        // Team metrics
        public int maxTeamSize;
        public bool hadDesigner;
        public bool hadMarketer;
        public int teamChangeCount;
        public int reassignmentCount;
        
        // Morale/Wellbeing metrics
        public float avgMorale;
        public float lowestMorale;
        public float finalBurnout;
        public float maxBurnout;
        public float moraleChange;
        public float burnoutRecovered;
        public int burnoutSpikeCount;
        public bool skillImproved;
        public int negativeMoraleEvents;
        public float totalBurnoutGrowth;
        
        // Efficiency metrics
        public float labourCost;
        public float projectedLabourCost;
        public float avgProductivity;
        public float minProductivity;
        public bool wasOverstaffed;
        public float avgDailyProgress;
        public float baselineProgress;
        public float productivityVariance;
        public bool hadBurnoutMultiplier;
        public float skillCoveragePercent;
        
        // Reputation metrics
        public int penaltyCount;
        public int optionalGoalFailures;
        public int currentStreak;
        public float companyAvgMorale;
    }
}
