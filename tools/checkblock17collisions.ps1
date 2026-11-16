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
    # 1. privacy-app-access-location
    @{ id = "privacy-app-access-location"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessLocation" },
    # 2. privacy-app-access-microphone
    @{ id = "privacy-app-access-microphone"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessMicrophone" },
    # 3. privacy-app-access-voice-activation
    @{ id = "privacy-app-access-voice-activation"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsActivateWithVoice" },
    # 4. privacy-app-access-voice-above-lock
    @{ id = "privacy-app-access-voice-above-lock"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsActivateWithVoiceAboveLock" },
    # 5. privacy-app-access-spatial-perception
    @{ id = "privacy-app-access-spatial-perception"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessBackgroundSpatialPerception" },
    # 6. privacy-app-access-user-info
    @{ id = "privacy-app-access-user-info"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "AllowUserInfoAccess" },
    # 7. privacy-app-access-cellular-data
    @{ id = "privacy-app-access-cellular-data"; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\WwanSvc\CellularDataAccess"; name = "LetAppsAccessCellularData" },
    # 8. privacy-biometrics-anti-spoofing
    @{ id = "privacy-biometrics-anti-spoofing"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures"; name = "EnhancedAntiSpoofing" },
    # 9. privacy-terminal-encrypted-ticket
    @{ id = "privacy-terminal-encrypted-ticket"; hive = "HKLM"; path = "Software\policies\Microsoft\Windows NT\Terminal Services"; name = "CreateEncryptedOnlyTickets" },
    # 10. security-uac-enable-uia-desktop-toggle
    @{ id = "security-uac-enable-uia-desktop-toggle"; hive = "HKLM"; path = "SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"; name = "EnableUIADesktopToggle" },
    # 11. security-uac-enable-secure-uia-paths
    @{ id = "security-uac-enable-secure-uia-paths"; hive = "HKLM"; path = "SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"; name = "EnableSecureUIAPaths" },
    # 12. security-uac-enable-virtualization
    @{ id = "security-uac-enable-virtualization"; hive = "HKLM"; path = "SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"; name = "EnableVirtualization" },
    # 13. security-deviceguard-require-platform-security
    @{ id = "security-deviceguard-require-platform-security"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\DeviceGuard"; name = "RequirePlatformSecurityFeatures" },
    # 14. security-deviceguard-hvci-mat-required
    @{ id = "security-deviceguard-hvci-mat-required"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\DeviceGuard"; name = "HVCIMATRequired" },
    # 15. security-deviceguard-machine-identity-isolation
    @{ id = "security-deviceguard-machine-identity-isolation"; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\DeviceGuard"; name = "MachineIdentityIsolation" },
    # 16. system-fs-ntfs-allow-extended-char-8dot3
    @{ id = "system-fs-ntfs-allow-extended-char-8dot3"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsAllowExtendedCharacter8dot3Rename" },
    # 17. system-fs-ntfs-disable-compression-limit
    @{ id = "system-fs-ntfs-disable-compression-limit"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsDisableCompressionLimit" },
    # 18. system-fs-ntfs-encrypt-paging-file
    @{ id = "system-fs-ntfs-encrypt-paging-file"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsEncryptPagingFile" },
    # 19. system-fs-ntfs-memory-usage
    @{ id = "system-fs-ntfs-memory-usage"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsMemoryUsage" },
    # 20. system-fs-ntfs-mft-zone-reservation
    @{ id = "system-fs-ntfs-mft-zone-reservation"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "NtfsMftZoneReservation" },
    # 21. system-fs-refs-disable-last-access-update
    @{ id = "system-fs-refs-disable-last-access-update"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "RefsDisableLastAccessUpdate" },
    # 22. system-fs-win31-file-system
    @{ id = "system-fs-win31-file-system"; hive = "HKLM"; path = "System\CurrentControlSet\Control\FileSystem"; name = "Win31FileSystem" },
    # 23. peripheral-touchpad-right-click-zone
    @{ id = "peripheral-touchpad-right-click-zone"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "RightClickZoneEnabled" },
    # 24. peripheral-touchpad-scroll-direction
    @{ id = "peripheral-touchpad-scroll-direction"; hive = "HKCU"; path = "Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad"; name = "ScrollDirection" },
    # 25. peripheral-pen-sys-use-hw-timestamp
    @{ id = "peripheral-pen-sys-use-hw-timestamp"; hive = "HKCU"; path = "Software\Microsoft\Wisp\Pen\SysEventParameters"; name = "UseHWTimeStamp" }
)

Write-Host "Checking $($candidates.Count) candidates for Block 17..."
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
