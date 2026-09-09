using TrainService.Entities;

namespace TrainService.Repositories;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(int id);
    Task<List<Seat>> GetByCoachIdAsync(int coachId);
    Task AddAsync(Seat seat);
    Task UpdateAsync(Seat seat);
    Task DeleteAsync(Seat seat);
}
