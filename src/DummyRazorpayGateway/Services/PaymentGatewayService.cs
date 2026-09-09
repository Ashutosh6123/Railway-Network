using DummyRazorpayGateway.DTOs;

namespace DummyRazorpayGateway.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    public Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentReference))
        {
            throw new ArgumentException("Payment reference is required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.");
        }

        if (!request.SimulateSuccess)
        {
            return Task.FromResult(new ProcessPaymentResponse(
                false,
                null,
                "Simulated payment failed."));
        }

        var providerTransactionReference = $"dummy_payment_{Guid.NewGuid():N}";

        return Task.FromResult(new ProcessPaymentResponse(
            true,
            providerTransactionReference,
            "Simulated payment processed successfully."));
    }

    public Task<RefundResponse> RefundAsync(RefundRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderTransactionReference))
        {
            throw new ArgumentException("Provider transaction reference is required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Refund amount must be greater than zero.");
        }

        var providerRefundReference = $"dummy_refund_{Guid.NewGuid():N}";

        return Task.FromResult(new RefundResponse(
            true,
            providerRefundReference,
            "Simulated refund processed successfully."));
    }
}
