using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Data
{
    [CreateAssetMenu(fileName = "New Role", menuName = "TechMogul/Role")]
    public class RoleSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'role.dev_junior')")]
        public string id;
        public string roleName;
        [TextArea(2, 4)]
        public string description;
        
        public string Id => id;
        
        [Header("Skill Ranges (0-100) - Higher values are rarer")]
        [Range(0, 100)]
        public float devSkillMin = 20f;
        [Range(0, 100)]
        public float devSkillMax = 80f;
        
        [Range(0, 100)]
        public float designSkillMin = 10f;
        [Range(0, 100)]
        public float designSkillMax = 40f;
        
        [Range(0, 100)]
        public float marketingSkillMin = 5f;
        [Range(0, 100)]
        public float marketingSkillMax = 30f;
        
        [Header("Skill Distribution")]
        [Tooltip("Controls rarity curve. Higher = more common low skills. Range: 1.5-4.0")]
        [Range(1.5f, 4.0f)]
        public float skillRarityCurve = 2.5f;
        
        [Header("Financial")]
        public float baseSalaryMin = 30000f;
        public float baseSalaryMax = 60000f;
        
        [Header("Growth Rates (Multipliers)")]
        public float devGrowthRate = 1.0f;
        public float designGrowthRate = 1.0f;
        public float marketingGrowthRate = 1.0f;
        
        [Header("Visual")]
        public Sprite icon;
        public Color themeColor = Color.white;
        
        void OnValidate()
        {
            ValidateStableId();
            ClampAndCorrectSkillRanges();
            ClampAndCorrectSalaryRanges();
            ValidateGrowthRates();
        }
        
        void ValidateStableId()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"[{name}] Missing stable ID. Set unique ID (e.g., 'role.dev_junior') for save/load.");
                return;
            }
            
            if (id.Contains(" "))
            {
                Debug.LogWarning($"[{name}] ID contains spaces: '{id}'. Use underscores or hyphens instead.");
            }
            
            if (id != id.ToLower())
            {
                Debug.LogWarning($"[{name}] ID should be lowercase: '{id}' → '{id.ToLower()}'");
            }
            
            if (!id.StartsWith("role."))
            {
                Debug.LogWarning($"[{name}] ID should start with 'role.' prefix: '{id}' → 'role.{id}'");
            }
        }
        
        void ClampAndCorrectSkillRanges()
        {
            devSkillMin = Mathf.Clamp(devSkillMin, 0, 100);
            devSkillMax = Mathf.Clamp(devSkillMax, 0, 100);
            devSkillMax = Mathf.Max(devSkillMin, devSkillMax);
            
            designSkillMin = Mathf.Clamp(designSkillMin, 0, 100);
            designSkillMax = Mathf.Clamp(designSkillMax, 0, 100);
            designSkillMax = Mathf.Max(designSkillMin, designSkillMax);
            
            marketingSkillMin = Mathf.Clamp(marketingSkillMin, 0, 100);
            marketingSkillMax = Mathf.Clamp(marketingSkillMax, 0, 100);
            marketingSkillMax = Mathf.Max(marketingSkillMin, marketingSkillMax);
        }
        
        void ClampAndCorrectSalaryRanges()
        {
            baseSalaryMin = Mathf.Max(0, baseSalaryMin);
            baseSalaryMax = Mathf.Max(0, baseSalaryMax);
            baseSalaryMax = Mathf.Max(baseSalaryMin, baseSalaryMax);
        }
        
        void ValidateGrowthRates()
        {
            if (devGrowthRate < 0)
            {
                Debug.LogWarning($"[{name}] Negative dev growth rate ({devGrowthRate}), clamping to 0.");
                devGrowthRate = 0;
            }
            
            if (designGrowthRate < 0)
            {
                Debug.LogWarning($"[{name}] Negative design growth rate ({designGrowthRate}), clamping to 0.");
                designGrowthRate = 0;
            }
            
            if (marketingGrowthRate < 0)
            {
                Debug.LogWarning($"[{name}] Negative marketing growth rate ({marketingGrowthRate}), clamping to 0.");
                marketingGrowthRate = 0;
            }
            
            if (devGrowthRate > 5f)
            {
                Debug.LogWarning($"[{name}] Dev growth rate ({devGrowthRate}) exceeds max 5.0, clamping.");
                devGrowthRate = 5f;
            }
            
            if (designGrowthRate > 5f)
            {
                Debug.LogWarning($"[{name}] Design growth rate ({designGrowthRate}) exceeds max 5.0, clamping.");
                designGrowthRate = 5f;
            }
            
            if (marketingGrowthRate > 5f)
            {
                Debug.LogWarning($"[{name}] Marketing growth rate ({marketingGrowthRate}) exceeds max 5.0, clamping.");
                marketingGrowthRate = 5f;
            }
        }
    }
}
