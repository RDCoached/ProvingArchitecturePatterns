# Violation: Public Setters on Domain Entities

## The Problem

This violation shows what happens when domain entities expose public setters, allowing business rules to be bypassed through direct property mutation.

## The Violation

Modify the `Order` entity to have public setters:

```csharp
public sealed class Order
{
    // VIOLATION: Public setter allows bypassing business rules
    public OrderStatus Status { get; set; }  // ❌ Should be private set

    public void Confirm()
    {
        // Business rule can now be bypassed!
        // Anyone can just: order.Status = OrderStatus.Confirmed
    }
}
```

## Why This Is Wrong

1. **Business rules bypassed**: Can set Status directly without validation
2. **Breaks encapsulation**: Internal state exposed for modification
3. **No invariant protection**: Domain logic can't enforce rules
4. **Testing difficulty**: Tests can manipulate entities into invalid states
5. **Anemic domain model**: Encourages putting logic in services instead of entities

## Which Fit Function Catches This

**Test:** `Domain_Entities_Should_Have_Private_Setters`

**Error message:**
```
Expected violations to be empty, but found 1 item(s):
  Order.Status has public setter
```

## How to Reproduce

1. Copy `Order.cs.VIOLATION` to `src/OnionArch.Domain/Entities/Order.cs`
2. Run fit function tests: `dotnet test fit-functions/OnionArch.FitFunctions/`
3. Observe the failing test
4. Restore: `git restore src/OnionArch.Domain/Entities/Order.cs`

## The Impact

Without private setters, code can bypass business logic:

```csharp
var order = Order.Create(customerId);
// Should call: order.Confirm()
// But instead:
order.Status = OrderStatus.Confirmed;  // ❌ Skipped validation!
```

## The Correct Approach

**Use private setters and expose behavior through methods:**

```csharp
public sealed class Order
{
    public OrderStatus Status { get; private set; }  // ✅ Private setter

    public Result Confirm()
    {
        if (_items.Count == 0)
            return Result.Failure("Cannot confirm empty order");

        Status = OrderStatus.Confirmed;  // Only changeable through business method
        ConfirmedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
```

This ensures all state changes go through domain logic that enforces invariants.

## The Lesson

The fit function uses reflection to inspect all entity properties and catches public setters automatically. This ensures domain entities maintain proper encapsulation and force business logic through methods rather than property mutations.
