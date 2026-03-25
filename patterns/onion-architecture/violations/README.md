# Onion Architecture Violations

This directory contains examples of **intentionally broken** implementations that violate onion architecture principles. Each violation demonstrates what happens when architectural rules are broken and shows which fit function catches it.

## Purpose

These violations are for educational purposes to demonstrate:
1. Common mistakes developers make
2. How fit functions catch violations automatically
3. Why the architectural rules exist

## Violations Demonstrated

### 1. Domain Depends on EF Core
**Directory:** `domain-depends-on-ef-core/`
**Violation:** Domain layer references Microsoft.EntityFrameworkCore
**Caught by:** `Domain_Should_Not_Reference_Infrastructure_Concerns`
**Why it's wrong:** Domain should be infrastructure-agnostic

### 2. Application Depends on Infrastructure
**Directory:** `application-depends-on-infrastructure/`
**Violation:** Application layer references Infrastructure project
**Caught by:** `Application_Should_Not_Depend_On_Infrastructure_Or_Api`
**Why it's wrong:** Creates circular dependency, breaks dependency inversion

### 3. Repository Interface in Wrong Layer
**Directory:** `interface-in-wrong-layer/`
**Violation:** IOrderRepository defined in Infrastructure instead of Application
**Caught by:** `Repository_Interfaces_Should_Be_In_Application_Layer`
**Why it's wrong:** Violates dependency inversion, defeats onion architecture

### 4. Public Setters on Domain Entities
**Directory:** `public-setters/`
**Violation:** Domain entity properties have public setters
**Caught by:** `Domain_Entities_Should_Have_Private_Setters`
**Why it's wrong:** Allows bypassing business rules, breaks encapsulation

## How to Test Violations

Each violation directory contains:
- **README.md** - Explanation of the violation and which fit function catches it
- **\*.VIOLATION file** - The broken code to copy over the correct implementation

To see the fit function fail:

1. Read the violation's README for instructions
2. Copy the .VIOLATION file to the appropriate location (renaming it)
3. Run the fit function tests: `dotnet test fit-functions/OnionArch.FitFunctions/`
4. Observe which test fails and the error message
5. Restore the original file: `git restore <file>`

**DO NOT commit violations to your actual codebase!**

## Violations NOT Caught by Fit Functions

Some architectural violations cannot be caught by static analysis:

### Anemic Domain Model
**Problem:** Entities with only getters/setters, business logic in services
**Detection:** Code review, design review
**Why uncatchable:** Static analysis can't distinguish rich vs anemic models

### Direct Infrastructure Instantiation in API
**Problem:** `new OrderRepository(dbContext)` instead of DI
**Detection:** Code review, testing patterns
**Why uncatchable:** Static analysis can't detect DI bypass at runtime

### Poor Naming Conventions
**Problem:** Inconsistent or unclear names
**Detection:** Team conventions, code review
**Why uncatchable:** Subjective judgment required

## Summary

| Violation | Caught? | Fit Function |
|-----------|---------|--------------|
| Domain depends on EF Core | ✅ | `Domain_Should_Not_Reference_Infrastructure_Concerns` |
| Application depends on Infrastructure | ✅ | `Application_Should_Not_Depend_On_Infrastructure_Or_Api` |
| Interface in wrong layer | ✅ | `Repository_Interfaces_Should_Be_In_Application_Layer` |
| Public setters on entities | ✅ | `Domain_Entities_Should_Have_Private_Setters` |
| Anemic domain model | ❌ | Design review needed |
| Direct infrastructure instantiation | ❌ | Code review needed |

Fit functions catch **structural** violations. Design quality still requires human judgment.
