using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Data;
using TechMogul.Contracts;

namespace TechMogul.Systems
{
    public class RivalSystem : GameSystem
    {
        public static RivalSystem Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private List<ProductCategorySO> productCategories = new List<ProductCategorySO>();
        [SerializeField] private int initialMajorRivalsMin = 4;
        [SerializeField] private int initialMajorRivalsMax = 6;
        
        private List<RivalCompany> _companies = new List<RivalCompany>();
        private RivalCompany _playerCompany;
        
        private MarketCompetitionEngine _competitionEngine;
        private RivalStrategyController _strategyController;
        private CategoryLifecycleSystem _lifecycleSystem;
        private StartupManager _startupManager;
        
        private int _monthsElapsed = 0;
        
        public IReadOnlyList<RivalCompany> AllCompanies => _companies.AsReadOnly();
        public RivalCompany PlayerCompany => _playerCompany;
        public IReadOnlyList<ProductCategorySO> Categories => productCategories.AsReadOnly();
        
        protected override void Awake()
        {
            base.Awake();
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            _competitionEngine = new MarketCompetitionEngine();
            _strategyController = new RivalStrategyController();
            _lifecycleSystem = new CategoryLifecycleSystem();
            _startupManager = new StartupManager();
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
            Subscribe<OnMonthTickEvent>(HandleMonthTick);
            Subscribe<OnQuarterTickEvent>(HandleQuarterTick);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            InitializeCompetition();
        }
        
        void HandleMonthTick(OnMonthTickEvent evt)
        {
            _monthsElapsed++;
            ProcessMonthlyCompetition();
        }
        
        void HandleQuarterTick(OnQuarterTickEvent evt)
        {
            ProcessQuarterlyUpdates();
        }
        
        void InitializeCompetition()
        {
            _companies.Clear();
            _monthsElapsed = 0;
            
            CreatePlayerCompany();
            CreateMajorRivals();
            
            EventBus.Publish(new OnRivalsInitializedEvent(_companies.Count));
            
            Debug.Log($"Rival competition initialized with {_companies.Count} companies across {productCategories.Count} categories");
        }
        
        void CreatePlayerCompany()
        {
            _playerCompany = new RivalCompany("player", "Your Company", true, RivalType.MajorRival)
            {
                Cash = GameManager.Instance.CurrentCash,
                Revenue = 0f,
                Profit = 0f
            };
            
            _companies.Add(_playerCompany);
        }
        
        void CreateMajorRivals()
        {
            int rivalCount = Random.Range(initialMajorRivalsMin, initialMajorRivalsMax + 1);
            
            string[] rivalNames = {
                "TechVision Inc", "InnovateX", "CloudServe Solutions", "DataCore Systems",
                "PixelForge Studios", "ByteDynamics", "NexGenSoft", "Quantum Computing Corp"
            };
            
            for (int i = 0; i < rivalCount; i++)
            {
                string rivalId = $"rival_{i}";
                string rivalName = rivalNames[i % rivalNames.Length];
                
                var rival = new RivalCompany(rivalId, rivalName, false, RivalType.MajorRival)
                {
                    Cash = Random.Range(5000000f, 20000000f),
                    Revenue = Random.Range(1000000f, 10000000f),
                    Profit = Random.Range(100000f, 2000000f)
                };
                
                int categoryCount = Random.Range(1, 3);
                var selectedCategories = productCategories.OrderBy(x => Random.value).Take(categoryCount).ToList();
                
                foreach (var category in selectedCategories)
                {
                    rival.AddCategory(category, Random.Range(5f, 20f));
                    
                    var position = rival.CategoryPositions[category.id];
                    position.Quality = Random.Range(40f, 80f);
                    position.Marketing = Random.Range(40f, 80f);
                    position.Reputation = Random.Range(30f, 70f);
                    position.Price = Random.Range(90f, 110f);
                    position.StandardAlignment = Random.Range(40f, 60f);
                    position.EcosystemStrength = Random.Range(30f, 60f);
                }
                
                _companies.Add(rival);
            }
            
            foreach (var category in productCategories)
            {
                NormalizeInitialShares(category);
            }
        }
        
        void NormalizeInitialShares(ProductCategorySO category)
        {
            var competitorsInCategory = _companies
                .Where(c => c.CategoryPositions.ContainsKey(category.id))
                .ToList();
            
            if (competitorsInCategory.Count == 0)
                return;
            
            float total = competitorsInCategory.Sum(c => c.CategoryPositions[category.id].MarketShare);
            
            if (total > 0)
            {
                foreach (var company in competitorsInCategory)
                {
                    var position = company.CategoryPositions[category.id];
                    position.MarketShare = (position.MarketShare / total) * 100f;
                }
            }
        }
        
        void ProcessMonthlyCompetition()
        {
            _lifecycleSystem.ProcessMonthlyLifecycle(productCategories);
            
            ContractSystem contractSystem = ServiceLocator.Instance.Get<ContractSystem>();
            ContractWorldEffectsManager worldEffects = contractSystem != null ? contractSystem.WorldEffectsManager : null;
            
            foreach (var category in productCategories)
            {
                _competitionEngine.ProcessMonthlyCompetition(_companies, category, worldEffects);
            }
            
            CalculateRevenues();
            
            EventBus.Publish(new OnMarketSharesUpdatedEvent());
        }
        
        void ProcessQuarterlyUpdates()
        {
            foreach (var company in _companies)
            {
                company.QuartersSurvived++;
            }
            
            _strategyController.EvaluateQuarterlyStrategies(_companies, productCategories);
            _startupManager.ProcessQuarterlyStartups(_companies, productCategories);
            
            EventBus.Publish(new OnQuarterlyReportEvent());
        }
        
        void CalculateRevenues()
        {
            foreach (var company in _companies)
            {
                float totalRevenue = 0f;
                
                foreach (var kvp in company.CategoryPositions)
                {
                    var category = productCategories.Find(c => c.id == kvp.Key);
                    if (category != null)
                    {
                        var position = kvp.Value;
                        float categoryRevenue = (position.MarketShare / 100f) * category.baseMarketSize * category.baseGrowthRate;
                        totalRevenue += categoryRevenue;
                    }
                }
                
                company.RevenueTrend = totalRevenue - company.Revenue;
                company.Revenue = totalRevenue;
                
                float costs = totalRevenue * 0.7f;
                company.Profit = totalRevenue - costs;
                company.Cash += company.Profit;
            }
            
            if (_playerCompany != null)
            {
                _playerCompany.Cash = GameManager.Instance.CurrentCash;
            }
        }
        
        public RivalCompany GetCompanyById(string companyId)
        {
            return _companies.Find(c => c.CompanyId == companyId);
        }
        
        public float GetCategoryTotalMarketSize(string categoryId)
        {
            var category = productCategories.Find(c => c.id == categoryId);
            return category != null ? category.baseMarketSize : 0f;
        }
        
        public bool PlayerCanEnterCategory(ProductCategorySO category)
        {
            if (_playerCompany == null)
                return false;
                
            if (_playerCompany.CategoryPositions.ContainsKey(category.id))
                return false;
                
            if (_playerCompany.CategoryPositions.Count >= 3)
                return false;
                
            return _playerCompany.Cash >= category.entryCost;
        }
        
        public void PlayerEnterCategory(ProductCategorySO category)
        {
            if (_playerCompany == null || !PlayerCanEnterCategory(category))
            {
                Debug.LogWarning($"Player cannot enter {category.categoryName}");
                return;
            }
            
            _playerCompany.Cash -= category.entryCost;
            _playerCompany.AddCategory(category, 0f);
            
            EventBus.Publish(new RequestDeductCashEvent { Amount = category.entryCost });
            EventBus.Publish(new OnCompanyEnteredMarketEvent
            {
                CompanyId = _playerCompany.CompanyId,
                CompanyName = _playerCompany.Name,
                CategoryId = category.id
            });
            
            Debug.Log($"Player entered {category.categoryName} market for ${category.entryCost:N0}");
        }
        
        public void UpdatePlayerCategoryStats(string categoryId, float quality, float marketing, float reputation, float price)
        {
            if (_playerCompany == null || !_playerCompany.CategoryPositions.ContainsKey(categoryId))
                return;
                
            var position = _playerCompany.CategoryPositions[categoryId];
            position.Quality = Mathf.Clamp(quality, 0f, 100f);
            position.Marketing = Mathf.Clamp(marketing, 0f, 100f);
            position.Reputation = Mathf.Clamp(reputation, 0f, 100f);
            position.Price = Mathf.Clamp(price, 50f, 150f);
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Log Market State")]
        void DebugLogMarketState()
        {
            Debug.Log("=== MARKET STATE ===");
            foreach (var category in productCategories)
            {
                Debug.Log($"\n{category.categoryName} ({category.currentStage}):");
                
                var competitors = _companies.Where(c => c.CategoryPositions.ContainsKey(category.id)).ToList();
                foreach (var company in competitors.OrderByDescending(c => c.CategoryPositions[category.id].MarketShare))
                {
                    var position = company.CategoryPositions[category.id];
                    Debug.Log($"  {company.Name}: {position.MarketShare:F1}% (Power: {position.MarketPower:F1})");
                }
            }
        }
        #endif
    }
}
