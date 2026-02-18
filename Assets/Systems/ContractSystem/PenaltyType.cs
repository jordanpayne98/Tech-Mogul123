namespace TechMogul.Contracts
{
    public enum PenaltyType
    {
        // Financial Penalties
        ReduceBasePayout,
        FlatFine,
        RemoveQualityBonus,
        ReduceEarlyBonus,
        RemoveEfficiencyBonus,
        
        // Reputation Penalties
        ReputationLoss,
        ReduceReputationGain,
        ClientTrustDecrease,
        LimitHighTierContracts,
        
        // Employee Penalties
        BurnoutSpike,
        MoraleDrop,
        ReduceXPGain,
        TemporaryProductivityPenalty,
        
        // Quality Penalties
        CapMaxQuality,
        QualityMultiplierReduction,
        ClientDissatisfaction,
        
        // Company-Wide Effects
        ProductivityDebuff,
        IncreasedBurnoutNext,
        HigherSalaryExpectations,
        NextContractHarder
    }
    
    [System.Serializable]
    public class PenaltyDefinition
    {
        public PenaltyType type;
        
        [UnityEngine.Tooltip("Percentage value (e.g., 10 = 10% reduction)")]
        [UnityEngine.Range(0f, 100f)]
        public float percentValue = 10f;
        
        [UnityEngine.Tooltip("Flat value (e.g., $500 fine, +10 burnout)")]
        public float flatValue = 500f;
        
        [UnityEngine.Tooltip("Duration in days for temporary effects")]
        public int durationDays = 3;
        
        [UnityEngine.Tooltip("Scale this penalty by contract difficulty?")]
        public bool scaleByDifficulty = true;
        
        [UnityEngine.Tooltip("Scale this penalty by player reputation?")]
        public bool scaleByReputation = false;
        
        public float GetScaledValue(TechMogul.Data.ContractDifficulty difficulty, float reputation, float maxReputation)
        {
            float value = percentValue;
            
            if (scaleByDifficulty)
            {
                value *= GetDifficultyMultiplier(difficulty);
            }
            
            if (scaleByReputation)
            {
                float repFactor = reputation / maxReputation;
                value *= (1f + repFactor * 0.5f); // Up to 50% more penalty at max rep
            }
            
            return value;
        }
        
        public float GetScaledFlatValue(TechMogul.Data.ContractDifficulty difficulty, float reputation, float maxReputation)
        {
            float value = flatValue;
            
            if (scaleByDifficulty)
            {
                value *= GetDifficultyMultiplier(difficulty);
            }
            
            if (scaleByReputation)
            {
                float repFactor = reputation / maxReputation;
                value *= (1f + repFactor * 0.5f);
            }
            
            return value;
        }
        
        float GetDifficultyMultiplier(TechMogul.Data.ContractDifficulty difficulty)
        {
            return difficulty switch
            {
                TechMogul.Data.ContractDifficulty.Easy => 0.8f,
                TechMogul.Data.ContractDifficulty.Medium => 1.0f,
                TechMogul.Data.ContractDifficulty.Hard => 1.3f,
                _ => 1.0f
            };
        }
    }
}
