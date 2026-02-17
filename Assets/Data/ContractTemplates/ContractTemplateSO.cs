using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TechMogul.Data
{
    [CreateAssetMenu(fileName = "New Contract Template", menuName = "TechMogul/Contract Template")]
    public class ContractTemplateSO : ScriptableObject
    {
        [Header("Identity")]
        public string templateName;
        [TextArea(2, 4)]
        public string description;
        
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
        [Tooltip("Minimum dev skill requirement for this contract type")]
        [Range(0, 100)]
        public float devSkillMin = 30f;
        [Tooltip("Maximum dev skill requirement for this contract type")]
        [Range(0, 100)]
        public float devSkillMax = 70f;
        
        [Tooltip("Minimum design skill requirement for this contract type")]
        [Range(0, 100)]
        public float designSkillMin = 20f;
        [Tooltip("Maximum design skill requirement for this contract type")]
        [Range(0, 100)]
        public float designSkillMax = 60f;
        
        [Tooltip("Minimum marketing skill requirement for this contract type")]
        [Range(0, 100)]
        public float marketingSkillMin = 10f;
        [Tooltip("Maximum marketing skill requirement for this contract type")]
        [Range(0, 100)]
        public float marketingSkillMax = 50f;
        
        [Header("Employee Effects")]
        [Range(0, 50)]
        public float baseBurnoutImpact = 15f;
        [Range(0, 20)]
        public float baseXPReward = 8f;
        
        public ContractDifficulty GetRandomDifficulty()
        {
            float totalWeight = easyWeight + mediumWeight + hardWeight;
            if (totalWeight <= 0) return ContractDifficulty.Medium;
            
            float randomValue = Random.Range(0f, totalWeight);
            
            if (randomValue < easyWeight)
                return ContractDifficulty.Easy;
            else if (randomValue < easyWeight + mediumWeight)
                return ContractDifficulty.Medium;
            else
                return ContractDifficulty.Hard;
        }
        
        public float GetRandomDevSkill(ContractDifficulty difficulty)
        {
            float baseValue = Random.Range(devSkillMin, devSkillMax);
            return ApplyDifficultyModifier(baseValue, difficulty);
        }
        
        public float GetRandomDesignSkill(ContractDifficulty difficulty)
        {
            float baseValue = Random.Range(designSkillMin, designSkillMax);
            return ApplyDifficultyModifier(baseValue, difficulty);
        }
        
        public float GetRandomMarketingSkill(ContractDifficulty difficulty)
        {
            float baseValue = Random.Range(marketingSkillMin, marketingSkillMax);
            return ApplyDifficultyModifier(baseValue, difficulty);
        }
        
        float ApplyDifficultyModifier(float baseValue, ContractDifficulty difficulty)
        {
            float modifier;
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    modifier = 0.5f; // 50% easier
                    break;
                case ContractDifficulty.Medium:
                    modifier = 1.0f;
                    break;
                case ContractDifficulty.Hard:
                    modifier = 1.4f; // 40% harder
                    break;
                default:
                    modifier = 1.0f;
                    break;
            }
            
            return baseValue * modifier;
        }
        
        void OnValidate()
        {
            // Clamp max values to be at least equal to min values
            devSkillMax = Mathf.Max(devSkillMin, devSkillMax);
            designSkillMax = Mathf.Max(designSkillMin, designSkillMax);
            marketingSkillMax = Mathf.Max(marketingSkillMin, marketingSkillMax);
            
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
        
        public int GetRandomizedDeadline(ContractDifficulty difficulty)
        {
            int deadline = baseDeadlineDays;
            
            // Add random variance
            int variance = UnityEngine.Random.Range(-deadlineVariance, deadlineVariance + 1);
            deadline += variance;
            
            // Apply difficulty modifier
            if (applyDifficultyDeadlineModifier)
            {
                switch (difficulty)
                {
                    case ContractDifficulty.Easy:
                        deadline = Mathf.RoundToInt(deadline * 1.3f); // +30% time
                        break;
                    case ContractDifficulty.Medium:
                        // No modifier
                        break;
                    case ContractDifficulty.Hard:
                        deadline = Mathf.RoundToInt(deadline * 0.7f); // -30% time
                        break;
                }
            }
            
            // Ensure minimum deadline
            return Mathf.Max(deadline, 5);
        }
        
        public List<GoalDefinition> GetRandomGoals(ContractDifficulty difficulty)
        {
            if (possibleGoals.Count == 0)
                return new List<GoalDefinition>();
            
            // Difficulty STRONGLY affects number of goals
            int goalCount;
            
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    // Easy: Always minimum goals
                    goalCount = minGoals;
                    break;
                    
                case ContractDifficulty.Medium:
                    // Medium: Middle of the range
                    int midPoint = (minGoals + maxGoals) / 2;
                    goalCount = UnityEngine.Random.Range(midPoint, midPoint + 1); // Mostly mid, slight variance
                    break;
                    
                case ContractDifficulty.Hard:
                    // Hard: Maximum goals
                    goalCount = maxGoals;
                    break;
                    
                default:
                    goalCount = minGoals;
                    break;
            }
            
            goalCount = Mathf.Min(goalCount, possibleGoals.Count);
            
            // Shuffle and take random goals
            var shuffled = new List<GoalDefinition>(possibleGoals);
            for (int i = 0; i < shuffled.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, shuffled.Count);
                var temp = shuffled[i];
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }
            
            return shuffled.Take(goalCount).ToList();
        }
    }
    
    [System.Serializable]
    public class GoalDefinition
    {
        public string description;
        public GoalType type;
        [Tooltip("Base target skill level needed to complete this goal (0-100)")]
        [Range(0, 100)]
        public float targetValue = 50f;
        
        [Header("Financial Impact")]
        [Tooltip("Percentage of contract payout lost if this goal fails")]
        [Range(0f, 50f)]
        public float penaltyPercentMin = 5f;
        [Range(0f, 50f)]
        public float penaltyPercentMax = 15f;
        
        public float GetRandomPenalty()
        {
            return Random.Range(penaltyPercentMin, penaltyPercentMax);
        }
        
        public float GetAdjustedTargetValue(ContractDifficulty difficulty)
        {
            float adjustedTarget = targetValue;
            
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    adjustedTarget *= 0.7f; // 30% easier to achieve
                    break;
                case ContractDifficulty.Medium:
                    // No modifier
                    break;
                case ContractDifficulty.Hard:
                    adjustedTarget *= 1.4f; // 40% harder to achieve
                    break;
            }
            
            return Mathf.Clamp(adjustedTarget, 0f, 100f);
        }
    }
    
    public enum ContractDifficulty
    {
        Easy,
        Medium,
        Hard
    }
    
    public enum GoalType
    {
        QualityTarget,
        FeatureCount,
        TimeLimit
    }
}
