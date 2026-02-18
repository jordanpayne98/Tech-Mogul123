using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Traits
{
    public class TraitGenerator
    {
        readonly TraitDatabaseSO _database;
        
        readonly float[] _majorRarityWeights = { 0.40f, 0.30f, 0.25f, 0.05f };
        readonly float[] _minorRarityWeights = { 0.55f, 0.30f, 0.15f, 0.00f };
        
        public TraitGenerator(TraitDatabaseSO database)
        {
            _database = database;
        }
        
        public EmployeeTraits GenerateTraits()
        {
            TraitDefinitionSO majorTrait = GenerateMajorTrait();
            List<TraitDefinitionSO> minorTraits = GenerateMinorTraits(majorTrait);
            
            return new EmployeeTraits
            {
                majorTrait = majorTrait,
                minorTraits = minorTraits
            };
        }
        
        TraitDefinitionSO GenerateMajorTrait()
        {
            TraitRarity rarity = RollRarity(_majorRarityWeights);
            List<TraitDefinitionSO> candidates = _database.GetTraitsByRarity(rarity, TraitTier.Major);
            
            while (candidates.Count == 0 && rarity > TraitRarity.Common)
            {
                rarity--;
                candidates = _database.GetTraitsByRarity(rarity, TraitTier.Major);
            }
            
            if (candidates.Count == 0)
            {
                Debug.LogError("No major traits available");
                return null;
            }
            
            return candidates[Random.Range(0, candidates.Count)];
        }
        
        List<TraitDefinitionSO> GenerateMinorTraits(TraitDefinitionSO majorTrait)
        {
            List<TraitDefinitionSO> minorTraits = new List<TraitDefinitionSO>();
            List<TraitDefinitionSO> allMinors = _database.GetTraitsByTier(TraitTier.Minor);
            
            if (allMinors.Count < 2)
            {
                Debug.LogError("Not enough minor traits available");
                return minorTraits;
            }
            
            List<TraitDefinitionSO> availableMinors = new List<TraitDefinitionSO>(allMinors);
            
            for (int i = 0; i < 2; i++)
            {
                if (availableMinors.Count == 0)
                {
                    Debug.LogError("Ran out of available minor traits");
                    break;
                }
                
                TraitRarity rarity = RollRarity(_minorRarityWeights);
                List<TraitDefinitionSO> candidates = FilterByRarity(availableMinors, rarity);
                
                while (candidates.Count == 0 && rarity > TraitRarity.Common)
                {
                    rarity--;
                    candidates = FilterByRarity(availableMinors, rarity);
                }
                
                if (candidates.Count == 0)
                {
                    candidates = new List<TraitDefinitionSO>(availableMinors);
                }
                
                TraitDefinitionSO selectedTrait = candidates[Random.Range(0, candidates.Count)];
                minorTraits.Add(selectedTrait);
                availableMinors.Remove(selectedTrait);
            }
            
            return minorTraits;
        }
        
        TraitRarity RollRarity(float[] weights)
        {
            float roll = Random.value;
            float cumulative = 0f;
            
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return (TraitRarity)i;
                }
            }
            
            return TraitRarity.Common;
        }
        
        List<TraitDefinitionSO> FilterByRarity(List<TraitDefinitionSO> traits, TraitRarity rarity)
        {
            List<TraitDefinitionSO> result = new List<TraitDefinitionSO>();
            
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i] != null && traits[i].rarity == rarity)
                {
                    result.Add(traits[i]);
                }
            }
            
            return result;
        }
    }
    
    public struct EmployeeTraits
    {
        public TraitDefinitionSO majorTrait;
        public List<TraitDefinitionSO> minorTraits;
    }
}
