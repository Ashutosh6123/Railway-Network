var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<TrainService.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
