using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Entities;
using ReservationService.Enums;

namespace ReservationService.Repositories;

public class WaitlistRepository(ReservationDbContext dbContext) : IWaitlistRepository
{
    public Task<WaitlistEntry?> GetByBookingIdAsync(int bookingId)
    {
        return dbContext.WaitlistEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.BookingId == bookingId);
    }

    public Task<List<WaitlistEntry>> GetOrderedByQueueAsync(
        int trainId,
        DateTime journeyDate,
        CoachType coachType)
    {
        return GetQueueEntriesQuery(trainId, journeyDate, coachType)
            .OrderBy(entry => entry.Position)
            .ToListAsync();
    }

    public async Task<int> GetNextPositionAsync(
        int trainId,
        DateTime journeyDate,
        CoachType coachType)
    {
        var lastPosition = await GetQueueEntriesQuery(
                trainId,
                journeyDate,
                coachType)
            .Select(entry => (int?)entry.Position)
            .MaxAsync();

        return (lastPosition ?? 0) + 1;
    }

    public async Task AddAsync(WaitlistEntry entry)
    {
        dbContext.WaitlistEntries.Add(entry);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveAndRenumberAsync(
        WaitlistEntry entry,
        int trainId,
        DateTime journeyDate,
        CoachType coachType)
    {
        dbContext.WaitlistEntries.Remove(entry);
        await dbContext.SaveChangesAsync();

        var remainingEntries = await GetQueueEntriesQuery(
                trainId,
                journeyDate,
                coachType)
            .OrderBy(item => item.Position)
            .ThenBy(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .ToListAsync();

        for (var index = 0; index < remainingEntries.Count; index++)
        {
            remainingEntries[index].Position = index + 1;
        }

        await dbContext.SaveChangesAsync();
    }

    private IQueryable<WaitlistEntry> GetQueueEntriesQuery(
        int trainId,
        DateTime journeyDate,
        CoachType coachType)
    {
        return dbContext.WaitlistEntries.Where(entry =>
            dbContext.Bookings.Any(booking =>
                booking.Id == entry.BookingId &&
                booking.TrainId == trainId &&
                booking.JourneyDate == journeyDate.Date &&
                booking.CoachType == coachType));
    }
}
