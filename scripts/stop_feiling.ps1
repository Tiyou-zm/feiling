Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$statePath = Join-Path $projectRoot "data\runtime\feiling-pet.json"

if (-not (Test-Path $statePath)) {
    Write-Host "Feiling pet is not running."
    exit 0
}

$state = Get-Content $statePath -Raw | ConvertFrom-Json

if ($state.pid) {
    $proc = Get-Process -Id $state.pid -ErrorAction SilentlyContinue
    if ($proc) {
        $proc.CloseMainWindow() | Out-Null
        Start-Sleep -Milliseconds 800
        if (-not $proc.HasExited) {
            Stop-Process -Id $proc.Id -Force
        }
    }
}

Remove-Item $statePath -Force -ErrorAction SilentlyContinue
Write-Host "Feiling pet stopped."
