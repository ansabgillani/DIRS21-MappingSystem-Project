using FluentAssertions;
using MappingSystem.Core;
using MappingSystem.Implementation;
using MappingSystem.Implementation.Conventions;
using Microsoft.Extensions.Logging.Abstractions;

namespace MappingSystem.UnitTests;

public class CollectionMappingTests
{
    private readonly IMapHandler _handler;

    public CollectionMappingTests()
    {
        var registry = new MapperRegistry();
        var conventions = new IMappingConvention[]
        {
            new IdentityConvention(),
            new CollectionMappingConvention(),
            new PropertyConvention()
        };

        var factory = new MapperFactory(conventions, NullLogger<MapperFactory>.Instance);
        _handler = new MapHandler(registry, factory, NullLogger<MapHandler>.Instance);
    }

    [Fact]
    public void Map_ListOfObjects_MapsElements()
    {
        var source = new List<SourceItem>
        {
            new() { Value = 1 },
            new() { Value = 2 }
        };

        var result = _handler.Map<List<SourceItem>, List<TargetItem>>(source);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Value.Should().Be(1);
        result[1].Value.Should().Be(2);
    }

    [Fact]
    public void Map_NullList_ReturnsNull()
    {
        List<SourceItem>? source = null;

        var result = _handler.Map<List<SourceItem>?, List<TargetItem>?>(source);

        result.Should().BeNull();
    }

    private class SourceItem
    {
        public int Value { get; set; }
    }

    private class TargetItem
    {
        public int Value { get; set; }
    }
}
