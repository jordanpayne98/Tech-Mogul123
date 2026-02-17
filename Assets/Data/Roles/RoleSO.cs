using UnityEngine;

namespace TechMogul.Data
{
    [CreateAssetMenu(fileName = "New Role", menuName = "TechMogul/Role")]
    public class RoleSO : ScriptableObject
    {
        [Header("Identity")]
        public string roleName;
        [TextArea(2, 4)]
        public string description;
        
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
            if (baseSalaryMin > baseSalaryMax)
            {
                Debug.LogWarning($"Min salary ({baseSalaryMin}) is greater than max ({baseSalaryMax}) in {name}");
            }
            
            if (baseSalaryMin < 0)
            {
                Debug.LogWarning($"Negative salary values in {name}");
            }
            
            // Validate skill ranges
            if (devSkillMin > devSkillMax)
            {
                Debug.LogWarning($"Dev skill min ({devSkillMin}) > max ({devSkillMax}) in {name}");
            }
            
            if (designSkillMin > designSkillMax)
            {
                Debug.LogWarning($"Design skill min ({designSkillMin}) > max ({designSkillMax}) in {name}");
            }
            
            if (marketingSkillMin > marketingSkillMax)
            {
                Debug.LogWarning($"Marketing skill min ({marketingSkillMin}) > max ({marketingSkillMax}) in {name}");
            }
        }
    }
}
