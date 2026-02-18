using UnityEngine;

namespace TechMogul.Systems
{
    [CreateAssetMenu(fileName = "MarketDemandConfig", menuName = "TechMogul/Market/Demand Config")]
    public class MarketDemandConfigSO : ScriptableObject
    {
        [Header("Demand Bounds (Locked Values)")]
        [Tooltip("Minimum market demand multiplier")]
        [Range(0.1f, 0.5f)]
        public float demandFloor = 0.25f;
        
        [Tooltip("Maximum uplift from tech adoption")]
        [Range(0.2f, 0.5f)]
        public float adoptionDemandUpliftCap = 0.35f;
        
        [Tooltip("Exponent for adoption demand curve")]
        [Range(0.5f, 1.0f)]
        public float adoptionExponent = 0.8f;
        
        void OnValidate()
        {
            demandFloor = Mathf.Clamp(demandFloor, 0.1f, 0.5f);
            adoptionDemandUpliftCap = Mathf.Clamp(adoptionDemandUpliftCap, 0.2f, 0.5f);
            adoptionExponent = Mathf.Clamp(adoptionExponent, 0.5f, 1.0f);
        }
        
        public float CalculateAdoptionDemandFactor(float adoptionRate)
        {
            return (1f - adoptionDemandUpliftCap) + adoptionDemandUpliftCap * Mathf.Pow(adoptionRate, adoptionExponent);
        }
    }
}
