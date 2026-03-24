# Onion Architecture Pattern

A complete implementation of the Onion Architecture pattern with fit functions to validate architectural rules.

## Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 10 SDK (for local development)

### Run with Docker

```bash
# Start all services (PostgreSQL + API + Adminer)
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### Access Points

- **API**: http://localhost:5000/api/orders
- **OpenAPI**: http://localhost:5000/openapi/v1.json (in development)
- **Adminer** (Database UI): http://localhost:8080
  - System: PostgreSQL
  - Server: postgres
  - Username: demo
  - Password: demo123
  - Database: onionarch

## Local Development

### Build and Test

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run unit tests
dotnet test tests/

# Run fit function tests (architecture validation)
dotnet test fit-functions/OnionArch.FitFunctions/

# Run API locally
dotnet run --project src/OnionArch.Api/
```

### Database Migrations

```bash
# Add new migration
cd src/OnionArch.Infrastructure
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Or let the API apply them automatically (development only)
```

## Architecture Overview

This implementation follows the Onion Architecture pattern with strict dependency rules:

```
┌─────────────────────────────────────┐
│         API Layer (Outer)           │  ← HTTP endpoints, DI composition
│  ┌───────────────────────────────┐  │
│  │   Infrastructure Layer        │  │  ← EF Core, PostgreSQL, repositories
│  │  ┌─────────────────────────┐  │  │
│  │  │  Application Layer      │  │  │  ← Use cases, interfaces
│  │  │  ┌───────────────────┐  │  │  │
│  │  │  │  Domain Layer     │  │  │  │  ← Entities, value objects, rules
│  │  │  │   (Pure Core)     │  │  │  │     Zero dependencies
│  │  │  └───────────────────┘  │  │  │
│  │  └─────────────────────────┘  │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘

Dependencies flow inward only: API → Infra → App → Domain
```

## Project Structure

```
patterns/onion-architecture/
├── src/
│   ├── OnionArch.Domain/         # Core business logic
│   ├── OnionArch.Application/    # Use cases, interfaces
│   ├── OnionArch.Infrastructure/ # Data access, EF Core
│   └── OnionArch.Api/            # HTTP endpoints
├── tests/                         # Unit & integration tests
├── fit-functions/                 # Architecture validation tests
│   └── OnionArch.FitFunctions/   # 18 architectural rules
├── violations/                    # Example violations (educational)
├── docs/                          # Course documentation
├── docker-compose.yml            # Container orchestration
└── Dockerfile                    # API container definition
```

## Fit Functions (Architectural Tests)

Fit functions validate that the architecture rules are followed:

```bash
# Run all fit function tests
dotnet test fit-functions/OnionArch.FitFunctions/

# All 18 tests should pass:
# ✅ Dependency rules (6 tests)
# ✅ Layer isolation (4 tests)
# ✅ Domain purity (4 tests)
# ✅ Interface ownership (4 tests)
```

## API Endpoints

### Create Order
```http
POST http://localhost:5000/api/orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "currency": "USD"
}
```

### Add Order Item
```http
POST http://localhost:5000/api/orders/{orderId}/items
Content-Type: application/json

{
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "quantity": 2,
  "unitPrice": 29.99
}
```

### Confirm Order
```http
POST http://localhost:5000/api/orders/{orderId}/confirm
```

### Get Order
```http
GET http://localhost:5000/api/orders/{orderId}
```

### Get Customer Orders
```http
GET http://localhost:5000/api/orders/customer/{customerId}
```

## Key Features

- ✅ **Strict layer separation** with dependency inversion
- ✅ **Rich domain model** (not anemic)
- ✅ **Framework-agnostic core** (Domain has zero dependencies)
- ✅ **Automated architecture validation** (18 fit function tests)
- ✅ **PostgreSQL** for persistence
- ✅ **Docker containerization** for easy local development
- ✅ **Comprehensive documentation** and violation examples

## Learning Resources

- `IMPLEMENTATION.md` - Step-by-step build log
- `docs/` - Course documentation (9 sections)
- `violations/` - Examples of what NOT to do

## Next Steps

1. Review the implementation in `src/`
2. Run fit function tests to see validation
3. Try the violation examples to see what breaks
4. Explore the course documentation in `docs/`
