using UnityEngine;

namespace Scripts.Random
{
    /// <summary>
    /// Interface for a random number generator tied to a specific GameObject and seeded with an ID.
    /// Provides basic random and triangular-distributed number generation.
    /// </summary>
    public interface ISeedRandom
    {
        public GameObject Owner { get; }
        public int ID { get; }
        public System.Random Random { get; }

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (exclusive).
        /// </summary>
        /// <param name="minInclusive">The inclusive minimum bound.</param>
        /// <param name="maxExclusive">The exclusive maximum bound.</param>
        /// <returns>A random integer between the given bounds.</returns>
        public int Range(int minInclusive, int maxExclusive);

        /// <summary>
        /// Returns a random float between min (inclusive) and max (exclusive).
        /// </summary>
        /// <param name="minInclusive">The inclusive minimum bound.</param>
        /// <param name="maxExclusive">The exclusive maximum bound.</param>
        /// <returns>A random float between the given bounds.</returns>
        public float Range(float minInclusive, float maxExclusive);

        /// <summary>
        /// Returns a random integer using a triangular distribution between min and max.
        /// More likely to return values near the center.
        /// </summary>
        /// <param name="minInclusive">The inclusive minimum bound.</param>
        /// <param name="maxExclusive">The exclusive maximum bound.</param>
        /// <returns>A random integer skewed toward the center value.</returns>
        public int Triangular(int minInclusive, int maxExclusive);

        /// <summary>
        /// Returns a random float using a triangular distribution between min and max.
        /// More likely to return values near the center.
        /// </summary>
        /// <param name="minInclusive">The inclusive minimum bound.</param>
        /// <param name="maxExclusive">The exclusive maximum bound.</param>
        /// <returns>A random float skewed toward the center value.</returns>
        public float Triangular(float minInclusive, float maxExclusive);

        /// <summary>
        /// Returns a string representation of this random generator, usually showing owner and seed info.
        /// </summary>
        /// <returns>A human-readable string describing this random instance.</returns>
        public abstract string ToString();
    }
}