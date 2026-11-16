$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json

$block18 = @(
    @{ id="security-disable-system-restore-config"; hive="HKLM"; path="Software\Policies\Microsoft\Windows NT\SystemRestore"; name="DisableConfig" },
    @{ id="security-block-domain-picture-password"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\System"; name="BlockDomainPicturePassword" },
    @{ id="security-deny-removable-drive-write"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\RemovableStorageDevices\{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}"; name="Deny_Write" },
    @{ id="peripheral-no-autorun"; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="NoAutorun" },
    @{ id="peripheral-no-autoplay-non-volume"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Explorer"; name="NoAutoplayfornonVolume" },
    @{ id="peripheral-dont-set-autoplay-checkbox"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Explorer"; name="DontSetAutoplayCheckbox" },
    @{ id="peripheral-turn-off-touch-input"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\TabletPC"; name="TurnOffTouchInput" },
    @{ id="peripheral-wisp-panning-disabled"; hive="HKCU"; path="Software\Microsoft\Wisp\Touch"; name="PanningDisabled" },
    @{ id="peripheral-wisp-touch-gate"; hive="HKCU"; path="Software\Microsoft\Wisp\Touch"; name="TouchGate" },
    @{ id="peripheral-pen-flick-mode"; hive="HKCU"; path="Software\Microsoft\Wisp\Pen\SysEventParameters"; name="FlickMode" },
    @{ id="peripheral-pen-sample-time"; hive="HKCU"; path="Software\Microsoft\Wisp\Pen\SysEventParameters"; name="SampleTime" },
    @{ id="power-hiberboot-enabled"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\System"; name="HiberbootEnabled" },
    @{ id="power-hide-lock-option"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Explorer"; name="ShowLockOption" },
    @{ id="power-hide-sleep-option"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Explorer"; name="ShowSleepOption" },
    @{ id="power-hide-hibernate-option"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Explorer"; name="ShowHibernateOption" },
    @{ id="power-user-battery-discharge-estimator"; hive="HKLM"; path="System\CurrentControlSet\Control\Power"; name="UserBatteryDischargeEstimator" },
    @{ id="power-energy-estimation-enabled"; hive="HKLM"; path="System\CurrentControlSet\Control\Power"; name="EnergyEstimationEnabled" },
    @{ id="visibility-prevent-color-appearance-change"; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="NoDispAppearancePage" },
    @{ id="visibility-prevent-changing-wallpaper"; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop"; name="NoChangingWallPaper" },
    @{ id="visibility-prevent-changing-theme"; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="NoThemesTab" },
    @{ id="visibility-use-default-tile"; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="UseDefaultTile" },
    @{ id="system-verbose-status"; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="VerboseStatus" },
    @{ id="system-allow-automatic-app-archiving"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Appx"; name="AllowAutomaticAppArchiving" },
    @{ id="system-search-prevent-remote-queries"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="PreventRemoteQueries" },
    @{ id="system-search-prevent-index-on-battery"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="PreventIndexOnBattery" }
)

Write-Host "Count: $($block18.Count)"

# Check duplicate IDs within block18
$dupIdsWithin = $block18 | Group-Object id | Where-Object { $_.Count -gt 1 }
if ($dupIdsWithin) {
    Write-Host "ERROR: Duplicate IDs within Block 18:" -ForegroundColor Red
    $dupIdsWithin | ForEach-Object { Write-Host $_.Name }
} else {
    Write-Host "Block 18 internal IDs: OK" -ForegroundColor Green
}

# Check duplicate reg keys within block18
$dupRegWithin = $block18 | Group-Object { "$($_.hive):$($_.path):$($_.name)".ToLowerInvariant() } | Where-Object { $_.Count -gt 1 }
if ($dupRegWithin) {
    Write-Host "ERROR: Duplicate Reg Keys within Block 18:" -ForegroundColor Red
    $dupRegWithin | ForEach-Object { Write-Host $_.Name }
} else {
    Write-Host "Block 18 internal Reg Keys: OK" -ForegroundColor Green
}

# Check collisions with existing 400 tweaks
$existingIds = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
$existingKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($t in $tweaks) {
    $existingIds.Add($t.id) | Out-Null
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $key = "$($r.hive):$($r.path):$($r.name)".ToLowerInvariant()
            $existingKeys.Add($key) | Out-Null
        }
    }
}

$idCollision = $false
$keyCollision = $false

foreach ($b in $block18) {
    if ($existingIds.Contains($b.id)) {
        Write-Host "COLLISION ID: $($b.id)" -ForegroundColor Red
        $idCollision = $true
    }
    $k = "$($b.hive):$($b.path):$($b.name)".ToLowerInvariant()
    if ($existingKeys.Contains($k)) {
        Write-Host "COLLISION REG KEY: $k (in tweak $($b.id))" -ForegroundColor Red
        $keyCollision = $true
    }
}

if (-not $idCollision -and -not $keyCollision) {
    Write-Host "ZERO COLLISIONS WITH EXISTING 400 TWEAKS! Ready to proceed." -ForegroundColor Green
}
