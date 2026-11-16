$sec = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\security\desc.md'
$per = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\peripheral\desc.md'

Write-Host "Total sec lines: $($sec.Count)"
Write-Host "=== SEC 1105..1185 ==="
1105..1185 | ForEach-Object { "$($_): $($sec[$_ - 1])" }

Write-Host "Total per lines: $($per.Count)"
Write-Host "=== PER 1385..1465 ==="
1385..1465 | ForEach-Object { "$($_): $($per[$_ - 1])" }
