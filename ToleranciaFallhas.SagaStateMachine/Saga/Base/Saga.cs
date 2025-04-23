using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace ToleranciaFalhas.Shared.Saga.Base
{
    public abstract class Saga<TState, TStep, TKey>
        where TState : SagaState<TStep, TKey>
        where TStep : Enum
        where TKey : notnull
    {
        protected ConcurrentDictionary<TKey, TState> Instances { get; } = new();
        private ConcurrentDictionary<TransitionKey<TStep>, TStep> _transitions = new();
        private ConcurrentDictionary<TransitionKey<TStep>, Func<IServiceProvider, TState, Task>> _actions = new();
        private bool _isConfigured;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Saga(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected void Configure(Action<StateMachineBuilder<TState, TStep, TKey>> action)
        {
            var builder = new StateMachineBuilder<TState, TStep, TKey>();
            action(builder);

            _transitions = builder.BuildTransitions();
            _actions = builder.BuildActions();

            _isConfigured = true;
        }

        public async Task Update(ISagaEvent<TState, TStep, TKey> @event)
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException("Saga state machine is not configured.");
            }

            var key = @event.GetKey();

            if (!Instances.TryGetValue(key, out var instance))
            {
                Instances[key] = @event.Apply();
                instance = Instances[key];
            }

            var transitionKey = new TransitionKey<TStep>(instance.Step, @event.GetType().FullName!);

            if (_transitions.TryGetValue(transitionKey, out var nextStep))
            {
                Instances[key] = (TState)Instances[key].WithStep(nextStep);

                if (_actions.TryGetValue(transitionKey, out var action))
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    await action(scope.ServiceProvider, Instances[key]);
                }
            }
        }
    }

    public readonly record struct TransitionKey<TStep>(TStep From, string EventName);

    public class StateMachineBuilder<TState, TStep, TKey>
        where TState : SagaState<TStep, TKey>
        where TStep : Enum
    {
        private readonly ConcurrentDictionary<TransitionKey<TStep>, TStep> _transitions = new();
        private readonly ConcurrentDictionary<TransitionKey<TStep>, Func<IServiceProvider, TState, Task>> _actions = new();

        public ConcurrentDictionary<TransitionKey<TStep>, TStep> BuildTransitions() => _transitions;
        public ConcurrentDictionary<TransitionKey<TStep>, Func<IServiceProvider, TState, Task>> BuildActions() => _actions;

        internal void AddTransition(TransitionKey<TStep> key, TStep nextStep)
        {
            _transitions[key] = nextStep;
        }

        internal void AddAction(TransitionKey<TStep> key, Func<IServiceProvider, TState, Task> action)
        {
            _actions[key] = action;
        }

        public WhenStateMachineBuilder<TState, TStep, TKey> When<TEvent>(TStep fromStep) 
            where TEvent: ISagaEvent<TState, TStep, TKey>
        {
            var eventName = typeof(TEvent).FullName!;
            return new WhenStateMachineBuilder<TState, TStep, TKey>(this, new TransitionKey<TStep>(fromStep, eventName));
        }
    }

    public class WhenStateMachineBuilder<TState, TStep, TKey>
        where TState : SagaState<TStep, TKey>
        where TStep : Enum
    {
        private readonly StateMachineBuilder<TState, TStep, TKey> _parent;
        private readonly TransitionKey<TStep> _from;

        public WhenStateMachineBuilder(StateMachineBuilder<TState, TStep, TKey> parent, TransitionKey<TStep> from)
        {
            _parent = parent;
            _from = from;
        }

        public ThenExecuteStateMachineBuilder<TState, TStep, TKey> TransitionTo(TStep toStep)
        {
             _parent.AddTransition(_from, toStep);

            return new ThenExecuteStateMachineBuilder<TState, TStep, TKey>(_parent, _from);
        }
    }

    public class ThenExecuteStateMachineBuilder<TState, TStep, TKey>
        where TState : SagaState<TStep, TKey>
        where TStep : Enum
    {
        private readonly StateMachineBuilder<TState, TStep, TKey> _parent;
        private readonly TransitionKey<TStep> _from;

        public ThenExecuteStateMachineBuilder(StateMachineBuilder<TState, TStep, TKey> parent, TransitionKey<TStep> from)
        {
            _parent = parent;
            _from = from;
        }

        public StateMachineBuilder<TState, TStep, TKey> ThenExecute(Action<IServiceProvider, TState> action)
        {
            _parent.AddAction(_from, (serviceProvider, state) => {
                action(serviceProvider, state);
                return Task.CompletedTask;
            });
            return _parent;
        }

        public StateMachineBuilder<TState, TStep, TKey> ThenExecute(Func<IServiceProvider, TState, Task> action)
        {
            _parent.AddAction(_from, action);
            return _parent;
        }

        public WhenStateMachineBuilder<TState, TStep, TKey> When<TEvent>(TStep fromStep) 
            where TEvent: ISagaEvent<TState, TStep, TKey>
        {
            return _parent.When<TEvent>(fromStep);
        }
    }
}