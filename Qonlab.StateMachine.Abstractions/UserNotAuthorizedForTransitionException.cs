using System;

namespace Qonlab.StateMachine.Abstractions {
    public class UserNotAuthorizedForTransitionException : Exception {

        public UserNotAuthorizedForTransitionException( string message, Exception innerException )
            : base( message, innerException ) {

        }

        public UserNotAuthorizedForTransitionException( string message )
            : base( message ) {

        }

        public UserNotAuthorizedForTransitionException()
            : base() {

        }

    }
}
