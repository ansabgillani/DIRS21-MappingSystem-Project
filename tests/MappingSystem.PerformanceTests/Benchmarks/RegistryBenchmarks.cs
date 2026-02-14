using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using MappingSystem.Core;
using MappingSystem.Implementation;

namespace MappingSystem.PerformanceTests.Benchmarks;

[MemoryDiagnoser]
public class RegistryBenchmarks
{
    private IMapperRegistry _registry = null!;
    private ConcurrentDictionary<(Type, Type), object> _dict = null!;

    [GlobalSetup]
    public void Setup()
    {
        _registry = new MapperRegistry();
        _registry.Register<string, int>(s => s.Length);

        _dict = new ConcurrentDictionary<(Type, Type), object>();
        _dict.TryAdd((typeof(string), typeof(int)), (Func<string, int>)(s => s.Length));
    }

    [Benchmark]
    public bool RegistryLookup()
    {
        return _registry.TryGetMapper<string, int>(out _);
    }

    [Benchmark(Baseline = true)]
    public bool DirectConcurrentDictionaryLookup()
    {
        return _dict.TryGetValue((typeof(string), typeof(int)), out _);
    }
}
