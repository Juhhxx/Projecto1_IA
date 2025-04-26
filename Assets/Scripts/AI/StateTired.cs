using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;
using DotRecast.Core.Numerics;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateTired", menuName = "State Machines/StateTired")]
    public class StateTired : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        private Structure<GreenSpace> _green;

        protected override void EntryAction()
        {
            _green = GreenSpace.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _green.Ref, _green.Position);

            Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.TiredColor);
            _agent.StartDepletingHunger();
        }
        protected override void StateAction()
        {
            if ( FindSpot() )
                _agent.ReturnToNormal();
        }
        protected override void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
            _agent.StopDepletingHunger();
        }
        public override void InstantiateState()
        {
            gameObject  = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }

        private bool FindSpot()
        {
            if ( _green != null )
            {
                if ( _green.EnteredArea( _agent.ID.npos ) )
                {
                    _agent.NextRef = _green.GetBestSpot( _agent.ID.npos, ref _green );
                    _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
                    _green = null;
                }
            }
            else if ( RcVec3f.Distance(_agent.ID.npos, _agent.NextRef.Pos ) < _agent.AcceptedDist )
            {
                if ( _green.IsGoodSpot( _agent.NextRef.Ref ) )
                    return true;
            }
            if ( _agent.ID.vel.Length() < 0.1f )
            {
                _agent.NextRef = _green.GetBestSpot( _agent.ID.npos, ref _green );
                _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
            }

            return false;
        }
    }
}
    

