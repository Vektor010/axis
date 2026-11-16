# 5 runs of Baseline
$results = @()
for ($i = 1; $i -le 5; $i++) {
    Remove-Item "$env:TEMP\baseline_first_render.txt" -ErrorAction SilentlyContinue
    $p = Start-Process -FilePath "C:\Users\User\Documents\win-tweaker\tests\baseline\bin\BaselineWindow.exe" -PassThru -Wait
    Start-Sleep -Milliseconds 300
    if (Test-Path "$env:TEMP\baseline_first_render.txt") {
        $lines = Get-Content "$env:TEMP\baseline_first_render.txt"
        $preMain = ($lines | Where-Object { $_ -match '^PreMainMs=(.*)' } | ForEach-Object { $matches[1] }) -replace ',', '.'
        $mainToRender = ($lines | Where-Object { $_ -match '^MainToRenderMs=(.*)' } | ForEach-Object { $matches[1] }) -replace ',', '.'
        $total = ($lines | Where-Object { $_ -match '^TotalMs=(.*)' } | ForEach-Object { $matches[1] }) -replace ',', '.'
        $results += [PSCustomObject]@{
            Run = $i
            PreMainMs = [double]$preMain
            MainToRenderMs = [double]$mainToRender
            TotalMs = [double]$total
        }
    }
}
$results | Format-Table -AutoSize
$preMainSorted = $results.PreMainMs | Sort-Object
$mainToRenderSorted = $results.MainToRenderMs | Sort-Object
$totalSorted = $results.TotalMs | Sort-Object

Write-Host "Медиана PreMainMs: $($preMainSorted[2]) ms"
Write-Host "Медиана MainToRenderMs: $($mainToRenderSorted[2]) ms"
Write-Host "Медиана TotalMs: $($totalSorted[2]) ms"
