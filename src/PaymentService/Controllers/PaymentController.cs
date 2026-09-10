using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/internal/payments")]
public class PaymentController(
    IPaymentService paymentService,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("process")]
    public async Task<ActionResult<PaymentResultDto>> ProcessPayment(
        ProcessPaymentHttpRequest request,
        [FromHeader(Name = "X-Internal-Service-Key")] string? internalServiceKey,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (!HasValidInternalServiceKey(internalServiceKey))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency-Key header is required.");
        }

        var result = await paymentService.ProcessPaymentAsync(new ProcessPaymentRequest(
            request.BookingPnr,
            request.Amount,
            idempotencyKey));

        return Ok(result);
    }

    [HttpPost("refund")]
    public async Task<ActionResult<PaymentResultDto>> Refund(
        RefundPaymentHttpRequest request,
        [FromHeader(Name = "X-Internal-Service-Key")] string? internalServiceKey,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (!HasValidInternalServiceKey(internalServiceKey))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency-Key header is required.");
        }

        var result = await paymentService.RefundAsync(new RefundPaymentRequest(
            request.BookingPnr,
            request.Amount,
            idempotencyKey));

        return Ok(result);
    }

    private bool HasValidInternalServiceKey(string? suppliedKey)
    {
        var configuredKey = configuration["InternalService:ApiKey"];

        return !string.IsNullOrWhiteSpace(suppliedKey) &&
               !string.IsNullOrWhiteSpace(configuredKey) &&
               string.Equals(suppliedKey, configuredKey, StringComparison.Ordinal);
    }
}
