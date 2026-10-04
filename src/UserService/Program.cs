using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);

// builder.Configuration.AddEnvironmentVariables();

// builder.services manages the dependency injection container. We can register services here to be injected into controllers or other services.

// builder.Services.AddControllers() adds support for controllers in the application. Controllers are responsible for handling HTTP requests and returning responses.

// builder.Services.AddEndpointsApiExplorer() adds support for API endpoint exploration. This is useful for generating API documentation and testing endpoints. 

// builder.Services.AddSwaggerGen() adds support for generating Swagger documentation for the API. Swagger is a tool that helps document and test APIs. Swagger documentation provides a user-friendly interface to explore and test the API endpoints. It generates an interactive UI that allows developers to see the available endpoints, their request/response formats, and even test them directly from the browser.

// builder.Configuration.GetConnectionString("UserDb") retrieves the connection string for the UserDb database from the configuration. This connection string is used to connect to the database. Here in this file GetConnectionString uses "UserDb" as the key to retrieve the connection string from the configuration. The connection string is typically defined in the appsettings.json file or other configuration sources. It contains information such as the database server, database name, authentication credentials, and other parameters required to establish a connection to the database.


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var userDbConnectionString = builder.Configuration.GetConnectionString("UserDb");

if (string.IsNullOrWhiteSpace(userDbConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:UserDb must be configured.");
}

builder.Services.AddDbContext<UserService.Data.UserDbContext>(
    options => options.UseSqlServer(userDbConnectionString)
);

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
