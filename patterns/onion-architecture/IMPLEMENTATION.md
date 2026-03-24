# Onion Architecture - Implementation Log

This document tracks the step-by-step implementation of the Onion Architecture pattern for the Proving Architecture Patterns course.

## Implementation Steps

### Step 1: Repository Structure Setup

**Date:** 2026-03-24

**Objective:** Create the folder structure and .NET solution for the Onion Architecture pattern.

**Actions:**
1. Created base folder structure:
   ```
   patterns/onion-architecture/
   ├── src/           # Source code for all layers
   ├── tests/         # Unit and integration tests
   ├── fit-functions/ # Architecture validation tests
   ├── violations/    # Deliberately broken examples
   └── docs/          # 9-section course documentation
   ```

2. Created .NET 10 solution: `OnionArchitecture.sln`

3. Created source projects following onion architecture layers (innermost to outermost):
   - **OnionArch.Domain** (classlib) - Core domain logic, zero dependencies
   - **OnionArch.Application** (classlib) - Use cases and interfaces
   - **OnionArch.Infrastructure** (classlib) - Infrastructure implementations
   - **OnionArch.Api** (webapi) - HTTP endpoints and composition root

4. Set up project references following dependency inversion:
   ```
   Domain: No references (pure)
   Application → Domain
   Infrastructure → Application, Domain
   Api → Application, Infrastructure
   ```

5. Created test projects for each layer:
   - OnionArch.Domain.Tests
   - OnionArch.Application.Tests
   - OnionArch.Infrastructure.Tests
   - OnionArch.Api.Tests

6. Created fit functions project:
   - OnionArch.FitFunctions with NetArchTest.Rules package
   - References all source projects to validate architecture rules

7. Added all projects to the solution

8. Verified build: ✅ All 9 projects build successfully

**Result:** Complete solution structure with proper layer separation and dependency flow. Ready for implementation.

**Next:** Implement Domain layer with rich entities and value objects.

---

### Step 2: Domain Layer Implementation

**Date:** 2026-03-24

**Objective:** Build the core domain with rich behavior and zero infrastructure dependencies.

**Actions:**

1. Created **Value Objects** (immutable, self-validating):
   - `Money` - Currency-aware monetary values with arithmetic operations
   - `StronglyTypedId<T>` - Base class for type-safe IDs
   - `OrderId`, `CustomerId`, `ProductId` - Strongly-typed identifiers
   - `Quantity` - Positive integer quantities with domain logic

2. Created **Enums**:
   - `OrderStatus` - Order lifecycle states (Draft, Confirmed, Paid, Shipped, Delivered, Cancelled)

3. Created **Common Types**:
   - `Result` and `Result<T>` - Railway-oriented programming for operation outcomes
   - Provides explicit success/failure with error messages

4. Created **Entities** with rich business logic:
   - `OrderItem` - Line item with product, quantity, price calculations
   - `Order` (aggregate root) - Order entity with:
     * State management (status transitions)
     * Business rules (can't confirm empty order, can't modify confirmed order)
     * Invariant protection (total recalculation)
     * Behavior: AddItem, RemoveItem, Confirm, Cancel, MarkAsPaid, Ship, Deliver

**Key Design Decisions:**

- **Zero dependencies:** Domain has NO NuGet packages, only System libraries
- **Immutability:** Value objects are immutable records
- **Encapsulation:** All entity state changes go through behavior methods
- **Validation:** Business rules enforced at domain level, not in setters
- **Result pattern:** Operations return explicit success/failure instead of throwing
- **Strongly-typed IDs:** Prevent primitive obsession, catch ID mix-ups at compile time

**Verified:**
✅ Domain project builds successfully
✅ No external dependencies
✅ Rich domain model with actual business logic (not anemic)

**Next:** Implement Application layer with use cases and repository interfaces.

---

### Step 3: Application Layer Implementation

**Date:** 2026-03-24

**Objective:** Create use case handlers and define infrastructure interfaces.

**Actions:**

1. Created **Repository Interfaces** (owned by Application, implemented by Infrastructure):
   - `IOrderRepository` - Defines contract for order persistence
   - Methods: GetByIdAsync, GetByCustomerIdAsync, AddAsync, UpdateAsync, DeleteAsync, ExistsAsync
   - **Key insight:** Interface lives in Application layer, not Infrastructure (dependency inversion)

2. Created **DTOs** for data transfer:
   - `OrderDto` - Order data transfer object
   - `OrderItemDto` - Order item data transfer object
   - Flat structures for API responses, no domain logic

3. Created **Command Handlers** (write operations):
   - `CreateOrderCommandHandler` - Creates new orders
   - `AddOrderItemCommandHandler` - Adds items to existing orders
   - `ConfirmOrderCommandHandler` - Confirms draft orders
   - Each handler orchestrates domain logic through entities

4. Created **Query Handlers** (read operations):
   - `GetOrderByIdQueryHandler` - Retrieves single order
   - `GetOrdersByCustomerQueryHandler` - Retrieves customer's orders
   - Returns DTOs, not domain entities

**Key Design Decisions:**

- **Interface ownership:** Interfaces defined in Application, not Infrastructure (dependency inversion principle)
- **Command/Query separation:** Clear distinction between writes (commands) and reads (queries)
- **Thin handlers:** Handlers orchestrate, domain entities contain business logic
- **DTO mapping:** Domain entities mapped to DTOs at application boundary
- **Result pattern usage:** Handlers return Result<T> for explicit success/failure

**Dependencies:**
- Application → Domain (only)
- No references to Infrastructure or API

**Verified:**
✅ Application project builds successfully
✅ Only references Domain project
✅ Interfaces define infrastructure contracts

**Next:** Implement Infrastructure layer with EF Core repository implementations.

---

### Step 4: Infrastructure Layer Implementation

**Date:** 2026-03-24

**Objective:** Implement infrastructure concerns using EF Core and PostgreSQL.

**Actions:**

1. Added **NuGet Packages**:
   - Microsoft.EntityFrameworkCore (10.0.5)
   - Microsoft.EntityFrameworkCore.Design (10.0.5)
   - Npgsql.EntityFrameworkCore.PostgreSQL (10.0.1)

2. Created **DbContext**:
   - `ApplicationDbContext` - Main database context
   - Configures entities using separate configuration classes
   - Clean separation between domain and persistence concerns

3. Created **Entity Configurations** (EF Core mapping):
   - `OrderConfiguration` - Maps Order aggregate to database
     * Value conversions for strongly-typed IDs (OrderId, CustomerId)
     * Owned entity for Money value object
     * Cascade delete for order items
   - `OrderItemConfiguration` - Maps OrderItem entity
     * Shadow property for database-generated ID
     * Value conversions for ProductId and Quantity
     * Owned entities for Money values

4. Created **Repository Implementation**:
   - `OrderRepository` implements `IOrderRepository` from Application layer
   - Uses EF Core for data access
   - Includes navigation properties in queries
   - SaveChanges pattern for persistence

5. Created **Design-Time Factory**:
   - `ApplicationDbContextFactory` for EF Core migrations
   - Allows migrations without full API startup

6. Created **Database Migration**:
   - InitialCreate migration with Orders and OrderItems tables
   - Proper column types, constraints, and relationships

7. Updated **Domain Entities** for EF Core compatibility:
   - Added parameterless private constructors to Order and OrderItem
   - Required for EF Core materialization
   - Doesn't compromise domain encapsulation (still private)

**Key Design Decisions:**

- **Dependency inversion demonstrated:** Infrastructure implements interfaces defined in Application
- **Value object persistence:** Money is persisted as owned entity, not separate table
- **Strongly-typed ID conversions:** EF Core converts between Guid and strongly-typed IDs
- **Shadow properties:** OrderItem uses shadow property for database ID (not exposed in domain)
- **PostgreSQL choice:** Modern, open-source, production-ready database
- **Migration assembly:** Migrations live in Infrastructure project where DbContext is

**Dependencies:**
- Infrastructure → Application → Domain
- Infrastructure has EF Core packages, Domain/Application do not

**Verified:**
✅ Infrastructure project builds successfully
✅ Implements Application interfaces
✅ Database migration created successfully

**Next:** Implement API layer with minimal APIs and dependency injection.

---

### Step 5: API Layer Implementation

**Date:** 2026-03-24

**Objective:** Create HTTP endpoints and wire up dependency injection.

**Actions:**

1. Configured **Dependency Injection** in Program.cs:
   - DbContext with PostgreSQL connection string
   - Repository registrations (IOrderRepository → OrderRepository)
   - Command/Query handler registrations via extension method
   - Automatic database migration in development

2. Created **Minimal API Endpoints**:
   - POST /api/orders - Create new order
   - POST /api/orders/{id}/items - Add item to order
   - POST /api/orders/{id}/confirm - Confirm order
   - GET /api/orders/{id} - Get order by ID
   - GET /api/orders/customer/{customerId} - Get customer orders
   - All endpoints use proper HTTP verbs and status codes

3. Created **Request Models**:
   - CreateOrderRequest
   - AddOrderItemRequest
   - Separate from domain entities and DTOs

4. Created **Extension Methods**:
   - ServiceCollectionExtensions.AddApplicationHandlers() - Registers all handlers
   - OrderEndpoints.MapOrderEndpoints() - Groups order routes

5. Updated **Configuration**:
   - appsettings.json with database connection string
   - OpenAPI/Swagger configuration for API documentation

**Key Design Decisions:**

- **Composition root:** API layer is where all dependencies are wired together
- **Minimal APIs:** Modern .NET approach, less ceremony than controllers
- **Result pattern at API boundary:** Commands/queries return Result<T>, endpoints map to HTTP status codes
- **Endpoint grouping:** Related endpoints grouped with /api/orders prefix
- **Dependency injection:** Constructor injection for handlers in endpoints
- **Development convenience:** Auto-migration on startup in dev environment

**Dependencies:**
- Api → Infrastructure → Application → Domain (full dependency chain)
- Api references all layers to wire them together

**Verified:**
✅ API project builds successfully (with minor version conflict warnings)
✅ All layers properly wired through DI
✅ HTTP endpoints follow REST conventions

**Next:** Implement fit functions to validate onion architecture rules.
