var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MailService.Email.SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<MailService.Email.ISmtpMailSender, MailService.Email.SmtpMailSender>();
builder.Services.AddScoped<MailService.Services.IMailNotificationService, MailService.Services.MailNotificationService>();

var app = builder.Build();

app.UseMiddleware<MailService.Middleware.GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
