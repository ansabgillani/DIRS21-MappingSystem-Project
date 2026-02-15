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
var source = new GoogleReservation { ReservationCode = "R-100", PrimaryGuestName = "Alex" };

var genericTarget = handler.Map<GoogleReservation, Reservation>(source);
var runtimeTarget = (Reservation)handler.Map(source, typeof(GoogleReservation), typeof(Reservation));

Console.WriteLine($"Parity: {genericTarget.ReservationId == runtimeTarget.ReservationId}");

var sameTypeSource = new Reservation { ReservationId = "ID-1", GuestName = "Original" };
var sameTypeCopy = handler.Map<Reservation, Reservation>(sameTypeSource);
sameTypeSource.GuestName = "Changed";

Console.WriteLine($"Deep Copy: {ReferenceEquals(sameTypeSource, sameTypeCopy) == false}");
```

### Partner Mapper Registration

```csharp
using MappingSystems.Partners.Google.Extensions;

services.AddMapping();
services.AddGooglePartnerMappings();
```

`AddGooglePartnerMappings()` registers Google-specific `ITypeMapper<,>` implementations from `src/Partners/Google/Implementation`.

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
- `Map<TSource, TTarget>(TSource source)` uses a typed fast path (typed registry lookup + typed mapper delegate invocation) for lower per-call overhead.
- `MappingExecutionContext` uses `AsyncLocal` context for nested/collection recursion, scoped to the current logical flow.
- Runtime `Map(object, Type, Type)` validates that `source` is assignable to `sourceType` and fails fast on invalid usage.
- Explicit `ITypeMapper<,>` resolution is scope-safe and resolved per mapping invocation.
- `IdentityConvention` maps same-type value-like data as pass-through and deep-copies mutable reference objects via expression mapping.

Implementation utilities are grouped under `src/Core/Implementation/Utilities` and referenced by conventions/handlers to keep reflection and invocation helpers centralized.

## Key Classes and Methods

- `IMapHandler` / `MapHandler`
	- `Map<TSource, TTarget>(TSource source)` for generic mapping.
	- `Map(object source, Type sourceType, Type targetType)` for runtime type-based mapping.
- `IMapperFactory` / `MapperFactory`
	- `CreateMapper(Type sourceType, Type targetType)` for runtime mapper creation.
- `IMapperRegistry` / `MapperRegistry`
	- `TryGetMapper(...)` and `Register(...)` for cached mapper lifecycle.
- Conventions
	- `IdentityConvention`, `TypeMapperConvention`, `CollectionMappingConvention`, `PropertyConvention`.
- Diagnostics
	- `IMappingDiagnostics` and `MappingDiagnostics` for cache and registration insights.

## Extending the System

- Create partner DTOs in `src/Partners/<Partner>/Models`.
- Add explicit `ITypeMapper<TSource, TTarget>` implementations in `src/Partners/<Partner>/Implementation`.
- Add a partner registration extension in `src/Partners/<Partner>/Extensions`.
- Register in composition root:
	- `services.AddMapping();`
	- `services.Add<PartnerName>PartnerMappings();`

Detailed extension guidance is in [docs/partner-mapping-guide.md](docs/partner-mapping-guide.md).

## Assumptions and Limitations

- Assumptions are documented in [docs/assumptions.md](docs/assumptions.md).
- Design constraints and trade-offs are documented in [docs/decisions.md](docs/decisions.md).
- Current limitations include conservative property matching and `List<T>`-focused collection convention behavior (see [docs/system-design.md](docs/system-design.md)).

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
