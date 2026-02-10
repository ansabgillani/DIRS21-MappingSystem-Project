namespace MappingSystem.Core.Exceptions;

public class MappingNotFoundException : MappingException
{
    public MappingNotFoundException(Type sourceType, Type targetType, int conventionsEvaluated)
        : base(sourceType, targetType, BuildMessage(sourceType, targetType, conventionsEvaluated))
    {
    }

    public MappingNotFoundException(Type sourceType, Type targetType)
        : base(
            sourceType,
            targetType,
            $"No mapping found for {sourceType.Name} -> {targetType.Name}. " +
            $"Ensure target type has parameterless constructor or register explicit ITypeMapper<{sourceType.Name}, {targetType.Name}>.")
    {
    }

    private static string BuildMessage(Type sourceType, Type targetType, int conventionsEvaluated)
    {
        return $"No mapping found for {sourceType.Name} -> {targetType.Name}. "
            + $"Evaluated {conventionsEvaluated} convention(s). "
            + "Suggestions:\n"
            + $"  1. Ensure {targetType.Name} has a parameterless constructor\n"
            + $"  2. Register explicit ITypeMapper<{sourceType.Name}, {targetType.Name}>\n"
            + "  3. Check property names match (case-insensitive)\n"
            + "  4. Verify property types are compatible";
    }
}
