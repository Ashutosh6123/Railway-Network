using Microsoft.EntityFrameworkCore;
using TrainService.Data;
using TrainService.Entities;

namespace TrainService.Repositories;

public class RouteStopRepository(TrainDbContext dbContext) : IRouteStopRepository
{
    public Task<RouteStop?> GetByIdAsync(int id) => dbContext.RouteStops.AsNoTracking().FirstOrDefaultAsync(routeStop => routeStop.Id == id);
    public Task<List<RouteStop>> GetByTrainIdAsync(int trainId) => dbContext.RouteStops.AsNoTracking().Where(routeStop => routeStop.TrainId == trainId).OrderBy(routeStop => routeStop.StopOrder).ToListAsync();
    public async Task AddAsync(RouteStop routeStop) { dbContext.RouteStops.Add(routeStop); await dbContext.SaveChangesAsync(); }
    public async Task UpdateAsync(RouteStop routeStop) { dbContext.RouteStops.Update(routeStop); await dbContext.SaveChangesAsync(); }
    public async Task DeleteAsync(RouteStop routeStop) { dbContext.RouteStops.Remove(routeStop); await dbContext.SaveChangesAsync(); }
}
