using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Data;

namespace TechMogul.Systems
{
    public class StartupManager
    {
        private int _startupIdCounter = 1000;
        private const float STARTUP_SPAWN_CHANCE = 0.15f;
        private const float PROMOTION_REVENUE_THRESHOLD = 5000000f;
        private const int PROMOTION_QUARTERS_REQUIRED = 4;
        private const float FAILURE_CASH_THRESHOLD = 100000f;
        private const float FAILURE_SHARE_THRESHOLD = 0.5f;
        
        public void ProcessQuarterlyStartups(List<RivalCompany> companies, List<ProductCategorySO> categories)
        {
            TrySpawnStartups(companies, categories);
            EvaluatePromotions(companies);
            EvaluateFailures(companies);
        }
        
        void TrySpawnStartups(List<RivalCompany> companies, List<ProductCategorySO> categories)
        {
            foreach (var category in categories)
            {
                int competitorCount = companies.Count(c => c.CategoryPositions.ContainsKey(category.id));
                
                if (competitorCount < 2)
                    continue;
                
                float spawnChance = STARTUP_SPAWN_CHANCE;
                
                if (category.currentStage == CategoryLifecycleStage.Growth)
                    spawnChance *= 1.5f;
                else if (category.currentStage == CategoryLifecycleStage.Saturation)
                    spawnChance *= 0.5f;
                
                if (competitorCount < 4)
                    spawnChance *= 1.3f;
                else if (competitorCount > 6)
                    spawnChance *= 0.7f;
                
                if (Random.value < spawnChance)
                {
                    SpawnStartup(companies, category);
                }
            }
        }
        
        void SpawnStartup(List<RivalCompany> companies, ProductCategorySO category)
        {
            string startupId = $"startup_{_startupIdCounter++}";
            string startupName = GenerateStartupName();
            
            var startup = new RivalCompany(startupId, startupName, false, RivalType.Startup)
            {
                Cash = Random.Range(500000f, 2000000f),
                Revenue = 0f,
                Profit = 0f,
                QuartersSurvived = 0
            };
            
            startup.AddCategory(category, 0.5f);
            
            var position = startup.CategoryPositions[category.id];
            position.Quality = Random.Range(40f, 70f);
            position.Marketing = Random.Range(30f, 60f);
            position.Reputation = Random.Range(20f, 50f);
            position.Price = Random.Range(90f, 110f);
            
            companies.Add(startup);
            
            Debug.Log($"Startup '{startupName}' entered {category.categoryName} market");
        }
        
        void EvaluatePromotions(List<RivalCompany> companies)
        {
            var startups = companies.Where(c => c.Type == RivalType.Startup).ToList();
            
            foreach (var startup in startups)
            {
                if (startup.Revenue >= PROMOTION_REVENUE_THRESHOLD && 
                    startup.QuartersSurvived >= PROMOTION_QUARTERS_REQUIRED)
                {
                    startup.Type = RivalType.MajorRival;
                    Debug.Log($"{startup.Name} promoted to Major Rival");
                }
            }
        }
        
        void EvaluateFailures(List<RivalCompany> companies)
        {
            var startups = companies.Where(c => c.Type == RivalType.Startup).ToList();
            
            List<RivalCompany> toRemove = new List<RivalCompany>();
            
            foreach (var startup in startups)
            {
                bool cashFailed = startup.Cash < FAILURE_CASH_THRESHOLD;
                
                float avgShare = 0f;
                if (startup.CategoryPositions.Count > 0)
                {
                    avgShare = startup.CategoryPositions.Values.Average(p => p.MarketShare);
                }
                
                bool shareFailed = avgShare < FAILURE_SHARE_THRESHOLD && startup.QuartersSurvived > 4;
                
                if (cashFailed || shareFailed)
                {
                    toRemove.Add(startup);
                    Debug.Log($"Startup '{startup.Name}' failed and exited market");
                }
            }
            
            foreach (var failed in toRemove)
            {
                companies.Remove(failed);
            }
        }
        
        string GenerateStartupName()
        {
            string[] prefixes = { "Tech", "Inno", "Digital", "Cloud", "Smart", "Next", "Byte", "Pixel", "Data", "Cyber" };
            string[] suffixes = { "Labs", "Dynamics", "Solutions", "Systems", "Ventures", "Works", "Tech", "Hub", "Core", "Flow" };
            
            string prefix = prefixes[Random.Range(0, prefixes.Length)];
            string suffix = suffixes[Random.Range(0, suffixes.Length)];
            
            return $"{prefix}{suffix}";
        }
    }
}
