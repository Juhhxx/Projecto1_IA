using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

public class Exit : Structure<Exit>
{
    [field:SerializeField] private Transform[] _exits;
    protected override void SetUpPoints()
    {
        _places = new (RcVec3f, long)[_exits.Length];

        for ( int i = 0; i < _exits.Length; i++ )
        {
            if ( DRcHandle.FindNearest(_exits[i].position, out long polyRef, out RcVec3f polyPos, out _).Succeeded() &&  polyRef  != 0 )
                _places[i] = (polyPos, polyRef);
        }

        foreach ( (RcVec3f, long) pos in _places )
            _positions.Add( DRcHandle.ToUnityVec3( pos.Item1) );
    }

    public static Exit GetRandomGoodExit(int random, out (RcVec3f, long) pos)
    {
        if ( _structures == null || _structures.Count == 0 )
            Debug.LogWarning("Exit structure not init. ");

        int e = random % _structures.Count;
        Exit cur = _structures[e];
        
        e = random / _structures.Count % cur._places.Length;
        pos = cur._places[e];

        if ( cur._places == null || _structures.Count == 0 )
            Debug.LogWarning("Exit does not have exits yet. ");
        return cur;
    }

    public static bool AnyExitUnoccupied()
    {
        foreach ( Exit structure in _structures )
            foreach ( (RcVec3f, long) place in structure._places)
                if ( structure.IsGoodSpot(place.Item2) )
                    return true;
        
        return false;
    }
}

        /*for ( int offset1 = 0; offset1 < _structures.Count; offset1++ )
        {
            int e = (random + offset1) % _structures.Count;
            cur = _structures[e];

            for ( int offset2 = 0; offset2 < cur._places.Length; offset2++ )
            {
                e = (random + offset2) % cur._places.Length;
                pos = cur._places[e];

                if ( cur.IsGoodSpot( pos.Item2 ) )
                    return cur;
            }
        }*/