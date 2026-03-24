# The Problem

## What Goes Wrong Without Onion Architecture?

### 1. Infrastructure Coupling in Domain

**The Scenario:**
Your Order entity directly uses Entity Framework attributes:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class Order
{
    [Key]  // EF Core attribute!
    public int Id { get; set; }

    [Required]  // Data annotation!
    public string Status { get; set; }
}
```

**The Problem:**
- Domain now depends on Microsoft.EntityFrameworkCore
- Can't test Order without EF Core
- Can't swap persistence technology
- Business logic mixed with persistence concerns

**Failure Mode:**
When EF Core releases a breaking change, your domain breaks. When you want to try Dapper or another ORM, you can't without rewriting domain entities.

### 2. Business Logic in Services (Anemic Domain)

**The Scenario:**
```csharp
// Anemic entity - just a data bag
public class Order
{
    public Guid Id { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
}

// Business logic in service
public class OrderService
{
    public void ConfirmOrder(Order order)
    {
        if (order.Items.Count == 0)
            throw new Exception("Can't confirm empty order");

        order.Status = "Confirmed";
        order.Total = order.Items.Sum(i => i.Price * i.Quantity);
    }
}
```

**The Problem:**
- Order is just a data holder (anemic)
- Business rules scattered across services
- Hard to find where rules are enforced
- Easy to bypass rules (directly set Status)
- Can't ensure invariants

**Failure Mode:**
Developer directly sets `order.Status = "Confirmed"` without checking if order is empty. Business rule bypassed. Bug in production.

### 3. Direct Database Dependencies

**The Scenario:**
```csharp
public class GetOrderQuery
{
    private readonly ApplicationDbContext _db;

    public GetOrderQuery(ApplicationDbContext db)
    {
        _db = db;  // Direct dependency on DbContext!
    }

    public async Task<Order> Execute(Guid id)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
```

**The Problem:**
- Query handler directly depends on DbContext
- Can't swap data access technology
- Can't test without database
- Application layer coupled to infrastructure

**Failure Mode:**
You want to cache frequently accessed orders in Redis. Now you need to rewrite the query handler because it's tightly coupled to EF Core.

### 4. Circular Dependencies

**The Scenario:**
```
Application → Infrastructure (for DbContext)
Infrastructure → Application (for query handlers)
```

**The Problem:**
- Circular dependency prevents compilation
- Can't independently version layers
- Changes ripple in both directions
- Impossible to test in isolation

**Failure Mode:**
Build fails with circular dependency error. Or worse, runtime errors from dependency injection container.

### 5. Leaky Abstractions

**The Scenario:**
```csharp
public interface IOrderRepository
{
    IQueryable<Order> GetOrders();  // Leaky! Returns EF queryable
    Task<Order> GetByIdAsync(DbContext db, int id);  // Leaky! Exposes DbContext
}
```

**The Problem:**
- Interface exposes infrastructure details
- Consumer must understand EF Core
- Can't swap implementation
- Not truly abstract

**Failure Mode:**
You try to implement IOrderRepository with Dapper. But Dapper doesn't return IQueryable. Interface is unusable.

### 6. Testing Nightmare

**The Scenario:**
```csharp
[Fact]
public async Task ConfirmOrder_ValidOrder_SetsStatusToConfirmed()
{
    // Need entire database!
    var options = new DbContextOptionsBuilder()
        .UseInMemoryDatabase("TestDb")
        .Options;
    var db = new ApplicationDbContext(options);

    // Seed data
    var customer = new Customer { /* ... */ };
    var product = new Product { /* ... */ };
    await db.Customers.AddAsync(customer);
    await db.Products.AddAsync(product);
    await db.SaveChangesAsync();

    // Create order
    var order = new Order { CustomerId = customer.Id };
    await db.Orders.AddAsync(order);

    // Test the service
    var service = new OrderService(db);
    await service.ConfirmOrder(order.Id);

    // Verify
    var result = await db.Orders.FindAsync(order.Id);
    Assert.Equal("Confirmed", result.Status);
}
```

**The Problem:**
- Testing simple business logic requires database setup
- Slow tests (even in-memory DB has overhead)
- Fragile tests (break when DB schema changes)
- Complex test setup obscures intent

**Failure Mode:**
Tests take 5 minutes to run. Developers stop running them. Bugs slip through.

## Summary of Failure Modes

Without Onion Architecture:

- ❌ **Can't swap technologies** - Locked into EF Core, PostgreSQL, ASP.NET
- ❌ **Can't test easily** - Every test needs database infrastructure
- ❌ **Business rules leak** - Scattered across services, hard to find
- ❌ **Fragile architecture** - Changes ripple across all layers
- ❌ **Tight coupling** - Everything depends on everything
- ❌ **Slow development** - Can't work on domain without infrastructure
- ❌ **Hard to maintain** - No clear boundaries

Onion Architecture solves all of these by enforcing strict layer boundaries and dependency inversion.
