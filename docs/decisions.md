# DIRS21 Dynamic Mapping System - Design Decisions and Alternatives

## Purpose
This document records key architecture decisions in the current solution and compares them with viable alternatives, including pros and cons.

---

## Decision 1: Runtime Mapping Engine with Compiled Delegates
### Chosen
Use runtime expression generation + delegate compilation + caching.

### Why
Balances flexibility (dynamic type-pair resolution) with strong steady-state performance.

### Alternatives
1. Pure reflection-based mapping
- Pros: simplest implementation, no compile step
- Cons: slower hot path, higher per-call overhead

2. Source-generated mapping only
- Pros: very fast runtime, AOT-friendly
- Cons: less flexible for unknown partner types at runtime, more build-time complexity

3. Third-party mapper library
- Pros: mature ecosystem, less custom maintenance
- Cons: lower control over conventions/priorities/diagnostics design

---

## Decision 2: Two-Level Caching (MappingCache + MapperRegistry)
### Chosen
Use MappingCache for compiled delegates and MapperRegistry for runtime mapper objects.

### Why
Separates concerns:
- delegate reuse during factory creation path,
- runtime mapper retrieval and execution path.

### Alternatives
1. Single shared dictionary only
- Pros: fewer moving parts
- Cons: less explicit lifecycle separation, harder to reason about evolution

2. No persistent cache
- Pros: minimal memory footprint
- Cons: repeated compilation/reflection overhead, poor throughput

---

## Decision 3: Convention Pipeline with Explicit Priority
### Chosen
Priority order:
1) Identity, 2) Explicit ITypeMapper, 3) Collection, 4) Property.

### Why
Ensures deterministic behavior and protects partner-specific mapping intent.

### Alternatives
1. First-registered convention wins (implicit priority)
- Pros: simpler registration model
- Cons: brittle behavior, difficult to reason about at scale

2. Score-based dynamic convention resolution
- Pros: flexible
- Cons: more complexity, less predictability, harder debugging

---

## Decision 4: Explicit Partner Mapping via ITypeMapper<TSource, TTarget>
### Chosen
Partner customization implemented through interface-based explicit mappers registered in DI.

### Why
Enables adding new partner behavior without modifying core framework classes.

### Alternatives
1. Attribute-based mapping rules on DTOs
- Pros: concise for simple cases
- Cons: leaks mapping concerns into contracts, weaker separation of concerns

2. Central mega-configuration file
- Pros: one place to manage mappings
- Cons: high coupling and merge contention; harder module ownership

---

## Decision 5: AsyncLocal Execution Context for Nested Mapping (Current)
### Chosen
Use `MappingExecutionContext` (`AsyncLocal<IMapHandler?>`) for recursive nested/collection expression execution.

### Why
Maintains simple compiled delegate signatures and low per-map overhead.

### Alternatives
1. Pass IMapHandler as delegate parameter
- Pros: no global mutable state
- Cons: signature changes across pipeline, larger refactor

2. Capture handler in closure
- Pros: avoids static global
- Cons: extra closure allocations and lifetime complexity

---

## Decision 6: Conservative Property Convention (Exact Type Match)
### Chosen
Map only when source/target property types match exactly.

### Why
Reduces hidden conversion bugs and keeps behavior predictable.

### Alternatives
1. Implicit conversion matrix (e.g., int->long, string->enum)
- Pros: higher convenience
- Cons: surprising behavior, conversion edge cases, debugging complexity

2. Configurable conversion plugins by default
- Pros: flexible
- Cons: more moving parts and policy surface area

---

## Decision 7: Thread-Safe Lock-Free Structures
### Chosen
Use ConcurrentDictionary with first-writer-wins semantics.

### Why
Safe under concurrency while keeping operations simple and performant.

### Alternatives
1. Global locks around dictionary operations
- Pros: straightforward logic
- Cons: contention and lower throughput under load

2. Immutable snapshots
- Pros: clean read semantics
- Cons: write amplification and extra allocations

---

## Decision 8: Built-In Logging + Diagnostics
### Chosen
Include structured logging and mapping diagnostics in framework services.

### Why
Improves operability and production troubleshooting.

### Alternatives
1. Logging only at host layer
- Pros: thinner framework
- Cons: less visibility into internal resolution flow

2. Diagnostics only in debug builds
- Pros: reduced production surface
- Cons: limited supportability during live incidents

---

## Decision 9: Testing Strategy Layers
### Chosen
Layered coverage: unit -> integration -> API-host integration (plus performance/load).

### Why
Captures correctness at component level and runtime behavior at host boundary.

### Alternatives
1. Unit tests only
- Pros: faster pipelines
- Cons: misses wiring/host issues

2. End-to-end only
- Pros: realistic behavior
- Cons: slower feedback, weaker fault localization

---

## Summary Guidance
Use current architecture when you need:
- runtime flexibility for unknown type pairs,
- deterministic strategy precedence,
- strong performance after warm-up,
- partner extension without core code edits.

Re-evaluate major decisions if requirements shift toward:
- strict AOT-only constraints,
- multi-tenant isolation requirements,
- cross-process distributed mapper state.
