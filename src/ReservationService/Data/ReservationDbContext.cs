using Microsoft.EntityFrameworkCore;
using ReservationService.Entities;

namespace ReservationService.Data;

public class ReservationDbContext(DbContextOptions<ReservationDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();

    public DbSet<SeatAllocation> SeatAllocations => Set<SeatAllocation>();

    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(booking => booking.Id);
            entity.Property(booking => booking.Id).ValueGeneratedOnAdd();
            entity.Property(booking => booking.Pnr).IsRequired().HasMaxLength(20);
            entity.HasIndex(booking => booking.Pnr).IsUnique();
            entity.HasIndex(booking => new { booking.TrainId, booking.JourneyDate });
            entity.Property(booking => booking.UserId).IsRequired();
            entity.Property(booking => booking.TrainId).IsRequired();
            entity.Property(booking => booking.FromStationId).IsRequired();
            entity.Property(booking => booking.ToStationId).IsRequired();
            entity.Property(booking => booking.JourneyDate).IsRequired();
            entity.Property(booking => booking.CoachType).IsRequired();
            entity.Property(booking => booking.Quota).IsRequired();
            entity.Property(booking => booking.Status).IsRequired();
            entity.Property(booking => booking.TotalFare).IsRequired().HasPrecision(18, 2);
            entity.Property(booking => booking.CreatedAt).IsRequired();
            entity.Property(booking => booking.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<BookingPassenger>(entity =>
        {
            entity.ToTable("BookingPassengers");
            entity.HasKey(passenger => passenger.Id);
            entity.Property(passenger => passenger.Id).ValueGeneratedOnAdd();
            entity.Property(passenger => passenger.Name).IsRequired().HasMaxLength(100);
            entity.Property(passenger => passenger.Age).IsRequired();
            entity.Property(passenger => passenger.Gender).IsRequired();
            entity.Property(passenger => passenger.Address).IsRequired().HasMaxLength(250);
            entity.HasOne<Booking>()
                .WithMany(booking => booking.Passengers)
                .HasForeignKey(passenger => passenger.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SeatAllocation>(entity =>
        {
            entity.ToTable("SeatAllocations");
            entity.HasKey(allocation => allocation.Id);
            entity.Property(allocation => allocation.Id).ValueGeneratedOnAdd();
            entity.HasIndex(allocation => allocation.BookingId);
            entity.HasIndex(allocation => allocation.BookingPassengerId);
            entity.HasIndex(allocation => new { allocation.SeatId, allocation.FromStationId, allocation.ToStationId });
            entity.HasOne<Booking>()
                .WithMany(booking => booking.SeatAllocations)
                .HasForeignKey(allocation => allocation.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<BookingPassenger>()
                .WithMany(passenger => passenger.SeatAllocations)
                .HasForeignKey(allocation => allocation.BookingPassengerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WaitlistEntry>(entity =>
        {
            entity.ToTable("WaitlistEntries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).ValueGeneratedOnAdd();
            entity.Property(entry => entry.Position).IsRequired();
            entity.Property(entry => entry.CreatedAt).IsRequired();
            entity.HasIndex(entry => entry.BookingId).IsUnique();
            entity.HasIndex(entry => entry.Position);
            entity.HasOne<Booking>()
                .WithOne(booking => booking.WaitlistEntry)
                .HasForeignKey<WaitlistEntry>(entry => entry.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
