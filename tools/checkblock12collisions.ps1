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
    # 1. privacy-disable-onesettings-downloads
    @{ id = "privacy-disable-onesettings-downloads"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\DataCollection"; name = "DisableOneSettingsDownloads" },
    # 2. privacy-disable-find-my-device
    @{ id = "privacy-disable-find-my-device"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\FindMyDevice"; name = "AllowFindMyDevice" },
    # 3. privacy-disable-font-providers
    @{ id = "privacy-disable-font-providers"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "EnableFontProviders" },
    # 4. privacy-disable-thumbnail-cache-network
    @{ id = "privacy-disable-thumbnail-cache-network"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "DisableThumbsDBOnNetworkFolders" },
    # 5. privacy-disable-thumbnail-cache-local
    @{ id = "privacy-disable-thumbnail-cache-local"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name = "NoThumbnailCache" },
    # 6. privacy-disable-app-compat-engine
    @{ id = "privacy-disable-app-compat-engine"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "DisableEngine" },
    # 7. privacy-disable-program-compat-assistant
    @{ id = "privacy-disable-program-compat-assistant"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "DisablePCA" },
    # 8. privacy-disable-compat-ui
    @{ id = "privacy-disable-compat-ui"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "DisablePcaUI" },
    # 9. privacy-disable-install-tracing
    @{ id = "privacy-disable-install-tracing"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppCompat"; name = "DisableInstallTracing" },
    # 10. privacy-disable-file-history
    @{ id = "privacy-disable-file-history"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\FileHistory"; name = "Disabled" },
    # 11. privacy-disable-search-history
    @{ id = "privacy-disable-search-history"; hive = "HKCU"; path = "SOFTWARE\Policies\Microsoft\Windows\Explorer"; name = "DisableSearchHistory" },
    # 12. privacy-disable-publish-user-activities
    @{ id = "privacy-disable-publish-user-activities"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "PublishUserActivities" },
    # 13. privacy-disable-upload-user-activities
    @{ id = "privacy-disable-upload-user-activities"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "UploadUserActivities" },
    # 14. security-disable-remote-assistance
    @{ id = "security-disable-remote-assistance"; hive = "HKLM"; path = "Software\policies\Microsoft\Windows NT\Terminal Services"; name = "fAllowToGetHelp" },
    # 15. security-restrict-unauthenticated-rpc
    @{ id = "security-restrict-unauthenticated-rpc"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows NT\Rpc"; name = "RestrictRemoteClients" },
    # 16. security-disallow-projection-to-pc
    @{ id = "security-disallow-projection-to-pc"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Connect"; name = "AllowProjectionToPC" },
    # 17. security-disable-mdm-registration
    @{ id = "security-disable-mdm-registration"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\CurrentVersion\MDM"; name = "DisableRegistration" },
    # 18. system-turn-off-aero-shake
    @{ id = "system-turn-off-aero-shake"; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "NoWindowMinimizingShortcuts" },
    # 19. system-prevent-index-on-battery
    @{ id = "system-prevent-index-on-battery"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name = "PreventIndexOnBattery" },
    # 20. system-disable-web-search-in-start
    @{ id = "system-disable-web-search-in-start"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name = "ConnectedSearchUseWeb" },
    # 21. system-turn-off-user-tracking
    @{ id = "system-turn-off-user-tracking"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name = "NoInstrumentation" },
    # 22. system-disable-notification-center
    @{ id = "system-disable-notification-center"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Explorer"; name = "DisableNotificationCenter" },
    # 23. system-turn-off-toast-notifications
    @{ id = "system-turn-off-toast-notifications"; hive = "HKCU"; path = "SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name = "NoToastApplicationNotification" },
    # 24. peripheral-printers-disable-http
    @{ id = "peripheral-printers-disable-http"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows NT\Printers"; name = "DisableHTTPPrinting" },
    # 25. peripheral-printers-no-add-printer
    @{ id = "peripheral-printers-no-add-printer"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name = "NoAddPrinter" }
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
