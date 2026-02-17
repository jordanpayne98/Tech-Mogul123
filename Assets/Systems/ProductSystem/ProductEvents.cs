using System.Collections.Generic;

namespace TechMogul.Products
{
    public class RequestStartProductEvent
    {
        public string productName;
        public TechMogul.Data.ProductCategorySO category;
        public List<string> assignedEmployeeIds;
    }

    public class OnProductStartedEvent
    {
        public string productId;
        public string name;
        public TechMogul.Data.ProductCategorySO category;
        public List<string> assignedEmployeeIds;
    }

    public class RequestAssignEmployeeToProductEvent
    {
        public string productId;
        public string employeeId;
    }

    public class RequestUnassignEmployeeFromProductEvent
    {
        public string productId;
        public string employeeId;
    }

    public class OnProductProgressUpdatedEvent
    {
        public string productId;
        public float progress;
        public float daysRemaining;
    }

    public class OnProductReleasedEvent
    {
        public string productId;
        public string name;
        public float quality;
        public float estimatedRevenue;
    }

    public class OnProductRevenueEvent
    {
        public string productId;
        public float revenue;
    }

    public class OnTotalProductRevenueEvent
    {
        public float totalRevenue;
        public int productCount;
    }
}
