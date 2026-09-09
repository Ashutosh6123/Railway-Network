using Microsoft.AspNetCore.Mvc;
using TrainService.DTOs;
using TrainService.Enums;
using TrainService.Services;

namespace TrainService.Controllers;

[ApiController]
[Route("api/trains")]
public class TrainController(ITrainService trainService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<List<TrainDto>>> Search(int fromStationId, int toStationId) =>
        Ok(await trainService.SearchTrainsAsync(fromStationId, toStationId));

    [HttpGet("{trainId:int}")]
    public async Task<ActionResult<TrainDto>> GetTrain(int trainId) =>
        Ok(await trainService.GetTrainAsync(trainId));

    [HttpGet("{trainId:int}/route")]
    public async Task<ActionResult<List<RouteStopDto>>> GetRoute(int trainId) =>
        Ok(await trainService.GetRouteAsync(trainId));

    [HttpGet("{trainId:int}/fare")]
    public async Task<ActionResult<FareDto>> GetFare(
        int trainId,
        int fromStationId,
        int toStationId,
        CoachType coachType) =>
        Ok(await trainService.GetFareAsync(trainId, fromStationId, toStationId, coachType));
}
