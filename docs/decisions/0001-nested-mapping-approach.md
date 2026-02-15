# 0001: Nested Object Mapping Approach

## Context
Users need to map objects with nested complex properties (for example, Order -> Customer).
We must decide how generated expressions access IMapHandler for recursive mapping.

## Decision
Use static `MapHandlerAccessor.Instance` to provide IMapHandler to generated expressions.

## Alternatives

### Option A: Static Accessor (CHOSEN)
```csharp
var handler = MapHandlerAccessor.Instance;
target.Nested = handler.Map<TSource, TTarget>(source.Nested);
```

Pros:
- Simple expression generation
- No signature changes to compiled delegates
- Works with current caching architecture

Cons:
- Static mutable state
- Could cause issues in parallel test execution
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
- Static state requires careful test setup/teardown.
- Parallel test suites may require process isolation.

## Performance Impact
- Near-zero overhead static field access.
- No per-map closure allocations.

## Risks
- Multiple DI containers share last-constructed MapHandler.
- Parallel testing may need additional safeguards.

## Future Improvements
- Consider parameterized mapper delegate signatures in a future phase.
- Add `MapHandlerAccessor.Reset()` utility for test cleanup.