using FluentAssertions;
using MappingSystem.Abstractions;
using MappingSystem.Implementation.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.UnitTests;

public class TypeMapperConventionTests
{
    [Fact]
    public void CanMap_WithRegisteredMapper_ReturnsTrue()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITypeMapper<Source, Target>, ExplicitMapper>();
        var provider = services.BuildServiceProvider();
        var convention = new TypeMapperConvention(provider);

        var result = convention.CanMap(typeof(Source), typeof(Target));

        result.Should().BeTrue();
    }

    [Fact]
    public void CanMap_WithoutRegisteredMapper_ReturnsFalse()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var convention = new TypeMapperConvention(provider);

        var result = convention.CanMap(typeof(Source), typeof(Target));

        result.Should().BeFalse();
    }

    [Fact]
    public void BuildExpression_CallsMapperMapMethod()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITypeMapper<Source, Target>, ExplicitMapper>();
        var provider = services.BuildServiceProvider();
        var convention = new TypeMapperConvention(provider);

        var expression = convention.BuildExpression(typeof(Source), typeof(Target));
        var compiled = (Func<Source, Target>)expression.Compile();
        var result = compiled(new Source { Value = 42 });

        result.Value.Should().Be(42);
        result.ProcessedBy.Should().Be("ExplicitMapper");
    }

    private class Source
    {
        public int Value { get; set; }
    }

    private class Target
    {
        public int Value { get; set; }

        public string ProcessedBy { get; set; } = string.Empty;
    }

    private class ExplicitMapper : ITypeMapper<Source, Target>
    {
        public Target Map(Source source)
        {
            return new Target
            {
                Value = source.Value,
                ProcessedBy = "ExplicitMapper"
            };
        }
    }
}
