using System;
using DotRecast.Core;
using UnityEngine;

namespace Scripts.Random
{
    [Serializable]
    public class RcSeedRandom : IRcRand, ISeedRandom
    {
        public GameObject Owner { get; private set; }
        public int ID { get; private set; }
        public System.Random Random { get; private set; }
        public RcSeedRandom(GameObject owner)
        {
            Owner = owner;
            (ID, Random) = RandomManager.Instance.RegisterStream(this);
            Debug.Log("Registered new RC stream: " + this);
        }

        public float Next()
        {
            return (float) Random.NextDouble();
        }

        public double NextDouble()
        {
            return Random.NextDouble();
        }

        public int NextInt32()
        {
            return Random.Next();
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

        public int Triangular(int minInclusive, int maxExclusive)
        {
            return (Range(minInclusive, maxExclusive) + Range(minInclusive, maxExclusive)) / 2;
        }

        public float Triangular(float minInclusive, float maxExclusive)
        {
            return (Range(minInclusive, maxExclusive) + Range(minInclusive, maxExclusive)) / 2;
        }
    }
}