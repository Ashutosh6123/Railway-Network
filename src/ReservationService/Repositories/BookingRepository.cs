using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Entities;

namespace ReservationService.Repositories;

public class BookingRepository(ReservationDbContext dbContext) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(int id)
    {
        return dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.Id == id);
    }

    public Task<Booking?> GetByPnrAsync(string pnr)
    {
        return dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.Pnr == pnr);
    }

    public Task<List<Booking>> GetByUserIdAsync(int userId)
    {
        return dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.JourneyDate)
            .ToListAsync();
    }

    public async Task AddAsync(Booking booking)
    {
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        dbContext.Bookings.Update(booking);
        await dbContext.SaveChangesAsync();
    }
}
