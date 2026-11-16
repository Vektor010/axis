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
    # AppPrivacy policies (privacy/desc.md lines 1060-1087)
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessAccountInfo",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessCalendar",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessCallHistory",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessContacts",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessEmail",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessSystemAIModels",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessGraphicsCaptureProgrammatic",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessGraphicsCaptureWithoutBorder",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessHumanPresence",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessMessaging",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessMotion",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessPhone",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessRadios",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsSyncWithDevices",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessTasks",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessTrustedDevices",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsGetDiagnosticInfo",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessGazeInput",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsActivateWithVoice",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsActivateWithVoiceAboveLock",
    "HKLM\Software\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessBackgroundSpatialPerception",
    "HKLM\Software\Policies\Microsoft\Windows\System\AllowUserInfoAccess",
    "HKLM\Software\Policies\Microsoft\Windows\WwanSvc\CellularDataAccess\LetAppsAccessCellularData",

    # Biometrics & Terminal Services (privacy/desc.md lines 1004-1037)
    "HKLM\SOFTWARE\Policies\Microsoft\Biometrics\FacialFeatures\EnhancedAntiSpoofing",
    "HKLM\Software\policies\Microsoft\Windows NT\Terminal Services\LoggingEnabled",
    "HKLM\Software\policies\Microsoft\Windows NT\Terminal Services\CreateEncryptedOnlyTickets",
    "HKLM\Software\Policies\Microsoft\Windows\Connect\RequirePinForPairing",

    # FileSystem (system/desc.md lines 8511-8530)
    "HKLM\System\CurrentControlSet\Control\FileSystem\DisableDeleteNotification",
    "HKLM\System\CurrentControlSet\Control\FileSystem\DontVerifyRandomDrivers",
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsAllowExtendedCharacter8dot3Rename",
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsBugcheckOnCorrupt",
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsDisableCompressionLimit",
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsDisableSpotCorruptionHandling",
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsEncryptPagingFile",
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsMemoryUsage",
    "HKLM\System\CurrentControlSet\Control\FileSystem\NtfsMftZoneReservation",
    "HKLM\System\CurrentControlSet\Control\FileSystem\RefsDisableLastAccessUpdate",
    "HKLM\System\CurrentControlSet\Control\FileSystem\Win31FileSystem",

    # Multitasking (system/desc.md lines 8466-8500)
    "HKCU\Control Panel\Desktop\WindowArrangementActive",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\SnapAssist",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\EnableSnapAssistFlyout",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\EnableSnapBar",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\EnableTaskGroups",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\DITest",

    # Touchpad & Pen (peripheral/desc.md lines 1435-1465)
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\TapAndDrag",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\RightClickZoneEnabled",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\PanEnabled",
    "HKCU\Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\ScrollDirection",
    "HKCU\Software\Microsoft\Wisp\Pen\SysEventParameters\Latency",
    "HKCU\Software\Microsoft\Wisp\Pen\SysEventParameters\UseHWTimeStamp",
    "HKCU\Software\Microsoft\Wisp\Pen\SysEventParameters\SampleTime",
    "HKCU\Software\Microsoft\Wisp\Pen\SysEventParameters\FlickMode"
)

Write-Host "Probing $($probe.Count) keys against 350 existing tweaks..."
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
