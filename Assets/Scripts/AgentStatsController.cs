using System;
using System.Collections;
using UnityEngine;

namespace Scripts
{
    public class AgentStatsController : MonoBehaviour
    {
        [Header("Movement Stats")]
        [Space(5f)]
        [SerializeField] private float _maxSpeed;

        private float _currentMaxSeed;

        [Space(10f)]
        [Header("Movement Stats")]
        [Space(5f)]
        [SerializeField] private float _maxHunger;
        [SerializeField] private float _hungerDepleationRate;
        [SerializeField] private Color _hungryColor;
        [SerializeField] private float _maxEnergy;
        [SerializeField] private float _energyDepleationRate;
        [SerializeField] private Color _tiredColor;
        [SerializeField] private float _depleationSpeed;
        [SerializeField] private AgentStat _agentStat;

        [SerializeField]private float _hungerLevel;
        [SerializeField]private float _energyLevel;
        public AgentStat AgentStat => _agentStat;
        public Color HungryColor => _hungryColor;
        public Color TiredColor => _tiredColor;
        private Coroutine _changeHunger;
        private Coroutine _changeEnergy;
        private YieldInstruction _wfsDepleation;

        // Other Variables
        private Color _normalColor;
        public Color NormalColor => _normalColor;
        private ISeedRandom _random;
        private Renderer _renderer;

        private void Awake()
        {
            _renderer       = GetComponentInChildren<Renderer>();
            _normalColor    = _renderer.material.color;
            _random         = new SeedRandom(gameObject);
            _wfsDepleation  = new WaitForSeconds(_depleationSpeed);
            _hungerLevel = _maxHunger;
            _energyLevel = _maxEnergy;
            StartDepletingHunger();
            StartDepletingEnergy();
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
                    else            _hungerLevel += _hungerDepleationRate;
                }

                yield return _wfsDepleation;

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
                    else            _energyLevel += _energyDepleationRate;
                }

                yield return _wfsDepleation;

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
            if (_hungerLevel <= _maxHunger/2)
            {
                if (_random.Range(0f,1f) < (0.6f - (_hungerLevel/_maxHunger)))
                {
                    _agentStat = AgentStat.Hungry;
                    return;
                }
            }

            if (_energyLevel <= _maxEnergy/2)
            {
                if (_random.Range(0f,1f) < (0.6f - (_energyLevel/_maxEnergy)))
                {
                    _agentStat = AgentStat.Tired;
                }
            }
        }
        public void ChangeColor(Color color)
        {
            _renderer.material.color = color;
        }
    }
}
