using System.Collections.Concurrent;
using ToleranciaFalhas.Shared.Saga.Base;

namespace ToleranciaFallhas.Shared.Saga.Base
{
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
            where TEvent : ISagaEvent<TState, TStep, TKey>
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
            where TEvent : ISagaEvent<TState, TStep, TKey>
        {
            return _parent.When<TEvent>(fromStep);
        }
    }
}
