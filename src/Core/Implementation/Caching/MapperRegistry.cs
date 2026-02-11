using System.Collections.Concurrent;
using MappingSystem.Core;

namespace MappingSystem.Implementation;

public class MapperRegistry : IMapperRegistry
{
    private readonly ConcurrentDictionary<(Type Source, Type Target), object> _mappers = new();

    public bool TryGetMapper(Type sourceType, Type targetType, out object? mapper)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        return _mappers.TryGetValue((sourceType, targetType), out mapper);
    }

    public bool TryGetMapper<TSource, TTarget>(out Func<TSource, TTarget>? mapper)
    {
        if (TryGetMapper(typeof(TSource), typeof(TTarget), out var obj) && obj != null)
        {
            if (obj is Func<TSource, TTarget> compiledDelegate)
            {
                mapper = compiledDelegate;
                return true;
            }

            if (obj is CompiledMapper<TSource, TTarget> compiledMapper)
            {
                mapper = compiledMapper.Map;
                return true;
            }
        }

        mapper = null;
        return false;
    }

    public void Register(Type sourceType, Type targetType, object mapper)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(mapper);

        _mappers.TryAdd((sourceType, targetType), mapper);
    }

    public void Register<TSource, TTarget>(Func<TSource, TTarget> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        Register(typeof(TSource), typeof(TTarget), new CompiledMapper<TSource, TTarget>(mapper));
    }

    public bool IsRegistered(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        return _mappers.ContainsKey((sourceType, targetType));
    }
}
