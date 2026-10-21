# Product Catalog API

[![CI](https://github.com/Kamelof/ProductCatalog/actions/workflows/ci.yml/badge.svg)](https://github.com/Kamelof/ProductCatalog/actions/workflows/ci.yml)

**Quick start:** run `dotnet run --project src/ProductCatalog.Api` and open http://localhost:5202/swagger (mock data, no database needed).

REST API for an e-shop product catalog. It lists products (v1 returns everything, v2 is paginated), returns a single product by id and lets you update a product description.

Built with .NET 10, ASP.NET Core controllers, EF Core 10 and SQL Server. The API can also run on in-memory mock data, so SQL Server is optional.

## Contents

- [Prerequisites](#prerequisites)
- [Running the application](#running-the-application)
- [Configuration](#configuration)
- [API](#api)
- [Design decisions](#design-decisions)
- [Project structure](#project-structure)
- [Tests](#tests)
- [Database migrations](#database-migrations)
- [What I would add with more time](#what-i-would-add-with-more-time)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026, JetBrains Rider, or VS Code with the C# Dev Kit (the solution uses the `.slnx` format)
- Optional: [Docker Desktop](https://www.docker.com/products/docker-desktop/) to run the API with SQL Server
- Optional: SQL Server LocalDB (installed with Visual Studio) to run the API locally against SQL Server

## Running the application

### Option 1: mock data (no database needed)

This is the default. Products are kept in memory and reset when the application restarts.

```bash
dotnet run --project src/ProductCatalog.Api
```

Open http://localhost:5202/swagger.

From an IDE: open `ProductCatalog.slnx`, set `ProductCatalog.Api` as the startup project and run it. The browser opens Swagger automatically.

### Option 2: Docker Compose with SQL Server

Starts SQL Server 2022 and the API in containers. The database is created, migrated and seeded on startup.

1. Create a `.env` file from the template and set a password for the SQL Server `sa` account:

   ```bash
   cp .env.example .env
   ```

   ```
   MSSQL_SA_PASSWORD=YourStrong!Passw0rd
   ```

   SQL Server requires at least 8 characters and three of these four: uppercase letters, lowercase letters, digits and symbols. `.env` is git-ignored.

2. Start everything:

   ```bash
   docker compose up --build
   ```

3. Open http://localhost:8080/swagger.

Data is stored in the `sql-data` volume, so it survives restarts. To start again from the seed data, run `docker compose down -v`.

To check that the API really uses SQL Server, update a description, restart the API container and read the product again:

```bash
curl -X PATCH http://localhost:8080/api/v1/products/4/description \
  -H "Content-Type: application/json" \
  -d '{ "description": "Large AMOLED display and 5110 mAh battery." }'

docker compose restart api

curl http://localhost:8080/api/v1/products/4
```

The second request still returns the new description. With mock data it would be gone after the restart. I ran this check on Windows with Docker Desktop: on a fresh volume, the API applied the `InitialCreate` migration on startup, `GET /api/v2/products?page=3&pageSize=10` returned products 21–28 with `totalCount: 28` and `totalPages: 3`, and the updated description was still there after the restart, both through the API and in the `Products` table.

### Option 3: run locally against SQL Server

On Windows with LocalDB, the Development connection string already points to `(localdb)\MSSQLLocalDB`, so only the data source needs switching:

```bash
dotnet run --project src/ProductCatalog.Api -- --DataSource=Sql
```

To use the SQL Server container from Docker Compose instead, start only the database and pass the connection string through an environment variable (PowerShell):

```powershell
docker compose up -d sql
$env:DataSource = "Sql"
$env:ConnectionStrings__ProductCatalog = "Server=localhost,1433;Database=ProductCatalog;User Id=sa;Password=<your password>;TrustServerCertificate=true"
dotnet run --project src/ProductCatalog.Api
```

In Development, pending migrations are applied automatically when the application starts.

## Configuration

| Key | Values | Default |
|---|---|---|
| `DataSource` | `Mock` or `Sql` (case-insensitive) | `Mock` |
| `ConnectionStrings:ProductCatalog` | SQL Server connection string, required when `DataSource` is `Sql` | LocalDB in `appsettings.Development.json` |

Both can be set in `appsettings*.json`, through environment variables (`DataSource`, `ConnectionStrings__ProductCatalog`) or on the command line. An unknown `DataSource` value, or `Sql` without a connection string, stops the application at startup with a clear error message.

No secrets are stored in the repository. The only committed connection string is for LocalDB, which uses Windows authentication.

## API

Swagger UI is available at `/swagger` in the Development environment and has a separate document for each API version. Endpoint descriptions, parameters and response codes come from the XML comments on the controller.

| Method | URL | Description |
|---|---|---|
| `GET` | `/api/v1/products` | All products |
| `GET` | `/api/v2/products?page=1&pageSize=10` | One page of products |
| `GET` | `/api/v{version}/products/{id}` | A single product |
| `PATCH` | `/api/v{version}/products/{id}/description` | Update the description of a product |

`{version}` is `1` or `2`. Getting a product by id and updating its description work the same in both versions.

Product:

```json
{
  "id": 1,
  "name": "Apple iPhone 16 128GB Black",
  "imgUri": "https://example.com/img/1.jpg",
  "price": 22990.00,
  "description": "6.1\" Super Retina XDR display, A18 chip, 48 MP Fusion camera and Camera Control button."
}
```

Paged response (v2):

```json
{
  "items": [ ... ],
  "page": 2,
  "pageSize": 10,
  "totalCount": 28,
  "totalPages": 3
}
```

Updating a description:

```bash
curl -X PATCH http://localhost:5202/api/v1/products/4/description \
  -H "Content-Type: application/json" \
  -d '{ "description": "Large AMOLED display and 5110 mAh battery." }'
```

### Status codes

| Status | When |
|---|---|
| `200 OK` | Successful read or update |
| `400 Bad Request` | Invalid paging parameters, invalid description, or malformed JSON |
| `404 Not Found` | Unknown product id, non-numeric id, or unknown API version |
| `500 Internal Server Error` | Unexpected error, with a generic message only |

All errors use the [Problem Details](https://www.rfc-editor.org/rfc/rfc9457) format (`application/problem+json`). Validation errors list the problems for each field:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "pageSize": [ "Page size must be between 1 and 100." ]
  }
}
```

## Design decisions

### Versioning

- Versions are part of the URL (`/api/v1/...`, `/api/v2/...`) using `Asp.Versioning`. The version is visible in logs and links, and every version gets its own Swagger document.
- v2 is a complete API version, not just the paged list. It changes the list endpoint; the other endpoints behave the same as in v1.
- The Swagger documents are generated from the API versions found in the code, so a new version shows up without any Swagger changes.

### Pagination (v2)

- `page` starts at 1 and `pageSize` defaults to 10. The maximum page size is 100.
- Out-of-range values are rejected with `400`, not silently corrected. `page` must be 1 or more; `pageSize` must be between 1 and 100. Correcting values quietly would hide bugs in the client.
- A page past the end returns `200` with an empty `items` list and the real `totalCount`, so a client can stop reading pages without treating it as an error.
- Results are always ordered by `Id`. Without a fixed order, SQL Server can return rows in a different order for each query, and pages would overlap or skip items.
- Paging runs in the database (`OFFSET` / `FETCH`), not in memory.

### Updating the description

- The request body is a dedicated DTO, `{ "description": "..." }`. Other fields such as `name` or `price` are ignored, so the endpoint can't change anything else.
- A successful update returns `200` with the updated product, so the client doesn't need a second request.
- Validation rules:
  - required: `null`, an empty string or whitespace only is rejected (clearing the description isn't supported);
  - invisible-only text such as a zero-width space or a byte-order mark counts as empty;
  - at most 4000 characters;
  - control characters other than tab, CR and LF are rejected, and so are Unicode text-direction override characters, because they make text display differently from what's stored.
- HTML in a description is accepted and stored as plain text. Escaping it is the job of whatever displays it.
- Validation runs before the product lookup, so an invalid body for an unknown id returns `400`, not `404`.
- The domain entity enforces the same rules in `Product.UpdateDescription`, so no caller can bypass them.
- Only the description column is updated in the database (`UPDATE ... SET [Description] = ...`).

### Mock and SQL data sources

- `DataSource` decides at startup which `IProductRepository` is registered: `MockProductRepository` (in memory, thread-safe) or `ProductRepository` (EF Core).
- Both start from the same seed list (`SeedData.Products`). The mock repository loads it directly, and EF Core seeds it through `HasData` in the migration.
- The same test suite runs against both implementations.
- The mock repository returns copies, the same way EF Core behaves when it isn't tracking entities. A change is only kept after `UpdateAsync` is called.
- The seed data has 28 products (two full pages of 10 and a partial last page) and includes edge cases:
  - products without a description;
  - a price of `0` and a price in the millions;
  - a name close to the 200-character limit;
  - Czech characters.

### Errors and cross-cutting concerns

- Services return a small `Result` type for expected failures (validation, not found) instead of throwing exceptions. The controller turns these into ProblemDetails.
- Unexpected exceptions are logged and turned into a generic `500` response, without exception details or a stack trace, by `GlobalExceptionHandler`.
- API responses use DTOs with a small manual mapper, not entities and not AutoMapper.
- A `CancellationToken` is passed through from the controller down to the EF Core queries.
- A malformed id (`/products/abc`) never reaches the controller because of the `{id:int}` route constraint, and returns `404`.

## Project structure

```
ProductCatalog/
├── src/
│   ├── ProductCatalog.Domain          Product entity and its rules
│   ├── ProductCatalog.Application     Service, DTOs, mapping, validation, repository interface
│   ├── ProductCatalog.Infrastructure  EF Core DbContext, migrations, repositories, seed data
│   └── ProductCatalog.Api             Controllers, versioning, Swagger, error handling
├── tests/
│   └── ProductCatalog.Tests           Unit, repository and integration tests
├── Dockerfile
└── docker-compose.yml
```

Dependencies point inwards: `Api` → `Application` ← `Infrastructure`, and `Application` → `Domain`. The application layer only knows the `IProductRepository` interface, so the data source can be swapped without touching business logic. I kept the layering light on purpose: full DDD or CQRS would be too much for four endpoints.

## Tests

```bash
dotnet test
```

The tests use xUnit, NSubstitute for mocks and Shouldly for assertions. Test names follow `Method_Scenario_ExpectedResult`, so the list of tests reads like a specification.

| Area | What is covered |
|---|---|
| `Application` | `ProductService` with a mocked repository: paging slices, partial and out-of-range pages, total pages, invalid paging, not found, updates that change only the description, validation stopping the save. Also `ProductValidator` boundaries and forbidden characters. |
| `Domain` | `Product.UpdateDescription` rules |
| `Infrastructure` | One abstract test class run against both repositories: the mock repository and the EF Core repository on SQLite in memory. Also data source configuration parsing. |
| `Api` | Controller unit tests: each action is called directly with a mocked `IProductService` and checked for `200`, `400` and `404`. Integration tests with `WebApplicationFactory` cover every endpoint and error case over HTTP, ProblemDetails responses, a generic 500 without leaking exception details, and Swagger loading for v1 and v2 with descriptions (and hidden outside Development). |

The EF Core repository tests use SQLite in memory to keep the tests fast and independent of Docker. SQLite doesn't enforce column lengths, so the database schema itself is checked by the migration rather than by these tests.

GitHub Actions (`.github/workflows/ci.yml`) builds the solution and runs the tests on every push and pull request to `main`, then builds the Docker image. Test results are uploaded as a build artifact.

## Database migrations

The `dotnet-ef` tool is pinned in `dotnet-tools.json`:

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/ProductCatalog.Infrastructure --startup-project src/ProductCatalog.Api -- --DataSource=Sql
```

The `--DataSource=Sql` argument is needed because the API defaults to mock data, which doesn't register the `DbContext`.

## What I would add with more time

- **Authentication and authorization** for the `PATCH` endpoint, for example JWT bearer tokens with a policy for catalog editors. At the moment anyone who can reach the API can change descriptions.
- **Optimistic concurrency** with a `RowVersion` column, returning `409 Conflict` when two people edit the same product at once. Today the last write wins.
- **Production setup:**
  - running migrations as a separate deployment step instead of on startup;
  - a dedicated SQL login instead of `sa`, and a trusted TLS certificate instead of `TrustServerCertificate=true`;
  - HTTPS handled by a reverse proxy.
  - The Docker Compose setup runs in Development on purpose, so Swagger and automatic migrations work out of the box.
- **Integration tests against real SQL Server** with Testcontainers, to cover SQL Server-specific behaviour that SQLite doesn't have, such as column length limits.
- **Health checks** (`/health`), including a database check.
- **Rate limiting** and **response caching or ETags** for the read endpoints. v1 returns the whole catalog in one response, which won't scale for a large catalog, so it could be marked as deprecated in favour of v2.
- **Structured logging and tracing** with OpenTelemetry.
