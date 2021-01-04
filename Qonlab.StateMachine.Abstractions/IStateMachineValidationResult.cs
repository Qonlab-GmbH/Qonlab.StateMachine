using System.Collections.Generic;

namespace Qonlab.StateMachine.Abstractions {
    public interface IStateMachineValidationResult {

        ICollection<string> Errors { get; }

        ICollection<string> Warnings { get; }

    }
}
