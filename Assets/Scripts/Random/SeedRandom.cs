using System;
using UnityEngine;

namespace Scripts.Random
{
    /// <summary>
    /// General-purpose random generator tied to a specific GameObject.
    /// Provides uniform and triangular-distributed random numbers.
    /// </summary>
    [Serializable]
    public class SeedRandom : ISeedRandom
    {
        public GameObject Owner { get; private set; }
        public int ID { get; private set; }
        public System.Random Random { get; private set; }

        /// <summary>
        /// Creates a new seeded random generator linked to the specified GameObject.
        /// Automatically registers the generator with RandomManager.
        /// </summary>
        /// <param name="owner">The GameObject owning this random generator.</param>
        public SeedRandom(GameObject owner)
        {
            Owner = owner;
            (ID, Random) = RandomManager.Instance.RegisterStream(this);
            Debug.Log("Registered new stream: " + this);
        }

        /// <summary>
        /// Returns a random integer between the specified minimum (inclusive) and maximum (exclusive).
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A random integer within the range.</returns>
        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Returns a random float between the specified minimum (inclusive) and maximum (exclusive).
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A random float within the range.</returns>
        public float Range(float minInclusive, float maxExclusive)
        {
            return (float)(Random.NextDouble() * (maxExclusive - minInclusive) + minInclusive);
        }

        /// <summary>
        /// Returns a triangular-distributed random integer, favoring middle values between min and max.
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A random integer with triangular distribution.</returns>
        public int Triangular(int minInclusive, int maxExclusive)
        {
            // Average of two uniformly distributed integers to simulate a triangular distribution
            return (Range(minInclusive, maxExclusive) + Range(minInclusive, maxExclusive)) / 2;
        }

        /// <summary>
        /// Returns a triangular-distributed random float, favoring middle values between min and max.
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound.</param>
        /// <returns>A random float with triangular distribution.</returns>
        public float Triangular(float minInclusive, float maxExclusive)
        {
            // Average of two uniformly distributed floats to simulate a triangular distribution
            return (Range(minInclusive, maxExclusive) + Range(minInclusive, maxExclusive)) / 2;
        }

        /// <summary>
        /// Returns a readable string showing the owner GameObject name and ID.
        /// </summary>
        /// <returns>String representation of the random stream.</returns>
        public override string ToString()
        {
            return $"{Owner?.name ?? "NoObject"} (ID: {ID})";
        }
    }
}