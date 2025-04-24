using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using Scripts.Fire;
using Scripts.Pathfinding.DotRecast;
using UnityEngine;

namespace Scripts.Pathfinding
{
    public class DRCrowdManager : MonoBehaviour
    {
        [SerializeField] private DRcHandle _handle;
        // for object pooling
        [SerializeField] private int _maxAgents = 100;
        public float maxAgentRadius = 0.6f;

        private DtCrowd _crowd;
        private DtCrowdAgentParams _normalParams;
        private DtCrowdAgentParams _panicParams;

        [Header("Agent Regular Parameters")]
        [SerializeField] private float radius = 0.6f;
        [SerializeField] private float height = 2f;
        [SerializeField] private float maxAcceleration = 8f;
        [SerializeField] private float maxSpeed = 3.5f;
        [SerializeField] private float collisionQueryRange = 1.2f;
        [SerializeField] private float pathOptimizationRange = 30f;
        [SerializeField] private float separationWeight = 2f;

        [Header("Agent Panic Parameters")]
        [SerializeField] private float maxPanicSpeed = 3.5f;

        private IDtQueryFilter RegularFilter;
        private IDtQueryFilter PanicFilter;

        [SerializeField] private ExplosionManager _explosionManager;

        private void Awake()
        {
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
        }

        private void Update()
        {
            _crowd.Update(Time.deltaTime, null);
        }

        public DtCrowdAgent AddAgent(Vector3 position, bool isPanicked)
        {
            RcVec3f rcVec = DRcHandle.ToDotVec3(position);
            return _crowd.AddAgent(rcVec, isPanicked ? _panicParams : _normalParams);
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
    }
}