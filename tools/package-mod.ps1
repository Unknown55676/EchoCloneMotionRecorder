param(
    [string]$BuildConfiguration = "Release",
    [string]$OutputDirectory = "artifacts"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectDir = Join-Path $repoRoot "src/EchoCloneMotionRecorder.Nomad"
$manifestPath = Join-Path $projectDir "manifest.json"
$dllPath = Join-Path $projectDir "bin/$BuildConfiguration/netstandard2.0/EchoCloneMotionRecorder.Nomad.dll"
$outputPath = Join-Path $repoRoot $OutputDirectory
$stagePath = Join-Path $outputPath "EchoCloneMotionRecorder"

if (-not (Test-Path $manifestPath)) {
    throw "manifest.json is missing: $manifestPath"
}
if (-not (Test-Path $dllPath)) {
    throw "Compiled plugin DLL is missing: $dllPath. Build against the exact target Nomad assemblies first."
}

# Validate the manifest before packaging.
$manifest = Get-Content -Raw $manifestPath | ConvertFrom-Json
if (-not $manifest.Name -or -not $manifest.ModVersion -or -not $manifest.GameVersion) {
    throw "manifest.json must contain Name, ModVersion, and GameVersion."
}

if (Test-Path $stagePath) { Remove-Item -Recurse -Force $stagePath }
New-Item -ItemType Directory -Force -Path $stagePath | Out-Null
Copy-Item $manifestPath (Join-Path $stagePath "manifest.json")
Copy-Item $dllPath (Join-Path $stagePath "EchoCloneMotionRecorder.Nomad.dll")

# Never package proprietary game assemblies.
$unexpectedGameDlls = Get-ChildItem -Path $stagePath -Recurse -File |
    Where-Object { $_.Name -in @("ThunderRoad.dll", "UnityEngine.CoreModule.dll") }
if ($unexpectedGameDlls) {
    throw "Refusing to package proprietary game assemblies."
}

$zipPath = Join-Path $outputPath "EchoCloneMotionRecorder-Nomad.zip"
if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
Compress-Archive -Path (Join-Path $stagePath "*") -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host "Created package: $zipPath"
Write-Host "Contents: manifest.json, EchoCloneMotionRecorder.Nomad.dll"
Write-Host "This ZIP has not been verified in-game; test it on the exact Nomad build before release."
