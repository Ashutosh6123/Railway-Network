using TrainService.Entities;

namespace TrainService.Repositories;

public interface IStationRepository
{
    Task<Station?> GetByIdAsync(int id);
    Task<Station?> GetByCodeAsync(string code);
    Task<List<Station>> GetAllAsync();
    Task AddAsync(Station station);
    Task UpdateAsync(Station station);
    Task DeleteAsync(Station station);
}
