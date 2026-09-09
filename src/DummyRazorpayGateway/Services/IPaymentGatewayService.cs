using DummyRazorpayGateway.DTOs;

namespace DummyRazorpayGateway.Services;

public interface IPaymentGatewayService
{
    Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
    Task<RefundResponse> RefundAsync(RefundRequest request);
}
