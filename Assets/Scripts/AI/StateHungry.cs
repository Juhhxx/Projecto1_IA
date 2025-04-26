using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;
using DotRecast.Core.Numerics;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateHungry", menuName = "State Machines/StateHungry")]
    public class StateHungry : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        private Structure<FoodArea> _food;

        protected override void EntryAction()
        {
            _food = FoodArea.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _food.Ref, _food.Position);


            Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.HungryColor);
            _agent.StartDepletingEnergy();
        }
        protected override void StateAction()
        {
            if ( FindSpot() )
                _agent.ReturnToNormal();
        }
        protected override void ExitAction()
        {
            _agent.LastRef = _agent.NextRef;

            Debug.Log($"Exiting State {Name}");
            _agent.StopDepletingEnergy();
        }
        public override void InstantiateState()
        {
            gameObject  = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }

        private bool FindSpot()
        {
            if ( _food != null )
            {
                if ( _food.EnteredArea( _agent.ID.npos ) )
                {
                    _agent.NextRef = _food.GetBestSpot( _agent.ID.npos );
                    _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
                    _food = null;
                }
            }
            else if ( RcVec3f.Distance(_agent.ID.npos, _agent.NextRef.Pos ) < _agent.AcceptedDist )
            {
                if ( _food.IsGoodSpot( _agent.NextRef.Ref ) )
                    return true;
            }
            if ( _agent.ID.vel.Length() < 0.1f )
            {
                _agent.NextRef = _food.GetBestSpot( _agent.ID.npos );
                _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
            }

            return false;
        }
    }
}
    

