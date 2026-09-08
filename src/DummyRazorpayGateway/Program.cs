var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<DummyRazorpayGateway.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
