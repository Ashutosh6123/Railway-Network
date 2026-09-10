using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Entities;

namespace ReservationService.Tests;

public class ReservationDbContextTests
{
    private static ReservationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ReservationDbContext>()
            .UseSqlServer("Server=localhost;Database=RailwayReservationDbTests;Integrated Security=True;TrustServerCertificate=True")
            .Options;

        return new ReservationDbContext(options);
    }

    [Test]
    public void BookingModel_HasUniquePnrIndex()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(Booking))!;
        var index = entityType.GetIndexes().Single(index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(Booking.Pnr));

        Assert.That(index.IsUnique, Is.True);
    }

    [Test]
    public void BookingPassengerModel_HasRequiredBookingRelationship()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(BookingPassenger))!;
        var foreignKey = entityType.GetForeignKeys().Single();

        Assert.That(foreignKey.PrincipalEntityType.ClrType, Is.EqualTo(typeof(Booking)));
        Assert.That(foreignKey.IsRequired, Is.True);
    }

    [Test]
    public void SeatAllocationModel_HasBookingAndPassengerRelationships()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(SeatAllocation))!;

        Assert.That(entityType.GetForeignKeys().Count(), Is.EqualTo(2));
    }

    [Test]
    public void WaitlistEntryModel_HasUniqueBookingIdIndexAndPositionIndex()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(WaitlistEntry))!;
        var bookingIndex = entityType.GetIndexes().Single(index =>
            index.Properties.Single().Name == nameof(WaitlistEntry.BookingId));
        var positionIndex = entityType.GetIndexes().Single(index =>
            index.Properties.Single().Name == nameof(WaitlistEntry.Position));

        Assert.That(bookingIndex.IsUnique, Is.True);
        Assert.That(positionIndex.IsUnique, Is.False);
    }

    [Test]
    public void ReservationModel_DoesNotCreateCrossServiceForeignKeys()
    {
        using var dbContext = CreateDbContext();
        var entityTypes = dbContext.Model.GetEntityTypes();

        Assert.That(entityTypes.Select(entityType => entityType.ClrType), Is.EquivalentTo(new[]
        {
            typeof(Booking),
            typeof(BookingPassenger),
            typeof(SeatAllocation),
            typeof(WaitlistEntry)
        }));
    }
}
