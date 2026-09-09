namespace PaymentService.Clients;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ProcessPaymentAsync(
        string paymentReference,
        decimal amount,
        bool simulateSuccess);

    Task<RefundGatewayResult> RefundAsync(
        string providerTransactionReference,
        decimal amount);
}

public record PaymentGatewayResult(
    bool Success,
    string? ProviderTransactionReference,
    string Message);

public record RefundGatewayResult(
    bool Success,
    string? ProviderRefundReference,
    string Message);
