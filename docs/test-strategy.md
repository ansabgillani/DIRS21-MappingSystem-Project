# Dynamic Mapping System Test Strategy

## 1) Purpose
Define a practical, layered test strategy for the DIRS21 Dynamic Mapping System covering:
- Unit testing
- Integration testing
- API-host integration testing

This strategy ensures correctness, extensibility, performance confidence, and safe partner onboarding.

---

## 2) Quality Objectives
- Verify mapping correctness for explicit and convention paths
- Validate convention priority and fallback behavior
- Guarantee thread-safe caching behavior under concurrency
- Protect runtime type-based mapping APIs
- Ensure new partner mappings can be added without framework code changes

---

## 3) Test Pyramid
### A) Unit Tests (largest layer)
Scope: isolated component behavior without full app host.

Primary targets:
- `MapperRegistry`
- `MappingCache`
- `CompiledMapper<TSource, TTarget>`
- `MapperFactory`
- `MapHandler`
- Conventions (`Identity`, `TypeMapper`, `Collection`, `Property`)
- `ExpressionBuilder`

What to verify:
- Correct mapping output and null behavior
- Runtime and generic API compatibility
- Runtime API guardrail behavior for invalid source/sourceType combinations
- First-writer-wins registration semantics
- Cache-hit behavior (factory cache and registry cache)
- Exception behavior (`MappingNotFoundException`, compilation failures)

---

### B) Integration Tests (middle layer)
Scope: DI wiring + end-to-end mapping through `IMapHandler`.

Primary targets:
- `AddMapping()` registration correctness
- Convention order behavior
- Explicit mapper precedence over property convention
- Fallback to convention when explicit mapper is absent

What to verify:
- Services resolve from container
- Real map calls execute expected strategy
- Partner mapper registration works with no core code changes

---

### C) API-Host Integration Tests (top/system layer)
Scope: full application host behavior (HTTP + DI + mapping + serialization).

Recommended setup:
- ASP.NET Core minimal API or MVC host
- `WebApplicationFactory<TEntryPoint>`
- Test-only host configuration with `AddMapping()` and partner mapper registrations

What to verify:
- Endpoint request DTO -> domain mapping correctness
- Domain -> response DTO mapping correctness
- Error responses for unsupported mappings
- Logging/diagnostic telemetry presence (optional assertions)

Example scenarios:
1. `POST /reservations/import` receives partner DTO and persists mapped domain entity
2. Endpoint without partner mapper returns controlled mapping error response
3. Multiple partner mapper registrations coexist and route correctly by DTO type

---

## 4) Performance and Load Validation
Keep performance validation in `tests/MappingSystem.PerformanceTests`.

Objectives:
- Ensure cached mapping remains within defined ratio vs manual mapping
- Detect regressions in allocations
- Confirm concurrency throughput and memory growth bounds

Minimum checks:
- Benchmark: manual vs cached mapping
- Benchmark: registry lookup behavior
- Load run: high-concurrency map calls

---

## 5) Test Data and Determinism
Guidelines:
- Use deterministic values (fixed timestamps/IDs where practical)
- Avoid flaky time-based assertions
- Keep fixture objects small and focused
- For nested/collection tests, include both non-null and null cases

---

## 6) CI Execution Plan
Recommended stages:
1. Build: `dotnet build MappingSystem.slnx`
2. Unit + integration tests: `dotnet test MappingSystem.slnx`
3. Coverage report (threshold gate)
4. Optional nightly performance suite (`Release` mode)

Suggested coverage policy:
- Unit/integration gate: >= 80% on core implementation projects
- New partner mapper modules: require mapper-level unit tests and one integration test

---

## 7) API-Host Integration Test Blueprint
Suggested project:
- `tests/MappingSystem.ApiHostIntegrationTests`

Suggested dependencies:
- `Microsoft.AspNetCore.Mvc.Testing`
- `xunit`
- `FluentAssertions`

Suggested structure:
- `Fixtures/ApiHostFactory.cs` (`WebApplicationFactory` customization)
- `Tests/ReservationImportTests.cs`
- `Tests/MappingErrorHandlingTests.cs`
- `Tests/PartnerMapperRegistrationTests.cs`

---

## 8) Non-Functional Risks to Cover
- Async-flow context behavior for nested/collection recursion across concurrent tests
- Execution-context flow loss in custom thread scheduling scenarios
- Unexpected fallback from explicit to convention mapping due to DI misconfiguration

Mitigations:
- Keep unit test parallelization strategy explicit
- Add integration tests for mapper registration and precedence
- Document host startup requirements for mappings

---

## 9) Definition of Done (Testing)
A change is test-complete when:
- Relevant unit tests exist and pass
- Integration tests validate DI and resolution path
- API-host integration tests (if endpoint-facing change) validate request/response mapping
- No regression in existing mapping behavior
- Build and test commands pass in CI
