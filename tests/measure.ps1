param(
    [string]$TargetExe = "dist\Tweaker.exe",
    [string]$Description = "Замер"
)

$rootDir = Split-Path -Parent $PSScriptRoot
Set-Location $rootDir
$exePath = Join-Path $rootDir $TargetExe
$metricsFile = Join-Path $env:TEMP "tweaker_first_render.txt"

$runs = @()
for ($i = 1; $i -le 5; $i++) {
    Remove-Item $metricsFile -ErrorAction SilentlyContinue
    $p = Start-Process -FilePath $exePath -ArgumentList @("--benchmark") -PassThru -Wait
    Start-Sleep -Milliseconds 300
    if (Test-Path $metricsFile) {
        $lines = Get-Content $metricsFile
        $preMain = ($lines | Where-Object { $_ -match '^PreMainMs=(.*)' } | ForEach-Object { $matches[1] }) -replace ',', '.'
        $mainToRender = ($lines | Where-Object { $_ -match '^MainToRenderMs=(.*)' } | ForEach-Object { $matches[1] }) -replace ',', '.'
        $total = ($lines | Where-Object { $_ -match '^TotalMs=(.*)' } | ForEach-Object { $matches[1] }) -replace ',', '.'
        $runs += [PSCustomObject]@{
            Run = $i
            PreMainMs = [double]$preMain
            MainToRenderMs = [double]$mainToRender
            TotalMs = [double]$total
        }
    }
}

Write-Host "Результаты 5 прогонов [$Description]:"
$runs | Format-Table -AutoSize

$preSorted = $runs.PreMainMs | Sort-Object
$renderSorted = $runs.MainToRenderMs | Sort-Object
$totalSorted = $runs.TotalMs | Sort-Object

$medPre = $preSorted[2]
$medRender = $renderSorted[2]
$medTotal = $totalSorted[2]

Write-Host ("Медиана PreMainMs: {0:F2} ms | MainToRenderMs: {1:F2} ms | TotalMs: {2:F2} ms" -f $medPre, $medRender, $medTotal)
