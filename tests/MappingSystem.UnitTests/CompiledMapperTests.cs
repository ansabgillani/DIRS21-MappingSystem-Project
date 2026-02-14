using FluentAssertions;
using MappingSystem.Implementation;

namespace MappingSystem.UnitTests;

public class CompiledMapperTests
{
    [Fact]
    public void Map_Generic_UsesCompiledDelegate()
    {
        var compiledMapper = new CompiledMapper<string, int>(s => s.Length);

        var result = compiledMapper.Map("hello");

        result.Should().Be(5);
    }

    [Fact]
    public void Map_Object_UsesCompiledDelegate()
    {
        var compiledMapper = new CompiledMapper<string, int>(s => s.Length);

        var result = compiledMapper.Map((object)"world");

        result.Should().Be(5);
    }
}
