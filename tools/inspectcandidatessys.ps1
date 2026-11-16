$sys = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md'

Write-Host "Total sys lines: $($sys.Count)"
Write-Host "=== SYS 8660..8690 ==="
8660..8690 | ForEach-Object { "$($_): $($sys[$_ - 1])" }

Write-Host "=== SYS 9140..9170 ==="
9140..9170 | ForEach-Object { "$($_): $($sys[$_ - 1])" }

Write-Host "=== SYS 9400..9460 ==="
9400..9460 | ForEach-Object { "$($_): $($sys[$_ - 1])" }
