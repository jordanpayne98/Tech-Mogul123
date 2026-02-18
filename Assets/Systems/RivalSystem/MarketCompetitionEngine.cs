using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Data;
using TechMogul.Contracts;

namespace TechMogul.Systems
{
    public class MarketCompetitionEngine
    {
        private const float INERTIA_FACTOR = 0.70f;
        private const float SATURATION_FACTOR = 0.30f;
        
        private const float QUALITY_WEIGHT = 0.30f;
        private const float MARKETING_WEIGHT = 0.25f;
        private const float REPUTATION_WEIGHT = 0.20f;
        private const float PRICE_WEIGHT = 0.15f;
        private const float STANDARD_WEIGHT = 0.05f;
        private const float ECOSYSTEM_WEIGHT = 0.05f;
        
        public void ProcessMonthlyCompetition(
            List<RivalCompany> companies, 
            ProductCategorySO category,
            ContractWorldEffectsManager worldEffects = null)
        {
            var competitorsInCategory = companies
                .Where(c => c.CategoryPositions.ContainsKey(category.id))
                .ToList();
            
            if (competitorsInCategory.Count == 0)
            {
                return;
            }
            
            Dictionary<string, float> previousShares = new Dictionary<string, float>();
            Dictionary<string, float> marketPowers = new Dictionary<string, float>();
            
            foreach (var company in competitorsInCategory)
            {
                var position = company.CategoryPositions[category.id];
                previousShares[company.CompanyId] = position.MarketShare;
                
                float marketPower = CalculateMarketPower(position, company.CompanyId, category.id, worldEffects);
                marketPowers[company.CompanyId] = marketPower;
                position.MarketPower = marketPower;
            }
            
            Dictionary<string, float> adjustedPowers = new Dictionary<string, float>();
            foreach (var company in competitorsInCategory)
            {
                var position = company.CategoryPositions[category.id];
                float momentum = position.Momentum;
                
                float adjustedPower = (marketPowers[company.CompanyId] * momentum) 
                                    + (previousShares[company.CompanyId] * INERTIA_FACTOR);
                
                adjustedPowers[company.CompanyId] = adjustedPower;
            }
            
            float totalAdjustedPower = adjustedPowers.Values.Sum();
            Dictionary<string, float> rawShares = new Dictionary<string, float>();
            
            foreach (var company in competitorsInCategory)
            {
                float rawShare = (adjustedPowers[company.CompanyId] / totalAdjustedPower) * 100f;
                rawShares[company.CompanyId] = rawShare;
            }
            
            foreach (var company in competitorsInCategory)
            {
                var position = company.CategoryPositions[category.id];
                float previousShare = previousShares[company.CompanyId];
                float targetShare = rawShares[company.CompanyId];
                
                float elasticityAdjusted = Mathf.Lerp(previousShare, targetShare, category.elasticity);
                
                float maxChange = previousShare * SATURATION_FACTOR;
                float actualChange = Mathf.Clamp(elasticityAdjusted - previousShare, -maxChange, maxChange);
                
                float baseVolatility = category.baseVolatility * GetLifecycleVolatilityMultiplier(category.currentStage);
                float volatility = Random.Range(-baseVolatility, baseVolatility) * 100f;
                
                float newShare = previousShare + actualChange + volatility;
                newShare = Mathf.Max(0f, newShare);
                
                position.ShareTrend = newShare - previousShare;
            }
            
            NormalizeMarketShares(competitorsInCategory, category.id);
            
            foreach (var company in competitorsInCategory)
            {
                var position = company.CategoryPositions[category.id];
                position.Momentum = CalculateMomentum(position.ShareTrend);
            }
        }
        
        float CalculateMarketPower(
            CategoryPosition position, 
            string companyId, 
            string categoryId,
            ContractWorldEffectsManager worldEffects)
        {
            float quality = position.Quality;
            float marketing = position.Marketing;
            float reputation = position.Reputation;
            float price = position.Price;
            float standard = position.StandardAlignment;
            float ecosystem = position.EcosystemStrength;
            
            if (worldEffects != null)
            {
                var effects = worldEffects.GetAllComponentEffectsForRivalCategory(companyId, categoryId);
                
                foreach (var effect in effects)
                {
                    float bonus = effect.Value * 100f;
                    
                    switch (effect.Key)
                    {
                        case WorldEffectComponent.Quality:
                            quality = Mathf.Min(100f, quality + bonus);
                            break;
                        case WorldEffectComponent.Marketing:
                            marketing = Mathf.Min(100f, marketing + bonus);
                            break;
                        case WorldEffectComponent.Reputation:
                            reputation = Mathf.Min(100f, reputation + bonus);
                            break;
                        case WorldEffectComponent.Price:
                            price = Mathf.Min(100f, price + bonus);
                            break;
                        case WorldEffectComponent.Standard:
                            standard = Mathf.Min(100f, standard + bonus);
                            break;
                        case WorldEffectComponent.Ecosystem:
                            ecosystem = Mathf.Min(100f, ecosystem + bonus);
                            break;
                    }
                }
            }
            
            float marketPower = 
                (quality * QUALITY_WEIGHT) +
                (marketing * MARKETING_WEIGHT) +
                (reputation * REPUTATION_WEIGHT) +
                (price * PRICE_WEIGHT) +
                (standard * STANDARD_WEIGHT) +
                (ecosystem * ECOSYSTEM_WEIGHT);
            
            return marketPower;
        }
        
        float CalculateMomentum(float shareTrend)
        {
            if (shareTrend > 0.5f)
                return 1.1f;
            else if (shareTrend < -0.5f)
                return 0.9f;
            else
                return 1.0f;
        }
        
        float GetLifecycleVolatilityMultiplier(CategoryLifecycleStage stage)
        {
            return stage switch
            {
                CategoryLifecycleStage.Emerging => 2.0f,
                CategoryLifecycleStage.Growth => 1.5f,
                CategoryLifecycleStage.Maturity => 1.0f,
                CategoryLifecycleStage.Saturation => 0.8f,
                CategoryLifecycleStage.Decline => 1.2f,
                _ => 1.0f
            };
        }
        
        void NormalizeMarketShares(List<RivalCompany> competitors, string categoryId)
        {
            float total = 0f;
            foreach (var company in competitors)
            {
                total += company.CategoryPositions[categoryId].MarketShare;
            }
            
            if (total > 0f)
            {
                foreach (var company in competitors)
                {
                    var position = company.CategoryPositions[categoryId];
                    position.MarketShare = (position.MarketShare / total) * 100f;
                }
            }
        }
    }
}
