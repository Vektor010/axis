$raw = git -C "C:\Users\User\Documents\win-tweaker" show ad8afe0e:data/tweaks.json | ConvertFrom-Json
$ids = @{}
foreach ($t in $raw) { $ids[$t.id] = $true }

Write-Host "system-win32-priority-separation in base: $($ids.ContainsKey('system-win32-priority-separation'))"
Write-Host "peripheral-disable-mouse-acceleration in base: $($ids.ContainsKey('peripheral-disable-mouse-acceleration'))"
Write-Host "network-congestion-provider-cubic in base: $($ids.ContainsKey('network-congestion-provider-cubic'))"
