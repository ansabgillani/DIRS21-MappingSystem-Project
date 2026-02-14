using BenchmarkDotNet.Running;
using MappingSystem.PerformanceTests.Benchmarks;
using MappingSystem.PerformanceTests.LoadTests;

BenchmarkRunner.Run<MappingBenchmarks>();
BenchmarkRunner.Run<RegistryBenchmarks>();
await ConcurrencyLoadRunner.RunAsync();
