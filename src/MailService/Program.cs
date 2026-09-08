var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseMiddleware<MailService.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.Run();
