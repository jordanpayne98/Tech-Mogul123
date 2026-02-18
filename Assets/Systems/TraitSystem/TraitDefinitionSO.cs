using System.Collections.Generic;
using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Traits
{
    [CreateAssetMenu(fileName = "New Trait", menuName = "TechMogul/Trait")]
    public class TraitDefinitionSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID (e.g., 'work_ethic_driven')")]
        public string id;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Classification")]
        public TraitTier tier;
        public TraitCategory category;
        public TraitRarity rarity;
        
        [Header("Tags (for synergies)")]
        public List<TraitTag> tags = new List<TraitTag>();
        
        [Header("Modifiers")]
        public List<ModifierDef> modifiers = new List<ModifierDef>();
        
        [Header("Arc System")]
        public bool arcCapable;
        public ArcDefinitionSO arcDefinition;
        
        public string Id => id;
        
        void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"Trait '{name}' missing stable ID");
            }
            
            if (tags.Count > 3)
            {
                Debug.LogWarning($"Trait '{name}' has more than 3 tags. Spec allows 1-3.");
            }
            
            if (tier == TraitTier.Minor && rarity == TraitRarity.Legendary)
            {
                Debug.LogWarning($"Minor trait '{name}' marked as Legendary. Spec doesn't use Legendary for minors.");
            }
        }
        
        public int GetTagCount(TraitTag tag)
        {
            int count = 0;
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i] == tag)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
