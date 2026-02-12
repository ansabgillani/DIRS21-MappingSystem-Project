namespace MappingSystems.Partners.Google.Models;

public class GoogleReservationWithGuest
{
    public string ReservationCode { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public GoogleGuestProfile? Guest { get; set; }
}

public class GoogleGuestProfile
{
    public string GoogleGuestId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}
