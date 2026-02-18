using System;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    [Serializable]
    public class ContractWorldEffect
    {
        public string effectId;
        public string issuingRivalId;
        public string targetCategoryId;
        public WorldEffectComponent component;
        public float magnitude;
        public int durationQuarters;
        public int quartersRemaining;
        public GameDate expiryDate;
        
        public bool IsActive => quartersRemaining > 0;
        public bool IsExpired => quartersRemaining <= 0;
    }
    
    public enum WorldEffectComponent
    {
        Quality,        // Step 1: Quality component (0.30 weight)
        Marketing,      // Step 1: Marketing component (0.25 weight)
        Reputation,     // Step 1: Reputation component (0.20 weight)
        Price,          // Step 1: Price component (0.15 weight)
        Standard,       // Step 1: Standard component (0.05 weight)
        Ecosystem       // Step 1: Ecosystem component (0.05 weight)
    }
}
