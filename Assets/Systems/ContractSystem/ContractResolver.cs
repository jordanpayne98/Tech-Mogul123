using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class ContractResolver
    {
        private readonly IRng _rng;
        private readonly EmployeeSystem _employeeSystem;
        private readonly bool _showDebugLogs;

        public ContractResolver(IRng rng, EmployeeSystem employeeSystem, bool showDebugLogs)
        {
            _rng = rng;
            _employeeSystem = employeeSystem;
            _showDebugLogs = showDebugLogs;
        }

        public void CompleteContract(ContractData contract, bool success, int currentDay, IEventBus eventBus)
        {
            contract.state = success ? ContractState.Completed : ContractState.Failed;
            contract.completionDay = currentDay;
            contract.progress = Mathf.Min(contract.progress, 100f);

            if (success)
            {
                float quality = CalculateQuality(contract);
                
                float qualityBonus = Mathf.Max((quality - 50f) / 100f, 0f) * contract.basePayout * 0.5f;
                contract.qualityBonus = qualityBonus;
                contract.totalPayout = contract.basePayout + qualityBonus;

                ApplyCompletionEffects(contract, eventBus);

                foreach (var employeeId in contract.assignedEmployeeIds)
                {
                    eventBus.Publish(new RequestUnassignEmployeeEvent
                    {
                        EmployeeId = employeeId
                    });
                }

                eventBus.Publish(new RequestAddCashEvent { Amount = contract.totalPayout });

                eventBus.Publish(new OnContractCompletedEvent
                {
                    contractId = contract.contractId,
                    clientName = contract.clientName,
                    payout = contract.totalPayout,
                    quality = quality,
                    success = true
                });

                if (_showDebugLogs)
                {
                    Debug.Log($"Contract completed: {contract.clientName} - ${contract.totalPayout:N0} (Quality: {quality:F0})");
                }
            }
            else
            {
                contract.totalPayout = 0f;

                foreach (var employeeId in contract.assignedEmployeeIds)
                {
                    eventBus.Publish(new RequestUnassignEmployeeEvent
                    {
                        EmployeeId = employeeId
                    });
                }

                eventBus.Publish(new OnContractCompletedEvent
                {
                    contractId = contract.contractId,
                    clientName = contract.clientName,
                    quality = 0f,
                    payout = 0f,
                    success = false
                });

                if (_showDebugLogs)
                {
                    Debug.LogWarning($"Contract failed: {contract.clientName} - Progress: {contract.progress:F0}%");
                }
            }
            
            // Clear assigned employees after unassigning them
            contract.assignedEmployeeIds.Clear();
        }

        public float CalculateQuality(ContractData contract)
        {
            if (contract.assignedEmployeeIds.Count == 0) return 0f;

            float qualitySum = 0f;
            float totalMorale = 0f;
            float totalBurnout = 0f;
            int employeeCount = 0;

            foreach (var employeeId in contract.assignedEmployeeIds)
            {
                var employee = GetEmployeeData(employeeId);
                if (employee != null)
                {
                    // Use effective skills (accounts for morale/burnout) for consistency with productivity
                    float devEffective = employee.GetEffectiveSkill(SkillType.Development);
                    float designEffective = employee.GetEffectiveSkill(SkillType.Design);
                    float marketingEffective = employee.GetEffectiveSkill(SkillType.Marketing);
                    
                    float employeeQuality = (devEffective + designEffective + marketingEffective) / 3f;
                    qualitySum += employeeQuality;
                    totalMorale += employee.morale;
                    totalBurnout += employee.burnout;
                    employeeCount++;
                }
            }

            if (employeeCount == 0) return 0f;

            float baseQuality = qualitySum / employeeCount;
            
            // Morale/burnout already applied via GetEffectiveSkill
            // Additional factor for team morale (average across team)
            float avgMorale = totalMorale / employeeCount;
            float avgBurnout = totalBurnout / employeeCount;
            float teamMoraleBonus = Mathf.Lerp(0.9f, 1.1f, avgMorale / 100f);
            float teamBurnoutPenalty = Mathf.Lerp(1.0f, 0.9f, avgBurnout / 100f);
            float teamFactor = teamMoraleBonus * teamBurnoutPenalty;
            
            float deadlineFactor = 1.0f;
            
            // Use startDay and completionDay for accurate timing
            // daysUsed = days worked on the contract
            int daysUsed = contract.completionDay - contract.startDay;
            int totalDays = contract.totalDays;
            
            if (daysUsed < totalDays)
            {
                float daysEarlyPercent = (float)(totalDays - daysUsed) / totalDays;
                deadlineFactor = 1.0f + (daysEarlyPercent * 0.15f);
            }
            else if (daysUsed > totalDays)
            {
                float daysLatePercent = (float)(daysUsed - totalDays) / totalDays;
                deadlineFactor = 1.0f - (daysLatePercent * 0.2f);
                deadlineFactor = Mathf.Max(deadlineFactor, 0.6f);
            }
            
            float finalQuality = baseQuality * teamFactor * deadlineFactor;
            return Mathf.Clamp(finalQuality, 0f, 100f);
        }

        private void ApplyCompletionEffects(ContractData contract, IEventBus eventBus)
        {
            float baseXP = contract.template.baseXPReward;
            
            float totalRequired = contract.requiredDevSkill + contract.requiredDesignSkill + contract.requiredMarketingSkill;
            
            if (totalRequired <= 0)
            {
                Debug.LogWarning($"Contract {contract.contractId} has zero total skill requirements. Skipping XP distribution.");
                return;
            }
            
            float devProportion = contract.requiredDevSkill / totalRequired;
            float designProportion = contract.requiredDesignSkill / totalRequired;
            float marketingProportion = contract.requiredMarketingSkill / totalRequired;

            foreach (var employeeId in contract.assignedEmployeeIds)
            {
                eventBus.Publish(new RequestAddSkillXPEvent
                {
                    EmployeeId = employeeId,
                    DevXP = baseXP * devProportion,
                    DesignXP = baseXP * designProportion,
                    MarketingXP = baseXP * marketingProportion
                });

                var employee = GetEmployeeData(employeeId);
                if (employee != null)
                {
                    employee.totalContractsCompleted++;
                }
            }
        }

        private Employee GetEmployeeData(string employeeId)
        {
            if (_employeeSystem == null) return null;
            return _employeeSystem.Employees.FirstOrDefault(e => e.employeeId == employeeId);
        }
    }
}
