using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TrainService.Controllers;
using TrainService.DTOs;
using TrainService.Enums;
using TrainService.Middleware;
using TrainService.Services;

namespace TrainService.Tests;

public class TrainControllerTests
{
    [Test]
    public async Task Search_ReturnsOkWithMatchingTrains()
    {
        var controller = new TrainController(new FakeTrainService());

        var result = await controller.Search(1, 3);

        var okResult = result.Result as OkObjectResult;
        var trains = okResult?.Value as List<TrainDto>;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(trains, Has.Count.EqualTo(1));
        Assert.That(trains![0].TrainNumber, Is.EqualTo("12001"));
    }

    [Test]
    public async Task Search_ReturnsOkWithEmptyResultForUnservedPair()
    {
        var controller = new TrainController(new FakeTrainService());

        var result = await controller.Search(3, 1);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.Empty);
    }

    [Test]
    public async Task GetRoute_ReturnsRouteStopsInStopOrder()
    {
        var controller = new TrainController(new FakeTrainService());

        var result = await controller.GetRoute(1);

        var okResult = result.Result as OkObjectResult;
        var route = okResult?.Value as List<RouteStopDto>;
        Assert.That(route!.Select(stop => stop.StopOrder), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task GetFare_ReturnsOkWithRequestedFare()
    {
        var controller = new TrainController(new FakeTrainService());

        var result = await controller.GetFare(1, 1, 3, CoachType.Sleeper);

        var okResult = result.Result as OkObjectResult;
        var fare = okResult?.Value as FareDto;
        Assert.That(fare?.Amount, Is.EqualTo(450m));
    }

    [Test]
    public async Task InvalidServiceInput_IsMappedToBadRequestByGlobalMiddleware()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new ArgumentException("Station IDs must be different."),
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public void TrainAdminController_RequiresAdministratorRole()
    {
        var attribute = typeof(TrainAdminController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.That(attribute.Roles, Is.EqualTo("Administrator"));
    }

    [Test]
    public async Task CreateTrain_ReturnsCreatedAndDelegatesToAdminService()
    {
        var adminService = new FakeTrainAdminService();
        var controller = new TrainAdminController(adminService);

        var result = await controller.CreateTrain(new TrainAdminRequest("13001", "Demo Express"));

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(adminService.CreateTrainRequest, Is.EqualTo(new TrainAdminRequest("13001", "Demo Express")));
        Assert.That(createdResult?.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        Assert.That(createdResult?.ActionName, Is.EqualTo(nameof(TrainController.GetTrain)));
    }

    private sealed class FakeTrainService : ITrainService
    {
        public Task<List<TrainDto>> SearchTrainsAsync(int fromStationId, int toStationId) =>
            Task.FromResult(fromStationId == 1 && toStationId == 3
                ? new List<TrainDto> { new(1, "12001", "Northern Express") }
                : new List<TrainDto>());

        public Task<TrainDto> GetTrainAsync(int trainId) =>
            Task.FromResult(new TrainDto(trainId, "12001", "Northern Express"));

        public Task<List<RouteStopDto>> GetRouteAsync(int trainId) =>
            Task.FromResult(new List<RouteStopDto>
            {
                new(1, 1, "ALP", "Alpha Junction", TimeSpan.FromHours(6), TimeSpan.FromHours(6)),
                new(2, 2, "BRV", "Bravo Central", TimeSpan.FromHours(7.5), TimeSpan.FromHours(7.5))
            });

        public Task<FareDto> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType) =>
            Task.FromResult(new FareDto(trainId, fromStationId, toStationId, coachType, 450m));

        public Task<List<SeatInventoryDto>> GetSeatInventoryAsync(int trainId, CoachType coachType) =>
            Task.FromResult(new List<SeatInventoryDto>());
    }

    private sealed class FakeTrainAdminService : ITrainAdminService
    {
        public TrainAdminRequest? CreateTrainRequest { get; private set; }

        public Task<TrainDto> CreateTrainAsync(TrainAdminRequest request)
        {
            CreateTrainRequest = request;
            return Task.FromResult(new TrainDto(3, request.TrainNumber, request.Name));
        }

        public Task<TrainDto> UpdateTrainAsync(int trainId, TrainAdminRequest request) => throw new NotImplementedException();
        public Task DeleteTrainAsync(int trainId) => throw new NotImplementedException();
        public Task CreateStationAsync(StationAdminRequest request) => throw new NotImplementedException();
        public Task UpdateStationAsync(int stationId, StationAdminRequest request) => throw new NotImplementedException();
        public Task DeleteStationAsync(int stationId) => throw new NotImplementedException();
        public Task AddRouteStopAsync(RouteStopAdminRequest request) => throw new NotImplementedException();
        public Task UpdateRouteStopAsync(int routeStopId, RouteStopAdminRequest request) => throw new NotImplementedException();
        public Task DeleteRouteStopAsync(int routeStopId) => throw new NotImplementedException();
        public Task<List<RouteStopDto>> GetRouteStopsAsync(int trainId) => throw new NotImplementedException();
        public Task CreateCoachAsync(CoachAdminRequest request) => throw new NotImplementedException();
        public Task UpdateCoachAsync(int coachId, CoachAdminRequest request) => throw new NotImplementedException();
        public Task DeleteCoachAsync(int coachId) => throw new NotImplementedException();
        public Task CreateSeatAsync(SeatAdminRequest request) => throw new NotImplementedException();
        public Task UpdateSeatAsync(int seatId, SeatAdminRequest request) => throw new NotImplementedException();
        public Task DeleteSeatAsync(int seatId) => throw new NotImplementedException();
        public Task CreateFareAsync(FareAdminRequest request) => throw new NotImplementedException();
        public Task UpdateFareAsync(int fareId, FareAdminRequest request) => throw new NotImplementedException();
        public Task DeleteFareAsync(int fareId) => throw new NotImplementedException();
    }
}
