using System.Linq.Expressions;
using MappingSystem.Abstractions;
using MappingSystem.Core;
using MappingSystem.Implementation.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.Implementation.Conventions;

public class TypeMapperConvention : IMappingConvention
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TypeMapperConvention(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public bool CanMap(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        var mapperType = TypeMapperConventionUtilities.GetMapperInterfaceType(sourceType, targetType);
        using var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetService(mapperType) != null;
    }

    public LambdaExpression BuildExpression(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        var sourceParam = Expression.Parameter(sourceType, "source");
        var invokeMethod = TypeMapperConventionUtilities.InvokeScopedMapMethodDefinition.MakeGenericMethod(sourceType, targetType);
        var scopeFactoryConstant = Expression.Constant(_scopeFactory);
        var callMap = Expression.Call(invokeMethod, scopeFactoryConstant, sourceParam);

        return Expression.Lambda(callMap, sourceParam);
    }

    private static TTarget InvokeScopedMap<TSource, TTarget>(IServiceScopeFactory scopeFactory, TSource source)
    {
        using var scope = scopeFactory.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<ITypeMapper<TSource, TTarget>>();
        return mapper.Map(source);
    }
}
