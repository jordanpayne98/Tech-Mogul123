using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Systems
{
    [CreateAssetMenu(fileName = "New MarketCategory", menuName = "TechMogul/Market/Market Category")]
    public class MarketCategorySO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'market.online_services')")]
        public string id;
        
        [Header("Market Definition")]
        public string categoryName;
        
        [Tooltip("Base demand before tech/era modifiers")]
        [Range(0.1f, 10f)]
        public float baseDemand = 1f;
        
        [Tooltip("Rate at which market saturates (higher = faster saturation)")]
        [Range(0.01f, 0.5f)]
        public float saturationRate = 0.1f;
        
        [Header("Technology Links")]
        [Tooltip("Technologies that drive demand for this category")]
        public TechnologySO[] linkedTechnologies;
        
        [Header("Description")]
        [TextArea(2, 4)]
        public string description;
        
        public string Id => id;
        
        void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"{name} missing stable ID");
            }
        }
    }
}
