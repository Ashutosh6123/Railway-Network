using TrainService.Entities;
using TrainService.Enums;
using TrainService.Repositories;
using TrainService.Services;

namespace TrainService.Tests;

public class TrainServiceTests
{
    [Test]
    public async Task GetTrainAsync_ReturnsExistingTrain()
    {
        var service = CreateService();

        var train = await service.GetTrainAsync(1);

        Assert.That(train.TrainNumber, Is.EqualTo("12001"));
    }

    [Test]
    public void GetTrainAsync_ThrowsWhenTrainDoesNotExist()
    {
        var service = CreateService();

        Assert.ThrowsAsync<InvalidOperationException>(() => service.GetTrainAsync(99));
    }

    [Test]
    public async Task GetRouteAsync_ReturnsStopsInStopOrder()
    {
        var service = CreateService();

        var route = await service.GetRouteAsync(1);

        Assert.That(route.Select(stop => stop.StationCode), Is.EqualTo(new[] { "ALP", "BRV", "CRN" }));
    }

    [Test]
    public async Task SearchTrainsAsync_FindsOrderedStationPair()
    {
        var service = CreateService();

        var trains = await service.SearchTrainsAsync(1, 3);

        Assert.That(trains.Select(train => train.Id), Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public async Task SearchTrainsAsync_ReturnsEmptyForReversedPair()
    {
        var service = CreateService();

        var trains = await service.SearchTrainsAsync(3, 1);

        Assert.That(trains, Is.Empty);
    }

    [Test]
    public async Task SearchTrainsAsync_ReturnsEmptyWhenPairIsNotServed()
    {
        var service = CreateService();

        var trains = await service.SearchTrainsAsync(1, 99);

        Assert.That(trains, Is.Empty);
    }

    [Test]
    public async Task GetFareAsync_ReturnsExpectedFare()
    {
        var service = CreateService();

        var fare = await service.GetFareAsync(1, 1, 3, CoachType.Sleeper);

        Assert.That(fare.Amount, Is.EqualTo(450m));
    }

    [Test]
    public void GetFareAsync_ThrowsWhenFareDoesNotExist()
    {
        var service = CreateService();

        Assert.ThrowsAsync<InvalidOperationException>(() => service.GetFareAsync(1, 1, 3, CoachType.AC1Tier));
    }

    [Test]
    public void SearchTrainsAsync_RejectsSameOriginAndDestination()
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() => service.SearchTrainsAsync(1, 1));
    }

    private static TrainService.Services.TrainService CreateService()
    {
        var trains = new List<Train>
        {
            new() { Id = 1, TrainNumber = "12001", Name = "Northern Express" },
            new() { Id = 2, TrainNumber = "12002", Name = "Southern Express" }
        };

        var routeStops = new List<RouteStop>
        {
            new() { Id = 1, TrainId = 1, StationId = 1, StopOrder = 1 },
            new() { Id = 2, TrainId = 1, StationId = 2, StopOrder = 2 },
            new() { Id = 3, TrainId = 1, StationId = 3, StopOrder = 3 },
            new() { Id = 4, TrainId = 2, StationId = 3, StopOrder = 1 },
            new() { Id = 5, TrainId = 2, StationId = 2, StopOrder = 2 }
        };

        var stations = new List<Station>
        {
            new() { Id = 1, Code = "ALP", Name = "Alpha Junction" },
            new() { Id = 2, Code = "BRV", Name = "Bravo Central" },
            new() { Id = 3, Code = "CRN", Name = "Charlie Town" }
        };

        var fares = new List<Fare>
        {
            new() { Id = 1, TrainId = 1, FromStationId = 1, ToStationId = 3, CoachType = CoachType.Sleeper, Amount = 450m }
        };

        return new TrainService.Services.TrainService(
            new FakeTrainRepository(trains),
            new FakeRouteStopRepository(routeStops),
            new FakeStationRepository(stations),
            new FakeFareRepository(fares));
    }

    private sealed class FakeTrainRepository(List<Train> trains) : ITrainRepository
    {
        public Task<Train?> GetByIdAsync(int id) => Task.FromResult(trains.FirstOrDefault(train => train.Id == id));
        public Task<Train?> GetByTrainNumberAsync(string trainNumber) => Task.FromResult(trains.FirstOrDefault(train => train.TrainNumber == trainNumber));
        public Task<List<Train>> GetAllAsync() => Task.FromResult(trains);
        public Task<List<Train>> SearchAsync(string searchTerm) => Task.FromResult(trains);
        public Task AddAsync(Train train) => Task.CompletedTask;
        public Task UpdateAsync(Train train) => Task.CompletedTask;
        public Task DeleteAsync(Train train) => Task.CompletedTask;
    }

    private sealed class FakeRouteStopRepository(List<RouteStop> stops) : IRouteStopRepository
    {
        public Task<RouteStop?> GetByIdAsync(int id) => Task.FromResult(stops.FirstOrDefault(stop => stop.Id == id));
        public Task<List<RouteStop>> GetByTrainIdAsync(int trainId) => Task.FromResult(stops.Where(stop => stop.TrainId == trainId).OrderBy(stop => stop.StopOrder).ToList());
        public Task AddAsync(RouteStop routeStop) => Task.CompletedTask;
        public Task UpdateAsync(RouteStop routeStop) => Task.CompletedTask;
        public Task DeleteAsync(RouteStop routeStop) => Task.CompletedTask;
    }

    private sealed class FakeStationRepository(List<Station> stations) : IStationRepository
    {
        public Task<Station?> GetByIdAsync(int id) => Task.FromResult(stations.FirstOrDefault(station => station.Id == id));
        public Task<Station?> GetByCodeAsync(string code) => Task.FromResult(stations.FirstOrDefault(station => station.Code == code));
        public Task<List<Station>> GetAllAsync() => Task.FromResult(stations);
        public Task AddAsync(Station station) => Task.CompletedTask;
        public Task UpdateAsync(Station station) => Task.CompletedTask;
        public Task DeleteAsync(Station station) => Task.CompletedTask;
    }

    private sealed class FakeFareRepository(List<Fare> fares) : IFareRepository
    {
        public Task<Fare?> GetByIdAsync(int id) => Task.FromResult(fares.FirstOrDefault(fare => fare.Id == id));
        public Task<List<Fare>> GetByTrainIdAsync(int trainId) => Task.FromResult(fares.Where(fare => fare.TrainId == trainId).ToList());
        public Task<Fare?> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType) => Task.FromResult(fares.FirstOrDefault(fare => fare.TrainId == trainId && fare.FromStationId == fromStationId && fare.ToStationId == toStationId && fare.CoachType == coachType));
        public Task AddAsync(Fare fare) => Task.CompletedTask;
        public Task UpdateAsync(Fare fare) => Task.CompletedTask;
        public Task DeleteAsync(Fare fare) => Task.CompletedTask;
    }
}
