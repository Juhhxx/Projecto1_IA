using UnityEngine;
using UniRecast.Core;
using DotRecast.Detour;
using DotRecast.Core.Numerics;
using System.Collections.Generic;
using System;

public class DotRecastHandle : MonoBehaviour
{

    [SerializeField] private UniRcNavMeshSurface _navMesh;
    [SerializeField] private float _agentHeight = 2f, _agentRadius = 0.6f;
    [SerializeField] private Vector3 _snapBoxSize;
    private static RcVec3f _snapSize;
    private static RcVec3f _agentSize;
    private static DtNavMeshQuery _navQuery;
    public static IDtQueryFilter Filter { get; private set; }
    private const int MAX_POLYS = 256; // max steps a path can have
    private static DtFindPathOption _options;
    private static IDtQueryHeuristic _heuristic;
    private void Awake()
    {
        _snapSize = new RcVec3f(_snapBoxSize.x, _snapBoxSize.y, _snapBoxSize.z);
        _heuristic = new DtQueryHeuristic();
        _options = new DtFindPathOption( _heuristic, 0, float.MaxValue );

        _agentSize = new RcVec3f(_agentRadius, _agentHeight, _agentRadius);

        if ( _navMesh == null )
            _navMesh = FindFirstObjectByType<UniRcNavMeshSurface>();
        _navMesh.Bake();

        // _navQuery.GetAttachedNavMesh().

        navmesh = _navMesh.GetNavMeshData();
        _navQuery = new DtNavMeshQuery(navmesh);
        Debug.Log($"NavMesh has {navmesh.GetMaxTiles()} tiles");

        Filter = new DtQueryFilter();
    }

    public static DtStatus FindNearest(RcVec3f center, out long nearestRef, out RcVec3f nearestPt, out bool isOverPoly)
    {
        DtStatus result = _navQuery.FindNearestPoly(center, _snapSize, Filter, out nearestRef, out nearestPt, out isOverPoly);

        _lastQueryCenter.Add(center);
        _lastQueryExtent.Add(_snapSize);

        if (nearestRef == 0)
            Debug.LogWarning($"FindNearestPoly failed at {center}");

        return result;
    }

    // public DtStatus FindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter, ref List<long> path, DtFindPathOption fpo)
    // public virtual DtStatus FindStraightPath(RcVec3f startPos, RcVec3f endPos, List<long> path, int pathSize, Span<DtStraightPath> straightPath, out int straightPathCount, int maxStraightPath, int options)
    public static IList<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        RcVec3f startPos = ToDotVec3(start);
        RcVec3f endPos = ToDotVec3(end);

        if ( _navQuery.GetAttachedNavMesh() == null )
        {
            Debug.LogWarning("Nav mesh null");
            return null;
        }

        FindNearest(startPos, out var startRef, out var startNearest, out var _);
        FindNearest(endPos, out var endRef, out var endNearest, out var _);

        List<long> polyPath = new List<long>();
        DtStatus pathStatus = _navQuery.FindPath(startRef, endRef, startNearest, endNearest, Filter, ref polyPath, _options);

        if (pathStatus.Failed() || polyPath.Count == 0)
        {
            Debug.LogWarning("FindPath failed or produced no output.");
            return null;
        }

        Span<DtStraightPath> straightPath = stackalloc DtStraightPath[MAX_POLYS];
        DtStatus straightStatus = _navQuery.FindStraightPath(startNearest, endNearest, polyPath, polyPath.Count, straightPath, out int straightPathCount, MAX_POLYS, 0);

        if (straightStatus.Failed() || straightPathCount == 0)
        {
            Debug.LogWarning("FindStraightPath failed or produced no output.");
            return null;
        }

        IList<Vector3> waypoints = new List<Vector3>();
        for (int i = 0; i < straightPathCount; i++)
            waypoints.Add(ToUnityVec3(straightPath[i].pos));

        return waypoints;
    }

    public static List<RcVec3f> _lastQueryCenter = new();
    public static List<RcVec3f> _lastQueryExtent = new();
    public static DtNavMesh navmesh;
    private void OnDrawGizmos()
    {
        for ( int i = 0; i < _lastQueryCenter.Count; i++ )
        {
            Vector3 center = new(_lastQueryCenter[i].X, _lastQueryCenter[i].Y, _lastQueryCenter[i].Z);
            Vector3 size = new(_lastQueryExtent[i].X *2, _lastQueryExtent[i].Y *2, _lastQueryExtent[i].Z *2);

            Gizmos.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            Gizmos.DrawWireCube(center, size);
        }
    }

    public static Vector3 ToUnityVec3(RcVec3f vec)
    {
        // flip X, unity uses right-handed coordinate system but dotrecast uses left-handed
        return new Vector3(-vec.X, vec.Y, vec.Z);
    }
    public static RcVec3f ToDotVec3(Vector3 vec)
    {
        return new RcVec3f(-vec.x, vec.y, vec.z);
    }
}
