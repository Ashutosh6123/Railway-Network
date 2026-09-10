using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.DTOs;
using ReservationService.Services;

namespace ReservationService.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationController(
    IBookingService bookingService,
    IAvailabilityService availabilityService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingResponse>> Create(BookingRequest request)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await bookingService.CreateBookingAsync(userId, request));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{pnr}")]
    [Authorize]
    public async Task<ActionResult<ReservationDetailsResponse>> GetByPnr(string pnr)
    {
        if (string.IsNullOrWhiteSpace(pnr))
        {
            return BadRequest("PNR is required.");
        }

        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await bookingService.GetReservationAsync(userId, pnr));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("{pnr}/cancel")]
    [Authorize]
    public async Task<ActionResult<BookingResponse>> Cancel(string pnr)
    {
        if (string.IsNullOrWhiteSpace(pnr))
        {
            return BadRequest("PNR is required.");
        }

        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await bookingService.CancelBookingAsync(userId, pnr));
        }
        catch (InvalidOperationException exception) when (exception.Message == "Booking was not found.")
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("availability")]
    [AllowAnonymous]
    public async Task<ActionResult<AvailabilityResponse>> GetAvailability([FromQuery] AvailabilityRequest request)
    {
        if (request.TrainId <= 0 || request.FromStationId <= 0 || request.ToStationId <= 0 ||
            request.FromStationId == request.ToStationId || request.JourneyDate == default ||
            !Enum.IsDefined(request.CoachType))
        {
            return BadRequest("Availability request is invalid.");
        }

        try
        {
            var result = await availabilityService.GetAvailabilityAsync(
                request.TrainId,
                request.FromStationId,
                request.ToStationId,
                request.JourneyDate.Date,
                request.CoachType);
            return Ok(new AvailabilityResponse(result.AvailableSeats.Count));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out userId) && userId > 0;
    }
}
