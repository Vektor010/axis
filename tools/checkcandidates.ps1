$tw = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$names = @(
    "AutoGameModeEnabled",
    "GpuYieldPercentage",
    "AllowAutoGameMode",
    "DisallowSystemAllowedCpuSets",
    "DisallowShaking",
    "WindowArrangementActive",
    "SnapAssist",
    "EnableSnapAssistFlyout",
    "EnableSnapBar",
    "EnableTaskGroups",
    "DITest",
    "NoWindowMinimizingShortcuts",
    "DisableDeleteNotification",
    "LongPathsEnabled",
    "NtfsDisableCompression",
    "NtfsDisableEncryption",
    "NtfsEncryptPagingFile",
    "NtfsMemoryUsage",
    "NtfsMftZoneReservation",
    "RefsDisableLastAccessUpdate",
    "DisableAccountNotifications",
    "NoUseStoreOpenWith",
    "DisableLockScreenAppNotifications",
    "NotifyPasswordReuse",
    "NoTileApplicationNotification",
    "NoToastApplicationNotification",
    "NoToastApplicationNotificationOnLockScreen",
    "NoCloudApplicationNotification",
    "DisallowNotificationMirroring",
    "AlwaysShowNotificationIcon",
    "EnableLegacyBalloonNotifications",
    "NoAutoTrayNotify",
    "TaskbarNoNotification",
    "NoBalloonFeatureAdvertisements"
)

$out = @()
foreach ($n in $names) {
    $found = $false
    $tid = ""
    foreach ($t in $tw) {
        if ($t.registry) {
            foreach ($r in $t.registry) {
                if ($r.name -eq $n) {
                    $found = $true
                    $tid = $t.id
                    break
                }
            }
        }
    }
    $out += [PSCustomObject]@{ Name=$n; Found=$found; TweakId=$tid }
}

$out | Format-Table -AutoSize
