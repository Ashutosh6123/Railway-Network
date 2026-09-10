using System.Text;
using ApiGateway.DTOs;
using ApiGateway.Services;
using ApiGateway.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

var userServiceBaseUrl = builder.Configuration["ServiceUrls:UserService"];
var trainServiceBaseUrl = builder.Configuration["ServiceUrls:TrainService"];
var reservationServiceBaseUrl = builder.Configuration["ServiceUrls:ReservationService"];
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (!Uri.TryCreate(userServiceBaseUrl, UriKind.Absolute, out var userServiceUri) ||
    !Uri.TryCreate(trainServiceBaseUrl, UriKind.Absolute, out var trainServiceUri) ||
    !Uri.TryCreate(reservationServiceBaseUrl, UriKind.Absolute, out var reservationServiceUri))
{
    throw new InvalidOperationException("Gateway service URLs must be configured.");
}

if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("Jwt:Key, Jwt:Issuer, and Jwt:Audience must be configured.");
}

builder.Services.AddHttpClient("UserService", client => client.BaseAddress = userServiceUri);
builder.Services.AddHttpClient("TrainService", client => client.BaseAddress = trainServiceUri);
builder.Services.AddHttpClient("ReservationService", client => client.BaseAddress = reservationServiceUri);
builder.Services.AddSingleton<GatewayProxy>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    options.OperationFilter<AuthorizeOperationFilter>();
    options.SchemaFilter<CoachTypeSchemaFilter>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ApiGateway.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Authentication
app.MapPost("/api/auth/register", (HttpContext context, RegisterRequest request, GatewayProxy proxy) =>
        proxy.ForwardJsonAsync(context, "UserService", request))
    .WithTags("Authentication")
    .Produces<UserResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.MapPost("/api/auth/login", (HttpContext context, LoginRequest request, GatewayProxy proxy) =>
        proxy.ForwardJsonAsync(context, "UserService", request))
    .WithTags("Authentication")
    .Produces<LoginResponse>()
    .Produces(StatusCodes.Status401Unauthorized);

// Public train information
app.MapGet("/api/trains/search", (HttpContext context, int fromStationId, int toStationId, GatewayProxy proxy) =>
        proxy.ForwardAsync(context, "TrainService"))
    .WithTags("Trains")
    .Produces<List<TrainResponse>>();

app.MapGet("/api/trains/{trainId:int}", (HttpContext context, int trainId, GatewayProxy proxy) =>
        proxy.ForwardAsync(context, "TrainService"))
    .WithTags("Trains")
    .Produces<TrainResponse>()
    .Produces(StatusCodes.Status404NotFound);

app.MapGet("/api/trains/{trainId:int}/route", (HttpContext context, int trainId, GatewayProxy proxy) =>
        proxy.ForwardAsync(context, "TrainService"))
    .WithTags("Trains")
    .Produces<List<RouteStopResponse>>();

app.MapGet("/api/trains/{trainId:int}/fare", (HttpContext context, int trainId, int fromStationId, int toStationId, CoachType coachType, GatewayProxy proxy) =>
        proxy.ForwardAsync(context, "TrainService"))
    .WithTags("Trains")
    .Produces<FareResponse>();

// Public reservation availability
app.MapGet("/api/reservations/availability", (
        HttpContext context,
        int trainId,
        int fromStationId,
        int toStationId,
        DateTime journeyDate,
        CoachType coachType,
        GatewayProxy proxy) => proxy.ForwardAsync(context, "ReservationService"))
    .WithTags("Reservations")
    .Produces<AvailabilityResponse>()
    .Produces(StatusCodes.Status400BadRequest);

// Passenger reservation operations
app.MapPost("/api/reservations", (HttpContext context, BookingRequest request, GatewayProxy proxy) =>
        proxy.ForwardJsonAsync(context, "ReservationService", request))
    .WithTags("Reservations")
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Passenger" })
    .Produces<BookingResponse>()
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized);

app.MapGet("/api/reservations/{pnr}", (HttpContext context, string pnr, GatewayProxy proxy) =>
        proxy.ForwardAsync(context, "ReservationService"))
    .WithTags("Reservations")
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Passenger" })
    .Produces<ReservationResponse>()
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden)
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/reservations/{pnr}/cancel", (HttpContext context, string pnr, GatewayProxy proxy) =>
        proxy.ForwardAsync(context, "ReservationService"))
    .WithTags("Reservations")
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Passenger" })
    .Produces<BookingResponse>()
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden)
    .Produces(StatusCodes.Status404NotFound);

// Administrator train-management operations
var admin = app.MapGroup("/api/admin")
    .WithTags("Administration")
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrator" });

admin.MapPost("/trains", (HttpContext context, TrainAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request)).Produces<TrainResponse>(StatusCodes.Status201Created);
admin.MapPut("/trains/{trainId:int}", (HttpContext context, int trainId, TrainAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request)).Produces<TrainResponse>();
admin.MapDelete("/trains/{trainId:int}", (HttpContext context, int trainId, GatewayProxy proxy) => proxy.ForwardAsync(context, "TrainService")).Produces(StatusCodes.Status204NoContent);
admin.MapPost("/stations", (HttpContext context, StationAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request)).Produces(StatusCodes.Status201Created);
admin.MapPut("/stations/{stationId:int}", (HttpContext context, int stationId, StationAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request));
admin.MapDelete("/stations/{stationId:int}", (HttpContext context, int stationId, GatewayProxy proxy) => proxy.ForwardAsync(context, "TrainService")).Produces(StatusCodes.Status204NoContent);
admin.MapPost("/trains/{trainId:int}/route-stops", (HttpContext context, int trainId, RouteStopRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request)).Produces(StatusCodes.Status201Created);
admin.MapPut("/route-stops/{routeStopId:int}", (HttpContext context, int routeStopId, RouteStopAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request));
admin.MapDelete("/route-stops/{routeStopId:int}", (HttpContext context, int routeStopId, GatewayProxy proxy) => proxy.ForwardAsync(context, "TrainService")).Produces(StatusCodes.Status204NoContent);
admin.MapGet("/trains/{trainId:int}/route-stops", (HttpContext context, int trainId, GatewayProxy proxy) => proxy.ForwardAsync(context, "TrainService")).Produces<List<RouteStopResponse>>();
admin.MapPost("/trains/{trainId:int}/coaches", (HttpContext context, int trainId, CoachRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request)).Produces(StatusCodes.Status201Created);
admin.MapPut("/coaches/{coachId:int}", (HttpContext context, int coachId, CoachAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request));
admin.MapDelete("/coaches/{coachId:int}", (HttpContext context, int coachId, GatewayProxy proxy) => proxy.ForwardAsync(context, "TrainService")).Produces(StatusCodes.Status204NoContent);
admin.MapPost("/coaches/{coachId:int}/seats", (HttpContext context, int coachId, SeatRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request)).Produces(StatusCodes.Status201Created);
admin.MapPut("/seats/{seatId:int}", (HttpContext context, int seatId, SeatAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request));
admin.MapDelete("/seats/{seatId:int}", (HttpContext context, int seatId, GatewayProxy proxy) => proxy.ForwardAsync(context, "TrainService")).Produces(StatusCodes.Status204NoContent);
admin.MapPost("/fares", (HttpContext context, FareAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request)).Produces(StatusCodes.Status201Created);
admin.MapPut("/fares/{fareId:int}", (HttpContext context, int fareId, FareAdminRequest request, GatewayProxy proxy) => proxy.ForwardJsonAsync(context, "TrainService", request));
admin.MapDelete("/fares/{fareId:int}", (HttpContext context, int fareId, GatewayProxy proxy) => proxy.ForwardAsync(context, "TrainService")).Produces(StatusCodes.Status204NoContent);

app.Run();
