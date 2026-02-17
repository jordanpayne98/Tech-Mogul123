using System;

namespace TechMogul.Systems
{
    [Serializable]
    public class RivalCompanyData
    {
        public string CompanyId;
        public string Name;
        public string Industry;
        
        public float MarketShare;
        public int EmployeeCount;
        public string Description;
        
        public RivalCompanyData(string companyId, string name, string industry, float marketShare, int employeeCount, string description)
        {
            CompanyId = companyId;
            Name = name;
            Industry = industry;
            MarketShare = marketShare;
            EmployeeCount = employeeCount;
            Description = description;
        }
    }
}
