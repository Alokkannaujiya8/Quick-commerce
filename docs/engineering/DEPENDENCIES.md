# External Dependencies & Package Registry

This document catalogs every external NuGet package dependency currently referenced across the QuickCart backend projects, including verified package versions, hosting projects, and architectural responsibilities.

---

## 1. NuGet Package Inventory

| Package | Verified Version | Referencing Projects | Purpose & Architectural Role |
| :--- | :--- | :--- | :--- |
| **`BCrypt.Net-Next`** | `4.2.0` | `Identity.Infrastructure` | Secure password hashing using adaptive blowfish algorithms (`EnhancedHashPassword` / `EnhancedVerify`). |
| **`BenchmarkDotNet`** | `0.14.0` | `QuickCart.BenchmarkHost` | Microbenchmarking framework for latency, memory allocation, and hot-path execution analysis. |
| **`coverlet.collector`** | `6.0.4` | `Identity.Tests`, `Payment.Tests` | Code coverage collection integrated into the `dotnet test` pipeline. |
| **`Google.Apis.Auth`** | `1.76.0` | `Identity.Infrastructure` | Google OpenID Connect ID token signature validation, audience matching, and claim parsing (`GoogleJsonWebSignature`). |
| **`Microsoft.AspNetCore.Authentication.JwtBearer`** | `10.0.12` | `Identity.Infrastructure` | ASP.NET Core middleware for authenticating incoming HTTP requests using signed JWT bearer tokens. |
| **`Microsoft.AspNetCore.OpenApi`** | `10.0.12` | `QuickCart.Api` | Built-in ASP.NET Core OpenAPI metadata and document generation (`builder.Services.AddOpenApi()`). |
| **`Microsoft.EntityFrameworkCore`** | `10.0.4` | `BuildingBlocks.Infrastructure`, `QuickCart.DatabaseMigrator` | Core Object-Relational Mapper (ORM) abstractions, change tracking, and interceptor base classes. |
| **`Microsoft.EntityFrameworkCore.Design`** | `10.0.4` | `QuickCart.Api` | Design-time tooling support for EF Core command-line migration commands (`dotnet ef`). |
| **`Microsoft.NET.Test.Sdk`** | `17.14.1` | `Identity.Tests`, `Payment.Tests` | Core MSBuild test SDK required to build and execute test assemblies. |
| **`Moq`** | `4.20.72` | `Identity.Tests`, `Payment.Tests` | Mocking library used to isolate unit tests from external dependencies and database contexts. |
| **`Npgsql.EntityFrameworkCore.PostgreSQL`** | `10.0.3` | `Cart.Infrastructure`, `Catalog.Infrastructure`, `Delivery.Infrastructure`, `Identity.Infrastructure`, `Inventory.Infrastructure`, `Ordering.Infrastructure`, `Payment.Infrastructure`, `Promotion.Infrastructure`, `QuickCart.DatabaseMigrator` | PostgreSQL provider for EF Core 10, managing connection pooling, SQL generation, and PostgreSQL schema mappings. |
| **`Scalar.AspNetCore`** | `2.17.3` | `QuickCart.Api` | Interactive OpenAPI documentation web interface rendered at `/scalar/v1`. |
| **`System.IdentityModel.Tokens.Jwt`** | `8.22.0` | `Identity.Infrastructure` | Low-level cryptographic handler for creating, encoding, and signing JWT access tokens. |
| **`xunit`** | `2.9.3` | `Identity.Tests`, `Payment.Tests` | Core xUnit testing framework providing assertions, theories, and test fixtures. |
| **`xunit.runner.visualstudio`** | `3.1.4` | `Identity.Tests`, `Payment.Tests` | Visual Studio and `dotnet test` test runner adapter for xUnit. |

---

## 2. Package Governance & Rules

1. **Centralized Compatibility**: All runtime packages target .NET 10 (`net10.0`).
2. **Zero Dependencies in Domain**: `BuildingBlocks.Domain` and all `<Module>.Domain` projects must contain **zero** external NuGet package references.
3. **Application Layer Isolation**: `<Module>.Application` projects must not reference database providers or infrastructure libraries.
4. **Version Consistency**: Package versions must remain aligned across all referencing projects (e.g., all 8 infrastructure projects use `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.3`).

