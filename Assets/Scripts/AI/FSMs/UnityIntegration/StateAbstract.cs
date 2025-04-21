using System;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;

namespace Scripts.AI.FSMs.UnityIntegration
{
    public abstract class StateAbstract : ScriptableObject
    {
        protected State state;
        public State State => state;

        public void AddTransitions(TransitionAbstract transAbstract) => 
        state.AddTransition(transAbstract.Transition);

        public abstract void InstantiateState();
    }
}
