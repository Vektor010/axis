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
    # 1. system-storage-sense-recycle-bin-threshold
    @{ id = "system-storage-sense-recycle-bin-threshold"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\StorageSense"; name = "ConfigStorageSenseRecycleBinCleanupThreshold" },
    # 2. system-storage-sense-downloads-threshold
    @{ id = "system-storage-sense-downloads-threshold"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\StorageSense"; name = "ConfigStorageSenseDownloadsCleanupThreshold" },
    # 3. security-wtds-notify-password-reuse
    @{ id = "security-wtds-notify-password-reuse"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WTDS\Components"; name = "NotifyPasswordReuse" },
    # 4. security-wtds-notify-unsafe-app
    @{ id = "security-wtds-notify-unsafe-app"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WTDS\Components"; name = "NotifyUnsafeApp" },
    # 5. network-sandbox-no-printer-redirection
    @{ id = "network-sandbox-no-printer-redirection"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Sandbox"; name = "AllowPrinterRedirection" },
    # 6. security-powershell-disable-module-logging
    @{ id = "security-powershell-disable-module-logging"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging"; name = "EnableModuleLogging" },
    # 7. security-powershell-disable-scriptblock-logging
    @{ id = "security-powershell-disable-scriptblock-logging"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging"; name = "EnableScriptBlockLogging" },
    # 8. security-powershell-disable-transcription
    @{ id = "security-powershell-disable-transcription"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\PowerShell\Transcription"; name = "EnableTranscripting" },
    # 9. security-edge-disable-smartscreen-filter
    @{ id = "security-edge-disable-smartscreen-filter"; hive = "HKCU"; path = "Software\Policies\Microsoft\MicrosoftEdge\PhishingFilter"; name = "EnabledV9" },
    # 10. security-edge-chromium-smartscreen-off
    @{ id = "security-edge-chromium-smartscreen-off"; hive = "HKCU"; path = "Software\Policies\Microsoft\Edge"; name = "SmartScreenEnabled" },
    # 11. security-disable-wtds-service
    @{ id = "security-disable-wtds-service"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WTDS\Components"; name = "ServiceEnabled" },
    # 12. system-start-hide-recently-added-apps
    @{ id = "system-start-hide-recently-added-apps"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "HideRecentlyAddedApps" },
    # 13. system-ntfs-disable-compression
    @{ id = "system-ntfs-disable-compression"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsDisableCompression" },
    # 14. system-ntfs-disable-encryption
    @{ id = "system-ntfs-disable-encryption"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsDisableEncryption" },
    # 15. system-start-no-new-app-alert
    @{ id = "system-start-no-new-app-alert"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "NoNewAppAlert" },
    # 16. system-start-hide-most-used-apps
    @{ id = "system-start-hide-most-used-apps"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "ShowOrHideMostUsedApps" },
    # 17. system-start-no-mfu-programs-list
    @{ id = "system-start-no-mfu-programs-list"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name = "NoStartMenuMFUprogramsList" },
    # 18. privacy-paint-disable-image-creator
    @{ id = "privacy-paint-disable-image-creator"; hive = "HKLM"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Paint"; name = "DisableImageCreator" },
    # 19. privacy-paint-disable-cocreator
    @{ id = "privacy-paint-disable-cocreator"; hive = "HKLM"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Paint"; name = "DisableCocreator" },
    # 20. privacy-paint-disable-generative-fill
    @{ id = "privacy-paint-disable-generative-fill"; hive = "HKLM"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Paint"; name = "DisableGenerativeFill" },
    # 21. privacy-appcompat-turn-off-switchback
    @{ id = "privacy-appcompat-turn-off-switchback"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "SbEnable" },
    # 22. peripheral-touchpad-leave-on-with-mouse
    @{ id = "peripheral-touchpad-leave-on-with-mouse"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "LeaveOnWithMouse" },
    # 23. peripheral-touchpad-disable-taps
    @{ id = "peripheral-touchpad-disable-taps"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "TapsEnabled" },
    # 24. peripheral-touchpad-disable-two-finger-tap
    @{ id = "peripheral-touchpad-disable-two-finger-tap"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "TwoFingerTapEnabled" },
    # 25. peripheral-touchpad-disable-pinch-zoom
    @{ id = "peripheral-touchpad-disable-pinch-zoom"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "ZoomEnabled" }
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
