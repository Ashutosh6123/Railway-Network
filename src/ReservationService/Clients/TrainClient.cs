using System.Net.Http.Json;
using ReservationService.Enums;

namespace ReservationService.Clients;

public class TrainClient(HttpClient httpClient, IConfiguration configuration) : ITrainClient
{
    public Task<TrainClientDto> GetTrainAsync(int trainId) =>
        GetRequiredAsync<TrainClientDto>($"api/trains/{trainId}");

    public Task<List<RouteStopClientDto>> GetRouteAsync(int trainId) =>
        GetRequiredAsync<List<RouteStopClientDto>>($"api/trains/{trainId}/route");

    public Task<FareClientDto> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType) =>
        GetRequiredAsync<FareClientDto>($"api/trains/{trainId}/fare?fromStationId={fromStationId}&toStationId={toStationId}&coachType={coachType}");

    public async Task<List<SeatInventoryClientDto>> GetSeatInventoryAsync(int trainId, CoachType coachType)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/internal/trains/{trainId}/seats?coachType={coachType}");
        request.Headers.Add("X-Internal-Service-Key", GetInternalServiceApiKey());

        using var response = await httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<List<SeatInventoryClientDto>>()
            ?? throw new HttpRequestException("Train Service returned an empty response.");
    }

    private async Task<T> GetRequiredAsync<T>(string requestUri)
    {
        using var response = await httpClient.GetAsync(requestUri);
        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new HttpRequestException("Train Service returned an empty response.");
    }

    private string GetInternalServiceApiKey() => configuration["InternalService:ApiKey"]
        ?? throw new InvalidOperationException("InternalService:ApiKey must be configured.");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Train Service returned status code {(int)response.StatusCode}: {content}");
        }
    }
}
