namespace MappingSystems.Partners.Google.Models;

public class GoogleReservation
{
    public string ReservationCode { get; set; } = string.Empty;

    public string PrimaryGuestName { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; }
}
