<#
.SYNOPSIS
    Builds the QuickCart Modular Monolith solution.
.DESCRIPTION
    Restores dependencies and performs a clean build of QuickCart.sln.
#>

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "==> QuickCart Build Process" -ForegroundColor Cyan
Write-Host "    Repository Root: $RepoRoot"
Write-Host "    Configuration:   $Configuration" -ForegroundColor Yellow

Push-Location $RepoRoot
try {
    Write-Host "`n[1/3] Restoring .NET tools..." -ForegroundColor Green
    dotnet tool restore

    Write-Host "`n[2/3] Restoring solution NuGet packages..." -ForegroundColor Green
    dotnet restore QuickCart.sln

    Write-Host "`n[3/3] Building solution ($Configuration)..." -ForegroundColor Green
    dotnet build QuickCart.sln --configuration $Configuration --no-restore

    Write-Host "`n[OK] Build completed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}

