namespace MappingSystem.Abstractions;

public interface ITypeMapper<in TSource, out TTarget>
{
    TTarget Map(TSource source);
}
