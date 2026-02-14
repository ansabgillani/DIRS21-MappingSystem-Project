using FluentAssertions;
using MappingSystem.Core;
using MappingSystem.Implementation;
using MappingSystem.Implementation.Conventions;
using MappingSystem.UnitTests.TestModels;
using Microsoft.Extensions.Logging.Abstractions;

namespace MappingSystem.UnitTests;

public class NestedMappingTests
{
    private readonly IMapHandler _handler;
    private readonly IMapperRegistry _mapperRegistry;

    public NestedMappingTests()
    {
        _mapperRegistry = new MapperRegistry();
        var conventions = new IMappingConvention[] { new PropertyConvention() };
        var factory = new MapperFactory(conventions, NullLogger<MapperFactory>.Instance);
        _handler = new MapHandler(_mapperRegistry, factory, NullLogger<MapHandler>.Instance);
    }

    [Fact]
    public void Map_WithNestedObject_MapsRecursively()
    {
        var external = new OrderExternal
        {
            OrderId = "ORD-001",
            TotalAmount = 99.99m,
            Customer = new CustomerExternal
            {
                CustomerId = "CUST-123",
                Name = "Alice Johnson",
                Email = "alice@example.com"
            }
        };

        var internalOrder = _handler.Map<OrderExternal, OrderInternal>(external);

        internalOrder.OrderId.Should().Be("ORD-001");
        internalOrder.TotalAmount.Should().Be(99.99m);
        internalOrder.Customer.Should().NotBeNull();
        internalOrder.Customer!.CustomerId.Should().Be("CUST-123");
        internalOrder.Customer.Name.Should().Be("Alice Johnson");
        internalOrder.Customer.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void Map_WithNullNestedObject_AssignsNull()
    {
        var external = new OrderExternal
        {
            OrderId = "ORD-002",
            TotalAmount = 50.00m,
            Customer = null
        };

        var internalOrder = _handler.Map<OrderExternal, OrderInternal>(external);

        internalOrder.OrderId.Should().Be("ORD-002");
        internalOrder.Customer.Should().BeNull();
    }

    [Fact]
    public void Map_NestedObject_CachesNestedMapper()
    {
        var external = new OrderExternal
        {
            OrderId = "ORD-003",
            Customer = new CustomerExternal { CustomerId = "CUST-456" }
        };

        _handler.Map<OrderExternal, OrderInternal>(external);

        _mapperRegistry.IsRegistered(typeof(CustomerExternal), typeof(CustomerInternal)).Should().BeTrue();
    }
}
