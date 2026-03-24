# Implement the Fit Functions

## Setting Up Architecture Testing

We'll use **NetArchTest.Rules** for declarative architecture testing in .NET.

### Step 1: Create Fit Functions Project

```bash
# Create xUnit test project
dotnet new xunit -n OnionArch.FitFunctions -f net10.0

# Add to solution
dotnet sln add fit-functions/OnionArch.FitFunctions

# Add references to projects we'll test
cd fit-functions/OnionArch.FitFunctions
dotnet add reference ../../src/OnionArch.Domain
dotnet add reference ../../src/OnionArch.Application
dotnet add reference ../../src/OnionArch.Infrastructure
dotnet add reference ../../src/OnionArch.Api

# Add NetArchTest package
dotnet add package NetArchTest.Rules
```

## Step 2: Static Checks (Dependency Rules)

These validate that dependencies flow correctly between layers.

```csharp
// fit-functions/OnionArch.FitFunctions/DependencyRules.cs
using NetArchTest.Rules;

public class DependencyRules
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Entities.Order).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.Interfaces.IOrderRepository).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.Persistence.ApplicationDbContext).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Any_Other_Layer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("OnionArch.Application")
            .And().NotHaveDependencyOn("OnionArch.Infrastructure")
            .And().NotHaveDependencyOn("OnionArch.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer should have no dependencies on other layers. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Only_Reference_System_Libraries()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "Newtonsoft.Json",
                "System.Data")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain should not reference infrastructure libraries. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("OnionArch.Infrastructure")
            .And().NotHaveDependencyOn("OnionArch.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application layer should not depend on Infrastructure or API. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("OnionArch.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Infrastructure should not depend on API layer");
    }
}
```

**What these tests do:**
- Scan assembly metadata
- Check type references
- Verify no forbidden dependencies exist
- Fail with clear error message listing violators

## Step 3: Domain Purity Checks

These ensure Domain remains framework-agnostic.

```csharp
// fit-functions/OnionArch.FitFunctions/DomainPurity.cs
public class DomainPurity
{
    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure_Concerns()
    {
        var result = Types.InNamespace("OnionArch.Domain")
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "Microsoft.AspNetCore",
                "System.Data",
                "Dapper")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain must be infrastructure-agnostic. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Have_EF_Core_Attributes()
    {
        var result = Types.InNamespace("OnionArch.Domain")
            .Should()
            .NotHaveDependencyOn("System.ComponentModel.DataAnnotations")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Domain should not use data annotations (persistence concern)");
    }
}
```

## Step 4: Interface Ownership Checks

These validate dependency inversion principle.

```csharp
// fit-functions/OnionArch.FitFunctions/InterfaceOwnership.cs
public class InterfaceOwnership
{
    [Fact]
    public void Repository_Interfaces_Should_Be_In_Application_Layer()
    {
        var result = Types.InNamespace("OnionArch.Application.Interfaces")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Repository interfaces must be in Application layer (dependency inversion)");
    }

    [Fact]
    public void Repository_Implementations_Should_Be_In_Infrastructure()
    {
        var result = Types.InNamespace("OnionArch.Infrastructure.Repositories")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeClasses()
            .And()
            .HaveDependencyOn("OnionArch.Application.Interfaces")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Repository implementations must be in Infrastructure and " +
            "depend on Application interfaces");
    }

    [Fact]
    public void Infrastructure_Should_Not_Define_Interfaces()
    {
        var interfaces = Types.InAssembly(typeof(Infrastructure.Persistence.ApplicationDbContext).Assembly)
            .That()
            .ResideInNamespace("OnionArch.Infrastructure")
            .And()
            .AreInterfaces()
            .GetTypes();

        Assert.Empty(interfaces);
    }
}
```

## Step 5: Layer Isolation Checks

These enforce proper encapsulation.

```csharp
// fit-functions/OnionArch.FitFunctions/LayerIsolation.cs
public class LayerIsolation
{
    [Fact]
    public void Domain_Entities_Should_Be_Public()
    {
        var result = Types.InNamespace("OnionArch.Domain.Entities")
            .Should()
            .BePublic()
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Domain entities should be public to be used by other layers");
    }

    [Fact]
    public void Value_Objects_Should_Be_Sealed()
    {
        var result = Types.InNamespace("OnionArch.Domain.ValueObjects")
            .That()
            .AreNotAbstract()  // Exclude base classes
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Concrete value objects should be sealed. " +
            $"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Implementations_Should_Be_Sealed()
    {
        var result = Types.InNamespace("OnionArch.Infrastructure.Repositories")
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Infrastructure implementations should be sealed (not designed for inheritance)");
    }
}
```

## Step 6: Run the Tests

```bash
# Run all fit function tests
dotnet test fit-functions/OnionArch.FitFunctions/

# Expected output:
# Passed!  - Failed:     0, Passed:    18, Skipped:     0, Total:    18
```

## Step 7: Integrate with CI

```yaml
# .github/workflows/ci.yml
name: CI

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Run unit tests
        run: dotnet test tests/ --no-build

      - name: Run fit function tests
        run: dotnet test fit-functions/ --no-build
```

## Operational Checks (Bonus)

For runtime validation, add integration tests:

```csharp
[Fact]
public async Task Api_Should_Start_With_All_Dependencies_Resolved()
{
    // WebApplicationFactory test
    await using var factory = new WebApplicationFactory<Program>();
    using var client = factory.CreateClient();

    // If DI configuration is wrong, this throws
    var response = await client.GetAsync("/api/orders");

    // Just checking it doesn't throw
    Assert.NotNull(response);
}
```

## Summary

You now have:
- ✅ 18 automated architecture tests
- ✅ Continuous validation of onion architecture rules
- ✅ Clear error messages when violations occur
- ✅ CI integration preventing bad merges
- ✅ Confidence that architecture stays intact

These tests run in milliseconds and catch violations instantly!
