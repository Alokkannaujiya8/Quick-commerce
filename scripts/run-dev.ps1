<#
.SYNOPSIS
    Launches QuickCart development environment.
.DESCRIPTION
    Starts QuickCart.Api backend and optionally quick-cart-app frontend in separate windows.
#>

[CmdletBinding()]
param(
    [switch]$BackendOnly,
    [switch]$FrontendOnly
)

$RepoRoot = Split-Path -Parent $PSScriptRoot

if (-not $FrontendOnly) {
    Write-Host "Starting QuickCart.Api backend on https://localhost:7189 / http://localhost:5242..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RepoRoot\src\QuickCart.Api'; dotnet run"
}

if (-not $BackendOnly) {
    Write-Host "Starting quick-cart-app frontend on http://localhost:4200..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RepoRoot\quick-cart-app'; npm start"
}

Write-Host "`n[OK] Development services initiated." -ForegroundColor Green
