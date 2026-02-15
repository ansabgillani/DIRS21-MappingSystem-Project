using MappingSystem.Core;
using MappingSystem.Implementation.Utilities;
using Microsoft.Extensions.Logging;

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
