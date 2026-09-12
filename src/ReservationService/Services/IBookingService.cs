using ReservationService.DTOs;
using ReservationService.Enums;

namespace ReservationService.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(int userId, BookingRequest request);

    Task<BookingResponse> CancelBookingAsync(int userId, string pnr);

    Task<ReservationDetailsResponse> GetReservationAsync(int userId, string pnr);

    Task<bool> PromoteEarliestWaitlistedBookingAsync(int trainId, DateTime journeyDate, CoachType coachType);
}
