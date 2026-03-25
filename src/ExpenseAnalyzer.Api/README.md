\# Expense Analyzer API



Expense Analyzer API is a backend project built with ASP.NET Core, Entity Framework Core, and PostgreSQL.

It allows users to register, authenticate with JWT, import transactions from CSV files, and query their transaction history with filtering, pagination, sorting, and summary statistics.



\---



\## Features



\### Implemented in V1



\- Layered architecture

\- Swagger / OpenAPI

\- Health endpoint

\- PostgreSQL integration with EF Core

\- Database migrations

\- User registration

\- Login with JWT authentication

\- Protected endpoints with \[Authorize]

\- Authenticated user endpoint (/api/auth/me)

\- CSV transaction import

\- Row-by-row CSV validation

\- Partial import support:

&#x20; - valid rows are imported

&#x20; - invalid rows are skipped and reported

\- Import job tracking

\- Transaction listing

\- Transaction detail endpoint

\- Transaction summary endpoint

\- Filtering by:

&#x20; - date range

&#x20; - minimum / maximum amount

&#x20; - description text

&#x20; - import job

\- Pagination

\- Sorting

\- Global exception handling middleware

\- Unit tests for TransactionService

\- Integration tests for:

&#x20; - auth endpoints

&#x20; - transaction endpoints

&#x20; - import endpoints



\---



\## Tech Stack



\- .NET 10

\- ASP.NET Core Web API

\- Entity Framework Core

\- PostgreSQL

\- JWT Authentication

\- CsvHelper

\- xUnit

\- Moq



\---



\## Solution Structure



src/

&#x20; ExpenseAnalyzer.Api

&#x20; ExpenseAnalyzer.Application

&#x20; ExpenseAnalyzer.Domain

&#x20; ExpenseAnalyzer.Infrastructure



tests/

&#x20; ExpenseAnalyzer.UnitTests

&#x20; ExpenseAnalyzer.IntegrationTests



\### Project responsibilities



\- ExpenseAnalyzer.Api

&#x20; - controllers

&#x20; - HTTP contracts

&#x20; - Swagger

&#x20; - middleware

&#x20; - dependency injection setup



\- ExpenseAnalyzer.Application

&#x20; - DTOs

&#x20; - service interfaces

&#x20; - business logic

&#x20; - validation rules

&#x20; - application exceptions



\- ExpenseAnalyzer.Domain

&#x20; - entities

&#x20; - core domain models



\- ExpenseAnalyzer.Infrastructure

&#x20; - EF Core

&#x20; - database context

&#x20; - repositories

&#x20; - persistence implementations



\- ExpenseAnalyzer.UnitTests

&#x20; - service-level automated tests



\- ExpenseAnalyzer.IntegrationTests

&#x20; - end-to-end API behavior tests



\---



\## Main Business Flow



\### 1. Authentication

A user can:

\- register

\- log in

\- receive a JWT token

\- access protected endpoints

\- fetch their own profile through /api/auth/me



\### 2. CSV Import

An authenticated user can upload a CSV file with transactions.



The import process:

\- validates file format

\- validates CSV headers

\- validates each row independently

\- imports valid rows

\- skips invalid rows

\- stores an import job

\- stores imported transactions

\- returns a result summary with imported and skipped rows



\### 3. Transactions

An authenticated user can:

\- list their own transactions

\- retrieve transaction details by id

\- query summary statistics

\- filter by multiple criteria

\- paginate results

\- sort results



All transaction queries are scoped to the authenticated user.



\---



\## Example CSV Format



Date,Description,Amount

2026-01-10,Amazon Purchase,25.50

2026-01-11,Uber Ride,10.00



\---



\## API Endpoints



\### Health

\- GET /health



\### Auth

\- POST /api/auth/register

\- POST /api/auth/login

\- GET /api/auth/me

\- GET /api/auth/{id}



\### Imports

\- POST /api/imports

\- GET /api/imports

\- GET /api/imports/{id}



\### Transactions

\- GET /api/transactions

\- GET /api/transactions/{id}

\- GET /api/transactions/summary



\---



\## Transaction Query Features



GET /api/transactions supports:



\### Filters

\- from

\- to

\- minAmount

\- maxAmount

\- description

\- importJobId



\### Pagination

\- pageNumber

\- pageSize



\### Sorting

\- sortBy

&#x20; - date

&#x20; - amount

\- sortDirection

&#x20; - asc

&#x20; - desc



\### Example

GET /api/transactions?description=week\&pageNumber=1\&pageSize=20\&sortBy=amount\&sortDirection=desc



\---



\## Transaction Response Behavior



The transactions endpoint returns a paged result object with:

\- items

\- total count

\- current page

\- page size

\- total pages

\- page range information



If no records match the filters, the API returns an empty list plus a descriptive message.



If the requested page is outside the available range, the API returns:

\- items: \[]

\- isPageOutOfRange: true

\- a descriptive message



\---



\## Error Handling



The API uses global exception handling middleware to return consistent error responses.



Examples of handled scenarios:

\- invalid filters

\- invalid pagination

\- invalid sorting

\- unauthorized access

\- missing resources

\- CSV validation failures

\- business rule violations



\---



\## Testing



\### Unit Tests

Unit tests cover core TransactionService logic, including:

\- pagination validation

\- sorting validation

\- out-of-range page handling

\- no-results behavior

\- unauthorized access handling

\- transaction summary edge cases

\- transaction detail not found behavior



\### Integration Tests

Integration tests cover:

\- register / login / /me

\- protected transaction access

\- pagination contract

\- filtering + sorting + paging

\- transaction ownership

\- valid CSV imports

\- mixed valid/invalid CSV imports



Run all tests with:



dotnet test



\---



\## How to Run the Project



\### 1. Clone the repository



git clone <YOUR\_REPOSITORY\_URL>

cd expense-analyzer-api



\### 2. Configure PostgreSQL



Create a PostgreSQL database, for example:



CREATE DATABASE expense\_analyzer;



Update your connection string in appsettings.json (or user secrets / environment variables).



Example:



"ConnectionStrings": {

&#x20; "DefaultConnection": "Host=localhost;Port=5432;Database=expense\_analyzer;Username=postgres;Password=YOUR\_PASSWORD"

}



\### 3. Configure JWT settings



Set your JWT configuration in appsettings.json:



"JwtSettings": {

&#x20; "Key": "YOUR\_SUPER\_SECRET\_KEY\_HERE",

&#x20; "Issuer": "ExpenseAnalyzer",

&#x20; "Audience": "ExpenseAnalyzerUsers",

&#x20; "ExpirationMinutes": 60

}



\### 4. Apply migrations



dotnet ef database update --project ./src/ExpenseAnalyzer.Infrastructure --startup-project ./src/ExpenseAnalyzer.Api



\### 5. Run the API



dotnet run --project ./src/ExpenseAnalyzer.Api



\### 6. Open Swagger



Open the Swagger UI in your browser after starting the API.



Typical local URL:



https://localhost:xxxx/swagger



\---



\## Development Notes



This project intentionally avoids unnecessary complexity.



\### Deliberately not used in V1

\- microservices

\- full CQRS

\- MediatR

\- AutoMapper

\- frontend UI

\- OCR / PDF processing

\- AI-based categorization



The goal of V1 is to demonstrate strong backend fundamentals with a realistic scope.



\---



\## What This Project Demonstrates



This project demonstrates the ability to:



\- structure a professional .NET solution

\- build REST APIs with ASP.NET Core

\- use PostgreSQL with EF Core

\- manage migrations

\- implement JWT authentication

\- process uploaded files

\- validate and transform imported data

\- apply business rules cleanly

\- return consistent API responses

\- write automated tests

\- work with Git/GitHub in a real project structure



\---



\## Possible Future Improvements



Planned or possible next steps:



\- accounts

\- transaction categorization

\- richer financial reports

\- duplicate detection

\- Docker support

\- CI/CD pipeline

\- deployment setup



\---



\## Status



Current status: V1 complete / portfolio-ready baseline, with authentication, CSV imports, transaction querying, validation, and automated tests already implemented.



