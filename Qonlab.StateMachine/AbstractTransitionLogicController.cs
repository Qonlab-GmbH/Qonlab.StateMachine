using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Qonlab.StateMachine.Abstractions;

namespace Qonlab.StateMachine {
    public abstract class AbstractTransitionLogicController<TStateMachineDataController, TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        : ITransitionLogicController<TStateMachineDataController, TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStateMachineDataController : class, IStateMachineDataController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance> {

        public TStateMachineDataController StateMachineDataController { get; set; }
        protected readonly ILogger Logger;

        protected const int NumberParametersIsValid = 1;
        protected const int NumberParametersExecute = 2;

        protected AbstractTransitionLogicController( ILogger logger ) {
            Logger = logger;
        }

        public abstract string StateDefinitionName { get; }

        public virtual void InitializeStateMachineInstance( TStateMachineInstance stateMachineInstance ) {
        }

        public virtual bool IsTransitionValid( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition ) {
            var method = GetTransitionIsValidMethod( transitionDefinition.Name );
            if ( method != null ) {
                return ( bool ) method.Invoke( this, new object[] { stateMachineInstance } );
            }
            return true;
        }

        public virtual void ExecuteTransition( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input ) {
            var method = GetTransitionExecuteMethod( transitionDefinition.Name );
            if ( method != null ) {
                method.Invoke( this, new object[] { stateMachineInstance, input } );
            }
        }

        public virtual void AfterExecuteTransition( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input ) {
            var method = GetTransitionAfterExecuteMethod( transitionDefinition.Name );
            if ( method != null ) {
                method.Invoke( this, new object[] { stateMachineInstance, input } );
            }
        }

        public virtual MethodInfo GetTransitionIsValidMethod( string transitionDefinitionName ) {
            return GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault( m => m.GetCustomAttributes( typeof( TransitionIsValidMethodAttribute ), inherit: true )
                    .Cast<TransitionIsValidMethodAttribute>()
                    .Any( a => a.TransitionDefinitionName.ToString() == transitionDefinitionName ) );
        }

        public virtual MethodInfo GetTransitionExecuteMethod( string transitionDefinitionName ) {
            return GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault( m => m.GetCustomAttributes( typeof( TransitionExecuteMethodAttribute ), inherit: true )
                    .Cast<TransitionExecuteMethodAttribute>()
                    .Any( a => a.TransitionDefinitionName.ToString() == transitionDefinitionName ) );
        }

        public virtual MethodInfo GetTransitionAfterExecuteMethod( string transitionDefinitionName ) {
            return GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault( m => m.GetCustomAttributes( typeof( TransitionAfterExecuteMethodAttribute ), inherit: true )
                    .Cast<TransitionAfterExecuteMethodAttribute>()
                    .Any( a => a.TransitionDefinitionName.ToString() == transitionDefinitionName ) );
        }

        public virtual IStateMachineValidationResult ValidateAllTransitionMethods( TStateMachineDefinition stateMachineDefinition ) {
            Logger.LogInformation( "Start" );

            var result = new StateMachineValidationResult();

            var transitionDefinitionNames = stateMachineDefinition.StateDefinitions.SelectMany( i => i.StartTransitionDefinitions ).Select( i => i.Name ).ToHashSet();

            Logger.LogDebug( "Analyzing is valid methods" );
            var isValidMethods = GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .Where( m => m.GetCustomAttributes( typeof( TransitionIsValidMethodAttribute ), inherit: true ).Any() )
                .ToList();

            var isValidMethodByTransition = new Dictionary<string, IList<MethodInfo>>();
            foreach ( var method in isValidMethods ) {
                var parameters = method.GetParameters();
                if ( parameters.Count() != NumberParametersIsValid ) {
                    AddValidationError( result, string.Format( "Is valid method {0} has {1} parameters but it should have {2}.", method.Name, parameters.Count(), NumberParametersIsValid ) );
                }
                if ( parameters[ 0 ].ParameterType != typeof( TStateMachineInstance ) ) {
                    AddValidationError( result, string.Format( "First parameter of is valid method {0} is of type {1} but it should be {2}.", method.Name, parameters[ 0 ].ParameterType.Name, typeof( TStateMachineInstance ).Name ) );
                }
                if ( method.ReturnType != typeof( bool ) ) {
                    AddValidationError( result, string.Format( "Return type of is valid method {0} is of type {1} but it should be bool.", method.Name, parameters[ 1 ].ParameterType.Name ) );
                }
                var attributes = method.GetCustomAttributes( typeof( TransitionIsValidMethodAttribute ), inherit: true ).Cast<TransitionIsValidMethodAttribute>();
                foreach ( var attribute in attributes ) {
                    var transitionName = attribute.TransitionDefinitionName.ToString();
                    if ( !transitionDefinitionNames.Contains( transitionName ) ) {
                        AddValidationWarning( result, string.Format( "No transition with name {0} exists for is valid method {1}.", transitionName, method.Name ) );
                    }
                    if ( !isValidMethodByTransition.ContainsKey( transitionName ) ) {
                        isValidMethodByTransition[ transitionName ] = new List<MethodInfo>();
                    }
                    isValidMethodByTransition[ transitionName ].Add( method );
                }
            }
            foreach ( var transitionName in isValidMethodByTransition.Keys ) {
                if ( isValidMethodByTransition[ transitionName ].Count > 1 ) {
                    var errorString = new StringBuilder( "Multiple is valid methods exist for the same transition." );
                    errorString.AppendLine( string.Format( "Transition: {0}", transitionName ) );
                    foreach ( var method in isValidMethodByTransition[ transitionName ] ) {
                        errorString.AppendLine( string.Format( "Method: {0}", method.Name ) );
                    }
                    AddValidationError( result, errorString.ToString() );
                }
            }

            Logger.LogDebug( "Analyzing execution methods" );
            var executeMethods = GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .Where( m => m.GetCustomAttributes( typeof( TransitionExecuteMethodAttribute ), inherit: true ).Any() )
                .ToList();

            var executeMethodByTransition = new Dictionary<string, IList<MethodInfo>>();
            foreach ( var method in executeMethods ) {
                var parameters = method.GetParameters();
                if ( parameters.Count() != NumberParametersExecute ) {
                    AddValidationError( result, string.Format( "Execute method {0} has {1} parameters but it should have {2}.", method.Name, parameters.Count(), NumberParametersExecute ) );
                }
                if ( parameters[ 0 ].ParameterType != typeof( TStateMachineInstance ) ) {
                    AddValidationError( result, string.Format( "First parameter of execute method {0} is of type {1} but it should be {2}.", method.Name, parameters[ 0 ].ParameterType.Name, typeof( TStateMachineInstance ).Name ) );
                }
                if ( parameters[ 1 ].ParameterType != typeof( TTransitionExecutionInput ) ) {
                    AddValidationError( result, string.Format( "Second parameter of execute method {0} is of type {1} but it should be {2}.", method.Name, parameters[ 1 ].ParameterType.Name, typeof( TTransitionExecutionInput ).Name ) );
                }
                if ( method.ReturnType != typeof( void ) ) {
                    AddValidationError( result, string.Format( "Return type of execute method {0} is of type {1} but it should be void.", method.Name, parameters[ 1 ].ParameterType.Name ) );
                }
                var attributes = method.GetCustomAttributes( typeof( TransitionExecuteMethodAttribute ), inherit: true ).Cast<TransitionExecuteMethodAttribute>();
                foreach ( var attribute in attributes ) {
                    var transitionName = attribute.TransitionDefinitionName.ToString();
                    if ( !transitionDefinitionNames.Contains( transitionName ) ) {
                        AddValidationWarning( result, string.Format( "No transition with name {0} exists for execute method {1}.", transitionName, method.Name ) );
                    }
                    if ( !executeMethodByTransition.ContainsKey( transitionName ) ) {
                        executeMethodByTransition[ transitionName ] = new List<MethodInfo>();
                    }
                    executeMethodByTransition[ transitionName ].Add( method );
                }
            }
            foreach ( var transitionName in executeMethodByTransition.Keys ) {
                if ( executeMethodByTransition[ transitionName ].Count > 1 ) {
                    var errorString = new StringBuilder( "Multiple execute methods exist for the same transition." );
                    errorString.AppendLine( string.Format( "Transition: {0}", transitionName ) );
                    foreach ( var method in isValidMethodByTransition[ transitionName ] ) {
                        errorString.AppendLine( string.Format( "Method: {0}", method.Name ) );
                    }
                    AddValidationError( result, errorString.ToString() );
                }
            }

            Logger.LogDebug( "Analyzing execution methods" );
            var afterExecuteMethods = GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .Where( m => m.GetCustomAttributes( typeof( TransitionAfterExecuteMethodAttribute ), inherit: true ).Any() )
                .ToList();

            var afterExecuteMethodByTransition = new Dictionary<string, IList<MethodInfo>>();
            foreach ( var method in afterExecuteMethods ) {
                var parameters = method.GetParameters();
                if ( parameters.Count() != NumberParametersExecute ) {
                    AddValidationError( result, string.Format( "After execute method {0} has {1} parameters but it should have {2}.", method.Name, parameters.Count(), NumberParametersExecute ) );
                }
                if ( parameters[ 0 ].ParameterType != typeof( TStateMachineInstance ) ) {
                    AddValidationError( result, string.Format( "First parameter of after execute method {0} is of type {1} but it should be {2}.", method.Name, parameters[ 0 ].ParameterType.Name, typeof( TStateMachineInstance ).Name ) );
                }
                if ( parameters[ 1 ].ParameterType != typeof( TTransitionExecutionInput ) ) {
                    AddValidationError( result, string.Format( "Second parameter of after execute method {0} is of type {1} but it should be {2}.", method.Name, parameters[ 1 ].ParameterType.Name, typeof( TTransitionExecutionInput ).Name ) );
                }
                if ( method.ReturnType != typeof( void ) ) {
                    AddValidationError( result, string.Format( "Return type of after execute method {0} is of type {1} but it should be void.", method.Name, parameters[ 1 ].ParameterType.Name ) );
                }
                var attributes = method.GetCustomAttributes( typeof( TransitionAfterExecuteMethodAttribute ), inherit: true ).Cast<TransitionAfterExecuteMethodAttribute>();
                foreach ( var attribute in attributes ) {
                    var transitionName = attribute.TransitionDefinitionName.ToString();
                    if ( !transitionDefinitionNames.Contains( transitionName ) ) {
                        AddValidationWarning( result, string.Format( "No transition with name {0} exists for after execute method {1}.", transitionName, method.Name ) );
                    }
                    if ( !executeMethodByTransition.ContainsKey( transitionName ) ) {
                        executeMethodByTransition[ transitionName ] = new List<MethodInfo>();
                    }
                    executeMethodByTransition[ transitionName ].Add( method );
                }
            }
            foreach ( var transitionName in afterExecuteMethodByTransition.Keys ) {
                if ( afterExecuteMethodByTransition[ transitionName ].Count > 1 ) {
                    var errorString = new StringBuilder( "Multiple after execute methods exist for the same transition." );
                    errorString.AppendLine( string.Format( "Transition: {0}", transitionName ) );
                    foreach ( var method in isValidMethodByTransition[ transitionName ] ) {
                        errorString.AppendLine( string.Format( "Method: {0}", method.Name ) );
                    }
                    AddValidationError( result, errorString.ToString() );
                }
            }

            Logger.LogInformation( "End" );

            return result;
        }

        private void AddValidationError( IStateMachineValidationResult result, string error ) {
            Logger.LogWarning( "VerifyAllTransitionMethods", error );
            result.Errors.Add( error );
        }

        private void AddValidationWarning( IStateMachineValidationResult result, string warning ) {
            Logger.LogWarning( "VerifyAllTransitionMethods", warning );
            result.Warnings.Add( warning );
        }

    }
}
