using System;
using System.Collections.Generic;

namespace Qonlab.StateMachine.Abstractions {
    public interface IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance> {

        int Id { get; }
        string Name { get; }
        string DisplayedName { get; }
        string Description { get; }
        bool IsDisplayed { get; }
        Type ValueType { get; }
        ICollection<TVariableInstance> VariableInstances { get; }
    }
}
