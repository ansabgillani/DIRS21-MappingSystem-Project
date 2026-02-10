namespace MappingSystem.Core;

public class MappingInfo
{
    public required string SourceTypeName { get; init; }

    public required string TargetTypeName { get; init; }

    public required DateTime RegisteredAt { get; init; }
}
