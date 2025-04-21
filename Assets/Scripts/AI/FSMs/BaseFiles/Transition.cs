using System;

namespace Scripts.AI.FSMs.BaseFiles
{
    public class Transition
    {
        public string Name { get; private set; }
        public Action Actions { get; private set; }
        public State ToState {get; private set; }
        private Func<bool> _conditions;

        public Transition(string name, Func<bool> conditions, State toState, Action actions)
        {
            Name = name;
            _conditions = conditions;
            ToState = toState;
            Actions = actions;
        }
        public bool IsTriggered()
        {
            return _conditions();
        }
    }
}
