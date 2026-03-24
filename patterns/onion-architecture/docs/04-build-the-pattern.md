# Build the Pattern

## Step-by-Step Implementation Guide

Follow these steps to build Onion Architecture from scratch. This guide matches the actual implementation in this repository.

### Prerequisites

- .NET 10 SDK installed
- PostgreSQL (or Docker for running it)
- Your favorite IDE (Rider, VS Code, Visual Studio)

## Step 1: Create Solution Structure

```bash
# Create solution
dotnet new sln -n OnionArchitecture

# Create projects (innermost to outermost)
dotnet new classlib -n OnionArch.Domain -f net10.0
dotnet new classlib -n OnionArch.Application -f net10.0
dotnet new classlib -n OnionArch.Infrastructure -f net10.0
dotnet new webapi -n OnionArch.Api -f net10.0 --use-controllers false

# Add to solution
dotnet sln add src/OnionArch.Domain
dotnet sln add src/OnionArch.Application
dotnet sln add src/OnionArch.Infrastructure
dotnet sln add src/OnionArch.Api
```

## Step 2: Set Up Project References

**Critical:** Dependencies flow inward only!

```bash
# Application depends on Domain
cd src/OnionArch.Application
dotnet add reference ../OnionArch.Domain

# Infrastructure depends on Application and Domain
cd ../OnionArch.Infrastructure
dotnet add reference ../OnionArch.Application
dotnet add reference ../OnionArch.Domain

# API depends on Infrastructure and Application (for DI wiring)
cd ../OnionArch.Api
dotnet add reference ../OnionArch.Application
dotnet add reference ../OnionArch.Infrastructure
```

**Verify:** Domain project has ZERO references!

## Step 3: Build the Domain Layer

### 3.1: Create Value Objects

```csharp
// Domain/ValueObjects/Money.cs
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency required");
        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");
        return Create(Amount + other.Amount, Currency);
    }
}
```

### 3.2: Create Strongly-Typed IDs

```csharp
// Domain/ValueObjects/OrderId.cs
public sealed record OrderId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);
}
```

### 3.3: Create Entities with Behavior

```csharp
// Domain/Entities/Order.cs
public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }

    // Private constructor for EF Core
#pragma warning disable CS8618
    private Order() { }
#pragma warning restore CS8618

    private Order(OrderId id, CustomerId customerId, string currency)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        TotalAmount = Money.Zero(currency);
    }

    public static Order Create(CustomerId customerId, string currency = "USD")
    {
        return new Order(OrderId.New(), customerId, currency);
    }

    public Result AddItem(ProductId productId, Quantity quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Cannot modify confirmed order");

        var item = OrderItem.Create(productId, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotal();
        return Result.Success();
    }

    public Result Confirm()
    {
        if (!_items.Any())
            return Result.Failure("Cannot confirm empty order");

        Status = OrderStatus.Confirmed;
        return Result.Success();
    }

    private void RecalculateTotal()
    {
        var total = Money.Zero(TotalAmount.Currency);
        foreach (var item in _items)
            total = total.Add(item.TotalPrice);
        TotalAmount = total;
    }
}
```

**Key points:**
- Private setters (encapsulation)
- Business logic in methods
- Factory method for creation
- Validates invariants
- Zero infrastructure dependencies

## Step 4: Build the Application Layer

### 4.1: Define Repository Interface

```csharp
// Application/Interfaces/IOrderRepository.cs
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
```

**Critical:** Interface lives in Application, NOT Infrastructure!

### 4.2: Create Command Handlers

```csharp
// Application/Commands/ConfirmOrderCommand.cs
public sealed record ConfirmOrderCommand(Guid OrderId);

public sealed class ConfirmOrderCommandHandler
{
    private readonly IOrderRepository _repository;

    public ConfirmOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> HandleAsync(ConfirmOrderCommand command, CancellationToken ct)
    {
        var orderId = OrderId.From(command.OrderId);
        var order = await _repository.GetByIdAsync(orderId, ct);

        if (order is null)
            return Result.Failure("Order not found");

        var result = order.Confirm();  // Domain logic!
        if (result.IsFailure)
            return result;

        await _repository.UpdateAsync(order, ct);
        return Result.Success();
    }
}
```

## Step 5: Build the Infrastructure Layer

### 5.1: Add NuGet Packages

```bash
cd src/OnionArch.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### 5.2: Create DbContext

```csharp
// Infrastructure/Persistence/ApplicationDbContext.cs
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new OrderConfiguration());
    }
}
```

### 5.3: Create Entity Configurations

```csharp
// Infrastructure/Persistence/Configurations/OrderConfiguration.cs
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.From(value));

        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount");
            money.Property(m => m.Currency).HasColumnName("Currency");
        });
    }
}
```

### 5.4: Implement Repository

```csharp
// Infrastructure/Repositories/OrderRepository.cs
public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken ct)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task UpdateAsync(Order order, CancellationToken ct)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }
}
```

## Step 6: Build the API Layer

### 6.1: Configure Dependency Injection

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Handlers
builder.Services.AddScoped<ConfirmOrderCommandHandler>();

var app = builder.Build();
```

### 6.2: Create Endpoints

```csharp
// Endpoints/OrderEndpoints.cs
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapPost("{id}/confirm", async (
            Guid id,
            ConfirmOrderCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new ConfirmOrderCommand(id);
            var result = await handler.HandleAsync(command, ct);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        });
    }
}
```

## Step 7: Create Database Migration

```bash
cd src/OnionArch.Infrastructure
dotnet ef migrations add InitialCreate
```

## Final Result

You now have a complete Onion Architecture implementation with:
- ✅ Domain layer with rich entities (zero dependencies)
- ✅ Application layer with use cases (depends on Domain only)
- ✅ Infrastructure layer implementing interfaces (depends on Application)
- ✅ API layer wiring everything together

**Test it:**
```bash
# Run the API
dotnet run --project src/OnionArch.Api

# Or use Docker
docker-compose up
```

The pattern is now ready for fit function validation!
