$p = 'C:\Users\User\Documents\win-tweaker\tests\measure.ps1'
$b = [System.IO.File]::ReadAllBytes($p)
$bom = [byte[]]@(0xEF, 0xBB, 0xBF)
[System.IO.File]::WriteAllBytes($p, $bom + $b)
Write-Host "BOM added to $p"
