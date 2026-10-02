# QuickCart Technology Stack Specification

This document details the verified technology stack, runtimes, frameworks, libraries, and tooling across the QuickCart backend repository.

---

## 1. Core Platform & Runtime

| Component | Specification | Verified Configuration / Source |
| :--- | :--- | :--- |
| **.NET SDK** | `10.0.401` | Configured in root [`global.json`](file:///d:/Asp.net%20core_Project/Quick-commerce/global.json) (`"rollForward": "latestFeature"`) |
| **Target Framework** | `.NET 10.0` (`net10.0`) | Configured in root [`Directory.Build.props`](file:///d:/Asp.net%20core_Project/Quick-commerce/Directory.Build.props) |
| **C# Language** | C# 14.0 (`LangVersion: 14.0`) | Enforced globally via [`Directory.Build.props`](file:///d:/Asp.net%20core_Project/Quick-commerce/Directory.Build.props) |
| **Nullability** | Enabled (`<Nullable>enable</Nullable>`) | Enforced globally via [`Directory.Build.props`](file:///d:/Asp.net%20core_Project/Quick-commerce/Directory.Build.props) |
| **Implicit Usings** | Enabled (`<ImplicitUsings>enable</ImplicitUsings>`) | Enforced globally via [`Directory.Build.props`](file:///d:/Asp.net%20core_Project/Quick-commerce/Directory.Build.props) |
| **Deterministic Builds**| Enabled (`<Deterministic>true</Deterministic>`) | Enforced globally via [`Directory.Build.props`](file:///d:/Asp.net%20core_Project/Quick-commerce/Directory.Build.props) |

---

## 2. Backend Web & Application Framework

| Component | Version / Package | Description & Usage |
| :--- | :--- | :--- |
| **Host Application** | ASP.NET Core 10 Web API | Web application host in `src/QuickCart.Api/` |
| **Controller Architecture** | ASP.NET Core Controllers | Standard MVC controllers inheriting `ControllerBase` |
| **Real-Time Communication** | ASP.NET Core SignalR | High-frequency telemetry via WebSockets/Long Polling (`/hubs/delivery-tracking`) |
| **Interactive API Documentation**| `Scalar.AspNetCore` (`2.17.3`) | Modern interactive OpenAPI documentation UI accessible at `/scalar/v1` in Development |
| **OpenAPI Specification** | `Microsoft.AspNetCore.OpenApi` (`10.0.12`) | Generates OpenAPI v3 specification documents via `builder.Services.AddOpenApi()` |
| **Error Handling Protocol** | RFC 7807 Problem Details | Global exception handler mapping domain exceptions to standard RFC 7807 JSON envelopes |

---

## 3. Persistence & Database

| Component | Version / Package | Description & Usage |
| :--- | :--- | :--- |
| **Database Engine** | PostgreSQL 16+ | Multi-schema relational database storage (`quickcartdb`) |
| **ORM** | EF Core 10 (`Microsoft.EntityFrameworkCore` `10.0.4`) | High-performance object-relational mapping |
| **EF Core Design Tools** | `Microsoft.EntityFrameworkCore.Design` (`10.0.4`) | EF Core CLI migration scaffolding tools (`QuickCart.Api`) |
| **PostgreSQL Provider** | `Npgsql.EntityFrameworkCore.PostgreSQL` (`10.0.3`) | Native Npgsql PostgreSQL provider for EF Core 10 |
| **Interceptors** | `AuditableEntitySaveChangesInterceptor` | Shared interceptor auto-populating UTC timestamps (`CreatedAt`, `UpdatedAt`) |
| **Migration Runner** | `tools/QuickCart.DatabaseMigrator` | Dedicated console tool automating schema-level database migrations |

---

## 4. Authentication, Authorization & Cryptography

| Component | Version / Package | Description & Usage |
| :--- | :--- | :--- |
| **JWT Authentication** | `Microsoft.AspNetCore.Authentication.JwtBearer` (`10.0.12`) | JWT Bearer middleware validating signature, issuer, audience, and expiration |
| **JWT Token Generation** | `System.IdentityModel.Tokens.Jwt` (`8.22.0`) | Creates HMAC-SHA256 signed access tokens with custom claims |
| **Password Hashing** | `BCrypt.Net-Next` (`4.2.0`) | Salted password hashing via `EnhancedHashPassword` and `EnhancedVerify` |
| **Google Sign-In** | `Google.Apis.Auth` (`1.76.0`) | Validates Google OpenID Connect ID tokens with clock tolerance and audience verification |
| **CSPRNG Generators** | `System.Security.Cryptography.RandomNumberGenerator` | Cryptographically strong 64-byte random refresh tokens |
| **Signature Verification** | `System.Security.Cryptography.HMACSHA256` | Webhook and checkout signature computation using constant-time comparison |

---

## 5. Testing & Benchmarking

| Component | Version / Package | Description & Usage |
| :--- | :--- | :--- |
| **Test Framework** | `xunit` (`2.9.3`) | Modern unit and integration test framework |
| **Visual Studio Test Runner** | `xunit.runner.visualstudio` (`3.1.4`) | VS/CLI test runner adapter |
| **Test Platform SDK** | `Microsoft.NET.Test.Sdk` (`17.14.1`) | Core MSBuild test runner SDK |
| **Mocking Framework** | `Moq` (`4.20.72`) | Dynamic interface mocking for unit tests |
| **Code Coverage** | `coverlet.collector` (`6.0.4`) | Cross-platform code coverage data collector |
| **Performance Benchmarks** | `BenchmarkDotNet` (`0.14.0`) | Microbenchmarking suite in `benchmarks/QuickCart.BenchmarkHost` |

---

## 6. Automation Scripts

* [`scripts/build.ps1`](file:///d:/Asp.net%20core_Project/Quick-commerce/scripts/build.ps1): Restores .NET tools, restores solution NuGet packages, and compiles `QuickCart.sln`.
* [`scripts/migrate-database.ps1`](file:///d:/Asp.net%20core_Project/Quick-commerce/scripts/migrate-database.ps1): Executes `dotnet ef database update` across all 8 module DbContexts.
* [`scripts/run-dev.ps1`](file:///d:/Asp.net%20core_Project/Quick-commerce/scripts/run-dev.ps1): Concurrently launches the backend API and Angular frontend for local development.



