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
    # 1. visibility-disable-widgets
    @{ id = "visibility-disable-widgets"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Dsh"; name = "AllowNewsAndInterests" },
    # 2. visibility-disable-widgets-lockscreen
    @{ id = "visibility-disable-widgets-lockscreen"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Dsh"; name = "DisableWidgetsOnLockScreen" },
    # 3. visibility-disable-widgets-board
    @{ id = "visibility-disable-widgets-board"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Dsh"; name = "DisableWidgetsBoard" },
    # 4. visibility-hide-people-bar
    @{ id = "visibility-hide-people-bar"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "HidePeopleBar" },
    # 5. visibility-hide-task-view-button
    @{ id = "visibility-hide-task-view-button"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "HideTaskViewButton" },
    # 6. power-remove-sleep-option
    @{ id = "power-remove-sleep-option"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "ShowSleepOption" },
    # 7. power-remove-hibernate-option
    @{ id = "power-remove-hibernate-option"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "ShowHibernateOption" },
    # 8. visibility-disable-acrylic-logon-bg
    @{ id = "visibility-disable-acrylic-logon-bg"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "DisableAcrylicBackgroundOnLogon" },
    # 9. visibility-prevent-lockscreen-slideshow
    @{ id = "visibility-prevent-lockscreen-slideshow"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Personalization"; name = "NoLockScreenSlideshow" },
    # 10. visibility-prevent-lockscreen-camera
    @{ id = "visibility-prevent-lockscreen-camera"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Personalization"; name = "NoLockScreenCamera" },
    # 11. visibility-no-recent-docs-history
    @{ id = "visibility-no-recent-docs-history"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name = "NoRecentDocsHistory" },
    # 12. security-prevent-device-metadata
    @{ id = "security-prevent-device-metadata"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Device Metadata"; name = "PreventDeviceMetadataFromNetwork" },
    # 13. security-disable-root-auto-update
    @{ id = "security-disable-root-auto-update"; hive = "HKLM"; path = "Software\Policies\Microsoft\SystemCertificates\AuthRoot"; name = "DisableRootAutoUpdate" },
    # 14. security-driver-search-order
    @{ id = "security-driver-search-order"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\DriverSearching"; name = "SearchOrderConfig" },
    # 15. security-removable-disks-deny-write
    @{ id = "security-removable-disks-deny-write"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\RemovableStorageDevices\{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}"; name = "Deny_Write" },
    # 16. system-notifications-no-use-store
    @{ id = "system-notifications-no-use-store"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "NoUseStoreOpenWith" },
    # 17. system-notifications-disable-lockscreen
    @{ id = "system-notifications-disable-lockscreen"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "DisableLockScreenAppNotifications" },
    # 18. system-notifications-disable-enhanced
    @{ id = "system-notifications-disable-enhanced"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows Defender\Reporting"; name = "DisableEnhancedNotifications" },
    # 19. system-notifications-no-tile
    @{ id = "system-notifications-no-tile"; hive = "HKCU"; path = "SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name = "NoTileApplicationNotification" },
    # 20. system-notifications-no-tray-cleanup
    @{ id = "system-notifications-no-tray-cleanup"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "NoAutoTrayNotify" },
    # 21. system-notifications-turn-off-balloons
    @{ id = "system-notifications-turn-off-balloons"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name = "TaskbarNoNotification" },
    # 22. system-notifications-no-feature-ads
    @{ id = "system-notifications-no-feature-ads"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "NoBalloonFeatureAdvertisements" },
    # 23. system-search-disable-removable-drive-indexing
    @{ id = "system-search-disable-removable-drive-indexing"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name = "DisableRemovableDriveIndexing" },
    # 24. peripheral-printers-disable-webpnp-download
    @{ id = "peripheral-printers-disable-webpnp-download"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows NT\Printers"; name = "DisableWebPnPDownload" },
    # 25. peripheral-printers-legacy-default-mode
    @{ id = "peripheral-printers-legacy-default-mode"; hive = "HKCU"; path = "Software\Microsoft\Windows NT\CurrentVersion\Windows"; name = "LegacyDefaultPrinterMode" }
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
