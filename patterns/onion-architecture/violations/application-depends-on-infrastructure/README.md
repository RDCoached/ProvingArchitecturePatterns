# Violation: Application Depends on Infrastructure

## The Problem

This violation shows what happens when the Application layer adds a project reference to the Infrastructure layer, creating a circular dependency.

## The Violation

Modified `OnionArch.Application.csproj` includes:

```xml
<ItemGroup>
  <ProjectReference Include="..\OnionArch.Domain\OnionArch.Domain.csproj" />
  <!-- VIOLATION: Application should NOT reference Infrastructure -->
  <ProjectReference Include="..\OnionArch.Infrastructure\OnionArch.Infrastructure.csproj" />
</ItemGroup>
```

## Why This Is Wrong

1. **Circular dependency**: Infrastructure already references Application (for interfaces)
   - Application → Infrastructure
   - Infrastructure → Application
   - = Circular dependency!

2. **Defeats dependency inversion**: Application should define interfaces, not use implementations

3. **Breaks the onion**: Dependency flow must be inward only

4. **Coupling**: Application becomes coupled to specific infrastructure technology

## Which Fit Function Catches This

**Test:** `Application_Should_Not_Depend_On_Infrastructure_Or_Api`

**Error message:**
```
Application layer should not depend on Infrastructure or API.
Violations: [Types in Application that reference Infrastructure]
```

## Common Scenario Where This Happens

Developer wants to use `DbContext` directly in a query handler:

```csharp
// WRONG - This requires Application to reference Infrastructure
public class GetOrderByIdQueryHandler
{
    private readonly ApplicationDbContext _dbContext;  // ❌ Infrastructure type!

    public GetOrderByIdQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}
```

## The Correct Approach

Use repository interface defined in Application:

```csharp
// CORRECT - Application defines and uses interface
public class GetOrderByIdQueryHandler
{
    private readonly IOrderRepository _repository;  // ✅ Application interface!

    public GetOrderByIdQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }
}
```

Infrastructure implements `IOrderRepository` using `DbContext`, but Application doesn't know or care about the implementation details.

## Dependency Flow

```
✅ CORRECT:
Domain (no dependencies)
  ↑
Application (depends on Domain only)
  ↑
Infrastructure (depends on Application + Domain)
  ↑
API (depends on all layers for DI wiring)

❌ WRONG:
Application ← Infrastructure  (circular!)
```
