using UnityEngine;

namespace TechMogul.Systems
{
    [CreateAssetMenu(fileName = "ExpectationPenaltyConfig", menuName = "TechMogul/Market/Expectation Penalty Config")]
    public class ExpectationPenaltyConfigSO : ScriptableObject
    {
        [Header("Phase Thresholds (Locked Values)")]
        [Tooltip("Adoption threshold for 'Expected' tech")]
        [Range(0.5f, 0.9f)]
        public float expectedThreshold = 0.75f;
        
        [Tooltip("Adoption threshold for 'Mandatory' tech")]
        [Range(0.8f, 1.0f)]
        public float mandatoryThreshold = 0.90f;
        
        [Header("Penalties (Demand-Only)")]
        [Tooltip("Demand penalty for missing expected tech")]
        [Range(0.05f, 0.20f)]
        public float expectedMissingPenalty = 0.10f;
        
        [Tooltip("Demand penalty for missing mandatory tech")]
        [Range(0.10f, 0.30f)]
        public float mandatoryMissingPenalty = 0.20f;
        
        [Tooltip("Maximum total penalty from all missing tech")]
        [Range(0.20f, 0.50f)]
        public float totalPenaltyCap = 0.30f;
        
        [Header("Stacking")]
        [Tooltip("Multiplier for 2nd missing tech")]
        [Range(0.3f, 0.8f)]
        public float stackingSecondMultiplier = 0.6f;
        
        [Tooltip("Multiplier for 3rd+ missing tech")]
        [Range(0.1f, 0.5f)]
        public float stackingThirdPlusMultiplier = 0.3f;
        
        void OnValidate()
        {
            if (expectedThreshold >= mandatoryThreshold)
            {
                expectedThreshold = mandatoryThreshold - 0.05f;
            }
        }
    }
}
