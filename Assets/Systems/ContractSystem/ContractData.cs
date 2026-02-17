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
        public int daysAvailable; // How many days this contract has been available (for expiration)
        
        public List<string> assignedEmployeeIds;
        public List<TechMogul.Data.GoalDefinition> selectedGoals;
        public List<bool> goalCompletionStatus;
        public List<float> goalPenalties; // Penalty % for each goal if failed
        public List<float> goalTargetValues; // Adjusted target values based on difficulty
        
        public float basePayout;
        public float qualityBonus;
        public float totalPayout;
        
        public int startDay;
        public int completionDay;
        
        // Randomized skill requirements for this specific contract
        public float requiredDevSkill;
        public float requiredDesignSkill;
        public float requiredMarketingSkill;

        public ContractData(string id, string client, TechMogul.Data.ContractTemplateSO temp, int currentDay)
            : this(id, client, temp, currentDay, 0f)
        {
        }
        
        public ContractData(string id, string client, TechMogul.Data.ContractTemplateSO temp, int currentDay, float playerReputation)
        {
            contractId = id;
            clientName = client;
            template = temp;
            state = ContractState.Available;
            progress = 0f;
            daysAvailable = 0;
            
            // Adjust difficulty weights based on player reputation
            difficulty = GetReputationAdjustedDifficulty(temp, playerReputation);
            
            // Randomize deadline based on difficulty
            int randomizedDeadline = temp.GetRandomizedDeadline(difficulty);
            daysRemaining = randomizedDeadline;
            totalDays = randomizedDeadline;
            
            assignedEmployeeIds = new List<string>();
            
            // Select random goals from possible goals (influenced by difficulty)
            selectedGoals = temp.GetRandomGoals(difficulty);
            goalCompletionStatus = new List<bool>();
            goalPenalties = new List<float>();
            goalTargetValues = new List<float>();
            for (int i = 0; i < selectedGoals.Count; i++)
            {
                goalCompletionStatus.Add(false);
                // Randomize penalty for this specific goal instance
                goalPenalties.Add(selectedGoals[i].GetRandomPenalty());
                // Store adjusted target value based on difficulty
                goalTargetValues.Add(selectedGoals[i].GetAdjustedTargetValue(difficulty));
            }
            
            // Randomize skill requirements based on contract type and difficulty
            // THEN scale by reputation to match available employee quality
            float repScaling = GetReputationSkillScaling(playerReputation);
            requiredDevSkill = temp.GetRandomDevSkill(difficulty) * repScaling;
            requiredDesignSkill = temp.GetRandomDesignSkill(difficulty) * repScaling;
            requiredMarketingSkill = temp.GetRandomMarketingSkill(difficulty) * repScaling;
            
            // Randomize base payout and apply difficulty modifier
            float randomPayout = UnityEngine.Random.Range(temp.basePayoutMin, temp.basePayoutMax);
            
            // Apply difficulty multiplier to payout
            switch (difficulty)
            {
                case TechMogul.Data.ContractDifficulty.Easy:
                    randomPayout *= 0.5f; // 50% less pay
                    break;
                case TechMogul.Data.ContractDifficulty.Medium:
                    randomPayout *= 1.0f; // Base pay
                    break;
                case TechMogul.Data.ContractDifficulty.Hard:
                    randomPayout *= 2.0f; // 100% more pay (double!)
                    break;
            }
            
            // Apply deadline multiplier to payout
            // Shorter deadlines = higher pay (urgency premium)
            // Longer deadlines = lower pay (more time to complete)
            float baseDeadline = temp.baseDeadlineDays;
            float deadlineRatio = randomizedDeadline / baseDeadline;
            
            // Deadline affects payout inversely:
            // 0.7x deadline (30% shorter) = 1.3x payout (+30%)
            // 1.0x deadline (normal) = 1.0x payout
            // 1.3x deadline (30% longer) = 0.8x payout (-20%)
            float deadlineMultiplier = 1.0f / Mathf.Pow(deadlineRatio, 0.5f);
            randomPayout *= deadlineMultiplier;
            
            basePayout = randomPayout;
            
            UnityEngine.Debug.Log($"[Contract Gen] {difficulty} contract: Deadline {randomizedDeadline}d (base: {baseDeadline}), Payout ${randomPayout:F0}");
            
            qualityBonus = 0f;
            totalPayout = 0f;
            startDay = -1;
            completionDay = -1;
        }
        
        float GetReputationSkillScaling(float reputation)
        {
            // Get the reputation system to understand actual max reputation and scaling
            var reputationSystem = UnityEngine.Object.FindFirstObjectByType<TechMogul.Systems.ReputationSystem>();
            
            float maxReputation = 100f;
            float employeeMaxSkill = 90f; // New max with -10% adjustment
            
            if (reputationSystem != null)
            {
                maxReputation = reputationSystem.MaxReputation;
                employeeMaxSkill = reputationSystem.GetEmployeeQualityMultiplier();
            }
            
            // Calculate reputation as percentage
            float reputationPercent = (reputation / maxReputation) * 100f;
            
            // Scale contract requirements to be CHALLENGING but achievable with team building
            // Early game should require MORE teamwork (higher multiplier)
            // Late game scales down as employees get better at teamwork
            
            // Progressive multiplier: higher at low rep, stabilizes at high rep
            // 0% rep: 2.0× multiplier (need 4-5 employees)
            // 20% rep: 1.8× multiplier (need 3-4 employees)
            // 50% rep: 1.65× multiplier (need 3 employees)
            // 80%+ rep: 1.56× multiplier (need 2-3 employees)
            
            float targetMultiplier;
            if (reputationPercent < 20f)
            {
                // Very early game: 2.0× to 1.8× (need many employees)
                targetMultiplier = 2.0f - (reputationPercent / 20f) * 0.2f;
            }
            else if (reputationPercent < 50f)
            {
                // Early-mid game: 1.8× to 1.65×
                float t = (reputationPercent - 20f) / 30f; // 0 to 1
                targetMultiplier = 1.8f - (t * 0.15f);
            }
            else
            {
                // Mid-late game: 1.65× to 1.56×
                float t = (reputationPercent - 50f) / 50f; // 0 to 1
                targetMultiplier = 1.65f - (t * 0.09f);
            }
            
            float baseScaling = (employeeMaxSkill * targetMultiplier) / 100f; // Normalize to 0-1+ range
            
            // Add variance so not all contracts are same difficulty
            float variance = UnityEngine.Random.Range(0.85f, 1.15f);
            float scaling = baseScaling * variance;
            
            // Clamp to reasonable range (never below 0.3, never above 2.5)
            scaling = UnityEngine.Mathf.Clamp(scaling, 0.3f, 2.5f);
            
            UnityEngine.Debug.Log($"[Contract Scaling] Rep: {reputation:F0}/{maxReputation:F0} ({reputationPercent:F0}%), Employee Max: {employeeMaxSkill:F0}, Multiplier: {targetMultiplier:F2}×, Contract Scaling: {scaling:F2}×");
            
            return scaling;
        }
        
        TechMogul.Data.ContractDifficulty GetReputationAdjustedDifficulty(TechMogul.Data.ContractTemplateSO template, float reputation)
        {
            // Get max reputation to calculate percentages
            var reputationSystem = UnityEngine.Object.FindFirstObjectByType<TechMogul.Systems.ReputationSystem>();
            float maxReputation = 100f;
            
            if (reputationSystem != null)
            {
                maxReputation = reputationSystem.MaxReputation;
            }
            
            // Calculate reputation as percentage
            float reputationPercent = (reputation / maxReputation) * 100f;
            
            // Adjust difficulty weights based on player reputation percentage
            float easyWeight = template.easyWeight;
            float mediumWeight = template.mediumWeight;
            float hardWeight = template.hardWeight;
            
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
                // Mix of Easy, Medium, and Hard
                easyWeight = 0.3f;
                mediumWeight = 0.5f;
                hardWeight = 0.2f;
            }
            else
            {
                // All difficulties available, favor Hard
                easyWeight = 0.2f;
                mediumWeight = 0.3f;
                hardWeight = 0.5f;
            }
            
            // Weighted random selection
            float totalWeight = easyWeight + mediumWeight + hardWeight;
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            
            if (randomValue < easyWeight)
                return TechMogul.Data.ContractDifficulty.Easy;
            else if (randomValue < easyWeight + mediumWeight)
                return TechMogul.Data.ContractDifficulty.Medium;
            else
                return TechMogul.Data.ContractDifficulty.Hard;
        }
    }
}
