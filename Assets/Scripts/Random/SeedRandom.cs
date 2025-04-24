using System;
using UnityEngine;

namespace Scripts.Random
{
    [Serializable]
    public class SeedRandom : ISeedRandom
    {
        public GameObject Owner { get; private set; }
        public int ID { get; private set; }
        public System.Random Random { get; private set; }
        public SeedRandom(GameObject owner)
        {
            Owner = owner;
            (ID, Random) = RandomManager.Instance.RegisterStream(this);
            Debug.Log("Registered new stream: " + this);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Next(minInclusive, maxExclusive);
        }

        public float Range(float minInclusive, float maxExclusive)
        {
            return (float)(Random.NextDouble() * (maxExclusive - minInclusive) + minInclusive);
        }

        public override string ToString()
        {
            return $"{Owner?.name ?? "NoObject"} (ID: {ID})";
        }
    }
}