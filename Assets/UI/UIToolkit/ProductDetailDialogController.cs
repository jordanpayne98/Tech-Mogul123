using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Products;
using TechMogul.Systems;

namespace TechMogul.UI
{
    public class ProductDetailDialogController : UIController
    {
        [Header("References")]
        [SerializeField] private GameObject productFeaturesDialogGO;
        
        private VisualElement overlay;
        private ProductData currentProduct;
        private ProductSystem productSystem;
        private EmployeeSystem employeeSystem;
        private IDefinitionResolver definitionResolver;
        private ProductFeaturesDialogController featuresDialog;
        
        private Label productNameLabel;
        private Label categoryValue;
        private Label statusValue;
        private Label phaseValue;
        private Label progressValue;
        
        private Label qualityScore;
        private Label stabilityScore;
        private Label usabilityScore;
        private Label innovationScore;
        private Label bugCount;
        
        private ProgressBar qualityBar;
        private ProgressBar stabilityBar;
        private ProgressBar usabilityBar;
        private ProgressBar innovationBar;
        
        private ScrollView featuresList;
        private Label noFeaturesLabel;
        private Label qaTierValue;
        
        private ScrollView teamList;
        private Label noTeamLabel;
        
        private VisualElement revenueSection;
        private Label monthlyRevenue;
        private Label totalRevenue;
        private Label monthsActive;
        
        private Button closeBtn;
        private Button editFeaturesBtn;
        private Button advancePhaseBtn;

        protected override void SubscribeToEvents()
        {
            Subscribe<OnProductProgressUpdatedEvent>(HandleProductProgress);
            Subscribe<OnProductReleasedEvent>(HandleProductReleased);
            Subscribe<OnFeatureAddedToProductEvent>(HandleFeatureAdded);
            Subscribe<OnProductQATierChangedEvent>(HandleQATierChanged);
            Subscribe<OnProductPhaseAdvancedEvent>(HandlePhaseAdvanced);
        }

        protected override void Awake()
        {
            base.Awake();
            
            productSystem = FindFirstObjectByType<ProductSystem>();
            employeeSystem = FindFirstObjectByType<EmployeeSystem>();
            
            var serviceLocator = ServiceLocator.Instance;
            if (serviceLocator != null)
            {
                definitionResolver = serviceLocator.Get<IDefinitionResolver>();
            }
            
            if (productFeaturesDialogGO != null)
            {
                featuresDialog = productFeaturesDialogGO.GetComponent<ProductFeaturesDialogController>();
            }
        }
        
        void EnsureUIInitialized()
        {
            if (overlay != null) return;
            
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc == null || uiDoc.rootVisualElement == null)
            {
                Debug.LogError("ProductDetailDialog UIDocument not ready");
                return;
            }
            
            overlay = uiDoc.rootVisualElement.Q<VisualElement>("overlay");
            
            if (overlay != null)
                {
                    productNameLabel = overlay.Q<Label>("product-name");
                    categoryValue = overlay.Q<Label>("category-value");
                    statusValue = overlay.Q<Label>("status-value");
                    phaseValue = overlay.Q<Label>("phase-value");
                    progressValue = overlay.Q<Label>("progress-value");
                    
                    qualityScore = overlay.Q<Label>("quality-score");
                    stabilityScore = overlay.Q<Label>("stability-score");
                    usabilityScore = overlay.Q<Label>("usability-score");
                    innovationScore = overlay.Q<Label>("innovation-score");
                    bugCount = overlay.Q<Label>("bug-count");
                    
                    qualityBar = overlay.Q<ProgressBar>("quality-bar");
                    stabilityBar = overlay.Q<ProgressBar>("stability-bar");
                    usabilityBar = overlay.Q<ProgressBar>("usability-bar");
                    innovationBar = overlay.Q<ProgressBar>("innovation-bar");
                    
                    featuresList = overlay.Q<ScrollView>("features-list");
                    noFeaturesLabel = overlay.Q<Label>("no-features-label");
                    qaTierValue = overlay.Q<Label>("qa-tier-value");
                    
                    teamList = overlay.Q<ScrollView>("team-list");
                    noTeamLabel = overlay.Q<Label>("no-team-label");
                    
                    revenueSection = overlay.Q<VisualElement>("revenue-section");
                    monthlyRevenue = overlay.Q<Label>("monthly-revenue");
                    totalRevenue = overlay.Q<Label>("total-revenue");
                    monthsActive = overlay.Q<Label>("months-active");
                    
                    closeBtn = overlay.Q<Button>("close-detail-btn");
                    editFeaturesBtn = overlay.Q<Button>("edit-features-btn");
                    advancePhaseBtn = overlay.Q<Button>("advance-phase-btn");
                    
                    closeBtn?.RegisterCallback<ClickEvent>(evt => Hide());
                    editFeaturesBtn?.RegisterCallback<ClickEvent>(OnEditFeaturesClicked);
                    advancePhaseBtn?.RegisterCallback<ClickEvent>(OnAdvancePhaseClicked);
                    
                    overlay.style.display = DisplayStyle.None;
                }
        }

        public void Show(ProductData product)
        {
            if (product == null) 
            {
                Debug.LogWarning("Attempted to show ProductDetailDialog with null product");
                return;
            }
            
            EnsureUIInitialized();
            
            if (overlay == null)
            {
                Debug.LogError("ProductDetailDialog overlay is null - UI not initialized");
                return;
            }
            
            currentProduct = product;
            gameObject.SetActive(true);
            
            RefreshUI();
            overlay.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (overlay != null)
            {
                overlay.style.display = DisplayStyle.None;
            }
            currentProduct = null;
            gameObject.SetActive(false);
        }

        void RefreshUI()
        {
            if (currentProduct == null) return;
            
            UpdateBasicInfo();
            UpdateStats();
            UpdateFeatures();
            UpdateQATier();
            UpdateTeam();
            UpdateRevenue();
            UpdatePhaseButton();
        }

        void UpdateBasicInfo()
        {
            productNameLabel.text = currentProduct.name.ToUpper();
            categoryValue.text = currentProduct.category?.categoryName ?? "Unknown";
            statusValue.text = currentProduct.state.ToString();
            phaseValue.text = currentProduct.currentPhase.ToString();
            
            if (currentProduct.state == ProductState.InDevelopment)
            {
                progressValue.text = $"{currentProduct.developmentProgress:F1}%";
            }
            else
            {
                progressValue.text = "Complete";
            }
        }

        void UpdateStats()
        {
            qualityScore.text = currentProduct.qualityScore.ToString("F1");
            stabilityScore.text = currentProduct.stabilityScore.ToString("F1");
            usabilityScore.text = currentProduct.usabilityScore.ToString("F1");
            innovationScore.text = currentProduct.innovationScore.ToString("F1");
            
            if (currentProduct.state == ProductState.InDevelopment)
            {
                bugCount.text = $"{currentProduct.estimatedBugs:F0} (est)";
            }
            else
            {
                bugCount.text = currentProduct.bugCount.ToString("F0");
            }
            
            qualityBar.value = currentProduct.qualityScore;
            stabilityBar.value = currentProduct.stabilityScore;
            usabilityBar.value = currentProduct.usabilityScore;
            innovationBar.value = currentProduct.innovationScore;
        }

        void UpdateFeatures()
        {
            featuresList.Clear();
            
            if (currentProduct.selectedFeatureIds == null || currentProduct.selectedFeatureIds.Count == 0)
            {
                noFeaturesLabel.style.display = DisplayStyle.Flex;
                featuresList.style.display = DisplayStyle.None;
                return;
            }
            
            noFeaturesLabel.style.display = DisplayStyle.None;
            featuresList.style.display = DisplayStyle.Flex;
            
            foreach (var featureId in currentProduct.selectedFeatureIds)
            {
                var feature = definitionResolver?.Resolve<FeatureNodeSO>(featureId);
                if (feature != null)
                {
                    var tag = new Label(feature.featureName);
                    tag.AddToClassList("feature-tag");
                    featuresList.Add(tag);
                }
            }
        }

        void UpdateQATier()
        {
            if (string.IsNullOrEmpty(currentProduct.selectedQATierId))
            {
                qaTierValue.text = "None";
            }
            else
            {
                var tier = definitionResolver?.Resolve<QATierSO>(currentProduct.selectedQATierId);
                qaTierValue.text = tier != null ? tier.tierName : "Unknown";
            }
        }

        void UpdateTeam()
        {
            teamList.Clear();
            
            if (currentProduct.assignedEmployeeIds == null || currentProduct.assignedEmployeeIds.Count == 0)
            {
                noTeamLabel.style.display = DisplayStyle.Flex;
                teamList.style.display = DisplayStyle.None;
                return;
            }
            
            noTeamLabel.style.display = DisplayStyle.None;
            teamList.style.display = DisplayStyle.Flex;
            
            foreach (var empId in currentProduct.assignedEmployeeIds)
            {
                var emp = employeeSystem?.Employees.FirstOrDefault(e => e.employeeId == empId);
                if (emp != null)
                {
                    var empRow = new VisualElement();
                    empRow.style.flexDirection = FlexDirection.Row;
                    empRow.style.justifyContent = Justify.SpaceBetween;
                    empRow.style.marginBottom = 5;
                    
                    var nameLabel = new Label(emp.employeeName);
                    var roleLabel = new Label(emp.role?.roleName ?? "Unknown");
                    roleLabel.AddToClassList("text-muted");
                    
                    empRow.Add(nameLabel);
                    empRow.Add(roleLabel);
                    teamList.Add(empRow);
                }
            }
        }

        void UpdateRevenue()
        {
            if (currentProduct.state == ProductState.Released)
            {
                revenueSection.style.display = DisplayStyle.Flex;
                monthlyRevenue.text = $"${currentProduct.monthlyRevenue:N0}";
                totalRevenue.text = $"${currentProduct.totalRevenue:N0}";
                monthsActive.text = currentProduct.monthsActive.ToString();
            }
            else
            {
                revenueSection.style.display = DisplayStyle.None;
            }
        }

        void UpdatePhaseButton()
        {
            if (currentProduct.state == ProductState.InDevelopment)
            {
                advancePhaseBtn.style.display = DisplayStyle.Flex;
                
                string nextPhaseText = currentProduct.currentPhase switch
                {
                    ProjectPhase.Implementation => "Advance to Bug Fix Phase",
                    ProjectPhase.BugFix => "Advance to Polish Phase",
                    ProjectPhase.Polish => "Already in Final Phase",
                    _ => "Advance Phase"
                };
                
                advancePhaseBtn.text = nextPhaseText;
                advancePhaseBtn.SetEnabled(currentProduct.currentPhase != ProjectPhase.Polish);
            }
            else
            {
                advancePhaseBtn.style.display = DisplayStyle.None;
            }
            
            if (currentProduct.state == ProductState.InDevelopment)
            {
                editFeaturesBtn.style.display = DisplayStyle.Flex;
                editFeaturesBtn.SetEnabled(true);
            }
            else
            {
                editFeaturesBtn.style.display = DisplayStyle.None;
            }
        }

        void OnEditFeaturesClicked(ClickEvent evt)
        {
            if (featuresDialog != null && currentProduct != null)
            {
                featuresDialog.Show(currentProduct);
            }
        }

        void OnAdvancePhaseClicked(ClickEvent evt)
        {
            if (currentProduct != null && productSystem != null)
            {
                EventBus.Publish(new RequestAdvanceProductPhaseEvent
                {
                    productId = currentProduct.productId
                });
            }
        }

        void HandleProductProgress(OnProductProgressUpdatedEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                currentProduct = productSystem?.GetProduct(currentProduct.productId);
                if (currentProduct != null)
                {
                    RefreshUI();
                }
            }
        }

        void HandleProductReleased(OnProductReleasedEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                currentProduct = productSystem?.GetProduct(currentProduct.productId);
                if (currentProduct != null)
                {
                    RefreshUI();
                }
            }
        }

        void HandleFeatureAdded(OnFeatureAddedToProductEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                currentProduct = productSystem?.GetProduct(currentProduct.productId);
                if (currentProduct != null)
                {
                    RefreshUI();
                }
            }
        }

        void HandleQATierChanged(OnProductQATierChangedEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                currentProduct = productSystem?.GetProduct(currentProduct.productId);
                if (currentProduct != null)
                {
                    RefreshUI();
                }
            }
        }

        void HandlePhaseAdvanced(OnProductPhaseAdvancedEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                currentProduct = productSystem?.GetProduct(currentProduct.productId);
                if (currentProduct != null)
                {
                    RefreshUI();
                }
            }
        }
    }
}
