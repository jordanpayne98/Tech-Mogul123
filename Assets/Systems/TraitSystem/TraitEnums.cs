namespace TechMogul.Traits
{
    public enum TraitTier
    {
        Major,
        Minor
    }
    
    public enum TraitCategory
    {
        WorkEthic,
        Personality,
        WorkStability,
        SalaryBehaviour,
        TechnicalAptitude,
        CreativeStyle,
        SocialInfluence,
        SkillGrowth
    }
    
    public enum TraitRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }
    
    public enum TraitTag
    {
        Speed,
        Quality,
        Stability,
        Pressure,
        Innovation,
        Leadership,
        Loyalty,
        Growth,
        Influence,
        Risk
    }
    
    public enum StatType
    {
        Productivity,
        Quality,
        BugRate,
        BurnoutRate,
        StressRecoveryRate,
        Morale,
        TeamMoraleImpact,
        TeamProductivityImpact,
        ConflictProbability,
        SkillGainRate,
        SalaryDemandGrowth,
        QuitChance,
        ContractSuccess,
        ProductivityVariance,
        EventWeightModifier
    }
    
    public enum ModifierOp
    {
        AddPercent,
        AddFlat
    }
    
    public enum ConditionType
    {
        MoraleCompare,
        StressCompare,
        DeadlineRemainingPct,
        ProjectPhaseEquals,
        MultitaskingState,
        TeamSizeCompare,
        UsesNewTech,
        UsesOldTech,
        ProjectDayIndex,
        IsProjectLead,
        PairedWithHigherSkilledTeammate,
        CompanyRevenueTrend,
        PromotionState,
        TenureYearsCompare,
        ProjectComplexity,
        All,
        Any
    }
    
    public enum ComparisonOp
    {
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Equal
    }
    
    public enum MultitaskingState
    {
        Single,
        Multi
    }
    
    public enum ProjectPhase
    {
        Implementation,
        BugFix,
        Polish
    }
    
    public enum EventCategory
    {
        Conflict,
        RaiseRequest,
        JobOffer,
        Mentorship,
        StressSpike,
        Praise,
        MarketBuzz
    }
}
