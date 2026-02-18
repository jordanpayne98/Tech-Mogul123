using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Products
{
    [CreateAssetMenu(fileName = "New QA Tier", menuName = "TechMogul/QA Tier")]
    public class QATierSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'qa.tier1_basic')")]
        public string id;
        public string tierName;
        [TextArea(2, 4)] public string description;

        [Header("Requirements")]
        [Range(1, 4)] public int tier = 1;
        public QATierSO prerequisiteTier;

        [Header("Impact")]
        [Tooltip("Bonus to stability (reduces bugs)")]
        [Range(0f, 50f)] public float stabilityBonus = 10f;
        
        [Tooltip("Bonus to usability")]
        [Range(0f, 30f)] public float usabilityBonus = 5f;
        
        [Tooltip("Penalty to innovation (over-testing reduces novelty)")]
        [Range(0f, 30f)] public float innovationPenalty = 5f;

        [Header("Cost")]
        [Tooltip("Multiplier for development time (1.0 = no change, 1.5 = 50% longer)")]
        [Range(1.0f, 3.0f)] public float devTimeMultiplier = 1.2f;
        
        [Tooltip("Additional cost in development resources")]
        [Range(0f, 50f)] public float additionalDevCost = 10f;

        public string Id => id;

        void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"{name} missing stable ID");
            }

            if (prerequisiteTier != null && prerequisiteTier.tier >= tier)
            {
                Debug.LogError($"{name} prerequisite tier must be lower than current tier");
            }
        }
    }
}
