using Microsoft.EntityFrameworkCore;
using TrainService.Data;
using TrainService.Entities;

namespace TrainService.Repositories;

public class CoachRepository(TrainDbContext dbContext) : ICoachRepository
{
    public Task<Coach?> GetByIdAsync(int id) => dbContext.Coaches.AsNoTracking().FirstOrDefaultAsync(coach => coach.Id == id);
    public Task<List<Coach>> GetByTrainIdAsync(int trainId) => dbContext.Coaches.AsNoTracking().Where(coach => coach.TrainId == trainId).OrderBy(coach => coach.CoachNumber).ToListAsync();
    public async Task AddAsync(Coach coach) { dbContext.Coaches.Add(coach); await dbContext.SaveChangesAsync(); }
    public async Task UpdateAsync(Coach coach) { dbContext.Coaches.Update(coach); await dbContext.SaveChangesAsync(); }
    public async Task DeleteAsync(Coach coach) { dbContext.Coaches.Remove(coach); await dbContext.SaveChangesAsync(); }
}
