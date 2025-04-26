using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Random
{
    /// <summary>
    /// Manages random number streams seeded uniquely per object.
    /// Ensures consistent random behavior across the project.
    /// </summary>
    public class RandomManager : Manager
    {
        [field:SerializeField] public int BaseSeed { get; private set; } = 12345;

        [SerializeReference] private List<ISeedRandom> _streams;

        public static RandomManager Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance and prepares the streams list.
        /// Called in an ordered initialization phase.
        /// </summary>
        internal protected override void AwakeOrdered()
        {
            _streams = new List<ISeedRandom>();

            // destroy any duplicate manager
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// No setup needed at StartOrdered phase for this manager.
        /// </summary>
        internal protected override void StartOrdered() {}

        /// <summary>
        /// Registers a new random stream by generating a unique ID based on the owner's hierarchy path.
        /// </summary>
        /// <param name="stream">The random stream to register.</param>
        /// <returns>
        /// A tuple containing:
        /// - The generated unique ID.
        /// - A System.Random instance seeded using the BaseSeed and the generated ID.
        /// </returns>
        public (int, System.Random) RegisterStream(ISeedRandom stream)
        {
            // int ID = stream.Owner.transform.hierarchyCapacity ^ stream.Owner.transform.GetSiblingIndex();
            int ID = stream.Owner.transform.hierarchyCapacity;

            // Build a full path from hierarchy to generate a unique ID, should maintain between sessions and systems
            Transform t = stream.Owner.transform;
            string path = t.name;

            while ( t.parent != null )
            {
                t = t.parent;
                path = t.name + "/" + path;
            }

            // Combine the path into the ID using prime number 31
            foreach (char c in path)
                ID = ID * 31 + c;

            _streams.Add(stream);
        
            // Create a seeded Random based on the BaseSeed XOR the unique ID
            return (ID, new System.Random(BaseSeed ^ stream.ID));
        }

        /// <summary>
        /// No updates needed per frame for this manager.
        /// </summary>
        internal protected override void UpdateOrdered() {}

        /// <summary>
        /// No baking operation needed for this manager.
        /// </summary>
        internal protected override void Bake() {}
    }
}