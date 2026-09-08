var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<UserService.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
