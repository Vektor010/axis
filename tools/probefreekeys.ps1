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
    "HKLM\Software\Policies\Microsoft\Windows\LLTD\AllowLLTDIOOnPublicNet",
    "HKLM\Software\Policies\Microsoft\Windows\LLTD\ProhibitLLTDIOOnPrivateNet",
    "HKLM\Software\Policies\Microsoft\Windows\LLTD\AllowRspndrOnPublicNet",
    "HKLM\Software\Policies\Microsoft\Windows\LLTD\ProhibitRspndrOnPrivateNet",
    "HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\DoNotConnectToWindowsUpdateInternetLocations",
    "HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\AllowTemporaryEnterpriseFeatureControl",
    "HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\ExcludeWUDriversInQualityUpdate",
    "HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\SetDisableUXWUAccess",
    "HKLM\Software\Policies\Microsoft\Windows\WindowsUpdate\SetDisablePauseUXAccess",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\ClickForceSensitivity",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\TapAndDrag",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\RightClickZoneEnabled",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\PanEnabled",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\ScrollDirection",
    "HKCU\Software\Microsoft\Wisp\Pen\SysEventParameters\Latency",
    "HKCU\Software\Microsoft\Wisp\Pen\SysEventParameters\UseHWTimeStamp",
    "HKLM\SOFTWARE\Policies\Microsoft\PassportForWork\DynamicLock\DynamicLock",
    "HKCU\Software\Policies\Microsoft\Windows\Explorer\NoAutoTrayNotify",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\TaskbarNoNotification",
    "HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications\EnableExpandedToastNotifications"
)

foreach ($p in $probe) {
    $k = $p.ToLowerInvariant()
    if ($existingKeys[$k]) {
        Write-Output "USED: $p (by $($existingKeys[$k]))"
    } else {
        Write-Output "FREE: $p"
    }
}
