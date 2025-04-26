using System;
using DotRecast.Core;
using UnityEngine;

namespace Scripts.Random
{

    /// <summary>
    /// Random generator for DotRecast operations, tied to a specific GameObject and seeded for reproducibility.
    /// Implements both IRcRand and ISeedRandom interfaces.
    /// </summary>
    [Serializable]
    public class RcSeedRandom : IRcRand, ISeedRandom
    {
        public GameObject Owner { get; private set; }
        public int ID { get; private set; }
        public System.Random Random { get; private set; }

        /// <summary>
        /// Constructs a new seeded random generator linked to a specific GameObject.
        /// Automatically registers itself with the RandomManager.
        /// </summary>
        /// <param name="owner">The GameObject that owns this random generator.</param>
        public RcSeedRandom(GameObject owner)
        {
            Owner = owner;
            (ID, Random) = RandomManager.Instance.RegisterStream(this);
            Debug.Log("Registered new RC stream: " + this);
        }

        /// <summary>
        /// Returns a random float between 0 (inclusive) and 1 (exclusive).
        /// </summary>
        /// <returns>A float between 0 and 1.</returns>
        public float Next()
        {
            return (float) Random.NextDouble();
        }

        /// <summary>
        /// Returns a random double between 0 (inclusive) and 1 (exclusive).
        /// </summary>
        /// <returns>A double between 0 and 1.</returns>
        public double NextDouble()
        {
            return Random.NextDouble();
        }

        /// <summary>
        /// Returns a random 32-bit integer.
        /// </summary>
        /// <returns>A random integer.</returns>
        public int NextInt32()
        {
            return Random.Next();
        }

        /// <summary>
        /// Returns a random integer between the specified min (inclusive) and max (exclusive).
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A random integer between min and max.</returns>
        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Returns a random float between the specified min (inclusive) and max (exclusive).
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A random float between min and max.</returns>
        public float Range(float minInclusive, float maxExclusive)
        {
            return (float)(Random.NextDouble() * (maxExclusive - minInclusive) + minInclusive);
        }

        /// <summary>
        /// Returns a triangular-distributed random integer between the specified bounds.
        /// More likely to return values near the center.
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A triangular-distributed random integer.</returns>
        public int Triangular(int minInclusive, int maxExclusive)
        {
            // Average of two uniform random integers
            return (Range(minInclusive, maxExclusive) + Range(minInclusive, maxExclusive)) / 2;
        }

        /// <summary>
        /// Returns a triangular-distributed random float between the specified bounds.
        /// More likely to return values near the center.
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A triangular-distributed random float.</returns>
        public float Triangular(float minInclusive, float maxExclusive)
        {
            // Average of two uniform random floats
            return (Range(minInclusive, maxExclusive) + Range(minInclusive, maxExclusive)) / 2;
        }

        /// <summary>
        /// Returns a readable string representation of this random generator, showing owner and ID.
        /// </summary>
        /// <returns>A human-readable string for debugging purposes.</returns>
        public override string ToString()
        {
            return $"{Owner?.name ?? "NoObject"} (ID: {ID})";
        }
    }
}