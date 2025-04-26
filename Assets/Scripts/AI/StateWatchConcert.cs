using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;
using DotRecast.Core.Numerics;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateWatchConcert", menuName = "State Machines/StateWatchConcert")]
    public class StateWatchConcert : StateAbstract
    {
        private GameObject gameObject;
        private AgentStatsController _agent;

        private Structure<Stage> _stage;

        protected override void EntryAction()
        {
            _stage = Stage.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _stage.Ref, _stage.Position);

            Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.NormalColor);
            _agent.StartDepletingHunger();
            _agent.StartDepletingEnergy();
        }
        protected override void StateAction()
        {
            _agent.UpdateStats();
        }
        protected override void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
            _agent.StopDepletingHunger();
            _agent.StopDepletingEnergy();
        }
        public override void InstantiateState()
        {
            gameObject = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }

        private bool FindSpot()
        {
            if ( _stage != null )
            {
                if ( _stage.EnteredArea( _agent.ID.npos ) )
                {
                    _agent.NextRef = _stage.GetBestSpot( _agent.ID.npos, ref _stage );
                    _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
                    _stage = null;
                }
            }
            else if ( RcVec3f.Distance(_agent.ID.npos, _agent.NextRef.Pos ) < _agent.AcceptedDist )
            {
                if ( _stage.IsGoodSpot( _agent.NextRef.Ref ) )
                    return true;
            }
            if ( _agent.ID.vel.Length() < 0.1f )
            {
                _agent.NextRef = _stage.GetBestSpot( _agent.ID.npos, ref _stage );
                _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
            }

            return false;
        }
    }
}
    

