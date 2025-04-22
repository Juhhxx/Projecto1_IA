using System;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;

namespace Scripts.AI.FSMs.UnityIntegration
{
    public abstract class StateAbstract : ScriptableObject
    {
        protected State state;
        public State State => state;
        protected GameObject objectReference;

        public void AddTransitions(TransitionAbstract transAbstract) => 
        state.AddTransition(transAbstract.Transition);
        public void SetObjectReference(GameObject go) => objectReference = go;

        protected abstract void EntryAction();
        protected abstract void StateAction();
        protected abstract void ExitAction();
        public abstract void InstantiateState();
    }
}
 