using TrainService.Enums;

namespace TrainService.Entities;

public class Coach
{
    public int Id { get; set; }

    public int TrainId { get; set; }

    public string CoachNumber { get; set; } = string.Empty;

    public CoachType CoachType { get; set; }
}
