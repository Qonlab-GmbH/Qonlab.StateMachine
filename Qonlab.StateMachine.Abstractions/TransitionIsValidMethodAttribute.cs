using System;

namespace Qonlab.StateMachine.Abstractions {
    [AttributeUsage( AttributeTargets.Method )]
    public class TransitionIsValidMethodAttribute : Attribute {

        public object TransitionDefinitionName { get; set; }


    }
}
