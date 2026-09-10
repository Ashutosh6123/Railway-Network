using System.Net.Http.Json;

namespace ReservationService.Clients;

public class MailClient(HttpClient httpClient, IConfiguration configuration) : IMailClient
{
    public async Task<MailClientResult> SendAsync(string to, string template, Dictionary<string, string> data)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/mail/send")
        {
            Content = JsonContent.Create(new SendMailRequest(to, template, data))
        };
        request.Headers.Add("X-Internal-Service-Key", GetInternalServiceApiKey());

        using var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Mail Service returned status code {(int)response.StatusCode}: {content}");
        }

        return await response.Content.ReadFromJsonAsync<MailClientResult>()
            ?? throw new HttpRequestException("Mail Service returned an empty response.");
    }

    private string GetInternalServiceApiKey() => configuration["InternalService:ApiKey"]
        ?? throw new InvalidOperationException("InternalService:ApiKey must be configured.");

    private record SendMailRequest(string To, string Template, Dictionary<string, string> Data);
}
