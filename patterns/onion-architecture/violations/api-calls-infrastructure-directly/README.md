# Violation: API Calls Infrastructure Directly

## The Problem

This violation shows what happens when the API layer bypasses the Application layer's interfaces and directly instantiates Infrastructure implementations.

## The Violation

In the endpoint, instead of:
```csharp
[FromServices] GetOrderByIdQueryHandler handler
```

The violation uses:
```csharp
// WRONG: Direct dependency on Infrastructure
var repository = new OrderRepository(dbContext);
var handler = new GetOrderByIdQueryHandler(repository);
```

## Why This Is Wrong

1. **Violates dependency inversion**: API depends on concrete implementations, not abstractions
2. **Tight coupling**: Can't swap implementations without changing API code
3. **Testing difficulty**: Can't mock dependencies in tests
4. **No dependency injection**: Manually constructing objects defeats DI container
5. **Breaks onion flow**: API should only know about Application layer

## Which Fit Function Catches This

This wouldn't be caught by static analysis alone, but violates the principle that:
- API should only depend on Application interfaces
- Infrastructure is plugged in via DI, not directly referenced

## The Impact

- Changes to Infrastructure require changes to API
- Can't use in-memory implementations for testing
- Defeats the purpose of layered architecture

## The Correct Approach

Always inject interfaces from the Application layer:
```csharp
group.MapGet("{id}", async (
    Guid id,
    [FromServices] GetOrderByIdQueryHandler handler,  // Injected via DI
    CancellationToken ct) =>
{
    var query = new GetOrderByIdQuery(id);
    var result = await handler.HandleAsync(query, ct);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
});
```

The DI container wires up the concrete implementation at runtime.
