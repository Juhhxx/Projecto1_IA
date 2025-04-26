using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

namespace Scripts.Structure
{
    /// <summary>
    /// Represents a FoodArea where points are placed at each table's location.
    /// </summary>
    public class FoodArea : Structure<FoodArea>
    {
        [field:SerializeField] private Transform[] _tables;

        /// <summary>
        /// Sets up points at each table's position within the FoodArea.
        /// </summary>
        protected override void SetUpPoints()
        {
            _places = new (RcVec3f, long)[_tables.Length];

            for ( int i = 0; i < _tables.Length; i++ )
            {
                if ( DRcHandle.FindNearest(_tables[i].position, out long polyRef, out RcVec3f polyPos, out _).Succeeded() &&  polyRef  != 0 )
                    _places[i] = (polyPos, polyRef);
            }
        }
    }
}