using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Traits
{
    public class TraitEvaluator
    {
        readonly float[] _diminishingReturnsMultipliers = { 1.0f, 0.6f, 0.35f, 0.2f };
        
        readonly SynergyRuleSetSO _synergyRules;
        
        public TraitEvaluator(SynergyRuleSetSO synergyRules)
        {
            _synergyRules = synergyRules;
        }
        
        public Dictionary<StatType, float> EvaluateTraits(
            TraitDefinitionSO majorTrait,
            List<TraitDefinitionSO> minorTraits,
            EmployeeConditionContext context)
        {
            Dictionary<StatType, List<ModifierDef>> modifiersByStat = new Dictionary<StatType, List<ModifierDef>>();
            
            CollectModifiers(majorTrait, context, modifiersByStat);
            
            for (int i = 0; i < minorTraits.Count; i++)
            {
                CollectModifiers(minorTraits[i], context, modifiersByStat);
            }
            
            Dictionary<TraitTag, int> tagCounts = CountTags(majorTrait, minorTraits);
            List<ModifierDef> synergyModifiers = _synergyRules.EvaluateSynergies(tagCounts);
            
            for (int i = 0; i < synergyModifiers.Count; i++)
            {
                if (synergyModifiers[i].ConditionMet(context))
                {
                    if (!modifiersByStat.ContainsKey(synergyModifiers[i].stat))
                    {
                        modifiersByStat[synergyModifiers[i].stat] = new List<ModifierDef>();
                    }
                    modifiersByStat[synergyModifiers[i].stat].Add(synergyModifiers[i]);
                }
            }
            
            return ApplyDiminishingReturns(modifiersByStat);
        }
        
        void CollectModifiers(
            TraitDefinitionSO trait,
            EmployeeConditionContext context,
            Dictionary<StatType, List<ModifierDef>> modifiersByStat)
        {
            if (trait == null)
            {
                return;
            }
            
            for (int i = 0; i < trait.modifiers.Count; i++)
            {
                ModifierDef modifier = trait.modifiers[i];
                
                if (modifier.ConditionMet(context))
                {
                    if (!modifiersByStat.ContainsKey(modifier.stat))
                    {
                        modifiersByStat[modifier.stat] = new List<ModifierDef>();
                    }
                    modifiersByStat[modifier.stat].Add(modifier);
                }
            }
        }
        
        Dictionary<TraitTag, int> CountTags(TraitDefinitionSO majorTrait, List<TraitDefinitionSO> minorTraits)
        {
            Dictionary<TraitTag, int> tagCounts = new Dictionary<TraitTag, int>();
            
            if (majorTrait != null)
            {
                for (int i = 0; i < majorTrait.tags.Count; i++)
                {
                    TraitTag tag = majorTrait.tags[i];
                    if (!tagCounts.ContainsKey(tag))
                    {
                        tagCounts[tag] = 0;
                    }
                    tagCounts[tag]++;
                }
            }
            
            for (int i = 0; i < minorTraits.Count; i++)
            {
                if (minorTraits[i] == null)
                {
                    continue;
                }
                
                for (int j = 0; j < minorTraits[i].tags.Count; j++)
                {
                    TraitTag tag = minorTraits[i].tags[j];
                    if (!tagCounts.ContainsKey(tag))
                    {
                        tagCounts[tag] = 0;
                    }
                    tagCounts[tag]++;
                }
            }
            
            return tagCounts;
        }
        
        Dictionary<StatType, float> ApplyDiminishingReturns(Dictionary<StatType, List<ModifierDef>> modifiersByStat)
        {
            Dictionary<StatType, float> finalModifiers = new Dictionary<StatType, float>();
            
            foreach (var kvp in modifiersByStat)
            {
                StatType stat = kvp.Key;
                List<ModifierDef> modifiers = kvp.Value;
                
                modifiers.Sort((a, b) => b.GetAbsoluteValue().CompareTo(a.GetAbsoluteValue()));
                
                float totalModifier = 0f;
                
                for (int i = 0; i < modifiers.Count; i++)
                {
                    int multiplierIndex = Mathf.Min(i, _diminishingReturnsMultipliers.Length - 1);
                    float multiplier = _diminishingReturnsMultipliers[multiplierIndex];
                    
                    totalModifier += modifiers[i].value * multiplier;
                }
                
                finalModifiers[stat] = totalModifier;
            }
            
            return finalModifiers;
        }
        
        public float RollProductivityVariance(Dictionary<StatType, float> traitModifiers)
        {
            if (traitModifiers.TryGetValue(StatType.ProductivityVariance, out float varianceModifier))
            {
                if (varianceModifier < -100f)
                {
                    return 1.0f;
                }
                
                float varianceRange = varianceModifier;
                return 1.0f + Random.Range(-varianceRange, varianceRange);
            }
            
            return 1.0f;
        }
        
        public float GetEventWeightModifier(Dictionary<StatType, float> traitModifiers, EventCategory category)
        {
            if (traitModifiers.TryGetValue(StatType.EventWeightModifier, out float modifier))
            {
                return Mathf.Clamp(1.0f + modifier, 0.1f, 3.0f);
            }
            
            return 1.0f;
        }
    }
}
