using MappingSystem.Core;
using MappingSystem.Implementation;
using MappingSystem.Implementation.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLogging();

        services.AddSingleton<MappingCache>();
        services.AddSingleton<IMapperRegistry, MapperRegistry>();
        services.AddSingleton<IMapperFactory, MapperFactory>();
        services.AddSingleton<IMappingDiagnostics, MappingDiagnostics>();
        services.AddSingleton<IMapHandler, MapHandler>();

        services.AddSingleton<IMappingConvention, IdentityConvention>();
        services.AddSingleton<IMappingConvention, TypeMapperConvention>();
        services.AddSingleton<IMappingConvention, CollectionMappingConvention>();
        services.AddSingleton<IMappingConvention, PropertyConvention>();

        return services;
    }
}
