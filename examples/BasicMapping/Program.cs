using Microsoft.Extensions.DependencyInjection;
using MappingSystem.Core;
using MappingSystem.Extensions;
using MappingSystems.Models.Reservations;
using MappingSystems.Partners.Google.Extensions;
using MappingSystems.Partners.Google.Models;

var services = new ServiceCollection();
services.AddMapping();
services.AddGooglePartnerMappings();
var provider = services.BuildServiceProvider();
var handler = provider.GetRequiredService<IMapHandler>();

var googleReservation = new GoogleReservation
{
	ReservationCode = "GOOGLE-BASIC-1",
	PrimaryGuestName = "Basic Guest",
	CheckInDate = DateTime.UtcNow.AddDays(2),
	CheckOutDate = DateTime.UtcNow.AddDays(4),
	NumberOfGuests = 1
};
var mappedReservation = handler.Map<GoogleReservation, Reservation>(googleReservation);

Console.WriteLine($"Mapped: {mappedReservation.ReservationId} / {mappedReservation.GuestName}");

var runtimeMapped = (Reservation)handler.Map(googleReservation, typeof(GoogleReservation), typeof(Reservation));
Console.WriteLine($"Runtime Mapped: {runtimeMapped.ReservationId} / {runtimeMapped.GuestName}");

try
{
	_ = handler.Map(new Reservation { ReservationId = "WRONG", GuestName = "Wrong" }, typeof(GoogleReservation), typeof(Reservation));
}
catch (ArgumentException ex)
{
	Console.WriteLine($"Guardrail: {ex.Message}");
}
