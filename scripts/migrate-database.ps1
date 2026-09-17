<#
.SYNOPSIS
    Applies EF Core database migrations across all QuickCart modules.
.DESCRIPTION
    Runs `dotnet ef database update` for each module DbContext using QuickCart.Api as the startup project.
#>

[CmdletBinding()]
param(
    [string]$Module = "all"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$StartupProject = "$RepoRoot\src\QuickCart.Api\QuickCart.Api.csproj"

$ModulesConfig = @(
    @{ Name = "Catalog";   Project = "$RepoRoot\src\Modules\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj";       Context = "CatalogDbContext" },
    @{ Name = "Inventory"; Project = "$RepoRoot\src\Modules\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj"; Context = "InventoryDbContext" },
    @{ Name = "Ordering";  Project = "$RepoRoot\src\Modules\Ordering\Ordering.Infrastructure\Ordering.Infrastructure.csproj";   Context = "OrderingDbContext" },
    @{ Name = "Cart";      Project = "$RepoRoot\src\Modules\Cart\Cart.Infrastructure\Cart.Infrastructure.csproj";               Context = "CartDbContext" },
    @{ Name = "Delivery";  Project = "$RepoRoot\src\Modules\Delivery\Delivery.Infrastructure\Delivery.Infrastructure.csproj";   Context = "DeliveryDbContext" },
    @{ Name = "Identity";  Project = "$RepoRoot\src\Modules\Identity\Identity.Infrastructure\Identity.Infrastructure.csproj";   Context = "IdentityDbContext" },
    @{ Name = "Payment";   Project = "$RepoRoot\src\Modules\Payment\Payment.Infrastructure\Payment.Infrastructure.csproj";       Context = "PaymentDbContext" },
    @{ Name = "Promotion"; Project = "$RepoRoot\src\Modules\Promotion\Promotion.Infrastructure\Promotion.Infrastructure.csproj"; Context = "PromotionDbContext" }
)

Write-Host "==> QuickCart EF Core Database Migrations" -ForegroundColor Cyan

$Targets = if ($Module -eq "all") {
    $ModulesConfig
} else {
    $ModulesConfig | Where-Object { $_.Name -like "*$Module*" }
}

if (-not $Targets) {
    Write-Error "No matching module found for: $Module. Valid options: Catalog, Inventory, Ordering, Cart, Delivery, Identity, Payment, Promotion, all."
    exit 1
}

foreach ($t in $Targets) {
    Write-Host "`n--> Applying migrations for [$($t.Name)] (Context: $($t.Context))..." -ForegroundColor Yellow
    dotnet ef database update `
        --project $t.Project `
        --startup-project $StartupProject `
        --context $t.Context
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Migration failed for module: $($t.Name)"
        exit $LASTEXITCODE
    }
}

Write-Host "`n[OK] All database migrations applied successfully!" -ForegroundColor Green
