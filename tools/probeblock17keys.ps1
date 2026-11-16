$tweaks = Get-Content -Raw "C:\Users\User\Documents\win-tweaker\data\tweaks.json" | ConvertFrom-Json
$existingKeys = @{}
$existingIds = @{}
foreach ($t in $tweaks) {
    $existingIds[$t.id] = $true
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $key = ($r.hive + "\" + $r.path + "\" + $r.name).ToLowerInvariant()
            $existingKeys[$key] = $t.id
        }
    }
}

$probe = @(
    # 1. privacy-app-access-location (privacy/desc.md line 1071)
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessLocation",
    # 2. privacy-app-access-microphone (privacy/desc.md line 1073)
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessMicrophone",
    # 3. privacy-app-access-voice-activation (privacy/desc.md line 1083)
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsActivateWithVoice",
    # 4. privacy-app-access-voice-above-lock (privacy/desc.md line 1084)
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsActivateWithVoiceAboveLock",
    # 5. privacy-app-access-spatial-perception (privacy/desc.md line 1085)
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessBackgroundSpatialPerception",
    # 6. privacy-app-access-user-info (privacy/desc.md line 1086)
    "HKLM\Software\Policies\Microsoft\Windows\System\AllowUserInfoAccess",
    # 7. privacy-app-access-cellular-data (privacy/desc.md line 1087)
    "HKLM\Software\Policies\Microsoft\Windows\WwanSvc\CellularDataAccess\LetAppsAccessCellularData",
    # 8. privacy-biometrics-anti-spoofing (privacy/desc.md line 1007)
    "HKLM\SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures\EnhancedAntiSpoofing",
    # 9. privacy-terminal-encrypted-ticket (privacy/desc.md line 1034)
    "HKLM\Software\policies\Microsoft\Windows NT\Terminal Services\CreateEncryptedOnlyTickets",
    # 10. security-uac-filter-admin-token (security/desc.md lines 522-530)
    "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\FilterAdministratorToken",
    # 11. security-uac-enable-uia-desktop-toggle (security/desc.md lines 531-539)
    "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\EnableUIADesktopToggle",
    # 12. security-uac-enable-secure-uia-paths (security/desc.md lines 581-589)
    "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\EnableSecureUIAPaths",
    # 13. security-uac-enable-virtualization (security/desc.md lines 608-616)
    "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\EnableVirtualization",
    # 14. security-deviceguard-require-platform-security (security/desc.md line 1027)
    "HKLM\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard\RequirePlatformSecurityFeatures",
    # 15. security-deviceguard-hvci-mat-required (security/desc.md line 1027)
    "HKLM\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard\HVCIMATRequired",
    # 16. system-fs-ntfs-allow-extended-char-8dot3 (system/desc.md line 8516)
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsAllowExtendedCharacter8dot3Rename",
    # 17. system-fs-ntfs-disable-compression-limit (system/desc.md line 8520)
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsDisableCompressionLimit",
    # 18. system-fs-ntfs-encrypt-paging-file (system/desc.md line 8524)
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsEncryptPagingFile",
    # 19. system-fs-ntfs-memory-usage (system/desc.md line 8525)
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsMemoryUsage",
    # 20. system-fs-ntfs-mft-zone-reservation (system/desc.md line 8526)
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsMftZoneReservation",
    # 21. system-fs-refs-disable-last-access-update (system/desc.md line 8527)
    "HKLM\System\CurrentControlSet\Control\FileSystem\RefsDisableLastAccessUpdate",
    # 22. system-fs-win31-file-system (system/desc.md line 8529)
    "HKLM\System\CurrentControlSet\Control\FileSystem\Win31FileSystem",
    # 23. peripheral-touchpad-right-click-zone (peripheral/desc.md line 1441)
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\RightClickZoneEnabled",
    # 24. peripheral-touchpad-scroll-direction (peripheral/desc.md line 1443)
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\ScrollDirection",
    # 25. peripheral-pen-sys-use-hw-timestamp (peripheral/desc.md line 1460)
    "HKCU\Software\Microsoft\Wisp\Pen\SysEventParameters\UseHWTimeStamp"
)

Write-Host "Probing $($probe.Count) keys against 375 existing tweaks..."
$freeCount = 0
foreach ($p in $probe) {
    $k = $p.ToLowerInvariant()
    if ($existingKeys[$k]) {
        Write-Host "USED: $p (by $($existingKeys[$k]))"
    } else {
        Write-Host "FREE: $p"
        $freeCount++
    }
}
Write-Host "Total FREE keys: $freeCount"
