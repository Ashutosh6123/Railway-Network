namespace ReservationService.Entities;

public class WaitlistEntry
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; }
}
