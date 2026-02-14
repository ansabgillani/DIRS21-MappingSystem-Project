using System.Diagnostics;
using MappingSystem.Core;
using MappingSystem.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MappingSystem.PerformanceTests.LoadTests;

public static class ConcurrencyLoadRunner
{
    public static async Task RunAsync()
    {
        var services = new ServiceCollection();
        services.AddMapping();
        var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IMapHandler>();

        var source = new TestSource { Value = 42 };
        var callCount = 10_000;

        var sw = Stopwatch.StartNew();
        var tasks = Enumerable.Range(0, callCount)
            .Select(_ => Task.Run(() => handler.Map<TestSource, TestTarget>(source)))
            .ToArray();

        await Task.WhenAll(tasks);
        sw.Stop();

        var throughput = callCount / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"Completed {callCount} mappings in {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Throughput: {throughput:N0} mappings/sec");

        var initialMemory = GC.GetTotalMemory(forceFullCollection: true);
        var pressureTasks = new List<Task>();
        for (var i = 0; i < 100; i++)
        {
            var typedSource = new TestSource { Value = i };
            pressureTasks.AddRange(Enumerable.Range(0, 10_000)
                .Select(_ => Task.Run(() => handler.Map<TestSource, TestTarget>(typedSource))));
        }

        await Task.WhenAll(pressureTasks);
        var finalMemory = GC.GetTotalMemory(forceFullCollection: true);
        var memoryGrowth = finalMemory - initialMemory;
        Console.WriteLine($"Memory growth: {memoryGrowth / 1024 / 1024}MB for stress run");
    }

    private class TestSource
    {
        public int Value { get; set; }
    }

    private class TestTarget
    {
        public int Value { get; set; }
    }
}
