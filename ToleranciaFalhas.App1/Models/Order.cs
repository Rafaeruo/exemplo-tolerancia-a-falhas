using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.OrderService.Models;

public record InOrderDto
{
    public required string Item { get; set; }
}

public record Order
{
    public Guid Id { get; set; }
    public required string Item { get; set; }

    public required PaymentStatus PaymentStatus { get; set; }
}