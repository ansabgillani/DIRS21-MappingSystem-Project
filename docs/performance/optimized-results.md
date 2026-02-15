# Optimized Performance Results

## Optimizations Applied
1. PropertyInfo caching in ExpressionBuilder
2. Registry lookup benchmark verification

## Results Comparison

| Method                | Baseline | Optimized | Improvement |
|-----------------------|----------|-----------|-------------|
| CachedDynamicMapping  | 39.504 ns | 21.090 ns | -46.61%     |

## Memory Impact

| Method                | Baseline Alloc | Optimized Alloc | Improvement |
|-----------------------|----------------|-----------------|-------------|
| CachedDynamicMapping  | 56 B           | 56 B            | 0 B         |

## Registry Comparison

| Method                            | Mean     | Ratio |
|-----------------------------------|----------|-------|
| DirectConcurrentDictionaryLookup  | 11.443 ns | 1.00 |
| RegistryLookup                    | 12.482 ns | 1.09 |

## Acceptance
- [x] Cached mapping ≤ 10x manual baseline
- [x] Memory allocations minimized

## Notes
- Registry lookup overhead is within ~9% of direct `ConcurrentDictionary` mean performance.
- Load-run throughput observed at ~449k mappings/sec with ~3MB stress memory growth.

## Next Steps
- Continue periodic benchmark runs and use median of multiple runs for regression guardrails.
