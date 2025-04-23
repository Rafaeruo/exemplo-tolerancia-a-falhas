namespace ToleranciaFallhas.Shared.Saga.OrderSaga;

public class OrderSagaStateDto
{
    public PaymentStatus Step { get; set; }
    public Guid Key { get; set; }

    public OrderSagaStateDto(PaymentStatus step, Guid key)
    {
        Step = step;
        Key = key;
    }
}
