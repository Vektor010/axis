$tweaks = Get-Content -Raw "C:\Users\User\Documents\win-tweaker\data\tweaks.json" | ConvertFrom-Json

$existingIds = @{}
$existingKeys = @{}

foreach ($t in $tweaks) {
    $existingIds[$t.id] = $true
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $key = ($r.hive + "\" + $r.path + "\" + $r.name).ToLowerInvariant()
            $existingKeys[$key] = $t.id
        }
    }
}

$candidates = @(
    # 1. privacy-appcompat-disable-api-sampling
    @{ id = "privacy-appcompat-disable-api-sampling"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "DisableAPISamping" },
    # 2. privacy-appcompat-disable-app-footprint
    @{ id = "privacy-appcompat-disable-app-footprint"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "DisableApplicationFootprint" },
    # 3. privacy-appcompat-disable-win32-backup
    @{ id = "privacy-appcompat-disable-win32-backup"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "DisableWin32AppBackup" },
    # 4. privacy-disable-onesettings-auditing
    @{ id = "privacy-disable-onesettings-auditing"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\DataCollection"; name = "EnableOneSettingsAuditing" },
    # 5. privacy-video-playback-allow-low-resolution
    @{ id = "privacy-video-playback-allow-low-resolution"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\VideoSettings"; name = "AllowLowResolution" },
    # 6. privacy-video-playback-disable-auto-enhance
    @{ id = "privacy-video-playback-disable-auto-enhance"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\VideoSettings"; name = "EnableAutoEnhanceDuringPlayback" },
    # 7. security-sudo-disable-normal-mode
    @{ id = "security-sudo-disable-normal-mode"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Sudo"; name = "Enabled" },
    # 8. security-camera-osd-indicator
    @{ id = "security-camera-osd-indicator"; hive = "HKLM"; path = "SOFTWARE\Microsoft\OEM\Device\Capture"; name = "NoPhysicalCameraLED" },
    # 9. security-wu-exclude-drivers-in-quality-updates
    @{ id = "security-wu-exclude-drivers-in-quality-updates"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WindowsUpdate"; name = "ExcludeWUDriversInQualityUpdate" },
    # 10. security-wu-disable-pause-updates-ux
    @{ id = "security-wu-disable-pause-updates-ux"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WindowsUpdate"; name = "SetDisablePauseUXAccess" },
    # 11. security-wu-disable-internet-locations
    @{ id = "security-wu-disable-internet-locations"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WindowsUpdate"; name = "DoNotConnectToWindowsUpdateInternetLocations" },
    # 12. security-wu-allow-servicing-enterprise-features
    @{ id = "security-wu-allow-servicing-enterprise-features"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WindowsUpdate"; name = "AllowTemporaryEnterpriseFeatureControl" },
    # 13. security-passport-dynamic-lock-factors
    @{ id = "security-passport-dynamic-lock-factors"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\PassportForWork\DynamicLock"; name = "DynamicLock" },
    # 14. system-notifications-lock-screen-toast-off
    @{ id = "system-notifications-lock-screen-toast-off"; hive = "HKCU"; path = "SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name = "NoToastApplicationNotificationOnLockScreen" },
    # 15. system-notifications-turn-off-cloud
    @{ id = "system-notifications-turn-off-cloud"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name = "NoCloudApplicationNotification" },
    # 16. system-notifications-disallow-mirroring
    @{ id = "system-notifications-disallow-mirroring"; hive = "HKCU"; path = "SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name = "DisallowNotificationMirroring" },
    # 17. system-notifications-always-show-bell-icon
    @{ id = "system-notifications-always-show-bell-icon"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "AlwaysShowNotificationIcon" },
    # 18. system-notifications-enable-legacy-balloons
    @{ id = "system-notifications-enable-legacy-balloons"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "EnableLegacyBalloonNotifications" },
    # 19. system-notifications-expanded-toast-action-center
    @{ id = "system-notifications-expanded-toast-action-center"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name = "EnableExpandedToastNotifications" },
    # 20. network-lltd-allow-io-public-net
    @{ id = "network-lltd-allow-io-public-net"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\LLTD"; name = "AllowLLTDIOOnPublicNet" },
    # 21. network-lltd-prohibit-io-private-net
    @{ id = "network-lltd-prohibit-io-private-net"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\LLTD"; name = "ProhibitLLTDIOOnPrivateNet" },
    # 22. network-lltd-prohibit-rspndr-private-net
    @{ id = "network-lltd-prohibit-rspndr-private-net"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\LLTD"; name = "ProhibitRspndrOnPrivateNet" },
    # 23. peripheral-touchpad-aap-threshold-low
    @{ id = "peripheral-touchpad-aap-threshold-low"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "AAPThreshold" },
    # 24. peripheral-touchpad-feedback-intensity-max
    @{ id = "peripheral-touchpad-feedback-intensity-max"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "FeedbackIntensity" },
    # 25. peripheral-touchpad-honor-mouse-accel
    @{ id = "peripheral-touchpad-honor-mouse-accel"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "HonorMouseAccelSetting" }
)

Write-Output "Checking $($candidates.Count) candidates..."
$foundIssues = 0
foreach ($c in $candidates) {
    if ($existingIds[$c.id]) {
        Write-Output "[ID Collision] $($c.id) already exists!"
        $foundIssues++
    }
    $k = ($c.hive + "\" + $c.path + "\" + $c.name).ToLowerInvariant()
    if ($existingKeys[$k]) {
        Write-Output "[Registry Collision] $k is already used by $($existingKeys[$k])!"
        $foundIssues++
    }
}

if ($foundIssues -eq 0) {
    Write-Output "ALL 25 CANDIDATES ARE 100% COLLISION-FREE!"
} else {
    Write-Output "Found $foundIssues collisions."
}
