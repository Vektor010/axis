$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json

$revertModes = $tweaks | ForEach-Object { $_.actions.registry } | ForEach-Object { $_.revert.mode } | Group-Object | Select-Object Name, Count
Write-Host "Revert modes distribution:"
$revertModes | Format-Table -AutoSize
