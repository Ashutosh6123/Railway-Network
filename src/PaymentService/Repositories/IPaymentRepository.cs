using PaymentService.Entities;

namespace PaymentService.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);

    Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey);

    Task<Payment?> GetByRefundIdempotencyKeyAsync(string refundIdempotencyKey);

    Task<Payment?> GetByTransactionReferenceAsync(string transactionReference);

    Task<bool> TryClaimRefundAsync(int paymentId, string refundIdempotencyKey);

    Task AddAsync(Payment payment);

    Task UpdateAsync(Payment payment);
}
