using System.Collections.Generic;
using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Systems
{
    public class MarketSystem : GameSystem
    {
        [Header("Configuration")]
        [SerializeField] private MarketDemandConfigSO demandConfig;
        [SerializeField] private ExpectationPenaltyConfigSO expectationConfig;
        [SerializeField] private EarlyTechRiskRewardConfigSO riskRewardConfig;
        
        [Header("Market Categories")]
        [SerializeField] private MarketCategorySO[] marketCategories;
        
        private TechnologySystem _techSystem;
        private EraSystem _eraSystem;
        
        private Dictionary<string, float> _categoryDemandMultipliers = new Dictionary<string, float>();
        
        public MarketDemandConfigSO DemandConfig => demandConfig;
        public ExpectationPenaltyConfigSO ExpectationConfig => expectationConfig;
        public EarlyTechRiskRewardConfigSO RiskRewardConfig => riskRewardConfig;
        
        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.Instance.TryRegister<MarketSystem>(this);
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnTechnologiesUpdatedEvent>(HandleTechnologiesUpdated);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            _techSystem = ServiceLocator.Instance.Get<TechnologySystem>();
            _eraSystem = ServiceLocator.Instance.Get<EraSystem>();
            
            UpdateAllCategoryDemand();
        }
        
        void HandleTechnologiesUpdated(OnTechnologiesUpdatedEvent evt)
        {
            UpdateAllCategoryDemand();
        }
        
        void UpdateAllCategoryDemand()
        {
            if (marketCategories == null || marketCategories.Length == 0)
            {
                return;
            }
            
            if (_techSystem == null)
            {
                _techSystem = ServiceLocator.Instance.Get<TechnologySystem>();
            }
            
            if (_eraSystem == null)
            {
                _eraSystem = ServiceLocator.Instance.Get<EraSystem>();
            }
            
            foreach (MarketCategorySO category in marketCategories)
            {
                if (category == null)
                {
                    continue;
                }
                
                float demandMultiplier = CalculateCategoryDemand(category);
                _categoryDemandMultipliers[category.id] = demandMultiplier;
            }
        }
        
        float CalculateCategoryDemand(MarketCategorySO category)
        {
            if (_techSystem == null || _eraSystem == null || demandConfig == null)
            {
                return category.baseDemand;
            }
            
            float averageAdoption = CalculateAverageTechAdoption(category);
            
            float adoptionDemandFactor = demandConfig.CalculateAdoptionDemandFactor(averageAdoption);
            
            float eraMultiplier = _eraSystem.GetCurrentMarketSizeMultiplier();
            
            float marketDemandMultiplier = Mathf.Max(
                demandConfig.demandFloor,
                eraMultiplier * adoptionDemandFactor
            );
            
            return category.baseDemand * marketDemandMultiplier;
        }
        
        float CalculateAverageTechAdoption(MarketCategorySO category)
        {
            if (category.linkedTechnologies == null || category.linkedTechnologies.Length == 0)
            {
                return 0f;
            }
            
            float totalAdoption = 0f;
            int validCount = 0;
            
            foreach (TechnologySO tech in category.linkedTechnologies)
            {
                if (tech == null)
                {
                    continue;
                }
                
                float adoption = _techSystem.GetAdoptionRate(tech.id);
                totalAdoption += adoption;
                validCount++;
            }
            
            return validCount > 0 ? totalAdoption / validCount : 0f;
        }
        
        public float GetCategoryDemandMultiplier(string categoryId)
        {
            return _categoryDemandMultipliers.ContainsKey(categoryId) 
                ? _categoryDemandMultipliers[categoryId] 
                : 1f;
        }
        
        public string GetMarketDemandTooltip(string categoryId)
        {
            if (_techSystem == null || _eraSystem == null)
            {
                return "Market data unavailable";
            }
            
            MarketCategorySO category = GetCategory(categoryId);
            if (category == null)
            {
                return "Unknown category";
            }
            
            float avgAdoption = CalculateAverageTechAdoption(category);
            float demandMultiplier = GetCategoryDemandMultiplier(categoryId);
            float eraMultiplier = _eraSystem.GetCurrentMarketSizeMultiplier();
            
            return $"Market Demand: {demandMultiplier:F2}x\n" +
                   $"Era Multiplier: {eraMultiplier:F2}x\n" +
                   $"Tech Adoption: {avgAdoption:P0}";
        }
        
        MarketCategorySO GetCategory(string categoryId)
        {
            if (marketCategories == null)
            {
                return null;
            }
            
            foreach (MarketCategorySO cat in marketCategories)
            {
                if (cat != null && cat.id == categoryId)
                {
                    return cat;
                }
            }
            
            return null;
        }
        
        public float CalculateExpectationPenalty(List<TechnologySO> missingTechs)
        {
            if (expectationConfig == null || missingTechs == null || missingTechs.Count == 0)
            {
                return 0f;
            }
            
            float totalPenalty = 0f;
            int penaltyCount = 0;
            
            foreach (TechnologySO tech in missingTechs)
            {
                if (tech == null)
                {
                    continue;
                }
                
                float adoption = _techSystem.GetAdoptionRate(tech.id);
                float penalty = 0f;
                
                if (adoption >= expectationConfig.mandatoryThreshold)
                {
                    penalty = expectationConfig.mandatoryMissingPenalty;
                }
                else if (adoption >= expectationConfig.expectedThreshold)
                {
                    penalty = expectationConfig.expectedMissingPenalty;
                }
                
                if (penalty > 0f)
                {
                    float stackingMultiplier = penaltyCount switch
                    {
                        0 => 1.0f,
                        1 => expectationConfig.stackingSecondMultiplier,
                        _ => expectationConfig.stackingThirdPlusMultiplier
                    };
                    
                    totalPenalty += penalty * stackingMultiplier;
                    penaltyCount++;
                }
            }
            
            return Mathf.Min(totalPenalty, expectationConfig.totalPenaltyCap);
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Print Category Demand")]
        void DebugPrintCategoryDemand()
        {
            Debug.Log("=== Market Category Demand ===");
            foreach (MarketCategorySO category in marketCategories)
            {
                if (category == null) continue;
                
                float demand = GetCategoryDemandMultiplier(category.id);
                Debug.Log($"{category.categoryName}: {demand:F2}x");
            }
        }
        #endif
    }
}
