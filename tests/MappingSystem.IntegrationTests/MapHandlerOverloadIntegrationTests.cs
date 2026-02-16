using FluentAssertions;
using MappingSystem.Abstractions;
using MappingSystem.Core;
using MappingSystem.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.IntegrationTests;

public class MapHandlerOverloadIntegrationTests
{
    [Fact]
    public void AllOverloads_WithExplicitMapper_ReturnEquivalentOutput()
    {
        var services = new ServiceCollection();
        services.AddMapping();
        services.AddScoped<ITypeMapper<Source, Target>, ExplicitMapper>();

        var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IMapHandler>();

        var source = new Source { Value = 321 };
        object data = source;

        var genericTyped = handler.Map<Source, Target>(source);
        var genericObject = handler.Map<Source, Target>(data);
        var runtimeType = (Target)handler.Map(data, typeof(Source), typeof(Target));
        var runtimeString = (Target)handler.Map(data, typeof(Source).FullName!, typeof(Target).FullName!);

        genericTyped.Value.Should().Be(321);
        genericTyped.MappedBy.Should().Be("Explicit");

        genericObject.Value.Should().Be(321);
        genericObject.MappedBy.Should().Be("Explicit");

        runtimeType.Value.Should().Be(321);
        runtimeType.MappedBy.Should().Be("Explicit");

        runtimeString.Value.Should().Be(321);
        runtimeString.MappedBy.Should().Be("Explicit");
    }

    [Fact]
    public void StringOverload_WithUnknownTypeName_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        services.AddMapping();
        var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IMapHandler>();

        var source = new Source { Value = 10 };

        var act = () => handler.Map(source, "Unknown.Source.Type", typeof(Target).FullName!);

        act.Should().Throw<ArgumentException>();
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
