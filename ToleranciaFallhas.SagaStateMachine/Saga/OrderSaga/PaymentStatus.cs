namespace ToleranciaFallhas.Shared.Saga.OrderSaga;

public enum PaymentStatus
{
    Pending,
    AwaitingPayment,
    Paid,
    PaymentRejected
}
