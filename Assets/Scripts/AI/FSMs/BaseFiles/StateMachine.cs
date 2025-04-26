using System;
using System.Linq;

namespace Scripts.AI.FSMs.BaseFiles
{
    /// <summary>
    /// Class that defines and controls a State Machine.
    /// </summary>
    public class StateMachine
    {
        // The current State that is being executed.
        private State _currentState;

        public StateMachine( State initialState)
        {
            _currentState = initialState;
        }

        /// <summary>
        /// Method that runs the State Machines Actiosn and manages all States.
        /// </summary>
        /// <returns></returns>
        public Action Update()
        {
            // Define variables for Actions performed and for a triggered Transition.
            Action actions = null;
            Transition triggeredTransition = null;

            // Check if any Transition is triggered, if yes set triggeredTransition to it.
            foreach (Transition trans in _currentState.Transitions)
            {
                if (trans.IsTriggered())
                {
                    triggeredTransition = trans;
                    break;
                }
            }

            // If a transition as been triggered move to next State and perform all required actions,
            // if not, perform the current active States state actions.
            if (triggeredTransition != null)
            {
                // Get the next State of the State Machine.
                State moveToState = triggeredTransition.ToState;

                // Load all Actions to be performed.
                actions += _currentState.ExitActions;
                actions += triggeredTransition.Actions;
                actions += moveToState.EntryActions;

                // Set _currentState as te new State.
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