using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

public class FoodArea : Structure<FoodArea>
{
    [field:SerializeField] private Transform[] _tables;
    protected override void SetUpPoints()
    {
        _places = new (RcVec3f, long)[_tables.Length];

        for ( int i = 0; i < _tables.Length; i++ )
        {
            if ( DRcHandle.FindNearest(_tables[i].position, out long polyRef, out RcVec3f polyPos, out _).Succeeded() &&  polyRef  != 0 )
                _places[i] = (polyPos, polyRef);
        }

        foreach ( (RcVec3f, long) pos in _places )
            _positions.Add( DRcHandle.ToUnityVec3( pos.Item1) );
    }
}
