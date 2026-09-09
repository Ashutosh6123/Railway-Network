using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainService.DTOs;
using TrainService.Services;

namespace TrainService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Administrator")]
public class TrainAdminController(ITrainAdminService trainAdminService) : ControllerBase
{
    [HttpPost("trains")]
    public async Task<ActionResult<TrainDto>> CreateTrain(TrainAdminRequest request)
    {
        var train = await trainAdminService.CreateTrainAsync(request);
        return CreatedAtAction(nameof(TrainController.GetTrain), "Train", new { trainId = train.Id }, train);
    }

    [HttpPut("trains/{trainId:int}")]
    public async Task<ActionResult<TrainDto>> UpdateTrain(int trainId, TrainAdminRequest request) =>
        Ok(await trainAdminService.UpdateTrainAsync(trainId, request));

    [HttpDelete("trains/{trainId:int}")]
    public async Task<IActionResult> DeleteTrain(int trainId)
    {
        await trainAdminService.DeleteTrainAsync(trainId);
        return NoContent();
    }

    [HttpPost("stations")]
    public async Task<IActionResult> CreateStation(StationAdminRequest request)
    {
        await trainAdminService.CreateStationAsync(request);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("stations/{stationId:int}")]
    public async Task<IActionResult> UpdateStation(int stationId, StationAdminRequest request)
    {
        await trainAdminService.UpdateStationAsync(stationId, request);
        return Ok();
    }

    [HttpDelete("stations/{stationId:int}")]
    public async Task<IActionResult> DeleteStation(int stationId)
    {
        await trainAdminService.DeleteStationAsync(stationId);
        return NoContent();
    }

    [HttpPost("trains/{trainId:int}/route-stops")]
    public async Task<IActionResult> AddRouteStop(int trainId, RouteStopRequest request)
    {
        await trainAdminService.AddRouteStopAsync(new(trainId, request.StationId, request.StopOrder, request.ArrivalTime, request.DepartureTime));
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("route-stops/{routeStopId:int}")]
    public async Task<IActionResult> UpdateRouteStop(int routeStopId, RouteStopAdminRequest request)
    {
        await trainAdminService.UpdateRouteStopAsync(routeStopId, request);
        return Ok();
    }

    [HttpDelete("route-stops/{routeStopId:int}")]
    public async Task<IActionResult> DeleteRouteStop(int routeStopId)
    {
        await trainAdminService.DeleteRouteStopAsync(routeStopId);
        return NoContent();
    }

    [HttpGet("trains/{trainId:int}/route-stops")]
    public async Task<ActionResult<List<RouteStopDto>>> GetRouteStops(int trainId) =>
        Ok(await trainAdminService.GetRouteStopsAsync(trainId));

    [HttpPost("trains/{trainId:int}/coaches")]
    public async Task<IActionResult> CreateCoach(int trainId, CoachRequest request)
    {
        await trainAdminService.CreateCoachAsync(new(trainId, request.CoachNumber, request.CoachType));
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("coaches/{coachId:int}")]
    public async Task<IActionResult> UpdateCoach(int coachId, CoachAdminRequest request)
    {
        await trainAdminService.UpdateCoachAsync(coachId, request);
        return Ok();
    }

    [HttpDelete("coaches/{coachId:int}")]
    public async Task<IActionResult> DeleteCoach(int coachId)
    {
        await trainAdminService.DeleteCoachAsync(coachId);
        return NoContent();
    }

    [HttpPost("coaches/{coachId:int}/seats")]
    public async Task<IActionResult> CreateSeat(int coachId, SeatRequest request)
    {
        await trainAdminService.CreateSeatAsync(new(coachId, request.SeatNumber));
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("seats/{seatId:int}")]
    public async Task<IActionResult> UpdateSeat(int seatId, SeatAdminRequest request)
    {
        await trainAdminService.UpdateSeatAsync(seatId, request);
        return Ok();
    }

    [HttpDelete("seats/{seatId:int}")]
    public async Task<IActionResult> DeleteSeat(int seatId)
    {
        await trainAdminService.DeleteSeatAsync(seatId);
        return NoContent();
    }

    [HttpPost("fares")]
    public async Task<IActionResult> CreateFare(FareAdminRequest request)
    {
        await trainAdminService.CreateFareAsync(request);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("fares/{fareId:int}")]
    public async Task<IActionResult> UpdateFare(int fareId, FareAdminRequest request)
    {
        await trainAdminService.UpdateFareAsync(fareId, request);
        return Ok();
    }

    [HttpDelete("fares/{fareId:int}")]
    public async Task<IActionResult> DeleteFare(int fareId)
    {
        await trainAdminService.DeleteFareAsync(fareId);
        return NoContent();
    }
}
