using ReservationService.Enums;

namespace ReservationService.Clients;

public interface ITrainClient
{
    Task<TrainClientDto> GetTrainAsync(int trainId);

    Task<List<RouteStopClientDto>> GetRouteAsync(int trainId);

    Task<FareClientDto> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType);

    Task<List<SeatInventoryClientDto>> GetSeatInventoryAsync(int trainId, CoachType coachType);
}
