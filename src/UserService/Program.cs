using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var userDbConnectionString = builder.Configuration.GetConnectionString("UserDb");

if (string.IsNullOrWhiteSpace(userDbConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:UserDb must be configured.");
}

builder.Services.AddDbContext<UserService.Data.UserDbContext>(options =>
    options.UseSqlServer(userDbConnectionString));
builder.Services.AddScoped<UserService.Repositories.IUserRepository,UserService.Repositories.UserRepository>();
builder.Services.AddScoped<UserService.Repositories.IRoleRepository,UserService.Repositories.RoleRepository>();
builder.Services.AddScoped<UserService.Services.IUserService,UserService.Services.UserService>();
builder.Services.AddScoped<UserService.Services.IAuthService,UserService.Services.AuthService>();

var app = builder.Build();

app.UseMiddleware<UserService.Middleware.GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
