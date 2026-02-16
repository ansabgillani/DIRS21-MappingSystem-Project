using FluentAssertions;
using MappingSystem.Core;
using MappingSystem.Core.Exceptions;
using MappingSystem.Implementation;
using MappingSystem.Implementation.Conventions;
using Microsoft.Extensions.Logging.Abstractions;

namespace MappingSystem.UnitTests;

public class MapHandlerTests
{
    private readonly MapHandler _handler;
    private readonly IMapperRegistry _registry;

    public MapHandlerTests()
    {
        _registry = new MapperRegistry();
        var conventions = new IMappingConvention[] { new IdentityConvention(), new PropertyConvention() };
        var factory = new MapperFactory(conventions, NullLogger<MapperFactory>.Instance);
        _handler = new MapHandler(_registry, factory, NullLogger<MapHandler>.Instance);
    }

    [Fact]
    public void Map_NullSource_ReturnsDefault()
    {
        var result = _handler.Map<string, int>(null!);

        result.Should().Be(default(int));
    }

    [Fact]
    public void Map_FirstCall_CompilesAndCaches()
    {
        var source = new SimpleSource { Value = 99 };

        var result = _handler.Map<SimpleSource, SimpleTarget>(source);

        result.Value.Should().Be(99);
        _registry.IsRegistered(typeof(SimpleSource), typeof(SimpleTarget)).Should().BeTrue();
    }

    [Fact]
    public void Map_FirstCall_RegistersCompiledMapperObject()
    {
        _handler.Map<SimpleSource, SimpleTarget>(new SimpleSource { Value = 5 });

        var found = _registry.TryGetMapper(typeof(SimpleSource), typeof(SimpleTarget), out var mapperObject);

        found.Should().BeTrue();
        mapperObject.Should().BeOfType<CompiledMapper<SimpleSource, SimpleTarget>>();
    }

    [Fact]
    public void Map_SecondCall_UsesCachedMapper()
    {
        var source1 = new SimpleSource { Value = 10 };
        var source2 = new SimpleSource { Value = 20 };

        _handler.Map<SimpleSource, SimpleTarget>(source1);
        var result2 = _handler.Map<SimpleSource, SimpleTarget>(source2);

        result2.Value.Should().Be(20);
    }

    [Fact]
    public async Task Map_ConcurrentFirstCalls_BothSucceed()
    {
        var source = new SimpleSource { Value = 100 };
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => _handler.Map<SimpleSource, SimpleTarget>(source)))
            .ToArray();

        await Task.WhenAll(tasks);

        tasks.Should().AllSatisfy(task => task.Result.Value.Should().Be(100));
    }

    [Fact]
    public void Map_UnmappableTypes_ThrowsMappingNotFoundException()
    {
        var act = () => _handler.Map<SimpleSource, NoConstructor>(new SimpleSource());

        act.Should().Throw<MappingNotFoundException>();
    }

    [Fact]
    public void Map_NonGeneric_WorksCorrectly()
    {
        var source = new SimpleSource { Value = 42 };

        var result = (SimpleTarget)_handler.Map(source, typeof(SimpleSource), typeof(SimpleTarget));

        result.Value.Should().Be(42);
    }

    [Fact]
    public void Map_NonGeneric_WithMismatchedSourceType_ThrowsArgumentException()
    {
        var source = new SimpleTarget { Value = 42 };

        var act = () => _handler.Map(source, typeof(SimpleSource), typeof(SimpleTarget));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Map_NonGeneric_WithNullSource_ReturnsDefault()
    {
        var result = _handler.Map(null!, typeof(SimpleSource), typeof(SimpleTarget));

        result.Should().BeNull();
    }

    [Fact]
    public void Map_GenericObjectOverload_WorksCorrectly()
    {
        object source = new SimpleSource { Value = 77 };

        var result = _handler.Map<SimpleSource, SimpleTarget>(source);

        result.Value.Should().Be(77);
    }

    [Fact]
    public void Map_GenericObjectOverload_WithMismatchedSourceType_ThrowsArgumentException()
    {
        object source = new SimpleTarget { Value = 10 };

        var act = () => _handler.Map<SimpleSource, SimpleTarget>(source);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Map_StringTypeOverload_WorksCorrectly()
    {
        var source = new SimpleSource { Value = 64 };
        var sourceType = typeof(SimpleSource).FullName!;
        var targetType = typeof(SimpleTarget).FullName!;

        var result = (SimpleTarget)_handler.Map(source, sourceType, targetType);

        result.Value.Should().Be(64);
    }

    [Fact]
    public void Map_StringTypeOverload_WithUnknownType_ThrowsArgumentException()
    {
        var source = new SimpleSource { Value = 64 };

        var act = () => _handler.Map(source, "Unknown.Source.Type", typeof(SimpleTarget).FullName!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Map_StringTypeOverload_WithMismatchedSourceType_ThrowsArgumentException()
    {
        var source = new SimpleTarget { Value = 10 };

        var act = () => _handler.Map(source, typeof(SimpleSource).FullName!, typeof(SimpleTarget).FullName!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Map_StringTypeOverload_WithNullSource_ReturnsDefault()
    {
        var result = _handler.Map(null!, typeof(SimpleSource).FullName!, typeof(SimpleTarget).FullName!);

        result.Should().BeNull();
    }

    [Fact]
    public void Map_SameType_ReturnsDeepCopyInstance()
    {
        var source = new SimpleSource { Value = 42 };

        var result = _handler.Map<SimpleSource, SimpleSource>(source);

        result.Should().NotBeSameAs(source);
        result.Value.Should().Be(42);

        source.Value = 99;
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Map_GenericAndRuntimeOverloads_ReturnEquivalentResults()
    {
        var source = new SimpleSource { Value = 55 };

        var genericResult = _handler.Map<SimpleSource, SimpleTarget>(source);
        var runtimeResult = (SimpleTarget)_handler.Map(source, typeof(SimpleSource), typeof(SimpleTarget));

        genericResult.Value.Should().Be(55);
        runtimeResult.Value.Should().Be(55);
    }

    [Fact]
    public void Map_AllFourOverloads_ReturnEquivalentResults()
    {
        var source = new SimpleSource { Value = 88 };
        object data = source;

        var genericTyped = _handler.Map<SimpleSource, SimpleTarget>(source);
        var genericObject = _handler.Map<SimpleSource, SimpleTarget>(data);
        var runtimeType = (SimpleTarget)_handler.Map(data, typeof(SimpleSource), typeof(SimpleTarget));
        var runtimeString = (SimpleTarget)_handler.Map(
            data,
            typeof(SimpleSource).FullName!,
            typeof(SimpleTarget).FullName!);

        genericTyped.Value.Should().Be(88);
        genericObject.Value.Should().Be(88);
        runtimeType.Value.Should().Be(88);
        runtimeString.Value.Should().Be(88);
    }

    private class SimpleSource
    {
        public int Value { get; set; }
    }

    private class SimpleTarget
    {
        public int Value { get; set; }
    }

    private class NoConstructor
    {
        public NoConstructor(int required)
        {
            _ = required;
        }
    }
}
