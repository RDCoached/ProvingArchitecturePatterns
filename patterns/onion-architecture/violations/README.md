# Onion Architecture Violations

This directory contains examples of **intentionally broken** implementations that violate onion architecture principles. Each violation demonstrates what happens when architectural rules are broken and shows which fit function catches it.

## Purpose

These violations are for educational purposes to demonstrate:
1. Common mistakes developers make
2. How fit functions catch violations automatically
3. Why the architectural rules exist

## Violations Demonstrated

### 1. Domain Depends on EF Core
**File:** `domain-depends-on-ef-core/`
**Violation:** Domain layer references Microsoft.EntityFrameworkCore
**Caught by:** `Domain_Should_Not_Depend_On_Any_Other_Layer`
**Why it's wrong:** Domain should be infrastructure-agnostic

### 2. API Calls Infrastructure Directly
**File:** `api-calls-infrastructure-directly/`
**Violation:** API layer directly uses Infrastructure implementations instead of Application interfaces
**Caught by:** Violates dependency inversion principle
**Why it's wrong:** API should depend on abstractions, not concrete implementations

### 3. Application Depends on Infrastructure
**File:** `application-depends-on-infrastructure/`
**Violation:** Application layer references Infrastructure project
**Caught by:** `Application_Should_Not_Depend_On_Infrastructure_Or_Api`
**Why it's wrong:** Creates circular dependency, breaks dependency inversion

## How to Test Violations

Each violation directory contains a modified version of a project file or source file. To see the fit function fail:

1. Copy the violation file over the correct implementation
2. Run the fit function tests
3. Observe which test fails and the error message
4. Restore the original file

**DO NOT commit violations to your actual codebase!**
