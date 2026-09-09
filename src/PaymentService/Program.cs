using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);

var paymentDbConnectionString = builder.Configuration.GetConnectionString("PaymentDb");
var dummyRazorpayBaseUrl = builder.Configuration["DummyRazorpay:BaseUrl"];
var internalServiceApiKey = builder.Configuration["InternalService:ApiKey"];
var dummyRazorpayTimeoutSeconds = builder.Configuration.GetValue("DummyRazorpay:TimeoutSeconds", 30);

if (string.IsNullOrWhiteSpace(paymentDbConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:PaymentDb must be configured.");
}

if (!Uri.TryCreate(dummyRazorpayBaseUrl, UriKind.Absolute, out var dummyRazorpayUri))
{
    throw new InvalidOperationException("DummyRazorpay:BaseUrl must be configured.");
}

if (string.IsNullOrWhiteSpace(internalServiceApiKey))
{
    throw new InvalidOperationException("InternalService:ApiKey must be configured.");
}

if (dummyRazorpayTimeoutSeconds <= 0)
{
    dummyRazorpayTimeoutSeconds = 30;
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PaymentService.Data.PaymentDbContext>(options =>
    options.UseSqlServer(paymentDbConnectionString));
builder.Services.AddScoped<PaymentService.Repositories.IPaymentRepository, PaymentService.Repositories.PaymentRepository>();
builder.Services.AddScoped<PaymentService.Services.IPaymentService, PaymentService.Services.PaymentService>();
builder.Services.AddHttpClient<PaymentService.Clients.IPaymentGateway, PaymentService.Clients.DummyRazorpayClient>(client =>
{
    client.BaseAddress = dummyRazorpayUri;
    client.Timeout = TimeSpan.FromSeconds(dummyRazorpayTimeoutSeconds);
});

var app = builder.Build();

app.UseMiddleware<PaymentService.Middleware.GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
