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
        public float developmentProgress;
        public float targetQuality;
        public float actualQuality;
        
        public List<string> assignedEmployeeIds;
        
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
            developmentProgress = 0f;
            targetQuality = 0f;
            actualQuality = 0f;
            assignedEmployeeIds = new List<string>();
            monthlyRevenue = 0f;
            totalRevenue = 0f;
            monthsActive = 0;
            startDay = currentDay;
            releaseDay = -1;
        }
    }
}
