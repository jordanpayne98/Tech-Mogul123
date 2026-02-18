using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Data;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class RivalContractOfferGenerator
    {
        private readonly List<ContractTemplateSO> _templates;
        private readonly ContractBalanceConfigSO _balanceConfig;
        private readonly ContractNamingSystem _namingSystem;
        private readonly IRng _rng;
        private readonly bool _showDebugLogs;
        
        private Dictionary<string, int> _issuerCategoryCooldowns = new Dictionary<string, int>();
        
        public RivalContractOfferGenerator(
            List<ContractTemplateSO> templates,
            ContractBalanceConfigSO balanceConfig,
            ContractNamingSystem namingSystem,
            IRng rng,
            bool showDebugLogs)
        {
            _templates = templates;
            _balanceConfig = balanceConfig;
            _namingSystem = namingSystem;
            _rng = rng;
            _showDebugLogs = showDebugLogs;
        }
        
        public List<ContractData> GenerateRivalContracts(
            int count,
            int currentDay,
            RivalSystem rivalSystem,
            EraSystem eraSystem,
            TechnologySystem techSystem,
            MarketSystem marketSystem)
        {
            var contracts = new List<ContractData>();
            
            if (rivalSystem == null || rivalSystem.AllCompanies.Count == 0)
            {
                return contracts;
            }
            
            for (int i = 0; i < count; i++)
            {
                var contract = GenerateSingleRivalContract(
                    currentDay, 
                    rivalSystem, 
                    eraSystem, 
                    techSystem,
                    marketSystem);
                    
                if (contract != null)
                {
                    contracts.Add(contract);
                }
            }
            
            return contracts;
        }
        
        ContractData GenerateSingleRivalContract(
            int currentDay,
            RivalSystem rivalSystem,
            EraSystem eraSystem,
            TechnologySystem techSystem,
            MarketSystem marketSystem)
        {
            var eligibleIssuers = GetEligibleIssuers(rivalSystem);
            if (eligibleIssuers.Count == 0)
            {
                if (_showDebugLogs)
                {
                    Debug.Log("[RivalContracts] No eligible issuers available");
                }
                return null;
            }
            
            var issuer = eligibleIssuers[_rng.Range(0, eligibleIssuers.Count)];
            
            var eligibleCategories = GetEligibleCategoriesForIssuer(issuer.CompanyId, rivalSystem);
            if (eligibleCategories.Count == 0)
            {
                return null;
            }
            
            string categoryId = eligibleCategories[_rng.Range(0, eligibleCategories.Count)];
            
            float marketShare = GetMarketShareForCategory(issuer.CompanyId, categoryId, rivalSystem);
            bool isDominant = marketShare >= _balanceConfig.dominanceShareThreshold;
            
            ContractType contractType = SelectContractType(isDominant);
            
            float techWeight = CalculateTechWeight(categoryId, techSystem, marketSystem);
            float eraWeight = eraSystem != null ? eraSystem.GetCurrentMarketSizeMultiplier() : 1f;
            
            float basePayout = CalculateBasePayout(contractType, techWeight, eraWeight, isDominant);
            
            int duration = CalculateDuration(contractType, isDominant);
            
            var contract = CreateContractData(
                currentDay,
                issuer,
                categoryId,
                contractType,
                basePayout,
                duration,
                isDominant,
                eraSystem);
                
            SetCooldown(issuer.CompanyId, categoryId);
            
            return contract;
        }
        
        List<RivalCompany> GetEligibleIssuers(RivalSystem rivalSystem)
        {
            var eligible = new List<RivalCompany>();
            
            foreach (var company in rivalSystem.AllCompanies)
            {
                if (company.CompanyId == "player") continue;
                if (company.Cash < 5000f) continue;
                
                eligible.Add(company);
            }
            
            return eligible;
        }
        
        List<string> GetEligibleCategoriesForIssuer(string issuerId, RivalSystem rivalSystem)
        {
            var eligible = new List<string>();
            
            var company = rivalSystem.AllCompanies.FirstOrDefault(c => c.CompanyId == issuerId);
            if (company == null) return eligible;
            
            foreach (var kvp in company.CategoryPositions)
            {
                string categoryId = kvp.Key;
                float share = kvp.Value.MarketShare;
                
                if (share > 0.01f && !IsOnCooldown(issuerId, categoryId))
                {
                    eligible.Add(categoryId);
                }
            }
            
            return eligible;
        }
        
        bool IsOnCooldown(string issuerId, string categoryId)
        {
            string key = $"{issuerId}_{categoryId}";
            return _issuerCategoryCooldowns.ContainsKey(key) && _issuerCategoryCooldowns[key] > 0;
        }
        
        void SetCooldown(string issuerId, string categoryId)
        {
            string key = $"{issuerId}_{categoryId}";
            _issuerCategoryCooldowns[key] = _balanceConfig.perIssuerPerCategoryOfferCooldownQuarters;
        }
        
        public void ProcessQuarterTickCooldowns()
        {
            var keys = _issuerCategoryCooldowns.Keys.ToList();
            foreach (var key in keys)
            {
                _issuerCategoryCooldowns[key]--;
                if (_issuerCategoryCooldowns[key] <= 0)
                {
                    _issuerCategoryCooldowns.Remove(key);
                }
            }
        }
        
        float GetMarketShareForCategory(string companyId, string categoryId, RivalSystem rivalSystem)
        {
            var company = rivalSystem.AllCompanies.FirstOrDefault(c => c.CompanyId == companyId);
            if (company == null) return 0f;
            
            return company.CategoryPositions.ContainsKey(categoryId) 
                ? company.CategoryPositions[categoryId].MarketShare 
                : 0f;
        }
        
        ContractType SelectContractType(bool isDominant)
        {
            float roll = _rng.Range(0f, 1f);
            
            if (isDominant)
            {
                return roll < 0.6f ? ContractType.MarketingCampaign : ContractType.OptimizationCost;
            }
            
            if (roll < 0.3f) return ContractType.ModuleDevelopment;
            if (roll < 0.5f) return ContractType.ComplianceStandards;
            if (roll < 0.65f) return ContractType.EcosystemIntegration;
            if (roll < 0.85f) return ContractType.MarketingCampaign;
            return ContractType.OptimizationCost;
        }
        
        float CalculateTechWeight(string categoryId, TechnologySystem techSystem, MarketSystem marketSystem)
        {
            if (techSystem == null || marketSystem == null) return 1.0f;
            
            TechMogul.Systems.TechAdoptionPhase phase = TechMogul.Systems.TechAdoptionPhase.Growth;
            
            float randomFactor = _rng.Range(0f, 1f);
            return _balanceConfig.GetTechAdoptionWeight(phase, randomFactor);
        }
        
        float CalculateBasePayout(ContractType type, float techWeight, float eraWeight, bool isDominant)
        {
            float basePayout = 5000f;
            
            basePayout *= techWeight;
            basePayout *= eraWeight;
            
            if (isDominant)
            {
                basePayout *= _balanceConfig.dominanceMagnitudeMultiplier;
            }
            
            return Mathf.Round(basePayout / 100f) * 100f;
        }
        
        int CalculateDuration(ContractType type, bool isDominant)
        {
            if (isDominant && _balanceConfig.forceDominanceDurationTo1Q)
            {
                return 1;
            }
            
            return type.IsPermanentEffect() ? 0 : _rng.Range(1, _balanceConfig.maxEffectDurationQuarters + 1);
        }
        
        ContractData CreateContractData(
            int currentDay,
            RivalCompany issuer,
            string categoryId,
            ContractType contractType,
            float basePayout,
            int duration,
            bool isDominant,
            EraSystem eraSystem)
        {
            if (_templates == null || _templates.Count == 0)
            {
                Debug.LogError("[RivalContracts] No contract templates available");
                return null;
            }
            
            var template = _templates[_rng.Range(0, _templates.Count)];
            
            string eraId = eraSystem != null && eraSystem.CurrentEra != null ? eraSystem.CurrentEra.id : "era.digital_age";
            
            int seed = currentDay + issuer.CompanyId.GetHashCode() + categoryId.GetHashCode();
            string contractName = _namingSystem != null 
                ? _namingSystem.GenerateContractName(contractType, eraId, categoryId, seed)
                : $"{contractType} - {categoryId}";
            
            var contract = new ContractData(
                System.Guid.NewGuid().ToString(),
                issuer.Name,
                template,
                currentDay)
            {
                issuingRivalId = issuer.CompanyId,
                targetCategoryId = categoryId,
                contractType = contractType,
                basePayout = basePayout,
                totalPayout = basePayout,
                daysRemaining = template.baseDeadlineDays,
                totalDays = template.baseDeadlineDays
            };
            
            if (contractType.IsTemporaryEffect())
            {
                contract.worldEffect = CreateWorldEffect(contract, duration, isDominant);
            }
            
            return contract;
        }
        
        ContractWorldEffect CreateWorldEffect(ContractData contract, int duration, bool isDominant)
        {
            WorldEffectComponent component = contract.contractType == ContractType.MarketingCampaign
                ? WorldEffectComponent.Marketing
                : WorldEffectComponent.Price;
            
            float magnitude = _rng.Range(0.03f, 0.06f);
            if (isDominant)
            {
                magnitude *= _balanceConfig.dominanceMagnitudeMultiplier;
            }
            
            magnitude = Mathf.Min(magnitude, _balanceConfig.maxEffectPerComponentPerCompanyCategory);
            
            return new ContractWorldEffect
            {
                effectId = System.Guid.NewGuid().ToString(),
                issuingRivalId = contract.issuingRivalId,
                targetCategoryId = contract.targetCategoryId,
                component = component,
                magnitude = magnitude,
                durationQuarters = duration,
                quartersRemaining = duration
            };
        }
    }
}
