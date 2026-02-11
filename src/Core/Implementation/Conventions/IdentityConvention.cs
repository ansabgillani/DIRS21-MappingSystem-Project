using System.Linq.Expressions;
using MappingSystem.Core;

namespace MappingSystem.Implementation.Conventions;

public class IdentityConvention : IMappingConvention
{
    public bool CanMap(Type sourceType, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(targetType);
        return sourceType == targetType;
    }

    public LambdaExpression BuildExpression(Type sourceType, Type targetType)
    {
        var param = Expression.Parameter(sourceType, "source");
        return Expression.Lambda(param, param);
    }
}
