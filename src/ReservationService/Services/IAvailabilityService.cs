using ReservationService.Enums;

namespace ReservationService.Services;

public interface IAvailabilityService
{
    Task<AvailabilityResult> GetAvailabilityAsync(
        int trainId,
        int fromStationId,
        int toStationId,
        DateTime journeyDate,
        CoachType coachType);
}
