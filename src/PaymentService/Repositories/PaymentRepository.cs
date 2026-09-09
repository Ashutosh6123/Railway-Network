using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Entities;
using PaymentService.Enums;

namespace PaymentService.Repositories;

public class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(int id)
    {
        return dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(payment => payment.Id == id);
    }

    public Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(payment => payment.IdempotencyKey == idempotencyKey);
    }

    public Task<Payment?> GetByRefundIdempotencyKeyAsync(string refundIdempotencyKey)
    {
        return dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(payment => payment.RefundIdempotencyKey == refundIdempotencyKey);
    }

    public Task<Payment?> GetByTransactionReferenceAsync(string transactionReference)
    {
        return dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(payment => payment.TransactionReference == transactionReference);
    }

    public async Task<bool> TryClaimRefundAsync(int paymentId, string refundIdempotencyKey)
    {
        // This conditional update lets only one request claim a refund on this payment.
        var updatedRows = await dbContext.Payments
            .Where(payment =>
                payment.Id == paymentId &&
                payment.PaymentStatus == PaymentStatus.Successful &&
                payment.RefundIdempotencyKey == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(payment => payment.RefundIdempotencyKey, refundIdempotencyKey)
                .SetProperty(payment => payment.RefundStatus, RefundStatus.Pending));

        return updatedRows == 1;
    }

    public async Task AddAsync(Payment payment)
    {
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        dbContext.Payments.Update(payment);
        await dbContext.SaveChangesAsync();
    }
}
