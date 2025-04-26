using System;
using System.Linq;

namespace Scripts.AI.FSMs.BaseFiles
{
    public class StateMachine
    {
        private State _currentState;

        public StateMachine( State initialState)
        {
            _currentState = initialState;
        }

        public Action Update()
        {
            Action actions = null;
            Transition triggeredTransition = null;

            if (_currentState.Transitions.Count() > 0)
            {
                foreach (Transition trans in _currentState.Transitions)
                {
                    if (trans.IsTriggered())
                    {
                        triggeredTransition = trans;
                        break;
                    }
            }
            }

            if (triggeredTransition != null)
            {
                State moveToState = triggeredTransition.ToState;

                actions += _currentState.ExitActions;
                actions += triggeredTransition.Actions;
                actions += moveToState.EntryActions;

                _currentState = moveToState;
            }
            else
            {
                actions += _currentState.StateActions;
            }

            return actions;
        }
    }
}