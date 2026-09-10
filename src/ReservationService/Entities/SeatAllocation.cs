namespace ReservationService.Entities;

public class SeatAllocation
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int BookingPassengerId { get; set; }

    public int CoachId { get; set; }

    public int SeatId { get; set; }

    public int FromStationId { get; set; }

    public int ToStationId { get; set; }
}
