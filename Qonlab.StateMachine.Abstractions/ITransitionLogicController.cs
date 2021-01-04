
namespace Qonlab.StateMachine.Abstractions {
    public interface ITransitionLogicController<TStateMachineDataController, TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStateMachineDataController : IStateMachineDataController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance> {

        TStateMachineDataController StateMachineDataController { get; set; }

        string StateDefinitionName { get; }

        void InitializeStateMachineInstance( TStateMachineInstance stateMachineInstance );
        bool IsTransitionValid( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition );
        void ExecuteTransition( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input );
        void AfterExecuteTransition( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input );

        IStateMachineValidationResult ValidateAllTransitionMethods( TStateMachineDefinition stateMachineDefinition );
    }
}