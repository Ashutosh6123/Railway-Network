using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);

var trainDbConnectionString = builder.Configuration.GetConnectionString("TrainDb");

if (string.IsNullOrWhiteSpace(trainDbConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:TrainDb must be configured.");
}

builder.Services.AddDbContext<TrainService.Data.TrainDbContext>(options =>
    options.UseSqlServer(trainDbConnectionString));
builder.Services.AddScoped<TrainService.Repositories.ITrainRepository, TrainService.Repositories.TrainRepository>();
builder.Services.AddScoped<TrainService.Repositories.IStationRepository, TrainService.Repositories.StationRepository>();
builder.Services.AddScoped<TrainService.Repositories.IRouteStopRepository, TrainService.Repositories.RouteStopRepository>();
builder.Services.AddScoped<TrainService.Repositories.ICoachRepository, TrainService.Repositories.CoachRepository>();
builder.Services.AddScoped<TrainService.Repositories.ISeatRepository, TrainService.Repositories.SeatRepository>();
builder.Services.AddScoped<TrainService.Repositories.IFareRepository, TrainService.Repositories.FareRepository>();
builder.Services.AddScoped<TrainService.Services.ITrainService, TrainService.Services.TrainService>();

var app = builder.Build();

app.UseMiddleware<TrainService.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
