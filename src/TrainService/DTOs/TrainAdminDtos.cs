using TrainService.Enums;

namespace TrainService.DTOs;

public record TrainAdminRequest(string TrainNumber, string Name);
public record StationAdminRequest(string Code, string Name);
public record RouteStopAdminRequest(int TrainId, int StationId, int StopOrder, TimeSpan ArrivalTime, TimeSpan DepartureTime);
public record CoachAdminRequest(int TrainId, string CoachNumber, CoachType CoachType);
public record SeatAdminRequest(int CoachId, string SeatNumber);
public record FareAdminRequest(int TrainId, int FromStationId, int ToStationId, CoachType CoachType, decimal Amount);
