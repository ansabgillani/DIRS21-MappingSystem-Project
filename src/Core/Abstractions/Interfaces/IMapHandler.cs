namespace MappingSystem.Core;

public interface IMapHandler
{
    TTarget Map<TSource, TTarget>(TSource source);

    object Map(object source, Type sourceType, Type targetType);
}
