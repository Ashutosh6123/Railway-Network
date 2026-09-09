using TrainService.Entities;

namespace TrainService.Repositories;

public interface IRouteStopRepository
{
    Task<RouteStop?> GetByIdAsync(int id);
    Task<List<RouteStop>> GetByTrainIdAsync(int trainId);
    Task AddAsync(RouteStop routeStop);
    Task UpdateAsync(RouteStop routeStop);
    Task DeleteAsync(RouteStop routeStop);
}
