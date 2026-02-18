namespace TechMogul.Systems
{
    public class OnRivalsInitializedEvent
    {
        public int RivalCount;
        
        public OnRivalsInitializedEvent(int rivalCount)
        {
            RivalCount = rivalCount;
        }
    }
    
    public class OnMarketSharesUpdatedEvent
    {
    }
    
    public class OnQuarterlyReportEvent
    {
    }
    
    public class OnCompanyEnteredMarketEvent
    {
        public string CompanyId;
        public string CompanyName;
        public string CategoryId;
    }
    
    public class OnCompanyExitedMarketEvent
    {
        public string CompanyId;
        public string CompanyName;
        public string Reason;
    }
    
    public class OnStartupPromotedEvent
    {
        public string CompanyId;
        public string CompanyName;
    }
}
