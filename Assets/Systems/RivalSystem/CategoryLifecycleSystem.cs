using UnityEngine;
using System.Collections.Generic;
using TechMogul.Data;

namespace TechMogul.Systems
{
    public class CategoryLifecycleSystem
    {
        private Dictionary<string, int> _categoryAgeMonths = new Dictionary<string, int>();
        
        public void ProcessMonthlyLifecycle(List<ProductCategorySO> categories)
        {
            foreach (var category in categories)
            {
                if (!_categoryAgeMonths.ContainsKey(category.id))
                {
                    _categoryAgeMonths[category.id] = 0;
                }
                
                _categoryAgeMonths[category.id]++;
                
                UpdateLifecycleStage(category);
                ApplyLifecycleModifiers(category);
            }
        }
        
        void UpdateLifecycleStage(ProductCategorySO category)
        {
            int ageMonths = _categoryAgeMonths[category.id];
            
            CategoryLifecycleStage newStage = category.currentStage;
            
            switch (category.currentStage)
            {
                case CategoryLifecycleStage.Emerging:
                    if (ageMonths > 12)
                        newStage = CategoryLifecycleStage.Growth;
                    break;
                    
                case CategoryLifecycleStage.Growth:
                    if (ageMonths > 36)
                        newStage = CategoryLifecycleStage.Maturity;
                    break;
                    
                case CategoryLifecycleStage.Maturity:
                    if (ageMonths > 72)
                        newStage = CategoryLifecycleStage.Saturation;
                    break;
                    
                case CategoryLifecycleStage.Saturation:
                    if (ageMonths > 120)
                        newStage = CategoryLifecycleStage.Decline;
                    break;
            }
            
            if (newStage != category.currentStage)
            {
                Debug.Log($"{category.categoryName} lifecycle advanced from {category.currentStage} to {newStage}");
                category.currentStage = newStage;
            }
        }
        
        void ApplyLifecycleModifiers(ProductCategorySO category)
        {
            switch (category.currentStage)
            {
                case CategoryLifecycleStage.Emerging:
                    category.elasticity = Mathf.Lerp(category.elasticity, 0.9f, 0.01f);
                    break;
                    
                case CategoryLifecycleStage.Growth:
                    category.elasticity = Mathf.Lerp(category.elasticity, 0.8f, 0.01f);
                    break;
                    
                case CategoryLifecycleStage.Maturity:
                    category.elasticity = Mathf.Lerp(category.elasticity, 0.6f, 0.01f);
                    break;
                    
                case CategoryLifecycleStage.Saturation:
                    category.elasticity = Mathf.Lerp(category.elasticity, 0.4f, 0.01f);
                    break;
                    
                case CategoryLifecycleStage.Decline:
                    category.elasticity = Mathf.Lerp(category.elasticity, 0.5f, 0.01f);
                    break;
            }
        }
        
        public float GetGrowthRateMultiplier(CategoryLifecycleStage stage)
        {
            return stage switch
            {
                CategoryLifecycleStage.Emerging => 1.5f,
                CategoryLifecycleStage.Growth => 1.2f,
                CategoryLifecycleStage.Maturity => 1.0f,
                CategoryLifecycleStage.Saturation => 0.7f,
                CategoryLifecycleStage.Decline => 0.4f,
                _ => 1.0f
            };
        }
        
        public float GetEntryAttractiveness(CategoryLifecycleStage stage)
        {
            return stage switch
            {
                CategoryLifecycleStage.Emerging => 0.8f,
                CategoryLifecycleStage.Growth => 1.2f,
                CategoryLifecycleStage.Maturity => 0.7f,
                CategoryLifecycleStage.Saturation => 0.3f,
                CategoryLifecycleStage.Decline => 0.1f,
                _ => 0.5f
            };
        }
    }
}
