using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Qonlab.StateMachine.Abstractions;

namespace Qonlab.StateMachine {
    public class StateMachineValidationController<TTransitionLogicController, TStateMachineDataController, TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        : IStateMachineValidationController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TTransitionLogicController : ITransitionLogicController<TStateMachineDataController, TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStateMachineDataController : IStateMachineDataController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance> {


        protected readonly IEnumerable<TTransitionLogicController> TransitionLogicControllers;
        protected readonly ILogger Logger;

        public StateMachineValidationController( IEnumerable<TTransitionLogicController> transitionLogicControllers, ILogger logger ) {
            Logger = logger;
            TransitionLogicControllers = transitionLogicControllers;
        }

        public void ValidateTransitionMethods( IEnumerable<TStateMachineDefinition> stateMachineDefinitions ) {
            Logger.LogInformation( "Start" );

            var stateMachineDefinitionByName = stateMachineDefinitions.ToDictionary( i => i.Name );

            var anyError = false;
            var anyWarning = false;
            var errorStringBuilder = new StringBuilder( "Errors encountered when validating transition logic controllers:" );
            foreach ( var transitionLogicController in TransitionLogicControllers ) {
                var stateMachineDefinitionName = transitionLogicController.StateDefinitionName;
                if ( stateMachineDefinitionByName.ContainsKey( stateMachineDefinitionName ) ) {
                    var result = transitionLogicController.ValidateAllTransitionMethods( stateMachineDefinitionByName[ stateMachineDefinitionName ] );
                    if ( result.Errors.Any() || result.Warnings.Any() ) {
                        anyError = anyError || result.Errors.Any();
                        anyWarning = anyWarning || result.Warnings.Any();
                        errorStringBuilder.AppendLine();
                        errorStringBuilder.AppendLine( string.Format( "{0} for {1}:", transitionLogicController.GetType().Name, transitionLogicController.StateDefinitionName ) );
                        errorStringBuilder.AppendLine();
                        foreach ( var error in result.Errors ) {
                            errorStringBuilder.AppendLine( error );
                        }
                        errorStringBuilder.AppendLine();
                        foreach ( var error in result.Warnings ) {
                            errorStringBuilder.AppendLine( error );
                        }
                    }
                } else {
                    Logger.LogWarning( string.Format( "No state machine definition for controller {0} found", transitionLogicController.GetType().Name ) );
                }
            }

            if ( anyError ) {
                var errorString = errorStringBuilder.ToString();
                //Logger.LogError( "ValidateTransitionMethods", errorString );
                throw new StateMachineValidationException( errorString );
            } else if ( anyWarning ) {
                var errorString = errorStringBuilder.ToString();
                Logger.LogWarning( errorString );
            } else {
                Logger.LogInformation( "No errors found." );
            }

            Logger.LogInformation( "End" );
        }
    }
}
