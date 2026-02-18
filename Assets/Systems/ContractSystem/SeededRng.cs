using UnityEngine;

namespace TechMogul.Contracts
{
    public class SeededRng : IRng
    {
        private System.Random _random;
        
        public SeededRng(int seed)
        {
            _random = new System.Random(seed);
        }
        
        public float Range(float min, float max)
        {
            double normalized = _random.NextDouble();
            return min + (float)(normalized * (max - min));
        }
        
        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }
}
