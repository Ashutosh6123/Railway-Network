using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Entities;

namespace ReservationService.Repositories;

public class WaitlistRepository(ReservationDbContext dbContext) : IWaitlistRepository
{
    public Task<WaitlistEntry?> GetByBookingIdAsync(int bookingId)
    {
        return dbContext.WaitlistEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.BookingId == bookingId);
    }

    public Task<List<WaitlistEntry>> GetAllOrderedByPositionAsync()
    {
        return dbContext.WaitlistEntries
            .AsNoTracking()
            .OrderBy(entry => entry.Position)
            .ToListAsync();
    }

    public async Task<int> GetNextPositionAsync()
    {
        var lastPosition = await dbContext.WaitlistEntries
            .Select(entry => (int?)entry.Position)
            .MaxAsync();

        return (lastPosition ?? 0) + 1;
    }

    public async Task AddAsync(WaitlistEntry entry)
    {
        dbContext.WaitlistEntries.Add(entry);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(WaitlistEntry entry)
    {
        dbContext.WaitlistEntries.Remove(entry);
        await dbContext.SaveChangesAsync();
    }
}
