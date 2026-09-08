namespace PaymentService.Middleware;

public sealed record ErrorResponse(int StatusCode, string Error, string Message, string TraceId);
