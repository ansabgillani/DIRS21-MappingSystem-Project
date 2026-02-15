using System.Collections.Concurrent;
using System.Linq.Expressions;
using MappingSystem.Core;

namespace MappingSystem.Implementation.Conventions;

public class IdentityConvention : IMappingConvention
{
    private static readonly ConcurrentDictionary<Type, bool> HasDefaultConstructorCache = new();

    public bool CanMap(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        if (sourceType != targetType)
        {
            return false;
        }

        if (IsGenericList(sourceType))
        {
            return false;
        }

        if (CanUseIdentityPassThrough(sourceType))
        {
            return true;
        }

        return HasDefaultConstructor(sourceType);
    }

    public LambdaExpression BuildExpression(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        var param = Expression.Parameter(sourceType, "source");

        if (CanUseIdentityPassThrough(sourceType))
        {
            return Expression.Lambda(param, param);
        }

        return ExpressionBuilder.BuildPropertyMappingExpression(sourceType, targetType);
    }

    private static bool CanUseIdentityPassThrough(Type type)
    {
        return type.IsValueType || type == typeof(string);
    }

    private static bool IsGenericList(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    private static bool HasDefaultConstructor(Type type)
    {
        return HasDefaultConstructorCache.GetOrAdd(type, static t => t.GetConstructor(Type.EmptyTypes) != null);
    }
}
