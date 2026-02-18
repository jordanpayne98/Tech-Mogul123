using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Traits
{
    [CreateAssetMenu(fileName = "TraitDatabase", menuName = "TechMogul/Trait Database")]
    public class TraitDatabaseSO : ScriptableObject
    {
        [Header("All Traits")]
        public List<TraitDefinitionSO> allTraits = new List<TraitDefinitionSO>();
        
        Dictionary<string, TraitDefinitionSO> _traitLookup;
        
        public void Initialize()
        {
            _traitLookup = new Dictionary<string, TraitDefinitionSO>();
            
            for (int i = 0; i < allTraits.Count; i++)
            {
                if (allTraits[i] != null && !string.IsNullOrEmpty(allTraits[i].id))
                {
                    if (!_traitLookup.ContainsKey(allTraits[i].id))
                    {
                        _traitLookup[allTraits[i].id] = allTraits[i];
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate trait ID: {allTraits[i].id}");
                    }
                }
            }
        }
        
        public TraitDefinitionSO GetTraitById(string traitId)
        {
            if (_traitLookup == null)
            {
                Initialize();
            }
            
            if (_traitLookup.TryGetValue(traitId, out TraitDefinitionSO trait))
            {
                return trait;
            }
            
            Debug.LogWarning($"Trait not found: {traitId}");
            return null;
        }
        
        public List<TraitDefinitionSO> GetTraitsByTier(TraitTier tier)
        {
            List<TraitDefinitionSO> result = new List<TraitDefinitionSO>();
            
            for (int i = 0; i < allTraits.Count; i++)
            {
                if (allTraits[i] != null && allTraits[i].tier == tier)
                {
                    result.Add(allTraits[i]);
                }
            }
            
            return result;
        }
        
        public List<TraitDefinitionSO> GetTraitsByRarity(TraitRarity rarity, TraitTier tier)
        {
            List<TraitDefinitionSO> result = new List<TraitDefinitionSO>();
            
            for (int i = 0; i < allTraits.Count; i++)
            {
                if (allTraits[i] != null && allTraits[i].rarity == rarity && allTraits[i].tier == tier)
                {
                    result.Add(allTraits[i]);
                }
            }
            
            return result;
        }
    }
}
