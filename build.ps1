# WinTweaker Build Script (.NET Framework 4.x / MSBuild)
$ErrorActionPreference = "Stop"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "       BUILD WINTWEAKER & TESTS (.NET FRAMEWORK 4.x / MSBUILD)    " -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan

$frameworkDir = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
$msBuildExe = Join-Path $frameworkDir "MSBuild.exe"

if (-not (Test-Path $msBuildExe)) {
    $frameworkDir = "C:\Windows\Microsoft.NET\Framework\v4.0.30319"
    $msBuildExe = Join-Path $frameworkDir "MSBuild.exe"
}

if (-not (Test-Path $msBuildExe)) {
    Write-Host "[ERROR] MSBuild.exe not found in $frameworkDir!" -ForegroundColor Red
    exit 1
}

Write-Host "[1/4] Compiler found: $msBuildExe" -ForegroundColor Green

$distDir = Join-Path $PSScriptRoot "dist"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir -Force | Out-Null
}

$tweakerProj = Join-Path $PSScriptRoot "Tweaker.csproj"
$testsProj = Join-Path $PSScriptRoot "Tests\Tests.csproj"

Write-Host "[2/4] Compiling Tweaker.exe (WPF / BAML / Embedded tweaks.json)..." -ForegroundColor Yellow
$tweakerArgs = @(
    $tweakerProj,
    "/t:Rebuild",
    "/p:Configuration=Release",
    "/p:Platform=AnyCPU",
    "/v:minimal",
    "/nologo"
)
$p1 = Start-Process -FilePath $msBuildExe -ArgumentList $tweakerArgs -Wait -NoNewWindow -PassThru
if ($p1.ExitCode -ne 0) {
    Write-Host "[ERROR] Tweaker.exe build failed with exit code $($p1.ExitCode)!" -ForegroundColor Red
    exit $p1.ExitCode
}

Write-Host "[3/4] Compiling Tests.exe (Console test runner)..." -ForegroundColor Yellow
$testsArgs = @(
    $testsProj,
    "/t:Rebuild",
    "/p:Configuration=Release",
    "/p:Platform=AnyCPU",
    "/v:minimal",
    "/nologo"
)
$p2 = Start-Process -FilePath $msBuildExe -ArgumentList $testsArgs -Wait -NoNewWindow -PassThru
if ($p2.ExitCode -ne 0) {
    Write-Host "[ERROR] Tests.exe build failed with exit code $($p2.ExitCode)!" -ForegroundColor Red
    exit $p2.ExitCode
}

$tweakerExe = Join-Path $distDir "Tweaker.exe"
$testsExe = Join-Path $distDir "Tests.exe"

if ((Test-Path $tweakerExe) -and (Test-Path $testsExe)) {
    $s1 = [math]::Round((Get-Item $tweakerExe).Length / 1KB, 1)
    $s2 = [math]::Round((Get-Item $testsExe).Length / 1KB, 1)

    Write-Host "[4/4] Both binaries built successfully!" -ForegroundColor Green
    Write-Host "  -> Tweaker.exe: $tweakerExe ($s1 KB, target: < 5 MB)" -ForegroundColor Cyan
    Write-Host "  -> Tests.exe:   $testsExe ($s2 KB)" -ForegroundColor Cyan
    Write-Host "==================================================================" -ForegroundColor Cyan
    exit 0
} else {
    Write-Host "[ERROR] One or more target binaries were not generated!" -ForegroundColor Red
    exit 1
}
