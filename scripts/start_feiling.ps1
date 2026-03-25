Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$runtimeDir = Join-Path $projectRoot "data\runtime"
$statePath = Join-Path $runtimeDir "feiling-pet.json"
$dotnetExe = "C:\Program Files\dotnet\dotnet.exe"
$projectPath = Join-Path $projectRoot "desktop\FeilingPetShell\FeilingPetShell.csproj"
$buildDir = Join-Path $projectRoot "desktop\FeilingPetShell\bin\Debug\net8.0-windows"
$exePath = Join-Path $buildDir "FeilingPetShell.exe"

New-Item -ItemType Directory -Force -Path $runtimeDir | Out-Null

if (Test-Path $statePath) {
    $existing = Get-Content $statePath -Raw | ConvertFrom-Json
    if ($existing.pid) {
        $alive = Get-Process -Id $existing.pid -ErrorAction SilentlyContinue
        if ($alive) {
            throw "Feiling pet is already running. Use .\\scripts\\stop_feiling.ps1 first."
        }
    }

    Remove-Item $statePath -Force
}

& $dotnetExe build $projectPath -c Debug | Out-Host

if (-not (Test-Path $exePath)) {
    throw "Feiling pet executable not found at $exePath"
}

$proc = Start-Process -FilePath $exePath -WorkingDirectory $projectRoot -PassThru

$state = @{
    pid = $proc.Id
    started_at = (Get-Date).ToString("s")
    exe = $exePath
}

$state | ConvertTo-Json | Set-Content -Path $statePath -Encoding UTF8

Write-Host ""
Write-Host "Feiling pet started."
Write-Host "PID: $($proc.Id)"
Write-Host "State file: $statePath"
