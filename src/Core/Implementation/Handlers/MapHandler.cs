using MappingSystem.Core;
using MappingSystem.Implementation.Utilities;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MappingSystem.Implementation;

public class MapHandler : IMapHandler
{
    private readonly IMapperRegistry _mapperRegistry;
    private readonly IMapperFactory _mapperFactory;
    private readonly ILogger<MapHandler> _logger;
    private readonly IMappingDiagnostics? _diagnostics;

    public MapHandler(
        IMapperRegistry mapperRegistry,
        IMapperFactory mapperFactory,
        ILogger<MapHandler> logger,
        IMappingDiagnostics? diagnostics = null)
    {
        _mapperRegistry = mapperRegistry ?? throw new ArgumentNullException(nameof(mapperRegistry));
        _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _diagnostics = diagnostics;

        MappingExecutionContext.SetCurrentHandler(this);
    }

    public TTarget Map<TSource, TTarget>(TSource source)
    {
        if (source == null)
        {
            return default!;
        }

        return ExecuteWithContext(() =>
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            LogMappingAttempt(sourceType, targetType);

            if (!_mapperRegistry.TryGetMapper<TSource, TTarget>(out var mapper) || mapper == null)
            {
                LogCacheMiss(sourceType, targetType);

                mapper = _mapperFactory.CreateMapper<TSource, TTarget>();
                _mapperRegistry.Register(mapper);

                RecordRegistration(sourceType, targetType);
                LogRegisteredMapper(sourceType, targetType);
            }

            return mapper(source);
        });
    }

    public TTarget Map<TSource, TTarget>(object data)
    {
        if (data == null)
        {
            return default!;
        }

        if (data is not TSource typedSource)
        {
            throw new ArgumentException(
                $"Input object type {data.GetType().Name} is not assignable to expected source type {typeof(TSource).Name}.",
                nameof(data));
        }

        return Map<TSource, TTarget>(typedSource);
    }

    public object Map(object source, Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        if (source == null)
        {
            return default!;
        }

        if (!sourceType.IsInstanceOfType(source))
        {
            throw new ArgumentException(
                $"Source object type {source.GetType().Name} is not assignable to declared source type {sourceType.Name}.",
                nameof(source));
        }

        return ExecuteWithContext(() =>
        {
            LogMappingAttempt(sourceType, targetType);

            if (!_mapperRegistry.TryGetMapper(sourceType, targetType, out var mapper) || mapper == null)
            {
                LogCacheMiss(sourceType, targetType);

                mapper = _mapperFactory.CreateMapper(sourceType, targetType);
                _mapperRegistry.Register(sourceType, targetType, mapper);

                RecordRegistration(sourceType, targetType);
                LogRegisteredMapper(sourceType, targetType);
            }

            return MapperInvocationUtilities.ExecuteMapper(mapper, source);
        });
    }

    public object Map(object data, string sourceType, string targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        var resolvedSourceType = ResolveType(sourceType);
        var resolvedTargetType = ResolveType(targetType);

        return Map(data, resolvedSourceType, resolvedTargetType);
    }

    private static Type ResolveType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Type name cannot be null, empty, or whitespace.", nameof(typeName));
        }

        var resolved = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
        if (resolved != null)
        {
            return resolved;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var directMatch = assembly.GetType(typeName, throwOnError: false, ignoreCase: true);
            if (directMatch != null)
            {
                return directMatch;
            }

            var nameMatch = GetLoadableTypes(assembly)
                .FirstOrDefault(type =>
                    string.Equals(type.Name, typeName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(type.FullName, typeName, StringComparison.OrdinalIgnoreCase));

            if (nameMatch != null)
            {
                return nameMatch;
            }
        }

        throw new ArgumentException(
            $"Unable to resolve type '{typeName}'. Use an assembly-qualified name, full type name, or unique type name from loaded assemblies.",
            nameof(typeName));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
    }

    private T ExecuteWithContext<T>(Func<T> action)
    {
        MappingExecutionContext.TryGetCurrentHandler(out var previousHandler);
        MappingExecutionContext.SetCurrentHandler(this);

        try
        {
            return action();
        }
        finally
        {
            MappingExecutionContext.SetCurrentHandler(previousHandler);
        }
    }

    private void RecordRegistration(Type sourceType, Type targetType)
    {
        _diagnostics?.RecordRegistration(sourceType, targetType);
    }

    private void LogMappingAttempt(Type sourceType, Type targetType)
    {
        _logger.LogDebug("Mapping {SourceType} -> {TargetType}", sourceType.Name, targetType.Name);
    }

    private void LogCacheMiss(Type sourceType, Type targetType)
    {
        _logger.LogInformation(
            "Cache miss: Creating mapper {SourceType} -> {TargetType}",
            sourceType.Name,
            targetType.Name);
    }

    private void LogRegisteredMapper(Type sourceType, Type targetType)
    {
        _logger.LogInformation(
            "Registered mapper: {SourceType} -> {TargetType}",
            sourceType.Name,
            targetType.Name);
    }
}
