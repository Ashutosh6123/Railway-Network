var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<ApiGateway.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
