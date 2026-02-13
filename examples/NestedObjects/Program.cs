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

var googleReservation = new GoogleReservationWithGuest
{
	ReservationCode = "GOOGLE-NESTED-1",
	CheckInDate = DateTime.UtcNow.AddDays(1),
	Guest = new GoogleGuestProfile { GoogleGuestId = "GG-100", FullName = "Nested" }
};

var mappedReservation = handler.Map<GoogleReservationWithGuest, ReservationWithGuest>(googleReservation);
Console.WriteLine($"Reservation: {mappedReservation.ReservationId}, Guest: {mappedReservation.Guest?.FullName}");

var googleReservationBatch = new List<GoogleReservationWithGuest>
{
	new() { ReservationCode = "GOOGLE-NESTED-2", CheckInDate = DateTime.UtcNow.AddDays(2), Guest = new GoogleGuestProfile { GoogleGuestId = "GG-101", FullName = "List-1" } },
	new() { ReservationCode = "GOOGLE-NESTED-3", CheckInDate = DateTime.UtcNow.AddDays(3), Guest = null }
};

var mappedReservations = handler.Map<List<GoogleReservationWithGuest>, List<ReservationWithGuest>>(googleReservationBatch);
Console.WriteLine($"List mapped count: {mappedReservations.Count}, first guest: {mappedReservations[0].Guest?.FullName}");
