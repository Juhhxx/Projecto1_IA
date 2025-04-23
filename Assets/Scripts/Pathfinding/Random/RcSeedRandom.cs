using System;
using DotRecast.Core;
using UnityEngine;

[Serializable]
public class RcSeedRandom : IRcRand, ISeedRandom
{
    public GameObject Owner { get; set; }
    public int ID { get; set; }
    public System.Random Random { get; set; }

    public RcSeedRandom(GameObject owner)
    {
        Owner = owner;
        RandomManager.Instance.RegisterStream(this);
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
}
