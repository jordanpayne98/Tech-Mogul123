namespace TechMogul.Systems
{
    public enum TechAdoptionPhase
    {
        Locked,        // Prerequisites not met or year < researchYear
        Research,      // A < 0.15
        EarlyAdoption, // 0.15 ≤ A < 0.40
        Growth,        // 0.40 ≤ A < 0.75
        Mainstream,    // 0.75 ≤ A < 0.90
        Mandatory      // A ≥ 0.90
    }
}
