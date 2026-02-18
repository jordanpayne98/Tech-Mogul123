using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using TechMogul.Core;
using TechMogul.Contracts;
using TechMogul.Systems;

namespace TechMogul.UI
{
    public class ContractPanelController : UIController
    {
        [Header("Standalone Dialogs")]
        [SerializeField] private GameObject acceptContractDialogGO;
        [SerializeField] private GameObject contractDetailDialogGO;
        
        private ContractSystem contractSystem;
        private EmployeeSystem employeeSystem;
        private VisualElement contractPanel;
        
        private TimeSpeed previousTimeSpeed;
        private bool wasTimePaused;
        
        private VisualElement availableContractsList;
        private VisualElement activeContractsList;
        private VisualElement completedContractsList;
        
        private VisualElement acceptContractDialog;
        private VisualElement contractDetailView;
        
        private Button closeDialogBtn;
        private Button confirmAcceptBtn;
        private Toggle autoAssignToggle;
        
        private ScrollView employeeSelector;
        private List<string> selectedEmployeeIds = new List<string>();
        
        private ContractData selectedContract;

        protected override void SubscribeToEvents()
        {
            Subscribe<OnContractsChangedEvent>(HandleContractsChanged);
            Subscribe<OnContractAcceptedEvent>(HandleContractAccepted);
            Subscribe<OnContractProgressUpdatedEvent>(HandleContractProgress);
            Subscribe<OnContractCompletedEvent>(HandleContractCompleted);
            Subscribe<OnContractFailedEvent>(HandleContractFailed);
            Subscribe<OnContractExpiredEvent>(HandleContractExpired);
        }

        public void Initialize(VisualElement panel)
        {
            contractPanel = panel;
            
            // Find systems
            contractSystem = FindFirstObjectByType<ContractSystem>();
            employeeSystem = FindFirstObjectByType<EmployeeSystem>();
            
            if (contractSystem == null)
            {
                Debug.LogError("ContractSystem not found in scene!");
                return;
            }
            
            if (employeeSystem == null)
            {
                Debug.LogError("EmployeeSystem not found in scene!");
                return;
            }
            
            Debug.Log($"ContractSystem found with {contractSystem.Contracts.Count} total contracts");
            
            // Get UI elements from panel
            availableContractsList = panel.Q<VisualElement>("available-contracts");
            activeContractsList = panel.Q<VisualElement>("active-contracts");
            completedContractsList = panel.Q<VisualElement>("completed-contracts");
            
            Debug.Log($"Available contracts element: {(availableContractsList != null ? "Found" : "NOT FOUND")}");
            Debug.Log($"Active contracts element: {(activeContractsList != null ? "Found" : "NOT FOUND")}");
            Debug.Log($"Completed contracts element: {(completedContractsList != null ? "Found" : "NOT FOUND")}");
            
            // Get dialogs from standalone GameObjects
            if (acceptContractDialogGO != null)
            {
                var dialogDoc = acceptContractDialogGO.GetComponent<UIDocument>();
                if (dialogDoc != null && dialogDoc.rootVisualElement != null)
                {
                    acceptContractDialog = dialogDoc.rootVisualElement.Q<VisualElement>("overlay");
                    if (acceptContractDialog != null)
                    {
                        acceptContractDialog.style.display = DisplayStyle.None;
                        closeDialogBtn = acceptContractDialog.Q<Button>("close-dialog-btn");
                        confirmAcceptBtn = acceptContractDialog.Q<Button>("confirm-accept-btn");
                        employeeSelector = acceptContractDialog.Q<ScrollView>("employee-selector");
                        autoAssignToggle = acceptContractDialog.Q<Toggle>("auto-assign-toggle");
                        
                        SetupDialogEvents();
                    }
                    else
                    {
                        Debug.LogError("Accept Contract Dialog overlay element not found!");
                    }
                }
                else
                {
                    Debug.LogError("AcceptContractDialog GameObject is missing UIDocument component or rootVisualElement!");
                }
            }
            else
            {
                Debug.LogWarning("AcceptContractDialog GameObject not assigned in ContractPanelController!");
            }
            
            if (contractDetailDialogGO != null)
            {
                var dialogDoc = contractDetailDialogGO.GetComponent<UIDocument>();
                if (dialogDoc != null && dialogDoc.rootVisualElement != null)
                {
                    contractDetailView = dialogDoc.rootVisualElement.Q<VisualElement>("overlay");
                    if (contractDetailView != null)
                    {
                        contractDetailView.style.display = DisplayStyle.None;
                    }
                }
            }
            
            // Delay refresh to allow ContractSystem.Start() to generate initial contracts
            Invoke(nameof(RefreshContractLists), 0.1f);
            
            Debug.Log("ContractPanelController initialized successfully!");
        }

        void SetupDialogEvents()
        {
            if (closeDialogBtn != null)
            {
                closeDialogBtn.clicked += HideAcceptDialog;
            }
            
            if (confirmAcceptBtn != null)
            {
                confirmAcceptBtn.clicked += AcceptContract;
            }
            
            if (autoAssignToggle != null)
            {
                autoAssignToggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        AutoAssignBestTeam();
                    }
                    else
                    {
                        ClearAllAssignments();
                    }
                });
            }
        }

        void ShowAcceptDialog(ContractData contract)
        {
            if (acceptContractDialog == null) return;
            
            // Pause time
            PauseTime();
            
            selectedContract = contract;
            selectedEmployeeIds.Clear();
            
            // Reset auto-assign toggle
            if (autoAssignToggle != null)
            {
                autoAssignToggle.SetValueWithoutNotify(false);
            }
            
            acceptContractDialog.style.display = DisplayStyle.Flex;
            
            // Update dialog content
            var contractInfo = acceptContractDialog.Q<VisualElement>("contract-info");
            if (contractInfo != null)
            {
                contractInfo.Clear();
                
                // Basic info
                var clientLabel = new Label($"Client: {contract.clientName}");
                clientLabel.style.fontSize = 14;
                clientLabel.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                clientLabel.style.marginBottom = 8;
                contractInfo.Add(clientLabel);
                
                contractInfo.Add(new Label($"Type: {contract.template.templateName}"));
                
                // Description right after type
                var descLabel = new Label(contract.template.description);
                descLabel.style.marginTop = 5;
                descLabel.style.marginBottom = 10;
                descLabel.style.fontSize = 11;
                descLabel.style.color = new UnityEngine.Color(0.6f, 0.6f, 0.7f);
                descLabel.style.whiteSpace = UnityEngine.UIElements.WhiteSpace.Normal;
                contractInfo.Add(descLabel);
                
                // Difficulty badge
                var difficultyLabel = new Label($"Difficulty: {contract.difficulty}");
                difficultyLabel.style.marginTop = 5;
                difficultyLabel.style.marginBottom = 5;
                switch (contract.difficulty)
                {
                    case TechMogul.Data.ContractDifficulty.Easy:
                        difficultyLabel.style.color = new UnityEngine.Color(0.3f, 0.9f, 0.5f);
                        break;
                    case TechMogul.Data.ContractDifficulty.Medium:
                        difficultyLabel.style.color = new UnityEngine.Color(0.95f, 0.6f, 0.07f);
                        break;
                    case TechMogul.Data.ContractDifficulty.Hard:
                        difficultyLabel.style.color = new UnityEngine.Color(0.9f, 0.3f, 0.24f);
                        break;
                }
                contractInfo.Add(difficultyLabel);
                
                contractInfo.Add(new Label($"Deadline: {contract.daysRemaining} days"));
                contractInfo.Add(new Label($"Payout: ${contract.basePayout:N0}"));
                
                // Show randomized skill requirements
                var reqHeader = new Label("Skill Requirements:");
                reqHeader.style.marginTop = 10;
                reqHeader.style.fontSize = 12;
                reqHeader.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                reqHeader.style.color = new UnityEngine.Color(0.7f, 0.7f, 0.8f);
                contractInfo.Add(reqHeader);
                
                var devReq = new Label($"• Development: {contract.requiredDevSkill:F0}");
                devReq.style.fontSize = 11;
                devReq.style.marginLeft = 10;
                devReq.style.color = new UnityEngine.Color(0.2f, 0.6f, 0.9f);
                contractInfo.Add(devReq);
                
                var designReq = new Label($"• Design: {contract.requiredDesignSkill:F0}");
                designReq.style.fontSize = 11;
                designReq.style.marginLeft = 10;
                designReq.style.color = new UnityEngine.Color(0.9f, 0.4f, 0.8f);
                contractInfo.Add(designReq);
                
                var marketingReq = new Label($"• Marketing: {contract.requiredMarketingSkill:F0}");
                marketingReq.style.fontSize = 11;
                marketingReq.style.marginLeft = 10;
                marketingReq.style.marginBottom = 5;
                marketingReq.style.color = new UnityEngine.Color(0.3f, 0.8f, 0.5f);
                contractInfo.Add(marketingReq);
                
                // Goals section
                if (contract.selectedGoals != null && contract.selectedGoals.Count > 0)
                {
                    var goalsHeader = new Label("Contract Goals:");
                    goalsHeader.style.marginTop = 12;
                    goalsHeader.style.marginBottom = 5;
                    goalsHeader.style.fontSize = 13;
                    goalsHeader.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                    goalsHeader.style.color = new UnityEngine.Color(0.7f, 0.7f, 0.9f);
                    contractInfo.Add(goalsHeader);
                    
                    for (int i = 0; i < contract.selectedGoals.Count; i++)
                    {
                        var goal = contract.selectedGoals[i];
                        float penalty = i < contract.goalPenalties.Count ? contract.goalPenalties[i] : 0f;
                        float penaltyAmount = contract.basePayout * (penalty / 100f);
                        float targetValue = i < contract.goalTargetValues.Count ? contract.goalTargetValues[i] : goal.targetValue;
                        
                        var goalLabel = new Label($"• {goal.description} (Target: {targetValue:F0}, Penalty: ${penaltyAmount:F0})");
                        goalLabel.style.fontSize = 12;
                        goalLabel.style.marginLeft = 10;
                        goalLabel.style.marginBottom = 3;
                        goalLabel.style.color = new UnityEngine.Color(0.8f, 0.8f, 0.9f);
                        goalLabel.style.whiteSpace = UnityEngine.UIElements.WhiteSpace.Normal;
                        contractInfo.Add(goalLabel);
                    }
                }
            }
            
            RefreshEmployeeSelector();
        }

        void HideAcceptDialog()
        {
            if (acceptContractDialog != null)
            {
                acceptContractDialog.style.display = DisplayStyle.None;
            }
            selectedContract = null;
            
            // Resume time
            ResumeTime();
        }

        void RefreshEmployeeSelector()
        {
            if (employeeSelector == null || employeeSystem == null) return;
            
            employeeSelector.Clear();
            
            var availableEmployees = employeeSystem.Employees.Where(e => e.isAvailable).ToList();
            
            if (availableEmployees.Count == 0)
            {
                var noEmployeesLabel = new Label("No available employees. All employees are currently assigned.");
                noEmployeesLabel.AddToClassList("empty-state-text");
                employeeSelector.Add(noEmployeesLabel);
                return;
            }
            
            // Sort: Selected employees first, then unselected
            // Within each group, sort by total skill (descending)
            var sortedEmployees = availableEmployees
                .OrderByDescending(e => selectedEmployeeIds.Contains(e.employeeId)) // Selected first
                .ThenByDescending(e => e.devSkill + e.designSkill + e.marketingSkill) // Then by total skill
                .ToList();
            
            foreach (var employee in sortedEmployees)
            {
                var employeeRow = new VisualElement();
                employeeRow.AddToClassList("employee-selector-row");
                
                var checkbox = new Toggle();
                checkbox.value = selectedEmployeeIds.Contains(employee.employeeId);
                checkbox.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        selectedEmployeeIds.Add(employee.employeeId);
                    }
                    else
                    {
                        selectedEmployeeIds.Remove(employee.employeeId);
                    }
                    UpdateEstimatedCompletion();
                });
                
                var nameLabel = new Label(employee.employeeName);
                nameLabel.AddToClassList("employee-name");
                
                var skillsLabel = new Label($"Dev: {employee.devSkill:F0} | Design: {employee.designSkill:F0} | Marketing: {employee.marketingSkill:F0}");
                skillsLabel.AddToClassList("employee-skills");
                
                employeeRow.Add(checkbox);
                employeeRow.Add(nameLabel);
                employeeRow.Add(skillsLabel);
                
                employeeSelector.Add(employeeRow);
            }
        }

        void UpdateEstimatedCompletion()
        {
            if (selectedContract == null || selectedEmployeeIds.Count == 0) return;
            
            // Track skill coverage by category
            float totalDevSkill = 0f;
            float totalDesignSkill = 0f;
            float totalMarketingSkill = 0f;
            
            // Build employee list sorted by productivity
            var employees = new List<Employee>();
            foreach (var empId in selectedEmployeeIds)
            {
                var emp = employeeSystem.GetEmployee(empId);
                if (emp != null) 
                {
                    employees.Add(emp);
                    totalDevSkill += emp.devSkill;
                    totalDesignSkill += emp.designSkill;
                    totalMarketingSkill += emp.marketingSkill;
                }
            }
            
            if (employees.Count == 0) return;
            
            // Calculate skill coverage ratios using TOTAL team skills (not average)
            // More employees = more total skill = faster completion!
            float devCoverage = totalDevSkill / Mathf.Max(selectedContract.requiredDevSkill, 1f);
            float designCoverage = totalDesignSkill / Mathf.Max(selectedContract.requiredDesignSkill, 1f);
            float marketingCoverage = totalMarketingSkill / Mathf.Max(selectedContract.requiredMarketingSkill, 1f);
            
            // Use MINIMUM coverage as the bottleneck (weakest skill limits productivity)
            // This prevents one high skill from compensating for low skills
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
            
            // Coordination overhead: penalty for large teams
            if (employees.Count > 3)
            {
                float coordinationPenalty = 1f - ((employees.Count - 3) * 0.08f); // -8% per employee over 3
                coordinationPenalty = Mathf.Max(coordinationPenalty, 0.7f); // Cap penalty at -30%
                finalProductivity *= coordinationPenalty;
            }
            
            // Apply 5% completion speed boost to make contracts slightly easier
            finalProductivity *= 1.05f;
            
            // Cap productivity to prevent trivializing deadlines
            finalProductivity = Mathf.Min(finalProductivity, 150f);
            
            // Calculate estimated days based on productivity
            // 100% productivity = complete in totalDays
            // 50% productivity = takes 2x totalDays
            float daysEstimate = selectedContract.totalDays / (finalProductivity / 100f);
            
            // Update estimate text
            var estimateLabel = acceptContractDialog?.Q<Label>("completion-estimate");
            if (estimateLabel != null)
            {
                string warningText = daysEstimate > selectedContract.daysRemaining 
                    ? " ⚠️ May miss deadline!" 
                    : " ✓ Should complete on time";
                estimateLabel.text = $"Estimated: ~{Mathf.RoundToInt(daysEstimate)} days{warningText}";
            }
            
            // Update skill coverage bars
            UpdateSkillCoverageBars(totalDevSkill, totalDesignSkill, totalMarketingSkill, employees.Count);
        }
        
        void UpdateSkillCoverageBars(float totalDev, float totalDesign, float totalMarketing, int employeeCount)
        {
            if (employeeSelector == null || selectedContract == null) return;
            
            var skillBarsContainer = acceptContractDialog?.Q<VisualElement>("skill-coverage-bars");
            if (skillBarsContainer == null) return;
            
            skillBarsContainer.Clear();
            
            // Use TOTAL team skills (not average) for display
            // This matches the actual calculation used for contract progress
            CreateSkillBar(skillBarsContainer, "Development", totalDev, selectedContract.requiredDevSkill, new UnityEngine.Color(0.2f, 0.6f, 0.9f));
            CreateSkillBar(skillBarsContainer, "Design", totalDesign, selectedContract.requiredDesignSkill, new UnityEngine.Color(0.9f, 0.4f, 0.8f));
            CreateSkillBar(skillBarsContainer, "Marketing", totalMarketing, selectedContract.requiredMarketingSkill, new UnityEngine.Color(0.3f, 0.8f, 0.5f));
        }
        
        void CreateSkillBar(VisualElement container, string skillName, float teamSkill, float requiredSkill, UnityEngine.Color color)
        {
            var barRow = new VisualElement();
            barRow.style.marginBottom = 12;
            
            // Label row
            var labelRow = new VisualElement();
            labelRow.style.flexDirection = UnityEngine.UIElements.FlexDirection.Row;
            labelRow.style.justifyContent = UnityEngine.UIElements.Justify.SpaceBetween;
            labelRow.style.marginBottom = 4;
            labelRow.style.alignItems = UnityEngine.UIElements.Align.Center;
            
            var nameLabel = new Label($"{skillName}: {teamSkill:F0} / {requiredSkill:F0}");
            nameLabel.style.fontSize = 11;
            nameLabel.style.color = new UnityEngine.Color(0.8f, 0.8f, 0.9f);
            nameLabel.style.flexShrink = 0;
            
            // Show status
            float skillRatio = teamSkill / Mathf.Max(requiredSkill, 1f);
            string status = skillRatio >= 1f ? "✓" : skillRatio >= 0.7f ? "~" : "✗";
            var statusLabel = new Label(status);
            statusLabel.style.fontSize = 12;
            statusLabel.style.color = skillRatio >= 1f ? new UnityEngine.Color(0.3f, 0.9f, 0.5f) : 
                                       skillRatio >= 0.7f ? new UnityEngine.Color(0.95f, 0.6f, 0.07f) : 
                                       new UnityEngine.Color(0.9f, 0.3f, 0.24f);
            statusLabel.style.flexShrink = 0;
            statusLabel.style.marginLeft = 10;
            
            labelRow.Add(nameLabel);
            labelRow.Add(statusLabel);
            
            // Progress bar background
            var barBg = new VisualElement();
            barBg.style.height = 14;
            barBg.style.backgroundColor = new UnityEngine.Color(0.15f, 0.15f, 0.2f, 1f);
            barBg.style.borderTopLeftRadius = 3;
            barBg.style.borderTopRightRadius = 3;
            barBg.style.borderBottomLeftRadius = 3;
            barBg.style.borderBottomRightRadius = 3;
            barBg.style.borderTopWidth = 1;
            barBg.style.borderBottomWidth = 1;
            barBg.style.borderLeftWidth = 1;
            barBg.style.borderRightWidth = 1;
            barBg.style.borderTopColor = new UnityEngine.Color(0.3f, 0.3f, 0.4f, 1f);
            barBg.style.borderBottomColor = new UnityEngine.Color(0.3f, 0.3f, 0.4f, 1f);
            barBg.style.borderLeftColor = new UnityEngine.Color(0.3f, 0.3f, 0.4f, 1f);
            barBg.style.borderRightColor = new UnityEngine.Color(0.3f, 0.3f, 0.4f, 1f);
            
            // Progress bar fill - shows team skill vs requirement
            var barFill = new VisualElement();
            float fillPercent = Mathf.Clamp01(teamSkill / Mathf.Max(requiredSkill, 1f));
            barFill.style.width = UnityEngine.UIElements.Length.Percent(fillPercent * 100f);
            barFill.style.height = UnityEngine.UIElements.Length.Percent(100);
            barFill.style.backgroundColor = color;
            barFill.style.borderTopLeftRadius = 2;
            barFill.style.borderTopRightRadius = 2;
            barFill.style.borderBottomLeftRadius = 2;
            barFill.style.borderBottomRightRadius = 2;
            
            barBg.Add(barFill);
            
            barRow.Add(labelRow);
            barRow.Add(barBg);
            
            container.Add(barRow);
        }

        void AcceptContract()
        {
            if (selectedContract == null)
            {
                Debug.LogWarning("No contract selected");
                return;
            }
            
            if (selectedEmployeeIds.Count == 0)
            {
                Debug.LogWarning("At least one employee must be assigned");
                return;
            }
            
            EventBus.Publish(new RequestAcceptContractEvent
            {
                contractId = selectedContract.contractId,
                assignedEmployeeIds = new List<string>(selectedEmployeeIds)
            });
            
            HideAcceptDialog();
        }
        
        void AutoAssignBestTeam()
        {
            if (selectedContract == null || employeeSystem == null) return;
            
            selectedEmployeeIds.Clear();
            
            var availableEmployees = employeeSystem.Employees
                .Where(e => e.isAvailable)
                .ToList();
            
            if (availableEmployees.Count == 0)
            {
                Debug.LogWarning("No available employees to auto-assign");
                RefreshEmployeeSelector();
                return;
            }
            
            // Score each employee based on how well they match contract requirements
            var scoredEmployees = availableEmployees.Select(emp => new
            {
                Employee = emp,
                Score = CalculateEmployeeScore(emp, selectedContract)
            })
            .OrderByDescending(x => x.Score)
            .ToList();
            
            // Try different team combinations to find the best one that can complete on time
            float bestEstimate = float.MaxValue;
            List<string> bestTeam = new List<string>();
            
            // Try team sizes from 1 to all available (max 6 for performance)
            int maxTeamSize = Mathf.Min(6, scoredEmployees.Count);
            
            for (int teamSize = 1; teamSize <= maxTeamSize; teamSize++)
            {
                // Build test team with top N employees
                selectedEmployeeIds.Clear();
                for (int i = 0; i < teamSize; i++)
                {
                    selectedEmployeeIds.Add(scoredEmployees[i].Employee.employeeId);
                }
                
                // Calculate estimated completion for this team
                float estimate = CalculateEstimateForCurrentTeam();
                
                Debug.Log($"Auto-assign testing team size {teamSize}: estimate {estimate:F1} days (deadline: {selectedContract.daysRemaining})");
                
                // If this team can complete on time (with small buffer)
                if (estimate <= selectedContract.daysRemaining * 1.05f)
                {
                    // Found a team that works! Use smallest successful team
                    bestTeam = new List<string>(selectedEmployeeIds);
                    bestEstimate = estimate;
                    Debug.Log($"Auto-assign found working team of size {teamSize} with estimate {estimate:F1} days");
                    break;
                }
                
                // Track best team even if can't meet deadline
                if (estimate < bestEstimate)
                {
                    bestEstimate = estimate;
                    bestTeam = new List<string>(selectedEmployeeIds);
                }
            }
            
            // Use the best team we found (either one that meets deadline, or closest to it)
            selectedEmployeeIds.Clear();
            selectedEmployeeIds.AddRange(bestTeam);
            
            RefreshEmployeeSelector();
            UpdateEstimatedCompletion();
            
            string outcomeMsg = bestEstimate <= selectedContract.daysRemaining 
                ? "that can complete on time" 
                : $"(best possible, but may miss deadline by {Mathf.RoundToInt(bestEstimate - selectedContract.daysRemaining)} days)";
            
            Debug.Log($"Auto-assigned {selectedEmployeeIds.Count} employee(s) for {selectedContract.difficulty} contract {outcomeMsg}");
        }
        
        float CalculateEstimateForCurrentTeam()
        {
            if (selectedContract == null || selectedEmployeeIds.Count == 0) return 999f;
            
            float totalDevSkill = 0f;
            float totalDesignSkill = 0f;
            float totalMarketingSkill = 0f;
            int empCount = 0;
            float totalMorale = 0f;
            float totalBurnout = 0f;
            
            foreach (var empId in selectedEmployeeIds)
            {
                var emp = employeeSystem.GetEmployee(empId);
                if (emp != null)
                {
                    totalDevSkill += emp.devSkill;
                    totalDesignSkill += emp.designSkill;
                    totalMarketingSkill += emp.marketingSkill;
                    totalMorale += emp.morale;
                    totalBurnout += emp.burnout;
                    empCount++;
                }
            }
            
            if (empCount == 0) return 999f;
            
            // Use TOTAL team skills (not average) - more employees = more skill!
            float devCoverage = totalDevSkill / Mathf.Max(selectedContract.requiredDevSkill, 1f);
            float designCoverage = totalDesignSkill / Mathf.Max(selectedContract.requiredDesignSkill, 1f);
            float marketingCoverage = totalMarketingSkill / Mathf.Max(selectedContract.requiredMarketingSkill, 1f);
            
            // Use BOTTLENECK system - same as UpdateEstimatedCompletion()
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
            
            float avgMorale = totalMorale / empCount;
            float avgBurnout = totalBurnout / empCount;
            float moraleMultiplier = Mathf.Lerp(0.5f, 1.0f, avgMorale / 100f);
            float burnoutPenalty = Mathf.Lerp(1.0f, 0.5f, avgBurnout / 100f);
            float effectiveMultiplier = moraleMultiplier * burnoutPenalty;
            effectiveMultiplier = Mathf.Max(effectiveMultiplier, 0.25f);
            
            float finalProductivity = baseProductivity * effectiveMultiplier;
            float teamBonus = 1f + (Mathf.Min(empCount - 1, 4) * 0.12f);
            finalProductivity *= teamBonus;
            
            // Coordination overhead: penalty for large teams
            if (empCount > 3)
            {
                float coordinationPenalty = 1f - ((empCount - 3) * 0.08f); // -8% per employee over 3
                coordinationPenalty = Mathf.Max(coordinationPenalty, 0.7f); // Cap penalty at -30%
                finalProductivity *= coordinationPenalty;
            }
            
            // Apply 5% completion speed boost to make contracts slightly easier
            finalProductivity *= 1.05f;
            
            // Cap productivity to prevent trivializing deadlines
            finalProductivity = Mathf.Min(finalProductivity, 150f);
            
            return selectedContract.totalDays / (finalProductivity / 100f);
        }
        
        float CalculateEmployeeScore(Employee emp, ContractData contract)
        {
            // Calculate how well employee skills match contract requirements
            float devMatch = emp.devSkill / Mathf.Max(contract.requiredDevSkill, 1f);
            float designMatch = emp.designSkill / Mathf.Max(contract.requiredDesignSkill, 1f);
            float marketingMatch = emp.marketingSkill / Mathf.Max(contract.requiredMarketingSkill, 1f);
            
            // Overall match - heavily penalize being below requirements
            float avgMatch = (devMatch + designMatch + marketingMatch) / 3f;
            
            // Apply exponential penalty for being under-skilled
            float skillScore;
            if (avgMatch < 0.8f)
            {
                // Very under-skilled: severe penalty
                skillScore = Mathf.Pow(avgMatch, 2f) * 100f;
            }
            else if (avgMatch < 1.0f)
            {
                // Slightly under-skilled: moderate penalty
                skillScore = 64f + ((avgMatch - 0.8f) / 0.2f) * 36f;
            }
            else
            {
                // Meets or exceeds: good score with bonus
                float excess = avgMatch - 1f;
                skillScore = 100f + (excess * 50f); // Reward over-skilled employees more
            }
            
            // Factor in morale and burnout
            float moraleMultiplier = Mathf.Lerp(0.7f, 1.0f, emp.morale / 100f);
            float burnoutPenalty = Mathf.Lerp(1.0f, 0.7f, emp.burnout / 100f);
            
            return skillScore * moraleMultiplier * burnoutPenalty;
        }
        
        void ClearAllAssignments()
        {
            selectedEmployeeIds.Clear();
            RefreshEmployeeSelector();
            
            // Clear estimate
            var estimateLabel = acceptContractDialog?.Q<Label>("completion-estimate");
            if (estimateLabel != null)
            {
                estimateLabel.text = "Select employees to see estimate";
            }
            
            // Clear skill bars
            var skillBarsContainer = acceptContractDialog?.Q<VisualElement>("skill-coverage-bars");
            if (skillBarsContainer != null)
            {
                skillBarsContainer.Clear();
            }
        }

        void HandleContractsChanged(OnContractsChangedEvent evt)
        {
            RefreshContractLists();
        }

        void HandleContractAccepted(OnContractAcceptedEvent evt)
        {
            RefreshContractLists();
        }

        void HandleContractProgress(OnContractProgressUpdatedEvent evt)
        {
            RefreshContractLists();
        }

        void HandleContractCompleted(OnContractCompletedEvent evt)
        {
            RefreshContractLists();
            
            if (evt.success)
            {
                var notification = $"✅ Completed contract from {evt.clientName} for ${evt.payout:N0}!";
                Debug.Log(notification);
            }
            else
            {
                var notification = $"❌ Failed contract from {evt.clientName} (missed deadline)";
                Debug.LogWarning(notification);
            }
        }

        void HandleContractFailed(OnContractFailedEvent evt)
        {
            RefreshContractLists();
        }

        void HandleContractExpired(OnContractExpiredEvent evt)
        {
            RefreshContractLists();
            Debug.Log($"⏱️ Contract from {evt.clientName} expired");
        }

        void RefreshContractLists()
        {
            if (contractSystem == null) return;
            
            RefreshAvailableContracts();
            RefreshActiveContracts();
            RefreshCompletedContracts();
        }

        void RefreshAvailableContracts()
        {
            if (availableContractsList == null)
            {
                Debug.LogWarning("availableContractsList is null!");
                return;
            }
            
            availableContractsList.Clear();
            
            var contracts = contractSystem.GetAvailableContracts();
            
            Debug.Log($"Refreshing available contracts: {contracts.Count} contracts found");
            
            if (contracts.Count == 0)
            {
                var emptyLabel = new Label("No contracts available. Check back later!");
                emptyLabel.AddToClassList("empty-state-text");
                availableContractsList.Add(emptyLabel);
                return;
            }
            
            foreach (var contract in contracts)
            {
                var card = CreateContractCard(contract, true);
                availableContractsList.Add(card);
            }
        }

        void RefreshActiveContracts()
        {
            if (activeContractsList == null) return;
            
            activeContractsList.Clear();
            
            var contracts = contractSystem.GetActiveContracts();
            
            if (contracts.Count == 0)
            {
                var emptyLabel = new Label("No active contracts");
                emptyLabel.AddToClassList("empty-state-text");
                activeContractsList.Add(emptyLabel);
                return;
            }
            
            foreach (var contract in contracts)
            {
                var card = CreateContractCard(contract, false);
                activeContractsList.Add(card);
            }
        }

        void RefreshCompletedContracts()
        {
            if (completedContractsList == null) return;
            
            completedContractsList.Clear();
            
            var contracts = contractSystem.GetCompletedContracts().Take(10).ToList();
            
            if (contracts.Count == 0)
            {
                var emptyLabel = new Label("No completed contracts yet");
                emptyLabel.AddToClassList("empty-state-text");
                completedContractsList.Add(emptyLabel);
                return;
            }
            
            // Add clear button at the top if there are completed contracts
            var clearButton = new Button(() =>
            {
                EventBus.Publish(new RequestClearCompletedContractsEvent());
            });
            clearButton.text = "Clear Completed";
            clearButton.AddToClassList("clear-completed-btn");
            clearButton.style.marginBottom = 10;
            clearButton.style.backgroundColor = new Color(0.8f, 0.3f, 0.3f, 0.8f);
            clearButton.style.paddingTop = 8;
            clearButton.style.paddingBottom = 8;
            completedContractsList.Add(clearButton);
            
            foreach (var contract in contracts)
            {
                var card = CreateContractCard(contract, false);
                completedContractsList.Add(card);
            }
        }

        VisualElement CreateContractCard(ContractData contract, bool showAcceptButton)
        {
            var card = new VisualElement();
            card.AddToClassList("contract-card");
            
            // Make completed contracts clickable
            if (contract.state == ContractState.Completed || contract.state == ContractState.Failed)
            {
                card.RegisterCallback<ClickEvent>(evt => ShowContractDetail(contract));
            }
            
            // Header
            var header = new VisualElement();
            header.AddToClassList("card-header");
            
            var clientLabel = new Label(contract.clientName);
            clientLabel.AddToClassList("contract-client");
            
            var typeLabel = new Label(contract.template.templateName);
            typeLabel.AddToClassList("contract-type");
            
            // Difficulty badge
            var difficultyBadge = new Label(contract.difficulty.ToString());
            difficultyBadge.style.fontSize = 10;
            difficultyBadge.style.paddingTop = 2;
            difficultyBadge.style.paddingBottom = 2;
            difficultyBadge.style.paddingLeft = 6;
            difficultyBadge.style.paddingRight = 6;
            difficultyBadge.style.borderTopLeftRadius = 3;
            difficultyBadge.style.borderTopRightRadius = 3;
            difficultyBadge.style.borderBottomLeftRadius = 3;
            difficultyBadge.style.borderBottomRightRadius = 3;
            difficultyBadge.style.marginLeft = 8;
            
            switch (contract.difficulty)
            {
                case TechMogul.Data.ContractDifficulty.Easy:
                    difficultyBadge.style.backgroundColor = new UnityEngine.Color(0.2f, 0.8f, 0.4f, 0.3f);
                    difficultyBadge.style.color = new UnityEngine.Color(0.3f, 0.9f, 0.5f);
                    break;
                case TechMogul.Data.ContractDifficulty.Medium:
                    difficultyBadge.style.backgroundColor = new UnityEngine.Color(0.95f, 0.6f, 0.07f, 0.3f);
                    difficultyBadge.style.color = new UnityEngine.Color(0.95f, 0.6f, 0.07f);
                    break;
                case TechMogul.Data.ContractDifficulty.Hard:
                    difficultyBadge.style.backgroundColor = new UnityEngine.Color(0.9f, 0.3f, 0.24f, 0.3f);
                    difficultyBadge.style.color = new UnityEngine.Color(0.9f, 0.3f, 0.24f);
                    break;
            }
            
            header.Add(clientLabel);
            header.Add(typeLabel);
            header.Add(difficultyBadge);
            
            // Info
            var infoContainer = new VisualElement();
            infoContainer.AddToClassList("contract-info");
            
            // Show goals if any
            if (contract.selectedGoals != null && contract.selectedGoals.Count > 0)
            {
                var goalsLabel = new Label($"Goals: {contract.selectedGoals.Count}");
                goalsLabel.style.fontSize = 11;
                goalsLabel.style.color = new UnityEngine.Color(0.7f, 0.7f, 0.9f);
                goalsLabel.style.marginBottom = 3;
                infoContainer.Add(goalsLabel);
                
                foreach (var goal in contract.selectedGoals)
                {
                    var goalLabel = new Label($"• {goal.description}");
                    goalLabel.style.fontSize = 10;
                    goalLabel.style.color = new UnityEngine.Color(0.6f, 0.6f, 0.7f);
                    goalLabel.style.marginLeft = 8;
                    goalLabel.style.marginBottom = 2;
                    infoContainer.Add(goalLabel);
                }
            }
            
            if (contract.state == ContractState.Available)
            {
                var deadlineLabel = new Label($"Deadline: {contract.daysRemaining} days");
                var payoutLabel = new Label($"Payout: ${contract.basePayout:N0}");
                
                // Show expiration countdown
                int daysUntilExpire = 7 - contract.daysAvailable;
                var expireLabel = new Label($"Expires in: {daysUntilExpire} day{(daysUntilExpire != 1 ? "s" : "")}");
                expireLabel.style.fontSize = 10;
                expireLabel.style.marginTop = 4;
                
                if (daysUntilExpire <= 2)
                {
                    expireLabel.style.color = new UnityEngine.Color(0.9f, 0.3f, 0.3f); // Red warning
                }
                else if (daysUntilExpire <= 4)
                {
                    expireLabel.style.color = new UnityEngine.Color(0.95f, 0.6f, 0.07f); // Orange warning
                }
                else
                {
                    expireLabel.style.color = new UnityEngine.Color(0.7f, 0.7f, 0.7f); // Gray
                }
                
                infoContainer.Add(deadlineLabel);
                infoContainer.Add(payoutLabel);
                infoContainer.Add(expireLabel);
            }
            else if (contract.state == ContractState.Active)
            {
                var progressLabel = new Label($"Progress: {contract.progress:F1}%");
                var deadlineLabel = new Label($"Days Left: {contract.daysRemaining}");
                var teamLabel = new Label($"Team: {contract.assignedEmployeeIds.Count} employees");
                
                infoContainer.Add(progressLabel);
                infoContainer.Add(deadlineLabel);
                infoContainer.Add(teamLabel);
            }
            else if (contract.state == ContractState.Completed)
            {
                var payoutLabel = new Label($"Earned: ${contract.totalPayout:N0}");
                var statusLabel = new Label("✓ Completed");
                statusLabel.AddToClassList("status-success");
                
                infoContainer.Add(statusLabel);
                infoContainer.Add(payoutLabel);
            }
            else if (contract.state == ContractState.Failed)
            {
                var statusLabel = new Label("✗ Failed");
                statusLabel.AddToClassList("status-failed");
                
                infoContainer.Add(statusLabel);
            }
            
            card.Add(header);
            card.Add(infoContainer);
            
            // Accept button for available contracts
            if (showAcceptButton)
            {
                var acceptBtn = new Button(() => ShowAcceptDialog(contract));
                acceptBtn.text = "Accept Contract";
                acceptBtn.AddToClassList("accept-btn");
                card.Add(acceptBtn);
            }
            
            return card;
        }
        
        void ShowContractDetail(ContractData contract)
        {
            if (contractDetailView == null) return;
            
            contractDetailView.style.display = DisplayStyle.Flex;
            
            var detailContent = contractDetailView.Q<VisualElement>("detail-content");
            if (detailContent != null)
            {
                detailContent.Clear();
                
                // Header
                var titleLabel = new Label($"{contract.clientName} - {contract.template.templateName}");
                titleLabel.style.fontSize = 16;
                titleLabel.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                titleLabel.style.marginBottom = 15;
                detailContent.Add(titleLabel);
                
                // Status
                var statusLabel = new Label(contract.state == ContractState.Completed ? "✓ COMPLETED" : "✗ FAILED");
                statusLabel.style.fontSize = 14;
                statusLabel.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                statusLabel.AddToClassList(contract.state == ContractState.Completed ? "status-success" : "status-failed");
                detailContent.Add(statusLabel);
                
                // Stats
                var statsContainer = new VisualElement();
                statsContainer.style.marginTop = 10;
                statsContainer.style.marginBottom = 15;
                
                if (contract.state == ContractState.Completed)
                {
                    statsContainer.Add(new Label($"Payout: ${contract.totalPayout:N0}"));
                }
                statsContainer.Add(new Label($"Completed in: {contract.completionDay - contract.startDay} days (Deadline: {contract.totalDays})"));
                detailContent.Add(statsContainer);
                
                // Goals
                var goalsHeader = new Label("Contract Goals:");
                goalsHeader.style.fontSize = 14;
                goalsHeader.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                goalsHeader.style.marginTop = 15;
                goalsHeader.style.marginBottom = 8;
                detailContent.Add(goalsHeader);
                
                if (contract.selectedGoals != null)
                {
                    for (int i = 0; i < contract.selectedGoals.Count; i++)
                    {
                        var goal = contract.selectedGoals[i];
                        bool completed = i < contract.goalCompletionStatus.Count ? contract.goalCompletionStatus[i] : false;
                        float penalty = i < contract.goalPenalties.Count ? contract.goalPenalties[i] : 0f;
                        
                        var goalRow = new VisualElement();
                        goalRow.style.flexDirection = UnityEngine.UIElements.FlexDirection.Row;
                        goalRow.style.marginBottom = 5;
                        goalRow.style.justifyContent = UnityEngine.UIElements.Justify.SpaceBetween;
                        
                        var leftSide = new VisualElement();
                        leftSide.style.flexDirection = UnityEngine.UIElements.FlexDirection.Row;
                        
                        var checkmark = new Label(completed ? "✓" : "✗");
                        checkmark.style.width = 20;
                        checkmark.style.color = completed ? new UnityEngine.Color(0.3f, 0.9f, 0.5f) : new UnityEngine.Color(0.9f, 0.3f, 0.24f);
                        checkmark.style.fontSize = 14;
                        
                        var goalLabel = new Label(goal.description);
                        goalLabel.style.fontSize = 12;
                        goalLabel.style.color = new UnityEngine.Color(0.8f, 0.8f, 0.9f);
                        
                        leftSide.Add(checkmark);
                        leftSide.Add(goalLabel);
                        goalRow.Add(leftSide);
                        
                        // Show penalty if goal was failed
                        if (!completed && penalty > 0)
                        {
                            float penaltyAmount = contract.basePayout * (penalty / 100f);
                            var penaltyLabel = new Label($"-${penaltyAmount:F0} ({penalty:F0}%)");
                            penaltyLabel.style.fontSize = 11;
                            penaltyLabel.style.color = new UnityEngine.Color(0.9f, 0.3f, 0.24f);
                            penaltyLabel.style.marginLeft = 10;
                            goalRow.Add(penaltyLabel);
                        }
                        
                        detailContent.Add(goalRow);
                    }
                }
            }
            
            var closeBtn = contractDetailView.Q<Button>("close-detail-btn");
            if (closeBtn != null)
            {
                closeBtn.clicked += () => contractDetailView.style.display = DisplayStyle.None;
            }
        }
        
        void PauseTime()
        {
            var timeSystem = FindFirstObjectByType<TimeSystem>();
            if (timeSystem != null)
            {
                previousTimeSpeed = timeSystem.CurrentSpeed;
                wasTimePaused = (previousTimeSpeed == TimeSpeed.Paused);
                
                if (!wasTimePaused)
                {
                    EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Paused });
                }
            }
        }
        
        void ResumeTime()
        {
            if (!wasTimePaused)
            {
                EventBus.Publish(new RequestChangeSpeedEvent { Speed = previousTimeSpeed });
            }
        }
    }
}
