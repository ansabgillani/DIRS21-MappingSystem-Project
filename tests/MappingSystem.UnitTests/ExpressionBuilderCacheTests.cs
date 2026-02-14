using FluentAssertions;
using MappingSystem.Implementation;

namespace MappingSystem.UnitTests;

public class ExpressionBuilderCacheTests
{
    [Fact]
    public void GetMappableProperties_CalledTwice_UsesCachedResults()
    {
        var type = typeof(SimpleSource);

        var props1 = ExpressionBuilder.GetMappablePropertiesForTesting(type, canRead: true);
        var props2 = ExpressionBuilder.GetMappablePropertiesForTesting(type, canRead: true);

        props1.Should().BeSameAs(props2);
    }

    private class SimpleSource
    {
        public int Value { get; set; }
    }
}
