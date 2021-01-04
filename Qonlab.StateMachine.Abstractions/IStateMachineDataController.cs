using System.Collections.Generic;

namespace Qonlab.StateMachine.Abstractions {
    public interface IStateMachineDataController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance> {

        bool IsCurrentUserInAnyRoleForTransition( TStatefulElement statefulElement, TTransitionDefinition transitionDefinition );

        TStateMachineInstance GetStateMachineInstance( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition );
        TStateMachineInstance CreateStateMachineInstance( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition );

        IEnumerable<string> GetStateMachineDefinitionNames();
        IEnumerable<string> GetRelevantStateMachineDefinitionNames( TStatefulElement statefulElement );
        TStateMachineDefinition GetStateMachineDefinition( string stateMachineDefinitionName );

        void CacheTransitionsForStatefulElement( TStatefulElement statefulElement, IList<TTransitionDefinition> transitionDefinitions );

        void UpdateStateMachineInstance( TStateMachineInstance stateMachineInstance, TStateDefinition newStateDefinition, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input );
        void UpdateStateMachineInstanceAfterException( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input );

        string GetVariableValue( TStateMachineInstance stateMachineInstance, string variableName );
        string GetVariableValue( TStateMachineInstance stateMachineInstance, TVariableDefinition variableDefinition );
        void SetVariableToValue( TStateMachineInstance stateMachineInstance, string variableName, string value );
        void SetVariableToValue( TStateMachineInstance stateMachineInstance, TVariableDefinition variableDefinition, string value );
    }
}
