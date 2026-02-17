using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using TechMogul.Core;
using TechMogul.Products;
using TechMogul.Systems;
using TechMogul.Data;

namespace TechMogul.UI
{
    public class ProductPanelController : MonoBehaviour
    {
        [Header("Product Categories")]
        [SerializeField] private List<TechMogul.Data.ProductCategorySO> productCategories;
        
        [Header("Standalone Dialogs")]
        [SerializeField] private GameObject startProductDialogGO;
        [SerializeField] private GameObject productDetailDialogGO;
        
        private ProductSystem productSystem;
        private EmployeeSystem employeeSystem;
        private VisualElement productPanel;
        
        private VisualElement productList;
        private VisualElement startProductDialog;
        private VisualElement productDetailView;
        
        private Button startProductBtn;
        private Button closeDialogBtn;
        private Button confirmStartBtn;
        
        private DropdownField categoryDropdown;
        private TextField productNameField;
        private ScrollView employeeSelector;
        private List<string> selectedEmployeeIds = new List<string>();
        
        private ProductData selectedProduct;

        void OnEnable()
        {
            EventBus.Subscribe<OnProductStartedEvent>(HandleProductStarted);
            EventBus.Subscribe<OnProductProgressUpdatedEvent>(HandleProductProgress);
            EventBus.Subscribe<OnProductReleasedEvent>(HandleProductReleased);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<OnProductStartedEvent>(HandleProductStarted);
            EventBus.Unsubscribe<OnProductProgressUpdatedEvent>(HandleProductProgress);
            EventBus.Unsubscribe<OnProductReleasedEvent>(HandleProductReleased);
        }

        public void Initialize(VisualElement panel)
        {
            productPanel = panel;
            
            // Find systems
            productSystem = FindFirstObjectByType<ProductSystem>();
            employeeSystem = FindFirstObjectByType<EmployeeSystem>();
            
            if (productSystem == null)
            {
                Debug.LogError("ProductSystem not found in scene!");
                return;
            }
            
            if (employeeSystem == null)
            {
                Debug.LogError("EmployeeSystem not found in scene!");
                return;
            }
            
            // Get UI elements from panel
            productList = panel.Q<VisualElement>("product-list");
            startProductBtn = panel.Q<Button>("start-product-btn");
            
            // Get dialogs from standalone GameObjects
            if (startProductDialogGO != null)
            {
                var dialogDoc = startProductDialogGO.GetComponent<UIDocument>();
                if (dialogDoc != null && dialogDoc.rootVisualElement != null)
                {
                    startProductDialog = dialogDoc.rootVisualElement.Q<VisualElement>("overlay");
                    if (startProductDialog != null)
                    {
                        startProductDialog.style.display = DisplayStyle.None;
                        closeDialogBtn = startProductDialog.Q<Button>("close-dialog-btn");
                        confirmStartBtn = startProductDialog.Q<Button>("confirm-start-btn");
                        categoryDropdown = startProductDialog.Q<DropdownField>("category-dropdown");
                        productNameField = startProductDialog.Q<TextField>("product-name-field");
                        employeeSelector = startProductDialog.Q<ScrollView>("employee-selector");
                        
                        SetupDialogEvents();
                    }
                    else
                    {
                        Debug.LogError("Start Product Dialog overlay element not found!");
                    }
                }
                else
                {
                    Debug.LogError("StartProductDialog GameObject is missing UIDocument component or rootVisualElement!");
                }
            }
            else
            {
                Debug.LogWarning("StartProductDialog GameObject not assigned in ProductPanelController!");
            }
            
            if (productDetailDialogGO != null)
            {
                var dialogDoc = productDetailDialogGO.GetComponent<UIDocument>();
                if (dialogDoc != null && dialogDoc.rootVisualElement != null)
                {
                    productDetailView = dialogDoc.rootVisualElement.Q<VisualElement>("overlay");
                    if (productDetailView != null)
                    {
                        productDetailView.style.display = DisplayStyle.None;
                    }
                }
            }
            
            if (startProductBtn != null)
            {
                startProductBtn.clicked += ShowStartProductDialog;
            }
            
            RefreshProductList();
        }
        
        void SetupDialogEvents()
        {
            if (closeDialogBtn != null)
            {
                closeDialogBtn.clicked += HideStartProductDialog;
            }
            
            if (confirmStartBtn != null)
            {
                confirmStartBtn.clicked += StartProduct;
            }
            
            if (categoryDropdown != null && productCategories != null && productCategories.Count > 0)
            {
                categoryDropdown.choices = productCategories.Select(c => c.categoryName).ToList();
                categoryDropdown.value = categoryDropdown.choices[0];
            }
        }

        void ShowStartProductDialog()
        {
            if (startProductDialog == null) return;
            
            startProductDialog.style.display = DisplayStyle.Flex;
            selectedEmployeeIds.Clear();
            
            if (productNameField != null)
            {
                productNameField.value = "";
            }
            
            RefreshEmployeeSelector();
        }

        void HideStartProductDialog()
        {
            if (startProductDialog != null)
            {
                startProductDialog.style.display = DisplayStyle.None;
            }
        }

        void RefreshEmployeeSelector()
        {
            if (employeeSelector == null || employeeSystem == null) return;
            
            employeeSelector.Clear();
            
            var availableEmployees = employeeSystem.Employees.Where(e => e.isAvailable && !e.isFired).ToList();
            
            if (availableEmployees.Count == 0)
            {
                var noEmployeesLabel = new Label("No available employees. Hire more employees or wait for current work to complete.");
                noEmployeesLabel.AddToClassList("empty-state-text");
                employeeSelector.Add(noEmployeesLabel);
                return;
            }
            
            foreach (var employee in availableEmployees)
            {
                var employeeRow = new VisualElement();
                employeeRow.AddToClassList("employee-selector-row");
                
                var checkbox = new Toggle();
                checkbox.value = selectedEmployeeIds.Contains(employee.employeeId);
                checkbox.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        selectedEmployeeIds.Add(employee.employeeId);
                    }
                    else
                    {
                        selectedEmployeeIds.Remove(employee.employeeId);
                    }
                    UpdateEstimatedCompletion();
                });
                
                var nameLabel = new Label(employee.employeeName);
                nameLabel.AddToClassList("employee-name");
                
                var skillsLabel = new Label($"Dev: {employee.devSkill:F0} | Design: {employee.designSkill:F0} | Marketing: {employee.marketingSkill:F0}");
                skillsLabel.AddToClassList("employee-skills");
                
                employeeRow.Add(checkbox);
                employeeRow.Add(nameLabel);
                employeeRow.Add(skillsLabel);
                
                employeeSelector.Add(employeeRow);
            }
        }

        void UpdateEstimatedCompletion()
        {
            // Calculate estimated days based on selected employees and category
            var selectedCategory = GetSelectedCategory();
            if (selectedCategory == null || selectedEmployeeIds.Count == 0) return;
            
            // Build employee list sorted by productivity
            var employees = new List<Employee>();
            foreach (var empId in selectedEmployeeIds)
            {
                var emp = employeeSystem.GetEmployee(empId);
                if (emp != null) employees.Add(emp);
            }
            
            if (employees.Count == 0) return;
            
            // Sort by productivity (best first)
            employees = employees.OrderByDescending(e =>
            {
                float prod = (e.devSkill * selectedCategory.devSkillWeight) +
                            (e.designSkill * selectedCategory.designSkillWeight) +
                            (e.marketingSkill * selectedCategory.marketingSkillWeight);
                            
                // Apply morale/burnout
                float moraleMultiplier = e.morale / 100f;
                float burnoutPenalty = e.burnout / 100f;
                return prod * moraleMultiplier * (1f - burnoutPenalty);
            }).ToList();
            
            float totalProductivity = 0f;
            
            for (int i = 0; i < employees.Count; i++)
            {
                var emp = employees[i];
                float baseProductivity = 
                    (emp.devSkill * selectedCategory.devSkillWeight) +
                    (emp.designSkill * selectedCategory.designSkillWeight) +
                    (emp.marketingSkill * selectedCategory.marketingSkillWeight);
                
                // Apply morale and burnout
                float moraleMultiplier = emp.morale / 100f;
                float burnoutPenalty = emp.burnout / 100f;
                float effectiveMultiplier = moraleMultiplier * (1f - burnoutPenalty);
                effectiveMultiplier = Mathf.Max(effectiveMultiplier, 0.1f);
                
                float adjustedProductivity = baseProductivity * effectiveMultiplier;
                
                if (i == 0)
                {
                    // First employee = 1.0x base
                    totalProductivity = adjustedProductivity;
                }
                else
                {
                    // Additional employees: 0.1x to 0.6x based on skill match
                    float skillMatchRatio = Mathf.Clamp(adjustedProductivity / 100f, 0f, 1f);
                    float additionalBonus = 0.1f + (skillMatchRatio * 0.5f);
                    totalProductivity += adjustedProductivity * additionalBonus;
                }
            }
            
            float daysEstimate = selectedCategory.baseDevelopmentDays * (50f / totalProductivity);
            
            // Update UI with estimate (if you have a label for it)
            var estimateLabel = startProductDialog?.Q<Label>("completion-estimate");
            if (estimateLabel != null)
            {
                estimateLabel.text = $"Estimated: ~{Mathf.RoundToInt(daysEstimate)} days";
            }
        }

        TechMogul.Data.ProductCategorySO GetSelectedCategory()
        {
            if (categoryDropdown == null || productCategories == null) return null;
            
            string selectedName = categoryDropdown.value;
            return productCategories.FirstOrDefault(c => c.categoryName == selectedName);
        }

        void StartProduct()
        {
            var category = GetSelectedCategory();
            string productName = productNameField?.value ?? "Unnamed Product";
            
            if (category == null)
            {
                Debug.LogWarning("No category selected");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(productName))
            {
                Debug.LogWarning("Product name is required");
                return;
            }
            
            if (selectedEmployeeIds.Count == 0)
            {
                Debug.LogWarning("At least one employee must be assigned");
                return;
            }
            
            EventBus.Publish(new RequestStartProductEvent
            {
                productName = productName,
                category = category,
                assignedEmployeeIds = new List<string>(selectedEmployeeIds)
            });
            
            HideStartProductDialog();
        }

        void HandleProductStarted(OnProductStartedEvent evt)
        {
            RefreshProductList();
        }

        void HandleProductProgress(OnProductProgressUpdatedEvent evt)
        {
            RefreshProductList();
        }

        void HandleProductReleased(OnProductReleasedEvent evt)
        {
            RefreshProductList();
            
            // Show notification
            var notification = $"🎉 Released '{evt.name}' with quality {evt.quality:F0}!";
            Debug.Log(notification);
        }

        void RefreshProductList()
        {
            if (productList == null || productSystem == null) return;
            
            productList.Clear();
            
            var products = productSystem.Products.ToList();
            
            if (products.Count == 0)
            {
                var emptyLabel = new Label("No products yet. Start your first product!");
                emptyLabel.AddToClassList("empty-state-text");
                productList.Add(emptyLabel);
                return;
            }
            
            foreach (var product in products)
            {
                var productCard = CreateProductCard(product);
                productList.Add(productCard);
            }
        }

        VisualElement CreateProductCard(ProductData product)
        {
            var card = new VisualElement();
            card.AddToClassList("product-card");
            
            // Header
            var header = new VisualElement();
            header.AddToClassList("card-header");
            
            var nameLabel = new Label(product.name);
            nameLabel.AddToClassList("product-name");
            
            var categoryLabel = new Label(product.category.categoryName);
            categoryLabel.AddToClassList("product-category");
            
            header.Add(nameLabel);
            header.Add(categoryLabel);
            
            // State badge
            var stateBadge = new Label(product.state.ToString());
            stateBadge.AddToClassList("state-badge");
            stateBadge.AddToClassList($"state-{product.state.ToString().ToLower()}");
            
            // Progress/Revenue info
            var infoContainer = new VisualElement();
            infoContainer.AddToClassList("product-info");
            
            if (product.state == ProductState.InDevelopment)
            {
                var progressLabel = new Label($"Progress: {product.developmentProgress:F1}%");
                var employeesLabel = new Label($"Team: {product.assignedEmployeeIds.Count} employees");
                
                infoContainer.Add(progressLabel);
                infoContainer.Add(employeesLabel);
            }
            else if (product.state == ProductState.Released)
            {
                var qualityLabel = new Label($"Quality: {product.actualQuality:F0}");
                var revenueLabel = new Label($"Revenue: ${product.monthlyRevenue:N0}/month");
                var totalLabel = new Label($"Total: ${product.totalRevenue:N0}");
                
                infoContainer.Add(qualityLabel);
                infoContainer.Add(revenueLabel);
                infoContainer.Add(totalLabel);
            }
            
            card.Add(header);
            card.Add(stateBadge);
            card.Add(infoContainer);
            
            // Click to view details
            card.RegisterCallback<ClickEvent>(evt => ShowProductDetails(product));
            
            return card;
        }

        void ShowProductDetails(ProductData product)
        {
            selectedProduct = product;
            
            if (productDetailView == null) return;
            
            productDetailView.style.display = DisplayStyle.Flex;
            
            // Update detail view content (implement as needed)
            var detailContent = productDetailView.Q<VisualElement>("detail-content");
            if (detailContent != null)
            {
                detailContent.Clear();
                
                detailContent.Add(new Label($"Name: {product.name}"));
                detailContent.Add(new Label($"Category: {product.category.categoryName}"));
                detailContent.Add(new Label($"State: {product.state}"));
                detailContent.Add(new Label($"Progress: {product.developmentProgress:F1}%"));
                detailContent.Add(new Label($"Quality: {product.actualQuality:F0}"));
                detailContent.Add(new Label($"Monthly Revenue: ${product.monthlyRevenue:N0}"));
                detailContent.Add(new Label($"Team Size: {product.assignedEmployeeIds.Count}"));
            }
        }
    }
}
