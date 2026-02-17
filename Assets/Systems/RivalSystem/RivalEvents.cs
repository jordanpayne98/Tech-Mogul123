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
}
