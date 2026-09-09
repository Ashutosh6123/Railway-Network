namespace TrainService.Entities;

public class RouteStop
{
    public int Id { get; set; }

    public int TrainId { get; set; }

    public int StationId { get; set; }

    public int StopOrder { get; set; }

    public TimeSpan ArrivalTime { get; set; }

    public TimeSpan DepartureTime { get; set; }
}
