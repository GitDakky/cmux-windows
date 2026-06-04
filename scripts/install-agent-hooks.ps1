#Requires -Version 5.1
<#
.SYNOPSIS
  Install a PowerShell helper that forwards agent attention events to cmux via CLI.
.DESCRIPTION
  Appends a small profile snippet that defines Send-CmuxAgentNotify for use in
  Claude Code / Codex hook scripts. Requires cmux.exe on PATH and cmux running.
#>
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$profileDir = Split-Path -Parent $PROFILE
if (-not (Test-Path $profileDir)) {
    New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
}

$hookFile = Join-Path $profileDir "cmux-agent-hooks.ps1"
$snippet = @'
# cmux-windows agent notification helper (installed by install-agent-hooks.ps1)
function Send-CmuxAgentNotify {
    param(
        [string]$Title = "Agent",
        [string]$Body = "Needs your attention"
    )
    if (-not (Get-Command cmux -ErrorAction SilentlyContinue)) { return }
    cmux notify --title $Title --body $Body
}
'@

if ((Test-Path $hookFile) -and -not $Force) {
    Write-Host "Already exists: $hookFile (use -Force to overwrite)"
} else {
    Set-Content -Path $hookFile -Value $snippet -Encoding UTF8
    Write-Host "Wrote $hookFile"
}

$importLine = ". `"$hookFile`""
if (Test-Path $PROFILE) {
    $existing = Get-Content $PROFILE -Raw
    if ($existing -notlike "*cmux-agent-hooks.ps1*") {
        Add-Content -Path $PROFILE -Value "`n$importLine"
        Write-Host "Appended import to $PROFILE"
    }
} else {
    Set-Content -Path $PROFILE -Value $importLine -Encoding UTF8
    Write-Host "Created $PROFILE"
}

Write-Host "Done. Call Send-CmuxAgentNotify -Title 'Claude Code' -Body 'Waiting for input' from hooks."
