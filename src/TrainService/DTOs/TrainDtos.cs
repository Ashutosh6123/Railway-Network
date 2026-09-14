using TrainService.Enums;

namespace TrainService.DTOs;

public record TrainDto(int Id, string TrainNumber, string Name);

public record StationDto(
    int Id,
    string Code,
    string Name
);

public record RouteStopDto(
    int Id,
    int StopOrder,
    int StationId,
    string StationCode,
    string StationName,
    TimeSpan ArrivalTime,
    TimeSpan DepartureTime);

public record FareDto(
    int TrainId,
    int FromStationId,
    int ToStationId,
    CoachType CoachType,
    decimal Amount);

public record SeatInventoryDto(
    int CoachId,
    string CoachNumber,
    int SeatId,
    string SeatNumber);


