namespace MappingSystem.Core;

public interface IMapperRegistry
{
    bool TryGetMapper(Type sourceType, Type targetType, out object? mapper);

    bool TryGetMapper<TSource, TTarget>(out Func<TSource, TTarget>? mapper);

    void Register(Type sourceType, Type targetType, object mapper);

    void Register<TSource, TTarget>(Func<TSource, TTarget> mapper);

    bool IsRegistered(Type sourceType, Type targetType);
}
