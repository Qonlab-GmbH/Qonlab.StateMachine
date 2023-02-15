using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Qonlab.StateMachine.Abstractions;

namespace Qonlab.StateMachine {
    public abstract class StateMachineDataController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        : IStateMachineDataController<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance, TTransitionExecutionInput>
        where TStatefulElement : class, IStatefulElement<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineDefinition : class, IStateMachineDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateMachineInstance : class, IStateMachineInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TStateDefinition : class, IStateDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TTransitionDefinition : class, ITransitionDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableDefinition : class, IVariableDefinition<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance>
        where TVariableInstance : class, IVariableInstance<TStatefulElement, TStateMachineDefinition, TStateMachineInstance, TStateDefinition, TTransitionDefinition, TVariableDefinition, TVariableInstance> {

        protected readonly ILogger _logger;

        public IDictionary<string, TStatefulElement> StatefulElementByName { get; set; }

        public IList<string> StateMachineDefinitionNames { get; set; }
        public IDictionary<string, TStateMachineDefinition> StateMachineDefinitionByName { get; set; }

        public IDictionary<string, TVariableDefinition> VariableDefinitionByName { get; set; }

        protected StateMachineDataController( ILogger logger ) {
            _logger = logger;
        }

        public virtual void Initialize( IList<TStateMachineDefinition> stateMachineDefinitions, IList<TVariableDefinition> variableDefinitions, IList<TStatefulElement> statefulElements ) {

            _logger.LogTrace( "Loading state machine definitions." );


            StateMachineDefinitionNames = new List<string>();
            StateMachineDefinitionByName = new Dictionary<string, TStateMachineDefinition>();
            foreach ( TStateMachineDefinition stateMachineDefinition in stateMachineDefinitions ) {
                StateMachineDefinitionNames.Add( stateMachineDefinition.Name );
                StateMachineDefinitionByName[ stateMachineDefinition.Name ] = stateMachineDefinition;
            }

            VariableDefinitionByName = variableDefinitions.ToDictionary(
                    processVariableDefinition => processVariableDefinition.Name );

            StatefulElementByName = statefulElements
                .ToDictionary( i => i.Name );


            _logger.LogTrace( message: "Initialization finished." );
        }

        public virtual void CacheTransitionsForStatefulElement( TStatefulElement statefulElement, IList<TTransitionDefinition> transitionDefinitions ) {
        }

        public virtual bool IsCurrentUserInAnyRoleForTransition( TStatefulElement statefulElement, TTransitionDefinition transitionDefinition ) {
            return true;
        }

        public IEnumerable<string> GetStateMachineDefinitionNames() {
            return StateMachineDefinitionNames.ToList();
        }

        public virtual IEnumerable<string> GetRelevantStateMachineDefinitionNames( TStatefulElement statefulElement ) {
            return GetStateMachineDefinitionNames();
        }

        public TStateMachineDefinition GetStateMachineDefinition( string stateMachineDefinitionName ) {
            if ( StateMachineDefinitionByName.ContainsKey( stateMachineDefinitionName ) ) {
                return StateMachineDefinitionByName[ stateMachineDefinitionName ];
            }
            return null;
        }

        public virtual TStateMachineInstance GetStateMachineInstance( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition ) {
            if ( statefulElement.StateMachineInstanceByStateMachineDefinition.ContainsKey( stateMachineDefinition ) ) {
                return statefulElement.StateMachineInstanceByStateMachineDefinition[ stateMachineDefinition ];
            }
            return null;
        }

        public abstract TStateMachineInstance CreateStateMachineInstance( TStatefulElement statefulElement, TStateMachineDefinition stateMachineDefinition );

        public virtual void UpdateStateMachineInstance( TStateMachineInstance stateMachineInstance, TStateDefinition newStateDefinition, TTransitionDefinition usedTransitionDefinition, TTransitionExecutionInput input ) {
            stateMachineInstance.StateDefinition = newStateDefinition;
        }

        public virtual void UpdateStateMachineInstanceAfterException( TStateMachineInstance stateMachineInstance, TTransitionDefinition usedTransitionDefinition, TTransitionExecutionInput input ) {
        }


        public virtual string GetVariableValue( TStateMachineInstance stateMachineInstance, string variableName ) {
            var variableDefinition = VariableDefinitionByName[ variableName ];
            return GetVariableValue( stateMachineInstance, variableDefinition );
        }

        public virtual string GetVariableValue( TStateMachineInstance stateMachineInstance, TVariableDefinition variableDefinition ) {
            if ( stateMachineInstance.VariableInstanceByVariableDefinition.ContainsKey( variableDefinition ) ) {
                var variableInstance = stateMachineInstance.VariableInstanceByVariableDefinition[ variableDefinition ];
                return variableInstance.Value;
            } else {
                return null;
            }
        }

        public virtual void SetVariableToValue( TStateMachineInstance stateMachineInstance, string variableName, string value ) {
            var variableDefinition = VariableDefinitionByName[ variableName ];
            SetVariableToValue( stateMachineInstance, variableDefinition, value );
        }

        public virtual void SetVariableToValue( TStateMachineInstance stateMachineInstance, TVariableDefinition variableDefinition, string value ) {
            if ( stateMachineInstance.VariableInstanceByVariableDefinition.ContainsKey( variableDefinition ) ) {
                var variableInstance = stateMachineInstance.VariableInstanceByVariableDefinition[ variableDefinition ];
                variableInstance.Value = value;
            } else {
                CreateVariableInstance( stateMachineInstance, variableDefinition, value );
            }
        }

        protected abstract void CreateVariableInstance( TStateMachineInstance stateMachineInstance, TVariableDefinition variableDefinition, string value );

    }

}
