using System.Reflection;
using MappingSystem.Core;

namespace MappingSystem.Implementation.Utilities;

public static class MappingReflectionUtilities
{
    public static PropertyInfo CurrentMapHandlerProperty { get; } = typeof(MappingExecutionContext)
        .GetProperty(nameof(MappingExecutionContext.CurrentHandler))!;

    public static MethodInfo GenericMapMethodDefinition { get; } = typeof(IMapHandler)
        .GetMethods()
        .Single(method =>
            method.Name == nameof(IMapHandler.Map)
            && method.IsGenericMethodDefinition
            && method.GetParameters().Length == 1);

    public static MethodInfo EnumerableSelectMethodDefinition { get; } = typeof(Enumerable)
        .GetMethods()
        .Single(method =>
            method.Name == nameof(Enumerable.Select)
            && method.GetParameters().Length == 2
            && method.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2);

    public static MethodInfo EnumerableToListMethodDefinition { get; } = typeof(Enumerable)
        .GetMethods()
        .Single(method => method.Name == nameof(Enumerable.ToList) && method.IsGenericMethodDefinition);
}
