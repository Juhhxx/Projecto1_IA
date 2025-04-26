using System;
using System.Collections;
using DotRecast.Core.Numerics;
using DotRecast.Detour.Crowd;
using Scripts.Pathfinding;
using Scripts.Random;
using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts
{
    public class AgentStatsController : MonoBehaviour
    {
        [Header("Movement Stats")]
        [Space(5f)]
        [SerializeField] private float _maxSpeed;

        private float _currentMaxSeed;

        [Space(10f)]
        [Header("Status Stats")]
        [Space(5f)]
        [SerializeField] private float _maxHunger;
        [SerializeField] private float _hungerDepleationRate;
        [SerializeField] private Color _hungryColor;
        [SerializeField] private float _maxEnergy;
        [SerializeField] private float _energyDepleationRate;
        [SerializeField] private Color _tiredColor;
        [SerializeField] private float _depleationSpeed;
        [SerializeField] private AgentStat _agentStat;

        [SerializeField] private float _acceptedDist = 2f;
        public float AcceptedDist => _acceptedDist;
        private DtCrowdAgent _agentID;
        public DtCrowdAgent ID => _agentID;
        [SerializeField] private DRCrowdManager _crowd;
        public DRCrowdManager Crowd => _crowd;

        public (RcVec3f Pos, long Ref) LastRef { get; set; }
        public (RcVec3f Pos, long Ref) CurRef { get; set; }
        public (RcVec3f Pos, long Ref) NextRef { get; set; }

        private float _hungerLevel;
        private float _energyLevel;
        public AgentStat AgentStat => _agentStat;
        public Color HungryColor => _hungryColor;
        public Color TiredColor => _tiredColor;
        private Coroutine _changeHunger;
        private Coroutine _changeEnergy;
        private Coroutine _updateStats;
        private YieldInstruction _wfsChange;
        private YieldInstruction _wfsUpdate;

        // Other Variables
        private Color _normalColor;
        public Color NormalColor => _normalColor;
        private ISeedRandom _random;
        private Renderer _renderer;
        private int _explosionRadius;
        public int ExplosionRadius
        {
            get => _explosionRadius;

            set
            {
                if (value <= 3) _explosionRadius = value;
                else            _explosionRadius = 3;
            }
        }

        private void Awake()
        {
            _renderer       = GetComponentInChildren<Renderer>();
            _normalColor    = _renderer.material.color;
            _random         = new SeedRandom(gameObject);
            _wfsChange      = new WaitForSeconds(_depleationSpeed);
            _wfsUpdate      = new WaitForSeconds(1f);
        }

        public void Activate()
        {
            ChooseRandomState();

            Exit.GetRandomGoodExit( _crowd.Rand.Range(0, 32), out (RcVec3f, long) pos);
            LastRef = pos;

            transform.position = _crowd.SnapToNavMesh(pos.Item1);;
            
            _agentID = _crowd.AddAgent(transform.position, false);
            _crowd.SwitchToNormal(_agentID);
        }

        private long _lastPolyRef = 0;
        private void Update()
        {
            long currPolyRef = _agentID.corridor.GetFirstPoly();

            if (currPolyRef != _lastPolyRef)
            {
                _lastPolyRef = currPolyRef;
                if ( _crowd.Explosion.PolyHasFire(_lastPolyRef) )
                    Deactivate();
            }
        }
        public void UpdateOrdered()
        {
            Profiler.BeginSample("DR Agent");

            if ( _agentID != null )
            {
                transform.position = DRcHandle.ToUnityVec3(_agentID.npos);
                if ( _agentID.vel.Length() > 0.1f )
                    transform.rotation = DRcHandle.ToDotQuat(_agentID.vel);
            }
        }

        private void Deactivate()
        {
            _crowd.RemoveAgent(_agentID);
            _agentID = null;
            
            gameObject.SetActive(false);
        }

        public void ChooseRandomState()
        {
            AgentStat startingStat;

            Array enumValues    = Enum.GetValues(typeof(AgentStat));
            int enumLengh       = enumValues.Length;
            startingStat        = (AgentStat)enumValues.GetValue(_random.Range(0, enumLengh));

            if (startingStat == AgentStat.Hungry)
            {
                _hungerLevel = _maxHunger/2;
                _energyLevel = _maxEnergy;
            }
            else if (startingStat == AgentStat.Tired)
            {
                _energyLevel = _maxEnergy/2;
                _hungerLevel = _maxHunger;
            }
            else
            {
                _hungerLevel = _maxHunger;
                _energyLevel = _maxEnergy;
                StartDepletingHunger();
                StartDepletingEnergy();
            }

            _agentStat = startingStat;
        }
        private IEnumerator ChangeHunger(bool subtract)
        {
            bool parameter;

            if (subtract)   parameter = _hungerLevel > 0;
            else            parameter = _hungerLevel < _maxHunger;
            
            while(parameter)
            {
                if (_random.Range(0f,1f) < 0.5f)
                {
                    if (subtract)   _hungerLevel -= _hungerDepleationRate;
                    else            _hungerLevel += _hungerDepleationRate * 2;

                    if (_hungerLevel > 100) _hungerLevel = 100f;
                }

                yield return _wfsChange;

                if (!subtract && !parameter) _agentStat = AgentStat.Normal;

                if (subtract)   parameter = _hungerLevel > 0;
                else            parameter = _hungerLevel < _maxHunger;
            }

            if (!subtract) _agentStat = AgentStat.Normal;
        }
        private IEnumerator ChangeEnergy(bool subtract)
        {
            bool parameter;

            if (subtract)   parameter = _energyLevel > 0;
            else            parameter = _energyLevel < _maxEnergy;

            while(parameter)
            {
                if (_random.Range(0f,1f) < 0.5f)
                {
                    if (subtract)   _energyLevel -= _energyDepleationRate;
                    else            _energyLevel += _energyDepleationRate * 2;

                    if (_energyLevel > 100) _energyLevel = 100;
                }

                yield return _wfsChange;

                if (subtract)   parameter = _energyLevel > 0;
                else            parameter = _energyLevel < _maxEnergy;
            }

            if (!subtract) _agentStat = AgentStat.Normal;
        }
        public void StartDepletingHunger() => _changeHunger = StartCoroutine(ChangeHunger(true));
        public void StopDepletingHunger()
        {
            if (_changeHunger != null) StopCoroutine(_changeHunger);
            _changeHunger = null;
        }
        public void StartDepletingEnergy() => _changeEnergy = StartCoroutine(ChangeEnergy(true));
        public void StopDepletingEnergy() 
        {
            if (_changeEnergy != null) StopCoroutine(_changeEnergy);
            _changeEnergy = null;
        }
        public void ReturnToNormal()
        {
            Debug.Log("Returning to Normal");
            if (_agentStat == AgentStat.Hungry)
            {
                if (_changeHunger == null) _changeHunger = StartCoroutine(ChangeHunger(false));
            }
            else
            {
                if (_changeEnergy == null) _changeEnergy = StartCoroutine(ChangeEnergy(false));
            }
        }
        public void UpdateStats()
        {
            if (_updateStats == null) _updateStats = StartCoroutine(UpdateStatsCoroutine());
        }
        private IEnumerator UpdateStatsCoroutine()
        {
            while (AgentStat == AgentStat.Normal)
            {
                if (_hungerLevel <= _maxHunger/2)
                {
                    if (_random.Range(0f,1f) < (0.6f - (_hungerLevel/_maxHunger)))
                    {
                        _agentStat = AgentStat.Hungry;
                    }
                }
                else if (_energyLevel <= _maxEnergy/2)
                {
                    if (_random.Range(0f,1f) < (0.6f - (_energyLevel/_maxEnergy)))
                    {
                        _agentStat = AgentStat.Tired;
                    }
                }

                yield return _wfsUpdate;
            }
            _updateStats = null;
        }
        public void ChangeColor(Color color)
        {
            _renderer.material.color = color;
        }

        #if UNITY_EDITOR
        public void SetRefs(DRCrowdManager manager)
        {
            _crowd = manager;
        }
        #endif

        public override string ToString()
        {
            if (_agentID == null)
                return $"null";

            return $"[DRAgent] Pos: {_agentID.npos} | Target: {_agentID.targetPos} | Vel: {_agentID.vel} | State: {_agentID.state} | TargetState: {_agentID.targetState} | Speed: {_agentID.desiredSpeed }";
        }
    }
}
