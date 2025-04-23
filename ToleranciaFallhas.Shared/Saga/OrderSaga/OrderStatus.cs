namespace ToleranciaFallhas.Shared.Saga.OrderSaga;

public enum OrderStatus

{
    Pending,
    AwaitingPayment,
    Paid,
    PaymentRejected
}
