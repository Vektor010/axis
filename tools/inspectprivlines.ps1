$priv = Get-Content 'C:\Users\User\Documents\win-tweaker\data\source\win-config\privacy\desc.md'

Write-Host "=== PRIV 124..140 ==="
124..140 | ForEach-Object { "$($_): $($priv[$_ - 1])" }

Write-Host "=== PRIV 444..455 ==="
444..455 | ForEach-Object { "$($_): $($priv[$_ - 1])" }
