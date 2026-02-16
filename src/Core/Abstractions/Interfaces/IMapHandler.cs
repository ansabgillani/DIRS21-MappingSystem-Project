namespace MappingSystem.Core;

public interface IMapHandler
{
    TTarget Map<TSource, TTarget>(TSource source);

    TTarget Map<TSource, TTarget>(object data);

    object Map(object source, Type sourceType, Type targetType);

    object Map(object data, string sourceType, string targetType);
}
