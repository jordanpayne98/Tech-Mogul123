using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Data
{
    [CreateAssetMenu(fileName = "New Product Category", menuName = "TechMogul/Product Category")]
    public class ProductCategorySO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'productcat.games')")]
        public string id;
        public string categoryName;
        [TextArea(2, 4)]
        public string description;
        
        public string Id => id;
        
        [Header("Development")]
        [Range(10, 180)]
        public float baseDevelopmentDays = 60f;
        [Range(0, 1)]
        public float qualityImportance = 0.7f;
        
        [Header("Revenue")]
        public float baseRevenueMin = 5000f;
        public float baseRevenueMax = 20000f;
        [Range(0.5f, 3f)]
        public float marketSizeMultiplier = 1.0f;
        
        [Header("Skill Weights")]
        [Tooltip("Auto-normalize weights to sum to 1.0 when enabled")]
        public bool autoNormalizeSkillWeights = true;
        [Range(0, 1)]
        public float devSkillWeight = 0.5f;
        [Range(0, 1)]
        public float designSkillWeight = 0.3f;
        [Range(0, 1)]
        public float marketingSkillWeight = 0.2f;
        
        [Header("MILESTONE 1: Competition Properties")]
        public CategoryLifecycleStage currentStage = CategoryLifecycleStage.Growth;
        
        [Range(1000000f, 100000000f)]
        [Tooltip("Total market size for competition calculations")]
        public float baseMarketSize = 10000000f;
        
        [Range(0.01f, 0.25f)]
        [Tooltip("Monthly growth rate for this category")]
        public float baseGrowthRate = 0.05f;
        
        [Range(0.3f, 1.0f)]
        [Tooltip("How responsive market share is to MarketPower changes. Lower = more sticky.")]
        public float elasticity = 0.7f;
        
        [Range(100000f, 10000000f)]
        [Tooltip("Cash required to enter this category")]
        public float entryCost = 1000000f;
        
        [Range(0.01f, 0.15f)]
        [Tooltip("Baseline volatility for random market fluctuations")]
        public float baseVolatility = 0.02f;
        
        [Header("Timeline Gating (Future)")]
        [Tooltip("Minimum year this product category becomes available")]
        public int minYear = 1950;
        [Tooltip("Maximum year this product category is available")]
        public int maxYear = 9999;
        
        [Header("Market Weighting (Future)")]
        [Tooltip("Popularity multiplier for market weighting (1.0 = normal)")]
        [Range(0.1f, 5f)]
        public float popularity = 1f;
        
        [Header("Visual")]
        public Sprite icon;
        public Color categoryColor = Color.white;
        
        void OnValidate()
        {
            ValidateID();
            ClampDevelopmentDays();
            ClampRevenueRange();
            ClampMarketSizeMultiplier();
            ValidateSkillWeights();
            ValidateTimelineGating();
        }
        
        void ValidateID()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"ProductCategorySO '{name}' has empty ID. Save/load will break.", this);
                return;
            }
            
            if (id.Contains(" "))
            {
                Debug.LogWarning($"ProductCategorySO '{name}' ID contains spaces: '{id}'. Consider using lowercase with dots/underscores (e.g., 'productcat.{name.ToLower().Replace(" ", "_")}').", this);
            }
            
            bool hasUpperCase = false;
            for (int i = 0; i < id.Length; i++)
            {
                if (char.IsUpper(id[i]))
                {
                    hasUpperCase = true;
                    break;
                }
            }
            
            if (hasUpperCase)
            {
                Debug.LogWarning($"ProductCategorySO '{name}' ID contains uppercase: '{id}'. Recommended convention: lowercase with dots/underscores (e.g., 'productcat.{id.ToLower()}').", this);
            }
        }
        
        void ClampDevelopmentDays()
        {
            if (baseDevelopmentDays < 1f)
            {
                Debug.LogWarning($"ProductCategorySO '{name}': Development days was {baseDevelopmentDays}, clamping to minimum 1 day.", this);
                baseDevelopmentDays = 1f;
            }
        }
        
        void ClampRevenueRange()
        {
            if (baseRevenueMin > baseRevenueMax)
            {
                Debug.LogWarning($"ProductCategorySO '{name}': Min revenue ({baseRevenueMin:F0}) > max revenue ({baseRevenueMax:F0}). Clamping max to min.", this);
                baseRevenueMax = baseRevenueMin;
            }
        }
        
        void ClampMarketSizeMultiplier()
        {
            if (marketSizeMultiplier <= 0f)
            {
                Debug.LogWarning($"ProductCategorySO '{name}': Market size multiplier was {marketSizeMultiplier}, clamping to minimum 0.1.", this);
                marketSizeMultiplier = 0.1f;
            }
        }
        
        void ValidateSkillWeights()
        {
            float totalWeight = devSkillWeight + designSkillWeight + marketingSkillWeight;
            const float epsilon = 0.01f;
            
            if (totalWeight < epsilon)
            {
                if (autoNormalizeSkillWeights)
                {
                    Debug.LogWarning($"ProductCategorySO '{name}': Skill weights sum to {totalWeight:F3} (near zero). Setting defaults: Dev=0.5, Design=0.3, Marketing=0.2.", this);
                    devSkillWeight = 0.5f;
                    designSkillWeight = 0.3f;
                    marketingSkillWeight = 0.2f;
                }
                else
                {
                    Debug.LogWarning($"ProductCategorySO '{name}': Skill weights sum to {totalWeight:F3} (near zero). Enable auto-normalize or set valid weights.", this);
                }
                return;
            }
            
            if (Mathf.Abs(totalWeight - 1.0f) > epsilon)
            {
                if (autoNormalizeSkillWeights)
                {
                    float originalDev = devSkillWeight;
                    float originalDesign = designSkillWeight;
                    float originalMarketing = marketingSkillWeight;
                    
                    devSkillWeight /= totalWeight;
                    designSkillWeight /= totalWeight;
                    marketingSkillWeight /= totalWeight;
                    
                    Debug.LogWarning($"ProductCategorySO '{name}': Skill weights summed to {totalWeight:F3}, auto-normalized to 1.0. " +
                                   $"Dev: {originalDev:F3}→{devSkillWeight:F3}, Design: {originalDesign:F3}→{designSkillWeight:F3}, Marketing: {originalMarketing:F3}→{marketingSkillWeight:F3}.", this);
                }
                else
                {
                    Debug.LogWarning($"ProductCategorySO '{name}': Skill weights sum to {totalWeight:F3}, should be 1.0. Enable auto-normalize or adjust weights manually.", this);
                }
            }
        }
        
        void ValidateTimelineGating()
        {
            if (minYear > maxYear)
            {
                Debug.LogWarning($"ProductCategorySO '{name}': Min year ({minYear}) > max year ({maxYear}). Timeline gating may not work as expected.", this);
            }
        }
    }
    
    public enum CategoryLifecycleStage
    {
        Emerging,
        Growth,
        Maturity,
        Saturation,
        Decline
    }
}
