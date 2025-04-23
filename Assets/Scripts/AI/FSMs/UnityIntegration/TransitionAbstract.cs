using System;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;

namespace Scripts.AI.FSMs.UnityIntegration
{
    public abstract class TransitionAbstract : ScriptableObject
    {
        public string Name;
        protected Transition transition;
        public Transition Transition => transition;
        public StateAbstract ToState;
        protected GameObject objectReference;

        public bool IsTriggered() => transition.IsTriggered();
        public void SetObjectReference(GameObject go) => objectReference = go;

        protected abstract void Action();
        protected abstract bool Condition();
        public abstract void IntantiateTransition();
        public TransitionAbstract CreateTransition() => Instantiate(this);
    }
}
