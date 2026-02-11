using System.Collections.Concurrent;
using MappingSystem.Core;

namespace MappingSystem.Implementation;

public class MappingDiagnostics : IMappingDiagnostics
{
    private readonly ConcurrentDictionary<(Type, Type), DateTime> _registrationTimes = new();

    public void RecordRegistration(Type sourceType, Type targetType)
    {
        _registrationTimes.TryAdd((sourceType, targetType), DateTime.UtcNow);
    }

    public IEnumerable<MappingInfo> GetRegisteredMappings()
    {
        return _registrationTimes.Select(kvp => new MappingInfo
        {
            SourceTypeName = kvp.Key.Item1.Name,
            TargetTypeName = kvp.Key.Item2.Name,
            RegisteredAt = kvp.Value
        });
    }

    public MappingInfo? GetMappingInfo(Type sourceType, Type targetType)
    {
        if (_registrationTimes.TryGetValue((sourceType, targetType), out var registeredAt))
        {
            return new MappingInfo
            {
                SourceTypeName = sourceType.Name,
                TargetTypeName = targetType.Name,
                RegisteredAt = registeredAt
            };
        }

        return null;
    }

    public int GetCacheSize()
    {
        return _registrationTimes.Count;
    }
}
