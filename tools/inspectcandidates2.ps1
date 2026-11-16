$per = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\peripheral\desc.md'
$pow = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\power\desc.md'
$sys = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md'
$vis = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\visibility\desc.md'

Write-Host "=== PER 1535..1560 ==="
1535..1560 | ForEach-Object { "$($_): $($per[$_ - 1])" }

Write-Host "=== POW 2360..2410 ==="
2360..2410 | ForEach-Object { "$($_): $($pow[$_ - 1])" }

Write-Host "=== VIS 1350..1400 ==="
1350..1400 | ForEach-Object { "$($_): $($vis[$_ - 1])" }
