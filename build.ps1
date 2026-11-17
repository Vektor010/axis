# WinTweaker Build Script (.NET Framework 4.x / MSBuild)
param(
    [switch]$SkipVerify
)
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

Write-Host "[1/5] Compiler found: $msBuildExe" -ForegroundColor Green

$distDir = Join-Path $PSScriptRoot "dist"
if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir -Force | Out-Null
}

$tweakerProj = Join-Path $PSScriptRoot "Tweaker.csproj"
$testsProj = Join-Path $PSScriptRoot "Tests\Tests.csproj"

Write-Host "[2/5] Compiling Axis.exe (WPF / BAML / Embedded tweaks.json)..." -ForegroundColor Yellow
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
    Write-Host "[ERROR] Axis.exe build failed with exit code $($p1.ExitCode)!" -ForegroundColor Red
    exit $p1.ExitCode
}

Write-Host "[3/5] Compiling Tests.exe (Console test runner)..." -ForegroundColor Yellow
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

$axisExe = Join-Path $distDir "Axis.exe"
$tweakerExe = Join-Path $distDir "Tweaker.exe"
$testsExe = Join-Path $distDir "Tests.exe"

# Сохраняем и Axis.exe (флагман), и Tweaker.exe (обратная совместимость)
if (Test-Path $axisExe) {
    Copy-Item $axisExe $tweakerExe -Force
}

if ((Test-Path $axisExe) -and (Test-Path $testsExe)) {
    $s1 = [math]::Round((Get-Item $axisExe).Length / 1KB, 1)
    $s2 = [math]::Round((Get-Item $testsExe).Length / 1KB, 1)

    Write-Host "[4/5] Both binaries built successfully!" -ForegroundColor Green
    Write-Host "  -> Axis.exe (Flagship): $axisExe ($s1 KB, target: < 5 MB)" -ForegroundColor Cyan
    Write-Host "  -> Tests.exe:           $testsExe ($s2 KB)" -ForegroundColor Cyan

    if (-not $SkipVerify) {
        $verifyScript = Join-Path $PSScriptRoot "verify-all.ps1"
        if (Test-Path $verifyScript) {
            Write-Host "`n[5/5] Running mandatory code & tweak verification suite (verify-all.ps1)..." -ForegroundColor Yellow
            & $verifyScript
            if ($LASTEXITCODE -ne 0) {
                Write-Host "[ERROR] Verification failed! Build rejected." -ForegroundColor Red
                exit $LASTEXITCODE
            }
        }
    }

    Write-Host "==================================================================" -ForegroundColor Cyan
    exit 0
} else {
    Write-Host "[ERROR] One or more target binaries were not generated!" -ForegroundColor Red
    exit 1
}
