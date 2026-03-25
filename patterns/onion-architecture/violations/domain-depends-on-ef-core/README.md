# Violation: Domain Depends on EF Core

## The Problem

This violation shows what happens when the Domain layer takes a direct dependency on Entity Framework Core.

## The Violation

The modified `OnionArch.Domain.csproj` includes:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
```

## Why This Is Wrong

1. **Infrastructure coupling**: Domain becomes coupled to a specific ORM
2. **Not framework-agnostic**: Can't swap EF Core for another persistence technology
3. **Breaks onion architecture**: Domain is supposed to be the innermost layer with zero dependencies
4. **Testing difficulty**: Domain tests now require EF Core infrastructure

## Which Fit Function Catches This

**Test:** `Domain_Should_Not_Reference_Infrastructure_Concerns`

**Error message:**
```
Domain must be infrastructure-agnostic.
Violations: [Types that reference Microsoft.EntityFrameworkCore]
```

**Note:** The test `Domain_Should_Only_Reference_System_Libraries` would also catch this violation.

## How to Reproduce

1. Copy `OnionArch.Domain.csproj.VIOLATION` to `src/OnionArch.Domain/OnionArch.Domain.csproj`
2. Run fit function tests: `dotnet test fit-functions/OnionArch.FitFunctions/`
3. Observe the failing test
4. Restore the original file

## The Correct Approach

Keep Domain pure - no infrastructure dependencies. Use EF Core only in the Infrastructure layer, with proper entity configurations to map domain entities to the database.
