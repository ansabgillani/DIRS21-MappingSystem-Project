# MappingSystem

High-performance, runtime-resolved object mapping for .NET 8.

## Quick Start

Build and test:

```bash
dotnet build MappingSystem.slnx
dotnet test MappingSystem.slnx
```

Run sample app:

```bash
dotnet run --project samples/MappingSystem.Sample/MappingSystem.Sample.csproj
```

## Minimal Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using MappingSystem.Core;
using MappingSystem.Extensions;

var services = new ServiceCollection();
services.AddMapping();

var provider = services.BuildServiceProvider();
var handler = provider.GetRequiredService<IMapHandler>();

var target = handler.Map<SourceDto, TargetDto>(new SourceDto());

object data = new SourceDto();
var targetFromObject = handler.Map<SourceDto, TargetDto>(data);

object runtimeTarget = handler.Map(
    data,
    "Your.Namespace.SourceDto",
    "Your.Namespace.TargetDto");
```

## Additional Mapping Methodologies

High-priority runtime API:

- `mapHandler.Map(object data, string sourceType, string targetType)`
  - Supports mapping when types are discovered at runtime (config, metadata, plugin boundaries).
  - Resolves type names from loaded assemblies, then executes the same validated runtime mapping flow.

Additional generic-object API:

- `handler.Map<SourceDto, TargetDto>(object data)`
  - Keeps strong target typing while accepting `object` input from dynamic pipelines.
  - Removes repetitive caller-side casts and centralizes type safety/validation in the mapper.
  - Improves system ergonomics for mixed static/dynamic workflows without bypassing cache or convention behavior.

## IMapHandler Overload Guide

1) `Map<TSource, TTarget>(TSource source)`
- When to use: default path for strongly typed source objects.

```csharp
var target = handler.Map<SourceDto, TargetDto>(typedSource);
```

2) `Map<TSource, TTarget>(object data)`
- When to use: source arrives as `object`, but `TSource`/`TTarget` are known.

```csharp
object data = typedSource;
var target = handler.Map<SourceDto, TargetDto>(data);
```

3) `Map(object data, Type sourceType, Type targetType)`
- When to use: runtime metadata already provides `Type` instances.

```csharp
var target = (TargetDto)handler.Map(data, typeof(SourceDto), typeof(TargetDto));
```

4) `Map(object data, string sourceType, string targetType)`
- When to use: runtime metadata provides type names as strings.

```csharp
var target = (TargetDto)handler.Map(data, "My.Namespace.SourceDto", "My.Namespace.TargetDto");
```

## Features

- Compiled and cached mappers for steady-state performance
- Generic and runtime mapping APIs
- Convention pipeline with deterministic order
- Explicit mapper extension via `ITypeMapper<TSource, TTarget>`
- Nested object and list mapping support
- Logging and diagnostics support

## Documentation

- Architecture overview: [docs/architecture.md](docs/architecture.md)
- System design: [docs/system-design.md](docs/system-design.md)
- Design decisions: [docs/decisions.md](docs/decisions.md)
- Assumptions: [docs/assumptions.md](docs/assumptions.md)
- Partner mapping guide: [docs/partner-mapping-guide.md](docs/partner-mapping-guide.md)
- Test strategy: [docs/test-strategy.md](docs/test-strategy.md)
- Nested mapping ADR: [docs/decisions/0001-nested-mapping-approach.md](docs/decisions/0001-nested-mapping-approach.md)
- Performance reports:
  - [docs/performance/baseline-results.md](docs/performance/baseline-results.md)
  - [docs/performance/optimized-results.md](docs/performance/optimized-results.md)

## Examples

```bash
dotnet run --project examples/BasicMapping/BasicMapping.csproj
dotnet run --project examples/ExplicitMappers/ExplicitMappers.csproj
dotnet run --project examples/NestedObjects/NestedObjects.csproj
dotnet run --project examples/Performance/Performance.csproj
```

## Requirements

- .NET SDK 8.0+
- C# 12

## License

MIT (placeholder)
