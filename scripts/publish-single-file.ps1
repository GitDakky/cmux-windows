#Requires -Version 5.1
<#
.SYNOPSIS
  Publish a single-file self-contained cmuxw.exe.
#>
param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "publish/cmux-win-x64-single"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

dotnet publish src/Cmux/Cmux.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=false `
    -o $OutputDir

Write-Host "Single-file build: $((Resolve-Path (Join-Path $OutputDir 'cmuxw.exe')).Path)"
