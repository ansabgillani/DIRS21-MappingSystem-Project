namespace MappingSystems.Models.Reservations;

public class ReservationWithGuest
{
    public string ReservationId { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public GuestProfile? Guest { get; set; }
}

public class GuestProfile
{
    public string GuestId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}
