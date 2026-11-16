$t = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$t | Group-Object category | Select-Object Name, Count | Sort-Object Count -Descending | Format-Table -AutoSize
