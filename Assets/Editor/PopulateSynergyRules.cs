using UnityEngine;
using UnityEditor;
using TechMogul.Traits;
using System.Collections.Generic;

namespace TechMogul.Editor
{
    public static class PopulateSynergyRules
    {
        [MenuItem("TechMogul/Populate Synergy Rules")]
        public static void Populate()
        {
            SynergyRuleSetSO synergySet = AssetDatabase.LoadAssetAtPath<SynergyRuleSetSO>("Assets/Data/Traits/SynergyRuleSet.asset");
            
            if (synergySet == null)
            {
                Debug.LogError("SynergyRuleSet not found at Assets/Data/Traits/SynergyRuleSet.asset");
                return;
            }
            
            synergySet.tier1Rules = CreateTier1Rules();
            synergySet.tier2Rules = CreateTier2Rules();
            
            EditorUtility.SetDirty(synergySet);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Synergy rules populated successfully! Tier 1: " + synergySet.tier1Rules.Count + " rules, Tier 2: " + synergySet.tier2Rules.Count + " rules");
        }
        
        static List<SynergyRule> CreateTier1Rules()
        {
            List<SynergyRule> rules = new List<SynergyRule>();
            
            // Speed: +4% Productivity (if deadline < 40%, use +6% instead)
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Speed,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = 0.04f },
                    new ModifierDef 
                    { 
                        stat = StatType.Productivity, 
                        op = ModifierOp.AddPercent, 
                        value = 0.02f,
                        condition = new ConditionDef { type = ConditionType.DeadlineRemainingPct, op = ComparisonOp.LessThan, value = 0.40f }
                    }
                }
            });
            
            // Quality: -6% Bug Rate
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Quality,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = -0.06f }
                }
            });
            
            // Stability: -8% Burnout Rate
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Stability,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.BurnoutRate, op = ModifierOp.AddPercent, value = -0.08f }
                }
            });
            
            // Pressure: +6% Productivity when Stress > 60% AND +3% Bug Rate under stress
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Pressure,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef 
                    { 
                        stat = StatType.Productivity, 
                        op = ModifierOp.AddPercent, 
                        value = 0.06f,
                        condition = new ConditionDef { type = ConditionType.StressCompare, op = ComparisonOp.GreaterThan, value = 0.60f }
                    },
                    new ModifierDef 
                    { 
                        stat = StatType.BugRate, 
                        op = ModifierOp.AddPercent, 
                        value = 0.03f,
                        condition = new ConditionDef { type = ConditionType.StressCompare, op = ComparisonOp.GreaterThan, value = 0.60f }
                    }
                }
            });
            
            // Innovation: +5% Feature Value (not implemented yet) AND +3% Bug Rate on new tech
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Innovation,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef 
                    { 
                        stat = StatType.BugRate, 
                        op = ModifierOp.AddPercent, 
                        value = 0.03f,
                        condition = new ConditionDef { type = ConditionType.UsesNewTech, boolValue = true }
                    }
                }
            });
            
            // Leadership: +5% Team Productivity when assigned as lead
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Leadership,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef 
                    { 
                        stat = StatType.TeamProductivityImpact, 
                        op = ModifierOp.AddPercent, 
                        value = 0.05f,
                        condition = new ConditionDef { type = ConditionType.IsProjectLead, boolValue = true }
                    }
                }
            });
            
            // Loyalty: -10% Quit Chance
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Loyalty,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.QuitChance, op = ModifierOp.AddPercent, value = -0.10f }
                }
            });
            
            // Growth: +8% Skill Gain Rate
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Growth,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.SkillGainRate, op = ModifierOp.AddPercent, value = 0.08f }
                }
            });
            
            // Influence: +4% Team Productivity AND +4% Conflict Probability
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Influence,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.TeamProductivityImpact, op = ModifierOp.AddPercent, value = 0.04f },
                    new ModifierDef { stat = StatType.ConflictProbability, op = ModifierOp.AddPercent, value = 0.04f }
                }
            });
            
            // Risk: +6% Productivity Variance
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Risk,
                requiredCount = 2,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.ProductivityVariance, op = ModifierOp.AddPercent, value = 0.06f }
                }
            });
            
            return rules;
        }
        
        static List<SynergyRule> CreateTier2Rules()
        {
            List<SynergyRule> rules = new List<SynergyRule>();
            
            // Speed: +10% Productivity when Morale > 60% AND +5% Burnout Rate
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Speed,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef 
                    { 
                        stat = StatType.Productivity, 
                        op = ModifierOp.AddPercent, 
                        value = 0.10f,
                        condition = new ConditionDef { type = ConditionType.MoraleCompare, op = ComparisonOp.GreaterThan, value = 0.60f }
                    },
                    new ModifierDef { stat = StatType.BurnoutRate, op = ModifierOp.AddPercent, value = 0.05f }
                }
            });
            
            // Quality: -12% Bug Rate AND -5% Productivity
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Quality,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.BugRate, op = ModifierOp.AddPercent, value = -0.12f },
                    new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = -0.05f }
                }
            });
            
            // Stability: -15% Burnout Rate AND -10% Conflict Probability
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Stability,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.BurnoutRate, op = ModifierOp.AddPercent, value = -0.15f },
                    new ModifierDef { stat = StatType.ConflictProbability, op = ModifierOp.AddPercent, value = -0.10f }
                }
            });
            
            // Pressure: +12% Productivity when Deadline < 30% AND +5% Bug Rate under deadline
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Pressure,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef 
                    { 
                        stat = StatType.Productivity, 
                        op = ModifierOp.AddPercent, 
                        value = 0.12f,
                        condition = new ConditionDef { type = ConditionType.DeadlineRemainingPct, op = ComparisonOp.LessThan, value = 0.30f }
                    },
                    new ModifierDef 
                    { 
                        stat = StatType.BugRate, 
                        op = ModifierOp.AddPercent, 
                        value = 0.05f,
                        condition = new ConditionDef { type = ConditionType.DeadlineRemainingPct, op = ComparisonOp.LessThan, value = 0.30f }
                    }
                }
            });
            
            // Innovation: +12% Feature Value (not implemented yet) AND +10% Bug Rate on new tech
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Innovation,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef 
                    { 
                        stat = StatType.BugRate, 
                        op = ModifierOp.AddPercent, 
                        value = 0.10f,
                        condition = new ConditionDef { type = ConditionType.UsesNewTech, boolValue = true }
                    }
                }
            });
            
            // Leadership: +10% Team Productivity AND +8% Salary Demand Growth
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Leadership,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef 
                    { 
                        stat = StatType.TeamProductivityImpact, 
                        op = ModifierOp.AddPercent, 
                        value = 0.10f,
                        condition = new ConditionDef { type = ConditionType.IsProjectLead, boolValue = true }
                    },
                    new ModifierDef { stat = StatType.SalaryDemandGrowth, op = ModifierOp.AddPercent, value = 0.08f }
                }
            });
            
            // Loyalty: -20% Quit Chance AND -5% Productivity
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Loyalty,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.QuitChance, op = ModifierOp.AddPercent, value = -0.20f },
                    new ModifierDef { stat = StatType.Productivity, op = ModifierOp.AddPercent, value = -0.05f }
                }
            });
            
            // Growth: +15% Skill Gain Rate AND +5% Salary Demand Growth
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Growth,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.SkillGainRate, op = ModifierOp.AddPercent, value = 0.15f },
                    new ModifierDef { stat = StatType.SalaryDemandGrowth, op = ModifierOp.AddPercent, value = 0.05f }
                }
            });
            
            // Influence: +10% Team Productivity AND +10% Conflict Probability
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Influence,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.TeamProductivityImpact, op = ModifierOp.AddPercent, value = 0.10f },
                    new ModifierDef { stat = StatType.ConflictProbability, op = ModifierOp.AddPercent, value = 0.10f }
                }
            });
            
            // Risk: +12% Productivity Variance AND +15% Quit Chance when Morale < 60%
            rules.Add(new SynergyRule
            {
                tag = TraitTag.Risk,
                requiredCount = 3,
                modifiers = new List<ModifierDef>
                {
                    new ModifierDef { stat = StatType.ProductivityVariance, op = ModifierOp.AddPercent, value = 0.12f },
                    new ModifierDef 
                    { 
                        stat = StatType.QuitChance, 
                        op = ModifierOp.AddPercent, 
                        value = 0.15f,
                        condition = new ConditionDef { type = ConditionType.MoraleCompare, op = ComparisonOp.LessThan, value = 0.60f }
                    }
                }
            });
            
            return rules;
        }
    }
}
