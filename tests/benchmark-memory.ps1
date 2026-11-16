# tests\benchmark-memory.ps1
$baselineExe = "C:\Users\User\Documents\win-tweaker\Tests\baseline\bin\BaselineWindow.exe"
$tweakerExe = "C:\Users\User\Documents\win-tweaker\dist\Tweaker.exe"

function Measure-AppMemory($name, $exe, $argsList) {
    Write-Host "`n=================================================================="
    Write-Host "ЗАМЕР ПАМЯТИ: $name (5 прогонов по 10 с idle)"
    Write-Host "=================================================================="
    $results = @()
    for ($i = 1; $i -le 5; $i++) {
        $p = Start-Process -FilePath $exe -ArgumentList $argsList -PassThru
        # Ждём 10 секунд (окно отрендерилось и находится в idle)
        Start-Sleep -Seconds 10
        $p.Refresh()
        $ws = [math]::Round($p.WorkingSet64 / 1MB, 2)
        $pm = [math]::Round($p.PrivateMemorySize64 / 1MB, 2)
        $results += [PSCustomObject]@{
            Прогон = $i
            WorkingSetMb = $ws
            PrivateMemoryMb = $pm
        }
        Write-Host ("Прогон {0}: WorkingSet64 = {1,6:F2} MB | PrivateMemorySize64 = {2,6:F2} MB" -f $i, $ws, $pm)
        if (-not $p.HasExited) {
            $p.Kill()
            $p.WaitForExit()
        }
        Start-Sleep -Milliseconds 500
    }
    $sortedWs = $results | ForEach-Object { $_.WorkingSetMb } | Sort-Object
    $medWs = $sortedWs[2]
    $sortedPm = $results | ForEach-Object { $_.PrivateMemoryMb } | Sort-Object
    $medPm = $sortedPm[2]
    Write-Host "------------------------------------------------------------------"
    Write-Host ("МЕДИАНА : WorkingSet64 = {0,6:F2} MB | PrivateMemorySize64 = {1,6:F2} MB" -f $medWs, $medPm) -ForegroundColor Green
    return @{
        Results = $results
        MedianWs = $medWs
        MedianPm = $medPm
    }
}

$memBaseline = Measure-AppMemory "Baseline" $baselineExe @("--idle-benchmark")
$memTweakerNoOpt = Measure-AppMemory "Tweaker без ProfileOptimization" $tweakerExe @("--idle-benchmark", "--no-profile-opt")
$memTweakerOpt = Measure-AppMemory "Tweaker с ProfileOptimization" $tweakerExe @("--idle-benchmark")

Write-Host "`n=================================================================="
Write-Host "ИТОГОВОЕ СРАВНЕНИЕ МЕДИАН ПАМЯТИ"
Write-Host "=================================================================="
$memSummary = @(
    [PSCustomObject]@{ Конфигурация = "Baseline"; WorkingSetMb = $memBaseline.MedianWs; PrivateMemoryMb = $memBaseline.MedianPm; Цель_100MB = $(if ($memBaseline.MedianWs -le 100) { "ДА" } else { "НЕТ" }) },
    [PSCustomObject]@{ Конфигурация = "Tweaker без ProfileOpt"; WorkingSetMb = $memTweakerNoOpt.MedianWs; PrivateMemoryMb = $memTweakerNoOpt.MedianPm; Цель_100MB = $(if ($memTweakerNoOpt.MedianWs -le 100) { "ДА" } else { "НЕТ" }) },
    [PSCustomObject]@{ Конфигурация = "Tweaker с ProfileOpt"; WorkingSetMb = $memTweakerOpt.MedianWs; PrivateMemoryMb = $memTweakerOpt.MedianPm; Цель_100MB = $(if ($memTweakerOpt.MedianWs -le 100) { "ДА" } else { "НЕТ" }) }
)
$memSummary | Format-Table -AutoSize
