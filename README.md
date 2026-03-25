# Proving Architecture Patterns

A comprehensive course demonstrating software architecture patterns through working implementations, automated validation, and practical examples.

## Course Philosophy

Each pattern includes:

1. **Complete Implementation** - Working code, not just theory
2. **Fit Functions** - Automated tests validating architectural rules
3. **Violation Examples** - Deliberately broken code showing what fit functions catch
4. **Docker Setup** - Run everything locally with `docker-compose up`
5. **9-Section Documentation** - From overview to practical takeaways
6. **Implementation Log** - Step-by-step build documentation

## Patterns

### 1. Onion Architecture

**Status:** ✅ Complete

**Location:** `patterns/onion-architecture/`

**What it demonstrates:**
- Dependency Inversion Principle
- Rich Domain Models
- Infrastructure Independence
- Layer Isolation
- 18 automated fit functions

**Quick Start:**
```bash
cd patterns/onion-architecture
docker-compose up
```

**Documentation:** See `patterns/onion-architecture/docs/` for complete 9-section course material.

## Documentation Template

Each pattern follows this structure:

1. **Overview** - What it is, when to use it
2. **The Problem** - What goes wrong without it
3. **Component Parts** - Layers, responsibilities, dependencies
4. **Build the Pattern** - Step-by-step implementation
5. **Fit Functions** - Architectural assumptions to protect
6. **Implement Fit Functions** - Automated validation tests
7. **Break It Deliberately** - Violation examples
8. **Trade-offs** - Benefits, drawbacks, when to skip
9. **Summary** - Key lessons, practical takeaways

## Repository Structure

```
ProvingArchitecturePatterns/
├── ProvingArchitecturePatterns.slnx  # Top-level solution (all patterns)
├── patterns/
│   └── onion-architecture/     # First pattern (complete)
│       ├── src/                # Source code (Domain, Application, Infrastructure, API)
│       ├── tests/              # Unit tests per layer
│       ├── fit-functions/      # Architecture validation tests
│       ├── violations/         # Deliberately broken examples
│       ├── docs/               # 9-section course documentation
│       ├── docker-compose.yml  # Local development environment
│       ├── IMPLEMENTATION.md   # Step-by-step build log
│       └── README.md           # Quick start guide
└── README.md                   # This file
```

## Working with the Solution

**Open entire course in IDE:**
```bash
# Open top-level solution containing all patterns
dotnet build ProvingArchitecturePatterns.slnx
```

**Build all patterns:**
```bash
dotnet build ProvingArchitecturePatterns.slnx
```

**Run all tests across all patterns:**
```bash
dotnet test ProvingArchitecturePatterns.slnx
```

## Tech Stack

- **.NET 10** - Latest .NET with C# 14
- **PostgreSQL** - Production database
- **EF Core** - ORM with migrations
- **Docker** - Containerization
- **NetArchTest.Rules** - Architecture testing
- **xUnit** - Test framework

## Future Patterns

Planned patterns to demonstrate:

- Clean Architecture
- Hexagonal Architecture (Ports & Adapters)
- Vertical Slice Architecture
- CQRS with Event Sourcing
- Modular Monolith
- Microservices Architecture

## Contributing

Each pattern implementation follows strict principles:

- **TDD workflow** - Tests first, always
- **Fit functions** - Automated architecture validation
- **Working examples** - Must run with `docker-compose up`
- **Complete documentation** - All 9 sections
- **Implementation log** - Document every step

## License

MIT License - Educational use encouraged.
