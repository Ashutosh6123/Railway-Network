using ReservationService.Entities;
using ReservationService.Enums;

namespace ReservationService.Repositories;

public interface IWaitlistRepository
{
    Task<WaitlistEntry?> GetByBookingIdAsync(int bookingId);

    Task<List<WaitlistEntry>> GetOrderedByQueueAsync(int trainId, DateTime journeyDate, CoachType coachType);

    Task<int> GetNextPositionAsync(int trainId, DateTime journeyDate, CoachType coachType);

    Task AddAsync(WaitlistEntry entry);

    Task RemoveAndRenumberAsync(WaitlistEntry entry, int trainId, DateTime journeyDate, CoachType coachType);
}
