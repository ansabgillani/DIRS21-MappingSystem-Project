# Baseline Performance Results

## Test Environment
- OS: macOS
- CPU: Apple M3 (8 cores)
- .NET: 8.0
- Date: 2026-02-15

## Results

| Method                | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated |
|-----------------------|-----------|-----------|-----------|-------|--------|-----------|
| ManualMapping         | 5.030 ns  | 1.2211 ns | 0.8077 ns | 1.00  | 0.0067 | 56 B      |
| CachedDynamicMapping  | 21.090 ns | 0.5111 ns | 0.2673 ns | 4.43  | 0.0067 | 56 B      |

## Analysis
- Target: CachedDynamicMapping ≤ 10x ManualMapping
- Actual: 4.43x
- Status: PASS

## Additional Load Results
- 10k concurrent calls: 22ms
- Throughput: 449,299 mappings/sec
- Memory growth under stress run: 3MB

## Next Steps
- Continue tracking trend over multiple benchmark runs to smooth per-run variance.
