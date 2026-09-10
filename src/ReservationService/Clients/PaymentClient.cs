using System.Net.Http.Json;

namespace ReservationService.Clients;

public class PaymentClient(HttpClient httpClient, IConfiguration configuration) : IPaymentClient
{
    public Task<PaymentClientResult> ProcessPaymentAsync(string bookingPnr, decimal amount, string idempotencyKey) =>
        SendAsync("api/internal/payments/process", new ProcessPaymentRequest(bookingPnr, amount), idempotencyKey);

    public Task<PaymentClientResult> RefundAsync(string transactionReference, decimal amount, string idempotencyKey) =>
        SendAsync("api/internal/payments/refund", new RefundPaymentRequest(transactionReference, amount), idempotencyKey);

    private async Task<PaymentClientResult> SendAsync<TRequest>(string requestUri, TRequest requestBody, string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-Internal-Service-Key", GetInternalServiceApiKey());
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        using var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Payment Service returned status code {(int)response.StatusCode}: {content}");
        }

        return await response.Content.ReadFromJsonAsync<PaymentClientResult>()
            ?? throw new HttpRequestException("Payment Service returned an empty response.");
    }

    private string GetInternalServiceApiKey() => configuration["InternalService:ApiKey"]
        ?? throw new InvalidOperationException("InternalService:ApiKey must be configured.");

    private record ProcessPaymentRequest(string BookingPnr, decimal Amount);

    private record RefundPaymentRequest(string TransactionReference, decimal Amount);
}
