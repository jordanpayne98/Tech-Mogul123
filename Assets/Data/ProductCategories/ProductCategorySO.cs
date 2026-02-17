using UnityEngine;

namespace TechMogul.Data
{
    [CreateAssetMenu(fileName = "New Product Category", menuName = "TechMogul/Product Category")]
    public class ProductCategorySO : ScriptableObject
    {
        [Header("Identity")]
        public string categoryName;
        [TextArea(2, 4)]
        public string description;
        
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
        
        [Header("Skill Weights (Should sum to 1.0)")]
        [Range(0, 1)]
        public float devSkillWeight = 0.5f;
        [Range(0, 1)]
        public float designSkillWeight = 0.3f;
        [Range(0, 1)]
        public float marketingSkillWeight = 0.2f;
        
        [Header("Visual")]
        public Sprite icon;
        public Color categoryColor = Color.white;
        
        void OnValidate()
        {
            float totalWeight = devSkillWeight + designSkillWeight + marketingSkillWeight;
            if (Mathf.Abs(totalWeight - 1.0f) > 0.01f)
            {
                Debug.LogWarning($"Skill weights in {name} sum to {totalWeight:F2}, should be 1.0");
            }
            
            if (baseRevenueMin > baseRevenueMax)
            {
                Debug.LogWarning($"Min revenue ({baseRevenueMin}) is greater than max ({baseRevenueMax}) in {name}");
            }
            
            if (baseDevelopmentDays < 1)
            {
                Debug.LogWarning($"Development days should be at least 1 in {name}");
            }
        }
    }
}
