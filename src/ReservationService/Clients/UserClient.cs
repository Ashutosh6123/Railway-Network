using System.Net;
using System.Net.Http.Json;

namespace ReservationService.Clients;

public class UserClient(HttpClient httpClient, IConfiguration configuration) : IUserClient
{
    public async Task<UserClientDto?> GetUserAsync(int userId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/internal/users/{userId}");
        request.Headers.Add("X-Internal-Service-Key", GetInternalServiceApiKey());

        using var response = await httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, "User Service");

        return await response.Content.ReadFromJsonAsync<UserClientDto>()
            ?? throw new HttpRequestException("User Service returned an empty response.");
    }

    private string GetInternalServiceApiKey() => configuration["InternalService:ApiKey"]
        ?? throw new InvalidOperationException("InternalService:ApiKey must be configured.");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string serviceName)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{serviceName} returned status code {(int)response.StatusCode}: {content}");
        }
    }
}
