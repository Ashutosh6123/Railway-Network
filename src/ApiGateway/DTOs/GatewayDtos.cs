namespace ApiGateway.DTOs;

// These request and response shapes mirror the public downstream contracts.
// They exist only for Gateway request binding and Swagger documentation.
public record RegisterRequest(string Name, string Email, string PhoneNumber, string Password);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string Role, int ExpiresIn);
public record UserResponse(int Id, string Name, string Email, string PhoneNumber, string Role);

public enum CoachType
{
    General,
    Sleeper,
    AC3Tier,
    AC2Tier,
    AC1Tier
}

public enum QuotaType
{
    General,
    Ladies
}

public enum Gender
{
    Male,
    Female
}

public enum BookingStatus
{
    Confirmed,
    Waitlisted,
    Cancelled
}

public record TrainResponse(int Id, string TrainNumber, string Name);
public record RouteStopResponse(int StopOrder, int StationId, string StationCode, string StationName, TimeSpan ArrivalTime, TimeSpan DepartureTime);
public record FareResponse(int TrainId, int FromStationId, int ToStationId, CoachType CoachType, decimal Amount);

public record BookingPassengerRequest(string Name, int Age, Gender Gender, string Address);
public record BookingRequest(
    int TrainId,
    int FromStationId,
    int ToStationId,
    DateTime JourneyDate,
    CoachType CoachType,
    QuotaType Quota,
    List<BookingPassengerRequest> Passengers);
public record BookingPassengerResponse(int BookingPassengerId, string Name, string? CoachNumber, string? SeatNumber);
public record BookingResponse(string Pnr, BookingStatus Status, decimal TotalFare, List<BookingPassengerResponse> Passengers);
public record ReservationResponse(
    string Pnr,
    BookingStatus Status,
    int TrainId,
    int FromStationId,
    int ToStationId,
    DateTime JourneyDate,
    CoachType CoachType,
    QuotaType Quota,
    decimal TotalFare,
    List<BookingPassengerResponse> Passengers);
public record AvailabilityResponse(int AvailableSeats);

public record TrainAdminRequest(string TrainNumber, string Name);
public record StationAdminRequest(string Code, string Name);
public record RouteStopRequest(int StationId, int StopOrder, TimeSpan ArrivalTime, TimeSpan DepartureTime);
public record RouteStopAdminRequest(int TrainId, int StationId, int StopOrder, TimeSpan ArrivalTime, TimeSpan DepartureTime);
public record CoachRequest(string CoachNumber, CoachType CoachType);
public record CoachAdminRequest(int TrainId, string CoachNumber, CoachType CoachType);
public record SeatRequest(string SeatNumber);
public record SeatAdminRequest(int CoachId, string SeatNumber);
public record FareAdminRequest(int TrainId, int FromStationId, int ToStationId, CoachType CoachType, decimal Amount);
