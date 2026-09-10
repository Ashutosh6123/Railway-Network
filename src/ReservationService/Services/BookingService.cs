using System.Data;
using Microsoft.EntityFrameworkCore;
using ReservationService.Clients;
using ReservationService.Data;
using ReservationService.DTOs;
using ReservationService.Entities;
using ReservationService.Enums;
using ReservationService.Repositories;

namespace ReservationService.Services;

public class BookingService(
    ReservationDbContext dbContext,
    IBookingRepository bookingRepository,
    IBookingPassengerRepository bookingPassengerRepository,
    ISeatAllocationRepository seatAllocationRepository,
    IWaitlistRepository waitlistRepository,
    IUserClient userClient,
    ITrainClient trainClient,
    IPaymentClient paymentClient,
    IMailClient mailClient,
    IAvailabilityService availabilityService,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateBookingAsync(int userId, BookingRequest request)
    {
        ValidateRequest(userId, request);

        var user = await userClient.GetUserAsync(userId)
            ?? throw new InvalidOperationException("User was not found.");

        await trainClient.GetTrainAsync(request.TrainId);
        var fare = await trainClient.GetFareAsync(
            request.TrainId,
            request.FromStationId,
            request.ToStationId,
            request.CoachType);
        var totalFare = fare.Amount * request.Passengers.Count;
        var pnr = await GenerateUniquePnrAsync();
        var initialAvailability = await availabilityService.GetAvailabilityAsync(
            request.TrainId,
            request.FromStationId,
            request.ToStationId,
            request.JourneyDate.Date,
            request.CoachType);
        var initiallyConfirmed = initialAvailability.AvailableSeats.Count >= request.Passengers.Count;
        var paymentKey = Guid.NewGuid().ToString("N");
        var payment = await paymentClient.ProcessPaymentAsync(pnr, totalFare, paymentKey);

        if (payment.PaymentStatus != 1 || string.IsNullOrWhiteSpace(payment.TransactionReference))
        {
            throw new InvalidOperationException("Payment failed. No reservation was created.");
        }

        try
        {
            var response = await PersistBookingAsync(userId, request, pnr, totalFare, initiallyConfirmed);
            await SendNotificationAsync(user.Email, response, request);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Reservation persistence failed after payment for PNR {Pnr}. Attempting compensating refund.", pnr);

            try
            {
                await paymentClient.RefundAsync(pnr, totalFare, Guid.NewGuid().ToString("N"));
            }
            catch (Exception refundException)
            {
                logger.LogError(refundException, "Compensating refund failed for PNR {Pnr}.", pnr);
            }

            throw;
        }
    }

    public async Task<BookingResponse> CancelBookingAsync(int userId, string pnr)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(pnr))
        {
            throw new ArgumentException("User ID and PNR are required.");
        }

        var booking = await bookingRepository.GetByPnrAsync(pnr)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("Only the booking owner can cancel this booking.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Booking is already cancelled.");
        }

        if (booking.Status is not (BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            throw new InvalidOperationException("Booking cannot be cancelled.");
        }

        await EnsureJourneyHasNotStartedAsync(booking);

        var passengers = await bookingPassengerRepository.GetByBookingIdAsync(booking.Id);
        var wasConfirmed = booking.Status == BookingStatus.Confirmed;

        await using (var transaction = await dbContext.BeginTransactionAsync())
        {
            var now = DateTime.UtcNow;
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = now;
            booking.UpdatedAt = now;
            await bookingRepository.UpdateAsync(booking);

            if (wasConfirmed)
            {
                var allocations = await seatAllocationRepository.GetByBookingIdAsync(booking.Id);
                await seatAllocationRepository.RemoveRangeAsync(allocations);
            }
            else
            {
                var waitlistEntry = await waitlistRepository.GetByBookingIdAsync(booking.Id);

                if (waitlistEntry is not null)
                {
                    await waitlistRepository.RemoveAsync(waitlistEntry);
                }
            }

            await transaction.CommitAsync();
        }

        // The refund is outside the Reservation database transaction because Payment Service has its own database.
        // If it fails, the cancellation remains recorded but this method reports the failure explicitly.
        try
        {
            var refund = await paymentClient.RefundAsync(booking.Pnr, booking.TotalFare, Guid.NewGuid().ToString("N"));

            if (refund.RefundStatus != 1)
            {
                throw new InvalidOperationException("Payment Service did not complete the refund.");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Refund failed for cancelled booking PNR {Pnr}.", booking.Pnr);
            throw new InvalidOperationException("Booking was cancelled, but the refund failed.", exception);
        }

        var response = CreateCancelledResponse(booking, passengers);
        await SendCancellationNotificationAsync(booking, response);

        if (wasConfirmed)
        {
            await PromoteEarliestWaitlistedBookingAsync();
        }

        return response;
    }

    public async Task<ReservationDetailsResponse> GetReservationAsync(int userId, string pnr)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(pnr))
        {
            throw new ArgumentException("User ID and PNR are required.");
        }

        var booking = await bookingRepository.GetByPnrAsync(pnr)
            ?? throw new KeyNotFoundException("Booking was not found.");

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("Only the booking owner can view this reservation.");
        }

        var passengers = await bookingPassengerRepository.GetByBookingIdAsync(booking.Id);
        var allocations = await seatAllocationRepository.GetByBookingIdAsync(booking.Id);
        var responses = passengers.Select(passenger =>
        {
            var allocation = allocations.FirstOrDefault(item => item.BookingPassengerId == passenger.Id);
            return new BookingPassengerResponse(
                passenger.Id,
                passenger.Name,
                allocation?.CoachId.ToString(),
                allocation?.SeatId.ToString());
        }).ToList();

        return new ReservationDetailsResponse(
            booking.Pnr,
            booking.Status,
            booking.TrainId,
            booking.FromStationId,
            booking.ToStationId,
            booking.JourneyDate,
            booking.CoachType,
            booking.Quota,
            booking.TotalFare,
            responses);
    }

    public async Task<bool> PromoteEarliestWaitlistedBookingAsync()
    {
        // SERIALIZABLE prevents two promotion requests from allocating the same released seat.
        await using var transaction = await dbContext.BeginTransactionAsync(IsolationLevel.Serializable);

        var waitlistEntries = await waitlistRepository.GetAllOrderedByPositionAsync();
        var earliestEntry = waitlistEntries.FirstOrDefault();

        if (earliestEntry is null)
        {
            return false;
        }

        var booking = await bookingRepository.GetByIdAsync(earliestEntry.BookingId);

        if (booking is null || booking.Status != BookingStatus.Waitlisted)
        {
            return false;
        }

        if (await HasJourneyStartedAsync(booking))
        {
            return false;
        }

        var passengers = await bookingPassengerRepository.GetByBookingIdAsync(booking.Id);
        var availability = await availabilityService.GetAvailabilityAsync(
            booking.TrainId,
            booking.FromStationId,
            booking.ToStationId,
            booking.JourneyDate.Date,
            booking.CoachType);

        // Strict FIFO: if this first booking does not fit completely, no later booking is considered.
        if (availability.AvailableSeats.Count < passengers.Count)
        {
            return false;
        }

        var allocations = new List<SeatAllocation>();

        for (var index = 0; index < passengers.Count; index++)
        {
            var seat = availability.AvailableSeats[index];
            allocations.Add(new SeatAllocation
            {
                BookingId = booking.Id,
                BookingPassengerId = passengers[index].Id,
                CoachId = seat.CoachId,
                SeatId = seat.SeatId,
                FromStationId = booking.FromStationId,
                ToStationId = booking.ToStationId
            });
        }

        await seatAllocationRepository.AddRangeAsync(allocations);
        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await bookingRepository.UpdateAsync(booking);
        await waitlistRepository.RemoveAsync(earliestEntry);
        await transaction.CommitAsync();

        await SendPromotionNotificationAsync(booking, passengers, availability.AvailableSeats);
        return true;
    }

    private async Task<BookingResponse> PersistBookingAsync(
        int userId,
        BookingRequest request,
        string pnr,
        decimal totalFare,
        bool initiallyConfirmed)
    {
        // SERIALIZABLE makes the final availability check and allocation one atomic operation.
        // A concurrent request must wait and re-check before it can allocate the same seat segment.
        await using var transaction = await dbContext.BeginTransactionAsync(IsolationLevel.Serializable);

        var finalAvailability = await availabilityService.GetAvailabilityAsync(
            request.TrainId,
            request.FromStationId,
            request.ToStationId,
            request.JourneyDate.Date,
            request.CoachType);
        var confirmed = initiallyConfirmed && finalAvailability.AvailableSeats.Count >= request.Passengers.Count;

        // If seats disappeared after payment, the paid booking is safely stored as waitlisted.
        // This avoids a second charge and never creates a partial confirmed booking.
        var now = DateTime.UtcNow;
        var booking = new Booking
        {
            Pnr = pnr,
            UserId = userId,
            TrainId = request.TrainId,
            FromStationId = request.FromStationId,
            ToStationId = request.ToStationId,
            JourneyDate = request.JourneyDate.Date,
            CoachType = request.CoachType,
            Quota = request.Quota,
            Status = confirmed ? BookingStatus.Confirmed : BookingStatus.Waitlisted,
            TotalFare = totalFare,
            CreatedAt = now,
            UpdatedAt = now
        };
        await bookingRepository.AddAsync(booking);

        var passengers = request.Passengers.Select(passenger => new BookingPassenger
        {
            BookingId = booking.Id,
            Name = passenger.Name,
            Age = passenger.Age,
            Gender = passenger.Gender,
            Address = passenger.Address
        }).ToList();
        await bookingPassengerRepository.AddRangeAsync(passengers);

        var responsePassengers = new List<BookingPassengerResponse>();

        if (confirmed)
        {
            var allocations = new List<SeatAllocation>();

            for (var index = 0; index < passengers.Count; index++)
            {
                var seat = finalAvailability.AvailableSeats[index];
                allocations.Add(new SeatAllocation
                {
                    BookingId = booking.Id,
                    BookingPassengerId = passengers[index].Id,
                    CoachId = seat.CoachId,
                    SeatId = seat.SeatId,
                    FromStationId = request.FromStationId,
                    ToStationId = request.ToStationId
                });
                responsePassengers.Add(new BookingPassengerResponse(
                    passengers[index].Id,
                    passengers[index].Name,
                    seat.CoachNumber,
                    seat.SeatNumber));
            }

            await seatAllocationRepository.AddRangeAsync(allocations);
        }
        else
        {
            var waitlistEntry = new WaitlistEntry
            {
                BookingId = booking.Id,
                Position = await waitlistRepository.GetNextPositionAsync(),
                CreatedAt = now
            };
            await waitlistRepository.AddAsync(waitlistEntry);

            responsePassengers.AddRange(passengers.Select(passenger => new BookingPassengerResponse(
                passenger.Id,
                passenger.Name,
                null,
                null)));
        }

        await transaction.CommitAsync();

        return new BookingResponse(booking.Pnr, booking.Status, booking.TotalFare, responsePassengers);
    }

    private async Task SendNotificationAsync(
        string email,
        BookingResponse response,
        BookingRequest request)
    {
        var data = new Dictionary<string, string>
        {
            ["pnr"] = response.Pnr,
            ["trainNumber"] = request.TrainId.ToString(),
            ["journeyDate"] = request.JourneyDate.Date.ToString("yyyy-MM-dd"),
            ["from"] = request.FromStationId.ToString(),
            ["to"] = request.ToStationId.ToString()
        };

        if (response.Status == BookingStatus.Confirmed && response.Passengers.Count > 0)
        {
            data["coach"] = response.Passengers[0].CoachNumber ?? string.Empty;
            data["seat"] = response.Passengers[0].SeatNumber ?? string.Empty;
        }

        try
        {
            var template = response.Status == BookingStatus.Confirmed ? "BookingConfirmed" : "BookingWaitlisted";
            await mailClient.SendAsync(email, template, data);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Notification failed for PNR {Pnr}. The booking remains successful.", response.Pnr);
        }
    }

    private async Task EnsureJourneyHasNotStartedAsync(Booking booking)
    {
        if (await HasJourneyStartedAsync(booking))
        {
            throw new InvalidOperationException("Booking cannot be cancelled after departure.");
        }
    }

    private async Task<bool> HasJourneyStartedAsync(Booking booking)
    {
        var routeStops = await trainClient.GetRouteAsync(booking.TrainId);
        var fromStop = routeStops.FirstOrDefault(stop => stop.StationId == booking.FromStationId)
            ?? throw new InvalidOperationException("Booking origin station was not found on the train route.");
        var scheduledDeparture = booking.JourneyDate.Date.Add(fromStop.DepartureTime);

        return DateTime.UtcNow >= scheduledDeparture;
    }

    private static BookingResponse CreateCancelledResponse(Booking booking, List<BookingPassenger> passengers)
    {
        return new BookingResponse(
            booking.Pnr,
            booking.Status,
            booking.TotalFare,
            passengers.Select(passenger => new BookingPassengerResponse(
                passenger.Id,
                passenger.Name,
                null,
                null)).ToList());
    }

    private async Task SendCancellationNotificationAsync(Booking booking, BookingResponse response)
    {
        try
        {
            var user = await userClient.GetUserAsync(booking.UserId)
                ?? throw new InvalidOperationException("User was not found.");
            await mailClient.SendAsync(user.Email, "Cancellation", CreateNotificationData(booking));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Cancellation notification failed for PNR {Pnr}. The cancellation remains successful.", response.Pnr);
        }
    }

    private async Task SendPromotionNotificationAsync(
        Booking booking,
        List<BookingPassenger> passengers,
        List<AvailableSeat> availableSeats)
    {
        try
        {
            var user = await userClient.GetUserAsync(booking.UserId)
                ?? throw new InvalidOperationException("User was not found.");
            var data = CreateNotificationData(booking);
            data["coach"] = availableSeats[0].CoachNumber;
            data["seat"] = availableSeats[0].SeatNumber;
            await mailClient.SendAsync(user.Email, "WaitlistPromotion", data);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Waitlist promotion notification failed for PNR {Pnr}. The promotion remains successful.", booking.Pnr);
        }
    }

    private static Dictionary<string, string> CreateNotificationData(Booking booking)
    {
        return new Dictionary<string, string>
        {
            ["pnr"] = booking.Pnr,
            ["trainNumber"] = booking.TrainId.ToString(),
            ["journeyDate"] = booking.JourneyDate.Date.ToString("yyyy-MM-dd"),
            ["from"] = booking.FromStationId.ToString(),
            ["to"] = booking.ToStationId.ToString()
        };
    }

    private async Task<string> GenerateUniquePnrAsync()
    {
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        while (true)
        {
            var pnr = string.Concat(Enumerable.Range(0, 8)
                .Select(_ => characters[Random.Shared.Next(characters.Length)]));

            if (await bookingRepository.GetByPnrAsync(pnr) is null)
            {
                return pnr;
            }
        }
    }

    private static void ValidateRequest(int userId, BookingRequest request)
    {
        if (userId <= 0) throw new ArgumentException("User ID must be positive.");
        if (request.Passengers is null || request.Passengers.Count is < 1 or > 6)
            throw new ArgumentException("Passenger count must be between 1 and 6.");
        if (request.TrainId <= 0 || request.FromStationId <= 0 || request.ToStationId <= 0)
            throw new ArgumentException("Train and station IDs must be positive.");
        if (request.FromStationId == request.ToStationId)
            throw new ArgumentException("Origin and destination stations must be different.");
        if (request.JourneyDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Journey date cannot be in the past.");
        if (!Enum.IsDefined(request.CoachType)) throw new ArgumentException("Coach type is invalid.");
        if (!Enum.IsDefined(request.Quota)) throw new ArgumentException("Quota is invalid.");

        foreach (var passenger in request.Passengers)
        {
            if (string.IsNullOrWhiteSpace(passenger.Name) || passenger.Age <= 0 ||
                string.IsNullOrWhiteSpace(passenger.Address) || !Enum.IsDefined(passenger.Gender))
                throw new ArgumentException("Passenger information is invalid.");
        }

        if (request.Quota == QuotaType.Ladies && request.Passengers.Any(passenger => passenger.Gender != Gender.Female))
            throw new ArgumentException("Ladies quota requires all passengers to be female.");
    }
}
