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
    # 1. privacy-app-access-account-info
    @{ id = "privacy-app-access-account-info"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessAccountInfo" },
    # 2. privacy-app-access-calendar
    @{ id = "privacy-app-access-calendar"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessCalendar" },
    # 3. privacy-app-access-call-history
    @{ id = "privacy-app-access-call-history"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessCallHistory" },
    # 4. privacy-app-access-contacts
    @{ id = "privacy-app-access-contacts"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessContacts" },
    # 5. privacy-app-access-email
    @{ id = "privacy-app-access-email"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessEmail" },
    # 6. privacy-app-access-system-ai-models
    @{ id = "privacy-app-access-system-ai-models"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessSystemAIModels" },
    # 7. privacy-app-access-graphics-capture
    @{ id = "privacy-app-access-graphics-capture"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessGraphicsCaptureProgrammatic" },
    # 8. privacy-app-access-human-presence
    @{ id = "privacy-app-access-human-presence"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessHumanPresence" },
    # 9. privacy-app-access-messaging
    @{ id = "privacy-app-access-messaging"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessMessaging" },
    # 10. privacy-app-access-motion
    @{ id = "privacy-app-access-motion"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessMotion" },
    # 11. privacy-app-access-radios
    @{ id = "privacy-app-access-radios"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessRadios" },
    # 12. privacy-app-access-sync-devices
    @{ id = "privacy-app-access-sync-devices"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsSyncWithDevices" },
    # 13. privacy-app-access-tasks
    @{ id = "privacy-app-access-tasks"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessTasks" },
    # 14. privacy-app-access-trusted-devices
    @{ id = "privacy-app-access-trusted-devices"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessTrustedDevices" },
    # 15. privacy-app-access-diagnostic-info
    @{ id = "privacy-app-access-diagnostic-info"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsGetDiagnosticInfo" },
    # 16. privacy-app-access-gaze-input
    @{ id = "privacy-app-access-gaze-input"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessGazeInput" },
    # 17. privacy-terminal-services-logging
    @{ id = "privacy-terminal-services-logging"; hive = "HKLM"; path = "Software\policies\Microsoft\Windows NT\Terminal Services"; name = "LoggingEnabled" },
    # 18. security-connect-require-pin-pairing
    @{ id = "security-connect-require-pin-pairing"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\Connect"; name = "RequirePinForPairing" },
    # 19. system-fs-trim-delete-notification
    @{ id = "system-fs-trim-delete-notification"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "DisableDeleteNotification" },
    # 20. system-fs-dont-verify-random-drivers
    @{ id = "system-fs-dont-verify-random-drivers"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "DontVerifyRandomDrivers" },
    # 21. system-fs-ntfs-bugcheck-on-corrupt
    @{ id = "system-fs-ntfs-bugcheck-on-corrupt"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsBugcheckOnCorrupt" },
    # 22. system-fs-ntfs-disable-spot-corruption
    @{ id = "system-fs-ntfs-disable-spot-corruption"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsDisableSpotCorruptionHandling" },
    # 23. peripheral-touchpad-tap-and-drag
    @{ id = "peripheral-touchpad-tap-and-drag"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "TapAndDrag" },
    # 24. peripheral-touchpad-pan-enabled
    @{ id = "peripheral-touchpad-pan-enabled"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "PanEnabled" },
    # 25. peripheral-pen-sys-latency-optimal
    @{ id = "peripheral-pen-sys-latency-optimal"; hive = "HKCU"; path = "Software\Microsoft\Wisp\Pen\SysEventParameters"; name = "Latency" }
)

Write-Host "Checking $($candidates.Count) candidates for Block 16..."
$foundIssues = 0
foreach ($c in $candidates) {
    if ($existingIds[$c.id]) {
        Write-Host "[ID Collision] $($c.id) already exists!"
        $foundIssues++
    }
    $k = ($c.hive + "\" + $c.path + "\" + $c.name).ToLowerInvariant()
    if ($existingKeys[$k]) {
        Write-Host "[Registry Collision] $k is already used by $($existingKeys[$k])!"
        $foundIssues++
    }
}

if ($foundIssues -eq 0) {
    Write-Host "ALL 25 CANDIDATES ARE 100% COLLISION-FREE!"
} else {
    Write-Host "Found $foundIssues collisions."
}
