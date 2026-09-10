using ReservationService.Entities;

namespace ReservationService.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);

    Task<Booking?> GetByPnrAsync(string pnr);

    Task AddAsync(Booking booking);

    Task UpdateAsync(Booking booking);
}
