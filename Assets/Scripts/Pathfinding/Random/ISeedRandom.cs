using UnityEngine;

public interface ISeedRandom
{
    public GameObject Owner { get; set; }
    public int ID { get; set; }
    public System.Random Random { get; set; }

    public int Range(int minInclusive, int maxExclusive);
    public float Range(float minInclusive, float maxExclusive);

    public abstract string ToString();
}