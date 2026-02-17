using UnityEngine;

namespace TechMogul.Data
{
    [CreateAssetMenu(fileName = "New Difficulty Preset", menuName = "TechMogul/Difficulty Preset")]
    public class DifficultyPresetSO : ScriptableObject
    {
        [Header("Identity")]
        public string presetName;
        [TextArea(2, 3)]
        public string description;
        
        [Header("Starting Conditions")]
        public float startingCash = 50000f;
        public int startingEmployees = 2;
        
        [Header("Economic Multipliers")]
        [Range(0.5f, 2f)]
        public float salaryMultiplier = 1.0f;
        [Range(0.5f, 2f)]
        public float revenueMultiplier = 1.0f;
        [Range(0.5f, 2f)]
        public float contractPayoutMultiplier = 1.0f;
        
        [Header("Employee Settings")]
        [Range(0.5f, 2f)]
        public float burnoutRateMultiplier = 1.0f;
        [Range(0.5f, 2f)]
        public float moraleDecayMultiplier = 1.0f;
        [Range(0.5f, 2f)]
        public float skillGrowthMultiplier = 1.0f;
        
        [Header("Market Competition (Phase 2+)")]
        [Range(0f, 1f)]
        public float rivalAggressiveness = 0.5f;
        
        void OnValidate()
        {
            if (startingCash < 0)
            {
                Debug.LogWarning($"Starting cash should not be negative in {name}");
            }
            
            if (startingEmployees < 0)
            {
                Debug.LogWarning($"Starting employees should not be negative in {name}");
            }
        }
    }
}
