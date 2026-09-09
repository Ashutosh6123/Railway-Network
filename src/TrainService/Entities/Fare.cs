using TrainService.Enums;

namespace TrainService.Entities;

public class Fare
{
    public int Id { get; set; }

    public int TrainId { get; set; }

    public int FromStationId { get; set; }

    public int ToStationId { get; set; }

    public CoachType CoachType { get; set; }

    public decimal Amount { get; set; }
}
