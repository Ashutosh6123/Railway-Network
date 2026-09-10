using ReservationService.Entities;

namespace ReservationService.Repositories;

public interface IBookingPassengerRepository
{
    Task<List<BookingPassenger>> GetByBookingIdAsync(int bookingId);

    Task AddRangeAsync(List<BookingPassenger> passengers);
}
