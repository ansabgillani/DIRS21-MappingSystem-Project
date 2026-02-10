namespace MappingSystem.Core.Exceptions;

public class MappingCompilationException : MappingException
{
    public MappingCompilationException(Type sourceType, Type targetType, string message, Exception innerException)
        : base(
            sourceType,
            targetType,
            $"Failed to compile mapper for {sourceType.Name} -> {targetType.Name}: {message}",
            innerException)
    {
    }
}
