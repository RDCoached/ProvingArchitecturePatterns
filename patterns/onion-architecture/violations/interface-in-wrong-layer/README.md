# Violation: Repository Interface in Infrastructure Layer

## The Problem

This violation shows what happens when repository interfaces are defined in the Infrastructure layer instead of the Application layer, violating the Dependency Inversion Principle.

## The Violation

Move `IOrderRepository` from `Application/Interfaces/` to `Infrastructure/Interfaces/`:

```csharp
namespace OnionArch.Infrastructure.Interfaces;  // ❌ WRONG LAYER!

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    // ...
}
```

## Why This Is Wrong

1. **Violates Dependency Inversion**: Infrastructure owns the abstraction, making Application depend on Infrastructure
2. **Creates circular dependency**: Application → Infrastructure → Application interfaces
3. **Defeats onion architecture**: Arrows point outward instead of inward
4. **Tight coupling**: Application becomes coupled to Infrastructure layer structure
5. **Testing difficulty**: Can't mock dependencies without referencing Infrastructure

## Which Fit Function Catches This

**Test:** `Repository_Interfaces_Should_Be_In_Application_Layer`

**Error message:**
```
Repository interfaces must be in Application layer (dependency inversion).
Expected namespace: OnionArch.Application.Interfaces
Found in: OnionArch.Infrastructure.Interfaces
```

## How to Reproduce

1. Move `src/OnionArch.Application/Interfaces/IOrderRepository.cs` to `src/OnionArch.Infrastructure/Interfaces/IOrderRepository.cs`
2. Update the namespace to `OnionArch.Infrastructure.Interfaces`
3. Run fit function tests: `dotnet test fit-functions/OnionArch.FitFunctions/`
4. Observe the failing test
5. Restore: Move the file back and fix the namespace

## The Correct Approach

**Application layer owns the interfaces:**
```csharp
namespace OnionArch.Application.Interfaces;  // ✅ CORRECT

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    // ...
}
```

**Infrastructure implements them:**
```csharp
namespace OnionArch.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository  // Implements Application's interface
{
    // Implementation details
}
```

This ensures Application doesn't depend on Infrastructure - the dependency arrow points inward.
