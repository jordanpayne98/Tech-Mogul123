using System;
using System.Collections.Generic;
using TechMogul.Contracts;
using TechMogul.Data;

namespace TechMogul.Core.Save
{
    [Serializable]
    public class SerializableContract
    {
        public string contractId;
        public string clientName;
        public string templateId;
        public int difficulty;
        
        public int state;
        public float progress;
        public int daysRemaining;
        public int totalDays;
        public int daysAvailable;
        
        public List<string> assignedEmployeeIds;
        public List<SerializableGoalDefinition> selectedGoals;
        public List<bool> goalCompletionStatus;
        public List<float> goalPenalties;
        public List<float> goalTargetValues;
        
        public float basePayout;
        public float qualityBonus;
        public float totalPayout;
        
        public int creationDay;
        public int startDay;
        public int completionDay;
        
        public float requiredDevSkill;
        public float requiredDesignSkill;
        public float requiredMarketingSkill;
        
        public static SerializableContract FromContract(ContractData contract, IDefinitionResolver resolver)
        {
            if (resolver == null)
            {
                UnityEngine.Debug.LogError("IDefinitionResolver is null in SerializableContract.FromContract");
                return null;
            }
            
            string templateId = string.Empty;
            if (contract.template != null)
            {
                templateId = resolver.GetId(contract.template);
            }
            
            SerializableContract serializable = new SerializableContract
            {
                contractId = contract.contractId,
                clientName = contract.clientName,
                templateId = templateId,
                difficulty = (int)contract.difficulty,
                
                state = (int)contract.state,
                progress = contract.progress,
                daysRemaining = contract.daysRemaining,
                totalDays = contract.totalDays,
                daysAvailable = contract.daysAvailable,
                
                assignedEmployeeIds = contract.assignedEmployeeIds != null ? new List<string>(contract.assignedEmployeeIds) : new List<string>(),
                selectedGoals = new List<SerializableGoalDefinition>(),
                goalCompletionStatus = contract.goalCompletionStatus != null ? new List<bool>(contract.goalCompletionStatus) : new List<bool>(),
                goalPenalties = contract.goalPenalties != null ? new List<float>(contract.goalPenalties) : new List<float>(),
                goalTargetValues = contract.goalTargetValues != null ? new List<float>(contract.goalTargetValues) : new List<float>(),
                
                basePayout = contract.basePayout,
                qualityBonus = contract.qualityBonus,
                totalPayout = contract.totalPayout,
                
                creationDay = contract.creationDay,
                startDay = contract.startDay,
                completionDay = contract.completionDay,
                
                requiredDevSkill = contract.requiredDevSkill,
                requiredDesignSkill = contract.requiredDesignSkill,
                requiredMarketingSkill = contract.requiredMarketingSkill
            };
            
            if (contract.selectedGoals != null)
            {
                foreach (GoalDefinition goal in contract.selectedGoals)
                {
                    serializable.selectedGoals.Add(SerializableGoalDefinition.FromGoalDefinition(goal));
                }
            }
            
            return serializable;
        }
        
        public ContractData ToContract(int currentDay, IDefinitionResolver resolver)
        {
            if (resolver == null)
            {
                UnityEngine.Debug.LogError("IDefinitionResolver is null in SerializableContract.ToContract");
                return null;
            }
            
            TechMogul.Data.ContractTemplateSO template = null;
            if (!string.IsNullOrEmpty(templateId))
            {
                template = resolver.Resolve<TechMogul.Data.ContractTemplateSO>(templateId);
                if (template == null)
                {
                    UnityEngine.Debug.LogWarning($"Failed to resolve contract template with ID: {templateId}");
                }
            }
            
            ContractData contract = new ContractData(contractId, clientName, template, currentDay)
            {
                difficulty = (ContractDifficulty)this.difficulty,
                
                state = (ContractState)this.state,
                progress = this.progress,
                daysRemaining = this.daysRemaining,
                totalDays = this.totalDays,
                daysAvailable = this.daysAvailable,
                
                assignedEmployeeIds = this.assignedEmployeeIds != null ? new List<string>(this.assignedEmployeeIds) : new List<string>(),
                selectedGoals = new List<GoalDefinition>(),
                goalCompletionStatus = this.goalCompletionStatus != null ? new List<bool>(this.goalCompletionStatus) : new List<bool>(),
                goalPenalties = this.goalPenalties != null ? new List<float>(this.goalPenalties) : new List<float>(),
                goalTargetValues = this.goalTargetValues != null ? new List<float>(this.goalTargetValues) : new List<float>(),
                
                basePayout = this.basePayout,
                qualityBonus = this.qualityBonus,
                totalPayout = this.totalPayout,
                
                creationDay = this.creationDay,
                startDay = this.startDay,
                completionDay = this.completionDay,
                
                requiredDevSkill = this.requiredDevSkill,
                requiredDesignSkill = this.requiredDesignSkill,
                requiredMarketingSkill = this.requiredMarketingSkill
            };
            
            if (selectedGoals != null)
            {
                foreach (SerializableGoalDefinition goal in selectedGoals)
                {
                    contract.selectedGoals.Add(goal.ToGoalDefinition());
                }
            }
            
            return contract;
        }
    }
    
    [Serializable]
    public class SerializableGoalDefinition
    {
        public string description;
        public int type;
        public int category;
        public float targetValue;
        public float thresholdValue;
        public float percentageTarget;
        public int integerTarget;
        public float bonusPercent;
        public float reputationBonus;
        public float xpMultiplier;
        public bool isOptional;
        public bool isHidden;
        
        public static SerializableGoalDefinition FromGoalDefinition(GoalDefinition goal)
        {
            return new SerializableGoalDefinition
            {
                description = goal.description ?? string.Empty,
                type = (int)goal.type,
                category = (int)goal.category,
                targetValue = goal.targetValue,
                thresholdValue = goal.thresholdValue,
                percentageTarget = goal.percentageTarget,
                integerTarget = goal.integerTarget,
                bonusPercent = goal.bonusPercent,
                reputationBonus = goal.reputationBonus,
                xpMultiplier = goal.xpMultiplier,
                isOptional = goal.isOptional,
                isHidden = goal.isHidden
            };
        }
        
        public GoalDefinition ToGoalDefinition()
        {
            return new GoalDefinition
            {
                description = this.description ?? string.Empty,
                type = (GoalType)this.type,
                category = (GoalCategory)this.category,
                targetValue = this.targetValue,
                thresholdValue = this.thresholdValue,
                percentageTarget = this.percentageTarget,
                integerTarget = this.integerTarget,
                bonusPercent = this.bonusPercent,
                reputationBonus = this.reputationBonus,
                xpMultiplier = this.xpMultiplier,
                isOptional = this.isOptional,
                isHidden = this.isHidden
            };
        }
    }
}
