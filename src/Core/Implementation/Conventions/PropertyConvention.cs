using System.Linq.Expressions;
using MappingSystem.Core;

namespace MappingSystem.Implementation.Conventions;

public class PropertyConvention : IMappingConvention
{
    public bool CanMap(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);

        return targetType.GetConstructor(Type.EmptyTypes) != null;
    }

    public LambdaExpression BuildExpression(Type sourceType, Type targetType)
    {
        return ExpressionBuilder.BuildPropertyMappingExpression(sourceType, targetType);
    }
}
