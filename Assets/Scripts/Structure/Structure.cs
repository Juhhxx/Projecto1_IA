using System.Collections.Generic;
using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using Scripts.Random;
using UnityEngine;


namespace Scripts.Structure
{
    /// <summary>
    /// Abstract class defining a Structure that agents may want to interact with.
    /// Subclasses represent specific structure types.
    /// </summary>
    /// <typeparam name="T">The type of structure inheriting from this class.</typeparam>
    public abstract class Structure<T> : MonoBehaviour where T : Structure<T>
    {
        [SerializeField] private float _radius = 10f;
        [SerializeField] private Transform _pivot;
        [SerializeField] private int _tooManyAgents = 5;
        [SerializeField] private int _tries = 3;

        // one structure list per subclass
        protected static List<T> _structures = new List<T>();
        protected (RcVec3f, long)[] _places;
        private Dictionary<long, int> _placeDict;

        public RcVec3f Position { get; private set; }
        public long Ref { get; private set; }

        protected ISeedRandom _rand;

        /// <summary>
        /// Initializes all structure instances at once.
        /// Needs to be called in a defined order externally, DRcHandle
        /// </summary>
        public static void AwakeOrdered()
        {
            foreach (T structure in _structures)
                structure.StartStructure();
        }

        /// <summary>
        /// Regular Mono behavior awake, runs before any other Mono behavior per project settings.
        /// </summary>
        private void Awake()
        {
            if (this is T type)
                _structures.Add(type);
        }

        /// <summary>
        /// Sets up structure specif reference points that agents will swarm to.
        /// </summary>
        private void StartStructure()
        {
            _rand = new SeedRandom(gameObject);
            _placeDict = new Dictionary<long, int>();

            DRcHandle.FindNearest(_pivot.position, out long nearestRef, out RcVec3f nearestPt, out _);
            Ref = nearestRef;
            Position = nearestPt;

            SetUpPoints();

            foreach ( (RcVec3f, long) y in _places )
                _placeDict[y.Item2] = 0;
        }

        /// <summary>
        /// Finds the nearest structure of type T to a given position.
        /// </summary>
        /// <param name="pos">The world position to search from.</param>
        /// <returns>The closest structure of type T.</returns>
        public static T FindNearest(RcVec3f pos, Structure<T> avoid = null)
        {
            if (_structures.Count == 0)
            {
                Debug.LogWarning("No structures of this type yet. ");
                return null;
            }

            T chosen = _structures[0];
            float minDist = RcVec3f.Distance(pos, chosen.Position);
            float dist;

            foreach (T structure in _structures)
            {
                if ( structure == avoid ) continue;

                dist = RcVec3f.Distance(pos, structure.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    chosen = structure;
                }
            }

            return chosen;
        }

        /// <summary>
        /// Sets up specific sample points for the structure.
        /// Must be implemented by each subclass.
        /// </summary>
        protected abstract void SetUpPoints();

        /// <summary>
        /// Checks whether a given position has entered this structure's zone.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>True if inside the structure radius; otherwise, false.</returns>
        public bool EnteredArea(RcVec3f pos)
        {
            return RcVec3f.Distance(pos, Position) < _radius;
        }

        /// <summary>
        /// Attempts to find a good spot for an agent to move to.
        /// If no good spot is found after several tries, selects a closer structure.
        /// </summary>
        /// <param name="pos">Current agent position.</param>
        /// <param name="structure">Reference to the current structure, which may be updated.</param>
        /// <returns>Tuple containing the selected position and its polygon reference.</returns>
        public static (RcVec3f, long) GetBestSpot(RcVec3f pos, Structure<T> structure, out Structure<T> newStruct)
        {
            newStruct = null;

            // Pick a random spot using triangular distribution
            (RcVec3f, long) next =
                structure._places[ structure._rand.Triangular(0, structure._places.Length) ];

            Debug.DrawLine(DRcHandle.ToUnityVec3(pos), DRcHandle.ToUnityVec3(next.Item1));

            for ( int i = 0; i < structure._tries ; i++ )
            {
                if ( structure.IsGoodSpot(next.Item2) ) // return if found good spot
                    return next;
                next = structure._places[ structure._rand.Triangular(0, structure._places.Length) ];
            }

            // If no good spot found, try finding a better structure
            newStruct = FindNearest(pos, structure);

            return (structure.Position, structure.Ref);
        }

        /// <summary>
        /// Checks if a navmesh polygon has space available for more agents.
        /// </summary>
        /// <param name="polyRef">The polygon reference ID to check.</param>
        /// <returns>True if the number of agents is below the limit; otherwise, false.</returns>
        public bool IsGoodSpot(long polyRef)
        {
            return  DRCrowdManager.AgentCountAt(polyRef) < _tooManyAgents;
        }

        /// <summary>
        /// Returns the name of the structure for logging or debugging purposes.
        /// </summary>
        /// <returns>The structure's name as a string.</returns>
        public override string ToString()
        {
            return $"Structure {gameObject.name}";
        }
    }
}