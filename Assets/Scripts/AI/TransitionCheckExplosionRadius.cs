using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "TransitionCheckExplosionRadius", menuName = "State Machines/TransitionCheckExplosionRadius")]
    public class TransitionCheckExplosionRadius : TransitionAbstract
    {
        public  int _radius;
        private GameObject gameObject;
        private AgentStatsController _agent;

        protected override void Action()
        {
            Debug.Log($"The agent is in radius {_radius}");
        }
        protected override bool Condition()
        {
            return _agent.ExplosionRadius == _radius;
        }
        public override void InstantiateTransition()
        {
            gameObject = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.transition = new Transition(base.Name,Condition,base.ToState.State,Action);
        }
    }
}
