using ReservationService.DTOs;

namespace ReservationService.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(int userId, BookingRequest request);
}
