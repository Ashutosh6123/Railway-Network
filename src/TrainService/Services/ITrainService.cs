using TrainService.DTOs;
using TrainService.Enums;

namespace TrainService.Services;

public interface ITrainService
{
    Task<List<TrainDto>> SearchTrainsAsync(int fromStationId, int toStationId);
    Task<TrainDto> GetTrainAsync(int trainId);
    Task<List<RouteStopDto>> GetRouteAsync(int trainId);
    Task<FareDto> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType);
    Task<List<SeatInventoryDto>> GetSeatInventoryAsync(int trainId, CoachType coachType);
}
