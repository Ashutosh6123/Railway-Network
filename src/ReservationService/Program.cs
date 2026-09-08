var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<ReservationService.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
