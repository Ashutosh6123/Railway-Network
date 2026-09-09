using TrainService.Entities;
using TrainService.Enums;

namespace TrainService.Repositories;

public interface IFareRepository
{
    Task<Fare?> GetByIdAsync(int id);
    Task<List<Fare>> GetByTrainIdAsync(int trainId);
    Task<Fare?> GetFareAsync(int trainId, int fromStationId, int toStationId, CoachType coachType);
    Task AddAsync(Fare fare);
    Task UpdateAsync(Fare fare);
    Task DeleteAsync(Fare fare);
}
