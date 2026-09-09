using Microsoft.EntityFrameworkCore;
using TrainService.Entities;
using TrainService.Enums;

namespace TrainService.Data;

public class TrainDbContext(DbContextOptions<TrainDbContext> options) : DbContext(options)
{
    public DbSet<Train> Trains => Set<Train>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<RouteStop> RouteStops => Set<RouteStop>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Fare> Fares => Set<Fare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Train>(entity =>
        {
            entity.HasKey(train => train.Id);
            entity.Property(train => train.TrainNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(train => train.TrainNumber).IsUnique();
            entity.Property(train => train.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(station => station.Id);
            entity.Property(station => station.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(station => station.Code).IsUnique();
            entity.Property(station => station.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<RouteStop>(entity =>
        {
            entity.HasKey(routeStop => routeStop.Id);
            entity.Property(routeStop => routeStop.StopOrder).IsRequired();
            entity.Property(routeStop => routeStop.ArrivalTime).IsRequired();
            entity.Property(routeStop => routeStop.DepartureTime).IsRequired();
            entity.HasOne<Train>().WithMany().HasForeignKey(routeStop => routeStop.TrainId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Station>().WithMany().HasForeignKey(routeStop => routeStop.StationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Coach>(entity =>
        {
            entity.HasKey(coach => coach.Id);
            entity.Property(coach => coach.CoachNumber).IsRequired().HasMaxLength(50);
            entity.Property(coach => coach.CoachType).IsRequired();
            entity.HasOne<Train>().WithMany().HasForeignKey(coach => coach.TrainId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(seat => seat.Id);
            entity.Property(seat => seat.SeatNumber).IsRequired().HasMaxLength(50);
            entity.HasOne<Coach>().WithMany().HasForeignKey(seat => seat.CoachId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Fare>(entity =>
        {
            entity.HasKey(fare => fare.Id);
            entity.Property(fare => fare.CoachType).IsRequired();
            entity.Property(fare => fare.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.HasOne<Train>().WithMany().HasForeignKey(fare => fare.TrainId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Station>().HasData(
            new Station { Id = 1, Code = "ALP", Name = "Alpha Junction" },
            new Station { Id = 2, Code = "BRV", Name = "Bravo Central" },
            new Station { Id = 3, Code = "CRN", Name = "Charlie Town" },
            new Station { Id = 4, Code = "DLT", Name = "Delta City" },
            new Station { Id = 5, Code = "ECH", Name = "Echo Terminal" });

        modelBuilder.Entity<Train>().HasData(
            new Train { Id = 1, TrainNumber = "12001", Name = "Northern Express" },
            new Train { Id = 2, TrainNumber = "12002", Name = "Southern Express" });

        modelBuilder.Entity<RouteStop>().HasData(
            new RouteStop { Id = 1, TrainId = 1, StationId = 1, StopOrder = 1, ArrivalTime = new TimeSpan(6, 0, 0), DepartureTime = new TimeSpan(6, 0, 0) },
            new RouteStop { Id = 2, TrainId = 1, StationId = 2, StopOrder = 2, ArrivalTime = new TimeSpan(7, 30, 0), DepartureTime = new TimeSpan(7, 35, 0) },
            new RouteStop { Id = 3, TrainId = 1, StationId = 3, StopOrder = 3, ArrivalTime = new TimeSpan(9, 0, 0), DepartureTime = new TimeSpan(9, 5, 0) },
            new RouteStop { Id = 4, TrainId = 1, StationId = 4, StopOrder = 4, ArrivalTime = new TimeSpan(10, 30, 0), DepartureTime = new TimeSpan(10, 35, 0) },
            new RouteStop { Id = 5, TrainId = 1, StationId = 5, StopOrder = 5, ArrivalTime = new TimeSpan(12, 0, 0), DepartureTime = new TimeSpan(12, 0, 0) },
            new RouteStop { Id = 6, TrainId = 2, StationId = 5, StopOrder = 1, ArrivalTime = new TimeSpan(14, 0, 0), DepartureTime = new TimeSpan(14, 0, 0) },
            new RouteStop { Id = 7, TrainId = 2, StationId = 4, StopOrder = 2, ArrivalTime = new TimeSpan(15, 25, 0), DepartureTime = new TimeSpan(15, 30, 0) },
            new RouteStop { Id = 8, TrainId = 2, StationId = 3, StopOrder = 3, ArrivalTime = new TimeSpan(17, 0, 0), DepartureTime = new TimeSpan(17, 5, 0) },
            new RouteStop { Id = 9, TrainId = 2, StationId = 2, StopOrder = 4, ArrivalTime = new TimeSpan(18, 30, 0), DepartureTime = new TimeSpan(18, 35, 0) },
            new RouteStop { Id = 10, TrainId = 2, StationId = 1, StopOrder = 5, ArrivalTime = new TimeSpan(20, 0, 0), DepartureTime = new TimeSpan(20, 0, 0) });

        modelBuilder.Entity<Coach>().HasData(
            new Coach { Id = 1, TrainId = 1, CoachNumber = "G1", CoachType = CoachType.General },
            new Coach { Id = 2, TrainId = 1, CoachNumber = "S1", CoachType = CoachType.Sleeper },
            new Coach { Id = 3, TrainId = 1, CoachNumber = "A3-1", CoachType = CoachType.AC3Tier },
            new Coach { Id = 4, TrainId = 2, CoachNumber = "S1", CoachType = CoachType.Sleeper },
            new Coach { Id = 5, TrainId = 2, CoachNumber = "A2-1", CoachType = CoachType.AC2Tier },
            new Coach { Id = 6, TrainId = 2, CoachNumber = "A1-1", CoachType = CoachType.AC1Tier });

        modelBuilder.Entity<Seat>().HasData(
            new Seat { Id = 1, CoachId = 1, SeatNumber = "1" }, new Seat { Id = 2, CoachId = 1, SeatNumber = "2" },
            new Seat { Id = 3, CoachId = 1, SeatNumber = "3" }, new Seat { Id = 4, CoachId = 1, SeatNumber = "4" },
            new Seat { Id = 5, CoachId = 2, SeatNumber = "1" }, new Seat { Id = 6, CoachId = 2, SeatNumber = "2" },
            new Seat { Id = 7, CoachId = 2, SeatNumber = "3" }, new Seat { Id = 8, CoachId = 2, SeatNumber = "4" },
            new Seat { Id = 9, CoachId = 3, SeatNumber = "1" }, new Seat { Id = 10, CoachId = 3, SeatNumber = "2" },
            new Seat { Id = 11, CoachId = 3, SeatNumber = "3" }, new Seat { Id = 12, CoachId = 3, SeatNumber = "4" },
            new Seat { Id = 13, CoachId = 4, SeatNumber = "1" }, new Seat { Id = 14, CoachId = 4, SeatNumber = "2" },
            new Seat { Id = 15, CoachId = 4, SeatNumber = "3" }, new Seat { Id = 16, CoachId = 4, SeatNumber = "4" },
            new Seat { Id = 17, CoachId = 5, SeatNumber = "1" }, new Seat { Id = 18, CoachId = 5, SeatNumber = "2" },
            new Seat { Id = 19, CoachId = 5, SeatNumber = "3" }, new Seat { Id = 20, CoachId = 5, SeatNumber = "4" },
            new Seat { Id = 21, CoachId = 6, SeatNumber = "1" }, new Seat { Id = 22, CoachId = 6, SeatNumber = "2" },
            new Seat { Id = 23, CoachId = 6, SeatNumber = "3" }, new Seat { Id = 24, CoachId = 6, SeatNumber = "4" });

        modelBuilder.Entity<Fare>().HasData(
            new Fare { Id = 1, TrainId = 1, FromStationId = 1, ToStationId = 3, CoachType = CoachType.General, Amount = 250.00m },
            new Fare { Id = 2, TrainId = 1, FromStationId = 1, ToStationId = 3, CoachType = CoachType.Sleeper, Amount = 450.00m },
            new Fare { Id = 3, TrainId = 1, FromStationId = 1, ToStationId = 3, CoachType = CoachType.AC3Tier, Amount = 750.00m },
            new Fare { Id = 4, TrainId = 1, FromStationId = 2, ToStationId = 4, CoachType = CoachType.General, Amount = 250.00m },
            new Fare { Id = 5, TrainId = 1, FromStationId = 2, ToStationId = 4, CoachType = CoachType.Sleeper, Amount = 450.00m },
            new Fare { Id = 6, TrainId = 1, FromStationId = 2, ToStationId = 4, CoachType = CoachType.AC3Tier, Amount = 750.00m },
            new Fare { Id = 7, TrainId = 1, FromStationId = 3, ToStationId = 5, CoachType = CoachType.General, Amount = 250.00m },
            new Fare { Id = 8, TrainId = 1, FromStationId = 3, ToStationId = 5, CoachType = CoachType.Sleeper, Amount = 450.00m },
            new Fare { Id = 9, TrainId = 1, FromStationId = 3, ToStationId = 5, CoachType = CoachType.AC3Tier, Amount = 750.00m },
            new Fare { Id = 10, TrainId = 1, FromStationId = 1, ToStationId = 5, CoachType = CoachType.General, Amount = 600.00m },
            new Fare { Id = 11, TrainId = 1, FromStationId = 1, ToStationId = 5, CoachType = CoachType.Sleeper, Amount = 1000.00m },
            new Fare { Id = 12, TrainId = 1, FromStationId = 1, ToStationId = 5, CoachType = CoachType.AC3Tier, Amount = 1600.00m },
            new Fare { Id = 13, TrainId = 2, FromStationId = 5, ToStationId = 3, CoachType = CoachType.Sleeper, Amount = 450.00m },
            new Fare { Id = 14, TrainId = 2, FromStationId = 5, ToStationId = 3, CoachType = CoachType.AC2Tier, Amount = 1100.00m },
            new Fare { Id = 15, TrainId = 2, FromStationId = 5, ToStationId = 3, CoachType = CoachType.AC1Tier, Amount = 1800.00m },
            new Fare { Id = 16, TrainId = 2, FromStationId = 4, ToStationId = 2, CoachType = CoachType.Sleeper, Amount = 450.00m },
            new Fare { Id = 17, TrainId = 2, FromStationId = 4, ToStationId = 2, CoachType = CoachType.AC2Tier, Amount = 1100.00m },
            new Fare { Id = 18, TrainId = 2, FromStationId = 4, ToStationId = 2, CoachType = CoachType.AC1Tier, Amount = 1800.00m },
            new Fare { Id = 19, TrainId = 2, FromStationId = 3, ToStationId = 1, CoachType = CoachType.Sleeper, Amount = 450.00m },
            new Fare { Id = 20, TrainId = 2, FromStationId = 3, ToStationId = 1, CoachType = CoachType.AC2Tier, Amount = 1100.00m },
            new Fare { Id = 21, TrainId = 2, FromStationId = 3, ToStationId = 1, CoachType = CoachType.AC1Tier, Amount = 1800.00m },
            new Fare { Id = 22, TrainId = 2, FromStationId = 5, ToStationId = 1, CoachType = CoachType.Sleeper, Amount = 1000.00m },
            new Fare { Id = 23, TrainId = 2, FromStationId = 5, ToStationId = 1, CoachType = CoachType.AC2Tier, Amount = 2100.00m },
            new Fare { Id = 24, TrainId = 2, FromStationId = 5, ToStationId = 1, CoachType = CoachType.AC1Tier, Amount = 3200.00m });
    }
}
