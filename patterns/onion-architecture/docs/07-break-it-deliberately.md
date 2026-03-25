# Break It Deliberately

## Learning by Breaking

The best way to understand fit functions is to see them catch real violations. This section shows you how to deliberately break the architecture and watch the fit functions fail.

## Violation 1: Domain Depends on EF Core

### The Violation

Modify `src/OnionArch.Domain/OnionArch.Domain.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- VIOLATION: Domain should NOT reference infrastructure -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
  </ItemGroup>
</Project>
```

### Run the Fit Function

```bash
dotnet test fit-functions/OnionArch.FitFunctions/
```

### What Breaks

```
❌ Failed! DomainPurity.Domain_Should_Not_Reference_Infrastructure_Concerns

Error Message:
  Domain must be infrastructure-agnostic.
  Violations: OnionArch.Domain.Entities.Order, OnionArch.Domain.Entities.OrderItem
```

### The Lesson

The fit function **immediately caught** that Domain is referencing an infrastructure library. The test fails with a clear message explaining:
- What rule was violated
- Which types are affected
- Why it's wrong

### Restore

```bash
git restore src/OnionArch.Domain/OnionArch.Domain.csproj
```

## Violation 2: Application Depends on Infrastructure

### The Violation

Modify `src/OnionArch.Application/OnionArch.Application.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\OnionArch.Domain\OnionArch.Domain.csproj" />
  <!-- VIOLATION: Creates circular dependency! -->
  <ProjectReference Include="..\OnionArch.Infrastructure\OnionArch.Infrastructure.csproj" />
</ItemGroup>
```

### What Happens

```bash
dotnet build
```

```
❌ Build FAILED

Error: Circular dependency detected:
  OnionArch.Application -> OnionArch.Infrastructure -> OnionArch.Application
```

The build itself fails before tests even run! This is an extreme violation.

### Run Fit Functions Anyway (After Commenting Out Infrastructure Project)

```
❌ Failed! DependencyRules.Application_Should_Not_Depend_On_Infrastructure_Or_Api

Error Message:
  Application layer should not depend on Infrastructure or API.
  Violations: OnionArch.Application.Commands.CreateOrderCommandHandler
```

### The Lesson

This violation is so severe that:
1. Build system catches it (circular dependency)
2. Fit function would also catch it

Multiple safety nets!

### Restore

```bash
git restore src/OnionArch.Application/OnionArch.Application.csproj
```

## Violation 3: Interface in Infrastructure

### The Violation

Move `IOrderRepository` from Application to Infrastructure:

```bash
# DON'T actually do this - just demonstration
mv src/OnionArch.Application/Interfaces/IOrderRepository.cs \
   src/OnionArch.Infrastructure/Interfaces/IOrderRepository.cs
```

### Run the Fit Function

```
❌ Failed! InterfaceOwnership.Repository_Interfaces_Should_Be_In_Application_Layer

Error Message:
  Repository interfaces must be in Application layer (dependency inversion).
  Expected namespace: OnionArch.Application.Interfaces
  Found in: OnionArch.Infrastructure.Interfaces
```

### The Lesson

Fit function enforces that interfaces belong in the Application layer, proving dependency inversion is correctly implemented.

## Violation 4: Public Setter on Entity

### The Violation

Modify `src/OnionArch.Domain/Entities/Order.cs`:

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

### Run the Fit Function

```bash
dotnet test fit-functions/OnionArch.FitFunctions/
```

### What Breaks

```
❌ Failed! DomainPurity.Domain_Entities_Should_Have_Private_Setters

Error Message:
  Expected violations to be empty, but found 1 item(s):
  Order.Status has public setter
```

### Why It Matters

Without private setters, business rules can be bypassed:

```csharp
var order = Order.Create(customerId);
// Should call: order.Confirm()
// But instead:
order.Status = OrderStatus.Confirmed;  // ❌ Skipped validation!
```

### The Lesson

The fit function uses reflection to inspect all entity properties and catches public setters automatically. This ensures domain entities maintain proper encapsulation and force business logic through methods rather than property mutations.

### Restore

```bash
git restore src/OnionArch.Domain/Entities/Order.cs
```

## Violation 5: Anemic Domain Model

### The Violation

Convert rich `Order` entity to anemic data bag:

```csharp
// VIOLATION: Anemic domain model
public sealed class Order
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public decimal Total { get; set; }
    public List<OrderItem> Items { get; set; }

    // No behavior! Just getters/setters
}

// Business logic leaks to service
public class OrderService
{
    public void ConfirmOrder(Order order)
    {
        if (order.Items.Count == 0)
            throw new Exception("Empty order");

        order.Status = "Confirmed";
        // Business rule in service, not domain!
    }
}
```

### What Doesn't Break (Surprisingly!)

Static analysis tools **won't catch** this. The dependencies are still correct. The architecture is technically valid.

### The Lesson

Fit functions protect **structure**, not **design quality**.

You still need:
- Code reviews to catch anemic models
- Team education on rich domain modeling
- Design principles beyond automated checks

## Summary of What Fit Functions Catch

| Violation Type | Caught By Fit Function? | Example |
|----------------|------------------------|---------|
| Domain depends on EF Core | ✅ Yes | `Domain_Should_Not_Reference_Infrastructure_Concerns` |
| Application depends on Infrastructure | ✅ Yes | `Application_Should_Not_Depend_On_Infrastructure_Or_Api` |
| Interface in wrong layer | ✅ Yes | `Repository_Interfaces_Should_Be_In_Application_Layer` |
| Circular dependencies | ✅ Yes (build + test) | Compiler + `DependencyRules` |
| Value objects not sealed | ✅ Yes | `Value_Objects_Should_Be_Sealed` |
| Public setters on entities | ✅ Yes | `Domain_Entities_Should_Have_Private_Setters` |
| Anemic domain model | ❌ No | Design review needed |
| Poor naming | ❌ No | Team conventions |

## How to Use These Violations

1. **Training**: Show new developers what NOT to do
2. **Workshops**: Live coding sessions breaking and fixing
3. **Documentation**: Examples of common mistakes
4. **Onboarding**: Demonstrate the value of fit functions

**Important:** The `violations/` directory in this repo contains ready-to-use violation examples. Each has documentation explaining the problem and which fit function catches it.

## Try It Yourself

1. Pick a violation from `violations/` directory
2. Copy the violation file over the correct implementation
3. Run `dotnet test fit-functions/OnionArch.FitFunctions/`
4. Observe which test fails and why
5. Read the error message
6. Restore the original file

This hands-on exercise makes the value of fit functions immediately clear!
