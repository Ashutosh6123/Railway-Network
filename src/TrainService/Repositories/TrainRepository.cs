using Microsoft.EntityFrameworkCore;
using TrainService.Data;
using TrainService.Entities;

namespace TrainService.Repositories;

public class TrainRepository(TrainDbContext dbContext) : ITrainRepository
{
    public Task<Train?> GetByIdAsync(int id) => dbContext.Trains.AsNoTracking().FirstOrDefaultAsync(train => train.Id == id);
    public Task<Train?> GetByTrainNumberAsync(string trainNumber) => dbContext.Trains.AsNoTracking().FirstOrDefaultAsync(train => train.TrainNumber == trainNumber);
    public Task<List<Train>> GetAllAsync() => dbContext.Trains.AsNoTracking().OrderBy(train => train.TrainNumber).ToListAsync();
    public Task<List<Train>> SearchAsync(string searchTerm) => dbContext.Trains.AsNoTracking().Where(train => train.TrainNumber.Contains(searchTerm) || train.Name.Contains(searchTerm)).OrderBy(train => train.TrainNumber).ToListAsync();
    public async Task AddAsync(Train train) { dbContext.Trains.Add(train); await dbContext.SaveChangesAsync(); }
    public async Task UpdateAsync(Train train) { dbContext.Trains.Update(train); await dbContext.SaveChangesAsync(); }
    public async Task DeleteAsync(Train train) { dbContext.Trains.Remove(train); await dbContext.SaveChangesAsync(); }
}
