param(
    [string]$TweaksJson = "data\tweaks.json",
    [string]$BaseDir = "data\source\win-config"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
Set-Location $rootDir

$tweaks = Get-Content $TweaksJson -Raw -Encoding UTF8 | ConvertFrom-Json

foreach ($t in $tweaks) {
    $filePath = Join-Path $BaseDir ($t.source.file -replace '/', '\')
    $from = [int]$t.source.line_from
    $to = [int]$t.source.line_to
    $skip = $from - 1
    $count = $to - $from + 1

    Write-Host ("`n==================================================================")
    Write-Host ("SOURCE DUMP: {0} | File: {1} | Lines: {2}..{3}" -f $t.id, $t.source.file, $from, $to)
    Write-Host ("==================================================================")

    $lines = Get-Content $filePath | Select-Object -Skip $skip -First $count
    $lineNum = $from
    $dataCount = 0
    $nameFound = $false
    $targetName = $t.registry[0].name

    foreach ($line in $lines) {
        Write-Host ('{0}: {1}' -f $lineNum++, $line)
        if ($line -match 'Data:\s*') {
            $dataCount++
        }
        if ($line -match [regex]::Escape($targetName)) {
            $nameFound = $true
        }
    }

    if ($dataCount -ne 2) {
        Write-Host ("ОШИБКА: пар Data не 2 (найдено: {0})" -f $dataCount)
    }
    if (-not $nameFound) {
        Write-Host ("ОШИБКА: имя параметра '{0}' не найдено в диапазоне" -f $targetName)
    }

    $applyVal = $t.registry[0].apply
    $revertMode = $t.registry[0].revert.mode
    $revertVal = if ($revertMode -eq "snapshot") { "snapshot" } else { $t.registry[0].revert.value }
    $totalLines = $count

    Write-Host ("------------------------------------------------------------------")
    Write-Host ("{0} | {1} | apply: {2} | revert.mode: {3} | {4} | Строк в диапазоне: {5}" -f $t.id, $t.name_ru, $applyVal, $revertMode, $revertVal, $totalLines)
}
