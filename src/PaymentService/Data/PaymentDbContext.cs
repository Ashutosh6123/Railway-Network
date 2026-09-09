using Microsoft.EntityFrameworkCore;
using PaymentService.Entities;

namespace PaymentService.Data;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.Id).ValueGeneratedOnAdd();
            entity.Property(payment => payment.BookingId).IsRequired();
            entity.Property(payment => payment.Amount).IsRequired().HasPrecision(18, 2);
            entity.Property(payment => payment.PaymentStatus).IsRequired();
            entity.Property(payment => payment.TransactionReference).IsRequired().HasMaxLength(200);
            entity.Property(payment => payment.IdempotencyKey).IsRequired().HasMaxLength(200);
            entity.HasIndex(payment => payment.IdempotencyKey).IsUnique();
            entity.Property(payment => payment.RefundIdempotencyKey).HasMaxLength(200);
            entity.HasIndex(payment => payment.RefundIdempotencyKey).IsUnique();
            entity.Property(payment => payment.RefundStatus);
            entity.Property(payment => payment.CreatedAt).IsRequired();
        });
    }
}
