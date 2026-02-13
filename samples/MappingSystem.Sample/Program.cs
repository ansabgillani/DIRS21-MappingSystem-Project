using MappingSystem.Core;
using MappingSystem.Extensions;
using MappingSystems.Models.Reservations;
using MappingSystems.Partners.Google.Extensions;
using MappingSystems.Partners.Google.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
services.AddMapping();
services.AddGooglePartnerMappings();
var provider = services.BuildServiceProvider();

var handler = provider.GetRequiredService<IMapHandler>();

var googleReservation = new GoogleReservation
{
    ReservationCode = "GOO-12345",
    PrimaryGuestName = "Alice Johnson",
    CheckInDate = DateTime.UtcNow.AddDays(7),
    CheckOutDate = DateTime.UtcNow.AddDays(10),
    NumberOfGuests = 2
};

Console.WriteLine("Mapping Google -> Internal Model...");
var internalReservation = handler.Map<GoogleReservation, Reservation>(googleReservation);

Console.WriteLine($"ReservationId: {internalReservation.ReservationId}");
Console.WriteLine($"GuestName: {internalReservation.GuestName}");
Console.WriteLine($"CheckIn: {internalReservation.CheckInDate:yyyy-MM-dd}");
Console.WriteLine($"Guests: {internalReservation.GuestCount}");
Console.WriteLine();

Console.WriteLine("Mapping again (cached)...");
var cachedReservation = handler.Map<GoogleReservation, Reservation>(googleReservation);
Console.WriteLine($"Second mapping successful: {cachedReservation.GuestName}");

Console.WriteLine();
Console.WriteLine("=== Nested Object Mapping ===");
var googleReservationWithGuest = new GoogleReservationWithGuest
{
    ReservationCode = "GOO-999",
    CheckInDate = DateTime.UtcNow.AddDays(5),
    Guest = new GoogleGuestProfile
    {
        GoogleGuestId = "GOOGLE-GUEST-777",
        FullName = "Bob Smith"
    }
};

var mappedGuestReservation = handler.Map<GoogleReservationWithGuest, ReservationWithGuest>(googleReservationWithGuest);
Console.WriteLine($"Mapped reservation: {mappedGuestReservation.ReservationId}");
Console.WriteLine($"Guest: {mappedGuestReservation.Guest?.FullName}");
