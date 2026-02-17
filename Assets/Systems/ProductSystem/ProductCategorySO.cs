using UnityEngine;

namespace TechMogul.Products
{
    [CreateAssetMenu(fileName = "ProductCategory", menuName = "TechMogul/Product Category")]
    public class ProductCategorySO : ScriptableObject
    {
        [Header("Basic Info")]
        public string categoryName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Development")]
        public float baseDevelopmentDays = 60f;
        [Range(0f, 1f)]
        public float qualityImportance = 0.8f;
        
        [Header("Revenue")]
        public float baseRevenueMin = 5000f;
        public float baseRevenueMax = 20000f;
        public float marketSizeMultiplier = 1.0f;
        
        [Header("Skill Weights (should sum to 1.0)")]
        [Range(0f, 1f)]
        public float devSkillWeight = 0.5f;
        [Range(0f, 1f)]
        public float designSkillWeight = 0.3f;
        [Range(0f, 1f)]
        public float marketingSkillWeight = 0.2f;
    }
}
