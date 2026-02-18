using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Systems
{
    [CreateAssetMenu(fileName = "New Technology", menuName = "TechMogul/Technology")]
    public class TechnologySO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'tech.web_dev')")]
        public string id;
        
        [Header("Technology Definition")]
        public string techName;
        
        [Tooltip("Category: Graphics, Networking, AI, Platform, etc.")]
        public string category;
        
        [Header("Timeline")]
        [Tooltip("Year this technology was introduced/researched")]
        public int researchYear;
        
        [Tooltip("Mainstream adoption midpoint year")]
        public int inflectionYear;
        
        [Tooltip("Speed of adoption curve (typical: 0.3-0.5)")]
        [Range(0.1f, 1.0f)]
        public float adoptionSpeedConstant = 0.4f;
        
        [Header("Effects")]
        [Tooltip("Innovation bonus for using this tech (0-0.10)")]
        [Range(0f, 0.10f)]
        public float innovationBonus = 0f;
        
        [Tooltip("Bug risk multiplier when using this tech (1.0-1.30)")]
        [Range(1.0f, 1.30f)]
        public float bugRiskMultiplier = 1.0f;
        
        [Tooltip("Market relevance/demand multiplier (0.5-2.0)")]
        [Range(0.5f, 2.0f)]
        public float marketRelevanceMultiplier = 1.0f;
        
        [Header("Prerequisites")]
        [Tooltip("Technologies that must exist before this one")]
        public TechnologySO[] prerequisites;
        
        [Tooltip("Technologies this one replaces (mutually exclusive)")]
        public TechnologySO[] replaces;
        
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
            
            if (researchYear > inflectionYear)
            {
                Debug.LogWarning($"{name} research year ({researchYear}) is greater than inflection year ({inflectionYear})");
            }
        }
    }
}
