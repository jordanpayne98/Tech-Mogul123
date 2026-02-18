namespace TechMogul.Contracts
{
    public enum BonusType
    {
        // Financial Bonuses
        IncreaseBasePayout,
        FlatBonus,
        EarlyCompletionMultiplier,
        QualityBonusMultiplier,
        EfficiencyBonus,
        
        // Reputation Bonuses
        ReputationGain,
        ReputationMultiplier,
        ClientTrustBoost,
        UnlockHighTierFaster,
        
        // Employee Bonuses
        ExtraXP,
        MoraleBoost,
        BurnoutRecovery,
        ProductivityBuffNext,
        SkillGrowthBoost,
        
        // Company Bonuses
        TemporaryProductivityBoost,
        ReducedBurnoutAccumulation,
        IncreasedContractFrequency,
        ReducedSalaryGrowth,
        
        // Special Bonuses
        PerfectContractBonus,
        StreakBonus,
        ClientSatisfactionBoost
    }
    
    [System.Serializable]
    public class BonusDefinition
    {
        public BonusType type;
        
        [UnityEngine.Tooltip("Percentage value (e.g., 10 = 10% increase)")]
        [UnityEngine.Range(0f, 100f)]
        public float percentValue = 10f;
        
        [UnityEngine.Tooltip("Flat value (e.g., $500 bonus, +10 morale)")]
        public float flatValue = 500f;
        
        [UnityEngine.Tooltip("Duration in days for temporary effects")]
        public int durationDays = 5;
        
        [UnityEngine.Tooltip("Scale this bonus by contract difficulty?")]
        public bool scaleByDifficulty = true;
        
        [UnityEngine.Tooltip("Scale this bonus by contract value?")]
        public bool scaleByContractValue = false;
        
        public float GetScaledValue(TechMogul.Data.ContractDifficulty difficulty, float contractValue)
        {
            float value = percentValue;
            
            if (scaleByDifficulty)
            {
                value *= GetDifficultyMultiplier(difficulty);
            }
            
            if (scaleByContractValue)
            {
                // Scale by contract size (larger contracts = better bonuses)
                float valueMultiplier = UnityEngine.Mathf.Clamp(contractValue / 10000f, 0.5f, 2.0f);
                value *= valueMultiplier;
            }
            
            return value;
        }
        
        public float GetScaledFlatValue(TechMogul.Data.ContractDifficulty difficulty, float contractValue)
        {
            float value = flatValue;
            
            if (scaleByDifficulty)
            {
                value *= GetDifficultyMultiplier(difficulty);
            }
            
            if (scaleByContractValue)
            {
                float valueMultiplier = UnityEngine.Mathf.Clamp(contractValue / 10000f, 0.5f, 2.0f);
                value *= valueMultiplier;
            }
            
            return value;
        }
        
        float GetDifficultyMultiplier(TechMogul.Data.ContractDifficulty difficulty)
        {
            return difficulty switch
            {
                TechMogul.Data.ContractDifficulty.Easy => 0.8f,
                TechMogul.Data.ContractDifficulty.Medium => 1.0f,
                TechMogul.Data.ContractDifficulty.Hard => 1.25f,
                _ => 1.0f
            };
        }
    }
}
