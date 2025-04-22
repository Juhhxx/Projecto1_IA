using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;

namespace Scripts.AI.FSMs.UnityIntegration
{
    [CreateAssetMenu(fileName = "StateMachine", menuName = "State Machines/StateMachine")]
    public class StateMachineCreator : ScriptableObject
    {
        private StateMachine stateMachine;
        public StateAbstract InitialState;
        public List<StateTransition> StateTransitions;
        
        private void InstantiateStates()
        {
            foreach (StateTransition st in StateTransitions)
            {
                Debug.Log($"Initializing State {st.State.name}");
                st.State.InstantiateState();
            }
            foreach (StateTransition st in StateTransitions)
            {
                foreach (TransitionAbstract t in st.Transitions)
                {
                    if (t.Transition == null) t.IntantiateTransition();
                    st.State.AddTransitions(t);
                }
            }
        }
        public void InstantiateStateMachine()
        {
            InstantiateStates();
            stateMachine = new StateMachine(InitialState.State);
        }
        public void Run()
        {
            stateMachine.Update()?.Invoke();
        }
    }
}

