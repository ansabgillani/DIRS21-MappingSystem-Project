using FluentAssertions;
using MappingSystem.Implementation;

namespace MappingSystem.UnitTests;

public class MapperRegistryTests
{
    private readonly MapperRegistry _registry = new();

    [Fact]
    public void TryGetMapper_EmptyRegistry_ReturnsFalse()
    {
        var result = _registry.TryGetMapper<string, int>(out var mapper);

        result.Should().BeFalse();
        mapper.Should().BeNull();
    }

    [Fact]
    public void Register_ThenTryGet_ReturnsMapper()
    {
        Func<string, int> expectedMapper = s => s.Length;

        _registry.Register(expectedMapper);
        var result = _registry.TryGetMapper<string, int>(out var actualMapper);

        result.Should().BeTrue();
        actualMapper.Should().NotBeNull();
        actualMapper!("abcd").Should().Be(expectedMapper("abcd"));
    }

    [Fact]
    public void Register_SameTypePairTwice_FirstWins()
    {
        Func<string, int> firstMapper = _ => 1;
        Func<string, int> secondMapper = _ => 2;

        _registry.Register(firstMapper);
        _registry.Register(secondMapper);
        _registry.TryGetMapper<string, int>(out var actualMapper);

        actualMapper.Should().NotBeNull();
        actualMapper!("test").Should().Be(1);
    }

    [Fact]
    public void Register_NullMapper_ThrowsArgumentNullException()
    {
        var act = () => _registry.Register<string, int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsRegistered_AfterRegister_ReturnsTrue()
    {
        _registry.Register<string, int>(s => s.Length);

        var result = _registry.IsRegistered(typeof(string), typeof(int));

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRegistered_NotRegistered_ReturnsFalse()
    {
        var result = _registry.IsRegistered(typeof(string), typeof(int));

        result.Should().BeFalse();
    }

    [Fact]
    public void RegisterRuntime_ThenTryGetRuntime_ReturnsMapperObject()
    {
        var mapper = new CompiledMapper<string, int>(s => s.Length);

        _registry.Register(typeof(string), typeof(int), mapper);
        var result = _registry.TryGetMapper(typeof(string), typeof(int), out var actualMapper);

        result.Should().BeTrue();
        actualMapper.Should().BeSameAs(mapper);
    }

    [Fact]
    public void TryGetMapperGeneric_WithRuntimeCompiledMapper_ReturnsMappedDelegate()
    {
        _registry.Register(typeof(string), typeof(int), new CompiledMapper<string, int>(s => s.Length));

        var result = _registry.TryGetMapper<string, int>(out var mapper);

        result.Should().BeTrue();
        mapper.Should().NotBeNull();
        mapper!("abcd").Should().Be(4);
    }
}
