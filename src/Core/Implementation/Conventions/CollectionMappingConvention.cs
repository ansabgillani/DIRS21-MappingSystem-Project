using System.Linq.Expressions;
using MappingSystem.Core;
using MappingSystem.Implementation.Utilities;

namespace MappingSystem.Implementation.Conventions;

public class CollectionMappingConvention : IMappingConvention
{
    public bool CanMap(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        return IsGenericList(sourceType) && IsGenericList(targetType);
    }

    public LambdaExpression BuildExpression(Type sourceType, Type targetType)
    {
        var sourceElementType = sourceType.GetGenericArguments()[0];
        var targetElementType = targetType.GetGenericArguments()[0];

        var sourceParam = Expression.Parameter(sourceType, "source");

        var mapHandlerInstance = Expression.Property(null, MappingReflectionUtilities.CurrentMapHandlerProperty);

        var mapMethod = MappingReflectionUtilities.GenericMapMethodDefinition.MakeGenericMethod(sourceElementType, targetElementType);

        var selectMethod = MappingReflectionUtilities.EnumerableSelectMethodDefinition.MakeGenericMethod(sourceElementType, targetElementType);

        var itemParam = Expression.Parameter(sourceElementType, "item");
        var mapCall = Expression.Call(mapHandlerInstance, mapMethod, itemParam);
        var lambda = Expression.Lambda(mapCall, itemParam);

        var selectCall = Expression.Call(selectMethod, sourceParam, lambda);

        var toListMethod = MappingReflectionUtilities.EnumerableToListMethodDefinition.MakeGenericMethod(targetElementType);

        var toListCall = Expression.Call(toListMethod, selectCall);
        var nullSource = Expression.Constant(null, sourceType);
        var nullTarget = Expression.Constant(null, targetType);
        var condition = Expression.Condition(
            Expression.NotEqual(sourceParam, nullSource),
            toListCall,
            nullTarget);

        return Expression.Lambda(condition, sourceParam);
    }

    private static bool IsGenericList(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }
}
