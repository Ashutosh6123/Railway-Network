using ReservationService.Clients;
using ReservationService.Enums;
using ReservationService.Repositories;

namespace ReservationService.Services;

public class AvailabilityService(
    ITrainClient trainClient,
    ISeatAllocationRepository seatAllocationRepository) : IAvailabilityService
{
    public async Task<AvailabilityResult> GetAvailabilityAsync(
        int trainId,
        int fromStationId,
        int toStationId,
        DateTime journeyDate,
        CoachType coachType)
    {
        var routeStops = await trainClient.GetRouteAsync(trainId);
        var fromStop = routeStops.FirstOrDefault(stop => stop.StationId == fromStationId);
        var toStop = routeStops.FirstOrDefault(stop => stop.StationId == toStationId);

        if (fromStop is null || toStop is null)
        {
            throw new ArgumentException("Both stations must be served by the train.");
        }

        if (fromStop.StopOrder >= toStop.StopOrder)
        {
            throw new ArgumentException("Origin station must occur before destination station.");
        }

        var seats = await trainClient.GetSeatInventoryAsync(trainId, coachType);
        var activeAllocations = await seatAllocationRepository.GetActiveByTrainAndJourneyDateAsync(trainId, journeyDate.Date);
        var availableSeats = new List<AvailableSeat>();

        foreach (var seat in seats)
        {
            var hasOverlappingAllocation = activeAllocations.Any(allocation =>
            {
                var existingFromStop = routeStops.FirstOrDefault(stop => stop.StationId == allocation.FromStationId);
                var existingToStop = routeStops.FirstOrDefault(stop => stop.StationId == allocation.ToStationId);

                return existingFromStop is not null &&
                       existingToStop is not null &&
                       allocation.SeatId == seat.SeatId &&
                       existingFromStop.StopOrder < toStop.StopOrder &&
                       fromStop.StopOrder < existingToStop.StopOrder;
            });

            if (!hasOverlappingAllocation)
            {
                availableSeats.Add(new AvailableSeat(
                    seat.CoachId,
                    seat.CoachNumber,
                    seat.SeatId,
                    seat.SeatNumber));
            }
        }

        return new AvailabilityResult(fromStop.StopOrder, toStop.StopOrder, availableSeats);
    }
}
