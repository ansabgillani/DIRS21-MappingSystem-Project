namespace MappingSystems.Models.Reservations;

public class Reservation
{
    public string ReservationId { get; set; } = string.Empty;

    public string GuestName { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int GuestCount { get; set; }
}
