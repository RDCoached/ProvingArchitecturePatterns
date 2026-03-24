# The Fit Functions

## What Architectural Assumptions Need Protecting?

Onion Architecture is defined by strict rules about dependencies and layer boundaries. These rules are easy to violate accidentally. **Fit functions** are automated tests that continuously verify architectural assumptions.

## Core Architectural Assumptions

### 1. Dependency Flow Is Inward Only

**Assumption:** Dependencies must flow from outer layers to inner layers, never the reverse.

**What needs protection:**
- Domain NEVER depends on Application, Infrastructure, or API
- Application NEVER depends on Infrastructure or API
- Infrastructure NEVER depends on API

**Why it matters:** Reversing dependencies defeats the entire pattern. If Domain depends on Infrastructure, you lose framework independence and testability.

### 2. Domain Is Pure

**Assumption:** The Domain layer has zero dependencies on infrastructure libraries.

**What needs protection:**
- No Entity Framework in Domain
- No ASP.NET in Domain
- No Npgsql in Domain
- Only System.* libraries allowed

**Why it matters:** Domain purity ensures business logic is framework-agnostic and can be tested in isolation.

### 3. Interfaces Are Owned by Consumers

**Assumption:** Interfaces are defined in the Application layer (consumer), not Infrastructure layer (provider).

**What needs protection:**
- IOrderRepository lives in Application
- OrderRepository implementation lives in Infrastructure
- Infrastructure implements Application interfaces

**Why it matters:** This is dependency inversion. Infrastructure adapts to Application's needs, not vice versa.

### 4. Entities Have Proper Encapsulation

**Assumption:** Domain entities protect their invariants through encapsulation.

**What needs protection:**
- Private setters on entity properties
- State changes through behavior methods only
- No public constructors (use factory methods)

**Why it matters:** Without encapsulation, business rules can be bypassed. Anyone could set `order.Status = "Confirmed"` without validation.

### 5. Layer Isolation

**Assumption:** Each layer is properly isolated with appropriate visibility.

**What needs protection:**
- Domain entities are public (used by other layers)
- Application interfaces are public (implemented by Infrastructure)
- Infrastructure implementations are sealed (not designed for inheritance)
- Value objects are sealed (immutable)

**Why it matters:** Proper visibility enforces contracts and prevents misuse.

## Fit Functions to Implement

Based on these assumptions, we need:

### Dependency Rules (6 fit functions)
1. Domain_Should_Not_Depend_On_Any_Other_Layer
2. Domain_Should_Only_Reference_System_Libraries
3. Application_Should_Not_Depend_On_Infrastructure_Or_Api
4. Application_Can_Only_Depend_On_Domain
5. Infrastructure_Should_Not_Depend_On_Api
6. Infrastructure_Must_Depend_On_Application

### Domain Purity (4 fit functions)
1. Domain_Should_Not_Have_EF_Core_Attributes
2. Domain_Entities_Should_Have_Private_Setters
3. Domain_Should_Not_Reference_Infrastructure_Concerns
4. Domain_Entities_Should_Be_In_Correct_Namespace

### Interface Ownership (4 fit functions)
1. Repository_Interfaces_Should_Be_In_Application_Layer
2. Infrastructure_Should_Not_Define_Interfaces
3. Repository_Implementations_Should_Be_In_Infrastructure
4. Interfaces_Should_Be_Named_With_I_Prefix

### Layer Isolation (4 fit functions)
1. Domain_Entities_Should_Be_Public
2. Application_Interfaces_Should_Be_Public
3. Infrastructure_Implementations_Should_Be_Sealed
4. Value_Objects_Should_Be_Sealed

## What Should Be Continuously Verified

Fit functions should run:

1. **On every build** - Fast feedback loop
2. **In CI pipeline** - Prevent violations from merging
3. **Before deployment** - Final gate
4. **In local development** - Catch issues early

## Expected Outcomes

When fit functions pass:
- ✅ Architecture is correctly implemented
- ✅ Dependencies flow in the right direction
- ✅ Domain is pure and testable
- ✅ Layers are properly isolated

When fit functions fail:
- ❌ Clear error message explaining the violation
- ❌ List of types that violate the rule
- ❌ Build fails (fail-fast)
- ❌ Cannot proceed until fixed

## Benefits of Fit Functions

1. **Prevents architectural drift** - Code can't violate rules over time
2. **Documents architecture** - Tests serve as executable documentation
3. **Onboards new developers** - Violations caught immediately with clear messages
4. **Enables refactoring** - Confidence that architecture stays intact
5. **Catches mistakes early** - Before code review, before production

## Relationship to Traditional Tests

| Test Type | What It Validates | When It Runs |
|-----------|-------------------|--------------|
| **Unit Tests** | Business logic correctness | Every build |
| **Integration Tests** | Components work together | CI pipeline |
| **Fit Functions** | **Architecture rules** | **Every build** |
| **E2E Tests** | User scenarios | Before deployment |

Fit functions are NOT about functionality - they're about **structure and design**.

Think of them as:
- Unit tests validate "does it work?"
- Fit functions validate "is it built correctly?"

Both are essential. You can have working code with terrible architecture. Fit functions prevent that.
