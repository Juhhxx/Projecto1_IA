using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;

namespace Scripts.AI.FSMs.UnityIntegration
{
    [CreateAssetMenu(fileName = "StateMachine", menuName = "State Machines/StateMachine")]
    public class StateMachineCreator : ScriptableObject
    {
        private StateMachine _stateMachine;
        private GameObject _objectReference;
        public StateAbstract InitialState;
        public List<StateTransition> StateTransitions;
        
        private void InstantiateStates()
        {
            foreach (StateTransition st in StateTransitions)
            {
                Debug.Log($"Initializing State {st.State.name}");
                st.State.SetObjectReference(_objectReference);
                st.State.InstantiateState();
            }
            foreach (StateTransition st in StateTransitions)
            {
                foreach (TransitionAbstract t in st.Transitions)
                {
                    t.SetObjectReference(_objectReference);
                    if (t.Transition == null) t.IntantiateTransition();
                    st.State.AddTransitions(t);
                }
            }
        }

        public void SetObjectReference(GameObject go)
        {
            _objectReference = go;
        }
        public void InstantiateStateMachine()
        {
            InstantiateStates();
            _stateMachine = new StateMachine(InitialState.State);
        }
        public void Run()
        {
            _stateMachine.Update()?.Invoke();
        }
        public StateMachineCreator CreateStateMachine()
        {
            Dictionary<string,ScriptableObject> instances = new Dictionary<string,ScriptableObject>();
            StateMachineCreator newSM = Instantiate(this);

            newSM.InitialState = InitialState.CreateState();
            instances.Add(newSM.InitialState.name,newSM.InitialState);

            newSM.StateTransitions = new List<StateTransition>();

            foreach (StateTransition st in StateTransitions)
            {
                StateAbstract state;
                List<TransitionAbstract> transitions = new List<TransitionAbstract>();

                if (instances.ContainsKey(st.State.name))
                {
                    state = instances[st.State.name] as StateAbstract;
                }
                else
                {
                    state = st.State.CreateState();
                    instances.Add(st.State.name,state);
                }

                foreach (TransitionAbstract t in st.Transitions)
                {
                    if (instances.ContainsKey(t.name))
                    {
                        transitions.Add(instances[t.name] as TransitionAbstract);
                    }
                    else
                    {
                        TransitionAbstract transition = t.CreateTransition();
                        
                        if (instances.ContainsKey(t.ToState.name))
                        {
                            transition.ToState = instances[t.ToState.name] as StateAbstract;
                        }
                        else
                        {
                            instances.Add(t.ToState.name,t.ToState.CreateState());
                            transition.ToState = instances[t.ToState.name] as StateAbstract;
                        }

                        transitions.Add(transition);
                    }
                }

                StateTransition newST = new StateTransition(state,transitions);
                newSM.StateTransitions.Add(newST);
            }

            return newSM;
        }
    }
}

