# Expense Analyzer API

A ASP.NET Core Web API for importing, validating, storing, and querying personal finance transactions from CSV files.

This project was built using layered architecture, JWT authentication, ownership enforcement, CSV import with partial success, PostgreSQL persistence, automated testing, Dockerized local setup, CI/CD, and cloud deployment.

## Live Demo

- **API Base URL:** `https://expense-analyzer-api.onrender.com`
- **Swagger UI:** `https://expense-analyzer-api.onrender.com/swagger`
- **Health Check:** `https://expense-analyzer-api.onrender.com/healthz`

## Overview

Expense Analyzer API allows authenticated users to:

- register and log in with JWT authentication
- import transaction data from CSV files
- validate rows individually without failing the entire import
- store import jobs and imported transactions
- list, filter, sort, paginate, and summarize transactions
- review import history and import details
- access only their own data

## Main Features

### Authentication

- User registration
- User login
- JWT token generation
- Protected endpoints with `[Authorize]`
- `/api/auth/me` endpoint to retrieve the authenticated user

### CSV Import

- Multipart file upload
- CSV header validation
- Row-by-row validation
- Partial import behavior:
  - valid rows are saved
  - invalid rows are skipped
  - one bad row does not fail the whole import
- Import job persistence with status and counters
- Detailed error reporting per skipped row

### Transactions

- List transactions for the authenticated user
- Ownership enforcement per user
- Filtering
- Sorting
- Pagination
- Detail by transaction ID
- Summary endpoint for aggregated values

### Import History

- Import history list
- Import detail by ID
- Access restricted to the owner of the import job

### API Quality

- Global exception handling middleware
- Consistent error responses
- Swagger / OpenAPI documentation
- Health check endpoint for deployment monitoring

## Tech Stack

- **Backend:** ASP.NET Core Web API
- **Language:** C#
- **Framework:** .NET 10
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer
- **CSV Parsing:** CsvHelper
- **Unit Testing:** xUnit, Moq
- **Integration Testing:** xUnit, Microsoft.AspNetCore.Mvc.Testing
- **Containerization:** Docker, Docker Compose
- **CI:** GitHub Actions
- **CD / Hosting:** Render Auto-Deploy
- **Cloud Database:** Neon PostgreSQL
- **Hosting Platform:** Render

## Architecture

The solution follows a layered architecture:

- **ExpenseAnalyzer.Api**
  - controllers
  - middleware
  - OpenAPI configuration
  - dependency injection and app startup

- **ExpenseAnalyzer.Application**
  - services
  - DTOs
  - interfaces
  - business rules
  - validation logic

- **ExpenseAnalyzer.Domain**
  - entities
  - enums
  - shared abstractions

- **ExpenseAnalyzer.Infrastructure**
  - DbContext
  - repositories
  - persistence
  - infrastructure services

- **ExpenseAnalyzer.UnitTests**
  - isolated business-logic tests

- **ExpenseAnalyzer.IntegrationTests**
  - full API pipeline tests with a real test database

## Project Structure

```text
src/
  ExpenseAnalyzer.Api/
  ExpenseAnalyzer.Application/
  ExpenseAnalyzer.Domain/
  ExpenseAnalyzer.Infrastructure/

tests/
  ExpenseAnalyzer.UnitTests/
  ExpenseAnalyzer.IntegrationTests/
```

## Core Business Rules

### Ownership Enforcement

All data access is scoped to the authenticated user.

Knowing an ID alone is not enough to access a transaction or import job belonging to another user.

### Partial CSV Import

The import process does not abort on the first invalid row.

Instead:

- valid rows are imported
- invalid rows are skipped
- the response returns imported count, skipped count, and row-level errors

### Pagination Behavior

When filters return no results:

- `items = []`
- `totalCount = 0`
- a descriptive message is returned

When a page outside the valid range is requested:

- the API still returns `200 OK`
- `items = []`
- `IsPageOutOfRange = true`
- a descriptive message explains the situation

This keeps the API predictable while still providing useful metadata.

## CSV Format

The expected CSV header is:

```csv
Date,Description,Amount
```

Example:

```csv
Date,Description,Amount
2026-03-01,Coffee,-4.50
2026-03-02,Salary,1200.00
2026-03-03,Groceries,-35.20
```

### Validation Rules

- the file must be `.csv`
- the header must match exactly
- date must be valid
- description is required
- amount must be a valid decimal number

## Main Endpoints

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Imports

- `POST /api/imports`
- `GET /api/imports`
- `GET /api/imports/{id}`

### Transactions

- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `GET /api/transactions/summary`

### Monitoring

- `GET /healthz`

## Transaction Query Features

`GET /api/transactions` supports:

- `pageNumber`
- `pageSize`
- `sortBy`
- `sortDirection`
- transaction filters implemented in the project

### Supported sorting

- `date`
- `amount`

### Supported sort directions

- `asc`
- `desc`

Defaults:

- `sortBy = date`
- `sortDirection = desc`

## Error Handling

The API uses a global exception middleware to return consistent error responses.

Typical error scenarios include:

- invalid credentials
- unauthorized access
- invalid filters or paging values
- invalid CSV files
- missing resources
- unexpected server errors

This keeps controllers cleaner and centralizes response formatting.

## Automated Testing

The project includes both unit tests and integration tests.

### Unit Tests

Focused on service-level business logic, especially around transaction querying behavior.

Examples covered:

- invalid page number
- invalid sorting values
- out-of-range pages
- empty results messaging
- unauthorized access
- summary calculations
- transaction detail not found

### Integration Tests

Focused on real API behavior using the full pipeline.

Covered areas:

- auth flow
- protected endpoints
- transaction listing and ownership
- pagination, sorting, and filtering
- transaction detail not found scenarios
- valid CSV import
- mixed CSV import with partial success

### Test Projects

- `tests/ExpenseAnalyzer.UnitTests`
- `tests/ExpenseAnalyzer.IntegrationTests`

## Running Locally

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Git

### 1. Clone the repository

```bash
git clone <https://github.com/DelroyCR/expense-analyzer-api>
cd expense-analyzer-api
```

### 2. Configure settings

Set your PostgreSQL connection string and JWT settings in `appsettings.Development.json` or environment variables.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=expense_analyzer;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Key": "your-local-development-secret-key",
    "Issuer": "ExpenseAnalyzerApi",
    "Audience": "ExpenseAnalyzerApiUsers",
    "ExpirationMinutes": 60
  }
}
```

### 3. Apply migrations

```bash
dotnet ef database update --project src/ExpenseAnalyzer.Infrastructure --startup-project src/ExpenseAnalyzer.Api
```

### 4. Run the API

```bash
dotnet run --project src/ExpenseAnalyzer.Api
```

### 5. Open Swagger

Use the local URL shown in the terminal output, then open `/swagger`.

Example:

```text
http://localhost:5000/swagger
```

## Running with Docker

This project includes Docker support for local development.

### Start services

```bash
docker compose up --build -d
```

### Open Swagger

```text
http://localhost:8080/swagger
```

### Stop services

```bash
docker compose down
```

### Remove containers and volumes

```bash
docker compose down -v
```

## Environment Variables

Common environment variables used by the project:

```text
ASPNETCORE_ENVIRONMENT
ASPNETCORE_HTTP_PORTS
ConnectionStrings__DefaultConnection
JwtSettings__Key
JwtSettings__Issuer
JwtSettings__Audience
JwtSettings__ExpirationMinutes
ApplyMigrationsOnStartup
```

### Production Notes

In the deployed environment:

- the API runs on Render
- PostgreSQL is hosted on Neon
- migrations are applied on startup
- Swagger is enabled for demo and review purposes
- forwarded headers are configured so the app works correctly behind the Render proxy

## CI/CD

The project includes a complete CI/CD flow.

### Continuous Integration

GitHub Actions runs automatically on push and pull request:

- restore
- build
- unit tests
- test database migrations
- integration tests

### Continuous Deployment

After CI checks pass:

- Render deploys the latest version automatically
- the API connects to Neon PostgreSQL
- `/healthz` is used for service health checks

This setup provides a complete delivery workflow for a portfolio backend project.

## Deployment

### Hosted API

- **Platform:** Render
- **Database:** Neon PostgreSQL

### Deployment Notes

- Swagger is available in production to simplify reviewer testing
- health checks are enabled through `/healthz`
- forwarded headers are configured to work correctly behind the hosting proxy
- the deployed app uses environment variables for secrets and connection strings

## Design Decisions

### Why layered architecture?

To separate responsibilities clearly between API, business logic, domain, and persistence.

### Why partial import instead of full failure?

Because real-world imports often contain mixed-quality data. Importing valid rows while reporting invalid ones is more useful than rejecting everything.

### Why ownership checks at query level?

To ensure users cannot access records that belong to someone else, even if they know an ID.

### Why both unit and integration tests?

Because unit tests validate isolated business rules, while integration tests validate the real HTTP pipeline, routing, auth, middleware, persistence, and serialization.

### Why Docker + CI/CD + deployment in this project?

Because they extend the API into a more complete, professional, portfolio-ready backend application instead of leaving it as a local-only prototype.

## Project Status

**Status:** V1 complete and deployed

This version includes:

- authentication with JWT
- CSV import with partial success behavior
- transaction querying with filtering, sorting, pagination, and summary
- import history and detail
- automated unit tests
- automated integration tests
- Docker support
- CI/CD pipeline
- public deployment

## Reviewer Notes

If you are reviewing this repository for technical evaluation, the best starting points are:

1. Swagger UI
2. the auth flow
3. CSV import
4. transaction listing with filters and pagination
5. the test projects
6. the GitHub Actions workflow