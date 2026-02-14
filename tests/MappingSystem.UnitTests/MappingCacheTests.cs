using FluentAssertions;
using MappingSystem.Implementation;

namespace MappingSystem.UnitTests;

public class MappingCacheTests
{
    [Fact]
    public void TryGet_EmptyCache_ReturnsFalse()
    {
        var cache = new MappingCache();

        var found = cache.TryGet(typeof(string), typeof(int), out var compiledDelegate);

        found.Should().BeFalse();
        compiledDelegate.Should().BeNull();
    }

    [Fact]
    public void Store_ThenTryGet_ReturnsDelegate()
    {
        var cache = new MappingCache();
        Func<string, int> compiledDelegate = s => s.Length;

        cache.Store(typeof(string), typeof(int), compiledDelegate);
        var found = cache.TryGet(typeof(string), typeof(int), out var actualDelegate);

        found.Should().BeTrue();
        actualDelegate.Should().BeSameAs(compiledDelegate);
    }

    [Fact]
    public void Store_SameTypePairTwice_FirstWins()
    {
        var cache = new MappingCache();
        Func<string, int> first = _ => 1;
        Func<string, int> second = _ => 2;

        cache.Store(typeof(string), typeof(int), first);
        cache.Store(typeof(string), typeof(int), second);
        cache.TryGet(typeof(string), typeof(int), out var actual);

        actual.Should().BeSameAs(first);
        ((Func<string, int>)actual!)("x").Should().Be(1);
    }
}
