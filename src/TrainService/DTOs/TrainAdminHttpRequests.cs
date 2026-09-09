using TrainService.Enums;

namespace TrainService.DTOs;

public record RouteStopRequest(int StationId, int StopOrder, TimeSpan ArrivalTime, TimeSpan DepartureTime);
public record CoachRequest(string CoachNumber, CoachType CoachType);
public record SeatRequest(string SeatNumber);
