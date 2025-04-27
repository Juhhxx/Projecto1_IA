using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using Scripts.Fire;
using Scripts.Pathfinding.DotRecast;
using Scripts.Random;
using Scripts.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Pathfinding
{

    /// <summary>
    /// Manages crowd agents using DotRecast's navigation and simulation systems.
    /// Handles agent pooling, movement updates, panic states, and explosion events.
    /// </summary>
    public class DRCrowdManager : Manager
    {
        [SerializeField] private DRcHandle _handle;
        [field:SerializeField] public ExplosionManager Explosion { get; private set; }

        [SerializeField] private int _AgentsPerUpdateBatch = 500;
        [SerializeField] private int _maxAgents = 100;

        // for object pooling
        [SerializeField] private GameObject _agentPrefab;
        [SerializeField] private AgentStatsController[] _agents;
        private HashSet<AgentStatsController> _activeAgents;
        private HashSet<AgentStatsController> _inactiveAgents;

        public ISeedRandom Rand { get; private set; }

        private DtCrowd _crowd;
        private DtCrowdAgentParams _normalParams;
        private DtCrowdAgentParams _panicParams;
        private DtCrowdAgentParams _paralyzedParams;

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

        private int _updateCursor = 0;
        private static Dictionary<long, int> _polyAgentCounts;

        /// <summary>
        /// Initializes the crowd manager, agents, filters, and navigation handle.
        /// </summary>
        internal protected override void AwakeOrdered()
        {
            _activeAgents = new HashSet<AgentStatsController>();
            _inactiveAgents = _agents.ToHashSet();
            _polyAgentCounts = new Dictionary<long, int>(ExplosionManager.PolyNum);

            Rand = new SeedRandom(gameObject);

            if (_handle == null)
                _handle = FindFirstObjectByType<DRcHandle>();

            if (Explosion == null)
                Explosion = FindFirstObjectByType<ExplosionManager>();

            RegularFilter = new DtQueryRegularFilter(Explosion);
            PanicFilter = new DtQueryPanicFilter(Explosion);

            _crowd = new DtCrowd(
                new DtCrowdConfig(radius),
                _handle.NavMeshData,
                i => i == 0 ? RegularFilter : i == 1 ? PanicFilter : new DtQueryDefaultFilter()
            );

            foreach ( AgentStatsController agent in _agents )
                agent.AwakeOrdered();

            InitializeAgentParams();
        }

        /// <summary>
        /// Initializes agent movement parameters for normal, panic, and paralyzed states.
        /// </summary>
        private void InitializeAgentParams()
        {
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
                radius = radius,
                height = height,
                maxAcceleration = maxAcceleration,
                obstacleAvoidanceType = 0,
                maxSpeed = maxPanicSpeed,
                separationWeight = separationPanicWeight,
                collisionQueryRange = collisionPanicQueryRange,
                pathOptimizationRange = pathPanicOptimizationRange,
                updateFlags = _normalParams.updateFlags,
                queryFilterType = 1
            };

            _paralyzedParams = new DtCrowdAgentParams
            {
                radius = radius,
                height = height,
                maxAcceleration = 0,
                obstacleAvoidanceType = 0,
                maxSpeed = 0,
                separationWeight = separationPanicWeight,
                collisionQueryRange = collisionPanicQueryRange,
                pathOptimizationRange = pathPanicOptimizationRange,
                updateFlags = _normalParams.updateFlags,
                queryFilterType = 1
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

        /// <summary>
        /// Empty StartOrdered. Not used here.
        /// </summary>
        internal protected override void StartOrdered() {}

        /// <summary>
        /// Updates active agents in batches and spawns new ones if possible.
        /// Also updates the underlying DtCrowd.
        /// </summary>
        internal protected override void UpdateOrdered()
        {
            Profiler.BeginSample("DRC DRCrowdManager");

            SetAgentCount();

            if (_activeAgents.Count > 0)
            {
                int skip = _updateCursor % _activeAgents.Count;
                int updated = 0;

                foreach (AgentStatsController agent in _activeAgents)
                {
                    if (skip > 0)
                    {
                        skip--;
                        continue;
                    }

                    agent.UpdateOrdered();
                    updated++;
                    _updateCursor++;

                    if (updated >= _AgentsPerUpdateBatch)
                        break;
                }
            }

            if (Exit.AnyExitUnoccupied())
            {
                AgentStatsController agent = _inactiveAgents.FirstOrDefault();
                if (agent != null)
                {
                    agent.gameObject.SetActive(true);
                    agent.Activate();
                    _inactiveAgents.Remove(agent);
                    _activeAgents.Add(agent);
                }
            }

            _crowd.Update(Time.deltaTime, null);

            Profiler.EndSample();
        }

        /// <summary>
        /// Instantiates a new agent at a given position with a specified state (normal or panicked).
        /// </summary>
        public DtCrowdAgent AddAgent(Vector3 position, bool isPanicked)
        {
            RcVec3f rcVec = DRcHandle.ToDotVec3(position);
            return _crowd.AddAgent(rcVec, isPanicked ? _panicParams : _normalParams);
        }

        /// <summary>
        /// Removes an agent from the crowd simulation.
        /// </summary>
        public void RemoveAgent(DtCrowdAgent agentId)
        {
            _crowd.RemoveAgent(agentId);
        }

        /// <summary>
        /// Sets a new movement target for an agent.
        /// </summary>
        public void SetTarget(DtCrowdAgent agentId, long targetRef, RcVec3f targetPos)
        {   
            _crowd.RequestMoveTarget(agentId, targetRef, targetPos);
        }

        /// <summary>
        /// Snaps a position onto the navmesh.
        /// </summary>
        public Vector3 SnapToNavMesh(RcVec3f position)
        {
            DRcHandle.FindNearest(position, out _, out var nearest, out _);
            return  DRcHandle.ToUnityVec3(nearest);
        }

        /// <summary>
        /// Recalculates agent counts per navmesh polygon for checking overcrowding.
        /// </summary>
        private void SetAgentCount()
        {
            Profiler.BeginSample("Crowd GOOD SPOT");

            _polyAgentCounts.Clear();

            foreach (DtCrowdAgent agent in _crowd.GetActiveAgents())
            {
                long polyRef = agent.corridor.GetFirstPoly();
                if (polyRef == 0) continue;

                if (_polyAgentCounts.TryGetValue(polyRef, out int count))
                    _polyAgentCounts[polyRef] = count + 1;
                else
                    _polyAgentCounts[polyRef] = 1;
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Returns the current number of agents at a given polygon reference.
        /// </summary>
        public static int AgentCountAt(long polyRef)
        {
            if (_polyAgentCounts.TryGetValue(polyRef, out int count))
                return count;
            return 0;
        }

        /// <summary>
        /// Simulates an explosion event and triggers panic/paralyze states in agents.
        /// </summary>
        public void ExplosionAt(RcVec3f center, float deathRadius, float fearRadius, float panicRadius)
        {
            cnt = DRcHandle.ToUnityVec3(center);
            death = deathRadius;
            fear = fearRadius;
            panic = panicRadius;
            float deathRadiusSq = deathRadius * deathRadius;
            float fearRadiusSq = fearRadius * fearRadius;
            float panicRadiusSq = panicRadius * panicRadius;

            foreach (AgentStatsController agent in _activeAgents)
            {
                if ( agent.ID == null ) continue;

                float distSq =
                    (agent.ID.npos.X - center.X) * (agent.ID.npos.X - center.X) +
                    (agent.ID.npos.Z - center.Z) * (agent.ID.npos.Z - center.Z);

                if (distSq <= deathRadiusSq)
                    agent.Deactivate(); // agent.ExplosionRadius = 1;
                else if (distSq <= fearRadiusSq)
                    Paralyze(agent);
                else if (distSq <= panicRadiusSq)
                    Panic(agent);
            }
        }

        float death;
        float fear;
        float panic;
        Vector3 cnt;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(cnt, death);
            Gizmos.DrawWireSphere(cnt, fear);
            Gizmos.DrawWireSphere(cnt, panic);
        }

        /// <summary>
        /// warn given agent to paralyze and set their movement to paralyze params
        /// </summary>
        /// <param name="agent"></param>
        private void Paralyze(AgentStatsController agent)
        {
            Debug.Log("Paralyze1");
            agent.ExplosionRadius = 1;

            _crowd.UpdateAgentParameters(agent.ID, _paralyzedParams);
            _crowd.ResetMoveTarget(agent.ID);
        }

        /// <summary>
        /// Call after finished with paralyze, fire, or explosion range
        /// </summary>
        /// <param name="agent"></param>
        public void Panic(AgentStatsController agent) // TODO
        {
            Debug.Log("Panic set");
            // warn all in range agents to panic
            agent.ExplosionRadius = 2;

            _crowd.UpdateAgentParameters(agent.ID, _panicParams);
        }


        /// <summary>
        /// Only called on object pool activation, as long as agents dont exit panic paralyze and death states
        /// </summary>
        /// <param name="agentId"></param>
        public void SwitchToNormal(DtCrowdAgent agentId)
        {
            _crowd.UpdateAgentParameters(agentId, _normalParams);
        }

        /// <summary>
        /// Checks if fire or explosion exists near a given polygon.
        /// </summary>
        public bool CheckForPanic((RcVec3f, long) poly, float checkRadius)
        {
            if (LookForFire(poly, checkRadius))
                return true;

            float deathRadiusSq = checkRadius * checkRadius;
            float x = poly.Item1.X;
            float z = poly.Item1.Z;

            foreach (AgentStatsController agent in _activeAgents)
            {
                if (agent.ExplosionRadius != 1) continue;

                float distSq = (x - agent.ID.npos.X) * (x - agent.ID.npos.X) + (z - agent.ID.npos.Z) * (z - agent.ID.npos.Z);

                if (distSq <= deathRadiusSq)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if any polygon within a radius has fire.
        /// </summary>
        public bool LookForFire((RcVec3f, long) poly, float radius)
        {
            List<long> resultRefs = _handle.PolysInCircle(poly.Item2, poly.Item1, radius);

            foreach (long polyRef in resultRefs)
                if (Explosion.PolyHasFire(polyRef))
                    return true;

            return false;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only agent baking method. Creates all agent objects from prefab.
        /// </summary>
        internal protected override void Bake()
        {
            _agents = new AgentStatsController[_maxAgents];

            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0));

            for ( int i = 0; i < _maxAgents ; i++)
            {
                GameObject newAgent =  (GameObject) PrefabUtility.InstantiatePrefab (_agentPrefab, transform);

            
                if ( newAgent.TryGetComponent(out AgentStatsController dr) )
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