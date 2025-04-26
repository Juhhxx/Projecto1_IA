using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;
using DotRecast.Core.Numerics;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StatePanic", menuName = "State Machines/StatePanic")]
    public class StatePanic : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        private Structure<Exit> _exit;

        protected override void EntryAction()
        {
            _exit = Exit.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _exit.Ref, _exit.Position);

            Debug.Log($"Start State {Name}");
        }
        protected override void StateAction()
        {
            if ( FindSpot() )
                _agent.Deactivate();
        }
        protected override void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
        }
        public override void InstantiateState()
        {
            gameObject  = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }

        private bool FindSpot()
        {
            if ( _exit != null )
            {
                if ( _exit.EnteredArea( _agent.ID.npos ) )
                {
                    _agent.NextRef = _exit.GetBestSpot( _agent.ID.npos, ref _exit );
                    _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
                    _exit = null;
                }
            }
            else if ( RcVec3f.Distance(_agent.ID.npos, _agent.NextRef.Pos ) < _agent.AcceptedDist )
            {
                if ( _exit.IsGoodSpot( _agent.NextRef.Ref ) )
                    return true;
            }
            if ( _agent.ID.vel.Length() < 0.1f )
            {
                _agent.NextRef = _exit.GetBestSpot( _agent.ID.npos, ref _exit );
                _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
            }

            return false;
        }
    }
}
    

