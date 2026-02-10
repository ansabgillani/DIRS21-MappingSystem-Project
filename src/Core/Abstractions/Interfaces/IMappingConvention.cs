using System.Linq.Expressions;

namespace MappingSystem.Core;

public interface IMappingConvention
{
    bool CanMap(Type sourceType, Type targetType);

    LambdaExpression BuildExpression(Type sourceType, Type targetType);
}
