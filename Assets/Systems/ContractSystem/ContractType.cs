namespace TechMogul.Contracts
{
    public enum ContractType
    {
        ModuleDevelopment,      // Permanent feature/module progress for issuer
        ComplianceStandards,    // Permanent standard alignment progress for issuer
        EcosystemIntegration,   // Permanent ecosystem bonus progress for issuer
        MarketingCampaign,      // Temporary marketing component improvement (bounded)
        OptimizationCost        // Temporary price competitiveness improvement (bounded)
    }
    
    public static class ContractTypeExtensions
    {
        public static bool IsPermanentEffect(this ContractType type)
        {
            return type == ContractType.ModuleDevelopment ||
                   type == ContractType.ComplianceStandards ||
                   type == ContractType.EcosystemIntegration;
        }
        
        public static bool IsTemporaryEffect(this ContractType type)
        {
            return type == ContractType.MarketingCampaign ||
                   type == ContractType.OptimizationCost;
        }
    }
}
