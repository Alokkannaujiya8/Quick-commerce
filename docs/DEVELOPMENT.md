# QuickCart Local Development Guide

This guide details instructions for setting up, running, testing, and developing **QuickCart** on a local developer machine.

---

## 1. Prerequisites

Ensure the following runtimes and tools are installed:

| Tool | Minimum Version | Check Command |
| :--- | :--- | :--- |
| **.NET SDK** | `10.0.100`+ (C# 14.0) | `dotnet --version` |
| **PostgreSQL** | `16.0`+ | `psql --version` |
| **Node.js** | `20.0`+ (LTS / Active) | `node -v` |
| **npm** | `10.0`+ | `npm -v` |
| **dotnet-ef** | `10.0.4` | `dotnet tool restore` |

---

## 2. Initial Setup

### Step 1: Clone and Restore .NET Tools
```powershell
cd "D:\Asp.net core_Project\Quick-commerce"
dotnet tool restore
```

### Step 2: Configure Database Connection
Update the connection string in `src/QuickCart.Api/appsettings.json` or create `appsettings.Local.json` with your local PostgreSQL credentials:
```json
{
  "ConnectionStrings": {
    "QuickCartDb": "Host=localhost;Port=5432;Database=quickcartdb;Username=postgres;Password=YourPasswordHere"
  }
}
```

### Step 3: Apply Database Migrations
Apply migrations across all 8 module schemas (`catalog`, `inventory`, `ordering`, `cart`, `delivery`, `identity`, `payment`, `promotion`):
```powershell
./scripts/migrate-database.ps1
```

Or run the dedicated console migration tool:
```powershell
dotnet run --project tools/QuickCart.DatabaseMigrator
```

---

## 3. Running the Solution

### Run Both Backend & Frontend Simultaneously
Use the helper script:
```powershell
./scripts/run-dev.ps1
```

### Run Backend Only
```powershell
cd src/QuickCart.Api
dotnet run
```
* **HTTPS**: `https://localhost:7189`
* **HTTP**: `http://localhost:5242`
* **Swagger/OpenAPI UI**: `https://localhost:7189/swagger`

### Run Frontend Only
```powershell
cd quick-cart-app
npm install
npm start
```
* **Frontend URL**: `http://localhost:4200`

---

## 4. Running Tests

Run the automated test suite across all modules:
```powershell
dotnet test QuickCart.sln
```

Or target an individual module:
```powershell
dotnet test tests/Catalog.Tests
dotnet test tests/Ordering.Tests
dotnet test tests/Inventory.Tests
```

---

## 5. Coding Standards & Conventions

1. **Inward Dependencies**: Code in `*.Domain` and `*.Application` must never reference `*.Infrastructure` or EF Core.
2. **Schema Separation**: Do not create navigation properties or foreign keys between entities belonging to different schemas. Reference them strictly by `Guid` identifiers.
3. **Nullability**: All projects enforce `<Nullable>enable</Nullable>`. Use appropriate null-checking and optional types.
4. **Dates**: Always store timestamps in UTC (`DateTime.UtcNow`).

