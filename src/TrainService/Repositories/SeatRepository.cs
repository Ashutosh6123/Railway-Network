using Microsoft.EntityFrameworkCore;
using TrainService.Data;
using TrainService.Entities;

namespace TrainService.Repositories;

public class SeatRepository(TrainDbContext dbContext) : ISeatRepository
{
    public Task<Seat?> GetByIdAsync(int id) => dbContext.Seats.AsNoTracking().FirstOrDefaultAsync(seat => seat.Id == id);
    public Task<List<Seat>> GetByCoachIdAsync(int coachId) => dbContext.Seats.AsNoTracking().Where(seat => seat.CoachId == coachId).OrderBy(seat => seat.SeatNumber).ToListAsync();
    public async Task AddAsync(Seat seat) { dbContext.Seats.Add(seat); await dbContext.SaveChangesAsync(); }
    public async Task UpdateAsync(Seat seat) { dbContext.Seats.Update(seat); await dbContext.SaveChangesAsync(); }
    public async Task DeleteAsync(Seat seat) { dbContext.Seats.Remove(seat); await dbContext.SaveChangesAsync(); }
}
