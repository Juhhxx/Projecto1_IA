using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateWatchConcert", menuName = "State Machines/StateWatchConcert")]
    public class StateWatchConcert : StateAbstract
    {
        private GameObject gameObject;
        private AgentStatsController _agent;
        protected override void EntryAction()
        {
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
    }
}
    

