namespace MappingSystem.UnitTests.TestModels;

public class OrderExternal
{
    public string OrderId { get; set; } = string.Empty;

    public CustomerExternal? Customer { get; set; }

    public decimal TotalAmount { get; set; }
}

public class CustomerExternal
{
    public string CustomerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class OrderInternal
{
    public string OrderId { get; set; } = string.Empty;

    public CustomerInternal? Customer { get; set; }

    public decimal TotalAmount { get; set; }
}

public class CustomerInternal
{
    public string CustomerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
