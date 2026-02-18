using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Data;

namespace TechMogul.Systems
{
    public class RivalStrategyController
    {
        private const float CASH_RESERVE_CRITICAL = 0.2f;
        private const float SHARE_DECLINING_THRESHOLD = -2f;
        private const float SHARE_GROWING_THRESHOLD = 2f;
        private const float PROFIT_MARGIN_LOW = 0.1f;
        
        public void EvaluateQuarterlyStrategies(List<RivalCompany> companies, List<ProductCategorySO> categories)
        {
            foreach (var company in companies)
            {
                if (company.IsPlayerCompany)
                    continue;
                
                EvaluateStrategy(company, companies, categories);
                ExecuteStrategy(company, categories);
            }
        }
        
        void EvaluateStrategy(RivalCompany company, List<RivalCompany> allCompanies, List<ProductCategorySO> categories)
        {
            float totalRevenue = 0f;
            float avgShareTrend = 0f;
            int categoryCount = company.CategoryPositions.Count;
            
            foreach (var position in company.CategoryPositions.Values)
            {
                avgShareTrend += position.ShareTrend;
            }
            
            if (categoryCount > 0)
            {
                avgShareTrend /= categoryCount;
            }
            
            totalRevenue = company.Revenue;
            float profitMargin = totalRevenue > 0 ? company.Profit / totalRevenue : 0;
            float cashReserveRatio = totalRevenue > 0 ? company.Cash / (totalRevenue * 0.25f) : 0;
            
            bool lowCash = cashReserveRatio < CASH_RESERVE_CRITICAL;
            bool decliningShare = avgShareTrend < SHARE_DECLINING_THRESHOLD;
            bool growingShare = avgShareTrend > SHARE_GROWING_THRESHOLD;
            bool lowProfit = profitMargin < PROFIT_MARGIN_LOW;
            
            if (lowCash)
            {
                company.StrategyState = RivalStrategicState.CashRecovery;
            }
            else if (decliningShare || lowProfit)
            {
                company.StrategyState = RivalStrategicState.Defensive;
            }
            else if (growingShare && !lowCash && categoryCount < 3)
            {
                company.StrategyState = RivalStrategicState.AggressiveExpansion;
            }
            else
            {
                company.StrategyState = RivalStrategicState.Stable;
            }
        }
        
        void ExecuteStrategy(RivalCompany company, List<ProductCategorySO> categories)
        {
            switch (company.StrategyState)
            {
                case RivalStrategicState.Defensive:
                    ExecuteDefensive(company);
                    break;
                    
                case RivalStrategicState.AggressiveExpansion:
                    ExecuteAggressiveExpansion(company, categories);
                    break;
                    
                case RivalStrategicState.CashRecovery:
                    ExecuteCashRecovery(company);
                    break;
                    
                case RivalStrategicState.Stable:
                    ExecuteStable(company);
                    break;
            }
        }
        
        void ExecuteDefensive(RivalCompany company)
        {
            foreach (var position in company.CategoryPositions.Values)
            {
                position.Marketing = Mathf.Min(100f, position.Marketing + 5f);
                position.Price = Mathf.Max(80f, position.Price - 2f);
            }
        }
        
        void ExecuteAggressiveExpansion(RivalCompany company, List<ProductCategorySO> categories)
        {
            foreach (var position in company.CategoryPositions.Values)
            {
                position.Quality = Mathf.Min(100f, position.Quality + 3f);
            }
            
            if (company.CategoryPositions.Count < 3)
            {
                TryEnterNewCategory(company, categories);
            }
        }
        
        void ExecuteCashRecovery(RivalCompany company)
        {
            foreach (var position in company.CategoryPositions.Values)
            {
                position.Marketing = Mathf.Max(20f, position.Marketing - 5f);
                position.Price = Mathf.Min(120f, position.Price + 3f);
            }
        }
        
        void ExecuteStable(RivalCompany company)
        {
            foreach (var position in company.CategoryPositions.Values)
            {
                position.Quality = Mathf.Min(100f, position.Quality + 1f);
                position.Reputation = Mathf.Min(100f, position.Reputation + 0.5f);
            }
        }
        
        void TryEnterNewCategory(RivalCompany company, List<ProductCategorySO> categories)
        {
            var availableCategories = categories
                .Where(c => !company.CategoryPositions.ContainsKey(c.id))
                .ToList();
            
            if (availableCategories.Count == 0)
                return;
            
            var targetCategory = availableCategories[Random.Range(0, availableCategories.Count)];
            
            float entryCost = targetCategory.entryCost;
            if (company.Cash >= entryCost * 1.5f)
            {
                company.Cash -= entryCost;
                company.AddCategory(targetCategory, 0f);
                
                Debug.Log($"{company.Name} entering {targetCategory.categoryName} market");
            }
        }
    }
}
