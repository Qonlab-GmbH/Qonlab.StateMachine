using System.Collections.Generic;

namespace Qonlab.StateMachine.Abstractions {
    public interface IStateMachineController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance> {

        bool UseTransitionsWhileAvailable( TStatefulElement statefulElement, TTransitionDefinition firstUsedTransition, TTransitionExecutionInput input );
        IEnumerable<TTransitionDefinition> EvaluateAvailableTransitions( TStatefulElement statefulElement, bool includeManual = true, bool includeUnauthorized = false );

        string GetVariableValue( TStatefulElement statefulElement, string stateMachineDefinitionName, string variableDefinitionName );
        string GetVariableValue( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition, TVariableDefinition variableDefinition );

        void SetVariableToValue( TStatefulElement statefulElement, string stateMachineDefinitionName, string variableDefinitionName, string value );
        void SetVariableToValue( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition, TVariableDefinition variableDefinition, string value );
    }
}
