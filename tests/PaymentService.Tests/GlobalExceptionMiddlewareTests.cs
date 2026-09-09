using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PaymentService.Middleware;

namespace PaymentService.Tests;

public class GlobalExceptionMiddlewareTests
{
    [TestCase("Payment was not found.")]
    [TestCase("Only successful payments can be refunded.")]
    public async Task InvokeAsync_MapsExpectedBusinessErrorsToConflictInsteadOfServerError(string message)
    {
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new InvalidOperationException(message),
            NullLogger<GlobalExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }
}
