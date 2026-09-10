using PaymentService.Enums;

namespace PaymentService.DTOs;

public record ProcessPaymentRequest(
    string BookingPnr,
    decimal Amount,
    string IdempotencyKey);

public record ProcessPaymentHttpRequest(
    string BookingPnr,
    decimal Amount);

public record RefundPaymentRequest(
    string BookingPnr,
    decimal Amount,
    string IdempotencyKey);

public record RefundPaymentHttpRequest(
    string BookingPnr,
    decimal Amount);

public record PaymentResultDto(
    string BookingPnr,
    decimal Amount,
    PaymentStatus PaymentStatus,
    string? TransactionReference,
    RefundStatus? RefundStatus = null);
