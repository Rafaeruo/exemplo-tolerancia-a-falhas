namespace ToleranciaFalhas.App1.Models;

public record InOrderDto
{

    public required string Item { get; set; }

}

public record Order : InOrderDto
{
    public required PaymentStatus PaymentStatus { get; set; }

    private Guid transactionKey { get; set; }

    // The transactionKey cannot be public as that would make it appear in the json... We dont want that
    public void SetTransactionKey(Guid key)
    {
        transactionKey = key;
    }

    public Guid GetTransactionKey()
    {
        return transactionKey;
    }

}

public record OutOrderDto : Order
{

    public required Guid OrderNumber { get; set; }
}