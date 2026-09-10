using ReservationService.Enums;

namespace ReservationService.Entities;

public class Booking
{
    public int Id { get; set; }

    public string Pnr { get; set; } = string.Empty;

    public int UserId { get; set; }

    public int TrainId { get; set; }

    public int FromStationId { get; set; }

    public int ToStationId { get; set; }

    public DateTime JourneyDate { get; set; }

    public CoachType CoachType { get; set; }

    public QuotaType Quota { get; set; }

    public BookingStatus Status { get; set; }

    public decimal TotalFare { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public List<BookingPassenger> Passengers { get; set; } = [];

    public List<SeatAllocation> SeatAllocations { get; set; } = [];

    public WaitlistEntry? WaitlistEntry { get; set; }
}
