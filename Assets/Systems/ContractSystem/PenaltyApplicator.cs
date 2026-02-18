using UnityEngine;
using System.Collections.Generic;
using TechMogul.Core;
using TechMogul.Data;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class PenaltyApplicator
    {
        private readonly IEventBus _eventBus;
        private readonly ReputationSystem _reputationSystem;
        private readonly bool _showDebugLogs;
        
        public PenaltyApplicator(IEventBus eventBus, ReputationSystem reputationSystem, bool showDebugLogs = false)
        {
            _eventBus = eventBus;
            _reputationSystem = reputationSystem;
            _showDebugLogs = showDebugLogs;
        }
        
        public PenaltyResult ApplyPenalties(ContractData contract, GoalDefinition goal, float basePayout)
        {
            var result = new PenaltyResult();
            
            float currentReputation = _reputationSystem != null ? _reputationSystem.CurrentReputation : 0f;
            float maxReputation = _reputationSystem != null ? _reputationSystem.MaxReputation : 100f;
            
            foreach (var penalty in goal.penalties)
            {
                ApplySinglePenalty(contract, penalty, basePayout, currentReputation, maxReputation, result);
            }
            
            if (_showDebugLogs && result.TotalPayoutReduction > 0)
            {
                Debug.Log($"Applied penalties for goal '{goal.description}': ${result.TotalPayoutReduction:N0} payout reduction, {result.ReputationLoss:F1} reputation loss");
            }
            
            return result;
        }
        
        void ApplySinglePenalty(ContractData contract, PenaltyDefinition penalty, float basePayout, float reputation, float maxReputation, PenaltyResult result)
        {
            switch (penalty.type)
            {
                // Financial Penalties
                case PenaltyType.ReduceBasePayout:
                    float percentReduction = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    float reduction = basePayout * (percentReduction / 100f);
                    result.TotalPayoutReduction += reduction;
                    result.BasePayoutReduction += reduction;
                    break;
                    
                case PenaltyType.FlatFine:
                    float fine = penalty.GetScaledFlatValue(contract.difficulty, reputation, maxReputation);
                    result.TotalPayoutReduction += fine;
                    result.FlatFines += fine;
                    break;
                    
                case PenaltyType.RemoveQualityBonus:
                    result.RemoveQualityBonus = true;
                    break;
                    
                case PenaltyType.ReduceEarlyBonus:
                    float earlyReduction = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.EarlyBonusMultiplier *= (1f - earlyReduction / 100f);
                    break;
                    
                case PenaltyType.RemoveEfficiencyBonus:
                    result.RemoveEfficiencyBonus = true;
                    break;
                
                // Reputation Penalties
                case PenaltyType.ReputationLoss:
                    float repLoss = penalty.GetScaledFlatValue(contract.difficulty, reputation, maxReputation);
                    result.ReputationLoss += repLoss;
                    _eventBus.Publish(new RequestChangeReputationEvent { Amount = -repLoss });
                    break;
                    
                case PenaltyType.ReduceReputationGain:
                    float gainReduction = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.ReputationGainMultiplier *= (1f - gainReduction / 100f);
                    break;
                    
                case PenaltyType.ClientTrustDecrease:
                    // Store client trust penalty for future contracts with this template
                    result.ClientTrustPenalty += penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    break;
                    
                case PenaltyType.LimitHighTierContracts:
                    result.HighTierLimitDays = penalty.durationDays;
                    break;
                
                // Employee Penalties
                case PenaltyType.BurnoutSpike:
                    float burnoutAmount = penalty.GetScaledFlatValue(contract.difficulty, reputation, maxReputation);
                    foreach (var employeeId in contract.assignedEmployeeIds)
                    {
                        _eventBus.Publish(new RequestAddBurnoutEvent
                        {
                            EmployeeId = employeeId,
                            Amount = burnoutAmount
                        });
                    }
                    result.BurnoutSpike += burnoutAmount;
                    break;
                    
                case PenaltyType.MoraleDrop:
                    float moraleAmount = penalty.GetScaledFlatValue(contract.difficulty, reputation, maxReputation);
                    foreach (var employeeId in contract.assignedEmployeeIds)
                    {
                        _eventBus.Publish(new RequestChangeMoraleEvent
                        {
                            EmployeeId = employeeId,
                            Amount = -moraleAmount
                        });
                    }
                    result.MoraleDrop += moraleAmount;
                    break;
                    
                case PenaltyType.ReduceXPGain:
                    float xpReduction = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.XPMultiplier *= (1f - xpReduction / 100f);
                    break;
                    
                case PenaltyType.TemporaryProductivityPenalty:
                    result.ProductivityPenaltyPercent = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.ProductivityPenaltyDays = penalty.durationDays;
                    break;
                
                // Quality Penalties
                case PenaltyType.CapMaxQuality:
                    float cap = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.QualityCap = Mathf.Min(result.QualityCap, cap);
                    break;
                    
                case PenaltyType.QualityMultiplierReduction:
                    float qualityReduction = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.QualityMultiplier *= (1f - qualityReduction / 100f);
                    break;
                    
                case PenaltyType.ClientDissatisfaction:
                    result.ClientDissatisfaction += penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    break;
                
                // Company-Wide Effects
                case PenaltyType.ProductivityDebuff:
                    result.CompanyProductivityDebuff = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.CompanyProductivityDebuffDays = penalty.durationDays;
                    break;
                    
                case PenaltyType.IncreasedBurnoutNext:
                    result.NextContractBurnoutMultiplier = 1f + (penalty.GetScaledValue(contract.difficulty, reputation, maxReputation) / 100f);
                    break;
                    
                case PenaltyType.HigherSalaryExpectations:
                    result.SalaryExpectationIncrease = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    result.SalaryExpectationDuration = penalty.durationDays;
                    break;
                    
                case PenaltyType.NextContractHarder:
                    result.NextContractDifficultyIncrease = penalty.GetScaledValue(contract.difficulty, reputation, maxReputation);
                    break;
            }
        }
    }
    
    public class PenaltyResult
    {
        // Financial
        public float TotalPayoutReduction;
        public float BasePayoutReduction;
        public float FlatFines;
        public bool RemoveQualityBonus;
        public float EarlyBonusMultiplier = 1f;
        public bool RemoveEfficiencyBonus;
        
        // Reputation
        public float ReputationLoss;
        public float ReputationGainMultiplier = 1f;
        public float ClientTrustPenalty;
        public int HighTierLimitDays;
        
        // Employee
        public float BurnoutSpike;
        public float MoraleDrop;
        public float XPMultiplier = 1f;
        public float ProductivityPenaltyPercent;
        public int ProductivityPenaltyDays;
        
        // Quality
        public float QualityCap = 100f;
        public float QualityMultiplier = 1f;
        public float ClientDissatisfaction;
        
        // Company-Wide
        public float CompanyProductivityDebuff;
        public int CompanyProductivityDebuffDays;
        public float NextContractBurnoutMultiplier = 1f;
        public float SalaryExpectationIncrease;
        public int SalaryExpectationDuration;
        public float NextContractDifficultyIncrease;
    }
}
