using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Traits
{
    [CreateAssetMenu(fileName = "SynergyRuleSet", menuName = "TechMogul/Synergy Rule Set")]
    public class SynergyRuleSetSO : ScriptableObject
    {
        [Header("Tier 1 Synergies (2+ same tag)")]
        public List<SynergyRule> tier1Rules = new List<SynergyRule>();
        
        [Header("Tier 2 Synergies (3+ same tag)")]
        public List<SynergyRule> tier2Rules = new List<SynergyRule>();
        
        [ContextMenu("Initialize Locked Synergy Rules")]
        public void InitializeLockedRules()
        {
            tier1Rules = CreateTier1Rules();
            tier2Rules = CreateTier2Rules();
            Debug.Log($"Synergy rules initialized! Tier 1: {tier1Rules.Count} rules, Tier 2: {tier2Rules.Count} rules");
        }
        
        public List<ModifierDef> EvaluateSynergies(Dictionary<TraitTag, int> tagCounts)
        {
            List<ModifierDef> appliedModifiers = new List<ModifierDef>();
            
            foreach (var kvp in tagCounts)
            {
                TraitTag tag = kvp.Key;
                int count = kvp.Value;
                
                if (count >= 3)
                {
                    SynergyRule tier2Rule = GetTier2Rule(tag);
                    if (tier2Rule != null)
                    {
                        for (int i = 0; i < tier2Rule.modifiers.Count; i++)
                        {
                            appliedModifiers.Add(tier2Rule.modifiers[i]);
                        }
                    }
                }
                else if (count >= 2)
                {
                    SynergyRule tier1Rule = GetTier1Rule(tag);
                    if (tier1Rule != null)
                    {
                        for (int i = 0; i < tier1Rule.modifiers.Count; i++)
                        {
                            appliedModifiers.Add(tier1Rule.modifiers[i]);
                        }
                    }
                }
            }
            
            return appliedModifiers;
        }
        
        SynergyRule GetTier1Rule(TraitTag tag)
        {
            for (int i = 0; i < tier1Rules.Count; i++)
            {
                if (tier1Rules[i].tag == tag)
                {
                    return tier1Rules[i];
                }
            }
            return null;
        }
        
        SynergyRule GetTier2Rule(TraitTag tag)
        {
            for (int i = 0; i < tier2Rules.Count; i++)
            {
                if (tier2Rules[i].tag == tag)
                {
                    return tier2Rules[i];
                }
            }
            return null;
        }
        
        List<SynergyRule> CreateTier1Rules()
        {
            List<SynergyRule> rules = new List<SynergyRule>();
            
            // Speed: +4% Productivity (if deadline < 40%, use +6% instead - additive)
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Speed,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = 0.04f },
                    new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = 0.02f, condition = new ConditionDef { type = ConditionType.DeadlineRemainingPct, op = ComparisonOp.LessThan, value = 0.40f }}
                }
            });
            
            // Quality
            rules.Add(new SynergyRule { tag = TraitTag.Quality, requiredCount = 2, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = -0.06f }}});
            
            // Stability
            rules.Add(new SynergyRule { tag = TraitTag.Stability, requiredCount = 2, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.BurnoutRate, op = ModifierOp.AddPercent, value = -0.08f }}});
            
            // Pressure
            rules.Add(new SynergyRule { tag = TraitTag.Pressure, requiredCount = 2, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = 0.06f, condition = new ConditionDef { type = ConditionType.StressCompare, op = ComparisonOp.GreaterThan, value = 0.60f }},
                new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = 0.03f, condition = new ConditionDef { type = ConditionType.StressCompare, op = ComparisonOp.GreaterThan, value = 0.60f }}
            }});
            
            // Innovation
            rules.Add(new SynergyRule { tag = TraitTag.Innovation, requiredCount = 2, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = 0.03f, condition = new ConditionDef { type = ConditionType.UsesNewTech, boolValue = true }}}});
            
            // Leadership
            rules.Add(new SynergyRule { tag = TraitTag.Leadership, requiredCount = 2, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.TeamProductivityImpact, op = ModifierOp.AddPercent, value = 0.05f, condition = new ConditionDef { type = ConditionType.IsProjectLead, boolValue = true }}}});
            
            // Loyalty
            rules.Add(new SynergyRule { tag = TraitTag.Loyalty, requiredCount = 2, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.QuitChance, op = ModifierOp.AddPercent, value = -0.10f }}});
            
            // Growth
            rules.Add(new SynergyRule { tag = TraitTag.Growth, requiredCount = 2, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.SkillGainRate, op = ModifierOp.AddPercent, value = 0.08f }}});
            
            // Influence
            rules.Add(new SynergyRule { tag = TraitTag.Influence, requiredCount = 2, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.TeamProductivityImpact, op = ModifierOp.AddPercent, value = 0.04f },
                new ModifierDef { stat = StatType.ConflictProbability, op = ModifierOp.AddPercent, value = 0.04f }
            }});
            
            // Risk
            rules.Add(new SynergyRule { tag = TraitTag.Risk, requiredCount = 2, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.ProductivityVariance, op = ModifierOp.AddPercent, value = 0.06f }}});
            
            return rules;
        }
        
        List<SynergyRule> CreateTier2Rules()
        {
            List<SynergyRule> rules = new List<SynergyRule>();
            
            // Speed
            rules.Add(new SynergyRule { tag = TraitTag.Speed, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = 0.10f, condition = new ConditionDef { type = ConditionType.MoraleCompare, op = ComparisonOp.GreaterThan, value = 0.60f }},
                new ModifierDef { stat = StatType.BurnoutRate, op = ModifierOp.AddPercent, value = 0.05f }
            }});
            
            // Quality
            rules.Add(new SynergyRule { tag = TraitTag.Quality, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = -0.12f },
                new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = -0.05f }
            }});
            
            // Stability
            rules.Add(new SynergyRule { tag = TraitTag.Stability, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.BurnoutRate, op = ModifierOp.AddPercent, value = -0.15f },
                new ModifierDef { stat = StatType.ConflictProbability, op = ModifierOp.AddPercent, value = -0.10f }
            }});
            
            // Pressure
            rules.Add(new SynergyRule { tag = TraitTag.Pressure, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = 0.12f, condition = new ConditionDef { type = ConditionType.DeadlineRemainingPct, op = ComparisonOp.LessThan, value = 0.30f }},
                new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = 0.05f, condition = new ConditionDef { type = ConditionType.DeadlineRemainingPct, op = ComparisonOp.LessThan, value = 0.30f }}
            }});
            
            // Innovation
            rules.Add(new SynergyRule { tag = TraitTag.Innovation, requiredCount = 3, modifiers = new List<ModifierDef> { new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = 0.10f, condition = new ConditionDef { type = ConditionType.UsesNewTech, boolValue = true }}}});
            
            // Leadership
            rules.Add(new SynergyRule { tag = TraitTag.Leadership, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.TeamProductivityImpact, op = ModifierOp.AddPercent, value = 0.10f, condition = new ConditionDef { type = ConditionType.IsProjectLead, boolValue = true }},
                new ModifierDef { stat = StatType.SalaryDemandGrowth, op = ModifierOp.AddPercent, value = 0.08f }
            }});
            
            // Loyalty
            rules.Add(new SynergyRule { tag = TraitTag.Loyalty, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.QuitChance, op = ModifierOp.AddPercent, value = -0.20f },
                new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = -0.05f }
            }});
            
            // Growth
            rules.Add(new SynergyRule { tag = TraitTag.Growth, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.SkillGainRate, op = ModifierOp.AddPercent, value = 0.15f },
                new ModifierDef { stat = StatType.SalaryDemandGrowth, op = ModifierOp.AddPercent, value = 0.05f }
            }});
            
            // Influence
            rules.Add(new SynergyRule { tag = TraitTag.Influence, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.TeamProductivityImpact, op = ModifierOp.AddPercent, value = 0.10f },
                new ModifierDef { stat = StatType.ConflictProbability, op = ModifierOp.AddPercent, value = 0.10f }
            }});
            
            // Risk
            rules.Add(new SynergyRule { tag = TraitTag.Risk, requiredCount = 3, modifiers = new List<ModifierDef>
            {
                new ModifierDef { stat = StatType.ProductivityVariance, op = ModifierOp.AddPercent, value = 0.12f },
                new ModifierDef { stat = StatType.QuitChance, op = ModifierOp.AddPercent, value = 0.15f, condition = new ConditionDef { type = ConditionType.MoraleCompare, op = ComparisonOp.LessThan, value = 0.60f }}
            }});
            
            return rules;
        }
    }
    
    [System.Serializable]
    public class SynergyRule
    {
        public TraitTag tag;
        public int requiredCount = 2;
        public List<ModifierDef> modifiers = new List<ModifierDef>();
    }
}
