using TrainService.DTOs;
using TrainService.Entities;
using TrainService.Enums;
using TrainService.Repositories;
using TrainService.Services;

namespace TrainService.Tests;

public class TrainAdminServiceTests
{
    [Test]
    public async Task CreateTrainAsync_AddsValidTrain()
    {
        var store = new AdminStore();
        await store.Service.CreateTrainAsync(new("13001", "Demo Express"));
        Assert.That(store.Trains.Any(train => train.TrainNumber == "13001"), Is.True);
    }

    [TestCase("", "Name")]
    [TestCase("13001", "")]
    public void CreateTrainAsync_RejectsMissingRequiredData(string number, string name) =>
        Assert.ThrowsAsync<ArgumentException>(() => new AdminStore().Service.CreateTrainAsync(new(number, name)));

    [Test]
    public void CreateTrainAsync_RejectsDuplicateNumber() =>
        Assert.ThrowsAsync<InvalidOperationException>(() => new AdminStore().Service.CreateTrainAsync(new("12001", "Duplicate")));

    [Test]
    public async Task UpdateTrainAsync_UpdatesExistingTrain()
    {
        var store = new AdminStore();
        await store.Service.UpdateTrainAsync(1, new("12009", "Updated Express"));
        Assert.That(store.Trains.Single(train => train.Id == 1).Name, Is.EqualTo("Updated Express"));
    }

    [Test]
    public void UpdateTrainAsync_RejectsMissingTrain() =>
        Assert.ThrowsAsync<InvalidOperationException>(() => new AdminStore().Service.UpdateTrainAsync(99, new("12009", "Missing")));

    [Test]
    public async Task DeleteTrainAsync_RemovesExistingTrain()
    {
        var store = new AdminStore();
        await store.Service.DeleteTrainAsync(2);
        Assert.That(store.Trains.Any(train => train.Id == 2), Is.False);
    }

    [Test]
    public void DeleteTrainAsync_RejectsMissingTrain() =>
        Assert.ThrowsAsync<InvalidOperationException>(() => new AdminStore().Service.DeleteTrainAsync(99));

    [Test]
    public async Task CreateAndUpdateStationAsync_WorkForValidStation()
    {
        var store = new AdminStore();
        await store.Service.CreateStationAsync(new("DLT", "Delta City"));
        var station = store.Stations.Single(item => item.Code == "DLT");
        await store.Service.UpdateStationAsync(station.Id, new("DLT", "Updated Delta"));
        Assert.That(station.Name, Is.EqualTo("Updated Delta"));
    }

    [Test]
    public void CreateStationAsync_RejectsDuplicateCode() =>
        Assert.ThrowsAsync<InvalidOperationException>(() => new AdminStore().Service.CreateStationAsync(new("ALP", "Duplicate")));

    [Test]
    public void DeleteStationAsync_RejectsMissingStation() =>
        Assert.ThrowsAsync<InvalidOperationException>(() => new AdminStore().Service.DeleteStationAsync(99));

    [Test]
    public void AddRouteStopAsync_ValidatesReferencedTrainAndStation()
    {
        var store = new AdminStore();
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.AddRouteStopAsync(new(99, 1, 1, TimeSpan.Zero, TimeSpan.Zero)));
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.AddRouteStopAsync(new(1, 99, 1, TimeSpan.Zero, TimeSpan.Zero)));
    }

    [Test]
    public void AddRouteStopAsync_RejectsInvalidOrDuplicatePositions()
    {
        var store = new AdminStore();
        Assert.ThrowsAsync<ArgumentException>(() => store.Service.AddRouteStopAsync(new(1, 3, 0, TimeSpan.Zero, TimeSpan.Zero)));
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.AddRouteStopAsync(new(1, 3, 1, TimeSpan.Zero, TimeSpan.Zero)));
    }

    [Test]
    public async Task GetRouteStopsAsync_ReturnsAscendingStopOrder()
    {
        var store = new AdminStore();
        var stops = await store.Service.GetRouteStopsAsync(1);
        Assert.That(stops.Select(stop => stop.StopOrder), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void CreateCoachAsync_ValidatesTrainAndDuplicateNumber()
    {
        var store = new AdminStore();
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.CreateCoachAsync(new(99, "S1", CoachType.Sleeper)));
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.CreateCoachAsync(new(1, "S1", CoachType.Sleeper)));
    }

    [Test]
    public void CreateSeatAsync_ValidatesCoachAndDuplicateNumber()
    {
        var store = new AdminStore();
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.CreateSeatAsync(new(99, "1")));
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.CreateSeatAsync(new(1, "1")));
    }

    [Test]
    public void CreateFareAsync_RejectsInvalidTrainStationsAndAmount()
    {
        var store = new AdminStore();
        Assert.ThrowsAsync<InvalidOperationException>(() => store.Service.CreateFareAsync(new(99, 1, 2, CoachType.General, 10m)));
        Assert.ThrowsAsync<ArgumentException>(() => store.Service.CreateFareAsync(new(1, 1, 1, CoachType.General, 10m)));
        Assert.ThrowsAsync<ArgumentException>(() => store.Service.CreateFareAsync(new(1, 2, 1, CoachType.General, 10m)));
        Assert.ThrowsAsync<ArgumentException>(() => store.Service.CreateFareAsync(new(1, 1, 2, CoachType.General, 0m)));
    }

    [Test]
    public void CreateFareAsync_RejectsDuplicateDefinition() =>
        Assert.ThrowsAsync<InvalidOperationException>(() => new AdminStore().Service.CreateFareAsync(new(1, 1, 2, CoachType.General, 20m)));

    [Test]
    public async Task UpdateFareAsync_UpdatesExistingFare()
    {
        var store = new AdminStore();
        await store.Service.UpdateFareAsync(1, new(1, 1, 2, CoachType.General, 35m));
        Assert.That(store.Fares.Single(fare => fare.Id == 1).Amount, Is.EqualTo(35m));
    }

    private sealed class AdminStore
    {
        public List<Train> Trains { get; } = [new() { Id = 1, TrainNumber = "12001", Name = "Northern" }, new() { Id = 2, TrainNumber = "12002", Name = "Southern" }];
        public List<Station> Stations { get; } = [new() { Id = 1, Code = "ALP", Name = "Alpha" }, new() { Id = 2, Code = "BRV", Name = "Bravo" }, new() { Id = 3, Code = "CRN", Name = "Charlie" }];
        public List<RouteStop> RouteStops { get; } = [new() { Id = 2, TrainId = 1, StationId = 2, StopOrder = 2 }, new() { Id = 1, TrainId = 1, StationId = 1, StopOrder = 1 }];
        public List<Coach> Coaches { get; } = [new() { Id = 1, TrainId = 1, CoachNumber = "S1", CoachType = CoachType.Sleeper }];
        public List<Seat> Seats { get; } = [new() { Id = 1, CoachId = 1, SeatNumber = "1" }];
        public List<Fare> Fares { get; } = [new() { Id = 1, TrainId = 1, FromStationId = 1, ToStationId = 2, CoachType = CoachType.General, Amount = 20m }];
        public ITrainAdminService Service { get; }

        public AdminStore()
        {
            Service = new TrainAdminService(new TrainRepo(Trains), new StationRepo(Stations), new RouteStopRepo(RouteStops), new CoachRepo(Coaches), new SeatRepo(Seats), new FareRepo(Fares));
        }
    }

    private sealed class TrainRepo(List<Train> items) : ITrainRepository
    {
        public Task<Train?> GetByIdAsync(int id) => Task.FromResult(items.FirstOrDefault(x => x.Id == id));
        public Task<Train?> GetByTrainNumberAsync(string value) => Task.FromResult(items.FirstOrDefault(x => x.TrainNumber == value));
        public Task<List<Train>> GetAllAsync() => Task.FromResult(items);
        public Task<List<Train>> SearchAsync(string value) => Task.FromResult(items);
        public Task AddAsync(Train item) { item.Id = items.Count == 0 ? 1 : items.Max(x => x.Id) + 1; items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(Train item) => Task.CompletedTask;
        public Task DeleteAsync(Train item) { items.Remove(item); return Task.CompletedTask; }
    }
    private sealed class StationRepo(List<Station> items) : IStationRepository
    {
        public Task<Station?> GetByIdAsync(int id) => Task.FromResult(items.FirstOrDefault(x => x.Id == id));
        public Task<Station?> GetByCodeAsync(string value) => Task.FromResult(items.FirstOrDefault(x => x.Code == value));
        public Task<List<Station>> GetAllAsync() => Task.FromResult(items);
        public Task AddAsync(Station item) { item.Id = items.Max(x => x.Id) + 1; items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(Station item) => Task.CompletedTask;
        public Task DeleteAsync(Station item) { items.Remove(item); return Task.CompletedTask; }
    }
    private sealed class RouteStopRepo(List<RouteStop> items) : IRouteStopRepository
    {
        public Task<RouteStop?> GetByIdAsync(int id) => Task.FromResult(items.FirstOrDefault(x => x.Id == id));
        public Task<List<RouteStop>> GetByTrainIdAsync(int id) => Task.FromResult(items.Where(x => x.TrainId == id).OrderBy(x => x.StopOrder).ToList());
        public Task AddAsync(RouteStop item) { item.Id = items.Max(x => x.Id) + 1; items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(RouteStop item) => Task.CompletedTask;
        public Task DeleteAsync(RouteStop item) { items.Remove(item); return Task.CompletedTask; }
    }
    private sealed class CoachRepo(List<Coach> items) : ICoachRepository
    {
        public Task<Coach?> GetByIdAsync(int id) => Task.FromResult(items.FirstOrDefault(x => x.Id == id));
        public Task<List<Coach>> GetByTrainIdAsync(int id) => Task.FromResult(items.Where(x => x.TrainId == id).ToList());
        public Task AddAsync(Coach item) { item.Id = items.Max(x => x.Id) + 1; items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(Coach item) => Task.CompletedTask;
        public Task DeleteAsync(Coach item) { items.Remove(item); return Task.CompletedTask; }
    }
    private sealed class SeatRepo(List<Seat> items) : ISeatRepository
    {
        public Task<Seat?> GetByIdAsync(int id) => Task.FromResult(items.FirstOrDefault(x => x.Id == id));
        public Task<List<Seat>> GetByCoachIdAsync(int id) => Task.FromResult(items.Where(x => x.CoachId == id).ToList());
        public Task AddAsync(Seat item) { item.Id = items.Max(x => x.Id) + 1; items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(Seat item) => Task.CompletedTask;
        public Task DeleteAsync(Seat item) { items.Remove(item); return Task.CompletedTask; }
    }
    private sealed class FareRepo(List<Fare> items) : IFareRepository
    {
        public Task<Fare?> GetByIdAsync(int id) => Task.FromResult(items.FirstOrDefault(x => x.Id == id));
        public Task<List<Fare>> GetByTrainIdAsync(int id) => Task.FromResult(items.Where(x => x.TrainId == id).ToList());
        public Task<Fare?> GetFareAsync(int train, int from, int to, CoachType type) => Task.FromResult(items.FirstOrDefault(x => x.TrainId == train && x.FromStationId == from && x.ToStationId == to && x.CoachType == type));
        public Task AddAsync(Fare item) { item.Id = items.Max(x => x.Id) + 1; items.Add(item); return Task.CompletedTask; }
        public Task UpdateAsync(Fare item) => Task.CompletedTask;
        public Task DeleteAsync(Fare item) { items.Remove(item); return Task.CompletedTask; }
    }
}
