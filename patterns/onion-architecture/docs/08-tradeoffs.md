# Trade-offs

## Benefits

### 1. Framework Independence

**Benefit:** Core business logic doesn't depend on any framework.

**Real-world impact:**
- Swap EF Core for Dapper: Only change Infrastructure layer
- Migrate from ASP.NET to gRPC: Only change API layer
- Replace PostgreSQL with MongoDB: Infrastructure change only

**Example:**
```csharp
// Domain entity - same code regardless of framework
public sealed class Order
{
    public Result Confirm() { /* ... */ }
}

// Tomorrow you could use:
// - EF Core, Dapper, or NHibernate (Infrastructure choice)
// - REST API, gRPC, or GraphQL (API choice)
// - PostgreSQL, MongoDB, or Cosmos DB (Database choice)
```

Domain code never changes!

### 2. Testability

**Benefit:** Test business logic without infrastructure.

**Real-world impact:**
```csharp
// Test pure domain logic - no database needed!
[Fact]
public void Confirm_EmptyOrder_ReturnsFailure()
{
    var order = Order.Create(CustomerId.New());
    var result = order.Confirm();

    Assert.True(result.IsFailure);
    Assert.Equal("Cannot confirm empty order", result.Error);
}
// Runs in milliseconds!
```

Versus traditional approach:
```csharp
// Traditional: Need database for everything
[Fact]
public async Task Confirm_EmptyOrder_ReturnsError()
{
    var db = new TestDatabase();  // Slow!
    await db.SeedData();           // More setup
    var service = new OrderService(db);

    await service.ConfirmOrder(orderId);
    // Test takes seconds instead of milliseconds
}
```

**Impact:** Tests run 100x faster. Developers actually run them!

### 3. Clear Boundaries

**Benefit:** Obvious where code belongs.

**Real-world impact:**
- New feature? Start in Domain
- Data access? Infrastructure only
- HTTP concern? API layer
- Use case? Application layer

No confusion, no "where does this go?" debates.

### 4. Parallel Development

**Benefit:** Teams can work independently on layers.

**Real-world impact:**
- Backend team implements Domain + Application
- Infrastructure team sets up database
- Frontend team mocks Application interfaces
- All work in parallel!

### 5. Enforced Design Principles

**Benefit:** Fit functions prevent architecture violations.

**Real-world impact:**
- Junior developer adds EF Core to Domain → Build fails
- Code reviewer sees clear fit function error
- Architecture stays intact over years

## Drawbacks

### 1. More Projects and Files

**Drawback:** More complexity than single-project solution.

**Real-world impact:**
```
Simple app: 1 project, 20 files
Onion arch: 4 projects, 50+ files (for same functionality)
```

**When it hurts:**
- Small CRUD apps
- Prototypes and MVPs
- Teams unfamiliar with pattern

**Mitigation:**
- Use simpler architecture for simple apps
- Template repositories for quick starts
- Good documentation

### 2. Interface Explosion

**Drawback:** Need interface for every infrastructure concern.

**Real-world impact:**
```csharp
// Need to define interface in Application
public interface IOrderRepository { /* ... */ }
public interface IEmailSender { /* ... */ }
public interface IPaymentGateway { /* ... */ }
public interface ICacheService { /* ... */ }

// Then implement in Infrastructure
public class OrderRepository : IOrderRepository { /* ... */ }
public class EmailSender : IEmailSender { /* ... */ }
// etc...
```

**When it hurts:**
- Many infrastructure concerns
- Rapid prototyping
- Simple scenarios where abstraction adds no value

**Mitigation:**
- Only abstract what needs substitution
- Start simple, add interfaces when needed
- Use code generation for boilerplate

### 3. Steeper Learning Curve

**Drawback:** Team needs to understand:
- Dependency inversion
- Layer responsibilities
- When to use each layer
- How to navigate the architecture

**Real-world impact:**
- Junior developers confused at first
- More onboarding time
- Initial slowdown

**Mitigation:**
- Good documentation (like this!)
- Code reviews teaching principles
- Pair programming
- Team training sessions

### 4. Overkill for CRUD

**Drawback:** Too much architecture for simple data entry.

**Real-world impact:**
```csharp
// For simple "Save a user" scenario:

// Domain layer
public class User { public Name Name { get; set; } }

// Application layer
public interface IUserRepository { /* ... */ }
public class CreateUserHandler { /* ... */ }

// Infrastructure layer
public class UserRepository : IUserRepository { /* ... */ }

// API layer
app.MapPost("/users", handler => /* ... */);

// All that for: INSERT INTO Users VALUES (...)
```

**When it hurts:**
- Admin panels
- Internal tools
- Simple data entry apps
- Reporting systems

**Mitigation:**
- Use vertical slice architecture instead
- Reserve onion for complex domains
- Hybrid: onion for core, simple approach for CRUD

### 5. Initial Development Slowdown

**Drawback:** More setup time upfront.

**Real-world impact:**
- Simple feature in traditional: 30 minutes
- Same feature in onion: 1 hour first time (setup layers)
- But second feature: 30 minutes (structure exists)

**When it hurts:**
- Tight deadlines
- Proof of concepts
- Startups finding product-market fit

**Mitigation:**
- Template repositories
- Code generators
- Start simple, refactor to onion when needed

## Common Mistakes

### 1. Leaky Repository Abstractions

**Mistake:**
```csharp
public interface IOrderRepository
{
    IQueryable<Order> GetAll();  // ❌ Leaks EF Core!
}
```

**Fix:**
```csharp
public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync();  // ✅ Clean abstraction
}
```

### 2. Anemic Domain Model

**Mistake:**
```csharp
public class Order
{
    public Guid Id { get; set; }
    public string Status { get; set; }  // ❌ Just a data bag
}
```

**Fix:**
```csharp
public class Order
{
    public OrderId Id { get; private set; }
    public OrderStatus Status { get; private set; }

    public Result Confirm() { /* Business logic here */ }  // ✅ Rich behavior
}
```

### 3. Business Logic in Application Services

**Mistake:**
```csharp
public class ConfirmOrderHandler
{
    public async Task Handle(ConfirmOrderCommand cmd)
    {
        var order = await _repo.GetByIdAsync(cmd.OrderId);

        // ❌ Business logic in handler!
        if (order.Items.Count == 0)
            throw new Exception("Cannot confirm empty order");

        order.Status = OrderStatus.Confirmed;
    }
}
```

**Fix:**
```csharp
public class ConfirmOrderHandler
{
    public async Task<Result> Handle(ConfirmOrderCommand cmd)
    {
        var order = await _repo.GetByIdAsync(cmd.OrderId);

        var result = order.Confirm();  // ✅ Domain does the work!

        if (result.IsFailure)
            return result;

        await _repo.UpdateAsync(order);
        return Result.Success();
    }
}
```

### 4. Application Depending on Infrastructure

**Mistake:** Application references Infrastructure project.

**Fix:** Application defines interfaces, Infrastructure implements them.

### 5. Too Many Layers

**Mistake:** Adding "Application.Contracts", "Domain.Services", "Infrastructure.Data", etc.

**Fix:** Stick to four layers. Don't over-engineer.

## When to Use Onion Architecture

### ✅ Good Fit

- Complex business domain
- Long-lived applications (5+ years)
- Multiple UI/API types
- High test coverage requirements
- Distributed teams
- Expected technology changes
- Domain-driven design

### ❌ Poor Fit

- Simple CRUD apps
- Short-lived prototypes
- Database-centric applications
- Tight deadlines with inexperienced team
- Admin panels
- Reporting tools
- Very small applications

## Alternatives

- **Vertical Slice Architecture** - Better for CRUD-heavy apps
- **Clean Architecture** - Very similar, slightly different layer names
- **Hexagonal Architecture (Ports & Adapters)** - Same principles, different metaphor
- **Traditional Layered** - Simpler, but couples business logic to infrastructure
- **Modular Monolith** - Better for large teams, complex domains

## Bottom Line

Onion Architecture is **powerful but not free**.

Use it when:
- Business logic is complex
- Application will live for years
- Team understands the principles
- Testability and flexibility are priorities

Skip it when:
- Building a quick MVP
- Domain is simple (CRUD)
- Team is unfamiliar and deadline is tight

**There's no silver bullet. Choose the right tool for the job.**
