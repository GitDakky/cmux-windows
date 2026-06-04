#Requires -Version 5.1
<#
.SYNOPSIS
  Publish self-contained win-x64 cmux Windows app and CLI.
#>
param(
    [string]$Configuration = "Release",
    [string]$OutputRoot = "publish"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

$version = (Select-Xml -Path "src/Cmux/Cmux.csproj" -XPath "//Version").Node.InnerText.Trim()
$appDir = Join-Path $OutputRoot "cmux-win-x64"
$cliDir = Join-Path $OutputRoot "cmux-cli-win-x64"
$zipPath = Join-Path $OutputRoot "cmux-windows-v$version-win-x64.zip"

Write-Host "Publishing cmux-windows v$version (win-x64, self-contained)..."

dotnet publish src/Cmux/Cmux.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -o $appDir

dotnet publish src/Cmux.Cli/Cmux.Cli.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -o $cliDir

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path $appDir, $cliDir -DestinationPath $zipPath

Write-Host "App:  $((Resolve-Path $appDir).Path)"
Write-Host "CLI:  $((Resolve-Path $cliDir).Path)"
Write-Host "Zip:  $((Resolve-Path $zipPath).Path)"
