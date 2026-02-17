using UnityEngine;

namespace TechMogul.Contracts
{
    [CreateAssetMenu(fileName = "ContractTemplate", menuName = "TechMogul/Contract Template")]
    public class ContractTemplateSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string contractType;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Requirements")]
        public int baseDeadlineDays = 30;
        public float targetQuality = 70f;
        
        [Header("Payout")]
        public float basePayoutMin = 10000f;
        public float basePayoutMax = 25000f;
        public float qualityBonusMultiplier = 0.2f;
        
        [Header("Skill Weights (should sum to 1.0)")]
        [Range(0f, 1f)]
        public float devSkillWeight = 0.5f;
        [Range(0f, 1f)]
        public float designSkillWeight = 0.3f;
        [Range(0f, 1f)]
        public float marketingSkillWeight = 0.2f;
        
        [Header("XP Rewards")]
        public float baseXP = 5f;
    }
}
