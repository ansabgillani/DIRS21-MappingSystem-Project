using MappingSystem.Abstractions;
using MappingSystems.Models.Reservations;
using MappingSystems.Partners.Google.Models;

namespace MappingSystems.Partners.Google.Implementation;

public class GoogleReservationWithGuestToReservationWithGuestMapper : ITypeMapper<GoogleReservationWithGuest, ReservationWithGuest>
{
    public ReservationWithGuest Map(GoogleReservationWithGuest source)
    {
        return new ReservationWithGuest
        {
            ReservationId = source.ReservationCode,
            CheckInDate = source.CheckInDate,
            Guest = source.Guest == null
                ? null
                : new GuestProfile
                {
                    GuestId = source.Guest.GoogleGuestId,
                    FullName = source.Guest.FullName
                }
        };
    }
}
