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
using var scope = provider.CreateScope();
var handler = scope.ServiceProvider.GetRequiredService<IMapHandler>();

var googleReservation = new GoogleReservation
{
	ReservationCode = "GOOGLE-EXPLICIT-1",
	PrimaryGuestName = "explicit",
	CheckInDate = DateTime.UtcNow.AddDays(1),
	CheckOutDate = DateTime.UtcNow.AddDays(3),
	NumberOfGuests = 2
};
var mappedReservation = handler.Map<GoogleReservation, Reservation>(googleReservation);
var objectGenericMappedReservation = handler.Map<GoogleReservation, Reservation>((object)googleReservation);
var runtimeMappedReservation = (Reservation)handler.Map(googleReservation, typeof(GoogleReservation), typeof(Reservation));
var runtimeStringMappedReservation = (Reservation)handler.Map(
	googleReservation,
	typeof(GoogleReservation).FullName!,
	typeof(Reservation).FullName!);

Console.WriteLine($"Generic Explicit Mapper: {mappedReservation.GuestName}");
Console.WriteLine($"Generic<Object> Explicit Mapper: {objectGenericMappedReservation.GuestName}");
Console.WriteLine($"Runtime Explicit Mapper: {runtimeMappedReservation.GuestName}");
Console.WriteLine($"Runtime<String> Explicit Mapper: {runtimeStringMappedReservation.GuestName}");
Console.WriteLine($"Parity Check: {mappedReservation.GuestName == objectGenericMappedReservation.GuestName && mappedReservation.GuestName == runtimeMappedReservation.GuestName && mappedReservation.GuestName == runtimeStringMappedReservation.GuestName}");
