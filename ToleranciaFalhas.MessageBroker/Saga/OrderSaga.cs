using Microsoft.Extensions.Options;
using ToleranciaFalhas.MessageBroker.Saga.Events;
using ToleranciaFalhas.Shared.Saga.Base;
using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.MessageBroker.Saga
{
    public class OrderSaga : Saga<OrderSagaState, OrderStatus, Guid>
    {
        public OrderSaga(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory)
        {
            Configure(builder =>
            {
                builder.When<NewOrderEvent>(OrderStatus.Pending)
                    .TransitionTo(OrderStatus.AwaitingPayment)
                    .ThenExecute((serviceProvider, state) => NotifyPaymentService(serviceProvider, state));

                builder.When<PaymentApprovedEvent>(OrderStatus.AwaitingPayment)
                    .TransitionTo(OrderStatus.Paid)
                    .ThenExecute((serviceProvider, state) => NotifyPaymentApproved(serviceProvider, state));

                builder.When<PaymentRejectedEvent>(OrderStatus.AwaitingPayment)
                    .TransitionTo(OrderStatus.PaymentRejected)
                    .ThenExecute((serviceProvider, state) => NotifyPaymentRejected(serviceProvider, state));
            });
        }

        private static void NotifyPaymentService(IServiceProvider serviceProvider, OrderSagaState state)
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();

            var config = serviceProvider.GetRequiredService<IOptions<GatewayConfig>>();
            var paymentServiceUrl = config.Value.Services.First(x => x.Name == "paymentservice").BaseUrl;

            // Fire and forget
            _ = client.PostAsJsonAsync(paymentServiceUrl + "/payment", state.ToDto());
        }

        private static void NotifyPaymentApproved(IServiceProvider serviceProvider, OrderSagaState state)
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();

            var config = serviceProvider.GetRequiredService<IOptions<GatewayConfig>>();
            var orderServiceUrl = config.Value.Services.First(x => x.Name == "orderservice").BaseUrl;

            // Fire and forget
            _ = client.PatchAsync(orderServiceUrl + "/shop/paymentApproved/" + state.GetKey(), content: null);
        }

        private static void NotifyPaymentRejected(IServiceProvider serviceProvider, OrderSagaState state)
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();

            var config = serviceProvider.GetRequiredService<IOptions<GatewayConfig>>();
            var orderServiceUrl = config.Value.Services.First(x => x.Name == "orderservice").BaseUrl;

            // Fire and forget
            _ = client.PatchAsync(orderServiceUrl + "/shop/paymentRejected/" + state.GetKey(), content: null);
        }
    }
}