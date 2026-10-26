param(
    [string]$BuildConfiguration = "Release",
    [string]$OutputDirectory = "artifacts"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectDir = Join-Path $repoRoot "src/EchoCloneMotionRecorder.Nomad"
$coreProjectDir = Join-Path $repoRoot "src/EchoCloneMotionRecorder"
$manifestPath = Join-Path $projectDir "manifest.json"
$dllPath = Join-Path $projectDir "bin/$BuildConfiguration/netstandard2.0/EchoCloneMotionRecorder.Nomad.dll"
$coreDllPath = Join-Path $coreProjectDir "bin/$BuildConfiguration/netstandard2.0/EchoCloneMotionRecorder.dll"
$outputPath = Join-Path $repoRoot $OutputDirectory
$stagePath = Join-Path $outputPath "EchoCloneMotionRecorder"

if (-not (Test-Path $manifestPath)) {
    throw "manifest.json is missing: $manifestPath"
}
if (-not (Test-Path $dllPath)) {
    throw "Compiled plugin DLL is missing: $dllPath. Build the Nomad plugin first."
}
if (-not (Test-Path $coreDllPath)) {
    throw "Recorder-core dependency is missing: $coreDllPath. Build the core project first."
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
Copy-Item $coreDllPath (Join-Path $stagePath "EchoCloneMotionRecorder.dll")

# Make the contract-build limitation visible so a test candidate is not mistaken
# for a Quest-verified release.
@"
EchoClone Motion Recorder - build status
Build target: Blade & Sorcery: Nomad
This package was compiled with build-only API contract assemblies unless an exact game
Managed directory was explicitly supplied to the build.
The contract assemblies are not included in this package.
This candidate has not been tested on a physical Quest or verified against the exact
Nomad runtime. Do not describe it as Quest-tested until it is tested in-game.
Known unfinished features: actual held-weapon replay, hostile AI, reliable player
damage, and verified native dismemberment.
"@ | Set-Content -Path (Join-Path $stagePath "BUILD_STATUS.txt") -Encoding utf8

# Never package compile-time contracts or proprietary game assemblies.
$unexpectedDlls = Get-ChildItem -Path $stagePath -Recurse -File |
    Where-Object { $_.Name -in @("ThunderRoad.dll", "UnityEngine.CoreModule.dll") }
if ($unexpectedDlls) {
    throw "Refusing to package game API contract or game assemblies."
}

$zipPath = Join-Path $outputPath "EchoCloneMotionRecorder-Nomad.zip"
if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
Compress-Archive -Path (Join-Path $stagePath "*") -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host "Created package: $zipPath"
Write-Host "Contents: manifest.json, EchoCloneMotionRecorder.Nomad.dll, EchoCloneMotionRecorder.dll, BUILD_STATUS.txt"
Write-Host "This is a contract-build candidate, not a Quest-verified release."
