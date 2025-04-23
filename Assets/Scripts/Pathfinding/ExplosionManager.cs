#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    [SerializeField] private GameObject _firePrefab;
    [SerializeField] private float _explosionRadius = 5f;

    private static Dictionary<long, GameObject> _firePolys = new();
    [SerializeField] private List<Fire> _firePolyList = new();

    private void Awake()
    {
        foreach (Fire fire in _firePolyList)
        {
            _firePolys[fire.PolyRef] = fire.gameObject;
            Debug.Log( "new fire go: " + _firePolys[fire.PolyRef] );
        }
    }

    #if UNITY_EDITOR
    public void EditorBakeFirePolys()
    {
        _firePolys = new Dictionary<long, GameObject>();
        _firePolyList = new List<Fire>();

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Bake All Fire Objects");
        int group = Undo.GetCurrentGroup();

        Fire[] fireChildren = GetComponentsInChildren<Fire>();

        foreach(Fire fire in fireChildren)
        {
            Undo.DestroyObjectImmediate(fire.gameObject);
            DestroyImmediate(fire.gameObject);
        }

        for (int tileIndex = 0; tileIndex < DRcHandle.NavMeshData.GetMaxTiles(); tileIndex++)
        {
            var tile = DRcHandle.NavMeshData.GetTile(tileIndex);
            if (tile == null || tile.data == null) continue;

            for (int polyIndex = 0; polyIndex < tile.data.header.polyCount; polyIndex++)
            {
                var poly = tile.data.polys[polyIndex];
                if (poly.GetPolyType() != DtPolyTypes.DT_POLYTYPE_GROUND)
                    continue;

                long polyRef = DRcHandle.NavMeshData.GetPolyRefBase(tile) | (uint)polyIndex;

                if (_firePolys.ContainsKey(polyRef))
                    continue;

                GameObject fire = NewFire(polyRef);
                fire.transform.parent = this.transform; // keep hierarchy clean
                fire.name = $"Fire_{polyRef}";
                _firePolys[polyRef] = fire;
            }
        }

        Undo.CollapseUndoOperations(group);
    }

    public GameObject NewFire(long polyRef)
    {
        GameObject newFire =  Instantiate(_firePrefab);

        newFire.transform.position =
            DRcHandle.ToUnityVec3( DRcHandle.NavMeshData.GetPolyCenter(polyRef) );

        DRcHandle.NavMeshData.GetTileAndPolyByRef(polyRef, out DtMeshTile tile, out DtPoly poly);
        Fire fire = newFire.GetComponent<Fire>();

        IList<long> neighborRefs = new List<long>();

        for (int edge = 0; edge < poly.vertCount; edge++)
        {
            int nei = poly.neis[edge];

            if (nei != 0 && (nei & 0x8000) == 0) // internal neis
            {
                int neighborIndex = nei - 1;
                long neighborRef = DRcHandle.NavMeshData.GetPolyRefBase(tile) | (uint)neighborIndex;
                neighborRefs.Add(neighborRef);
            }
            else if (nei != 0) // external neis
            {
                int linkIdx = poly.firstLink;
                DtLink link = tile.links[linkIdx];
                if (link.edge == edge && link.refs != 0)
                    neighborRefs.Add(link.refs);
            }
        }

        fire.SetNeighborReferences(neighborRefs.ToArray());

        Undo.RegisterCreatedObjectUndo(newFire, "Bake Fire");

        _firePolys[polyRef] = newFire;
        newFire.SetActive(false);

        fire.PolyRef = polyRef;
        _firePolyList.Add(fire);

        return newFire;
    }
    #endif

    private void Update()
    {
        float per = Random.Range(0f, 1f);
        if ( per < 0.005f )
        {
            int random = (int) Random.Range(0, _firePolys.Count -1);

            Debug.Log("firepolys count: " + _firePolys.Count);

            Vector3 pos = _firePolys.ElementAt(random).Value.transform.position;

            List<long> resultRefs = PolysInCircle( pos, _explosionRadius + per);

            foreach ( long fireRef in resultRefs )
                if ( Random.Range(0f, 1f) < 0.6f )
                    SetFire( fireRef );
        }
    }

    public bool LookForFire(Vector3 position, float radius, out RcVec3f fireSource)
    {
        fireSource = RcVec3f.Zero;

        List<long> resultRefs = PolysInCircle(position, radius);

        foreach ( long polyRef in resultRefs )
            if ( PolyHasFire(polyRef) )
            {
                fireSource = DRcHandle.NavMeshData.GetPolyCenter(polyRef);
                return true;
            }

        return false;
    }

    public List<long> PolysInCircle(Vector3 position, float radius)
    {
        DtStatus status = DRcHandle.FindNearest(DRcHandle.ToDotVec3(position), out long startRef, out RcVec3f nearest, out _);

        if ( status.Failed() )
            return null;

        List<long> resultRefs = new List<long>();
        List<long> resultParents = new List<long>();
        List<float> resultCosts = new List<float>();

        status = DRcHandle.NavQuery.FindPolysAroundCircle (
            startRef,
            nearest,
            radius,
            DRcHandle.Filter,
            ref resultRefs,
            ref resultParents,
            ref resultCosts
        );

        if ( status.Failed() )
            return null;

        return resultRefs;
    }

    public static bool PolyHasFire(long polyRef)
    {
        if ( _firePolys.TryGetValue(polyRef, out var fire) && fire != null )
        {
            if ( fire.activeSelf )
                return true;
            return false;
        }

        Debug.LogWarning("Poly not already baked, polyRef: " + polyRef);
        return false;
    }

    public static bool SetFire(long polyRef)
    {
        if ( _firePolys.TryGetValue(polyRef, out var fire) && fire != null )
        {
            if ( fire.activeSelf )
                return false;
            
            fire.SetActive(true);
            return true;
        }

        Debug.LogWarning("Fire not already baked, polyRef: " + polyRef);
        return true;
    }
}
