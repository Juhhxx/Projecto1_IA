#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UniRecast.Core;
using DotRecast.Detour;
using DotRecast.Core.Numerics;
using System.Collections.Generic;
using System;
using System.IO;
using DotRecast.Detour.Io;
using DotRecast.Core;
using Scripts.Pathfinding.DotRecast;
using UnityEngine.Profiling;

namespace Scripts.Pathfinding
{
    public class DRcHandle : Manager
    {

        [SerializeField] private UniRcNavMeshSurface _navMesh;
        [SerializeField] private float _agentHeight = 2f, _agentRadius = 0.6f;
        [SerializeField] private Vector3 _snapBoxSize;

        [SerializeField] private Manager[] _managers;

        private static RcVec3f _snapSize;
        private static RcVec3f _agentSize;

        public static DtNavMeshQuery NavQuery { get; private set; }
        public DtNavMesh NavMeshData { get; private set; }

        public static IDtQueryFilter Filter { get; private set; }

        private const int MAX_POLYS = 256; // max steps a path can have
        private static DtFindPathOption _options;
        private static IDtQueryHeuristic _heuristic;

        private void Awake()
        {
            _snapSize = new RcVec3f(_snapBoxSize.x, _snapBoxSize.y, _snapBoxSize.z);
            _agentSize = new RcVec3f(_agentRadius, _agentHeight, _agentRadius);

            _heuristic = new DtQueryRegularHeuristic();
            _options = new DtFindPathOption(_heuristic, 0, float.MaxValue);

            if (_navMesh == null)
                _navMesh = FindFirstObjectByType<UniRcNavMeshSurface>();

            NavMeshData = LoadNavMeshFromFile(gameObject.scene.name);
            
            if ( NavMeshData == null )
            {
                Debug.Log("Nav mesh data was null. ");
                _navMesh.Bake();
                NavMeshData = _navMesh.GetNavMeshData();
            }
            

            NavQuery = new DtNavMeshQuery(NavMeshData);
            Filter = new DtQueryDefaultFilter();

            foreach ( Manager m in _managers )
                m.AwakeOrdered();
        }

        private DtNavMesh LoadNavMeshFromFile(string fileName)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", fileName + ".bytes");

            if ( !File.Exists(fullPath) )
            {
                Debug.LogError("Saved navmesh not found: " + fullPath);
                return null;
            }

            byte[] navMeshBytes = File.ReadAllBytes(fullPath);
            RcByteBuffer bb = new RcByteBuffer(navMeshBytes);
            bb.Order(RcByteOrder.BIG_ENDIAN);

            return new DtMeshSetReader().Read(bb, 5);
        }

        #if UNITY_EDITOR
        [ContextMenu("Bake NavMesh and Fire Polys Into Scene")]
        public void EditorBake()
        {
            if (_navMesh == null)
            {
                _navMesh = GetComponent<UniRcNavMeshSurface>();
                if (_navMesh == null)
                {
                    Debug.LogError("UniRcNavMeshSurface not assigned!");
                    return;
                }
            }

            _navMesh.Bake();

            NavMeshData = _navMesh.GetNavMeshData();
            NavQuery = new DtNavMeshQuery(NavMeshData);
            Filter = new DtQueryDefaultFilter();

            foreach ( Manager m in _managers )
                m.Bake();

            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
        #endif

        private void Update()
        {
            Profiler.BeginSample("DRC HANDLE");

            foreach ( Manager m in _managers )
                m.UpdateOrdered();
        }

        public static DtStatus FindNearest(RcVec3f center, out long nearestRef, out RcVec3f nearestPt, out bool isOverPoly)
        {
            DtStatus result = NavQuery.FindNearestPoly(center, _snapSize, Filter, out nearestRef, out nearestPt, out isOverPoly);

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

            if ( NavQuery.GetAttachedNavMesh() == null )
            {
                Debug.LogWarning("Nav mesh null");
                return null;
            }

            FindNearest(startPos, out var startRef, out var startNearest, out var _);
            FindNearest(endPos, out var endRef, out var endNearest, out var _);

            List<long> polyPath = new List<long>(MAX_POLYS);
            DtStatus pathStatus = NavQuery.FindPath(startRef, endRef, startNearest, endNearest, Filter, ref polyPath, _options);

            if ( pathStatus.Failed() || polyPath.Count == 0 )
            {
                Debug.LogWarning("FindPath failed or produced no output.");
                return null;
            }

            Span<DtStraightPath> straightPath = stackalloc DtStraightPath[MAX_POLYS];
            DtStatus straightStatus = NavQuery.FindStraightPath(startNearest, endNearest, polyPath, polyPath.Count, straightPath, out int straightPathCount, MAX_POLYS, 0);

            if ( straightStatus.Failed() || straightPathCount == 0 )
            {
                Debug.LogWarning("FindStraightPath failed or produced no output.");
                return null;
            }

            IList<Vector3> waypoints = new List<Vector3>();
            for (int i = 0; i < straightPathCount; i++)
                waypoints.Add( ToUnityVec3(straightPath[i].pos) );

            return waypoints;
        }

        /// <summary>
        /// flip X, unity uses right-handed coordinate system but dotrecast uses left-handed
        /// </summary>
        /// <param name="vec"> vector3 to translate to dt's coordinates </param>
        /// <returns> translated rc vec 3f </returns>
        public static Vector3 ToUnityVec3(RcVec3f vec) => new Vector3(-vec.X, vec.Y, vec.Z);
        public static RcVec3f ToDotVec3(Vector3 vec) => new RcVec3f(-vec.x, vec.y, vec.z);
        public static Quaternion ToDotQuat(RcVec3f vec)
        {
            Vector3 dir = ToUnityVec3(vec);
            return Quaternion.LookRotation(dir, Vector3.up);
        }

        internal protected override void AwakeOrdered() {}

        internal protected override void Bake() {}

        internal protected override void UpdateOrdered() {}
    }
}