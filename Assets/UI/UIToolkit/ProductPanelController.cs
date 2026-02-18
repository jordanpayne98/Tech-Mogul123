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
    public class ProductPanelController : UIController
    {
        [Header("Product Categories")]
        [SerializeField] private List<TechMogul.Data.ProductCategorySO> productCategories;
        
        [Header("Standalone Dialogs")]
        [SerializeField] private GameObject startProductDialogGO;
        [SerializeField] private GameObject productDetailDialogGO;
        
        private ProductSystem productSystem;
        private EmployeeSystem employeeSystem;
        private StartProductDialogController startProductController;
        private ProductDetailDialogController productDetailController;
        private VisualElement productPanel;
        
        private VisualElement productList;
        private Button startProductBtn;
        
        private ProductData selectedProduct;

        protected override void SubscribeToEvents()
        {
            Subscribe<OnProductStartedEvent>(HandleProductStarted);
            Subscribe<OnProductProgressUpdatedEvent>(HandleProductProgress);
            Subscribe<OnProductReleasedEvent>(HandleProductReleased);
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
                startProductController = startProductDialogGO.GetComponent<StartProductDialogController>();
                if (startProductController != null)
                {
                    startProductController.SetProductCategories(productCategories);
                }
            }
            
            if (productDetailDialogGO != null)
            {
                productDetailController = productDetailDialogGO.GetComponent<ProductDetailDialogController>();
                if (productDetailController != null)
                {
                    Debug.Log("ProductDetailDialogController initialized successfully");
                }
                else
                {
                    Debug.LogError("ProductDetailDialogController component not found on ProductDetailDialog GameObject!");
                }
            }
            else
            {
                Debug.LogError("ProductDetailDialog GameObject not assigned in ProductPanelController Inspector!");
            }
            
            if (startProductBtn != null)
            {
                startProductBtn.clicked += ShowStartProductDialog;
            }
            else
            {
                Debug.LogWarning("Start Product button not found in ProductPanel UI!");
            }
            
            RefreshProductList();
        }
        
        void ShowStartProductDialog()
        {
            if (startProductController != null)
            {
                startProductController.Show();
            }
            else
            {
                Debug.LogError("StartProductDialogController not initialized");
            }
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
            
            if (productDetailController != null)
            {
                productDetailController.Show(product);
            }
        }
    }
}
