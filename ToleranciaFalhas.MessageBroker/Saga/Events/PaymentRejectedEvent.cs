using ToleranciaFalhas.Shared.Saga.Base;
using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.MessageBroker.Saga.Events;

public class PaymentRejectedEvent : ISagaEvent<OrderSagaState, PaymentStatus, Guid>
{
    private readonly OrderSagaState _state;

    public PaymentRejectedEvent(OrderSagaState state)
    {
        _state = state;
    }

    public Guid GetKey()
    {
        return _state.GetKey();
    }

    public OrderSagaState Apply(OrderSagaState? currentState = null)
    {
        return _state;
    }
}