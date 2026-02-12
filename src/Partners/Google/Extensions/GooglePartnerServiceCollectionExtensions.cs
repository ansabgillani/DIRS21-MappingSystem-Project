using MappingSystem.Abstractions;
using MappingSystems.Models.Reservations;
using MappingSystems.Partners.Google.Implementation;
using MappingSystems.Partners.Google.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystems.Partners.Google.Extensions;

public static class GooglePartnerServiceCollectionExtensions
{
    public static IServiceCollection AddGooglePartnerMappings(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ITypeMapper<GoogleReservation, Reservation>, GoogleReservationToReservationMapper>();
        services.AddScoped<ITypeMapper<GoogleReservationWithGuest, ReservationWithGuest>, GoogleReservationWithGuestToReservationWithGuestMapper>();

        return services;
    }
}
