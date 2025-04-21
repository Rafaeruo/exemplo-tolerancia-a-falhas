using System.Diagnostics.CodeAnalysis;

namespace ToleranciaFalhas.App1.Saga
{
    public class ExampleSaga : Saga<ExampleState, ExampleStep, Guid>
    {
        public ExampleSaga(IServiceScopeFactory serviceScopeFactory)
            : base (serviceScopeFactory)
        {
            Configure(builder => 
            {
                builder.When<ExampleEvent>(ExampleStep.NotStarted)
                    .TransitionTo(ExampleStep.Started)
                    .ThenExecute(_ => 
                    {
                        // publish a message to a queue, make an api call, etc
                        Console.WriteLine("Doing something meaningful...");
                    })
                    .When<AnotherExampleEvent>(ExampleStep.Started)
                    .TransitionTo(ExampleStep.Finished);
            });
        }
    }

    public record ExampleState : SagaState<ExampleStep, Guid>
    {
        [SetsRequiredMembers]
        public ExampleState()
        {
            Step = ExampleStep.NotStarted;
            _key = Guid.NewGuid();
        }

        private readonly Guid _key;

        public override Guid GetKey()
        {
            return _key;
        }
    }

    public enum ExampleStep
    {
        NotStarted,
        Started,
        Finished
    }

    public class ExampleEvent : ISagaEvent<ExampleState, ExampleStep, Guid>
    {
        private readonly ExampleState _state;

        public ExampleEvent(ExampleState state)
        {
            _state = state;
        }

        public Guid GetKey()
        {
            return _state.GetKey();
        }

        public ExampleState Apply(ExampleState? currentState = null)
        {
            return _state;
        }
    }

    public class AnotherExampleEvent : ISagaEvent<ExampleState, ExampleStep, Guid>
    {
        private readonly ExampleState _state;

        public AnotherExampleEvent(ExampleState state)
        {
            _state = state;
        }

        public Guid GetKey()
        {
            return _state.GetKey();
        }

        public ExampleState Apply(ExampleState? currentState = null)
        {
            return _state;
        }
    }
}