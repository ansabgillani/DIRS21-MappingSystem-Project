using FluentAssertions;
using MappingSystem.Core;
using MappingSystem.Extensions;
using MappingSystem.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.IntegrationTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMapping_RegistersAllComponents()
    {
        var services = new ServiceCollection();

        services.AddMapping();
        var provider = services.BuildServiceProvider();

        provider.GetService<IMapHandler>().Should().NotBeNull();
        provider.GetService<IMapperRegistry>().Should().NotBeNull();
        provider.GetService<IMapperFactory>().Should().NotBeNull();
        provider.GetService<IMappingDiagnostics>().Should().NotBeNull();
    }

    [Fact]
    public void AddMapping_MapHandlerIsResolvable()
    {
        var services = new ServiceCollection();
        services.AddMapping();
        var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IMapHandler>();

        handler.Should().NotBeNull();
        handler.Should().BeOfType<MapHandler>();
    }
}
