# tools\list-tweaks.ps1
$repoRoot = Split-Path -Parent $PSScriptRoot
$tweaksJsonPath = Join-Path $repoRoot "data\tweaks.json"
$baseSourceDir = Join-Path $repoRoot "data\source\win-config"

$jsonText = Get-Content $tweaksJsonPath -Raw -Encoding UTF8
$tweaks = ConvertFrom-Json $jsonText

foreach ($t in $tweaks) {
    $srcFile = $t.source.file
    $from = [int]$t.source.line_from
    $to = [int]$t.source.line_to
    $applyBlock = $t.registry[0].apply_block
    $apply = $t.registry[0].apply
    $revertMode = $t.registry[0].revert.mode
    $count = $to - $from + 1
    
    $fullSrcPath = Join-Path $baseSourceDir ($srcFile -replace '/', '\')
    $srcLines = Get-Content $fullSrcPath
    $firstLine = ($srcLines[$from - 1]) -replace "`t", "<TAB>"
    $lastLine = ($srcLines[$to - 1]) -replace "`t", "<TAB>"
    
    Write-Output ("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9}" -f $t.id, $srcFile, $from, $to, $applyBlock, $apply, $revertMode, $count, $firstLine, $lastLine)
}
