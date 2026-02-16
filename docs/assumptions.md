# DIRS21 Dynamic Mapping System - Assumptions

## Purpose
This document captures the explicit and implicit assumptions behind the current Dynamic Mapping System architecture and implementation.

## 1) Product and Domain Assumptions
- Partner payload shapes are heterogeneous and may change independently of internal domain models.
- Mapping logic must be extensible per partner without modifying framework core code.
- The system primarily maps DTO-style objects with readable/writable properties.

## 2) Runtime and Platform Assumptions
- Runtime target is .NET 8 with C# 12 features available.
- Dependency injection is available and used as the standard composition mechanism.
- Mapping execution is in-process; cross-process cache sharing is not required.

## 3) Extensibility Assumptions
- Explicit partner strategies are implemented through ITypeMapper<TSource, TTarget> registrations.
- Explicit strategies should take precedence over convention-based fallback.
- Teams integrating partners can create their own extension methods for registration.

## 4) Mapping Semantics Assumptions
- Same-type mapping returns pass-through for value-like types and deep-copy for mutable reference objects.
- Property convention mapping requires exact type match and case-insensitive property-name match.
- Collection convention targets List<T> to List<T> semantics.
- Nested mapping behavior uses recursive map invocation with null propagation.

## 5) Performance Assumptions
- First-call compile cost is acceptable in exchange for faster steady-state execution.
- Most production traffic is expected to be cache-hit dominated after warm-up.
- Delegate compilation and caching overhead is amortized over repeated type-pair usage.

## 6) Concurrency and State Assumptions
- Concurrent mapping calls are normal and expected under load.
- Concurrent first-map races are acceptable (first writer wins in caches/registries).
- AsyncLocal execution context access used for nested expression recursion is acceptable in current deployment model.

## 7) Observability Assumptions
- Logging should be available in production hosts and configurable by level.
- Diagnostics are informational and not required for core mapping correctness.

## 8) Testing Assumptions
- Unit tests provide the primary correctness safety net for mapper behavior.
- Integration tests verify DI registration, convention priority, and end-to-end flow.
- API-host integration testing is required for host-facing mapping behavior validation.

## 9) Security and Compliance Assumptions
- Mapping inputs are considered untrusted and should not bypass host-level validation.
- Mapping layer is not the primary authorization boundary.
- PII handling requirements are enforced at host/business layers, not by mapping primitives.

## 10) Known Constraint Assumptions
- MappingExecutionContext depends on async execution-context flow for nested/collection recursion.
- Multiple DI container scenarios may require careful initialization ordering.
- Out-of-box conventions are intentionally conservative rather than conversion-heavy.

## 11) Revisit Triggers
These assumptions should be revisited if:
- partner count and mapping variety grow substantially,
- cross-process reuse/distribution is required,
- AOT/source-generation becomes a hard requirement,
- static accessor limitations impact multi-tenant or parallel host scenarios.
