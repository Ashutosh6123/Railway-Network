using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);

var reservationDbConnectionString = builder.Configuration.GetConnectionString("ReservationDb");
var userServiceBaseUrl = builder.Configuration["ServiceUrls:UserService"];
var trainServiceBaseUrl = builder.Configuration["ServiceUrls:TrainService"];
var paymentServiceBaseUrl = builder.Configuration["ServiceUrls:PaymentService"];
var mailServiceBaseUrl = builder.Configuration["ServiceUrls:MailService"];
var internalServiceApiKey = builder.Configuration["InternalService:ApiKey"];
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
app.UseHttpsRedirection();

app.Run();
