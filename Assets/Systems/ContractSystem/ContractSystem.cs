using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class ContractSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxAvailableContracts = 5;
        [SerializeField] private int newContractsPerWeek = 2;

        [Header("Contract Templates")]
        [SerializeField] private List<TechMogul.Data.ContractTemplateSO> contractTemplates = new List<TechMogul.Data.ContractTemplateSO>();

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private List<ContractData> _contracts = new List<ContractData>();
        private int _currentDay = 0;
        private int _daysSinceLastGeneration = 0;

        public IReadOnlyList<ContractData> Contracts => _contracts;

        void Start()
        {
            GenerateInitialContracts();
        }

        void OnEnable()
        {
            SubscribeToEvents();
        }

        void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        void SubscribeToEvents()
        {
            EventBus.Subscribe<OnDayTickEvent>(HandleDayTick);
            EventBus.Subscribe<RequestAcceptContractEvent>(HandleAcceptContractRequest);
            EventBus.Subscribe<RequestAssignEmployeeToContractEvent>(HandleAssignEmployeeRequest);
            EventBus.Subscribe<RequestUnassignEmployeeFromContractEvent>(HandleUnassignEmployeeRequest);
            EventBus.Subscribe<RequestClearCompletedContractsEvent>(HandleClearCompletedContracts);
            EventBus.Subscribe<RequestLoadContractsEvent>(HandleLoadContracts);
            EventBus.Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<OnDayTickEvent>(HandleDayTick);
            EventBus.Unsubscribe<RequestAcceptContractEvent>(HandleAcceptContractRequest);
            EventBus.Unsubscribe<RequestAssignEmployeeToContractEvent>(HandleAssignEmployeeRequest);
            EventBus.Unsubscribe<RequestUnassignEmployeeFromContractEvent>(HandleUnassignEmployeeRequest);
            EventBus.Unsubscribe<RequestClearCompletedContractsEvent>(HandleClearCompletedContracts);
            EventBus.Unsubscribe<RequestLoadContractsEvent>(HandleLoadContracts);
            EventBus.Unsubscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            _contracts.Clear();
            _currentDay = 0;
            _daysSinceLastGeneration = 0;
            GenerateInitialContracts();
            Debug.Log("ContractSystem reset for new game");
        }
        
        void HandleLoadContracts(RequestLoadContractsEvent evt)
        {
            _contracts.Clear();
            
            foreach (var serializedContract in evt.Contracts)
            {
                ContractData contract = serializedContract.ToContract(_currentDay);
                _contracts.Add(contract);
            }
            
            Debug.Log($"Loaded {_contracts.Count} contracts");
        }

        void HandleDayTick(OnDayTickEvent evt)
        {
            _currentDay = ConvertGameDateToDays(evt.CurrentDate);
            _daysSinceLastGeneration++;

            // Update available contracts: increment days available and check for expiration
            foreach (var contract in _contracts.Where(c => c.state == ContractState.Available).ToList())
            {
                contract.daysAvailable++;
                
                // Expire contracts after 7 days
                if (contract.daysAvailable >= 7)
                {
                    ExpireContract(contract);
                }
            }

            // Update active contracts
            foreach (var contract in _contracts.Where(c => c.state == ContractState.Active).ToList())
            {
                contract.daysRemaining--;

                float dailyProgress = CalculateDailyProgress(contract);
                contract.progress += dailyProgress;

                if (contract.progress >= 100f)
                {
                    CompleteContract(contract, true);
                }
                else if (contract.daysRemaining <= 0)
                {
                    FailContract(contract);
                }
                else
                {
                    EventBus.Publish(new OnContractProgressUpdatedEvent
                    {
                        contractId = contract.contractId,
                        progress = contract.progress,
                        daysRemaining = contract.daysRemaining
                    });
                }
            }

            ApplyDailyBurnout();

            // Generate 2 new contracts every 2-3 days
            if (_daysSinceLastGeneration >= Random.Range(2, 4))
            {
                GenerateNewContracts(2);
                _daysSinceLastGeneration = 0;
            }
        }

        void HandleAcceptContractRequest(RequestAcceptContractEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.contractId);
            if (contract == null)
            {
                Debug.LogWarning($"Contract {evt.contractId} not found");
                return;
            }

            if (contract.state != ContractState.Available)
            {
                Debug.LogWarning($"Contract {evt.contractId} is not available");
                return;
            }

            contract.state = ContractState.Active;
            contract.startDay = _currentDay;

            if (evt.assignedEmployeeIds != null)
            {
                foreach (var employeeId in evt.assignedEmployeeIds)
                {
                    contract.assignedEmployeeIds.Add(employeeId);
                    EventBus.Publish(new RequestAssignEmployeeEvent
                    {
                        EmployeeId = employeeId,
                        AssignmentName = $"Contract: {contract.clientName}"
                    });
                }
            }

            EventBus.Publish(new OnContractAcceptedEvent
            {
                contractId = contract.contractId,
                clientName = contract.clientName,
                deadline = contract.daysRemaining
            });

            if (showDebugLogs)
            {
                Debug.Log($"Accepted contract from '{contract.clientName}' with {contract.assignedEmployeeIds.Count} employees");
            }
        }

        void HandleAssignEmployeeRequest(RequestAssignEmployeeToContractEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.contractId);
            if (contract == null)
            {
                Debug.LogWarning($"Contract {evt.contractId} not found");
                return;
            }

            if (contract.assignedEmployeeIds.Contains(evt.employeeId))
            {
                Debug.LogWarning($"Employee {evt.employeeId} already assigned to contract");
                return;
            }

            contract.assignedEmployeeIds.Add(evt.employeeId);

            EventBus.Publish(new RequestAssignEmployeeEvent
            {
                EmployeeId = evt.employeeId,
                AssignmentName = $"Contract: {contract.clientName}"
            });

            if (showDebugLogs)
            {
                Debug.Log($"Assigned employee to contract from '{contract.clientName}'");
            }
        }

        void HandleUnassignEmployeeRequest(RequestUnassignEmployeeFromContractEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.contractId);
            if (contract == null)
            {
                Debug.LogWarning($"Contract {evt.contractId} not found");
                return;
            }

            if (!contract.assignedEmployeeIds.Remove(evt.employeeId))
            {
                Debug.LogWarning($"Employee {evt.employeeId} not assigned to contract");
                return;
            }

            EventBus.Publish(new RequestUnassignEmployeeEvent
            {
                EmployeeId = evt.employeeId
            });

            if (showDebugLogs)
            {
                Debug.Log($"Unassigned employee from contract '{contract.clientName}'");
            }
        }

        float CalculateDailyProgress(ContractData contract)
        {
            if (contract.assignedEmployeeIds.Count == 0)
            {
                return 0f;
            }

            // Base completion rate: 100% should complete in totalDays
            float baseRate = 100f / contract.totalDays;
            
            // Calculate team productivity
            float teamProductivity = CalculateTeamProductivity(contract);
            
            // Productivity directly affects completion speed
            // 100% productivity = completes on time
            // 50% productivity = takes twice as long
            float productivityMultiplier = teamProductivity / 100f;

            return baseRate * productivityMultiplier;
        }

        float CalculateTeamProductivity(ContractData contract)
        {
            if (contract.assignedEmployeeIds.Count == 0) return 0f;
            
            var employees = new List<Employee>();
            foreach (var employeeId in contract.assignedEmployeeIds)
            {
                var emp = GetEmployeeData(employeeId);
                if (emp != null) employees.Add(emp);
            }
            
            if (employees.Count == 0) return 0f;
            
            // Calculate team average skills
            float totalDevSkill = 0f;
            float totalDesignSkill = 0f;
            float totalMarketingSkill = 0f;
            
            foreach (var emp in employees)
            {
                totalDevSkill += emp.GetEffectiveSkill(SkillType.Development);
                totalDesignSkill += emp.GetEffectiveSkill(SkillType.Design);
                totalMarketingSkill += emp.GetEffectiveSkill(SkillType.Marketing);
            }
            
            // Calculate skill coverage ratios using TOTAL team skills (not average)
            // More employees = more total skill = better coverage!
            float devCoverage = totalDevSkill / Mathf.Max(contract.requiredDevSkill, 1f);
            float designCoverage = totalDesignSkill / Mathf.Max(contract.requiredDesignSkill, 1f);
            float marketingCoverage = totalMarketingSkill / Mathf.Max(contract.requiredMarketingSkill, 1f);
            
            // Use MINIMUM coverage as the bottleneck (weakest skill limits productivity)
            float weakestCoverage = Mathf.Min(devCoverage, designCoverage, marketingCoverage);
            
            // Calculate overall coverage with HEAVY emphasis on weakest skill
            // 85% weight on weakest skill, 15% weight on average of all three
            float avgCoverage = (devCoverage + designCoverage + marketingCoverage) / 3f;
            float overallCoverage = (weakestCoverage * 0.85f) + (avgCoverage * 0.15f);
            
            // Calculate base productivity from skill coverage
            float baseProductivity;
            if (overallCoverage < 0.6f)
            {
                // Severe skill deficiency: exponential penalty
                baseProductivity = Mathf.Pow(overallCoverage, 2.5f) * 100f;
            }
            else if (overallCoverage < 0.9f)
            {
                // Below requirements: strong penalty
                baseProductivity = 21.6f + ((overallCoverage - 0.6f) / 0.3f) * 59.4f; // 22% to 81%
            }
            else if (overallCoverage < 1.0f)
            {
                // Close to requirements: moderate penalty
                baseProductivity = 81f + ((overallCoverage - 0.9f) / 0.1f) * 19f; // 81% to 100%
            }
            else
            {
                // Meets or exceeds requirements: good productivity with bonus
                float excess = overallCoverage - 1f;
                baseProductivity = 100f + (excess * 25f); // Max ~125% with great skills
            }
            
            // Apply team morale and burnout averages
            float totalMorale = 0f;
            float totalBurnout = 0f;
            foreach (var emp in employees)
            {
                totalMorale += emp.morale;
                totalBurnout += emp.burnout;
            }
            float avgMorale = totalMorale / employees.Count;
            float avgBurnout = totalBurnout / employees.Count;
            
            float moraleMultiplier = Mathf.Lerp(0.5f, 1.0f, avgMorale / 100f); // 50% to 100%
            float burnoutPenalty = Mathf.Lerp(1.0f, 0.5f, avgBurnout / 100f); // 100% to 50%
            float effectiveMultiplier = moraleMultiplier * burnoutPenalty;
            effectiveMultiplier = Mathf.Max(effectiveMultiplier, 0.25f);
            
            float finalProductivity = baseProductivity * effectiveMultiplier;
            
            // Team size bonus: diminishing returns
            float teamBonus = 1f + (Mathf.Min(employees.Count - 1, 4) * 0.12f); // Max +48% with 5 employees
            finalProductivity *= teamBonus;
            
            // Apply 5% completion speed boost to make contracts slightly easier
            finalProductivity *= 1.05f;
            
            return finalProductivity;
        }

        float CalculateEmployeeProductivity(Employee employee, ContractData contract)
        {
            // Calculate how well employee's skills match the contract requirements
            // Note: Do NOT clamp - let it go below 1.0 to penalize insufficient skills
            float devMatch = employee.GetEffectiveSkill(SkillType.Development) / Mathf.Max(contract.requiredDevSkill, 1f);
            float designMatch = employee.GetEffectiveSkill(SkillType.Design) / Mathf.Max(contract.requiredDesignSkill, 1f);
            float marketingMatch = employee.GetEffectiveSkill(SkillType.Marketing) / Mathf.Max(contract.requiredMarketingSkill, 1f);
            
            // Average the skill matches
            float averageMatch = (devMatch + designMatch + marketingMatch) / 3f;
            
            // Scale to 0-100+ productivity range
            return averageMatch * 100f;
        }
        
        void EvaluateGoalCompletion(ContractData contract)
        {
            if (contract.selectedGoals == null || contract.selectedGoals.Count == 0) return;
            
            // Calculate team's average skill levels
            float avgDev = 0f;
            float avgDesign = 0f;
            float avgMarketing = 0f;
            int count = 0;
            
            foreach (var employeeId in contract.assignedEmployeeIds)
            {
                var emp = GetEmployeeData(employeeId);
                if (emp != null)
                {
                    avgDev += emp.devSkill;
                    avgDesign += emp.designSkill;
                    avgMarketing += emp.marketingSkill;
                    count++;
                }
            }
            
            if (count > 0)
            {
                avgDev /= count;
                avgDesign /= count;
                avgMarketing /= count;
            }
            
            // Evaluate each goal based on team's overall skill average
            contract.goalCompletionStatus.Clear();
            float teamAverageSkill = (avgDev + avgDesign + avgMarketing) / 3f;
            
            for (int i = 0; i < contract.selectedGoals.Count; i++)
            {
                // Use the adjusted target value for this difficulty
                float targetValue = i < contract.goalTargetValues.Count ? contract.goalTargetValues[i] : contract.selectedGoals[i].targetValue;
                
                // Goal completed if team's average skill meets target (with some randomness)
                float threshold = targetValue + Random.Range(-10f, 10f);
                bool completed = teamAverageSkill >= threshold;
                
                contract.goalCompletionStatus.Add(completed);
            }
        }

        void CompleteContract(ContractData contract, bool success)
        {
            contract.state = success ? ContractState.Completed : ContractState.Failed;
            contract.completionDay = _currentDay;
            contract.progress = Mathf.Min(contract.progress, 100f);
            
            // Evaluate goal completion based on team performance
            EvaluateGoalCompletion(contract);

            if (success)
            {
                float quality = CalculateContractQuality(contract);
                
                // Simple quality bonus: 10% of base payout for every 10 points above 50 quality
                float qualityBonus = Mathf.Max((quality - 50f) / 100f, 0f) * contract.basePayout * 0.5f;

                contract.qualityBonus = qualityBonus;
                contract.totalPayout = contract.basePayout + qualityBonus;
                
                // Apply penalties for failed goals
                float totalPenalty = 0f;
                for (int i = 0; i < contract.goalCompletionStatus.Count; i++)
                {
                    if (!contract.goalCompletionStatus[i])
                    {
                        // Goal failed - apply penalty
                        float penaltyPercent = contract.goalPenalties[i];
                        float penaltyAmount = contract.basePayout * (penaltyPercent / 100f);
                        totalPenalty += penaltyAmount;
                    }
                }
                
                contract.totalPayout -= totalPenalty;
                contract.totalPayout = Mathf.Max(contract.totalPayout, 0f); // Never go negative

                ApplyContractCompletionEffects(contract);

                foreach (var employeeId in contract.assignedEmployeeIds.ToList())
                {
                    EventBus.Publish(new RequestUnassignEmployeeEvent
                    {
                        EmployeeId = employeeId
                    });
                }

                EventBus.Publish(new RequestAddCashEvent { Amount = contract.totalPayout });

                EventBus.Publish(new OnContractCompletedEvent
                {
                    contractId = contract.contractId,
                    clientName = contract.clientName,
                    quality = quality,
                    payout = contract.totalPayout,
                    success = true
                });

                if (showDebugLogs)
                {
                    int goalsCompleted = contract.goalCompletionStatus.Count(g => g);
                    int goalsFailed = contract.goalCompletionStatus.Count(g => !g);
                    string penaltyText = totalPenalty > 0 ? $" (Penalties: -${totalPenalty:F0})" : "";
                    Debug.Log($"Completed contract from '{contract.clientName}' for ${contract.totalPayout:F0} (Goals: {goalsCompleted}/{contract.selectedGoals.Count}){penaltyText}");
                }
            }
            else
            {
                foreach (var employeeId in contract.assignedEmployeeIds.ToList())
                {
                    EventBus.Publish(new RequestUnassignEmployeeEvent
                    {
                        EmployeeId = employeeId
                    });
                }

                EventBus.Publish(new OnContractCompletedEvent
                {
                    contractId = contract.contractId,
                    clientName = contract.clientName,
                    quality = 0f,
                    payout = 0f,
                    success = false
                });

                if (showDebugLogs)
                {
                    Debug.LogWarning($"Failed contract from '{contract.clientName}' (missed deadline)");
                }
            }
        }

        void FailContract(ContractData contract)
        {
            CompleteContract(contract, false);

            EventBus.Publish(new OnContractFailedEvent
            {
                contractId = contract.contractId,
                reason = "Deadline missed"
            });
        }

        void ExpireContract(ContractData contract)
        {
            if (showDebugLogs)
            {
                Debug.Log($"Contract '{contract.clientName}' expired after 7 days");
            }

            _contracts.Remove(contract);

            EventBus.Publish(new OnContractExpiredEvent
            {
                contractId = contract.contractId,
                clientName = contract.clientName
            });
        }

        void HandleClearCompletedContracts(RequestClearCompletedContractsEvent evt)
        {
            int removedCount = _contracts.RemoveAll(c => c.state == ContractState.Completed || c.state == ContractState.Failed);
            
            if (showDebugLogs && removedCount > 0)
            {
                Debug.Log($"Cleared {removedCount} completed/failed contract(s)");
            }

            // Refresh UI
            EventBus.Publish(new OnContractProgressUpdatedEvent
            {
                contractId = "",
                progress = 0,
                daysRemaining = 0
            });
        }

        float CalculateContractQuality(ContractData contract)
        {
            if (contract.assignedEmployeeIds.Count == 0) return 0f;

            float qualitySum = 0f;

            foreach (var employeeId in contract.assignedEmployeeIds)
            {
                var employee = GetEmployeeData(employeeId);
                if (employee != null)
                {
                    // Quality based on average skill level
                    float employeeQuality = (employee.devSkill + employee.designSkill + employee.marketingSkill) / 3f;
                    qualitySum += employeeQuality;
                }
            }

            float averageQuality = qualitySum / contract.assignedEmployeeIds.Count;
            return Mathf.Clamp(averageQuality, 0f, 100f);
        }

        void ApplyContractCompletionEffects(ContractData contract)
        {
            float baseXP = contract.template.baseXPReward;
            
            // Give XP proportional to the skills required for the contract
            float totalRequired = contract.requiredDevSkill + contract.requiredDesignSkill + contract.requiredMarketingSkill;
            float devProportion = contract.requiredDevSkill / totalRequired;
            float designProportion = contract.requiredDesignSkill / totalRequired;
            float marketingProportion = contract.requiredMarketingSkill / totalRequired;

            foreach (var employeeId in contract.assignedEmployeeIds)
            {
                EventBus.Publish(new RequestAddSkillXPEvent
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

        void ApplyDailyBurnout()
        {
            foreach (var contract in _contracts.Where(c => c.state == ContractState.Active))
            {
                float dailyBurnout = contract.template.baseBurnoutImpact / contract.totalDays;
                
                foreach (var employeeId in contract.assignedEmployeeIds)
                {
                    EventBus.Publish(new RequestAddBurnoutEvent
                    {
                        EmployeeId = employeeId,
                        Amount = dailyBurnout
                    });
                }
            }
        }

        void GenerateInitialContracts()
        {
            for (int i = 0; i < maxAvailableContracts; i++)
            {
                GenerateContract();
            }

            if (showDebugLogs)
            {
                Debug.Log($"Generated {maxAvailableContracts} initial contracts");
            }
        }

        void GenerateNewContracts(int count = -1)
        {
            int availableCount = _contracts.Count(c => c.state == ContractState.Available);
            
            // If count not specified, use the old weekly logic
            if (count < 0)
            {
                count = newContractsPerWeek;
            }
            
            int toGenerate = Mathf.Min(count, maxAvailableContracts - availableCount);

            for (int i = 0; i < toGenerate; i++)
            {
                GenerateContract();
            }

            if (toGenerate > 0 && showDebugLogs)
            {
                Debug.Log($"Generated {toGenerate} new contract(s)");
            }
        }

        void GenerateContract()
        {
            if (contractTemplates.Count == 0)
            {
                Debug.LogWarning("No contract templates available");
                return;
            }

            // Get player reputation to influence contract difficulty
            var reputationSystem = FindObjectOfType<ReputationSystem>();
            float reputation = reputationSystem != null ? reputationSystem.CurrentReputation : 0f;

            // Select template based on reputation
            var template = SelectTemplateByReputation(reputation);
            if (template == null)
            {
                template = contractTemplates[Random.Range(0, contractTemplates.Count)];
            }

            string contractId = System.Guid.NewGuid().ToString();
            string clientName = GenerateClientName();

            // Create contract with reputation-adjusted difficulty weights
            var contract = new ContractData(contractId, clientName, template, _currentDay, reputation);
            _contracts.Add(contract);
        }
        
        TechMogul.Data.ContractTemplateSO SelectTemplateByReputation(float reputation)
        {
            // Get max reputation to calculate percentages
            var reputationSystem = FindFirstObjectByType<ReputationSystem>();
            float maxReputation = 100f;
            
            if (reputationSystem != null)
            {
                maxReputation = reputationSystem.MaxReputation;
            }
            
            // Calculate reputation as percentage
            float reputationPercent = (reputation / maxReputation) * 100f;
            
            // Reputation determines which difficulties are available
            // 0-20%: Only Easy (100%)
            // 20-50%: Easy (70%) + Medium (30%)
            // 50-75%: Easy (30%) + Medium (50%) + Hard (20%)
            // 75-100%: Easy (10%) + Medium (40%) + Hard (50%)
            
            List<TechMogul.Data.ContractTemplateSO> availableTemplates = new List<TechMogul.Data.ContractTemplateSO>();
            List<float> weights = new List<float>();
            
            foreach (var template in contractTemplates)
            {
                float easyWeight = 0f;
                float mediumWeight = 0f;
                float hardWeight = 0f;
                
                if (reputationPercent < 20f)
                {
                    // Only Easy contracts
                    easyWeight = 1f;
                    mediumWeight = 0f;
                    hardWeight = 0f;
                }
                else if (reputationPercent < 50f)
                {
                    // Mostly Easy, some Medium
                    easyWeight = 0.7f;
                    mediumWeight = 0.3f;
                    hardWeight = 0f;
                }
                else if (reputationPercent < 75f)
                {
                    // Balanced with some Hard
                    easyWeight = 0.3f;
                    mediumWeight = 0.5f;
                    hardWeight = 0.2f;
                }
                else
                {
                    // Mostly Medium and Hard
                    easyWeight = 0.1f;
                    mediumWeight = 0.4f;
                    hardWeight = 0.5f;
                }
                
                float totalWeight = easyWeight + mediumWeight + hardWeight;
                if (totalWeight > 0f)
                {
                    availableTemplates.Add(template);
                    weights.Add(totalWeight);
                }
            }
            
            if (availableTemplates.Count == 0)
                return null;
                
            // Weighted random selection
            float totalSum = weights.Sum();
            float randomValue = Random.Range(0f, totalSum);
            float cumulative = 0f;
            
            for (int i = 0; i < availableTemplates.Count; i++)
            {
                cumulative += weights[i];
                if (randomValue <= cumulative)
                {
                    return availableTemplates[i];
                }
            }
            
            return availableTemplates[availableTemplates.Count - 1];
        }

        string GenerateClientName()
        {
            string[] prefixes = { "Tech", "Cyber", "Digital", "Cloud", "Smart", "Net", "Data", "Web", "Info", "Soft" };
            string[] suffixes = { "Corp", "Systems", "Solutions", "Industries", "Group", "Dynamics", "Innovations", "Partners", "Labs", "Tech" };

            string prefix = prefixes[Random.Range(0, prefixes.Length)];
            string suffix = suffixes[Random.Range(0, suffixes.Length)];

            return $"{prefix}{suffix}";
        }

        Employee GetEmployeeData(string employeeId)
        {
            var employeeSystem = FindObjectOfType<EmployeeSystem>();
            if (employeeSystem == null) return null;

            return employeeSystem.Employees.FirstOrDefault(e => e.employeeId == employeeId);
        }

        public ContractData GetContract(string contractId)
        {
            return _contracts.FirstOrDefault(c => c.contractId == contractId);
        }
        
        public ContractData GetContractById(string contractId)
        {
            return GetContract(contractId);
        }

        public List<ContractData> GetAvailableContracts()
        {
            return _contracts.Where(c => c.state == ContractState.Available).ToList();
        }

        public List<ContractData> GetActiveContracts()
        {
            return _contracts.Where(c => c.state == ContractState.Active).ToList();
        }

        public List<ContractData> GetCompletedContracts()
        {
            return _contracts.Where(c => c.state == ContractState.Completed).ToList();
        }
        
        int ConvertGameDateToDays(GameDate date)
        {
            return (date.Year * 365) + (date.Month * 30) + date.Day;
        }
    }
}
