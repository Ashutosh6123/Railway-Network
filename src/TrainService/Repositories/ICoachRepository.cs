using TrainService.Entities;

namespace TrainService.Repositories;

public interface ICoachRepository
{
    Task<Coach?> GetByIdAsync(int id);
    Task<List<Coach>> GetByTrainIdAsync(int trainId);
    Task AddAsync(Coach coach);
    Task UpdateAsync(Coach coach);
    Task DeleteAsync(Coach coach);
}
