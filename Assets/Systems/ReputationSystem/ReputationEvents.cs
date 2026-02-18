namespace TechMogul.Core
{
    public class OnReputationChangedEvent
    {
        public float newReputation;
        public float change;
        public int starRating;
    }
    
    public class RequestChangeReputationEvent
    {
        public float Amount;
    }
}
