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

        return (TTarget)Map(source!, typeof(TSource), typeof(TTarget));
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

        MappingExecutionContext.TryGetCurrentHandler(out var previousHandler);
        MappingExecutionContext.SetCurrentHandler(this);

        try
        {
            _logger.LogDebug("Mapping {SourceType} -> {TargetType}", sourceType.Name, targetType.Name);

            if (!_mapperRegistry.TryGetMapper(sourceType, targetType, out var mapper) || mapper == null)
            {
                _logger.LogInformation(
                    "Cache miss: Creating mapper {SourceType} -> {TargetType}",
                    sourceType.Name,
                    targetType.Name);

                mapper = _mapperFactory.CreateMapper(sourceType, targetType);
                _mapperRegistry.Register(sourceType, targetType, mapper);

                if (_diagnostics != null)
                {
                    _diagnostics.RecordRegistration(sourceType, targetType);
                }

                _logger.LogInformation(
                    "Registered mapper: {SourceType} -> {TargetType}",
                    sourceType.Name,
                    targetType.Name);
            }

            return MapperInvocationUtilities.ExecuteMapper(mapper, source);
        }
        finally
        {
            MappingExecutionContext.SetCurrentHandler(previousHandler);
        }
    }
}
