namespace DummyRazorpayGateway.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ErrorResponse(
                StatusCodes.Status400BadRequest,
                "ValidationError",
                exception.Message,
                context.TraceIdentifier));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ErrorResponse(
                StatusCodes.Status500InternalServerError,
                "UnexpectedError",
                "An unexpected error occurred.",
                context.TraceIdentifier));
        }
    }
}
