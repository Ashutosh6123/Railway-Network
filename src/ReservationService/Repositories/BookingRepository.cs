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
