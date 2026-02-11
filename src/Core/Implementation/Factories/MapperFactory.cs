using System.Linq.Expressions;
using MappingSystem.Core;
using MappingSystem.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace MappingSystem.Implementation;

public class MapperFactory : IMapperFactory
{
    private readonly IMappingConvention[] _conventions;
    private readonly ILogger<MapperFactory> _logger;
    private readonly MappingCache _mappingCache;

    public MapperFactory(IEnumerable<IMappingConvention> conventions, ILogger<MapperFactory> logger)
        : this(conventions, logger, new MappingCache())
    {
    }

    public MapperFactory(
        IEnumerable<IMappingConvention> conventions,
        ILogger<MapperFactory> logger,
        MappingCache mappingCache)
    {
        ArgumentNullException.ThrowIfNull(conventions);

        _conventions = conventions as IMappingConvention[] ?? conventions.ToArray();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mappingCache = mappingCache ?? throw new ArgumentNullException(nameof(mappingCache));
    }

    public object CreateMapper(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        var conventionCount = 0;

        if (_mappingCache.TryGet(sourceType, targetType, out var cachedDelegate) && cachedDelegate != null)
        {
            _logger.LogDebug(
                "Using cached compiled delegate for {SourceType} -> {TargetType}",
                sourceType.Name,
                targetType.Name);

            return CreateCompiledMapper(sourceType, targetType, cachedDelegate);
        }

        foreach (var convention in _conventions)
        {
            conventionCount++;
            _logger.LogDebug(
                "Evaluating convention {ConventionType} for {SourceType} -> {TargetType}",
                convention.GetType().Name,
                sourceType.Name,
                targetType.Name);

            if (convention.CanMap(sourceType, targetType))
            {
                _logger.LogInformation(
                    "Using convention {ConventionType} for {SourceType} -> {TargetType}",
                    convention.GetType().Name,
                    sourceType.Name,
                    targetType.Name);

                try
                {
                    var expression = convention.BuildExpression(sourceType, targetType);
                    ValidateExpressionReturnType(sourceType, targetType, expression);
                    var compiledDelegate = expression.Compile();
                    _mappingCache.Store(sourceType, targetType, compiledDelegate);

                    return CreateCompiledMapper(sourceType, targetType, compiledDelegate);
                }
                catch (Exception ex) when (ex is not MappingCompilationException)
                {
                    _logger.LogError(
                        ex,
                        "Convention {ConventionType} failed to build expression for {SourceType} -> {TargetType}",
                        convention.GetType().Name,
                        sourceType.Name,
                        targetType.Name);

                    throw new MappingCompilationException(
                        sourceType,
                        targetType,
                        $"Convention {convention.GetType().Name} failed to build expression",
                        ex);
                }
            }
        }

        _logger.LogWarning("No convention found for {SourceType} -> {TargetType}", sourceType.Name, targetType.Name);

        throw new MappingNotFoundException(sourceType, targetType, conventionCount);
    }

    public Func<TSource, TTarget> CreateMapper<TSource, TTarget>()
    {
        var mapperObject = CreateMapper(typeof(TSource), typeof(TTarget));

        if (mapperObject is CompiledMapper<TSource, TTarget> compiledMapper)
        {
            return compiledMapper.Map;
        }

        if (mapperObject is Func<TSource, TTarget> mapperDelegate)
        {
            return mapperDelegate;
        }

        throw new MappingCompilationException(
            typeof(TSource),
            typeof(TTarget),
            "Factory returned an incompatible mapper instance",
            new InvalidCastException());
    }

    private static object CreateCompiledMapper(Type sourceType, Type targetType, Delegate compiledDelegate)
    {
        var compiledMapperType = typeof(CompiledMapper<,>).MakeGenericType(sourceType, targetType);
        return Activator.CreateInstance(compiledMapperType, compiledDelegate)!;
    }

    private static void ValidateExpressionReturnType(Type sourceType, Type targetType, LambdaExpression expression)
    {
        if (!targetType.IsAssignableFrom(expression.ReturnType))
        {
            throw new MappingCompilationException(
                sourceType,
                targetType,
                $"Convention produced invalid return type {expression.ReturnType.Name} for target {targetType.Name}",
                new InvalidCastException());
        }
    }
}
