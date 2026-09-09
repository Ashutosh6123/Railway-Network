using Microsoft.EntityFrameworkCore;
using TrainService.Data;
using TrainService.Entities;
using TrainService.Enums;

namespace TrainService.Repositories;

public class FareRepository(TrainDbContext dbContext) : IFareRepository
{
    public Task<Fare?> GetByIdAsync(int id) => dbContext.Fares.AsNoTracking().FirstOrDefaultAsync(fare => fare.Id == id);
    public Task<List<Fare>> GetByTrainIdAsync(int trainId) => dbContext.Fares.AsNoTracking().Where(fare => fare.TrainId == trainId).ToListAsync();
    public Task<Fare?> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType) => dbContext.Fares.AsNoTracking().FirstOrDefaultAsync(fare => fare.TrainId == trainId && fare.FromStationId == fromStationId && fare.ToStationId == toStationId && fare.CoachType == coachType);
    public async Task AddAsync(Fare fare) { dbContext.Fares.Add(fare); await dbContext.SaveChangesAsync(); }
    public async Task UpdateAsync(Fare fare) { dbContext.Fares.Update(fare); await dbContext.SaveChangesAsync(); }
    public async Task DeleteAsync(Fare fare) { dbContext.Fares.Remove(fare); await dbContext.SaveChangesAsync(); }
}
