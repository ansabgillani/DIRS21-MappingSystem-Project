using BenchmarkDotNet.Attributes;
using MappingSystem.Core;
using MappingSystem.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.PerformanceTests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class MappingBenchmarks
{
    private IMapHandler _handler = null!;
    private SimpleSource _source = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMapping();
        var provider = services.BuildServiceProvider();
        _handler = provider.GetRequiredService<IMapHandler>();

        _source = new SimpleSource
        {
            Id = 123,
            Name = "Test",
            Value = 99.99m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _handler.Map<SimpleSource, SimpleTarget>(_source);
    }

    [Benchmark(Baseline = true)]
    public SimpleTarget ManualMapping()
    {
        return new SimpleTarget
        {
            Id = _source.Id,
            Name = _source.Name,
            Value = _source.Value,
            CreatedAt = _source.CreatedAt,
            IsActive = _source.IsActive
        };
    }

    [Benchmark]
    public SimpleTarget CachedDynamicMapping()
    {
        return _handler.Map<SimpleSource, SimpleTarget>(_source);
    }

    public class SimpleSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class SimpleTarget
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
