using System;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Scripts.AI.FSMs.UnityIntegration
{
    [Serializable]
    public struct StateTransition
    {
        [Expandable]
        public StateAbstract State;
        [Expandable]
        public List<TransitionAbstract> Transitions;

        public StateTransition(StateAbstract state, List<TransitionAbstract> trans)
        {
            State = state;
            Transitions = trans;
        }
    }
}