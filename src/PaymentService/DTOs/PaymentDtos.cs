using PaymentService.Enums;

namespace PaymentService.DTOs;

public record ProcessPaymentRequest(
    int BookingId,
    decimal Amount,
    string IdempotencyKey);

public record ProcessPaymentHttpRequest(
    int BookingId,
    decimal Amount);

public record RefundPaymentRequest(
    string TransactionReference,
    decimal Amount,
    string IdempotencyKey);

public record RefundPaymentHttpRequest(
    string TransactionReference,
    decimal Amount);

public record PaymentResultDto(
    int BookingId,
    decimal Amount,
    PaymentStatus PaymentStatus,
    string? TransactionReference,
    RefundStatus? RefundStatus = null);
