using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Products;
using TechMogul.Systems;
using TechMogul.Data;

namespace TechMogul.UI
{
    public class StartProductDialogController : UIController
    {
        private List<TechMogul.Data.ProductCategorySO> productCategories;
        
        private VisualElement overlay;
        private TextField productNameField;
        private DropdownField categoryDropdown;
        private ScrollView employeeSelector;
        private Label completionEstimate;
        private Button closeBtn;
        private Button cancelBtn;
        private Button confirmBtn;
        
        private EmployeeSystem employeeSystem;
        private List<string> selectedEmployeeIds = new List<string>();

        protected override void SubscribeToEvents()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            employeeSystem = FindFirstObjectByType<EmployeeSystem>();
        }
        
        void Start()
        {
            EnsureUIInitialized();
            Hide();
        }
        
        void EnsureUIInitialized()
        {
            if (overlay != null) return;
            
            var uiDoc = GetComponent<UIDocument>();
            if (uiDoc == null || uiDoc.rootVisualElement == null)
            {
                Debug.LogError("StartProductDialog UIDocument not ready");
                return;
            }
            
            overlay = uiDoc.rootVisualElement.Q<VisualElement>("overlay");
            
            if (overlay != null)
            {
                productNameField = overlay.Q<TextField>("product-name-field");
                categoryDropdown = overlay.Q<DropdownField>("category-dropdown");
                employeeSelector = overlay.Q<ScrollView>("employee-selector");
                completionEstimate = overlay.Q<Label>("completion-estimate");
                closeBtn = overlay.Q<Button>("close-dialog-btn");
                cancelBtn = overlay.Q<Button>("cancel-btn");
                confirmBtn = overlay.Q<Button>("confirm-start-btn");
                
                closeBtn?.RegisterCallback<ClickEvent>(evt => Hide());
                cancelBtn?.RegisterCallback<ClickEvent>(evt => Hide());
                confirmBtn?.RegisterCallback<ClickEvent>(OnConfirmClicked);
                
                overlay.style.display = DisplayStyle.None;
            }
        }

        public void SetProductCategories(List<TechMogul.Data.ProductCategorySO> categories)
        {
            productCategories = categories;
        }

        public void Show()
        {
            EnsureUIInitialized();
            
            if (overlay == null)
            {
                Debug.LogError("StartProductDialog overlay is null");
                return;
            }
            
            selectedEmployeeIds.Clear();
            
            if (productNameField != null) productNameField.value = "";
            
            SetupCategoryDropdown();
            RefreshEmployeeSelector();
            
            overlay.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (overlay != null)
            {
                overlay.style.display = DisplayStyle.None;
            }
        }

        void SetupCategoryDropdown()
        {
            if (categoryDropdown == null || productCategories == null || productCategories.Count == 0) return;
            
            categoryDropdown.choices = productCategories.Select(c => c.categoryName).ToList();
            categoryDropdown.index = 0;
        }

        void RefreshEmployeeSelector()
        {
            if (employeeSelector == null || employeeSystem == null) return;
            
            employeeSelector.Clear();
            
            var availableEmployees = employeeSystem.Employees.Where(e => e.isAvailable).ToList();
            
            if (availableEmployees.Count == 0)
            {
                var noEmployeesLabel = new Label("No available employees. Hire more employees first.");
                noEmployeesLabel.AddToClassList("text-muted");
                employeeSelector.Add(noEmployeesLabel);
                return;
            }
            
            foreach (var employee in availableEmployees)
            {
                var empRow = new VisualElement();
                empRow.style.flexDirection = FlexDirection.Row;
                empRow.style.justifyContent = Justify.SpaceBetween;
                empRow.style.marginBottom = 5;
                empRow.style.paddingTop = 4;
                empRow.style.paddingBottom = 4;
                
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
                });
                
                var infoContainer = new VisualElement();
                infoContainer.style.flexGrow = 1;
                infoContainer.style.marginLeft = 8;
                
                var nameLabel = new Label(employee.employeeName);
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                
                var skillsLabel = new Label($"Dev: {employee.devSkill:F0} | Design: {employee.designSkill:F0} | Marketing: {employee.marketingSkill:F0}");
                skillsLabel.AddToClassList("text-muted");
                skillsLabel.style.fontSize = 11;
                
                infoContainer.Add(nameLabel);
                infoContainer.Add(skillsLabel);
                
                empRow.Add(checkbox);
                empRow.Add(infoContainer);
                
                employeeSelector.Add(empRow);
            }
        }

        void OnConfirmClicked(ClickEvent evt)
        {
            string productName = productNameField?.value ?? "";
            if (string.IsNullOrWhiteSpace(productName))
            {
                Debug.LogWarning("Product name is required");
                return;
            }
            
            if (categoryDropdown == null || categoryDropdown.index < 0 || productCategories == null)
            {
                Debug.LogWarning("Please select a category");
                return;
            }
            
            var category = productCategories[categoryDropdown.index];
            
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
            
            Hide();
        }
    }
}
