# tests\benchmark-modes.ps1
$baselineExe = "C:\Users\User\Documents\win-tweaker\Tests\baseline\bin\BaselineWindow.exe"
$tweakerExe = "C:\Users\User\Documents\win-tweaker\dist\Tweaker.exe"
$tempDir = [System.IO.Path]::GetTempPath()
$bMetrics = Join-Path $tempDir "baseline_first_render.txt"
$tMetrics = Join-Path $tempDir "tweaker_first_render.txt"
$tweakerProfile = Join-Path $env:LOCALAPPDATA "Tweaker\Startup.profile"
$baselineProfileDir = Join-Path $tempDir "BaselineProfile"

function Get-Median($arr) {
    $sorted = $arr | Sort-Object
    $count = $sorted.Count
    if ($count % 2 -eq 1) {
        return $sorted[[Math]::Floor($count / 2)]
    } else {
        return ($sorted[$count / 2 - 1] + $sorted[$count / 2]) / 2.0
    }
}

function Parse-Metrics($file) {
    $content = Get-Content $file -Raw
    $preStr = [regex]::Match($content, "PreMainMs=([0-9.,]+)").Groups[1].Value
    $mainStr = [regex]::Match($content, "MainToRenderMs=([0-9.,]+)").Groups[1].Value
    $totalStr = [regex]::Match($content, "TotalMs=([0-9.,]+)").Groups[1].Value
    return @{
        PreMainMs = [double]($preStr -replace ',', '.')
        MainToRenderMs = [double]($mainStr -replace ',', '.')
        TotalMs = [double]($totalStr -replace ',', '.')
    }
}

function Run-Mode($modeName, $title, [scriptblock]$action) {
    Write-Host "`n=================================================================="
    Write-Host ("РЕЖИМ {0}: {1}" -f $modeName, $title)
    Write-Host "=================================================================="
    $results = @()
    for ($i = 1; $i -le 5; $i++) {
        & $action
        Start-Sleep -Milliseconds 200
        $m = $null
        if ($modeName -like "а*" -or $modeName -like "б*") {
            $m = Parse-Metrics $bMetrics
        } else {
            $m = Parse-Metrics $tMetrics
        }
        $results += [PSCustomObject]@{
            Прогон = $i
            PreMainMs = [math]::Round($m.PreMainMs, 2)
            MainToRenderMs = [math]::Round($m.MainToRenderMs, 2)
            TotalMs = [math]::Round($m.TotalMs, 2)
        }
        Write-Host ("Прогон {0}: PreMainMs = {1,7:F2} | MainToRenderMs = {2,7:F2} | TotalMs = {3,7:F2}" -f $i, $m.PreMainMs, $m.MainToRenderMs, $m.TotalMs)
    }
    $medPre = Get-Median ($results | ForEach-Object { $_.PreMainMs })
    $medMain = Get-Median ($results | ForEach-Object { $_.MainToRenderMs })
    $medTotal = Get-Median ($results | ForEach-Object { $_.TotalMs })
    Write-Host "------------------------------------------------------------------"
    Write-Host ("МЕДИАНА : PreMainMs = {0,7:F2} | MainToRenderMs = {1,7:F2} | TotalMs = {2,7:F2}" -f $medPre, $medMain, $medTotal) -ForegroundColor Green
    return @{
        Results = $results
        MedianPre = $medPre
        MedianMain = $medMain
        MedianTotal = $medTotal
    }
}

# Режим а: Baseline без ProfileOptimization
$resA = Run-Mode "а" "Baseline без ProfileOptimization" {
    Start-Process -FilePath $baselineExe -Wait -PassThru | Out-Null
}

# Режим б: Baseline с ProfileOptimization (1 прогон создания профиля, 5 замеряемых)
Remove-Item $baselineProfileDir -Recurse -Force -ErrorAction SilentlyContinue
Start-Process -FilePath $baselineExe -ArgumentList @("--profile-opt") -Wait -PassThru | Out-Null
Start-Sleep -Milliseconds 300
$resB = Run-Mode "б" "Baseline с ProfileOptimization (существующий профиль)" {
    Start-Process -FilePath $baselineExe -ArgumentList @("--profile-opt") -Wait -PassThru | Out-Null
}

# Режим в: Tweaker без ProfileOptimization
$resV = Run-Mode "в" "Tweaker без ProfileOptimization (--no-profile-opt)" {
    Start-Process -FilePath $tweakerExe -ArgumentList @("--benchmark", "--no-profile-opt") -Wait -PassThru | Out-Null
}

# Режим г: Tweaker первый запуск с ProfileOptimization (удаление профиля перед КАЖДЫМ прогоном)
$resG = Run-Mode "г" "Tweaker первый запуск с ProfileOptimization (удаление профиля перед каждым прогоном)" {
    Remove-Item $tweakerProfile -Force -ErrorAction SilentlyContinue
    Start-Process -FilePath $tweakerExe -ArgumentList @("--benchmark") -Wait -PassThru | Out-Null
}

# Режим д: Tweaker с ProfileOptimization и существующим профилем
# Разогрев (создание профиля)
Start-Process -FilePath $tweakerExe -ArgumentList @("--benchmark") -Wait -PassThru | Out-Null
Start-Sleep -Milliseconds 300
$resD = Run-Mode "д" "Tweaker с ProfileOptimization и существующим профилем" {
    Start-Process -FilePath $tweakerExe -ArgumentList @("--benchmark") -Wait -PassThru | Out-Null
}

Write-Host "`n=================================================================="
Write-Host "ИТОГОВОЕ СРАВНЕНИЕ МЕДИАН ВСЕХ 5 РЕЖИМОВ"
Write-Host "=================================================================="
$summary = @(
    [PSCustomObject]@{ Режим = "а) Baseline без ProfileOpt"; PreMainMs = $resA.MedianPre; MainToRenderMs = $resA.MedianMain; TotalMs = $resA.MedianTotal },
    [PSCustomObject]@{ Режим = "б) Baseline с ProfileOpt";   PreMainMs = $resB.MedianPre; MainToRenderMs = $resB.MedianMain; TotalMs = $resB.MedianTotal },
    [PSCustomObject]@{ Режим = "в) Tweaker без ProfileOpt";  PreMainMs = $resV.MedianPre; MainToRenderMs = $resV.MedianMain; TotalMs = $resV.MedianTotal },
    [PSCustomObject]@{ Режим = "г) Tweaker 1-й запуск (без профиля)"; PreMainMs = $resG.MedianPre; MainToRenderMs = $resG.MedianMain; TotalMs = $resG.MedianTotal },
    [PSCustomObject]@{ Режим = "д) Tweaker повторный (с профилем)";   PreMainMs = $resD.MedianPre; MainToRenderMs = $resD.MedianMain; TotalMs = $resD.MedianTotal }
)
$summary | Format-Table -AutoSize
