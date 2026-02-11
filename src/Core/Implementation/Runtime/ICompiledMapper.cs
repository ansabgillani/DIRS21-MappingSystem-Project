namespace MappingSystem.Implementation;

public interface ICompiledMapper
{
    Type SourceType { get; }

    Type TargetType { get; }

    object Map(object source);
}
