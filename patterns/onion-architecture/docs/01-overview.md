# Overview

## What is Onion Architecture?

Onion Architecture is a software architecture pattern that emphasizes **dependency inversion** and **separation of concerns** through concentric layers. Like an onion, the system is built in layers, with the core domain at the center and infrastructure concerns at the outer edges.

The fundamental rule: **dependencies flow inward only**.

```
┌─────────────────────────────────────┐
│         API Layer                   │  ← HTTP Endpoints, Controllers
│  ┌───────────────────────────────┐  │
│  │   Infrastructure Layer        │  │  ← Databases, File Systems, APIs
│  │  ┌─────────────────────────┐  │  │
│  │  │  Application Layer      │  │  │  ← Use Cases, Orchestration
│  │  │  ┌───────────────────┐  │  │  │
│  │  │  │  Domain Layer     │  │  │  │  ← Business Rules, Entities
│  │  │  │   (Pure Core)     │  │  │  │     Value Objects, Aggregates
│  │  │  └───────────────────┘  │  │  │
│  │  └─────────────────────────┘  │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

## Why It Exists

Onion Architecture solves several critical problems in software design:

1. **Dependency on Infrastructure**: Traditional layered architectures make the domain depend on infrastructure (databases, frameworks), making the core business logic hard to test and maintain.

2. **Framework Coupling**: Business logic becomes tightly coupled to specific frameworks (Entity Framework, ASP.NET), making it difficult to change technologies.

3. **Testing Complexity**: When business logic depends on databases and external services, testing becomes slow and brittle.

4. **Domain Model Anemia**: Business logic leaks into services instead of living in rich domain entities.

5. **Inflexible Architecture**: Changes to infrastructure force changes to business logic.

## When to Use It

### ✅ Use Onion Architecture When:

- **Complex business logic** that needs to be framework-independent
- **Long-lived applications** that will outlive current framework versions
- **Domain-driven design** where business rules are central
- **Multiple interfaces** (Web API, CLI, Message Queue) to the same domain
- **High testability** requirements for business rules
- **Team experience** with dependency injection and SOLID principles

### ❌ Consider Simpler Patterns When:

- **CRUD applications** with minimal business logic
- **Small projects** with short lifespan
- **Data-centric** applications where the database schema drives everything
- **Tight deadlines** where team learning curve is a concern
- **Simple requirements** that won't change significantly

## Core Principles

1. **Dependency Inversion**: All dependencies point inward. Outer layers depend on inner layers, never the reverse.

2. **Domain Independence**: The domain layer has zero dependencies on infrastructure, frameworks, or external concerns.

3. **Interface Ownership**: Interfaces are owned by the consumer (Application layer), not the provider (Infrastructure layer).

4. **Rich Domain Model**: Business logic lives in domain entities, not in services.

5. **Testability**: Core business logic can be tested without databases, web servers, or external services.

## Key Benefits

- **Framework Independence**: Swap out EF Core for Dapper without touching domain code
- **Database Independence**: Switch from PostgreSQL to SQL Server with minimal impact
- **UI Independence**: Add gRPC endpoints alongside REST without changing business logic
- **Testable**: Domain and application logic tested in isolation
- **Maintainable**: Clear boundaries make it obvious where code belongs
- **Flexible**: Easy to add new features without breaking existing code

## Comparison with Traditional Layered Architecture

### Traditional (N-Tier):
```
UI → Business Logic → Data Access → Database
     ↓ depends on ↓
```
Problem: Business logic depends on data access layer!

### Onion Architecture:
```
UI → Infrastructure → Application → Domain
     Infrastructure implements interfaces from Application
     ↑ outer depends on inner ↑
```
Solution: Dependency inversion - infrastructure implements interfaces defined by business needs!
