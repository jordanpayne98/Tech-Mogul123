using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Data;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class ContractOfferGenerator
    {
        private readonly List<ContractTemplateSO> _templates;
        private readonly ContractGenerator _contractGenerator;
        private readonly IRng _rng;
        private readonly ReputationSystem _reputationSystem;
        private readonly bool _showDebugLogs;

        public ContractOfferGenerator(
            List<ContractTemplateSO> templates,
            ContractGenerator contractGenerator,
            IRng rng,
            ReputationSystem reputationSystem,
            bool showDebugLogs)
        {
            _templates = templates;
            _contractGenerator = contractGenerator;
            _rng = rng;
            _reputationSystem = reputationSystem;
            _showDebugLogs = showDebugLogs;
        }

        public void GenerateInitialContracts(List<ContractData> contracts, int maxAvailableContracts, int currentDay)
        {
            for (int i = 0; i < maxAvailableContracts; i++)
            {
                var contract = GenerateSingleContract(currentDay);
                if (contract != null)
                {
                    contracts.Add(contract);
                }
            }

            if (_showDebugLogs)
            {
                Debug.Log($"Generated {maxAvailableContracts} initial contracts");
            }
        }

        public void GenerateNewContracts(List<ContractData> contracts, int count, int maxAvailableContracts, int currentDay)
        {
            int availableCount = contracts.Count(c => c.state == ContractState.Available);
            int toGenerate = Mathf.Min(count, maxAvailableContracts - availableCount);

            for (int i = 0; i < toGenerate; i++)
            {
                var contract = GenerateSingleContract(currentDay);
                if (contract != null)
                {
                    contracts.Add(contract);
                }
            }

            if (toGenerate > 0 && _showDebugLogs)
            {
                Debug.Log($"Generated {toGenerate} new contract(s)");
            }
        }

        public ContractData GenerateSingleContract(int currentDay)
        {
            if (_templates.Count == 0)
            {
                Debug.LogWarning("No contract templates available");
                return null;
            }

            float reputation = _reputationSystem != null ? _reputationSystem.CurrentReputation : 0f;
            float maxReputation = _reputationSystem != null ? _reputationSystem.MaxReputation : 100f;
            float employeeMaxSkill = _reputationSystem != null ? _reputationSystem.GetEmployeeQualityMultiplier() : 90f;

            var template = SelectTemplateByReputation(reputation, maxReputation, employeeMaxSkill);
            if (template == null)
            {
                Debug.LogWarning("No suitable contract template found");
                return null;
            }

            string contractId = System.Guid.NewGuid().ToString();
            string clientName = GenerateClientName();

            var ctx = new ContractGenContext
            {
                playerReputation = reputation,
                maxReputation = maxReputation,
                employeeMaxSkill = employeeMaxSkill
            };

            return new ContractData(contractId, clientName, template, currentDay, ctx, _contractGenerator);
        }

        private ContractTemplateSO SelectTemplateByReputation(float reputation, float maxReputation, float employeeMaxSkill)
        {
            var availableTemplates = new List<ContractTemplateSO>();
            var weights = new List<float>();

            foreach (var template in _templates)
            {
                float weight = CalculateTemplateWeight(template, reputation, maxReputation, employeeMaxSkill);
                if (weight > 0f)
                {
                    availableTemplates.Add(template);
                    weights.Add(weight);
                }
            }

            if (availableTemplates.Count == 0)
            {
                return SelectClosestTemplate(reputation, maxReputation, employeeMaxSkill);
            }

            float totalWeight = weights.Sum();
            float randomValue = _rng.Range(0f, totalWeight);

            float cumulative = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                cumulative += weights[i];
                if (randomValue <= cumulative)
                {
                    return availableTemplates[i];
                }
            }
            
            return availableTemplates[availableTemplates.Count - 1];
        }

        private ContractTemplateSO SelectClosestTemplate(float reputation, float maxReputation, float employeeMaxSkill)
        {
            if (_templates.Count == 0) return null;
            
            float rep01 = maxReputation > 0f ? reputation / maxReputation : 0f;
            float targetSkill = rep01 * employeeMaxSkill;
            
            ContractTemplateSO closest = _templates[0];
            float smallestDiff = float.MaxValue;
            
            foreach (var template in _templates)
            {
                float avgSkillRequired = (template.devSkillMin + template.devSkillMax +
                                         template.designSkillMin + template.designSkillMax +
                                         template.marketingSkillMin + template.marketingSkillMax) / 6f;
                
                float diff = Mathf.Abs(avgSkillRequired - targetSkill);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    closest = template;
                }
            }
            
            return closest;
        }

        private float CalculateTemplateWeight(ContractTemplateSO template, float reputation, float maxReputation, float employeeMaxSkill)
        {
            float avgSkillRequired = (template.devSkillMin + template.devSkillMax +
                                     template.designSkillMin + template.designSkillMax +
                                     template.marketingSkillMin + template.marketingSkillMax) / 6f;

            float rep01 = maxReputation > 0f ? reputation / maxReputation : 0f;
            float targetSkill = rep01 * employeeMaxSkill;
            float skillDifference = Mathf.Abs(avgSkillRequired - targetSkill);

            // Gaussian-like falloff for smoother weight distribution
            // sigma = 25 means 68% of weight is within ±25 skill points
            float sigma = 25f;
            float weight = Mathf.Exp(-(skillDifference * skillDifference) / (2f * sigma * sigma));
            
            return weight;
        }

        private string GenerateClientName()
        {
            string[] prefixes = { "Tech", "Cyber", "Digital", "Cloud", "Smart", "Net", "Data", "Web", "Info", "Soft" };
            string[] suffixes = { "Corp", "Systems", "Solutions", "Industries", "Group", "Dynamics", "Innovations", "Partners", "Labs", "Tech" };

            string prefix = prefixes[_rng.Range(0, prefixes.Length)];
            string suffix = suffixes[_rng.Range(0, suffixes.Length)];

            return $"{prefix}{suffix}";
        }
    }
}
