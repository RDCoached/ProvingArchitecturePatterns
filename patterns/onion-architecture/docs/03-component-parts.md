# Component Parts

## The Moving Pieces of Onion Architecture

Onion Architecture consists of four primary layers, each with distinct responsibilities.

## 1. Domain Layer (Core)

**Location in onion:** Center (innermost layer)
**Dependencies:** ZERO - only System libraries
**Responsibility:** Pure business logic

### Components:

#### Entities
Rich domain objects with behavior and business rules.

```csharp
public sealed class Order
{
    public OrderId Id { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<OrderItem> _items = [];

    public Result Confirm()
    {
        if (!_items.Any())
            return Result.Failure("Cannot confirm empty order");

        Status = OrderStatus.Confirmed;
        return Result.Success();
    }
}
```

**Key Characteristics:**
- Private setters (encapsulation)
- Business methods (not just properties)
- Validates invariants
- No infrastructure dependencies

#### Value Objects
Immutable objects defined by their attributes.

```csharp
public sealed record Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Currency mismatch");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

**Key Characteristics:**
- Immutable (record type)
- Self-validating
- Behavior methods
- Equality by value

#### Domain Events
Represent significant occurrences in the domain.

```csharp
public sealed record OrderConfirmed(OrderId OrderId, DateTime ConfirmedAt);
```

#### Enums
Domain-specific enumerations.

```csharp
public enum OrderStatus
{
    Draft,
    Confirmed,
    Paid,
    Shipped,
    Delivered
}
```

## 2. Application Layer

**Location in onion:** Second layer (wraps Domain)
**Dependencies:** Domain only
**Responsibility:** Orchestrate domain logic, define infrastructure contracts

### Components:

#### Interfaces (Owned by Application!)
Contracts for infrastructure services.

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
```

**Key Point:** Interface lives in Application, NOT Infrastructure!

#### Command Handlers
Handle write operations (state changes).

```csharp
public sealed class ConfirmOrderCommandHandler
{
    private readonly IOrderRepository _repository;

    public async Task<Result> HandleAsync(ConfirmOrderCommand command, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, ct);
        if (order is null) return Result.Failure("Order not found");

        var result = order.Confirm();  // Domain logic!
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(order, ct);
        return Result.Success();
    }
}
```

**Key Characteristics:**
- Thin orchestration
- Domain does the work
- Uses interfaces, not implementations

#### Query Handlers
Handle read operations.

```csharp
public sealed class GetOrderByIdQueryHandler
{
    private readonly IOrderRepository _repository;

    public async Task<Result<OrderDto>> HandleAsync(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, ct);
        if (order is null) return Result.Failure<OrderDto>("Not found");

        return Result.Success(MapToDto(order));
    }
}
```

#### DTOs (Data Transfer Objects)
Flat structures for data transfer across boundaries.

```csharp
public sealed record OrderDto(
    Guid Id,
    string Status,
    decimal TotalAmount,
    List<OrderItemDto> Items
);
```

**Key Characteristics:**
- No business logic
- Simple data structures
- Used at API boundary

## 3. Infrastructure Layer

**Location in onion:** Third layer (wraps Application)
**Dependencies:** Application, Domain
**Responsibility:** Implement infrastructure concerns

### Components:

#### Repository Implementations
Concrete implementations of Application interfaces.

```csharp
public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;

    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }
}
```

**Key Point:** Implements interface from Application layer!

#### DbContext
Entity Framework Core database context.

```csharp
public sealed class ApplicationDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new OrderConfiguration());
    }
}
```

#### Entity Configurations
Map domain entities to database schema.

```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OrderId.From(value));
        // More mappings...
    }
}
```

**Key Characteristics:**
- Converts domain types to database types
- Handles value objects as owned entities
- Maps private fields

## 4. API Layer (Presentation)

**Location in onion:** Outermost layer
**Dependencies:** All other layers (for DI wiring)
**Responsibility:** HTTP endpoints, composition root

### Components:

#### Endpoints
HTTP API endpoints using minimal APIs.

```csharp
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
```

#### Dependency Injection Configuration
Wires everything together.

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(/* ... */);
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ConfirmOrderCommandHandler>();
```

**Key Point:** This is the ONLY place where concrete implementations meet abstractions!

## Dependency Flow

```
API Layer
  ↓ depends on
Infrastructure Layer
  ↓ depends on
Application Layer (owns interfaces)
  ↓ depends on
Domain Layer (zero dependencies)
```

But at runtime, through dependency injection:

```
API → Application Interface ← Infrastructure Implementation
```

This is **Dependency Inversion** in action!

## Responsibilities Summary

| Layer | Knows About | Responsible For | Dependencies |
|-------|-------------|-----------------|--------------|
| **Domain** | Business rules only | Entities, Value Objects, Domain logic | None |
| **Application** | Domain + Use cases | Orchestration, Interfaces | Domain |
| **Infrastructure** | Data access, External services | Repositories, DbContext, External APIs | Application, Domain |
| **API** | HTTP, REST, JSON | Endpoints, Request/Response, DI wiring | All (for composition) |

Each layer has a clear, single responsibility. This makes the system maintainable and testable.
