using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TrainService.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);

var trainDbConnectionString = builder.Configuration.GetConnectionString("TrainDb");
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(trainDbConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:TrainDb must be configured.");
}

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key must be configured.");
}

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
        In = ParameterLocation.Header
    });
    options.OperationFilter<AuthorizeOperationFilter>();
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<TrainService.Data.TrainDbContext>(options =>
    options.UseSqlServer(trainDbConnectionString));
builder.Services.AddScoped<TrainService.Repositories.ITrainRepository, TrainService.Repositories.TrainRepository>();
builder.Services.AddScoped<TrainService.Repositories.IStationRepository, TrainService.Repositories.StationRepository>();
builder.Services.AddScoped<TrainService.Repositories.IRouteStopRepository, TrainService.Repositories.RouteStopRepository>();
builder.Services.AddScoped<TrainService.Repositories.ICoachRepository, TrainService.Repositories.CoachRepository>();
builder.Services.AddScoped<TrainService.Repositories.ISeatRepository, TrainService.Repositories.SeatRepository>();
builder.Services.AddScoped<TrainService.Repositories.IFareRepository, TrainService.Repositories.FareRepository>();
builder.Services.AddScoped<TrainService.Services.ITrainService, TrainService.Services.TrainService>();
builder.Services.AddScoped<TrainService.Services.ITrainAdminService, TrainService.Services.TrainAdminService>();

var app = builder.Build();

app.UseMiddleware<TrainService.Middleware.GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
