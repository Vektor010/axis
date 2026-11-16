$tw = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$selected = $tw | Where-Object { $_.id -in @('power-remove-sleep-option', 'power-remove-hibernate-option', 'power-hide-lock-option') }
$selected | ConvertTo-Json -Depth 5
