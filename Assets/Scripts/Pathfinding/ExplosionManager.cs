using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using UnityEngine;
using System.Collections;
using DotRecast.Core;

public class ExplosionManager : MonoBehaviour
{
    [SerializeField] private GameObject _firePrefab;
    [SerializeField] private GameObject _explosionObject;
    [SerializeField] private float _radius = 20f, _radiusDeviation = 3f, _fireChance = 0.5f;
    [SerializeField] private int _RunFireEveryFrame = 2;
    [SerializeField] private float _explosionChancePerFrame = 0.001f;

    private static Dictionary<long, GameObject> _firePolys = new();
    [SerializeField] private List<Fire> _firePolyList = new();

    
    public static ISeedRandom Rand { get; private set; }

    private void Awake()
    {
        Rand = new RcSeedRandom(gameObject);
        Debug.Log("This is RcRand: " + Rand.Range(0f, 40f));

        Debug.Log("Poly list count: " + _firePolyList.Count);
        foreach (Fire fire in _firePolyList)
        {
            _firePolys[fire.PolyRef] = fire.gameObject;
            // Debug.Log( "new fire go: " + _firePolys[fire.PolyRef] );
        }
        Debug.Log("Poly dict count: " + _firePolys.Count);
    }

    #if UNITY_EDITOR
    public void EditorBakeFirePolys()
    {
        _firePolys = new Dictionary<long, GameObject>();
        _firePolyList = new List<Fire>();

        /*Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Bake All Fire Objects");
        int group = Undo.GetCurrentGroup();

        Fire[] fireChildren = GetComponentsInChildren<Fire>(true); // true for include inactive

        foreach(Fire fire in fireChildren)
            Undo.DestroyObjectImmediate(fire.gameObject);*/

        Debug.Log("Init tile count: " + DRcHandle.NavMeshData.GetTileCount());

        for (int tileIndex = 0; tileIndex < DRcHandle.NavMeshData.GetTileCount(); tileIndex++)
        {
            DtMeshTile tile = DRcHandle.NavMeshData.GetTile(tileIndex);
            if (tile == null || tile.data == null) continue;

            for (int polyIndex = 0; polyIndex < tile.data.header.polyCount; polyIndex++)
            {
                long polyRef = DRcHandle.NavMeshData.GetPolyRefBase(tile) | (uint)polyIndex;
                NewFirePoly(polyRef);
            }
        }

        Debug.Log("Final poly count: " + _firePolyList.Count);

        // Undo.CollapseUndoOperations(group);
    }

    public void NewFire(long polyRef)
    {
        GameObject newFire =  Instantiate(_firePrefab, transform); // keep hierarchy clean
        
        newFire.name = $"Fire_{polyRef}";

        newFire.transform.position =
            DRcHandle.ToUnityVec3( DRcHandle.NavMeshData.GetPolyCenter(polyRef) ); // we should give it a bit of wiggle here

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
                for (int linkIdx = poly.firstLink; linkIdx != -1; linkIdx = tile.links[linkIdx].next)
                {
                    long neighborRef = tile.links[linkIdx].refs;
                    if ( neighborRef != 0 )
                        neighborRefs.Add(neighborRef);
                }
            }
        }

        fire.SetRefs(polyRef, neighborRefs.ToArray());
        
        // Debug.Log("polyref: " + fire.PolyRef);

        _firePolys[polyRef] = newFire;
        _firePolyList.Add(fire);

        foreach (long p in neighborRefs)
            NewFirePoly(p);

        newFire.SetActive(false);

        // Undo.RegisterCreatedObjectUndo(newFire, "Bake Fire");
    }

    private void NewFirePoly(long polyRef)
    {
        if (_firePolys.ContainsKey(polyRef))
            return;
        NewFire(polyRef);
    }
    #endif

    private void Update()
    {
        // run fire update only every _RunFireEveryFrame frames
        if (Time.frameCount % _RunFireEveryFrame != 0)
            return;

        if ( Rand.Range(0f, 1f) < _explosionChancePerFrame )
        {
            // Debug.Log("firepolys count: " + _firePolys.Count);

            DRcHandle.NavQuery.FindRandomPoint(DRcHandle.Filter, Rand as IRcRand, out long polyRef, out RcVec3f centerPos);

            float radius = _radius + Rand.Range(-_radiusDeviation, _radiusDeviation);;
            List<long> resultRefs = PolysInCircle( polyRef, centerPos, radius);
            StartCoroutine(ExplodeAt(_firePolys[polyRef].transform.position, radius));

            foreach ( long fireRef in resultRefs )
            {
                Debug.DrawLine(_firePolys[polyRef].transform.position, _firePolys[fireRef].transform.position, Color.yellow, 5f);
                if ( Rand.Range(0f, 1f) < _fireChance )
                    SetFire( fireRef );
            }
        }

        foreach ( Fire fire in _firePolyList )
            if ( fire.gameObject.activeSelf )
                fire.OrderedUpdate();
    }

    private IEnumerator ExplodeAt(Vector3 pos, float radius)
    {
        _explosionObject.SetActive(true);
        _explosionObject.transform.position = pos;
        radius *= 2;

        YieldInstruction wfs = new WaitForSeconds(0.01f);

        for ( int i = 0; i <= radius ; i++ )
        {
            _explosionObject.transform.localScale = Vector3.one * i;
            yield return wfs;
        }

        for ( int i = 0; i < radius ; i++ )
        {
            _explosionObject.transform.localScale = Vector3.one * (radius - i);
            yield return null;
        }

        _explosionObject.SetActive(false);
    }

    public bool LookForFire(RcVec3f position, float radius, out RcVec3f fireSource)
    {
        fireSource = RcVec3f.Zero;

        DtStatus status = DRcHandle.FindNearest(position, out long startRef, out RcVec3f nearest, out _);
        if ( status.Failed() )
            return false;

        List<long> resultRefs = PolysInCircle(startRef, nearest, radius);

        foreach ( long polyRef in resultRefs )
            if ( PolyHasFire(polyRef) )
            {
                fireSource = DRcHandle.NavMeshData.GetPolyCenter(polyRef);
                return true;
            }

        return false;
    }

    public List<long> PolysInCircle(long startRef, RcVec3f nearest, float radius)
    {
        List<long> resultRefs = new List<long>();
        List<long> resultParents = new List<long>();
        List<float> resultCosts = new List<float>();

        DtStatus status = DRcHandle.NavQuery.FindPolysAroundCircle (
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
        return false;
    }
}
