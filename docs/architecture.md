# DIRS21 Dynamic Mapping System Architecture

## Purpose
This document is a concise architecture overview. For detailed component behavior, constraints, and sequence details, see [docs/system-design.md](system-design.md).

## Core Pipeline
1. `MapHandler` receives map requests (generic and runtime overloads).
2. `MapperRegistry` returns cached runtime mappers by type pair.
3. `MapperFactory` creates missing mappers using `MappingCache` + convention pipeline.
4. Compiled mapper is registered and reused on subsequent calls.

## Strategy Order
1. `IdentityConvention`
2. `TypeMapperConvention` (explicit `ITypeMapper<,>`)
3. `CollectionMappingConvention` (`List<T>`)
4. `PropertyConvention`

This order ensures explicit partner mappers take precedence over convention fallback.

## Extension Model
- Keep core unchanged for partner onboarding.
- Add partner DTOs + explicit `ITypeMapper<TSource, TTarget>` implementations under partner modules.
- Register partner mappings through DI extension methods.

## Runtime Notes
- Generic map path uses a typed fast path.
- Generic map also supports `Map<TSource, TTarget>(object data)` for object-based pipelines with central type validation.
- Runtime map path validates source assignability.
- Runtime map supports `Map(object data, string sourceType, string targetType)` to resolve type names from loaded assemblies before mapping.
- Nested and collection recursion uses `MappingExecutionContext` (`AsyncLocal`) in generated expressions.

## IMapHandler Overloads: Examples and When to Use

1. `Map<TSource, TTarget>(TSource source)`
	- Use when caller already has strongly typed source data.
	- Best default for compile-time safety and lowest call-site overhead.

	```csharp
	var result = handler.Map<GoogleReservation, Reservation>(googleReservation);
	```

2. `Map<TSource, TTarget>(object data)`
	- Use when upstream pipeline provides `object` payloads but target types are known at compile-time.
	- Avoids repetitive casts in callers while preserving typed mapping output.

	```csharp
	object data = googleReservation;
	var result = handler.Map<GoogleReservation, Reservation>(data);
	```

3. `Map(object data, Type sourceType, Type targetType)`
	- Use when source/target types are resolved dynamically at runtime as `Type` instances.
	- Common for plugin systems or metadata-driven pipelines.

	```csharp
	var result = (Reservation)handler.Map(data, typeof(GoogleReservation), typeof(Reservation));
	```

4. `Map(object data, string sourceType, string targetType)`
	- Use when type identities are transported as strings (configuration, external metadata, message envelopes).
	- Convenient boundary API that resolves names to runtime `Type` and reuses existing runtime path.

	```csharp
	var result = (Reservation)handler.Map(
		 data,
		 "MappingSystems.Partners.Google.Models.GoogleReservation",
		 "MappingSystems.Models.Reservations.Reservation");
	```

## Diagrams
- Class diagram image: ![Dynamic Mapping Class UML](diagrams/class-uml.png)
- Mermaid source: [docs/diagrams/class-uml.mmd](diagrams/class-uml.mmd)

## Related Documents
- Design details: [docs/system-design.md](system-design.md)
- Design decisions: [docs/decisions.md](decisions.md)
- Partner onboarding: [docs/partner-mapping-guide.md](partner-mapping-guide.md)
