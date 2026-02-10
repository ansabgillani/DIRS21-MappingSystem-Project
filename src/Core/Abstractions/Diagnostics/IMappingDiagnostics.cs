namespace MappingSystem.Core;

public interface IMappingDiagnostics
{
    void RecordRegistration(Type sourceType, Type targetType);

    IEnumerable<MappingInfo> GetRegisteredMappings();

    MappingInfo? GetMappingInfo(Type sourceType, Type targetType);

    int GetCacheSize();
}
