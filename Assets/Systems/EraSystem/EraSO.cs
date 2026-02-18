using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Systems
{
    [CreateAssetMenu(fileName = "New Era", menuName = "TechMogul/Era")]
    public class EraSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'era.dotcom_boom')")]
        public string id;
        
        [Header("Era Definition")]
        public string eraName;
        
        [Tooltip("First year of this era (inclusive)")]
        public int startYear;
        
        [Tooltip("Last year of this era (inclusive)")]
        public int endYear;
        
        [Header("Market Impact")]
        [Tooltip("Multiplier applied to base market size during this era")]
        [Range(0.1f, 5f)]
        public float baseMarketSizeMultiplier = 1f;
        
        [Header("Description")]
        [TextArea(3, 6)]
        public string description;
        
        public string Id => id;
        
        void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"{name} missing stable ID");
            }
            
            if (startYear > endYear)
            {
                Debug.LogWarning($"{name} start year ({startYear}) is greater than end year ({endYear})");
            }
            
            if (baseMarketSizeMultiplier <= 0)
            {
                Debug.LogWarning($"{name} baseMarketSizeMultiplier must be greater than 0");
                baseMarketSizeMultiplier = 0.1f;
            }
        }
        
        public bool IsYearInEra(int year)
        {
            return year >= startYear && year <= endYear;
        }
    }
}
