param(
    [string]$TweakerExe = "dist\Tweaker.exe",
    [string]$BaselineExe = "Tests\baseline\bin\BaselineWindow.exe",
    [string]$OutputFile = "tests\out\sample_ws_raw.txt"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
Set-Location $rootDir

if (-not [System.IO.Path]::IsPathRooted($TweakerExe)) { $TweakerExe = Join-Path $rootDir $TweakerExe }
if (-not [System.IO.Path]::IsPathRooted($BaselineExe)) { $BaselineExe = Join-Path $rootDir $BaselineExe }
if (-not [System.IO.Path]::IsPathRooted($OutputFile)) { $OutputFile = Join-Path $rootDir $OutputFile }

$outDir = Split-Path -Parent $OutputFile
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

function Sample-Process($exePath, $appLabel, $runNum, [string]$extraArgs = "") {
    # Launch app in normal/keep-open mode (no --diag-mem, window remains open)
    $proc = if ($extraArgs -and $extraArgs.Trim().Length -gt 0) {
        Start-Process -FilePath $exePath -ArgumentList $extraArgs -PassThru
    } else {
        Start-Process -FilePath $exePath -PassThru
    }
    $procName = $proc.ProcessName
    $records = @()
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $intervals = 40 # 0.0s to 20.0s in 0.5s steps
    
    $pcWsPriv = $null
    $pcHeap = $null
    $pcGen0 = $null
    try {
        $pcWsPriv = New-Object System.Diagnostics.PerformanceCounter("Process", "Working Set - Private", $procName)
    } catch { }
    try {
        $pcHeap = New-Object System.Diagnostics.PerformanceCounter(".NET CLR Memory", "# Bytes in all Heaps", $procName)
    } catch { }
    try {
        $pcGen0 = New-Object System.Diagnostics.PerformanceCounter(".NET CLR Memory", "# Gen 0 Collections", $procName)
    } catch { }

    try {
        for ($step = 0; $step -le $intervals; $step++) {
            $targetElapsedMs = $step * 500
            $currentElapsedMs = $stopwatch.ElapsedMilliseconds
            if ($currentElapsedMs -lt $targetElapsedMs) {
                Start-Sleep -Milliseconds ($targetElapsedMs - $currentElapsedMs)
            }
            
            $sec = [math]::Round($stopwatch.ElapsedMilliseconds / 1000.0, 1)
            $p = Get-Process -Id $proc.Id -ErrorAction SilentlyContinue
            if (-not $p) { break }
            
            $ws = $p.WorkingSet64
            $pm = $p.PrivateMemorySize64
            
            $wsPriv = 0
            $heapBytes = 0
            $gen0 = 0
            
            if ($pcWsPriv) { try { $wsPriv = [long]$pcWsPriv.RawValue } catch { } }
            if ($pcHeap) { try { $heapBytes = [long]$pcHeap.RawValue } catch { } }
            if ($pcGen0) { try { $gen0 = [long]$pcGen0.RawValue } catch { } }
            
            $records += [PSCustomObject]@{
                App = $appLabel
                Run = $runNum
                Second = $sec
                WorkingSet64 = $ws
                PrivateMemorySize64 = $pm
                WorkingSetPrivate = $wsPriv
                BytesInAllHeaps = $heapBytes
                Gen0Collections = $gen0
            }
        }
    } finally {
        if ($pcWsPriv) { try { $pcWsPriv.Dispose() } catch { } }
        if ($pcHeap) { try { $pcHeap.Dispose() } catch { } }
        if ($pcGen0) { try { $pcGen0.Dispose() } catch { } }
        if (-not $proc.HasExited) {
            $proc.Kill()
            $proc.WaitForExit(2000)
        }
    }
    
    return $records
}

$allRecords = @()

Write-Host "Запуск замеров памяти снаружи (sample-ws.ps1): 3 прогона Tweaker, 3 прогона Baseline..."

for ($r = 1; $r -le 3; $r++) {
    Write-Host "  -> Tweaker Прогон $r..."
    $allRecords += Sample-Process $TweakerExe "Tweaker" $r ""
    Start-Sleep -Seconds 1
}

for ($r = 1; $r -le 3; $r++) {
    Write-Host "  -> Baseline Прогон $r..."
    $allRecords += Sample-Process $BaselineExe "Baseline" $r "--keep-open"
    Start-Sleep -Seconds 1
}

# Save raw output
$rawLines = @("App|Run|Second|WorkingSet64|PrivateMemorySize64|WorkingSetPrivate|BytesInAllHeaps|Gen0Collections")
foreach ($rec in $allRecords) {
    $rawLines += ("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}" -f $rec.App, $rec.Run, $rec.Second, $rec.WorkingSet64, $rec.PrivateMemorySize64, $rec.WorkingSetPrivate, $rec.BytesInAllHeaps, $rec.Gen0Collections)
}
[System.IO.File]::WriteAllLines($OutputFile, $rawLines, [System.Text.Encoding]::UTF8)
Write-Host "Сырые замеры сохранены в $OutputFile (строк: $($rawLines.Count))"

# Display summary medians per second (0..20s)
function Get-Med($arr) {
    if (-not $arr -or $arr.Count -eq 0) { return 0 }
    $s = @($arr | Sort-Object)
    $mid = [int]($s.Count / 2)
    if ($s.Count % 2 -eq 1) { return $s[$mid] }
    return [math]::Round(($s[$mid - 1] + $s[$mid]) / 2.0, 2)
}

Write-Host "`n=== ТАБЛИЦА МЕДИАН ПАМЯТИ ПО СЕКУНДАМ (0..20 с) ==="
$secs = 0..20
$table = @()

foreach ($s in $secs) {
    $twSec = $allRecords | Where-Object { $_.App -eq "Tweaker" -and [math]::Round($_.Second) -eq $s }
    $blSec = $allRecords | Where-Object { $_.App -eq "Baseline" -and [math]::Round($_.Second) -eq $s }
    
    $twWsMb = [math]::Round((Get-Med ($twSec | ForEach-Object { $_.WorkingSet64 })) / 1MB, 2)
    $twPmMb = [math]::Round((Get-Med ($twSec | ForEach-Object { $_.PrivateMemorySize64 })) / 1MB, 2)
    $twPrivMb = [math]::Round((Get-Med ($twSec | ForEach-Object { $_.WorkingSetPrivate })) / 1MB, 2)
    $twHeapMb = [math]::Round((Get-Med ($twSec | ForEach-Object { $_.BytesInAllHeaps })) / 1MB, 2)
    
    $blWsMb = [math]::Round((Get-Med ($blSec | ForEach-Object { $_.WorkingSet64 })) / 1MB, 2)
    $blPmMb = [math]::Round((Get-Med ($blSec | ForEach-Object { $_.PrivateMemorySize64 })) / 1MB, 2)
    $blPrivMb = [math]::Round((Get-Med ($blSec | ForEach-Object { $_.WorkingSetPrivate })) / 1MB, 2)
    $blHeapMb = [math]::Round((Get-Med ($blSec | ForEach-Object { $_.BytesInAllHeaps })) / 1MB, 2)
    
    $table += [PSCustomObject]@{
        Second = $s
        "Tweaker_WS_MB" = $twWsMb
        "Tweaker_PM_MB" = $twPmMb
        "Tweaker_WSPriv_MB" = $twPrivMb
        "Baseline_WS_MB" = $blWsMb
        "Baseline_PM_MB" = $blPmMb
        "Baseline_WSPriv_MB" = $blPrivMb
    }
}

$table | Format-Table -AutoSize
