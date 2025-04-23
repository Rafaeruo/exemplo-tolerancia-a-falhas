using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using ToleranciaFallhas.Shared.Saga.Base;

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
}