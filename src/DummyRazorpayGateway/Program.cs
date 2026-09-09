var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DummyRazorpayGateway.Services.IPaymentGatewayService, DummyRazorpayGateway.Services.PaymentGatewayService>();

var app = builder.Build();

app.UseMiddleware<DummyRazorpayGateway.Middleware.GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
