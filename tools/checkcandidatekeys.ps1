$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json

$candidates = @(
    "DisableConfig",
    "BlockDomainPicturePassword",
    "Deny_Write",
    "NoAutorun",
    "NoAutoplayfornonVolume",
    "DontSetAutoplayCheckbox",
    "TurnOffTouchInput",
    "PanningDisabled",
    "Friction",
    "TouchGate",
    "FlickMode",
    "SampleTime",
    "HiberbootEnabled",
    "ShowLockOption",
    "ShowSleepOption",
    "ShowHibernateOption",
    "UserBatteryDischargeEstimator",
    "UserBatteryChargeEstimator",
    "EnergyEstimationEnabled",
    "NoDispAppearancePage",
    "NoChangingWallPaper",
    "NoDispBackgroundPage",
    "NoChangingLockScreen",
    "NoChangingMousePointers",
    "NoChangingStartMenuBackground",
    "NoThemesTab",
    "UseDefaultTile",
    "VerboseStatus",
    "AllowAutomaticAppArchiving",
    "PreventRemoteQueries",
    "PreventIndexOnBattery",
    "AlwaysUseAutoLangDetection",
    "DisableRemovableDriveIndexing",
    "NoStartMenuMFUprogramsList",
    "NoInstrumentation",
    "HideRecentlyAddedApps",
    "NoNewAppAlert"
)

foreach ($c in $candidates) {
    $found = $tweaks | Where-Object { 
        $t = $_
        $hasKey = $false
        foreach ($r in $t.actions.registry) {
            if ($r.name -eq $c) { $hasKey = $true }
        }
        $hasKey
    }
    if ($found) {
        Write-Host "FOUND EXISTING: $c in tweak $($found.id)"
    } else {
        Write-Host "AVAILABLE: $c"
    }
}
