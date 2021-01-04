using System;

namespace Qonlab.StateMachine.Abstractions {
    [AttributeUsage( AttributeTargets.Method )]
    public class TransitionExecuteMethodAttribute : Attribute {

        public object TransitionDefinitionName { get; set; }


    }
}
