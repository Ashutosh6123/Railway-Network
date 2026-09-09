using PaymentService.Enums;

namespace PaymentService.Entities;

public class Payment
{
    public int Id { get; set; }

    // This is a logical reference to Reservation Service's Booking.
    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string TransactionReference { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string? RefundIdempotencyKey { get; set; }

    public RefundStatus? RefundStatus { get; set; }

    public DateTime CreatedAt { get; set; }
}
