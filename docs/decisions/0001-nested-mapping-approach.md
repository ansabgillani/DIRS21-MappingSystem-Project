# 0001: Nested Object Mapping Approach

## Context
Users need to map objects with nested complex properties (for example, Order -> Customer).
We must decide how generated expressions access IMapHandler for recursive mapping.

## Decision
Use `MappingExecutionContext.CurrentHandler` backed by `AsyncLocal<IMapHandler?>` to provide context-scoped handler access for generated expressions.

## Alternatives

### Option A: AsyncLocal Execution Context (CHOSEN)
```csharp
var handler = MappingExecutionContext.CurrentHandler;
target.Nested = handler.Map<TSource, TTarget>(source.Nested);
```

Pros:
- Simple expression generation
- No signature changes to compiled delegates
- Scoped to logical async execution flow

Cons:
- Still context-driven global access pattern
- Requires execution context to flow correctly in custom threading scenarios
- Requires MapHandler initialization

### Option B: Pass IMapHandler as Parameter
```csharp
// Signature: Func<TSource, IMapHandler, TTarget>
target.Nested = mapHandler.Map<TSource, TTarget>(source.Nested);
```

Pros:
- No static state
- Explicit dependency
- More testable

Cons:
- Changes delegate signature everywhere
- Breaks registry caching (needs refactor)
- More complex expression generation

### Option C: Inject IMapHandler via Closure
```csharp
var handler = _handler;
```

Pros:
- No static state
- Each expression has its own handler reference

Cons:
- Closure allocations on every compile
- Memory overhead per mapper
- More complex lifetime management

## Consequences
- MapHandler must be instantiated before any mapping.
- Execution context must be present for recursive nested/collection mapping.
- Parallel test suites should preserve async context flow.

## Performance Impact
- Near-zero overhead `AsyncLocal` access.
- No per-map closure allocations.

## Risks
- Async context flow can be lost if custom thread handoff bypasses execution context.
- Multiple DI containers still require deterministic initialization in host composition.

## Future Improvements
- Consider parameterized mapper delegate signatures in a future phase.
- Consider explicit handler-parameter mapping delegates if strict isolation is required.