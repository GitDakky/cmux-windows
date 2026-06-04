#Requires -Version 5.1
<#
.SYNOPSIS
  Run automated smoke checks (build, test, optional publish verify).
#>
param(
    [switch]$Publish
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

Write-Host "==> dotnet restore"
dotnet restore Cmux.sln

Write-Host "==> dotnet build Release"
dotnet build Cmux.sln -c Release --no-restore

Write-Host "==> dotnet test"
dotnet test Cmux.sln -c Release --no-build --verbosity minimal

if ($Publish) {
    Write-Host "==> publish win-x64"
    & "$PSScriptRoot/publish-win-x64.ps1"
    $exe = Join-Path $RepoRoot "publish/cmux-win-x64/cmuxw.exe"
    if (-not (Test-Path $exe)) {
        throw "Expected published app at $exe"
    }
    Write-Host "Published: $exe"
}

Write-Host "Smoke tests passed."
