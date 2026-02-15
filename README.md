# MappingSystem

High-performance, runtime-resolved object mapping for .NET 8.

## Features

- Fast cached mapping via compiled delegates
- Thread-safe registry with lock-free lookups
- Convention-based property mapping by name/type
- Explicit strategy support with `ITypeMapper<TSource, TTarget>`
- Nested object recursive mapping with null propagation
- List-to-list element mapping
- Built-in logging and diagnostics services

## Quick Start

### Install / Reference

```bash
dotnet build MappingSystem.slnx
```

### Basic Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using MappingSystem.Core;
using MappingSystem.Extensions;
using MappingSystems.Models.Reservations;
using MappingSystems.Partners.Google.Extensions;
using MappingSystems.Partners.Google.Models;

var services = new ServiceCollection();
services.AddMapping();
services.AddGooglePartnerMappings();
var provider = services.BuildServiceProvider();

var handler = provider.GetRequiredService<IMapHandler>();
var target = handler.Map<GoogleReservation, Reservation>(new GoogleReservation());
```

### Explicit Mapper

```csharp
public class CustomMapper : ITypeMapper<SourceDto, TargetDto>
{
	public TargetDto Map(SourceDto source) => new() { Name = source.Name.ToUpperInvariant() };
}

services.AddMapping();
services.AddGooglePartnerMappings();
```

## Performance

Latest benchmark highlights (Apple M3, .NET 8):

| Method               | Mean      | Allocated |
|----------------------|-----------|-----------|
| ManualMapping        | 5.030 ns  | 56 B      |
| CachedDynamicMapping | 21.090 ns | 56 B      |

Ratio: `4.43x` (within target `≤10x`).

Additional stress results:
- Throughput: ~449k mappings/sec
- 10k concurrent mappings: 22ms
- Stress memory growth: ~3MB

## Architecture

Core pipeline:

1. `MapHandler` receives map request
2. `MapperRegistry` checks cached runtime mapper (`CompiledMapper<TSource,TTarget>`)
3. On miss, `MapperFactory` checks `MappingCache` for compiled delegate
4. If needed, convention pipeline resolves/builds expression and compiles delegate
5. Factory returns `CompiledMapper<TSource,TTarget>` and `MapHandler` registers it
6. Subsequent calls hit registry/cache directly

Core runtime components:
- `MappingCache`: caches compiled delegates by `(sourceType, targetType)`
- `CompiledMapper<TSource,TTarget>`: wraps compiled delegate behind object-safe runtime map API

Convention priority order:

1. `IdentityConvention` (same source/target)
2. `TypeMapperConvention` (explicit `ITypeMapper<,>`)
3. `CollectionMappingConvention` (`List<T>` mappings)
4. `PropertyConvention` (automatic property mapping)

Runtime behavior notes:
- `MappingExecutionContext` uses `AsyncLocal` context for nested/collection recursion, scoped to the current logical flow.
- Runtime `Map(object, Type, Type)` validates that `source` is assignable to `sourceType` and fails fast on invalid usage.
- Explicit `ITypeMapper<,>` resolution is scope-safe and resolved per mapping invocation.

Implementation utilities are grouped under `src/Core/Implementation/Utilities` and referenced by conventions/handlers to keep reflection and invocation helpers centralized.

## Documentation Index

- System design: [docs/system-design.md](docs/system-design.md)
- Architecture overview: [docs/architecture.md](docs/architecture.md)
- Assumptions: [docs/assumptions.md](docs/assumptions.md)
- Design decisions & alternatives: [docs/decisions.md](docs/decisions.md)
- Partner mapping guide: [docs/partner-mapping-guide.md](docs/partner-mapping-guide.md)
- Test strategy: [docs/test-strategy.md](docs/test-strategy.md)
- Nested mapping ADR: [docs/decisions/0001-nested-mapping-approach.md](docs/decisions/0001-nested-mapping-approach.md)
- Performance reports:
	- [docs/performance/baseline-results.md](docs/performance/baseline-results.md)
	- [docs/performance/optimized-results.md](docs/performance/optimized-results.md)

## Logging

`MapHandler` and `MapperFactory` emit:
- `Debug`: mapping attempts and convention evaluation
- `Information`: cache misses and mapper registrations
- `Warning`: no convention found
- `Error`: convention compile failures

## Diagnostics

`IMappingDiagnostics` provides:
- Registered mappings list
- Single mapping lookup
- Current cache size

## Runtime Mapping API

`IMapHandler` supports runtime type-based mapping without requiring caller generics:

```csharp
object result = handler.Map(sourceObject, sourceType, targetType);
```

`IMapperFactory` and `IMapperRegistry` also expose runtime type-pair APIs used by the orchestration flow.

## Build, Test & Run

```bash
dotnet build MappingSystem.slnx
dotnet test MappingSystem.slnx
```

Run sample application:

```bash
dotnet run --project samples/MappingSystem.Sample/MappingSystem.Sample.csproj
```

Run all examples:

```bash
dotnet run --project examples/BasicMapping/BasicMapping.csproj
dotnet run --project examples/ExplicitMappers/ExplicitMappers.csproj
dotnet run --project examples/NestedObjects/NestedObjects.csproj
dotnet run --project examples/Performance/Performance.csproj
```

Run performance suite:

```bash
dotnet run -c Release --project tests/MappingSystem.PerformanceTests/MappingSystem.PerformanceTests.csproj
```

## Requirements

- .NET SDK 8.0+
- C# 12

## License

MIT (placeholder)
