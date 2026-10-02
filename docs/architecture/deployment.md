# Deployment & Infrastructure Architecture

This document evaluates the deployment topology, runtime hosting environments, build pipelines, and automation scripts across the QuickCart repository.

---

## 1. Cloud & Containerization Status

```text
Status: Not Implemented / Not Confirmed

Evidence:
1. Inspected repository root, scripts/, and tools/ for container manifests (Dockerfile, docker-compose.yml, Containerfile). None exist.
2. Inspected repository for cloud orchestration definitions (Kubernetes manifests, Helm charts, Terraform/OpenTofu configurations, Bicep, CloudFormation). None exist.
3. Inspected repository for CI/CD workflow automation (.github/workflows, azure-pipelines.yml, GitLab CI, Jenkinsfile). None exist.
```

---

## 2. Verified Local Deployment & Runtime Tooling

QuickCart currently utilizes a PowerShell automation suite for local developer orchestration:

### 2.1 Automated Build Pipeline ([`scripts/build.ps1`](file:///d:/Asp.net%20core_Project/Quick-commerce/scripts/build.ps1))
Restores .NET local tools, restores all NuGet packages across the 46 projects, and performs a clean build:
```powershell
./scripts/build.ps1 -Configuration Release
```

### 2.2 Database Migration Automation ([`scripts/migrate-database.ps1`](file:///d:/Asp.net%20core_Project/Quick-commerce/scripts/migrate-database.ps1))
Applies EF Core migrations sequentially across all 8 module schemas targeting the configured PostgreSQL instance:
```powershell
./scripts/migrate-database.ps1
```

### 2.3 Concurrent Development Runner ([`scripts/run-dev.ps1`](file:///d:/Asp.net%20core_Project/Quick-commerce/scripts/run-dev.ps1))
Launches both the backend API host and the Angular Single Page Application concurrently:
* **Backend API**: `https://localhost:7189` (Swagger/Scalar at `/scalar/v1`)
* **Frontend SPA**: `http://localhost:4200`

### 2.4 Standalone Migration Tool ([`tools/QuickCart.DatabaseMigrator`](file:///d:/Asp.net%20core_Project/Quick-commerce/tools/QuickCart.DatabaseMigrator/))
A compiled .NET 10 console application capable of migrating database schemas programmatically without requiring the `dotnet-ef` CLI tool:
```powershell
dotnet run --project tools/QuickCart.DatabaseMigrator
```

---

## 3. Production Deployment Roadmap (When Containerization is Needed)

To deploy QuickCart to enterprise cloud environments (e.g., AWS ECS/EKS, Azure Container Apps/AKS, Google Cloud Run/GKE):
1. **Multi-Stage Dockerfile**:
   * Build stage: SDK `mcr.microsoft.com/dotnet/sdk:10.0` restoring and compiling `src/QuickCart.Api`.
   * Runtime stage: ASP.NET Core `mcr.microsoft.com/dotnet/aspnet:10.0` running as a non-root user.
2. **Docker Compose for Local Staging**:
   * Service 1: `quickcart-api`
   * Service 2: `quickcart-db` (PostgreSQL 16)
   * Service 3: `quickcart-frontend` (Nginx serving built Angular SPA)
3. **Continuous Integration (CI)**:
   * GitHub Actions workflow executing `dotnet build QuickCart.sln` and `dotnet test QuickCart.sln`.

