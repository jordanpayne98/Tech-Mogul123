using UnityEngine;

namespace TechMogul.Systems
{
    [CreateAssetMenu(fileName = "EarlyTechRiskRewardConfig", menuName = "TechMogul/Market/Early Tech Risk-Reward Config")]
    public class EarlyTechRiskRewardConfigSO : ScriptableObject
    {
        [Header("Research Phase (A < 0.15)")]
        [Tooltip("Innovation bonus during Research phase")]
        [Range(0f, 0.10f)]
        public float researchInnovationBonus = 0.10f;
        
        [Tooltip("Bug risk multiplier during Research phase")]
        [Range(1.0f, 1.20f)]
        public float researchBugRiskMultiplier = 1.20f;
        
        [Tooltip("Dev time multiplier during Research phase")]
        [Range(1.0f, 1.15f)]
        public float researchDevTimeMultiplier = 1.15f;
        
        [Header("Early Adoption Phase (0.15 ≤ A < 0.40)")]
        [Tooltip("Innovation bonus during Early Adoption phase")]
        [Range(0f, 0.07f)]
        public float earlyInnovationBonus = 0.07f;
        
        [Tooltip("Bug risk multiplier during Early Adoption phase")]
        [Range(1.0f, 1.10f)]
        public float earlyBugRiskMultiplier = 1.10f;
        
        [Tooltip("Dev time multiplier during Early Adoption phase")]
        [Range(1.0f, 1.07f)]
        public float earlyDevTimeMultiplier = 1.07f;
        
        [Header("Growth Phase (0.40 ≤ A < 0.75)")]
        [Tooltip("Innovation bonus during Growth phase")]
        [Range(0f, 0.03f)]
        public float growthInnovationBonus = 0.03f;
        
        [Tooltip("Bug risk multiplier during Growth phase")]
        [Range(1.0f, 1.05f)]
        public float growthBugRiskMultiplier = 1.05f;
        
        [Tooltip("Dev time multiplier during Growth phase")]
        [Range(1.0f, 1.03f)]
        public float growthDevTimeMultiplier = 1.03f;
        
        [Header("Hard Caps")]
        [Tooltip("Absolute maximum bug risk multiplier")]
        [Range(1.0f, 1.30f)]
        public float maxBugRiskMultiplier = 1.30f;
        
        [Tooltip("Absolute maximum dev time multiplier")]
        [Range(1.0f, 1.25f)]
        public float maxDevTimeMultiplier = 1.25f;
        
        public float GetInnovationBonus(TechAdoptionPhase phase)
        {
            return phase switch
            {
                TechAdoptionPhase.Research => researchInnovationBonus,
                TechAdoptionPhase.EarlyAdoption => earlyInnovationBonus,
                TechAdoptionPhase.Growth => growthInnovationBonus,
                _ => 0f
            };
        }
        
        public float GetBugRiskMultiplier(TechAdoptionPhase phase)
        {
            float multiplier = phase switch
            {
                TechAdoptionPhase.Research => researchBugRiskMultiplier,
                TechAdoptionPhase.EarlyAdoption => earlyBugRiskMultiplier,
                TechAdoptionPhase.Growth => growthBugRiskMultiplier,
                _ => 1.0f
            };
            
            return Mathf.Min(multiplier, maxBugRiskMultiplier);
        }
        
        public float GetDevTimeMultiplier(TechAdoptionPhase phase)
        {
            float multiplier = phase switch
            {
                TechAdoptionPhase.Research => researchDevTimeMultiplier,
                TechAdoptionPhase.EarlyAdoption => earlyDevTimeMultiplier,
                TechAdoptionPhase.Growth => growthDevTimeMultiplier,
                _ => 1.0f
            };
            
            return Mathf.Min(multiplier, maxDevTimeMultiplier);
        }
    }
}
