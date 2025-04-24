using System.Collections.Generic;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using Scripts.Fire;
using Scripts.Pathfinding.DotRecast;
using Scripts.Random;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Pathfinding
{
    public class DRCrowdManager : Manager
    {
        [SerializeField] private DRcHandle _handle;
        [SerializeField] private ExplosionManager _explosionManager;

        [SerializeField] private int _AgentsPerUpdateBatch = 500;
        [SerializeField] private int _maxAgents = 100;

        // for object pooling
        [SerializeField] private GameObject _agentPrefab;
        [SerializeField] private DRAgent[] _agents;
        public float maxAgentRadius = 0.6f;
        [field:SerializeField] public Renderer BoundBox { get; private set; }

        public ISeedRandom Rand { get; private set; }

        private DtCrowd _crowd;
        private DtCrowdAgentParams _normalParams;
        private DtCrowdAgentParams _panicParams;

        [Header("Agent Regular Parameters")]
        // agent radius, this also affects avoidance
        [SerializeField] private float radius = 0.6f;
        [SerializeField] private float height = 2f;
        [SerializeField] private float maxAcceleration = 8f;
        [SerializeField] private float maxSpeed = 3.5f;
        // how much agents avoid each other
        [SerializeField] private float collisionQueryRange = 1.2f;
        // how far ahead agents try to think of their path ( like in pursue )
        [SerializeField] private float pathOptimizationRange = 30f;
        [SerializeField] private float separationWeight = 2f;

        [Header("Agent Panic Parameters")]
        [SerializeField] private float maxPanicSpeed = 3.5f;

        private IDtQueryFilter RegularFilter;
        private IDtQueryFilter PanicFilter;

        internal protected override void AwakeOrdered()
        {
            _AgentsPerUpdateBatch = _maxAgents / _AgentsPerUpdateBatch;

            Rand = new SeedRandom(gameObject);

            if ( _handle == null )
                _handle = FindFirstObjectByType<DRcHandle>();

            if ( _explosionManager == null )
                _explosionManager = FindFirstObjectByType<ExplosionManager>();

            RegularFilter = new DtQueryRegularFilter(_explosionManager);
            PanicFilter = new DtQueryPanicFilter(_explosionManager);


            _crowd = new DtCrowd(
                new DtCrowdConfig(maxAgentRadius),
                _handle.NavMeshData,
                i => {
                    if (i == 0) return RegularFilter;
                    if (i == 1) return PanicFilter;
                    return new DtQueryDefaultFilter();
                }
            );

            _normalParams = new DtCrowdAgentParams
            {
                radius = radius,
                height = height,
                maxAcceleration = maxAcceleration,
                maxSpeed = maxSpeed,
                collisionQueryRange = collisionQueryRange,
                pathOptimizationRange = pathOptimizationRange,
                updateFlags = 0,
                separationWeight = separationWeight,
                queryFilterType = 0
            };

            _panicParams = new DtCrowdAgentParams
            {
                radius = _normalParams.radius,
                height = _normalParams.height,
                maxAcceleration = _normalParams.maxAcceleration,
                collisionQueryRange = _normalParams.collisionQueryRange,
                pathOptimizationRange = _normalParams.pathOptimizationRange,
                updateFlags = _normalParams.updateFlags,
                separationWeight = _normalParams.separationWeight,
                queryFilterType = 1,
                maxSpeed = maxPanicSpeed
            };

            foreach ( DRAgent a in _agents )
            {
                a.gameObject.SetActive(true);
                a.StartOrdered();
            }
        }

        internal protected override void UpdateOrdered()
        {
            if ( Time.frameCount < 10 ) return;
            
            Profiler.BeginSample("DRC DRCrowdManager");

            _crowd.Update(Time.deltaTime, null);

            int offset = Time.frameCount % _AgentsPerUpdateBatch;

            for (int i = offset; i < _agents.Length; i += _AgentsPerUpdateBatch)
                _agents[i].UpdateOrdered();
        }

        public DtCrowdAgent AddAgent(Vector3 position, bool isPanicked)
        {
            RcVec3f rcVec = DRcHandle.ToDotVec3(position);
            return _crowd.AddAgent(rcVec, isPanicked ? _panicParams : _normalParams);
        }
        public void RemoveAgent(DtCrowdAgent agentId)
        {
            _crowd.RemoveAgent(agentId);
        }

        public void SetTarget(DtCrowdAgent agentId, Vector3 target)
        {
            RcVec3f rcVec = DRcHandle.ToDotVec3(target);
            DRcHandle.FindNearest(rcVec, out long nearestRef, out RcVec3f nearestPt, out bool _);

            _crowd.RequestMoveTarget(agentId, nearestRef, nearestPt);
        }

        public void SwitchToPanic(DtCrowdAgent agentId)
        {
            _crowd.UpdateAgentParameters(agentId, _panicParams);
        }

        public void SwitchToNormal(DtCrowdAgent agentId)
        {
            _crowd.UpdateAgentParameters(agentId, _normalParams);
        }

        public Vector3 SnapToNavMesh(Vector3 position)
        {
            RcVec3f rc = DRcHandle.ToDotVec3(position);
            DRcHandle.FindNearest(rc, out _, out var nearest, out _);

            return  DRcHandle.ToUnityVec3(nearest);
        }

        #if UNITY_EDITOR
        internal protected override void Bake()
        {
            _agents = new DRAgent[_maxAgents];

            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0));

            for ( int i = 0; i < _maxAgents ; i++)
            {
                GameObject newAgent =  (GameObject) PrefabUtility.InstantiatePrefab (_agentPrefab, transform);

            
                if ( newAgent.TryGetComponent(out DRAgent dr) )
                    _agents[i] = dr;
                else
                {
                    Debug.LogWarning("Fire prefab does not have agent component. ");
                    return;
                }

                dr.SetRefs(this);
                newAgent.SetActive(false);
            }
        }
        #endif
    }
}