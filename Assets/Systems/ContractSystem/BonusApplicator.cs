using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Data;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class BonusApplicator
    {
        private readonly IEventBus _eventBus;
        private readonly ReputationSystem _reputationSystem;
        private readonly bool _showDebugLogs;
        
        public BonusApplicator(IEventBus eventBus, ReputationSystem reputationSystem, bool showDebugLogs = false)
        {
            _eventBus = eventBus;
            _reputationSystem = reputationSystem;
            _showDebugLogs = showDebugLogs;
        }
        
        public BonusResult ApplyBonuses(ContractData contract, GoalDefinition goal, float basePayout)
        {
            var result = new BonusResult();
            
            // Apply new bonus system
            foreach (var bonus in goal.bonuses)
            {
                ApplySingleBonus(contract, bonus, basePayout, result);
            }
            
            // Apply legacy bonuses for backwards compatibility
            if (goal.bonusPercent > 0)
            {
                result.TotalPayoutIncrease += basePayout * (goal.bonusPercent / 100f);
            }
            
            if (goal.reputationBonus > 0)
            {
                result.ReputationGain += goal.reputationBonus;
            }
            
            if (goal.xpMultiplier > 1f)
            {
                result.XPMultiplier *= goal.xpMultiplier;
            }
            
            if (_showDebugLogs && result.TotalPayoutIncrease > 0)
            {
                Debug.Log($"Applied bonuses for goal '{goal.description}': ${result.TotalPayoutIncrease:N0} bonus, {result.ReputationGain:F1} reputation, {result.XPMultiplier:F2}× XP");
            }
            
            return result;
        }
        
        public BonusResult ApplyPerfectContractBonus(ContractData contract, float basePayout, int goalsCompleted)
        {
            var result = new BonusResult();
            
            // Perfect contract: All goals completed
            if (goalsCompleted == contract.selectedGoals.Count && goalsCompleted > 0)
            {
                float perfectBonus = basePayout * 0.15f; // 15% bonus
                result.TotalPayoutIncrease += perfectBonus;
                result.ReputationGain += 10f;
                result.MoraleBoost += 15f;
                result.IsPerfectContract = true;
                
                if (_showDebugLogs)
                {
                    Debug.Log($"PERFECT CONTRACT! All {goalsCompleted} goals completed. Bonus: ${perfectBonus:N0}, +10 reputation, +15 morale");
                }
            }
            
            return result;
        }
        
        void ApplySingleBonus(ContractData contract, BonusDefinition bonus, float basePayout, BonusResult result)
        {
            switch (bonus.type)
            {
                // Financial Bonuses
                case BonusType.IncreaseBasePayout:
                    float percentIncrease = bonus.GetScaledValue(contract.difficulty, basePayout);
                    float increase = basePayout * (percentIncrease / 100f);
                    result.TotalPayoutIncrease += increase;
                    result.BasePayoutIncrease += increase;
                    break;
                    
                case BonusType.FlatBonus:
                    float flatBonus = bonus.GetScaledFlatValue(contract.difficulty, basePayout);
                    result.TotalPayoutIncrease += flatBonus;
                    result.FlatBonuses += flatBonus;
                    break;
                    
                case BonusType.EarlyCompletionMultiplier:
                    float earlyMultiplier = bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    result.EarlyCompletionMultiplier += earlyMultiplier;
                    break;
                    
                case BonusType.QualityBonusMultiplier:
                    float qualityMultiplier = bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    result.QualityBonusMultiplier += qualityMultiplier;
                    break;
                    
                case BonusType.EfficiencyBonus:
                    float efficiencyBonus = basePayout * (bonus.GetScaledValue(contract.difficulty, basePayout) / 100f);
                    result.TotalPayoutIncrease += efficiencyBonus;
                    result.EfficiencyBonus += efficiencyBonus;
                    break;
                
                // Reputation Bonuses
                case BonusType.ReputationGain:
                    float repGain = bonus.GetScaledFlatValue(contract.difficulty, basePayout);
                    result.ReputationGain += repGain;
                    _eventBus.Publish(new RequestChangeReputationEvent { Amount = repGain });
                    break;
                    
                case BonusType.ReputationMultiplier:
                    float repMultiplier = bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    result.ReputationMultiplier += repMultiplier;
                    break;
                    
                case BonusType.ClientTrustBoost:
                    result.ClientTrustBonus += bonus.GetScaledValue(contract.difficulty, basePayout);
                    break;
                    
                case BonusType.UnlockHighTierFaster:
                    result.HighTierUnlockBonus += bonus.GetScaledValue(contract.difficulty, basePayout);
                    break;
                
                // Employee Bonuses
                case BonusType.ExtraXP:
                    float xpBonus = bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    result.XPMultiplier += xpBonus;
                    break;
                    
                case BonusType.MoraleBoost:
                    float moraleAmount = bonus.GetScaledFlatValue(contract.difficulty, basePayout);
                    foreach (var employeeId in contract.assignedEmployeeIds)
                    {
                        _eventBus.Publish(new RequestChangeMoraleEvent
                        {
                            EmployeeId = employeeId,
                            Amount = moraleAmount
                        });
                    }
                    result.MoraleBoost += moraleAmount;
                    break;
                    
                case BonusType.BurnoutRecovery:
                    float burnoutRecovery = bonus.GetScaledFlatValue(contract.difficulty, basePayout);
                    foreach (var employeeId in contract.assignedEmployeeIds)
                    {
                        _eventBus.Publish(new RequestAddBurnoutEvent
                        {
                            EmployeeId = employeeId,
                            Amount = -burnoutRecovery // Negative = recovery
                        });
                    }
                    result.BurnoutRecovery += burnoutRecovery;
                    break;
                    
                case BonusType.ProductivityBuffNext:
                    result.ProductivityBuffPercent = bonus.GetScaledValue(contract.difficulty, basePayout);
                    result.ProductivityBuffDays = bonus.durationDays;
                    break;
                    
                case BonusType.SkillGrowthBoost:
                    result.SkillGrowthMultiplier += bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    break;
                
                // Company Bonuses
                case BonusType.TemporaryProductivityBoost:
                    result.CompanyProductivityBoost = bonus.GetScaledValue(contract.difficulty, basePayout);
                    result.CompanyProductivityBoostDays = bonus.durationDays;
                    break;
                    
                case BonusType.ReducedBurnoutAccumulation:
                    result.NextContractBurnoutReduction = bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    result.BurnoutReductionDays = bonus.durationDays;
                    break;
                    
                case BonusType.IncreasedContractFrequency:
                    result.ContractFrequencyBoost = bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    result.FrequencyBoostDays = bonus.durationDays;
                    break;
                    
                case BonusType.ReducedSalaryGrowth:
                    result.SalaryGrowthReduction = bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    result.SalaryReductionDuration = bonus.durationDays;
                    break;
                
                // Special Bonuses
                case BonusType.PerfectContractBonus:
                    // Handled separately in ApplyPerfectContractBonus
                    break;
                    
                case BonusType.StreakBonus:
                    result.StreakBonusMultiplier += bonus.GetScaledValue(contract.difficulty, basePayout) / 100f;
                    break;
                    
                case BonusType.ClientSatisfactionBoost:
                    result.ClientSatisfactionBonus += bonus.GetScaledValue(contract.difficulty, basePayout);
                    break;
            }
        }
    }
    
    public class BonusResult
    {
        // Financial
        public float TotalPayoutIncrease;
        public float BasePayoutIncrease;
        public float FlatBonuses;
        public float EfficiencyBonus;
        public float EarlyCompletionMultiplier;
        public float QualityBonusMultiplier;
        
        // Reputation
        public float ReputationGain;
        public float ReputationMultiplier;
        public float ClientTrustBonus;
        public float HighTierUnlockBonus;
        
        // Employee
        public float XPMultiplier = 1f;
        public float MoraleBoost;
        public float BurnoutRecovery;
        public float ProductivityBuffPercent;
        public int ProductivityBuffDays;
        public float SkillGrowthMultiplier;
        
        // Company
        public float CompanyProductivityBoost;
        public int CompanyProductivityBoostDays;
        public float NextContractBurnoutReduction;
        public int BurnoutReductionDays;
        public float ContractFrequencyBoost;
        public int FrequencyBoostDays;
        public float SalaryGrowthReduction;
        public int SalaryReductionDuration;
        
        // Special
        public bool IsPerfectContract;
        public float StreakBonusMultiplier;
        public float ClientSatisfactionBonus;
        
        public void Merge(BonusResult other)
        {
            // Financial
            TotalPayoutIncrease += other.TotalPayoutIncrease;
            BasePayoutIncrease += other.BasePayoutIncrease;
            FlatBonuses += other.FlatBonuses;
            EfficiencyBonus += other.EfficiencyBonus;
            EarlyCompletionMultiplier += other.EarlyCompletionMultiplier;
            QualityBonusMultiplier += other.QualityBonusMultiplier;
            
            // Reputation
            ReputationGain += other.ReputationGain;
            ReputationMultiplier += other.ReputationMultiplier;
            ClientTrustBonus += other.ClientTrustBonus;
            HighTierUnlockBonus += other.HighTierUnlockBonus;
            
            // Employee
            XPMultiplier *= other.XPMultiplier;
            MoraleBoost += other.MoraleBoost;
            BurnoutRecovery += other.BurnoutRecovery;
            ProductivityBuffPercent = Mathf.Max(ProductivityBuffPercent, other.ProductivityBuffPercent);
            ProductivityBuffDays = Mathf.Max(ProductivityBuffDays, other.ProductivityBuffDays);
            SkillGrowthMultiplier += other.SkillGrowthMultiplier;
            
            // Company
            CompanyProductivityBoost = Mathf.Max(CompanyProductivityBoost, other.CompanyProductivityBoost);
            CompanyProductivityBoostDays = Mathf.Max(CompanyProductivityBoostDays, other.CompanyProductivityBoostDays);
            NextContractBurnoutReduction = Mathf.Max(NextContractBurnoutReduction, other.NextContractBurnoutReduction);
            BurnoutReductionDays = Mathf.Max(BurnoutReductionDays, other.BurnoutReductionDays);
            ContractFrequencyBoost += other.ContractFrequencyBoost;
            FrequencyBoostDays = Mathf.Max(FrequencyBoostDays, other.FrequencyBoostDays);
            SalaryGrowthReduction += other.SalaryGrowthReduction;
            SalaryReductionDuration = Mathf.Max(SalaryReductionDuration, other.SalaryReductionDuration);
            
            // Special
            IsPerfectContract |= other.IsPerfectContract;
            StreakBonusMultiplier += other.StreakBonusMultiplier;
            ClientSatisfactionBonus += other.ClientSatisfactionBonus;
        }
    }
}
