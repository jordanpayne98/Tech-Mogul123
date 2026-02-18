using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Products;

namespace TechMogul.UI
{
    public class ProductFeaturesDialogController : UIController
    {
        private VisualElement overlay;
        private ProductData currentProduct;
        private ProductSystem productSystem;
        private IDefinitionResolver definitionResolver;
        
        private Label productNameLabel;
        private Label productCategoryLabel;
        private Label qualityValue;
        private Label stabilityValue;
        private Label usabilityValue;
        private Label innovationValue;
        private Label bugsValue;
        private Label devTimeValue;
        
        private DropdownField qaTierDropdown;
        private ScrollView availableFeaturesList;
        private ScrollView selectedFeaturesList;
        
        private Button closeBtn;
        private Button cancelBtn;
        private Button applyBtn;
        
        private List<FeatureNodeSO> allFeatures;
        private List<QATierSO> allQATiers;
        private List<string> tempSelectedFeatureIds;
        private string tempSelectedQATierId;

        protected override void SubscribeToEvents()
        {
            Subscribe<OnFeatureAddedToProductEvent>(HandleFeatureAdded);
            Subscribe<OnFeatureAddFailedEvent>(HandleFeatureAddFailed);
            Subscribe<OnProductQATierChangedEvent>(HandleQATierChanged);
        }

        protected override void Awake()
        {
            base.Awake();
            
            productSystem = FindFirstObjectByType<ProductSystem>();
            definitionResolver = ServiceLocator.Instance.Get<IDefinitionResolver>();
            
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc != null && uiDoc.rootVisualElement != null)
            {
                overlay = uiDoc.rootVisualElement.Q<VisualElement>("overlay");
                
                productNameLabel = overlay.Q<Label>("product-name");
                productCategoryLabel = overlay.Q<Label>("product-category");
                qualityValue = overlay.Q<Label>("quality-value");
                stabilityValue = overlay.Q<Label>("stability-value");
                usabilityValue = overlay.Q<Label>("usability-value");
                innovationValue = overlay.Q<Label>("innovation-value");
                bugsValue = overlay.Q<Label>("bugs-value");
                devTimeValue = overlay.Q<Label>("devtime-value");
                
                qaTierDropdown = overlay.Q<DropdownField>("qa-tier-dropdown");
                availableFeaturesList = overlay.Q<ScrollView>("available-features-list");
                selectedFeaturesList = overlay.Q<ScrollView>("selected-features-list");
                
                closeBtn = overlay.Q<Button>("close-dialog-btn");
                cancelBtn = overlay.Q<Button>("cancel-btn");
                applyBtn = overlay.Q<Button>("apply-btn");
                
                closeBtn?.RegisterCallback<ClickEvent>(evt => Hide());
                cancelBtn?.RegisterCallback<ClickEvent>(evt => Hide());
                applyBtn?.RegisterCallback<ClickEvent>(OnApplyClicked);
                
                qaTierDropdown?.RegisterCallback<ChangeEvent<string>>(OnQATierChanged);
                
                LoadDefinitions();
                SetupQATierDropdown();
                
                overlay.style.display = DisplayStyle.None;
            }
        }

        void LoadDefinitions()
        {
            allFeatures = new List<FeatureNodeSO>();
            allQATiers = new List<QATierSO>();
            
            var registry = Resources.Load<DefinitionRegistrySO>("DefinitionRegistry");
            if (registry != null)
            {
                allFeatures.AddRange(registry.features);
                allQATiers.AddRange(registry.qaTiers);
            }
        }

        void SetupQATierDropdown()
        {
            var choices = new List<string> { "None" };
            choices.AddRange(allQATiers.OrderBy(q => q.tier).Select(q => q.tierName));
            
            qaTierDropdown.choices = choices;
            qaTierDropdown.index = 0;
        }

        public void Show(ProductData product)
        {
            if (product == null || overlay == null) return;
            
            currentProduct = product;
            tempSelectedFeatureIds = new List<string>(product.selectedFeatureIds ?? new List<string>());
            tempSelectedQATierId = product.selectedQATierId;
            
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
        }

        void RefreshUI()
        {
            if (currentProduct == null) return;
            
            productNameLabel.text = currentProduct.name;
            productCategoryLabel.text = currentProduct.category?.categoryName ?? "Unknown";
            
            UpdateQATierSelection();
            UpdateFeatureLists();
            UpdateStats();
        }

        void UpdateQATierSelection()
        {
            if (string.IsNullOrEmpty(tempSelectedQATierId))
            {
                qaTierDropdown.index = 0;
            }
            else
            {
                var tier = allQATiers.FirstOrDefault(t => t.id == tempSelectedQATierId);
                if (tier != null)
                {
                    var index = qaTierDropdown.choices.IndexOf(tier.tierName);
                    qaTierDropdown.index = index >= 0 ? index : 0;
                }
            }
        }

        void UpdateFeatureLists()
        {
            availableFeaturesList.Clear();
            selectedFeaturesList.Clear();
            
            var selectedFeatures = ResolveFeatures(tempSelectedFeatureIds);
            
            foreach (var feature in allFeatures.OrderBy(f => f.unlockYear).ThenBy(f => f.featureName))
            {
                if (tempSelectedFeatureIds.Contains(feature.id))
                {
                    AddSelectedFeatureElement(feature);
                }
                else
                {
                    AddAvailableFeatureElement(feature, selectedFeatures);
                }
            }
        }

        void AddAvailableFeatureElement(FeatureNodeSO feature, List<FeatureNodeSO> currentlySelected)
        {
            var container = new VisualElement();
            container.AddToClassList("feature-item");
            
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            
            var nameLabel = new Label(feature.featureName);
            nameLabel.AddToClassList("feature-name");
            
            var addBtn = new Button { text = "+" };
            addBtn.AddToClassList("button");
            addBtn.AddToClassList("button--small");
            
            bool canAdd = FeatureGraphValidator.ValidateFeatureSelection(
                currentlySelected, feature, out string errorMessage);
            
            addBtn.SetEnabled(canAdd);
            if (!canAdd)
            {
                var tooltip = new Label($"⚠ {errorMessage}");
                tooltip.AddToClassList("text-warning");
                tooltip.style.fontSize = 10;
                container.Add(tooltip);
            }
            
            addBtn.clicked += () => AddFeature(feature);
            
            headerRow.Add(nameLabel);
            headerRow.Add(addBtn);
            container.Add(headerRow);
            
            var statsRow = new VisualElement();
            statsRow.style.flexDirection = FlexDirection.Row;
            statsRow.style.fontSize = 10;
            statsRow.AddToClassList("text-muted");
            
            statsRow.Add(new Label($"Cost: {feature.devCost} | Time: {feature.devTime}d"));
            container.Add(statsRow);
            
            if (!string.IsNullOrEmpty(feature.description))
            {
                var desc = new Label(feature.description);
                desc.style.fontSize = 10;
                desc.AddToClassList("text-muted");
                container.Add(desc);
            }
            
            availableFeaturesList.Add(container);
        }

        void AddSelectedFeatureElement(FeatureNodeSO feature)
        {
            var container = new VisualElement();
            container.AddToClassList("feature-item");
            container.AddToClassList("feature-item--selected");
            
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            
            var nameLabel = new Label(feature.featureName);
            nameLabel.AddToClassList("feature-name");
            
            var removeBtn = new Button { text = "−" };
            removeBtn.AddToClassList("button");
            removeBtn.AddToClassList("button--small");
            removeBtn.AddToClassList("button--danger");
            removeBtn.clicked += () => RemoveFeature(feature);
            
            headerRow.Add(nameLabel);
            headerRow.Add(removeBtn);
            container.Add(headerRow);
            
            var statsRow = new VisualElement();
            statsRow.style.flexDirection = FlexDirection.Row;
            statsRow.style.fontSize = 10;
            
            var impactText = $"Stability: {feature.stabilityImpact:+0;-0;0} | Usability: {feature.usabilityImpact:+0;-0;0} | Innovation: {feature.innovationImpact:+0;-0;0}";
            statsRow.Add(new Label(impactText));
            container.Add(statsRow);
            
            selectedFeaturesList.Add(container);
        }

        void AddFeature(FeatureNodeSO feature)
        {
            if (!tempSelectedFeatureIds.Contains(feature.id))
            {
                tempSelectedFeatureIds.Add(feature.id);
                UpdateFeatureLists();
                UpdateStats();
            }
        }

        void RemoveFeature(FeatureNodeSO feature)
        {
            tempSelectedFeatureIds.Remove(feature.id);
            UpdateFeatureLists();
            UpdateStats();
        }

        void OnQATierChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == "None" || qaTierDropdown.index == 0)
            {
                tempSelectedQATierId = null;
            }
            else
            {
                var tier = allQATiers.FirstOrDefault(t => t.tierName == evt.newValue);
                if (tier != null)
                {
                    tempSelectedQATierId = tier.id;
                }
            }
            
            UpdateStats();
        }

        void UpdateStats()
        {
            var features = ResolveFeatures(tempSelectedFeatureIds);
            var qaTier = ResolveQATier(tempSelectedQATierId);
            
            float stability = FeatureGraphValidator.CalculateStabilityScore(features, qaTier);
            float usability = FeatureGraphValidator.CalculateUsabilityScore(features, qaTier);
            float innovation = FeatureGraphValidator.CalculateInnovationScore(features, qaTier);
            float featureValue = FeatureGraphValidator.CalculateFeatureValue(features);
            float marketValue = FeatureGraphValidator.CalculateMarketValue(features);
            
            float quality = (featureValue * 0.3f) + (marketValue * 0.2f);
            
            float bugRate = (100f - stability) / 100f;
            int bugs = Mathf.RoundToInt(features.Count * bugRate * 5f);
            
            int devTime = FeatureGraphValidator.CalculateTotalDevTime(features, qaTier);
            
            qualityValue.text = quality.ToString("F1");
            stabilityValue.text = stability.ToString("F1");
            usabilityValue.text = usability.ToString("F1");
            innovationValue.text = innovation.ToString("F1");
            bugsValue.text = bugs.ToString();
            devTimeValue.text = $"{devTime} days";
        }

        List<FeatureNodeSO> ResolveFeatures(List<string> featureIds)
        {
            var features = new List<FeatureNodeSO>();
            if (definitionResolver == null || featureIds == null) return features;
            
            foreach (var id in featureIds)
            {
                var feature = definitionResolver.Resolve<FeatureNodeSO>(id);
                if (feature != null)
                {
                    features.Add(feature);
                }
            }
            
            return features;
        }

        QATierSO ResolveQATier(string tierId)
        {
            if (string.IsNullOrEmpty(tierId) || definitionResolver == null) return null;
            return definitionResolver.Resolve<QATierSO>(tierId);
        }

        void OnApplyClicked(ClickEvent evt)
        {
            if (currentProduct == null || productSystem == null) return;
            
            foreach (var featureId in tempSelectedFeatureIds)
            {
                if (!currentProduct.selectedFeatureIds.Contains(featureId))
                {
                    var feature = definitionResolver.Resolve<FeatureNodeSO>(featureId);
                    if (feature != null)
                    {
                        EventBus.Publish(new RequestAddFeatureToProductEvent
                        {
                            productId = currentProduct.productId,
                            feature = feature
                        });
                    }
                }
            }
            
            var removedFeatures = currentProduct.selectedFeatureIds
                .Where(id => !tempSelectedFeatureIds.Contains(id))
                .ToList();
            
            foreach (var featureId in removedFeatures)
            {
                currentProduct.selectedFeatureIds.Remove(featureId);
            }
            
            var qaTier = ResolveQATier(tempSelectedQATierId);
            EventBus.Publish(new RequestSetProductQATierEvent
            {
                productId = currentProduct.productId,
                qaTier = qaTier
            });
            
            Hide();
        }

        void HandleFeatureAdded(OnFeatureAddedToProductEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                RefreshUI();
            }
        }

        void HandleFeatureAddFailed(OnFeatureAddFailedEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                Debug.LogWarning($"Feature add failed: {evt.errorMessage}");
            }
        }

        void HandleQATierChanged(OnProductQATierChangedEvent evt)
        {
            if (currentProduct != null && evt.productId == currentProduct.productId)
            {
                RefreshUI();
            }
        }
    }
}
