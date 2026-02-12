using MappingSystem.Abstractions;
using MappingSystems.Models.Reservations;
using MappingSystems.Partners.Google.Models;

namespace MappingSystems.Partners.Google.Implementation;

public class GoogleReservationToReservationMapper : ITypeMapper<GoogleReservation, Reservation>
{
    public Reservation Map(GoogleReservation source)
    {
        return new Reservation
        {
            ReservationId = source.ReservationCode,
            GuestName = source.PrimaryGuestName.ToUpperInvariant(),
            CheckInDate = source.CheckInDate,
            CheckOutDate = source.CheckOutDate,
            GuestCount = source.NumberOfGuests
        };
    }
}
