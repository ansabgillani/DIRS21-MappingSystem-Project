using FluentAssertions;
using MappingSystem.Implementation.Conventions;

namespace MappingSystem.UnitTests;

public class PropertyConventionTests
{
    private readonly PropertyConvention _convention = new();

    [Fact]
    public void CanMap_WithDefaultConstructor_ReturnsTrue()
    {
        var sourceType = typeof(string);
        var targetType = typeof(List<int>);

        var result = _convention.CanMap(sourceType, targetType);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanMap_WithoutDefaultConstructor_ReturnsFalse()
    {
        var sourceType = typeof(string);
        var targetType = typeof(NoConstructor);

        var result = _convention.CanMap(sourceType, targetType);

        result.Should().BeFalse();
    }

    [Fact]
    public void BuildExpression_ReturnsCompiledExpression()
    {
        var sourceType = typeof(SimpleSource);
        var targetType = typeof(SimpleTarget);

        var expression = _convention.BuildExpression(sourceType, targetType);

        expression.Should().NotBeNull();
        expression.ReturnType.Should().Be(targetType);
    }

    private class NoConstructor
    {
        public NoConstructor(int required)
        {
            _ = required;
        }
    }

    private class SimpleSource
    {
        public int Value { get; set; }
    }

    private class SimpleTarget
    {
        public int Value { get; set; }
    }
}
