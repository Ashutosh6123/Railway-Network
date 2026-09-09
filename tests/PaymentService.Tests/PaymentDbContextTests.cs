using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Entities;

namespace PaymentService.Tests;

public class PaymentDbContextTests
{
    private static PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseSqlServer("Server=localhost;Database=RailwayPaymentDbTests;Integrated Security=True;TrustServerCertificate=True")
            .Options;

        return new PaymentDbContext(options);
    }

    [Test]
    public void Payment_AllowsANullRefundIdempotencyKey()
    {
        var payment = new Payment { RefundIdempotencyKey = null };

        Assert.That(payment.RefundIdempotencyKey, Is.Null);
    }

    [Test]
    public void Payment_StoresARefundIdempotencyKey()
    {
        var payment = new Payment { RefundIdempotencyKey = "refund-key-1" };

        Assert.That(payment.RefundIdempotencyKey, Is.EqualTo("refund-key-1"));
    }

    [Test]
    public void PaymentModel_HasUniqueRefundIdempotencyKeyIndex()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(Payment))!;
        var index = entityType.GetIndexes().Single(index =>
            index.Properties.Single().Name == nameof(Payment.RefundIdempotencyKey));

        Assert.That(entityType.FindProperty(nameof(Payment.RefundIdempotencyKey))!.IsNullable, Is.True);
        Assert.That(index.IsUnique, Is.True);
    }

    [Test]
    public void PaymentModel_KeepsUniquePaymentIdempotencyKeyIndex()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(Payment))!;
        var index = entityType.GetIndexes().Single(index =>
            index.Properties.Single().Name == nameof(Payment.IdempotencyKey));

        Assert.That(entityType.FindProperty(nameof(Payment.IdempotencyKey))!.IsNullable, Is.False);
        Assert.That(index.IsUnique, Is.True);
    }

    [Test]
    public void Payment_AllowsANullRefundStatus()
    {
        var payment = new Payment { RefundStatus = null };

        Assert.That(payment.RefundStatus, Is.Null);
    }
}
