using System;
using System.Collections.Generic;
using TechMogul.Data;

namespace TechMogul.Systems
{
    [Serializable]
    public class RivalCompany
    {
        public string CompanyId;
        public string Name;
        public bool IsPlayerCompany;
        public RivalType Type;
        
        public float Cash;
        public float Revenue;
        public float Profit;
        public float RevenueTrend;
        
        public Dictionary<string, CategoryPosition> CategoryPositions = new Dictionary<string, CategoryPosition>();
        
        public EfficiencyModifiers Efficiency;
        
        public RivalStrategicState StrategyState = RivalStrategicState.Stable;
        
        public int QuartersSurvived;
        
        public RivalCompany(string id, string name, bool isPlayer = false, RivalType type = RivalType.MajorRival)
        {
            CompanyId = id;
            Name = name;
            IsPlayerCompany = isPlayer;
            Type = type;
            Efficiency = new EfficiencyModifiers();
            Efficiency.Randomize();
            QuartersSurvived = 0;
        }
        
        public void AddCategory(ProductCategorySO category, float initialShare = 0f)
        {
            if (!CategoryPositions.ContainsKey(category.id))
            {
                CategoryPositions[category.id] = new CategoryPosition
                {
                    CategoryId = category.id,
                    MarketShare = initialShare,
                    MarketPower = 50f,
                    Quality = 50f,
                    Marketing = 50f,
                    Reputation = 50f,
                    Price = 100f,
                    StandardAlignment = 50f,
                    EcosystemStrength = 50f
                };
            }
        }
    }
    
    [Serializable]
    public class CategoryPosition
    {
        public string CategoryId;
        public float MarketShare;
        public float MarketPower;
        
        public float Quality;
        public float Marketing;
        public float Reputation;
        public float Price;
        public float StandardAlignment;
        public float EcosystemStrength;
        
        public float Momentum = 1f;
        public float ShareTrend;
        public float RevenueTrend;
    }
    
    [Serializable]
    public class EfficiencyModifiers
    {
        public float MarketingEfficiency = 1f;
        public float RdEfficiency = 1f;
        public float CostEfficiency = 1f;
        public float PricingDiscipline = 1f;
        public float ExpansionEfficiency = 1f;
        
        public void Randomize()
        {
            MarketingEfficiency = UnityEngine.Random.Range(0.85f, 1.15f);
            RdEfficiency = UnityEngine.Random.Range(0.85f, 1.15f);
            CostEfficiency = UnityEngine.Random.Range(0.85f, 1.15f);
            PricingDiscipline = UnityEngine.Random.Range(0.85f, 1.15f);
            ExpansionEfficiency = UnityEngine.Random.Range(0.85f, 1.15f);
        }
    }
    
    public enum RivalType
    {
        MajorRival,
        Startup
    }
    
    public enum RivalStrategicState
    {
        Stable,
        Defensive,
        AggressiveExpansion,
        CashRecovery
    }
}
