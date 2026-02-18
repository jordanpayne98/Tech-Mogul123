using System;
using System.Collections.Generic;
using TechMogul.Data;
using UnityEngine;

namespace TechMogul.Contracts
{
    [Serializable]
    public enum ContractState
    {
        Available,
        Active,
        Completed,
        Failed
    }
    
    public struct ContractGenContext
    {
        public float playerReputation;
        public float maxReputation;
        public float employeeMaxSkill;
    }

    [Serializable]
    public class ContractData
    {
        public string contractId;
        public string clientName;
        public TechMogul.Data.ContractTemplateSO template;
        public TechMogul.Data.ContractDifficulty difficulty;
        
        public ContractState state;
        public float progress;
        public int daysRemaining;
        public int totalDays;
        public int daysAvailable;
        
        public List<string> assignedEmployeeIds;
        public List<TechMogul.Data.GoalDefinition> selectedGoals;
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
        
        public string issuingRivalId;
        public string targetCategoryId;
        public ContractType contractType;
        public ContractWorldEffect worldEffect;

        public ContractData(string id, string client, TechMogul.Data.ContractTemplateSO temp, int currentDay)
        {
            contractId = id;
            clientName = client;
            template = temp;
            state = ContractState.Available;
            progress = 0f;
            daysAvailable = 0;
            daysRemaining = 0;
            totalDays = 0;
            assignedEmployeeIds = new List<string>();
            selectedGoals = new List<TechMogul.Data.GoalDefinition>();
            goalCompletionStatus = new List<bool>();
            goalPenalties = new List<float>();
            goalTargetValues = new List<float>();
            basePayout = 0f;
            qualityBonus = 0f;
            totalPayout = 0f;
            creationDay = currentDay;
            startDay = -1;
            completionDay = -1;
            requiredDevSkill = 0f;
            requiredDesignSkill = 0f;
            requiredMarketingSkill = 0f;
            issuingRivalId = "";
            targetCategoryId = "";
            contractType = ContractType.ModuleDevelopment;
            worldEffect = null;
        }

        public ContractData(string id, string client, TechMogul.Data.ContractTemplateSO temp, int currentDay, ContractGenContext ctx, ContractGenerator generator)
        {
            contractId = id;
            clientName = client;
            template = temp;
            state = ContractState.Available;
            progress = 0f;
            daysAvailable = 0;
            
            creationDay = currentDay;
            
            difficulty = generator.SelectReputationAdjustedDifficulty(temp, ctx.playerReputation, ctx.maxReputation);
            
            int randomizedDeadline = generator.GenerateDeadline(temp, difficulty);
            daysRemaining = randomizedDeadline;
            totalDays = randomizedDeadline;
            
            assignedEmployeeIds = new List<string>();
            
            selectedGoals = generator.SelectGoals(temp, difficulty);
            goalCompletionStatus = new List<bool>();
            goalPenalties = new List<float>();
            goalTargetValues = new List<float>();
            for (int i = 0; i < selectedGoals.Count; i++)
            {
                goalCompletionStatus.Add(false);
                goalPenalties.Add(0f); // Penalties now defined in goal definition
                goalTargetValues.Add(selectedGoals[i].GetAdjustedTargetValue(difficulty));
            }
            
            float repScaling = generator.GenerateReputationScaling(ctx.playerReputation, ctx.maxReputation, ctx.employeeMaxSkill);
            
            requiredDevSkill = generator.GenerateDevSkill(temp, difficulty) * repScaling;
            requiredDesignSkill = generator.GenerateDesignSkill(temp, difficulty) * repScaling;
            requiredMarketingSkill = generator.GenerateMarketingSkill(temp, difficulty) * repScaling;
            
            basePayout = generator.GeneratePayout(temp, difficulty, randomizedDeadline);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            float reputationPercent = (ctx.playerReputation / ctx.maxReputation) * 100f;
            UnityEngine.Debug.Log($"[Contract Gen] {difficulty} contract: Deadline {randomizedDeadline}d (base: {temp.baseDeadlineDays}), Payout ${basePayout:F0}, Rep {ctx.playerReputation:F0}/{ctx.maxReputation:F0} ({reputationPercent:F0}%), Scaling: {repScaling:F2}×");
            #endif
            
            qualityBonus = 0f;
            totalPayout = 0f;
            startDay = -1;
            completionDay = -1;
        }
    }
}
