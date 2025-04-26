using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

namespace Scripts.Structure
{
    /// <summary>
    /// Represents an Exit structure, where points are placed at each exit location.
    /// </summary>
    public class Exit : Structure<Exit>
    {
        [field:SerializeField] private Transform[] _exits;

        /// <summary>
        /// Sets up points at each exit's position within the Exit area.
        /// </summary>
        protected override void SetUpPoints()
        {
            _places = new (RcVec3f, long)[_exits.Length];

            for ( int i = 0; i < _exits.Length; i++ )
            {
                if ( DRcHandle.FindNearest(_exits[i].position, out long polyRef, out RcVec3f polyPos, out _).Succeeded() &&  polyRef  != 0 )
                    _places[i] = (polyPos, polyRef);
            }
        }

       /// <summary>
        /// Selects a random Exit and returns one of its sample points.
        /// </summary>
        /// <param name="random">A random integer used to pick an Exit and a specific point within it.</param>
        /// <param name="pos">Outputs the selected point's world position and navmesh polygon reference.</param>
        /// <returns>The randomly selected Exit structure, or null if none are available.</returns>
        public static Exit GetRandomGoodExit(int random, out (RcVec3f, long) pos)
        {
            if ( _structures == null || _structures.Count == 0 )
                Debug.LogWarning("Exit structure not init. ");

            int e = random % _structures.Count;
            Exit cur = _structures[e];
            
            e = random / _structures.Count % cur._places.Length;
            pos = cur._places[e];

            return cur;
        }

        /// <summary>
        /// Checks whether there is at least one unoccupied exit point available.
        /// </summary>
        /// <returns>True if any exit point is free and usable; otherwise, false.</returns>
        public static bool AnyExitUnoccupied()
        {
            foreach ( Exit structure in _structures )
                foreach ( (RcVec3f, long) place in structure._places)
                    if ( structure.IsGoodSpot(place.Item2) )
                        return true;
            
            return false;
        }
    }
}