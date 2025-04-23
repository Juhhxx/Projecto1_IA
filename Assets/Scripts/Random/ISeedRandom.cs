using UnityEngine;

namespace Scripts.Random
{
    public interface ISeedRandom
    {
        public GameObject Owner { get; }
        public int ID { get; }
        public System.Random Random { get; }

        public int Range(int minInclusive, int maxExclusive);
        public float Range(float minInclusive, float maxExclusive);

        public abstract string ToString();
    }
}