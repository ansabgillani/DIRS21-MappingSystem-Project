using FluentAssertions;
using MappingSystem.Abstractions;
using MappingSystem.Core;
using MappingSystem.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.IntegrationTests;

public class ExplicitMapperPriorityTests
{
    [Fact]
    public void MapHandler_WithExplicitMapper_UsesExplicitNotConvention()
    {
        var services = new ServiceCollection();
        services.AddMapping();
        services.AddScoped<ITypeMapper<Source, Target>, ExplicitMapper>();
        var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IMapHandler>();

        var source = new Source { Value = 100 };

        var result = handler.Map<Source, Target>(source);

        result.Value.Should().Be(100);
        result.MappedBy.Should().Be("Explicit");
    }

    [Fact]
    public void MapHandler_WithoutExplicitMapper_FallsBackToConvention()
    {
        var services = new ServiceCollection();
        services.AddMapping();
        var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IMapHandler>();

        var source = new Source { Value = 200 };

        var result = handler.Map<Source, Target>(source);

        result.Value.Should().Be(200);
        result.MappedBy.Should().BeNull();
    }

    private class Source
    {
        public int Value { get; set; }
    }

    private class Target
    {
        public int Value { get; set; }

        public string? MappedBy { get; set; }
    }

    private class ExplicitMapper : ITypeMapper<Source, Target>
    {
        public Target Map(Source source)
        {
            return new Target
            {
                Value = source.Value,
                MappedBy = "Explicit"
            };
        }
    }
}
