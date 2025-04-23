namespace ToleranciaFallhas.Shared.Saga.OrderSaga;

public class OrderSagaStateDto
{
    public OrderStatus Step { get; set; }
    public Guid Key { get; set; }

    public OrderSagaStateDto(OrderStatus step, Guid key)
    {
        Step = step;
        Key = key;
    }
}
