using System.Net;
using System.Net.Http.Json;

namespace PaymentService.Clients;

public class DummyRazorpayClient(HttpClient httpClient) : IPaymentGateway
{
    public async Task<PaymentGatewayResult> ProcessPaymentAsync(
        string paymentReference,
        decimal amount,
        bool simulateSuccess)
    {
        var request = new DummyProcessPaymentRequest(paymentReference, amount, simulateSuccess);
        var response = await httpClient.PostAsJsonAsync("api/dummy-razorpay/process", request);

        // The simulator uses HTTP 400 to report its deterministic payment failure.
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.BadRequest)
        {
            throw new HttpRequestException(
                $"Dummy Razorpay payment request failed with status code {(int)response.StatusCode}.");
        }

        var providerResponse = await response.Content.ReadFromJsonAsync<DummyProcessPaymentResponse>();

        if (providerResponse is null)
        {
            throw new InvalidOperationException("Dummy Razorpay returned an empty payment response.");
        }

        return new PaymentGatewayResult(
            providerResponse.Success,
            providerResponse.ProviderTransactionReference,
            providerResponse.Message);
    }

    public async Task<RefundGatewayResult> RefundAsync(
        string providerTransactionReference,
        decimal amount)
    {
        var request = new DummyRefundRequest(providerTransactionReference, amount);
        var response = await httpClient.PostAsJsonAsync("api/dummy-razorpay/refund", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Dummy Razorpay refund request failed with status code {(int)response.StatusCode}.");
        }

        var providerResponse = await response.Content.ReadFromJsonAsync<DummyRefundResponse>();

        if (providerResponse is null)
        {
            throw new InvalidOperationException("Dummy Razorpay returned an empty refund response.");
        }

        return new RefundGatewayResult(
            providerResponse.Success,
            providerResponse.ProviderRefundReference,
            providerResponse.Message);
    }

    private record DummyProcessPaymentRequest(
        string PaymentReference,
        decimal Amount,
        bool SimulateSuccess);

    private record DummyProcessPaymentResponse(
        bool Success,
        string? ProviderTransactionReference,
        string Message);

    private record DummyRefundRequest(
        string ProviderTransactionReference,
        decimal Amount);

    private record DummyRefundResponse(
        bool Success,
        string? ProviderRefundReference,
        string Message);
}
