using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.OrderService.Models;

public record TransactionDto
{
    public OrderStatus Step { get; set; }
    public Guid Key { get; set; }

    public TransactionDto(OrderStatus step, Guid key)
    {
        Step = step;
        Key = key;
    }
}