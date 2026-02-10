namespace MappingSystem.Core;

public interface IMapperFactory
{
    object CreateMapper(Type sourceType, Type targetType);

    Func<TSource, TTarget> CreateMapper<TSource, TTarget>();
}
