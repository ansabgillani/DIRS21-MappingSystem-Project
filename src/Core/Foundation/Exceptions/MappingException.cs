namespace MappingSystem.Core.Exceptions;

public class MappingException : Exception
{
    public Type SourceType { get; }

    public Type TargetType { get; }

    public MappingException(Type sourceType, Type targetType, string message)
        : base(message)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    public MappingException(Type sourceType, Type targetType, string message, Exception innerException)
        : base(message, innerException)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }
}
