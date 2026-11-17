# WinTweaker Continuous Code & Tweak Verification Suite
# Validates:
#   1) PowerShell AST syntax of all scripts/ + build/verify/sync scripts (PS 5.1 + UTF-8)
#   2) Deep audit of all tweaks in data/tweaks.json
#   3) Embedded tweaks.json byte-for-byte parity inside dist/Tweaker.exe + binary size < 5 MB
#   4) Full execution of dist/Tests.exe (all 13 suites, 7670+ assertions)
$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$failed = 0

function Fail-Check([string]$msg) {
    Write-Host "[FAIL] $msg" -ForegroundColor Red
    $script:failed++
}

function Pass-Check([string]$msg) {
    Write-Host "[PASS] $msg" -ForegroundColor Green
}

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "      WINTWEAKER CONTINUOUS CODE & TWEAK VERIFICATION SUITE       " -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan

# [1/4] AST Syntax Check of all production .ps1 scripts
Write-Host "`n[1/4] Checking PowerShell AST syntax of all production scripts..." -ForegroundColor Yellow
$scriptTargets = @()
$scriptsDir = Join-Path $repoRoot "scripts"
if (Test-Path $scriptsDir) {
    $scriptTargets += Get-ChildItem -Path $scriptsDir -Filter "*.ps1" -File
}
foreach ($rootScript in @("build.ps1", "verify-all.ps1", "sync-upstream.ps1")) {
    $p = Join-Path $repoRoot $rootScript
    if (Test-Path $p) {
        $scriptTargets += Get-Item $p
    }
}

$astErrorFiles = 0
foreach ($f in $scriptTargets) {
    $tokens = $null; $errs = $null
    [void][System.Management.Automation.Language.Parser]::ParseFile($f.FullName, [ref]$tokens, [ref]$errs)
    $raw = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    $tokens2 = $null; $errs2 = $null
    [void][System.Management.Automation.Language.Parser]::ParseInput($raw, [ref]$tokens2, [ref]$errs2)

    if ($errs.Count -gt 0 -or $errs2.Count -gt 0) {
        $astErrorFiles++
        Fail-Check "AST syntax error in $($f.Name): PS5.1=$($errs.Count), UTF8=$($errs2.Count)"
    }
}
if ($astErrorFiles -eq 0) {
    Pass-Check "All $($scriptTargets.Count) PowerShell scripts passed AST syntax verification (0 errors)"
}

# [2/4] Deep JSON & Tweak Code Audit
Write-Host "`n[2/4] Auditing every tweak in data/tweaks.json..." -ForegroundColor Yellow
$tweaksPath = Join-Path $repoRoot "data\tweaks.json"
if (-not (Test-Path $tweaksPath)) {
    Fail-Check "Missing data/tweaks.json"
} else {
    $tweaksRaw = [System.IO.File]::ReadAllText($tweaksPath, [System.Text.Encoding]::UTF8)
    $tweaks = $tweaksRaw | ConvertFrom-Json
    $idSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    $auditErrors = 0

    foreach ($t in $tweaks) {
        if (-not $idSet.Add($t.id)) {
            Fail-Check "Duplicate tweak ID: $($t.id)"
            $auditErrors++
        }
        if ($t.name_ru -match "[\r\n]" -or $t.description_ru -match "[\r\n]") {
            Fail-Check "Embedded newline in text of $($t.id)"
            $auditErrors++
        }
        if (($t.name_ru + $t.description_ru).IndexOf([char]0xFFFD) -ge 0) {
            Fail-Check "Corrupted UTF-8 char in $($t.id)"
            $auditErrors++
        }
        foreach ($r in $t.registry) {
            if ($r.path -match "[\r\n\t]|\\\\|^\\|\\$|^\s|\s$" -or $r.name -match "[\r\n\t]|^\s|\s$") {
                Fail-Check "Invalid registry path/name in $($t.id): $($r.path)\$($r.name)"
                $auditErrors++
            }
            if ($null -eq $r.apply) {
                Fail-Check "Null apply value in $($t.id)"
                $auditErrors++
            }
        }
    }
    if ($auditErrors -eq 0) {
        Pass-Check "All $($tweaks.Count) tweaks passed deep structural, path, and text audit"
    }
}

# [3/4] Verify Binary Size & Embedded Resource Parity
Write-Host "`n[3/4] Checking dist/Axis.exe size and embedded tweaks.json parity..." -ForegroundColor Yellow
$axisExe = Join-Path $repoRoot "dist\Axis.exe"
$testsExe = Join-Path $repoRoot "dist\Tests.exe"

$targetExe = $axisExe

if (-not (Test-Path $targetExe)) {
    Fail-Check "Missing dist/Axis.exe"
} else {
    $sizeBytes = (Get-Item $targetExe).Length
    $sizeMB = [math]::Round($sizeBytes / 1MB, 2)
    if ($sizeBytes -ge 5MB) {
        Fail-Check "$([System.IO.Path]::GetFileName($targetExe)) exceeds 5 MB limit: $sizeMB MB"
    } else {
        Pass-Check "$([System.IO.Path]::GetFileName($targetExe)) size OK: $sizeMB MB (< 5.0 MB limit)"
    }

    # Verify embedded tweaks.json matches data/tweaks.json byte-for-byte
    try {
        $asmBytes = [System.IO.File]::ReadAllBytes($targetExe)
        $asm = [System.Reflection.Assembly]::Load($asmBytes)
        $resName = $asm.GetManifestResourceNames() | Where-Object { $_ -like "*tweaks.json" } | Select-Object -First 1
        if (-not $resName) {
            Fail-Check "Embedded tweaks.json resource not found inside Axis.exe"
        } else {
            $stream = $asm.GetManifestResourceStream($resName)
            $ms = New-Object System.IO.MemoryStream
            $stream.CopyTo($ms)
            $stream.Dispose()
            $embeddedBytes = $ms.ToArray()
            $ms.Dispose()
            $diskBytes = [System.IO.File]::ReadAllBytes($tweaksPath)

            $sha = [System.Security.Cryptography.SHA256]::Create()
            $h1 = [BitConverter]::ToString($sha.ComputeHash($embeddedBytes))
            $h2 = [BitConverter]::ToString($sha.ComputeHash($diskBytes))
            $sha.Dispose()

            if ($h1 -ne $h2) {
                Fail-Check "Embedded tweaks.json in Axis.exe is OUT OF SYNC with data/tweaks.json! Rebuild required."
            } else {
                Pass-Check "Embedded tweaks.json in Axis.exe matches data/tweaks.json byte-for-byte (SHA256 verified)"
            }
        }
    } catch {
        Fail-Check "Failed to inspect embedded resource in Axis.exe: $($_.Exception.Message)"
    }
}

# [4/4] Run full Tests.exe suite
Write-Host "`n[4/4] Running full automated test runner (dist/Tests.exe)..." -ForegroundColor Yellow
if (-not (Test-Path $testsExe)) {
    Fail-Check "Missing dist/Tests.exe"
} else {
    $testOutput = & $testsExe 2>&1
    $exitCode = $LASTEXITCODE
    $summaryLines = $testOutput | Select-String -Pattern "FAIL|ИТОГО|ПРОВАЛЕНО|ВСЕ ТЕСТЫ"
    foreach ($line in $summaryLines) {
        Write-Host "  $line"
    }
    if ($exitCode -ne 0) {
        Fail-Check "Tests.exe exited with code $exitCode"
    } else {
        Pass-Check "Tests.exe passed 100% of all suites (exit code 0)"
    }
}

Write-Host "`n==================================================================" -ForegroundColor Cyan
if ($failed -gt 0) {
    Write-Host "[FAIL] Verification failed with $failed error(s)!" -ForegroundColor Red
    exit 1
} else {
    Write-Host "[PASS] ALL CODE, SCRIPTS, BINARIES & $($tweaks.Count) TWEAKS VERIFIED 100%!" -ForegroundColor Green
    exit 0
}
