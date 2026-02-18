using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class ContractSimulation
    {
        private readonly IRng _rng;
        private readonly EmployeeSystem _employeeSystem;

        public ContractSimulation(IRng rng, EmployeeSystem employeeSystem)
        {
            _rng = rng;
            _employeeSystem = employeeSystem;
        }

        public void TickActiveContracts(List<ContractData> contracts, int currentDay, IEventBus eventBus)
        {
            foreach (var contract in contracts.Where(c => c.state == ContractState.Active).ToList())
            {
                contract.daysRemaining--;

                float dailyProgress = CalculateDailyProgress(contract);
                contract.progress += dailyProgress;

                if (contract.progress >= 100f)
                {
                    eventBus.Publish(new OnContractReadyToCompleteEvent { ContractId = contract.contractId });
                }
                else if (contract.daysRemaining <= 0)
                {
                    eventBus.Publish(new OnContractReadyToFailEvent
                    {
                        ContractId = contract.contractId,
                        ProgressAchieved = contract.progress
                    });
                }
                else
                {
                    eventBus.Publish(new OnContractProgressUpdatedEvent
                    {
                        contractId = contract.contractId,
                        progress = contract.progress,
                        daysRemaining = contract.daysRemaining
                    });
                }
            }
        }

        public void TickAvailableContracts(List<ContractData> contracts)
        {
            foreach (var contract in contracts.Where(c => c.state == ContractState.Available).ToList())
            {
                contract.daysAvailable++;
            }
        }

        public void ApplyDailyBurnout(List<ContractData> contracts, IEventBus eventBus)
        {
            foreach (var contract in contracts.Where(c => c.state == ContractState.Active))
            {
                float dailyBurnout = contract.template.baseBurnoutImpact / contract.totalDays;
                
                float productivity = CalculateTeamProductivity(contract);
                float productivityBurnoutMultiplier = 1f;
                
                if (productivity > 110f)
                {
                    productivityBurnoutMultiplier = 1f + ((productivity - 110f) / 100f * 0.5f);
                }
                
                foreach (var employeeId in contract.assignedEmployeeIds)
                {
                    eventBus.Publish(new RequestAddBurnoutEvent
                    {
                        EmployeeId = employeeId,
                        Amount = dailyBurnout * productivityBurnoutMultiplier
                    });
                }
            }
        }

        public float CalculateDailyProgress(ContractData contract)
        {
            if (contract.assignedEmployeeIds.Count == 0) return 0f;

            float productivity = CalculateTeamProductivity(contract);
            float baseProgress = 100f / contract.totalDays;
            float adjustedProgress = baseProgress * (productivity / 100f);

            return adjustedProgress;
        }

        public float CalculateTeamProductivity(ContractData contract)
        {
            var employees = new List<Employee>();
            foreach (var employeeId in contract.assignedEmployeeIds)
            {
                var emp = GetEmployeeData(employeeId);
                if (emp != null) employees.Add(emp);
            }
            
            if (employees.Count == 0) return 0f;
            
            float totalDevSkill = 0f;
            float totalDesignSkill = 0f;
            float totalMarketingSkill = 0f;
            
            foreach (var emp in employees)
            {
                totalDevSkill += emp.GetEffectiveSkill(SkillType.Development);
                totalDesignSkill += emp.GetEffectiveSkill(SkillType.Design);
                totalMarketingSkill += emp.GetEffectiveSkill(SkillType.Marketing);
            }
            
            float devCoverage = totalDevSkill / Mathf.Max(contract.requiredDevSkill, 1f);
            float designCoverage = totalDesignSkill / Mathf.Max(contract.requiredDesignSkill, 1f);
            float marketingCoverage = totalMarketingSkill / Mathf.Max(contract.requiredMarketingSkill, 1f);
            
            float weakestCoverage = Mathf.Min(devCoverage, designCoverage, marketingCoverage);
            float avgCoverage = (devCoverage + designCoverage + marketingCoverage) / 3f;
            float overallCoverage = (weakestCoverage * 0.85f) + (avgCoverage * 0.15f);
            
            float baseProductivity;
            if (overallCoverage < 0.6f)
            {
                baseProductivity = Mathf.Pow(overallCoverage, 2.5f) * 100f;
            }
            else if (overallCoverage < 0.9f)
            {
                baseProductivity = 21.6f + ((overallCoverage - 0.6f) / 0.3f) * 59.4f;
            }
            else if (overallCoverage < 1.0f)
            {
                baseProductivity = 81f + ((overallCoverage - 0.9f) / 0.1f) * 19f;
            }
            else
            {
                float excess = overallCoverage - 1f;
                baseProductivity = 100f + (excess * 25f);
            }
            
            float totalMorale = 0f;
            float totalBurnout = 0f;
            foreach (var emp in employees)
            {
                totalMorale += emp.morale;
                totalBurnout += emp.burnout;
            }
            float avgMorale = totalMorale / employees.Count;
            float avgBurnout = totalBurnout / employees.Count;
            
            float moraleMultiplier = Mathf.Lerp(0.5f, 1.0f, avgMorale / 100f);
            float burnoutPenalty = Mathf.Lerp(1.0f, 0.5f, avgBurnout / 100f);
            float effectiveMultiplier = moraleMultiplier * burnoutPenalty;
            effectiveMultiplier = Mathf.Max(effectiveMultiplier, 0.25f);
            
            float finalProductivity = baseProductivity * effectiveMultiplier;
            
            float teamBonus = 1f + (Mathf.Min(employees.Count - 1, 4) * 0.12f);
            finalProductivity *= teamBonus;
            
            if (employees.Count > 3)
            {
                float coordinationPenalty = 1f - ((employees.Count - 3) * 0.08f);
                coordinationPenalty = Mathf.Max(coordinationPenalty, 0.7f);
                finalProductivity *= coordinationPenalty;
            }
            
            finalProductivity *= 1.05f;
            finalProductivity = Mathf.Min(finalProductivity, 150f);
            
            return finalProductivity;
        }

        private Employee GetEmployeeData(string employeeId)
        {
            if (_employeeSystem == null) return null;
            return _employeeSystem.Employees.FirstOrDefault(e => e.employeeId == employeeId);
        }
    }

    public class OnContractReadyToCompleteEvent
    {
        public string ContractId;
    }

    public class OnContractReadyToFailEvent
    {
        public string ContractId;
        public float ProgressAchieved;
    }
}
