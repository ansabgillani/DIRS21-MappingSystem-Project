using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace MappingSystem.Implementation.Utilities;

public static class MapperInvocationUtilities
{
    private static readonly ConcurrentDictionary<(Type MapperType, Type SourceType), MethodInfo?> MapperMethodCache = new();

    public static object ExecuteMapper(object mapper, object source)
    {
        if (mapper is ICompiledMapper compiledMapper)
        {
            return compiledMapper.Map(source);
        }

        if (mapper is Delegate delegateMapper)
        {
            return InvokeDelegate(delegateMapper, source);
        }

        var mapperType = mapper.GetType();
        var sourceType = source.GetType();
        var mapMethod = MapperMethodCache.GetOrAdd((mapperType, sourceType), static key =>
            key.MapperType.GetMethod("Map", new[] { key.SourceType }));

        if (mapMethod == null)
        {
            throw new InvalidOperationException($"Mapper type {mapper.GetType().Name} does not expose a compatible Map method.");
        }

        try
        {
            return mapMethod.Invoke(mapper, new[] { source })!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static object InvokeDelegate(Delegate delegateMapper, object source)
    {
        try
        {
            return delegateMapper.DynamicInvoke(source)!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
