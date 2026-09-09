namespace TrainService.Entities;

public class Seat
{
    public int Id { get; set; }

    public int CoachId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;
}
