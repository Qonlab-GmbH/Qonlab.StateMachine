using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Qonlab.StateMachine.Abstractions;

namespace Qonlab.StateMachine {
    public class StateMachineController<TTransitionLogicController, TStateMachineDataController, TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput, TTransitionFilter>
        : IStateMachineController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TTransitionLogicController : ITransitionLogicController<TStateMachineDataController, TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStateMachineDataController : IStateMachineDataController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionFilter : StateMachineTransitionFilter, new() {


        protected readonly IDictionary<string, TTransitionLogicController> _transitionLogicControllerByName;
        protected readonly TStateMachineDataController _stateMachineDataController;
        protected readonly ILogger _logger;

        protected virtual string StatefulElementDisplayedName { get { return "stateful element"; } }
        protected virtual string StateMachineDisplayedName { get { return "state machine"; } }
        protected virtual string StateDisplayedName { get { return "state"; } }
        protected virtual string TransitionDisplayedName { get { return "transition"; } }

        protected virtual TTransitionExecutionInput AutomaticTransitionExecutionInput { get { return default( TTransitionExecutionInput ); } }

        public StateMachineController( ILogger logger, IEnumerable<TTransitionLogicController> transitionLogicControllers, TStateMachineDataController stateMachineDataController ) {
            _stateMachineDataController = stateMachineDataController;
            _logger = logger;
            _transitionLogicControllerByName = new Dictionary<string, TTransitionLogicController>();
            foreach ( var transitionLogicController in transitionLogicControllers ) {
                transitionLogicController.StateMachineDataController = _stateMachineDataController;
                _transitionLogicControllerByName.Add( transitionLogicController.StateDefinitionName, transitionLogicController );
            }
        }

        public virtual bool UseTransitionsWhileAvailable( TStatefulElement statefulElement, TTransitionDefinition firstUsedTransition, TTransitionExecutionInput input ) {
            _logger.LogTrace( message: "Going as far as possible for " + StatefulElementDisplayedName + " " + statefulElement.DisplayedName + "." );

            UseAutomaticTransitionsWhileAvailable( statefulElement );

            _logger.LogTrace( message: "First " + TransitionDisplayedName + " to use is \"" + firstUsedTransition.Name + "\"." );
            var lastTransitionSucceded = TryUseTransition( statefulElement, firstUsedTransition, input );

            if ( lastTransitionSucceded ) {
                UseAutomaticTransitionsWhileAvailable( statefulElement );
            }

            _stateMachineDataController.CacheTransitionsForStatefulElement( statefulElement, TryGetAvailableTransitions( statefulElement, GetAllTransitionFilter() ) );

            return lastTransitionSucceded;
        }


        public virtual IEnumerable<TTransitionDefinition> EvaluateAvailableTransitions( TStatefulElement statefulElement, bool includeManual = true, bool includeUnauthorized = false ) {
            var transitionFilter = new TTransitionFilter() {
                IncludeManual = includeManual,
                IncludeUnauthorized = includeUnauthorized
            };

            return EvaluateAvailableTransitions( statefulElement, transitionFilter );
        }

        protected virtual IEnumerable<TTransitionDefinition> EvaluateAvailableTransitions( TStatefulElement statefulElement, TTransitionFilter transitionFilter ) {
            _logger.LogTrace( message: "Going as far as possible for " + StatefulElementDisplayedName + " " + statefulElement.DisplayedName + "." );

            UseAutomaticTransitionsWhileAvailable( statefulElement );

            var availableTransitions = GetAvailableTransitions( statefulElement, GetAllTransitionFilter() );

            _stateMachineDataController.CacheTransitionsForStatefulElement( statefulElement, availableTransitions );

            availableTransitions = FilterTransitions( statefulElement, availableTransitions, transitionFilter ).ToList();

            return availableTransitions;
        }

        protected virtual void UseAutomaticTransitionsWhileAvailable( TStatefulElement statefulElement ) {
            var lastTransitionSucceded = true;

            var availableTransitions = GetAvailableTransitions( statefulElement, GetAutomaticTransitionFilter() );
            while ( lastTransitionSucceded && availableTransitions.Any() ) {
                var nextUsedTransition = availableTransitions.First();

                _logger.LogTrace( message: availableTransitions.Count + " " + TransitionDisplayedName + " available. Using \"" + nextUsedTransition.Name + "\"." );
                lastTransitionSucceded = TryUseTransition( statefulElement, nextUsedTransition, AutomaticTransitionExecutionInput );
                if ( lastTransitionSucceded ) {
                    availableTransitions = TryGetAvailableTransitions( statefulElement, GetAutomaticTransitionFilter() );
                }
            }
        }

        protected virtual bool TryUseTransition( TStatefulElement statefulElement, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input ) {
            var stateMachine = transitionDefinition.StateDefinitionStart.StateMachineDefinition;
            _logger.LogTrace( message: StateMachineDisplayedName + " definition is \"" + stateMachine.Name + "\"." );

            var stateMachineInstance = GetOrCreateStateMachineInstance( statefulElement, stateMachine );
            _logger.LogTrace( message: "Starting with " + StateDisplayedName + " \"" + stateMachineInstance.StateDefinition.Name + "." );

            return TryUseTransition( stateMachineInstance, transitionDefinition, input );
        }

        protected virtual bool TryUseTransition( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, TTransitionExecutionInput input ) {
            try {
                if ( !_stateMachineDataController.IsCurrentUserInAnyRoleForTransition( stateMachineInstance.StatefulElement, transitionDefinition ) ) {
                    throw new UserNotAuthorizedForTransitionException( "Current user is not authorized for " + TransitionDisplayedName + " \"" + transitionDefinition.Name + "\"." );
                }
                if ( !IsTransitionValid( stateMachineInstance, transitionDefinition ) ) {
                    throw new Exception( "Transition \"" + transitionDefinition.Name + "\" is not valid for given " + StateMachineDisplayedName + " instance." );
                }

                //_stateMachineDataController.ExecutionComment = comment;

                _logger.LogTrace( message: "Trying to use " + TransitionDisplayedName + " \"" + transitionDefinition.Name + "\"" );

                var stateMachineDefinitionName = transitionDefinition.StateDefinitionStart.StateMachineDefinition.Name;
                if ( _transitionLogicControllerByName.ContainsKey( stateMachineDefinitionName ) ) {
                    _transitionLogicControllerByName[ stateMachineDefinitionName ].ExecuteTransition( stateMachineInstance, transitionDefinition, input );
                }

                if ( stateMachineInstance.StateDefinition == null ) {
                    stateMachineInstance.StateDefinition = stateMachineInstance.StateMachineDefinition.StartStateDefinition;
                }

                if ( !stateMachineInstance.StateDefinition.StartTransitionDefinitions.Contains( transitionDefinition ) ) {
                    throw new ArgumentException( "The selected " + TransitionDisplayedName + " is no longer available!" );
                }

                var newStateDefinition = transitionDefinition.StateDefinitionEnd;
                _logger.LogTrace( message: "Using " + TransitionDisplayedName + " \"" + transitionDefinition.Name + "\" succeded. New " + StateDisplayedName + " definition is \"" + newStateDefinition.Name + "\"." );

                _stateMachineDataController.UpdateStateMachineInstance( stateMachineInstance, newStateDefinition, transitionDefinition, input );

                if ( _transitionLogicControllerByName.ContainsKey( stateMachineDefinitionName ) ) {
                    _transitionLogicControllerByName[ stateMachineDefinitionName ].AfterExecuteTransition( stateMachineInstance, transitionDefinition, input );
                }

                return true;

            } catch ( Exception e ) {
                HandleException( stateMachineInstance, transitionDefinition, e, "TryUseTransition" );
            }
            return false;
        }

        protected virtual IList<TTransitionDefinition> TryGetAvailableTransitions( TStatefulElement statefulElement, TTransitionFilter transitionFilter ) {
            _logger.LogTrace( string.Format( "Getting available " + TransitionDisplayedName + " for " + StatefulElementDisplayedName + " {0} including {1} ones", statefulElement.DisplayedName, transitionFilter.IncludeManual ? "manual and automatic" : "only automatic" ) );
            try {
                return GetAvailableTransitions( statefulElement, transitionFilter );
            } catch ( Exception e ) {
                _logger.LogError( message: "Getting " + TransitionDisplayedName + " for " + StatefulElementDisplayedName + " \"" + statefulElement.DisplayedName + "\" failed with exception: " + e );
            }
            return new List<TTransitionDefinition>();
        }

        protected virtual IList<TTransitionDefinition> GetAvailableTransitions( TStatefulElement statefulElement, TTransitionFilter transitionFilter ) {
            var transitions = new List<TTransitionDefinition>();
            foreach ( var stateMachineDefinitionName in _stateMachineDataController.GetRelevantStateMachineDefinitionNames( statefulElement ) ) {
                var stateMachineDefinition = _stateMachineDataController.GetStateMachineDefinition( stateMachineDefinitionName );
                var stateMachineInstance = GetOrCreateStateMachineInstance( statefulElement, stateMachineDefinition );
                _logger.LogTrace( message: "Getting " + TransitionDisplayedName + " for " + StateMachineDisplayedName + " definition \"" + stateMachineDefinitionName + "\" in " + StateDisplayedName + " \"" + stateMachineInstance.StateDefinition.Name + "\"" );
                transitions.AddRange( GetAvailableTransitions( stateMachineInstance, transitionFilter ) );
            }
            return transitions.OrderBy( t => t.StateDefinitionStart.StateMachineDefinition.Priority ).ThenBy( t => t.Priority ).ToList();
        }

        protected virtual IEnumerable<TTransitionDefinition> GetAvailableTransitions( TStateMachineInstance stateMachineInstance, TTransitionFilter transitionFilter ) {
            var stateDefinition = stateMachineInstance.StateDefinition;
            if ( stateDefinition == null && stateMachineInstance.StateMachineDefinition != null && stateMachineInstance.StateMachineDefinition.StartStateDefinition != null ) {
                stateDefinition = stateMachineInstance.StateMachineDefinition.StartStateDefinition;
            }

            if ( stateDefinition != null ) {
                IEnumerable<TTransitionDefinition> availableTransitions = stateDefinition.StartTransitionDefinitions;

                availableTransitions = FilterTransitions( stateMachineInstance.StatefulElement, availableTransitions, transitionFilter );

                availableTransitions = availableTransitions
                    .Where( transitionDefinition => IsTransitionValid( stateMachineInstance, transitionDefinition ) );

                return availableTransitions.ToList();
            } else {
                return new List<TTransitionDefinition>();
            }
        }

        protected virtual IEnumerable<TTransitionDefinition> FilterTransitions( TStatefulElement statefulElement, IEnumerable<TTransitionDefinition> transitions, TTransitionFilter transitionFilter ) {
            var filteredTransitions = transitions;
            if ( !transitionFilter.IncludeManual ) {
                filteredTransitions = filteredTransitions.Where( t => !t.IsManual );
            }
            if ( !transitionFilter.IncludeUnauthorized ) {
                filteredTransitions = filteredTransitions.Where( transitionDefinition => _stateMachineDataController.IsCurrentUserInAnyRoleForTransition( statefulElement, transitionDefinition ) );
            }

            return filteredTransitions;
        }

        protected virtual TTransitionFilter GetAutomaticTransitionFilter() {
            var transitionFilter = new TTransitionFilter() {
                IncludeManual = false,
                IncludeUnauthorized = false,
            };
            return transitionFilter;
        }

        protected virtual TTransitionFilter GetAllTransitionFilter() {
            var transitionFilter = new TTransitionFilter() {
                IncludeManual = true,
                IncludeUnauthorized = true,
            };
            return transitionFilter;
        }

        protected virtual bool IsTransitionValid( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition ) {
            try {
                var stateMachineDefinition = transitionDefinition.StateDefinitionStart.StateMachineDefinition.Name;
                if ( _transitionLogicControllerByName.ContainsKey( stateMachineDefinition ) ) {
                    return _transitionLogicControllerByName[ stateMachineDefinition ].IsTransitionValid( stateMachineInstance, transitionDefinition );
                }
                return true;
            } catch ( Exception e ) {
                HandleException( stateMachineInstance, transitionDefinition, e, "IsTransitionValid" );
                throw;
            }
        }

        public virtual TStateMachineInstance GetOrCreateStateMachineInstance( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition ) {
            var stateMachineInstance = _stateMachineDataController.GetStateMachineInstance( statefulElement, stateMachineDefinition );
            if ( stateMachineInstance == null ) {
                stateMachineInstance = _stateMachineDataController.CreateStateMachineInstance( statefulElement, stateMachineDefinition );
                if ( _transitionLogicControllerByName.ContainsKey( stateMachineDefinition.Name ) ) {
                    _transitionLogicControllerByName[ stateMachineDefinition.Name ].InitializeStateMachineInstance( stateMachineInstance );
                }
            }
            return stateMachineInstance;
        }

        public string GetVariableValue( TStatefulElement statefulElement, string stateMachineDefinitionName, string variableDefinitionName ) {
            var stateMachineInstance = GetOrCreateStateMachineInstance( statefulElement, _stateMachineDataController.GetStateMachineDefinition( stateMachineDefinitionName ) );
            return _stateMachineDataController.GetVariableValue( stateMachineInstance, variableDefinitionName );
        }

        public string GetVariableValue( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition, TVariableDefinition variableDefinition ) {
            var stateMachineInstance = GetOrCreateStateMachineInstance( statefulElement, stateMachineDefinition );
            return _stateMachineDataController.GetVariableValue( stateMachineInstance, variableDefinition );
        }

        public void SetVariableToValue( TStatefulElement statefulElement, string stateMachineDefinitionName, string variableDefinitionName, string value ) {
            var stateMachineInstance = GetOrCreateStateMachineInstance( statefulElement, _stateMachineDataController.GetStateMachineDefinition( stateMachineDefinitionName ) );
            _stateMachineDataController.SetVariableToValue( stateMachineInstance, variableDefinitionName, value );
        }

        public void SetVariableToValue( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition, TVariableDefinition variableDefinition, string value ) {
            var stateMachineInstance = GetOrCreateStateMachineInstance( statefulElement, stateMachineDefinition );
            _stateMachineDataController.SetVariableToValue( stateMachineInstance, variableDefinition, value );
        }

        protected virtual void HandleException( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, Exception exception, string methodName ) {
            var logLevelAndTransitionExecutionInput = GetLogLevelAndTransitionExecutionInput( stateMachineInstance, transitionDefinition, exception );
            var logLevel = logLevelAndTransitionExecutionInput.Item1;
            var transitionExecutionInput = logLevelAndTransitionExecutionInput.Item2;

            string exceptionDetails = exception.ToString();

            LogException( stateMachineInstance, transitionDefinition, logLevel, methodName, exceptionDetails, transitionExecutionInput );
        }

        protected virtual void LogException( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, LogLevel logLevel, string methodName, string exceptionDetails, TTransitionExecutionInput transitionExecutionInput ) {
            _logger.Log( logLevel: logLevel, message: TransitionDisplayedName + " \"" + transitionDefinition.Name + "\" for " + StatefulElementDisplayedName + " \"" + stateMachineInstance.StatefulElement.DisplayedName + "\" failed with exception: " + exceptionDetails );
            _stateMachineDataController.UpdateStateMachineInstanceAfterException( stateMachineInstance, transitionDefinition, transitionExecutionInput );
        }

        protected virtual Tuple<LogLevel, TTransitionExecutionInput> GetLogLevelAndTransitionExecutionInput( TStateMachineInstance stateMachineInstance, TTransitionDefinition transitionDefinition, Exception exception ) {
            return new Tuple<LogLevel, TTransitionExecutionInput>( LogLevel.Error, default( TTransitionExecutionInput ) );
        }


    }
}
