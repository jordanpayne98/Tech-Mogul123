using System;
using System.Collections.Generic;
using TechMogul.Data;

namespace TechMogul.Products
{
    [Serializable]
    public enum ProductState
    {
        InDevelopment,
        Released,
        Deprecated
    }

    [Serializable]
    public class ProductData
    {
        public string productId;
        public string name;
        public TechMogul.Data.ProductCategorySO category;
        
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

        public ProductData(string id, string productName, TechMogul.Data.ProductCategorySO cat, int currentDay)
        {
            productId = id;
            name = productName;
            category = cat;
            state = ProductState.InDevelopment;
            currentPhase = ProjectPhase.Implementation;
            developmentProgress = 0f;
            targetQuality = 0f;
            actualQuality = 0f;
            assignedEmployeeIds = new List<string>();
            selectedFeatureIds = new List<string>();
            selectedQATierId = null;
            
            qualityScore = 0f;
            stabilityScore = 50f;
            usabilityScore = 50f;
            innovationScore = 0f;
            bugCount = 0f;
            estimatedBugs = 0f;
            
            marketingSpend = 0f;
            reputationContribution = 0f;
            priceCompetitiveness = 0.5f;
            standardAlignmentBonus = 0f;
            ecosystemBonus = 0f;
            
            monthlyRevenue = 0f;
            totalRevenue = 0f;
            monthsActive = 0;
            startDay = currentDay;
            releaseDay = -1;
        }
    }
}
