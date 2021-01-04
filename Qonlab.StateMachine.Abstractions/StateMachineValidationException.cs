using System;

namespace Qonlab.StateMachine.Abstractions {
    public class StateMachineValidationException : Exception {
        //public StateMachineValidationException( string message, Exception innerException )
        //    : base( message, innerException ) {

        //}

        public StateMachineValidationException( string message )
            : base( message ) {

        }
    }
}
