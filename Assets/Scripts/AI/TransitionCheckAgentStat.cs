using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "TransitionCheckAgentStat", menuName = "State Machines/TransitionCheckAgentStat")]
    public class TransitionCheckAgentStat : TransitionAbstract
    {
        public  AgentStat Stat;
        private GameObject gameObject;
        private AgentStatsController _agent;

        protected override void Action()
        {
            // Debug.Log($"The agent is {Stat}");
        }
        protected override bool Condition()
        {
            return _agent.AgentStat == Stat;
        }
        public override void InstantiateTransition()
        {
            gameObject = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.transition = new Transition(base.Name,Condition,base.ToState.State,Action);
        }
    }
}
