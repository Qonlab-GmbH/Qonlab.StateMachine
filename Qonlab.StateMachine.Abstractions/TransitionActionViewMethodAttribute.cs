using System;

namespace Qonlab.StateMachine.Abstractions {
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
    public class TransitionActionViewMethodAttribute : Attribute {

        public object TransitionDefinitionName { get; set; }

    }
}
