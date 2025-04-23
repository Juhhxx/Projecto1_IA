using System;
using UnityEngine;

[Serializable]
public class SeedRandom : ISeedRandom
{
    public GameObject Owner { get; set; }
    public int ID { get; set; }
    public System.Random Random { get; set; }

    public SeedRandom(GameObject owner)
    {
        Owner = owner;
        RandomManager.Instance.RegisterStream(this);
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