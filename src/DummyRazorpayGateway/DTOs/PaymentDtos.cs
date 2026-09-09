namespace DummyRazorpayGateway.DTOs;

public record ProcessPaymentRequest(
    string PaymentReference,
    decimal Amount,
    bool SimulateSuccess);

public record ProcessPaymentResponse(
    bool Success,
    string? ProviderTransactionReference,
    string Message);

public record RefundRequest(
    string ProviderTransactionReference,
    decimal Amount);

public record RefundResponse(
    bool Success,
    string? ProviderRefundReference,
    string Message);
