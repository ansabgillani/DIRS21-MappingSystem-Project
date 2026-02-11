using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using MappingSystem.Core.Exceptions;
using MappingSystem.Implementation.Utilities;

namespace MappingSystem.Implementation;

public static class ExpressionBuilder
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ReadablePropertiesCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> WritablePropertiesCache = new();

    public static LambdaExpression BuildPropertyMappingExpression(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        var sourceParam = Expression.Parameter(sourceType, "source");
        var targetVar = Expression.Variable(targetType, "target");

        var targetCtor = targetType.GetConstructor(Type.EmptyTypes);
        if (targetCtor == null)
        {
            throw new MappingCompilationException(
                sourceType,
                targetType,
                $"Target type {targetType.Name} lacks parameterless constructor",
                new MissingMethodException());
        }

        var newTarget = Expression.New(targetCtor);
        var assignTarget = Expression.Assign(targetVar, newTarget);

        var expressions = new List<Expression> { assignTarget };

        var sourceProperties = GetMappableProperties(sourceType, canRead: true);
        var targetProperties = GetMappableProperties(targetType, canWrite: true);

        foreach (var targetProp in targetProperties)
        {
            var sourceProp = FindMatchingProperty(sourceProperties, targetProp.Name);

            if (sourceProp == null)
            {
                continue;
            }

            if (sourceProp.PropertyType == targetProp.PropertyType)
            {
                var getSourceValue = Expression.Property(sourceParam, sourceProp);
                var setTargetValue = Expression.Property(targetVar, targetProp);
                var assignment = Expression.Assign(setTargetValue, getSourceValue);
                expressions.Add(assignment);
            }
            else if (IsComplexType(sourceProp.PropertyType) && IsComplexType(targetProp.PropertyType))
            {
                var getSourceValue = Expression.Property(sourceParam, sourceProp);
                var setTargetValue = Expression.Property(targetVar, targetProp);

                var mapHandlerInstance = Expression.Property(null, MappingReflectionUtilities.CurrentMapHandlerProperty);
                var mapMethod = MappingReflectionUtilities.GenericMapMethodDefinition.MakeGenericMethod(sourceProp.PropertyType, targetProp.PropertyType);

                var nullCheck = Expression.NotEqual(getSourceValue, Expression.Constant(null, sourceProp.PropertyType));
                var callMap = Expression.Call(mapHandlerInstance, mapMethod, getSourceValue);
                var nullConstant = Expression.Constant(null, targetProp.PropertyType);
                var conditionalMap = Expression.Condition(nullCheck, callMap, nullConstant);

                var assignment = Expression.Assign(setTargetValue, conditionalMap);
                expressions.Add(assignment);
            }
        }

        expressions.Add(targetVar);

        var block = Expression.Block(new[] { targetVar }, expressions);
        return Expression.Lambda(block, sourceParam);
    }

    private static PropertyInfo[] GetMappableProperties(Type type, bool canRead = false, bool canWrite = false)
    {
        if (canRead && !canWrite)
        {
            return ReadablePropertiesCache.GetOrAdd(type, static t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToArray());
        }

        if (canWrite && !canRead)
        {
            return WritablePropertiesCache.GetOrAdd(type, static t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToArray());
        }

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => (!canRead || p.CanRead) && (!canWrite || p.CanWrite))
            .ToArray();
    }

    internal static PropertyInfo[] GetMappablePropertiesForTesting(Type type, bool canRead = false, bool canWrite = false)
    {
        return GetMappableProperties(type, canRead, canWrite);
    }

    private static PropertyInfo? FindMatchingProperty(PropertyInfo[] properties, string name)
    {
        return properties.FirstOrDefault(
            p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsComplexType(Type type)
    {
        return !type.IsPrimitive
            && type != typeof(string)
            && type != typeof(decimal)
            && type != typeof(DateTime)
            && type != typeof(DateTimeOffset)
            && type != typeof(TimeSpan)
            && type != typeof(Guid);
    }
}
