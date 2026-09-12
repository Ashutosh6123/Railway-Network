using ReservationService.Enums;

namespace ReservationService.DTOs;

public record BookingPassengerRequest(
    string Name,
    int Age,
    Gender Gender,
    string Address);

public record BookingRequest(
    int TrainId,
    int FromStationId,
    int ToStationId,
    DateTime JourneyDate,
    CoachType CoachType,
    QuotaType Quota,
    List<BookingPassengerRequest> Passengers);

public record BookingPassengerResponse(
    int BookingPassengerId,
    string Name,
    string? CoachNumber,
    string? SeatNumber);

public record BookingResponse(
    string Pnr,
    BookingStatus Status,
    decimal TotalFare,
    List<BookingPassengerResponse> Passengers,
    int? WaitlistPosition);

public record ReservationDetailsResponse(
    string Pnr,
    BookingStatus Status,
    int TrainId,
    int FromStationId,
    int ToStationId,
    DateTime JourneyDate,
    CoachType CoachType,
    QuotaType Quota,
    decimal TotalFare,
    List<BookingPassengerResponse> Passengers,
    int? WaitlistPosition);

public record AvailabilityRequest(
    int TrainId,
    int FromStationId,
    int ToStationId,
    DateTime JourneyDate,
    CoachType CoachType);

public record AvailabilityResponse(int AvailableSeats);
