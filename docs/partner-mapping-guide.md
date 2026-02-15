# Guide: Add New Partner Mappings Without Modifying Existing Code

## Goal
Integrate a new partner mapping into the DIRS21 Dynamic Mapping System by adding external mapping classes and DI registrations only.

## Core Rule
Do not modify `MapHandler`, `MapperFactory`, `MapperRegistry`, or built-in conventions to add partner-specific behavior.

Use `ITypeMapper<TSource, TTarget>` and DI registration.

---

## 1) Create Partner Contract Models
Define partner models in your partner integration project (or adapter layer), for example:

```csharp
public class PartnerReservationDto
{
    public string Id { get; set; } = string.Empty;
    public string GuestFullName { get; set; } = string.Empty;
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; }
}
```

Define/confirm internal target model:

```csharp
public class InternalReservation
{
    public string ReservationId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}
```

---

## 2) Implement an Explicit Mapper
Add an explicit mapper class:

```csharp
using MappingSystem.Abstractions;

public sealed class PartnerReservationToInternalMapper
    : ITypeMapper<PartnerReservationDto, InternalReservation>
{
    public InternalReservation Map(PartnerReservationDto source)
    {
        return new InternalReservation
        {
            ReservationId = source.Id,
            GuestName = source.GuestFullName,
            CheckInDate = source.ArrivalDate,
            CheckOutDate = source.DepartureDate
        };
    }
}
```

Why this works:
- `TypeMapperConvention` resolves `ITypeMapper<,>` from DI
- Mapper resolution is scope-safe per mapping invocation
- Explicit strategy runs before collection/property conventions

---

## 3) Register the Mapper in DI
In your composition root (API/worker/app startup):

```csharp
services.AddMapping();
services.AddScoped<ITypeMapper<PartnerReservationDto, InternalReservation>, PartnerReservationToInternalMapper>();
```

Optional helper extension in your partner module:

```csharp
public static class PartnerMappingServiceCollectionExtensions
{
    public static IServiceCollection AddPartnerXMappings(this IServiceCollection services)
    {
        services.AddScoped<ITypeMapper<PartnerReservationDto, InternalReservation>, PartnerReservationToInternalMapper>();
        return services;
    }
}
```

Then:

```csharp
services.AddMapping();
services.AddPartnerXMappings();
```

Google partner implementation in this repository follows this pattern with:
- `MappingSystems.Partners.Google.Extensions.AddGooglePartnerMappings()`
- Partner DTOs in `src/Partners/Google/Models`
- Partner mappers in `src/Partners/Google/Implementation`

---

## 4) Use the Mapper Through `IMapHandler`
```csharp
var handler = provider.GetRequiredService<IMapHandler>();
var internalReservation = handler.Map<PartnerReservationDto, InternalReservation>(partnerDto);
```

No framework code changes are required.

---

## 5) Recommended Pattern for Many Partners
For each partner, keep mapping logic isolated by folder/assembly:

- `Integrations.PartnerA/Mappings/...`
- `Integrations.PartnerB/Mappings/...`
- `Integrations.PartnerC/Mappings/...`

Each partner module exposes one extension method, e.g. `AddPartnerAMappings()`.

---

## 6) Validation Checklist for New Partner Mapping
- Mapper class implements `ITypeMapper<TSource, TTarget>`
- Mapper is registered in DI
- `IMapHandler` resolves and maps correctly
- Unit test asserts field-level mapping logic
- Integration test verifies DI + end-to-end mapping call

---

## 7) Common Pitfalls
- Missing DI registration -> fallback to convention or `MappingNotFoundException`
- Registering wrong generic pair -> mapper never selected
- Hidden null assumptions in mapper logic
- Type mismatch when expecting property convention fallback
- Invalid runtime API usage: passing a source object that does not match declared `sourceType`

---

## 8) Example Test Snippets
Unit test:

```csharp
[Fact]
public void PartnerMapper_MapsFieldsCorrectly()
{
    var mapper = new PartnerReservationToInternalMapper();
    var source = new PartnerReservationDto { Id = "P-1", GuestFullName = "Alex" };

    var result = mapper.Map(source);

    result.ReservationId.Should().Be("P-1");
    result.GuestName.Should().Be("Alex");
}
```

Integration test:

```csharp
[Fact]
public void AddMapping_WithPartnerMapper_UsesExplicitMapper()
{
    var services = new ServiceCollection();
    services.AddMapping();
    services.AddScoped<ITypeMapper<PartnerReservationDto, InternalReservation>, PartnerReservationToInternalMapper>();

    var provider = services.BuildServiceProvider();
    var handler = provider.GetRequiredService<IMapHandler>();

    var result = handler.Map<PartnerReservationDto, InternalReservation>(new PartnerReservationDto { Id = "P-1" });

    result.ReservationId.Should().Be("P-1");
}
```
