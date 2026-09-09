using Microsoft.EntityFrameworkCore;
using TrainService.Data;
using TrainService.Entities;

namespace TrainService.Repositories;

public class StationRepository(TrainDbContext dbContext) : IStationRepository
{
    public Task<Station?> GetByIdAsync(int id) => dbContext.Stations.AsNoTracking().FirstOrDefaultAsync(station => station.Id == id);
    public Task<Station?> GetByCodeAsync(string code) => dbContext.Stations.AsNoTracking().FirstOrDefaultAsync(station => station.Code == code);
    public Task<List<Station>> GetAllAsync() => dbContext.Stations.AsNoTracking().OrderBy(station => station.Code).ToListAsync();
    public async Task AddAsync(Station station) { dbContext.Stations.Add(station); await dbContext.SaveChangesAsync(); }
    public async Task UpdateAsync(Station station) { dbContext.Stations.Update(station); await dbContext.SaveChangesAsync(); }
    public async Task DeleteAsync(Station station) { dbContext.Stations.Remove(station); await dbContext.SaveChangesAsync(); }
}
