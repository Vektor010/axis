$tweaks = Get-Content -Raw "C:\Users\User\Documents\win-tweaker\data\tweaks.json" | ConvertFrom-Json
$existingKeys = @{}
foreach ($t in $tweaks) {
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $key = ($r.hive + "\" + $r.path + "\" + $r.name).ToLowerInvariant()
            $existingKeys[$key] = $t.id
        }
    }
}

$probe = @(
    "HKCU\SOFTWARE\Policies\Microsoft\Windows\DWM\DisallowAnimations",
    "HKCU\Control Panel\Desktop\WindowMetrics\MinAnimate",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarAnimations",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\TurnOffSPIAnimations",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects\VisualFXSetting"
)

foreach ($p in $probe) {
    $k = $p.ToLowerInvariant()
    if ($existingKeys[$k]) {
        Write-Host "USED: $p (by $($existingKeys[$k]))"
    } else {
        Write-Host "FREE: $p"
    }
}
