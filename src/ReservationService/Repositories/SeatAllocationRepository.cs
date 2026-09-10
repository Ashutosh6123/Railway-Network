using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Entities;
using ReservationService.Enums;

namespace ReservationService.Repositories;

public class SeatAllocationRepository(ReservationDbContext dbContext) : ISeatAllocationRepository
{
    public Task<List<SeatAllocation>> GetActiveByTrainAndJourneyDateAsync(int trainId, DateTime journeyDate)
    {
        return (
            from allocation in dbContext.SeatAllocations.AsNoTracking()
            join booking in dbContext.Bookings.AsNoTracking()
                on allocation.BookingId equals booking.Id
            where booking.TrainId == trainId &&
                  booking.JourneyDate == journeyDate &&
                  booking.Status != BookingStatus.Cancelled
            select allocation).ToListAsync();
    }

    public Task<List<SeatAllocation>> GetByBookingIdAsync(int bookingId)
    {
        return dbContext.SeatAllocations
            .AsNoTracking()
            .Where(allocation => allocation.BookingId == bookingId)
            .OrderBy(allocation => allocation.Id)
            .ToListAsync();
    }

    public async Task AddRangeAsync(List<SeatAllocation> allocations)
    {
        dbContext.SeatAllocations.AddRange(allocations);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveRangeAsync(List<SeatAllocation> allocations)
    {
        dbContext.SeatAllocations.RemoveRange(allocations);
        await dbContext.SaveChangesAsync();
    }
}
