using ReservationService.Enums;

namespace ReservationService.Entities;

public class BookingPassenger
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public string Address { get; set; } = string.Empty;

    public List<SeatAllocation> SeatAllocations { get; set; } = [];
}
