using PaymentService.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentRequest request);

    Task<PaymentResultDto> RefundAsync(RefundPaymentRequest request);
}
