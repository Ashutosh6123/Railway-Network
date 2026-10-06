using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainService.DTOs;
using TrainService.Services;

namespace TrainService.Controllers;

[ApiController]
[Route("api/stations")]
public class StationController(ITrainService trainService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<StationDto>>> GetStations(
        [FromQuery] string? search)
    {
        var stations = await trainService.GetStationsAsync(search);
        return Ok(stations);
    }
}