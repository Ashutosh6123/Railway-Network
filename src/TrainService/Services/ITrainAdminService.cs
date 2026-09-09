using TrainService.DTOs;

namespace TrainService.Services;

public interface ITrainAdminService
{
    Task<TrainDto> CreateTrainAsync(TrainAdminRequest request);
    Task<TrainDto> UpdateTrainAsync(int trainId, TrainAdminRequest request);
    Task DeleteTrainAsync(int trainId);
    Task CreateStationAsync(StationAdminRequest request);
    Task UpdateStationAsync(int stationId, StationAdminRequest request);
    Task DeleteStationAsync(int stationId);
    Task AddRouteStopAsync(RouteStopAdminRequest request);
    Task UpdateRouteStopAsync(int routeStopId, RouteStopAdminRequest request);
    Task DeleteRouteStopAsync(int routeStopId);
    Task<List<RouteStopDto>> GetRouteStopsAsync(int trainId);
    Task CreateCoachAsync(CoachAdminRequest request);
    Task UpdateCoachAsync(int coachId, CoachAdminRequest request);
    Task DeleteCoachAsync(int coachId);
    Task CreateSeatAsync(SeatAdminRequest request);
    Task UpdateSeatAsync(int seatId, SeatAdminRequest request);
    Task DeleteSeatAsync(int seatId);
    Task CreateFareAsync(FareAdminRequest request);
    Task UpdateFareAsync(int fareId, FareAdminRequest request);
    Task DeleteFareAsync(int fareId);
}
