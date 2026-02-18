using UnityEngine;

namespace TechMogul.Contracts
{
    public class UnityRng : IRng
    {
        public float Range(float min, float max)
        {
            return Random.Range(min, max);
        }
        
        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }
}
