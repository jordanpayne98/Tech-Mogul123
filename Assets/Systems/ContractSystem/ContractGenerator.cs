using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TechMogul.Contracts
{
    public class ContractGenerator
    {
        private readonly IRng _rng;
        
        public ContractGenerator(IRng rng)
        {
            _rng = rng;
        }
        
        public ContractGenerator() : this(new UnityRng())
        {
        }
        
        public TechMogul.Data.ContractDifficulty SelectDifficulty(TechMogul.Data.ContractTemplateSO template)
        {
            float totalWeight = template.easyWeight + template.mediumWeight + template.hardWeight;
            if (totalWeight <= 0) return TechMogul.Data.ContractDifficulty.Medium;
            
            float randomValue = _rng.Range(0f, totalWeight);
            
            if (randomValue < template.easyWeight)
                return TechMogul.Data.ContractDifficulty.Easy;
            else if (randomValue < template.easyWeight + template.mediumWeight)
                return TechMogul.Data.ContractDifficulty.Medium;
            else
                return TechMogul.Data.ContractDifficulty.Hard;
        }
        
        public TechMogul.Data.ContractDifficulty SelectReputationAdjustedDifficulty(TechMogul.Data.ContractTemplateSO template, float reputation, float maxReputation)
        {
            float reputationPercent = (reputation / maxReputation) * 100f;
            
            float easyWeight;
            float mediumWeight;
            float hardWeight;
            
            if (reputationPercent < 20f)
            {
                easyWeight = 1f;
                mediumWeight = 0f;
                hardWeight = 0f;
            }
            else if (reputationPercent < 50f)
            {
                easyWeight = 0.7f;
                mediumWeight = 0.3f;
                hardWeight = 0f;
            }
            else if (reputationPercent < 75f)
            {
                easyWeight = 0.3f;
                mediumWeight = 0.5f;
                hardWeight = 0.2f;
            }
            else
            {
                easyWeight = 0.2f;
                mediumWeight = 0.3f;
                hardWeight = 0.5f;
            }
            
            float totalWeight = easyWeight + mediumWeight + hardWeight;
            float randomValue = _rng.Range(0f, totalWeight);
            
            if (randomValue < easyWeight)
                return TechMogul.Data.ContractDifficulty.Easy;
            else if (randomValue < easyWeight + mediumWeight)
                return TechMogul.Data.ContractDifficulty.Medium;
            else
                return TechMogul.Data.ContractDifficulty.Hard;
        }
        
        public float GenerateDevSkill(TechMogul.Data.ContractTemplateSO template, TechMogul.Data.ContractDifficulty difficulty)
        {
            float baseValue = _rng.Range(template.devSkillMin, template.devSkillMax);
            return template.ApplyDifficultyModifier(baseValue, difficulty);
        }
        
        public float GenerateDesignSkill(TechMogul.Data.ContractTemplateSO template, TechMogul.Data.ContractDifficulty difficulty)
        {
            float baseValue = _rng.Range(template.designSkillMin, template.designSkillMax);
            return template.ApplyDifficultyModifier(baseValue, difficulty);
        }
        
        public float GenerateMarketingSkill(TechMogul.Data.ContractTemplateSO template, TechMogul.Data.ContractDifficulty difficulty)
        {
            float baseValue = _rng.Range(template.marketingSkillMin, template.marketingSkillMax);
            return template.ApplyDifficultyModifier(baseValue, difficulty);
        }
        
        public int GenerateDeadline(TechMogul.Data.ContractTemplateSO template, TechMogul.Data.ContractDifficulty difficulty)
        {
            int deadline = template.baseDeadlineDays;
            
            int variance = _rng.Range(-template.deadlineVariance, template.deadlineVariance + 1);
            deadline += variance;
            
            if (template.applyDifficultyDeadlineModifier)
            {
                deadline = template.ApplyDifficultyDeadlineModifier(deadline, difficulty);
            }
            
            return Mathf.Max(deadline, 5);
        }
        
        public List<TechMogul.Data.GoalDefinition> SelectGoals(TechMogul.Data.ContractTemplateSO template, TechMogul.Data.ContractDifficulty difficulty)
        {
            if (template.possibleGoals.Count == 0)
                return new List<TechMogul.Data.GoalDefinition>();
            
            int goalCount = GetGoalCount(template, difficulty);
            goalCount = Mathf.Min(goalCount, template.possibleGoals.Count);
            
            var shuffled = new List<TechMogul.Data.GoalDefinition>(template.possibleGoals);
            ShuffleList(shuffled);
            
            return shuffled.Take(goalCount).ToList();
        }
        
        public float GeneratePayout(TechMogul.Data.ContractTemplateSO template, TechMogul.Data.ContractDifficulty difficulty, int deadline)
        {
            float randomPayout = _rng.Range(template.basePayoutMin, template.basePayoutMax);
            
            switch (difficulty)
            {
                case TechMogul.Data.ContractDifficulty.Easy:
                    randomPayout *= 0.5f;
                    break;
                case TechMogul.Data.ContractDifficulty.Medium:
                    randomPayout *= 1.0f;
                    break;
                case TechMogul.Data.ContractDifficulty.Hard:
                    randomPayout *= 2.0f;
                    break;
            }
            
            float baseDeadline = template.baseDeadlineDays;
            float deadlineRatio = deadline / baseDeadline;
            float deadlineMultiplier = 1.0f / Mathf.Pow(deadlineRatio, 0.5f);
            randomPayout *= deadlineMultiplier;
            
            return randomPayout;
        }
        
        public float GenerateReputationScaling(float reputation, float maxReputation, float employeeMaxSkill)
        {
            float reputationPercent = (reputation / maxReputation) * 100f;
            
            float targetMultiplier;
            if (reputationPercent < 20f)
            {
                targetMultiplier = 2.0f - (reputationPercent / 20f) * 0.2f;
            }
            else if (reputationPercent < 50f)
            {
                float t = (reputationPercent - 20f) / 30f;
                targetMultiplier = 1.8f - (t * 0.15f);
            }
            else
            {
                float t = (reputationPercent - 50f) / 50f;
                targetMultiplier = 1.65f - (t * 0.09f);
            }
            
            float baseScaling = (employeeMaxSkill * targetMultiplier) / 100f;
            
            float variance = _rng.Range(0.85f, 1.15f);
            float scaling = baseScaling * variance;
            
            return Mathf.Clamp(scaling, 0.3f, 2.5f);
        }
        
        int GetGoalCount(TechMogul.Data.ContractTemplateSO template, TechMogul.Data.ContractDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TechMogul.Data.ContractDifficulty.Easy:
                    return template.minGoals;
                    
                case TechMogul.Data.ContractDifficulty.Medium:
                    return _rng.Range(template.minGoals, template.maxGoals + 1);
                    
                case TechMogul.Data.ContractDifficulty.Hard:
                    return template.maxGoals;
                    
                default:
                    return template.minGoals;
            }
        }
        
        void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = _rng.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
