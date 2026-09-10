using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Entities;

namespace ReservationService.Repositories;

public class BookingPassengerRepository(ReservationDbContext dbContext) : IBookingPassengerRepository
{
    public Task<List<BookingPassenger>> GetByBookingIdAsync(int bookingId)
    {
        return dbContext.BookingPassengers
            .AsNoTracking()
            .Where(passenger => passenger.BookingId == bookingId)
            .OrderBy(passenger => passenger.Id)
            .ToListAsync();
    }

    public async Task AddRangeAsync(List<BookingPassenger> passengers)
    {
        dbContext.BookingPassengers.AddRange(passengers);
        await dbContext.SaveChangesAsync();
    }
}
