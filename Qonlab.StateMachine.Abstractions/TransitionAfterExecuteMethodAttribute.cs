using System;

namespace Qonlab.StateMachine.Abstractions {
    [AttributeUsage( AttributeTargets.Method )]
    public class TransitionAfterExecuteMethodAttribute : Attribute {

        public object TransitionDefinitionName { get; set; }


    }
}
