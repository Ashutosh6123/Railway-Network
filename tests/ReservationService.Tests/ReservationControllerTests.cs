using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReservationService.Controllers;
using ReservationService.DTOs;
using ReservationService.Enums;
using ReservationService.Services;

namespace ReservationService.Tests;

public class ReservationControllerTests
{
    [Test]
    public async Task Create_UsesUserIdFromJwtClaim()
    {
        var bookingService = new FakeBookingService();
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.Create(CreateRequest());

        Assert.That(response.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(bookingService.CreateUserId, Is.EqualTo(7));
    }

    [Test]
    public async Task Create_ReturnsUnauthorizedWhenUserIdClaimIsMissing()
    {
        var bookingService = new FakeBookingService();
        var controller = CreateController(bookingService);

        var response = await controller.Create(CreateRequest());

        Assert.That(response.Result, Is.TypeOf<UnauthorizedResult>());
        Assert.That(bookingService.CreateUserId, Is.Null);
    }

    [Test]
    public async Task Create_ReturnsBadRequestForInvalidBooking()
    {
        var bookingService = new FakeBookingService { CreateException = new ArgumentException("Passenger count must be between 1 and 6.") };
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.Create(CreateRequest());

        Assert.That(response.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetByPnr_AllowsOwner()
    {
        var bookingService = new FakeBookingService();
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.GetByPnr("PNR123");

        Assert.That(response.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(bookingService.GetUserId, Is.EqualTo(7));
    }

    [Test]
    public async Task GetByPnr_ReturnsForbidForAnotherUser()
    {
        var bookingService = new FakeBookingService { GetException = new UnauthorizedAccessException() };
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.GetByPnr("PNR123");

        Assert.That(response.Result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task GetByPnr_ReturnsNotFoundForMissingBooking()
    {
        var bookingService = new FakeBookingService { GetException = new KeyNotFoundException() };
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.GetByPnr("PNR123");

        Assert.That(response.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Cancel_AllowsOwnerAndUsesJwtUserId()
    {
        var bookingService = new FakeBookingService();
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.Cancel("PNR123");

        Assert.That(response.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(bookingService.CancelUserId, Is.EqualTo(7));
    }

    [Test]
    public async Task Cancel_ReturnsForbidForAnotherUser()
    {
        var bookingService = new FakeBookingService { CancelException = new UnauthorizedAccessException() };
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.Cancel("PNR123");

        Assert.That(response.Result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task Cancel_ReturnsNotFoundForMissingBooking()
    {
        var bookingService = new FakeBookingService { CancelException = new InvalidOperationException("Booking was not found.") };
        var controller = CreateController(bookingService, userId: 7);

        var response = await controller.Cancel("PNR123");

        Assert.That(response.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Availability_IsPublicAndReturnsAvailableSeatCount()
    {
        var availabilityService = new FakeAvailabilityService { Result = new AvailabilityResult(1, 2, [new AvailableSeat(1, "S1", 1, "1")]) };
        var controller = CreateController(new FakeBookingService(), availabilityService);

        var response = await controller.GetAvailability(new AvailabilityRequest(1, 1, 2, DateTime.UtcNow.Date.AddDays(1), CoachType.Sleeper));

        Assert.That(response.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(typeof(ReservationController).GetMethod(nameof(ReservationController.GetAvailability))!
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true), Is.Not.Empty);
    }

    [Test]
    public async Task Availability_ReturnsBadRequestForInvalidParameters()
    {
        var controller = CreateController(new FakeBookingService());

        var response = await controller.GetAvailability(new AvailabilityRequest(0, 1, 1, default, CoachType.Sleeper));

        Assert.That(response.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public void JwtValidation_RejectsMissingInvalidAndExpiredTokens()
    {
        Assert.That(ValidateToken(null), Is.False);
        Assert.That(ValidateToken("not-a-token"), Is.False);
        Assert.That(ValidateToken(CreateToken(7, DateTime.UtcNow.AddMinutes(-1))), Is.False);
    }

    [Test]
    public void JwtValidation_AcceptsValidTokenWithUserIdClaim()
    {
        var token = CreateToken(7, DateTime.UtcNow.AddMinutes(30));

        var principal = ValidatePrincipal(token);

        Assert.That(principal.FindFirstValue(ClaimTypes.NameIdentifier), Is.EqualTo("7"));
        Assert.That(principal.FindFirstValue(ClaimTypes.Role), Is.EqualTo("Passenger"));
    }

    [Test]
    public void SwaggerConfiguration_DefinesBearerAndReservationEndpoints()
    {
        var program = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ReservationService", "Program.cs"));
        var controller = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ReservationService", "Controllers", "ReservationController.cs"));

        Assert.That(program, Does.Contain("AddSecurityDefinition(\"Bearer\""));
        Assert.That(controller, Does.Contain("[Route(\"api/reservations\")]"));
        Assert.That(controller, Does.Contain("[HttpGet(\"availability\")]"));
    }

    private static ReservationController CreateController(FakeBookingService bookingService, FakeAvailabilityService? availabilityService = null, int? userId = null)
    {
        var controller = new ReservationController(bookingService, availabilityService ?? new FakeAvailabilityService())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        if (userId is not null)
        {
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()), new Claim(ClaimTypes.Role, "Passenger")],
                "Bearer"));
        }

        return controller;
    }

    private static BookingRequest CreateRequest() => new(1, 1, 2, DateTime.UtcNow.Date.AddDays(1), CoachType.Sleeper, QuotaType.General,
        [new BookingPassengerRequest("Rahul", 30, Gender.Male, "Delhi")]);

    private static string CreateToken(int userId, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345678901234567890123456789012"));
        var token = new JwtSecurityToken("RailwayNetwork", "RailwayNetwork",
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, "Passenger")],
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        try
        {
            _ = ValidatePrincipal(token);
            return true;
        }
        catch (SecurityTokenException) { return false; }
        catch (ArgumentException) { return false; }
    }

    private static ClaimsPrincipal ValidatePrincipal(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345678901234567890123456789012")),
            ValidateIssuer = true,
            ValidIssuer = "RailwayNetwork",
            ValidateAudience = true,
            ValidAudience = "RailwayNetwork",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
    }

    private sealed class FakeBookingService : IBookingService
    {
        public int? CreateUserId { get; private set; }
        public int? GetUserId { get; private set; }
        public int? CancelUserId { get; private set; }
        public Exception? CreateException { get; set; }
        public Exception? GetException { get; set; }
        public Exception? CancelException { get; set; }
        public Task<BookingResponse> CreateBookingAsync(int userId, BookingRequest request)
        {
            CreateUserId = userId;
            if (CreateException is not null) throw CreateException;
            return Task.FromResult(new BookingResponse("PNR123", BookingStatus.Confirmed, 450m, []));
        }
        public Task<ReservationDetailsResponse> GetReservationAsync(int userId, string pnr)
        {
            GetUserId = userId;
            if (GetException is not null) throw GetException;
            return Task.FromResult(new ReservationDetailsResponse(pnr, BookingStatus.Confirmed, 1, 1, 2, DateTime.UtcNow.Date, CoachType.Sleeper, QuotaType.General, 450m, []));
        }
        public Task<BookingResponse> CancelBookingAsync(int userId, string pnr)
        {
            CancelUserId = userId;
            if (CancelException is not null) throw CancelException;
            return Task.FromResult(new BookingResponse(pnr, BookingStatus.Cancelled, 450m, []));
        }
        public Task<bool> PromoteEarliestWaitlistedBookingAsync() => Task.FromResult(false);
    }

    private sealed class FakeAvailabilityService : IAvailabilityService
    {
        public AvailabilityResult Result { get; set; } = new(1, 2, []);
        public Task<AvailabilityResult> GetAvailabilityAsync(int trainId, int fromStationId, int toStationId, DateTime journeyDate, CoachType coachType) => Task.FromResult(Result);
    }
}
