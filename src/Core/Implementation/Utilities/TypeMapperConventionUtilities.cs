using System.Collections.Concurrent;
using System.Reflection;
using MappingSystem.Abstractions;

namespace MappingSystem.Implementation.Utilities;

public static class TypeMapperConventionUtilities
{
    private static readonly ConcurrentDictionary<(Type SourceType, Type TargetType), Type> MapperInterfaceTypeCache = new();

    public static MethodInfo InvokeScopedMapMethodDefinition { get; } = typeof(Conventions.TypeMapperConvention)
        .GetMethod("InvokeScopedMap", BindingFlags.NonPublic | BindingFlags.Static)!;

    public static Type GetMapperInterfaceType(Type sourceType, Type targetType)
    {
        return MapperInterfaceTypeCache.GetOrAdd(
            (sourceType, targetType),
            static pair => typeof(ITypeMapper<,>).MakeGenericType(pair.SourceType, pair.TargetType));
    }
}
