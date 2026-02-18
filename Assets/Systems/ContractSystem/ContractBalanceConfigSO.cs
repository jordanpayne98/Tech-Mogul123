using UnityEngine;

namespace TechMogul.Contracts
{
    [CreateAssetMenu(fileName = "ContractBalanceConfig", menuName = "TechMogul/Contract/Balance Config")]
    public class ContractBalanceConfigSO : ScriptableObject
    {
        [Header("Offer Generation (Locked)")]
        [Tooltip("Maximum available contract offers at once")]
        public int maxAvailableOffers = 5;
        
        [Tooltip("Number of contracts generated per cycle")]
        public int offersPerGeneration = 2;
        
        [Tooltip("Days between contract generation")]
        public int generationIntervalDays = 7;
        
        [Tooltip("Maximum active contracts player can have")]
        public int maxActiveContracts = 3;
        
        [Tooltip("Days before an offer expires")]
        public int offerExpiryDays = 14;
        
        [Header("World Effect Caps (Locked)")]
        [Tooltip("Maximum total effect per company per category")]
        [Range(0f, 0.20f)]
        public float maxTotalEffectPerCompanyCategory = 0.12f;
        
        [Tooltip("Maximum effect per component per company per category")]
        [Range(0f, 0.15f)]
        public float maxEffectPerComponentPerCompanyCategory = 0.08f;
        
        [Tooltip("Maximum effect duration in quarters")]
        [Range(1, 8)]
        public int maxEffectDurationQuarters = 4;
        
        [Header("Dominance Limiter (Locked)")]
        [Tooltip("Market share threshold for dominance")]
        [Range(0.4f, 0.7f)]
        public float dominanceShareThreshold = 0.55f;
        
        [Tooltip("Effect magnitude multiplier when dominant")]
        [Range(0.3f, 0.8f)]
        public float dominanceMagnitudeMultiplier = 0.60f;
        
        [Tooltip("Offer weight multiplier when dominant")]
        [Range(0.3f, 0.8f)]
        public float dominanceOfferWeightMultiplier = 0.50f;
        
        [Tooltip("Force dominant company effects to 1 quarter")]
        public bool forceDominanceDurationTo1Q = true;
        
        [Header("Cooldown (Locked)")]
        [Tooltip("Quarters before same issuer can offer same category again")]
        [Range(0, 4)]
        public int perIssuerPerCategoryOfferCooldownQuarters = 1;
        
        [Header("Tech Adoption Weighting (Locked)")]
        [Tooltip("Maximum tech adoption multiplier cap")]
        [Range(1.0f, 2.0f)]
        public float maxTechAdoptionMultiplier = 1.5f;
        
        [Header("Tech Phase Weight Bands")]
        [Range(0.3f, 1.0f)] public float researchWeightMin = 0.6f;
        [Range(0.5f, 1.2f)] public float researchWeightMax = 0.9f;
        
        [Range(0.5f, 1.2f)] public float earlyWeightMin = 0.9f;
        [Range(0.8f, 1.5f)] public float earlyWeightMax = 1.2f;
        
        [Range(0.8f, 1.3f)] public float growthWeightMin = 1.0f;
        [Range(1.0f, 1.5f)] public float growthWeightMax = 1.3f;
        
        [Range(0.9f, 1.4f)] public float mainstreamWeightMin = 1.1f;
        [Range(1.2f, 1.6f)] public float mainstreamWeightMax = 1.4f;
        
        [Range(0.8f, 1.3f)] public float mandatoryWeightMin = 1.0f;
        [Range(1.0f, 1.5f)] public float mandatoryWeightMax = 1.2f;
        
        void OnValidate()
        {
            maxTechAdoptionMultiplier = Mathf.Clamp(maxTechAdoptionMultiplier, 1.0f, 2.0f);
        }
        
        public float GetTechAdoptionWeight(TechMogul.Systems.TechAdoptionPhase phase, float randomFactor)
        {
            float min = 0.6f;
            float max = 0.9f;
            
            switch (phase)
            {
                case TechMogul.Systems.TechAdoptionPhase.Research:
                    min = researchWeightMin;
                    max = researchWeightMax;
                    break;
                case TechMogul.Systems.TechAdoptionPhase.EarlyAdoption:
                    min = earlyWeightMin;
                    max = earlyWeightMax;
                    break;
                case TechMogul.Systems.TechAdoptionPhase.Growth:
                    min = growthWeightMin;
                    max = growthWeightMax;
                    break;
                case TechMogul.Systems.TechAdoptionPhase.Mainstream:
                    min = mainstreamWeightMin;
                    max = mainstreamWeightMax;
                    break;
                case TechMogul.Systems.TechAdoptionPhase.Mandatory:
                    min = mandatoryWeightMin;
                    max = mandatoryWeightMax;
                    break;
            }
            
            float weight = Mathf.Lerp(min, max, randomFactor);
            return Mathf.Min(weight, maxTechAdoptionMultiplier);
        }
    }
}
