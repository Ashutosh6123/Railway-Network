using TrainService.Entities;

namespace TrainService.Repositories;

public interface ITrainRepository
{
    Task<Train?> GetByIdAsync(int id);
    Task<Train?> GetByTrainNumberAsync(string trainNumber);
    Task<List<Train>> GetAllAsync();
    Task<List<Train>> SearchAsync(string searchTerm);
    Task AddAsync(Train train);
    Task UpdateAsync(Train train);
    Task DeleteAsync(Train train);
}
