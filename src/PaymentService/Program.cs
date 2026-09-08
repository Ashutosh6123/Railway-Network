var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<PaymentService.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
