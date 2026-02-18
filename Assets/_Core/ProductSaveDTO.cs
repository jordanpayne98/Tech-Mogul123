using System;
using System.Collections.Generic;
using TechMogul.Products;
using TechMogul.Data;

namespace TechMogul.Core.Save
{
    [Serializable]
    public class SerializableProduct
    {
        public string productId;
        public string name;
        public string categoryId;
        
        public ProductState state;
        public ProjectPhase currentPhase;
        public float developmentProgress;
        public float targetQuality;
        public float actualQuality;
        
        public List<string> assignedEmployeeIds;
        public List<string> selectedFeatureIds;
        public string selectedQATierId;
        
        public float qualityScore;
        public float stabilityScore;
        public float usabilityScore;
        public float innovationScore;
        public float bugCount;
        public float estimatedBugs;
        
        public float marketingSpend;
        public float reputationContribution;
        public float priceCompetitiveness;
        public float standardAlignmentBonus;
        public float ecosystemBonus;
        
        public float monthlyRevenue;
        public float totalRevenue;
        public int monthsActive;
        
        public int startDay;
        public int releaseDay;
        
        public static SerializableProduct FromProduct(ProductData product, IDefinitionResolver resolver)
        {
            if (resolver == null)
            {
                UnityEngine.Debug.LogError("IDefinitionResolver is null in SerializableProduct.FromProduct");
                return null;
            }
            
            string categoryId = string.Empty;
            if (product.category != null)
            {
                categoryId = resolver.GetId(product.category);
            }
            
            return new SerializableProduct
            {
                productId = product.productId,
                name = product.name,
                categoryId = categoryId,
                
                state = product.state,
                currentPhase = product.currentPhase,
                developmentProgress = product.developmentProgress,
                targetQuality = product.targetQuality,
                actualQuality = product.actualQuality,
                
                assignedEmployeeIds = product.assignedEmployeeIds != null ? new List<string>(product.assignedEmployeeIds) : new List<string>(),
                selectedFeatureIds = product.selectedFeatureIds != null ? new List<string>(product.selectedFeatureIds) : new List<string>(),
                selectedQATierId = product.selectedQATierId,
                
                qualityScore = product.qualityScore,
                stabilityScore = product.stabilityScore,
                usabilityScore = product.usabilityScore,
                innovationScore = product.innovationScore,
                bugCount = product.bugCount,
                estimatedBugs = product.estimatedBugs,
                
                marketingSpend = product.marketingSpend,
                reputationContribution = product.reputationContribution,
                priceCompetitiveness = product.priceCompetitiveness,
                standardAlignmentBonus = product.standardAlignmentBonus,
                ecosystemBonus = product.ecosystemBonus,
                
                monthlyRevenue = product.monthlyRevenue,
                totalRevenue = product.totalRevenue,
                monthsActive = product.monthsActive,
                
                startDay = product.startDay,
                releaseDay = product.releaseDay
            };
        }
        
        public ProductData ToProduct(int currentDay, IDefinitionResolver resolver)
        {
            if (resolver == null)
            {
                UnityEngine.Debug.LogError("IDefinitionResolver is null in SerializableProduct.ToProduct");
                return null;
            }
            
            TechMogul.Data.ProductCategorySO category = null;
            if (!string.IsNullOrEmpty(categoryId))
            {
                category = resolver.Resolve<TechMogul.Data.ProductCategorySO>(categoryId);
                if (category == null)
                {
                    UnityEngine.Debug.LogWarning($"Failed to resolve product category with ID: {categoryId}");
                }
            }
            
            ProductData product = new ProductData(productId, name, category, currentDay)
            {
                state = this.state,
                currentPhase = this.currentPhase,
                developmentProgress = this.developmentProgress,
                targetQuality = this.targetQuality,
                actualQuality = this.actualQuality,
                
                assignedEmployeeIds = this.assignedEmployeeIds != null ? new List<string>(this.assignedEmployeeIds) : new List<string>(),
                selectedFeatureIds = this.selectedFeatureIds != null ? new List<string>(this.selectedFeatureIds) : new List<string>(),
                selectedQATierId = this.selectedQATierId,
                
                qualityScore = this.qualityScore,
                stabilityScore = this.stabilityScore,
                usabilityScore = this.usabilityScore,
                innovationScore = this.innovationScore,
                bugCount = this.bugCount,
                estimatedBugs = this.estimatedBugs,
                
                marketingSpend = this.marketingSpend,
                reputationContribution = this.reputationContribution,
                priceCompetitiveness = this.priceCompetitiveness,
                standardAlignmentBonus = this.standardAlignmentBonus,
                ecosystemBonus = this.ecosystemBonus,
                
                monthlyRevenue = this.monthlyRevenue,
                totalRevenue = this.totalRevenue,
                monthsActive = this.monthsActive,
                
                startDay = this.startDay,
                releaseDay = this.releaseDay
            };
            
            return product;
        }
    }
}
