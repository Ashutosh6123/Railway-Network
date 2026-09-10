using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
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
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            []
        }
    });
});

var reservationDbConnectionString = builder.Configuration.GetConnectionString("ReservationDb");
var userServiceBaseUrl = builder.Configuration["ServiceUrls:UserService"];
var trainServiceBaseUrl = builder.Configuration["ServiceUrls:TrainService"];
var paymentServiceBaseUrl = builder.Configuration["ServiceUrls:PaymentService"];
var mailServiceBaseUrl = builder.Configuration["ServiceUrls:MailService"];
var internalServiceApiKey = builder.Configuration["InternalService:ApiKey"];
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var httpClientTimeoutSeconds = builder.Configuration.GetValue("HttpClient:TimeoutSeconds", 30);

if (string.IsNullOrWhiteSpace(reservationDbConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:ReservationDb must be configured.");
}

if (!Uri.TryCreate(userServiceBaseUrl, UriKind.Absolute, out var userServiceUri) ||
    !Uri.TryCreate(trainServiceBaseUrl, UriKind.Absolute, out var trainServiceUri) ||
    !Uri.TryCreate(paymentServiceBaseUrl, UriKind.Absolute, out var paymentServiceUri) ||
    !Uri.TryCreate(mailServiceBaseUrl, UriKind.Absolute, out var mailServiceUri))
{
    throw new InvalidOperationException("All Reservation Service URLs must be configured.");
}

if (string.IsNullOrWhiteSpace(internalServiceApiKey))
{
    throw new InvalidOperationException("InternalService:ApiKey must be configured.");
}

if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("Jwt:Key, Jwt:Issuer, and Jwt:Audience must be configured.");
}

if (httpClientTimeoutSeconds <= 0)
{
    httpClientTimeoutSeconds = 30;
}

builder.Services.AddDbContext<ReservationService.Data.ReservationDbContext>(options =>
    options.UseSqlServer(reservationDbConnectionString));
builder.Services.AddScoped<ReservationService.Repositories.IBookingRepository, ReservationService.Repositories.BookingRepository>();
builder.Services.AddScoped<ReservationService.Repositories.IBookingPassengerRepository, ReservationService.Repositories.BookingPassengerRepository>();
builder.Services.AddScoped<ReservationService.Repositories.ISeatAllocationRepository, ReservationService.Repositories.SeatAllocationRepository>();
builder.Services.AddScoped<ReservationService.Repositories.IWaitlistRepository, ReservationService.Repositories.WaitlistRepository>();
builder.Services.AddScoped<ReservationService.Services.IAvailabilityService, ReservationService.Services.AvailabilityService>();
builder.Services.AddScoped<ReservationService.Services.IBookingService, ReservationService.Services.BookingService>();
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
builder.Services.AddHttpClient<ReservationService.Clients.IUserClient, ReservationService.Clients.UserClient>(client =>
{
    client.BaseAddress = userServiceUri;
    client.Timeout = TimeSpan.FromSeconds(httpClientTimeoutSeconds);
});
builder.Services.AddHttpClient<ReservationService.Clients.ITrainClient, ReservationService.Clients.TrainClient>(client =>
{
    client.BaseAddress = trainServiceUri;
    client.Timeout = TimeSpan.FromSeconds(httpClientTimeoutSeconds);
});
builder.Services.AddHttpClient<ReservationService.Clients.IPaymentClient, ReservationService.Clients.PaymentClient>(client =>
{
    client.BaseAddress = paymentServiceUri;
    client.Timeout = TimeSpan.FromSeconds(httpClientTimeoutSeconds);
});
builder.Services.AddHttpClient<ReservationService.Clients.IMailClient, ReservationService.Clients.MailClient>(client =>
{
    client.BaseAddress = mailServiceUri;
    client.Timeout = TimeSpan.FromSeconds(httpClientTimeoutSeconds);
});

var app = builder.Build();

app.UseMiddleware<ReservationService.Middleware.GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
