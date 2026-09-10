using ReservationService.Clients;

namespace ReservationService.Services;

public record AvailableSeat(int CoachId, string CoachNumber, int SeatId, string SeatNumber);

public record AvailabilityResult(
    int FromStopOrder,
    int ToStopOrder,
    List<AvailableSeat> AvailableSeats);
