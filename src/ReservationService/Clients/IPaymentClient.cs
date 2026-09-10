namespace ReservationService.Clients;

public interface IPaymentClient
{
    Task<PaymentClientResult> ProcessPaymentAsync(string bookingPnr, decimal amount, string idempotencyKey);

    Task<PaymentClientResult> RefundAsync(string transactionReference, decimal amount, string idempotencyKey);
}
