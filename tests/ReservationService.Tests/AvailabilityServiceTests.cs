using ReservationService.Clients;
using ReservationService.Entities;
using ReservationService.Enums;
using ReservationService.Repositories;
using ReservationService.Services;

namespace ReservationService.Tests;

public class AvailabilityServiceTests
{
    [Test]
    public async Task GetAvailabilityAsync_ReturnsAllSeatsWhenThereAreNoAllocations()
    {
        var service = CreateService([]);

        var result = await service.GetAvailabilityAsync(1, 1, 3, new DateTime(2030, 1, 1), CoachType.Sleeper);

        Assert.That(result.AvailableSeats, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAvailabilityAsync_ExcludesSeatWithOverlappingAllocation()
    {
        var allocations = new List<SeatAllocation>
        {
            new() { SeatId = 1, FromStationId = 1, ToStationId = 3 }
        };
        var service = CreateService(allocations);

        var result = await service.GetAvailabilityAsync(1, 2, 4, new DateTime(2030, 1, 1), CoachType.Sleeper);

        Assert.That(result.AvailableSeats.Select(seat => seat.SeatId), Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public async Task GetAvailabilityAsync_AllowsSeatReuseForNonOverlappingAllocation()
    {
        var allocations = new List<SeatAllocation>
        {
            new() { SeatId = 1, FromStationId = 1, ToStationId = 2 }
        };
        var service = CreateService(allocations);

        var result = await service.GetAvailabilityAsync(1, 2, 4, new DateTime(2030, 1, 1), CoachType.Sleeper);

        Assert.That(result.AvailableSeats, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetAvailabilityAsync_RejectsStationsNotServedByTrain()
    {
        var service = CreateService([]);

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAvailabilityAsync(1, 1, 99, new DateTime(2030, 1, 1), CoachType.Sleeper));
    }

    [Test]
    public void GetAvailabilityAsync_RejectsReversedRoute()
    {
        var service = CreateService([]);

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAvailabilityAsync(1, 3, 1, new DateTime(2030, 1, 1), CoachType.Sleeper));
    }

    private static AvailabilityService CreateService(List<SeatAllocation> allocations)
    {
        return new AvailabilityService(new FakeTrainClient(), new FakeSeatAllocationRepository(allocations));
    }

    private sealed class FakeTrainClient : ITrainClient
    {
        public Task<TrainClientDto> GetTrainAsync(int trainId) => Task.FromResult(new TrainClientDto(1, "12001", "Northern Express"));

        public Task<List<RouteStopClientDto>> GetRouteAsync(int trainId) => Task.FromResult(new List<RouteStopClientDto>
        {
            new(1, 1, "A", "Alpha", TimeSpan.Zero, TimeSpan.Zero),
            new(2, 2, "B", "Bravo", TimeSpan.Zero, TimeSpan.Zero),
            new(3, 3, "C", "Charlie", TimeSpan.Zero, TimeSpan.Zero),
            new(4, 4, "D", "Delta", TimeSpan.Zero, TimeSpan.Zero)
        });

        public Task<FareClientDto> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType) =>
            Task.FromResult(new FareClientDto(trainId, fromStationId, toStationId, coachType, 450m));

        public Task<List<SeatInventoryClientDto>> GetSeatInventoryAsync(int trainId, CoachType coachType) =>
            Task.FromResult(new List<SeatInventoryClientDto>
            {
                new(1, "S1", 1, "1"),
                new(1, "S1", 2, "2")
            });
    }

    private sealed class FakeSeatAllocationRepository(List<SeatAllocation> allocations) : ISeatAllocationRepository
    {
        public Task<List<SeatAllocation>> GetActiveByTrainAndJourneyDateAsync(int trainId, DateTime journeyDate) => Task.FromResult(allocations);
        public Task<List<SeatAllocation>> GetByBookingIdAsync(int bookingId) => Task.FromResult(new List<SeatAllocation>());
        public Task AddRangeAsync(List<SeatAllocation> seatAllocations) => Task.CompletedTask;
        public Task RemoveRangeAsync(List<SeatAllocation> seatAllocations) => Task.CompletedTask;
    }
}
