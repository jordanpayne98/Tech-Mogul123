using UnityEngine;
using System.Collections.Generic;
using TechMogul.Core;

namespace TechMogul.Data
{
    [CreateAssetMenu(fileName = "New Contract Template", menuName = "TechMogul/Contract Template")]
    public class ContractTemplateSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'contracttemplate.website_basic')")]
        public string id;
        public string templateName;
        [TextArea(2, 4)]
        public string description;
        
        public string Id => id;
        
        [Header("Difficulty Weights")]
        [Tooltip("Probability weights for each difficulty (higher = more likely)")]
        [Range(0, 100)]
        public float easyWeight = 40f;
        [Range(0, 100)]
        public float mediumWeight = 40f;
        [Range(0, 100)]
        public float hardWeight = 20f;
        
        [Header("Goals")]
        public List<GoalDefinition> possibleGoals = new List<GoalDefinition>();
        [Range(1, 5)]
        public int minGoals = 1;
        [Range(1, 5)]
        public int maxGoals = 3;
        
        [Header("Goal Category Weights")]
        [Tooltip("Weight for Skill-based goals (0 = never, higher = more likely)")]
        [Range(0, 100)]
        public float skillGoalWeight = 40f;
        [Tooltip("Weight for Time/Deadline goals")]
        [Range(0, 100)]
        public float timeGoalWeight = 30f;
        [Tooltip("Weight for Team Composition goals")]
        [Range(0, 100)]
        public float teamGoalWeight = 10f;
        [Tooltip("Weight for Morale/Wellbeing goals")]
        [Range(0, 100)]
        public float wellbeingGoalWeight = 20f;
        [Tooltip("Weight for Efficiency/Financial goals")]
        [Range(0, 100)]
        public float efficiencyGoalWeight = 30f;
        [Tooltip("Weight for Reputation/Advanced goals")]
        [Range(0, 100)]
        public float reputationGoalWeight = 10f;
        
        [Header("Timing")]
        [Tooltip("Base deadline in days")]
        [Range(5, 60)]
        public int baseDeadlineDays = 20;
        
        [Tooltip("Randomization range for deadline based on difficulty")]
        [Range(0, 20)]
        public int deadlineVariance = 5;
        
        [Tooltip("Difficulty modifiers - Easy gets more time, Hard gets less")]
        public bool applyDifficultyDeadlineModifier = true;
        
        [Header("Financial")]
        public float basePayoutMin = 5000f;
        public float basePayoutMax = 15000f;
        
        [Header("Skill Requirements (Randomized Per Contract)")]
        [Tooltip("Minimum dev skill requirement (must be > 0)")]
        [Range(1, 100)]
        public float devSkillMin = 30f;
        [Tooltip("Maximum dev skill requirement")]
        [Range(1, 100)]
        public float devSkillMax = 70f;
        
        [Tooltip("Minimum design skill requirement (must be > 0)")]
        [Range(1, 100)]
        public float designSkillMin = 20f;
        [Tooltip("Maximum design skill requirement")]
        [Range(1, 100)]
        public float designSkillMax = 60f;
        
        [Tooltip("Minimum marketing skill requirement (must be > 0)")]
        [Range(1, 100)]
        public float marketingSkillMin = 10f;
        [Tooltip("Maximum marketing skill requirement")]
        [Range(1, 100)]
        public float marketingSkillMax = 50f;
        
        [Header("Employee Effects")]
        [Range(0, 50)]
        public float baseBurnoutImpact = 15f;
        [Range(0, 20)]
        public float baseXPReward = 8f;
        
        public float ApplyDifficultyModifier(float baseValue, ContractDifficulty difficulty)
        {
            float modifier;
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    modifier = 0.5f;
                    break;
                case ContractDifficulty.Medium:
                    modifier = 1.0f;
                    break;
                case ContractDifficulty.Hard:
                    modifier = 1.4f;
                    break;
                default:
                    modifier = 1.0f;
                    break;
            }
            
            return baseValue * modifier;
        }
        
        public int ApplyDifficultyDeadlineModifier(int deadline, ContractDifficulty difficulty)
        {
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    return Mathf.RoundToInt(deadline * 1.3f);
                case ContractDifficulty.Medium:
                    return deadline;
                case ContractDifficulty.Hard:
                    return Mathf.RoundToInt(deadline * 0.7f);
                default:
                    return deadline;
            }
        }
        
        void OnValidate()
        {
            // Enforce minimum values > 0 for all skill types
            devSkillMin = Mathf.Max(1f, devSkillMin);
            designSkillMin = Mathf.Max(1f, designSkillMin);
            marketingSkillMin = Mathf.Max(1f, marketingSkillMin);
            
            devSkillMax = Mathf.Max(devSkillMin, devSkillMax);
            designSkillMax = Mathf.Max(designSkillMin, designSkillMax);
            marketingSkillMax = Mathf.Max(marketingSkillMin, marketingSkillMax);
            
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"Contract template '{name}' is missing a stable ID. Save/load will fail.");
            }
            
            if (basePayoutMin > basePayoutMax)
            {
                Debug.LogWarning($"Min payout ({basePayoutMin}) is greater than max ({basePayoutMax}) in {name}");
            }
            
            if (minGoals > maxGoals)
            {
                Debug.LogWarning($"Min goals ({minGoals}) is greater than max goals ({maxGoals}) in {name}");
            }
            
            if (possibleGoals.Count < maxGoals)
            {
                Debug.LogWarning($"Not enough possible goals ({possibleGoals.Count}) for max goals ({maxGoals}) in {name}");
            }
        }
    }
    
    [System.Serializable]
    public class GoalDefinition
    {
        public string description;
        public GoalType type;
        public GoalCategory category;
        
        [Header("Difficulty Scaling")]
        [Tooltip("Base target skill level needed to complete this goal (0-100)")]
        [Range(0, 100)]
        public float targetValue = 50f;
        
        [Tooltip("Threshold value for comparison goals (e.g., minimum morale, maximum burnout)")]
        [Range(0, 100)]
        public float thresholdValue = 60f;
        
        [Tooltip("Percentage-based target (e.g., 20% faster, 10% overstaffing)")]
        [Range(0, 100)]
        public float percentageTarget = 10f;
        
        [Tooltip("Integer target (e.g., 3 days early, 2 employees max)")]
        public int integerTarget = 3;
        
        [Header("Penalties")]
        [Tooltip("Penalties applied when this goal fails")]
        public List<TechMogul.Contracts.PenaltyDefinition> penalties = new List<TechMogul.Contracts.PenaltyDefinition>();
        
        [Header("Bonuses")]
        [Tooltip("Bonuses applied when this goal succeeds")]
        public List<TechMogul.Contracts.BonusDefinition> bonuses = new List<TechMogul.Contracts.BonusDefinition>();
        
        [Tooltip("Legacy: Simple bonus payout percentage")]
        [Range(0f, 30f)]
        public float bonusPercent = 10f;
        
        [Tooltip("Legacy: Simple reputation bonus")]
        [Range(0f, 20f)]
        public float reputationBonus = 5f;
        
        [Tooltip("Legacy: Simple XP multiplier bonus")]
        [Range(1f, 2f)]
        public float xpMultiplier = 1.2f;
        
        [Header("Goal Properties")]
        [Tooltip("Is this goal mandatory or optional?")]
        public bool isOptional = false;
        
        [Tooltip("Is this goal visible to the player from the start?")]
        public bool isHidden = false;
        
        public float GetAdjustedTargetValue(ContractDifficulty difficulty)
        {
            float adjustedTarget = targetValue;
            
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    adjustedTarget *= 0.7f;
                    break;
                case ContractDifficulty.Medium:
                    break;
                case ContractDifficulty.Hard:
                    adjustedTarget *= 1.4f;
                    break;
            }
            
            return Mathf.Clamp(adjustedTarget, 0f, 100f);
        }
        
        public float GetAdjustedThreshold(ContractDifficulty difficulty)
        {
            float adjustedThreshold = thresholdValue;
            
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    adjustedThreshold *= 0.8f;
                    break;
                case ContractDifficulty.Medium:
                    break;
                case ContractDifficulty.Hard:
                    adjustedThreshold *= 1.3f;
                    break;
            }
            
            return Mathf.Clamp(adjustedThreshold, 0f, 100f);
        }
        
        public int GetAdjustedIntegerTarget(ContractDifficulty difficulty)
        {
            int adjustedTarget = integerTarget;
            
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    adjustedTarget = Mathf.RoundToInt(integerTarget * 0.7f);
                    break;
                case ContractDifficulty.Medium:
                    break;
                case ContractDifficulty.Hard:
                    adjustedTarget = Mathf.RoundToInt(integerTarget * 1.4f);
                    break;
            }
            
            return Mathf.Max(1, adjustedTarget);
        }
    }
    
    public enum ContractDifficulty
    {
        Easy,
        Medium,
        Hard
    }
    
    public enum GoalCategory
    {
        Skill,
        Time,
        Team,
        Wellbeing,
        Efficiency,
        Reputation
    }
    
    public enum GoalType
    {
        // Skill-Based Goals
        ReachDevQuality,
        ReachDesignQuality,
        ReachMarketingQuality,
        NoSkillBelowThreshold,
        BalancedSkillDistribution,
        OverperformDevRequirement,
        DeliverHighQuality,
        LowSkillDeficit,
        MaintainAvgEffectiveSkill,
        NoLowMoraleDuringProject,
        
        // Deadline & Time Goals
        FinishDaysEarly,
        FinishPercentFaster,
        NotExceedTimePercent,
        FinishExactDeadline,
        FinishFirstHalf,
        ConsistentProgress,
        NoDaysZeroProgress,
        ReachProgressMilestone,
        NoOvertimeBurnout,
        NoBurnoutSpikes,
        
        // Team Composition Goals
        UseMaxEmployees,
        UseMinEmployees,
        IncludeDesigner,
        IncludeMarketer,
        NotExceedEmployees,
        UseSingleSpecialist,
        UseLowSkillOnly,
        UseHighSkillEmployee,
        NoTeamChanges,
        NoReassignment,
        
        // Morale & Wellbeing Goals
        MaintainAvgMorale,
        FinishLowBurnout,
        IncreaseMorale,
        NoMoraleBelowThreshold,
        RecoverBurnout,
        ZeroBurnoutSpikes,
        ImproveEmployeeSkill,
        FinishHigherMorale,
        NoNegativeMoraleEvents,
        LimitBurnoutGrowth,
        
        // Financial / Efficiency Goals
        UnderLabourCost,
        HighProductivity,
        AvoidOverstaffing,
        HighEfficiency,
        LowProductivityVariance,
        NoProductivityDrop,
        LowWastedTime,
        NoBurnoutMultiplier,
        ExceedSkillCoverage,
        SteadyProductivity,
        
        // Advanced / Reputation Goals
        ClientSatisfaction,
        NoPenalties,
        ImpressClient,
        NoOptionalGoalFailures,
        ContractStreak,
        NoReassignmentAdvanced,
        UnderQualifiedTeam,
        Overdeliver,
        RecordCompletionTime,
        MaintainCompanyMorale
    }
}

