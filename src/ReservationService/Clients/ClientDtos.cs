using ReservationService.Enums;

namespace ReservationService.Clients;

public record UserClientDto(int Id, string Name, string Email, string PhoneNumber, string Role);

public record TrainClientDto(int Id, string TrainNumber, string Name);

public record RouteStopClientDto(
    int StopOrder,
    int StationId,
    string StationCode,
    string StationName,
    TimeSpan ArrivalTime,
    TimeSpan DepartureTime);

public record FareClientDto(
    int TrainId,
    int FromStationId,
    int ToStationId,
    CoachType CoachType,
    decimal Amount);

public record SeatInventoryClientDto(
    int CoachId,
    string CoachNumber,
    int SeatId,
    string SeatNumber);

public record PaymentClientResult(
    string BookingPnr,
    decimal Amount,
    int PaymentStatus,
    string? TransactionReference,
    int? RefundStatus);

public record MailClientResult(bool Success, string Message);
