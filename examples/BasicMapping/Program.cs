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

Console.WriteLine($"Generic Map: {mappedReservation.ReservationId} / {mappedReservation.GuestName}");

object objectPayload = googleReservation;
var objectGenericMapped = handler.Map<GoogleReservation, Reservation>(objectPayload);
Console.WriteLine($"Generic<Object> Map: {objectGenericMapped.ReservationId} / {objectGenericMapped.GuestName}");

var runtimeMapped = (Reservation)handler.Map(googleReservation, typeof(GoogleReservation), typeof(Reservation));
Console.WriteLine($"Runtime Map: {runtimeMapped.ReservationId} / {runtimeMapped.GuestName}");

var runtimeStringMapped = (Reservation)handler.Map(
	objectPayload,
	typeof(GoogleReservation).FullName!,
	typeof(Reservation).FullName!);
Console.WriteLine($"Runtime<String> Map: {runtimeStringMapped.ReservationId} / {runtimeStringMapped.GuestName}");

Console.WriteLine($"Parity Check (all overloads): {mappedReservation.ReservationId == objectGenericMapped.ReservationId && mappedReservation.ReservationId == runtimeMapped.ReservationId && mappedReservation.ReservationId == runtimeStringMapped.ReservationId}");

var sourceReservation = new Reservation
{
	ReservationId = "IDENTITY-1",
	GuestName = "Deep Copy Guest",
	CheckInDate = DateTime.UtcNow.AddDays(5),
	CheckOutDate = DateTime.UtcNow.AddDays(6),
	GuestCount = 2
};

var copiedReservation = handler.Map<Reservation, Reservation>(sourceReservation);
sourceReservation.GuestName = "Mutated After Map";

Console.WriteLine($"Identity Deep Copy: {ReferenceEquals(sourceReservation, copiedReservation) == false}");
Console.WriteLine($"Copied GuestName (unchanged): {copiedReservation.GuestName}");

try
{
	_ = handler.Map(new Reservation { ReservationId = "WRONG", GuestName = "Wrong" }, typeof(GoogleReservation), typeof(Reservation));
}
catch (ArgumentException ex)
{
	Console.WriteLine($"Guardrail: {ex.Message}");
}
