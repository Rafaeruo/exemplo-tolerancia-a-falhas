using System.Diagnostics.CodeAnalysis;
using ToleranciaFalhas.Shared.Saga.Base;
using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.MessageBroker.Saga;

public record OrderSagaState : SagaState<OrderStatus, Guid>
{
    [SetsRequiredMembers]
    public OrderSagaState()
    {
        Step = OrderStatus.Pending;
        _key = Guid.NewGuid();
    }

    [SetsRequiredMembers]
    public OrderSagaState(OrderSagaStateDto exampleStateDto)
    {
        Step = exampleStateDto.Step;
        _key = exampleStateDto.Key;
    }

    private readonly Guid _key;

    public override Guid GetKey()
    {
        return _key;
    }

    public OrderSagaStateDto ToDto()
    {
        return new OrderSagaStateDto(Step, _key);
    }
}
