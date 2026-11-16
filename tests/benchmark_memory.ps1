$rootDir = Split-Path -Parent $PSScriptRoot
Set-Location $rootDir
$exePath = Join-Path $rootDir "dist\Tweaker.exe"
$memFile = Join-Path $env:TEMP "tweaker_memory.txt"

$results = @()
Write-Host "Запуск 5 прогонов замера памяти (10 с простоя каждый)..."

for ($i = 1; $i -le 5; $i++) {
    Remove-Item $memFile -ErrorAction SilentlyContinue
    Write-Host "Прогон $i / 5..."
    $p = Start-Process -FilePath $exePath -ArgumentList @("--idle-benchmark") -PassThru -Wait
    Start-Sleep -Milliseconds 500
    if (Test-Path $memFile) {
        $lines = Get-Content $memFile
        $ws = ($lines | Where-Object { $_ -match '^WorkingSet64=(.*)' } | ForEach-Object { $matches[1] })
        $pm = ($lines | Where-Object { $_ -match '^PrivateMemorySize64=(.*)' } | ForEach-Object { $matches[1] })
        $wsp = ($lines | Where-Object { $_ -match '^WorkingSetPrivate=(.*)' } | ForEach-Object { $matches[1] })

        $wsMb = [math]::Round([double]$ws / 1MB, 2)
        $pmMb = [math]::Round([double]$pm / 1MB, 2)
        $wspMb = [math]::Round([double]$wsp / 1MB, 2)

        $results += [PSCustomObject]@{
            Run = $i
            WorkingSet64_MB = $wsMb
            PrivateMemorySize64_MB = $pmMb
            WorkingSetPrivate_MB = $wspMb
        }
    }
}

$results | Format-Table -AutoSize

$wsSorted = $results.WorkingSet64_MB | Sort-Object
$pmSorted = $results.PrivateMemorySize64_MB | Sort-Object
$wspSorted = $results.WorkingSetPrivate_MB | Sort-Object

Write-Host ("Медиана WorkingSet64: {0} МБ" -f $wsSorted[2])
Write-Host ("Медиана PrivateMemorySize64: {0} МБ" -f $pmSorted[2])
Write-Host ("Медиана WorkingSetPrivate: {0} МБ (справочно)" -f $wspSorted[2])
