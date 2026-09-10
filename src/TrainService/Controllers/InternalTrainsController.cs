using Microsoft.AspNetCore.Mvc;
using TrainService.DTOs;
using TrainService.Enums;
using TrainService.Services;

namespace TrainService.Controllers;

[ApiController]
[Route("api/internal/trains")]
public class InternalTrainsController(ITrainService trainService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("{trainId:int}/seats")]
    public async Task<ActionResult<List<SeatInventoryDto>>> GetSeatInventory(
        int trainId,
        CoachType coachType,
        [FromHeader(Name = "X-Internal-Service-Key")] string? internalServiceKey)
    {
        if (!HasValidInternalServiceKey(internalServiceKey))
        {
            return Unauthorized();
        }

        return Ok(await trainService.GetSeatInventoryAsync(trainId, coachType));
    }

    private bool HasValidInternalServiceKey(string? suppliedKey)
    {
        var configuredKey = configuration["InternalService:ApiKey"];

        return !string.IsNullOrWhiteSpace(suppliedKey) &&
               !string.IsNullOrWhiteSpace(configuredKey) &&
               string.Equals(suppliedKey, configuredKey, StringComparison.Ordinal);
    }
}
