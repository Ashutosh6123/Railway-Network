using ReservationService.Entities;

namespace ReservationService.Repositories;

public interface ISeatAllocationRepository
{
    Task<List<SeatAllocation>> GetActiveByTrainAndJourneyDateAsync(int trainId, DateTime journeyDate);

    Task<List<SeatAllocation>> GetByBookingIdAsync(int bookingId);

    Task AddRangeAsync(List<SeatAllocation> allocations);

    Task RemoveRangeAsync(List<SeatAllocation> allocations);
}
