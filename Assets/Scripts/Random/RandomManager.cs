using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Random
{
    public class RandomManager : Manager
    {
        [field:SerializeField] public int BaseSeed { get; private set; } = 12345;

        [SerializeReference] private List<ISeedRandom> _streams;

        public static RandomManager Instance { get; private set; }

        internal protected override void AwakeOrdered()
        {
            _streams = new List<ISeedRandom>();

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public (int, System.Random) RegisterStream(ISeedRandom stream)
        {
            // int ID = stream.Owner.transform.hierarchyCapacity ^ stream.Owner.transform.GetSiblingIndex();
            int ID = stream.Owner.transform.hierarchyCapacity;

            Transform t = stream.Owner.transform;
            string path = t.name;

            while ( t.parent != null )
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            foreach (char c in path)
                ID = ID * 31 + c;

            _streams.Add(stream);
        
            return (ID, new System.Random(BaseSeed ^ stream.ID));
        }

        internal protected override void UpdateOrdered() {}

        internal protected override void Bake() {}
    }
}