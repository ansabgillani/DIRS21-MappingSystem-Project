using System.Collections.Concurrent;

namespace MappingSystem.Implementation;

public class MappingCache
{
    private readonly ConcurrentDictionary<(Type Source, Type Target), Delegate> _cache = new();

    public bool TryGet(Type sourceType, Type targetType, out Delegate? compiledDelegate)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        return _cache.TryGetValue((sourceType, targetType), out compiledDelegate);
    }

    public void Store(Type sourceType, Type targetType, Delegate compiledDelegate)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(compiledDelegate);

        _cache.TryAdd((sourceType, targetType), compiledDelegate);
    }
}
