using System.Linq;
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
        [SerializeField] private float pathPanicOptimizationRange = 30f;
        [SerializeField] private float collisionPanicQueryRange = 1.2f;
        [SerializeField] private float separationPanicWeight = 2f;

        private IDtQueryFilter RegularFilter;
        private IDtQueryFilter PanicFilter;

        internal protected override void AwakeOrdered()
        {
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
                updateFlags = DRCrowdUpdateFlags.DT_CROWD_OBSTACLE_AVOIDANCE
                            | DRCrowdUpdateFlags.DT_CROWD_ANTICIPATE_TURNS
                            | DRCrowdUpdateFlags.DT_CROWD_SEPARATION
                            | DRCrowdUpdateFlags.DT_CROWD_OPTIMIZE_VIS
                            | DRCrowdUpdateFlags.DT_CROWD_OPTIMIZE_TOPO,
                obstacleAvoidanceType = 0,
                separationWeight = separationWeight,
                queryFilterType = 0
            };

            _panicParams = new DtCrowdAgentParams
            {
                radius = _normalParams.radius,
                height = _normalParams.height,
                maxAcceleration = _normalParams.maxAcceleration,
                obstacleAvoidanceType = _normalParams.obstacleAvoidanceType,

                separationWeight = separationPanicWeight,
                collisionQueryRange = collisionPanicQueryRange,
                pathOptimizationRange = pathPanicOptimizationRange,
                updateFlags = _normalParams.updateFlags,
                queryFilterType = 1,
                maxSpeed = maxPanicSpeed
            };
        }

        public static class DRCrowdUpdateFlags
        {
            public const int DT_CROWD_ANTICIPATE_TURNS = 1 << 0;
            public const int DT_CROWD_OBSTACLE_AVOIDANCE = 1 << 1;
            public const int DT_CROWD_SEPARATION = 1 << 2;
            public const int DT_CROWD_OPTIMIZE_VIS = 1 << 3;
            public const int DT_CROWD_OPTIMIZE_TOPO = 1 << 4;
        }

        internal protected override void StartOrdered() {}

        internal protected override void UpdateOrdered()
        {
            Profiler.BeginSample("DRC DRCrowdManager");

            _crowd.Update(Time.deltaTime, null);

            int offset = Time.frameCount % _AgentsPerUpdateBatch;

            for (int i = offset; i < _agents.Length; i += _AgentsPerUpdateBatch)
                _agents[i].UpdateOrdered();


            if ( ! Exit.AnyExitUnoccupied() ) return;

            DRAgent agent = _agents.FirstOrDefault(t => !t.IsActive);

            if ( agent != null )
            {
                agent.gameObject.SetActive(true);
                agent.Activate();
            }
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

        public void SetTarget(DtCrowdAgent agentId, long targetRef, RcVec3f targetPos)
        {   
            _crowd.RequestMoveTarget(agentId, targetRef, targetPos);
        }

        public void SwitchToPanic(DtCrowdAgent agentId)
        {
            _crowd.UpdateAgentParameters(agentId, _panicParams);
        }

        public void SwitchToNormal(DtCrowdAgent agentId)
        {
            _crowd.UpdateAgentParameters(agentId, _normalParams);
        }

        public Vector3 SnapToNavMesh(RcVec3f position)
        {
            DRcHandle.FindNearest(position, out _, out var nearest, out _);

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