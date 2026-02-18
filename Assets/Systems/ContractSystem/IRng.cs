namespace TechMogul.Contracts
{
    public interface IRng
    {
        float Range(float min, float max);
        int Range(int minInclusive, int maxExclusive);
    }
}
