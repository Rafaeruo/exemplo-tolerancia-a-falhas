namespace ToleranciaFalhas.App1.Models;

public record TransactionDto
{
    public PaymentStatus Step { get; set; }
    public Guid Key { get; set; }

    public TransactionDto(PaymentStatus step, Guid key)
    {
        Step = step;
        Key = key;
    }
}