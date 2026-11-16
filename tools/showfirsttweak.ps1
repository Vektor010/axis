$t = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$t[0] | ConvertTo-Json -Depth 5
