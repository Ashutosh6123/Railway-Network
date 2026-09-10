using ReservationService.Entities;

namespace ReservationService.Repositories;

public interface IWaitlistRepository
{
    Task<WaitlistEntry?> GetByBookingIdAsync(int bookingId);

    Task<List<WaitlistEntry>> GetAllOrderedByPositionAsync();

    Task<int> GetNextPositionAsync();

    Task AddAsync(WaitlistEntry entry);

    Task RemoveAsync(WaitlistEntry entry);
}
