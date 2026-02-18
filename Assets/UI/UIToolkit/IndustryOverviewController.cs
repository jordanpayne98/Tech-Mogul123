using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Systems;
using TechMogul.Data;

namespace TechMogul.UI
{
    public class IndustryOverviewController : MonoBehaviour
    {
        private ScrollView categoryList;
        private ScrollView companyRankings;
        
        private IEventBus _eventBus;
        private readonly List<System.IDisposable> _subs = new List<System.IDisposable>();
        
        void Awake()
        {
            if (ServiceLocator.Instance.TryGet<IEventBus>(out IEventBus eventBus))
            {
                _eventBus = eventBus;
            }
        }
        
        void OnEnable()
        {
            if (_eventBus != null)
            {
                _subs.Add(_eventBus.Subscribe<OnMarketSharesUpdatedEvent>(evt => RefreshDisplay()));
                _subs.Add(_eventBus.Subscribe<OnQuarterlyReportEvent>(evt => RefreshDisplay()));
                _subs.Add(_eventBus.Subscribe<OnRivalsInitializedEvent>(evt => RefreshDisplay()));
            }
        }
        
        void Start()
        {
            InitializeUIReferences();
            RefreshDisplay();
        }
        
        void InitializeUIReferences()
        {
            // Find the MainUI root and query from there
            var mainUIGO = GameObject.Find("UIManager");
            if (mainUIGO == null)
            {
                Debug.LogError("IndustryOverviewController: UIManager GameObject not found!");
                return;
            }
            
            var mainUIDoc = mainUIGO.GetComponent<UIDocument>();
            if (mainUIDoc == null)
            {
                Debug.LogError("IndustryOverviewController: UIDocument component not found on UIManager!");
                return;
            }
            
            var root = mainUIDoc.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("IndustryOverviewController: rootVisualElement is null!");
                return;
            }
            
            categoryList = root.Q<ScrollView>("category-list");
            companyRankings = root.Q<ScrollView>("company-rankings");
            
            if (categoryList == null)
                Debug.LogWarning("IndustryOverviewController: category-list ScrollView not found!");
            if (companyRankings == null)
                Debug.LogWarning("IndustryOverviewController: company-rankings ScrollView not found!");
        }
        
        public void ForceRefresh()
        {
            if (categoryList == null || companyRankings == null)
            {
                InitializeUIReferences();
            }
            RefreshDisplay();
        }
        
        void OnDisable()
        {
            for (int i = 0; i < _subs.Count; i++)
            {
                _subs[i]?.Dispose();
            }
            _subs.Clear();
        }
        
        void RefreshDisplay()
        {
            Debug.Log("IndustryOverviewController: RefreshDisplay called");
            
            if (RivalSystem.Instance == null)
            {
                Debug.LogWarning("IndustryOverviewController: RivalSystem.Instance is null");
                return;
            }
                
            PopulateCategoryBreakdown();
            PopulateCompanyRankings();
        }
        
        void PopulateCategoryBreakdown()
        {
            if (categoryList == null)
            {
                Debug.LogWarning("IndustryOverviewController: categoryList is null, cannot populate");
                return;
            }
                
            categoryList.Clear();
            
            var categories = RivalSystem.Instance.Categories;
            Debug.Log($"IndustryOverviewController: Populating {categories.Count} categories");
            
            foreach (var category in categories)
            {
                var categoryCard = CreateCategoryCard(category);
                categoryList.Add(categoryCard);
            }
        }
        
        VisualElement CreateCategoryCard(ProductCategorySO category)
        {
            var card = new VisualElement();
            card.style.backgroundColor = new Color(0.15f, 0.15f, 0.18f);
            card.style.marginBottom = 12;
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;
            
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 8;
            
            var titleLabel = new Label(category.categoryName);
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.color = new Color(0.95f, 0.95f, 1f);
            
            var stageLabel = new Label($"{category.currentStage} ({category.baseGrowthRate * 100f:F1}% growth)");
            stageLabel.style.fontSize = 12;
            stageLabel.style.color = GetStageColor(category.currentStage);
            stageLabel.style.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            stageLabel.style.paddingTop = 4;
            stageLabel.style.paddingBottom = 4;
            stageLabel.style.paddingLeft = 8;
            stageLabel.style.paddingRight = 8;
            stageLabel.style.borderTopLeftRadius = 4;
            stageLabel.style.borderTopRightRadius = 4;
            stageLabel.style.borderBottomLeftRadius = 4;
            stageLabel.style.borderBottomRightRadius = 4;
            
            header.Add(titleLabel);
            header.Add(stageLabel);
            
            var marketInfo = new Label($"Market Size: ${category.baseMarketSize / 1000000f:F1}M | Elasticity: {category.elasticity:F2} | Entry Cost: ${category.entryCost / 1000000f:F1}M");
            marketInfo.style.fontSize = 11;
            marketInfo.style.color = new Color(0.6f, 0.6f, 0.65f);
            marketInfo.style.marginBottom = 10;
            
            var companies = RivalSystem.Instance.AllCompanies
                .Where(c => c.CategoryPositions.ContainsKey(category.id))
                .OrderByDescending(c => c.CategoryPositions[category.id].MarketShare)
                .ToList();
            
            var shareList = new VisualElement();
            
            foreach (var company in companies.Take(5))
            {
                var position = company.CategoryPositions[category.id];
                var companyRow = CreateCompanyShareRow(company, position);
                shareList.Add(companyRow);
            }
            
            card.Add(header);
            card.Add(marketInfo);
            card.Add(shareList);
            
            return card;
        }
        
        VisualElement CreateCompanyShareRow(RivalCompany company, CategoryPosition position)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 6;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.5f);
            row.style.borderTopLeftRadius = 4;
            row.style.borderTopRightRadius = 4;
            row.style.borderBottomLeftRadius = 4;
            row.style.borderBottomRightRadius = 4;
            
            var nameLabel = new Label(company.Name);
            nameLabel.style.fontSize = 14;
            nameLabel.style.color = company.IsPlayerCompany ? new Color(0.3f, 0.9f, 0.5f) : new Color(0.9f, 0.9f, 0.95f);
            nameLabel.style.flexGrow = 1;
            
            var trendIcon = position.ShareTrend > 0.1f ? "▲" : position.ShareTrend < -0.1f ? "▼" : "—";
            var trendColor = position.ShareTrend > 0.1f ? new Color(0.3f, 0.9f, 0.5f) : 
                            position.ShareTrend < -0.1f ? new Color(0.9f, 0.3f, 0.3f) : 
                            new Color(0.6f, 0.6f, 0.65f);
            
            var trendLabel = new Label(trendIcon);
            trendLabel.style.fontSize = 12;
            trendLabel.style.color = trendColor;
            trendLabel.style.marginRight = 8;
            
            var shareLabel = new Label($"{position.MarketShare:F1}%");
            shareLabel.style.fontSize = 14;
            shareLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            shareLabel.style.color = new Color(0.5f, 0.8f, 1f);
            shareLabel.style.minWidth = 60;
            shareLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            
            var powerLabel = new Label($"Power: {position.MarketPower:F0}");
            powerLabel.style.fontSize = 11;
            powerLabel.style.color = new Color(0.5f, 0.5f, 0.55f);
            powerLabel.style.minWidth = 80;
            powerLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            
            row.Add(nameLabel);
            row.Add(trendLabel);
            row.Add(shareLabel);
            row.Add(powerLabel);
            
            return row;
        }
        
        void PopulateCompanyRankings()
        {
            if (companyRankings == null)
                return;
                
            companyRankings.Clear();
            
            var companies = RivalSystem.Instance.AllCompanies
                .OrderByDescending(c => c.Revenue)
                .ToList();
            
            int rank = 1;
            foreach (var company in companies)
            {
                var rankCard = CreateCompanyRankCard(rank, company);
                companyRankings.Add(rankCard);
                rank++;
            }
        }
        
        VisualElement CreateCompanyRankCard(int rank, RivalCompany company)
        {
            var card = new VisualElement();
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Center;
            card.style.backgroundColor = new Color(0.15f, 0.15f, 0.18f);
            card.style.marginBottom = 8;
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;
            
            var rankLabel = new Label($"#{rank}");
            rankLabel.style.fontSize = 20;
            rankLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            rankLabel.style.color = rank <= 3 ? new Color(1f, 0.8f, 0.2f) : new Color(0.5f, 0.5f, 0.55f);
            rankLabel.style.minWidth = 50;
            
            var infoContainer = new VisualElement();
            infoContainer.style.flexGrow = 1;
            infoContainer.style.marginLeft = 10;
            
            var nameLabel = new Label(company.Name);
            nameLabel.style.fontSize = 16;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = company.IsPlayerCompany ? new Color(0.3f, 0.9f, 0.5f) : new Color(0.9f, 0.9f, 0.95f);
            
            var typeLabel = new Label($"{company.Type} | {company.StrategyState}");
            typeLabel.style.fontSize = 12;
            typeLabel.style.color = new Color(0.5f, 0.7f, 0.9f);
            typeLabel.style.marginTop = 2;
            
            infoContainer.Add(nameLabel);
            infoContainer.Add(typeLabel);
            
            var statsContainer = new VisualElement();
            statsContainer.style.alignItems = Align.FlexEnd;
            
            var revenueLabel = new Label($"${company.Revenue / 1000000f:F1}M");
            revenueLabel.style.fontSize = 18;
            revenueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            revenueLabel.style.color = new Color(0.3f, 0.9f, 0.5f);
            
            var cashLabel = new Label($"Cash: ${company.Cash / 1000000f:F1}M");
            cashLabel.style.fontSize = 12;
            cashLabel.style.color = new Color(0.6f, 0.6f, 0.65f);
            cashLabel.style.marginTop = 2;
            
            var profitLabel = new Label($"Profit: ${company.Profit / 1000000f:F1}M");
            profitLabel.style.fontSize = 12;
            profitLabel.style.color = company.Profit >= 0 ? new Color(0.3f, 0.9f, 0.5f) : new Color(0.9f, 0.3f, 0.3f);
            profitLabel.style.marginTop = 2;
            
            statsContainer.Add(revenueLabel);
            statsContainer.Add(cashLabel);
            statsContainer.Add(profitLabel);
            
            card.Add(rankLabel);
            card.Add(infoContainer);
            card.Add(statsContainer);
            
            return card;
        }
        
        Color GetStageColor(CategoryLifecycleStage stage)
        {
            return stage switch
            {
                CategoryLifecycleStage.Emerging => new Color(0.3f, 0.9f, 0.5f),
                CategoryLifecycleStage.Growth => new Color(0.5f, 0.8f, 1f),
                CategoryLifecycleStage.Maturity => new Color(1f, 0.8f, 0.2f),
                CategoryLifecycleStage.Saturation => new Color(1f, 0.6f, 0.2f),
                CategoryLifecycleStage.Decline => new Color(0.9f, 0.3f, 0.3f),
                _ => new Color(0.6f, 0.6f, 0.65f)
            };
        }
    }
}
