using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using ToleranciaFalhas.MessageBroker;

namespace ToleranciaFalhas.App1.Saga
{
    public class ExampleSaga : Saga<ExampleState, PaymentStatus, Guid>
    {
        public ExampleSaga(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory)
        {
            Configure(builder =>
            {
                _ = builder.When<ExampleEvent>(PaymentStatus.Pending)
                    .TransitionTo(PaymentStatus.Paid)
                    .ThenExecute(serviceProvider => Notify(serviceProvider, "/not-started-started"))
                    .When<AnotherExampleEvent>(PaymentStatus.Pending)
                    .TransitionTo(PaymentStatus.Rejected)
                    ;

            });
        }
        static void Notify(IServiceProvider serviceProvider, string stepEndpoit)
        {
            IHttpClientFactory? httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            HttpClient? client = httpClientFactory?.CreateClient();

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("http://127.0.0.1:6000" + stepEndpoit),
            };
            client?.SendAsync(httpRequestMessage);
            // publish a message to a queue, make an api call, etc
            Console.WriteLine("Doing something meaningful...");

        }
    }

    public record ExampleState : SagaState<PaymentStatus, Guid>
    {
        [SetsRequiredMembers]
        public ExampleState()
        {
            Step = PaymentStatus.Pending;
            _key = Guid.NewGuid();
        }

        [SetsRequiredMembers]
        public ExampleState(ExampleStateDto exampleStateDto)
        {
            Step = exampleStateDto.Step;
            _key = exampleStateDto.Key;
        }

        private readonly Guid _key;

        public override Guid GetKey()
        {
            return _key;
        }

        public ExampleStateDto ToDto()
        {
            return new ExampleStateDto(Step, _key);
        }
    }

    public class ExampleEvent : ISagaEvent<ExampleState, PaymentStatus, Guid>
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

    public class AnotherExampleEvent : ISagaEvent<ExampleState, PaymentStatus, Guid>
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

    public record ExampleStateDto
    {
        public PaymentStatus Step { get; set; }
        public Guid Key { get; set; }

        public ExampleStateDto(PaymentStatus step, Guid key)
        {
            Step = step;
            Key = key;
        }
    }
}