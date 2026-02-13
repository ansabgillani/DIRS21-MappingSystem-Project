using System.Diagnostics;
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
	ReservationCode = "GOOGLE-PERF-1",
	PrimaryGuestName = "Performance Guest",
	CheckInDate = DateTime.UtcNow.AddDays(10),
	CheckOutDate = DateTime.UtcNow.AddDays(12),
	NumberOfGuests = 2
};
var iterations = 200_000;

handler.Map<GoogleReservation, Reservation>(googleReservation);

var sw = Stopwatch.StartNew();
for (var i = 0; i < iterations; i++)
{
	_ = handler.Map<GoogleReservation, Reservation>(googleReservation);
}

sw.Stop();
Console.WriteLine($"Iterations: {iterations}");
Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"Throughput: {iterations / sw.Elapsed.TotalSeconds:N0} maps/sec");
