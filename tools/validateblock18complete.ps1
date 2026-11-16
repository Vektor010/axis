$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json

$block18 = @(
    # 1. Security
    @{ id="security-disable-system-restore-config"; hive="HKLM"; path="Software\Policies\Microsoft\Windows NT\SystemRestore"; name="DisableConfig"; file="security/desc.md"; from=1111; to=1115 },
    @{ id="security-block-domain-picture-password"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\System"; name="BlockDomainPicturePassword"; file="security/desc.md"; from=1161; to=1165 },
    
    # 2. Peripheral
    @{ id="peripheral-no-autorun"; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="NoAutorun"; file="peripheral/desc.md"; from=1389; to=1395 },
    @{ id="peripheral-dont-set-autoplay-checkbox"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Explorer"; name="DontSetAutoplayCheckbox"; file="peripheral/desc.md"; from=1389; to=1395 },
    @{ id="peripheral-wisp-panning-disabled"; hive="HKCU"; path="Software\Microsoft\Wisp\Touch"; name="PanningDisabled"; file="peripheral/desc.md"; from=1413; to=1427 },
    @{ id="peripheral-wisp-touch-gate"; hive="HKCU"; path="Software\Microsoft\Wisp\Touch"; name="TouchGate"; file="peripheral/desc.md"; from=1413; to=1427 },
    @{ id="peripheral-pen-flick-mode"; hive="HKCU"; path="Software\Microsoft\Wisp\Pen\SysEventParameters"; name="FlickMode"; file="peripheral/desc.md"; from=1449; to=1465 },
    @{ id="peripheral-pen-sample-time"; hive="HKCU"; path="Software\Microsoft\Wisp\Pen\SysEventParameters"; name="SampleTime"; file="peripheral/desc.md"; from=1449; to=1465 },

    # 3. Power
    @{ id="power-hiberboot-enabled"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\System"; name="HiberbootEnabled"; file="power/desc.md"; from=2370; to=2373 },
    @{ id="power-hide-lock-option"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Explorer"; name="ShowLockOption"; file="power/desc.md"; from=2396; to=2401 },
    @{ id="power-user-battery-discharge-estimator"; hive="HKLM"; path="System\CurrentControlSet\Control\Power"; name="UserBatteryDischargeEstimator"; file="power/desc.md"; from=2406; to=2411 },

    # 4. Visibility
    @{ id="visibility-prevent-color-appearance-change"; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="NoDispAppearancePage"; file="visibility/desc.md"; from=1359; to=1368 },
    @{ id="visibility-prevent-changing-wallpaper"; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop"; name="NoChangingWallPaper"; file="visibility/desc.md"; from=1359; to=1368 },
    @{ id="visibility-prevent-changing-theme"; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="NoThemesTab"; file="visibility/desc.md"; from=1359; to=1368 },
    @{ id="visibility-use-default-tile"; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="UseDefaultTile"; file="visibility/desc.md"; from=1385; to=1388 },

    # 5. System
    @{ id="system-allow-automatic-app-archiving"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Appx"; name="AllowAutomaticAppArchiving"; file="system/desc.md"; from=9456; to=9459 },
    @{ id="system-disable-search-ui"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="DisableSearch"; file="system/desc.md"; from=9159; to=9161 },

    # 6. Privacy
    @{ id="privacy-disable-cloud-optimized-content"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableCloudOptimizedContent"; file="privacy/desc.md"; from=445; to=447 },
    @{ id="privacy-disable-consumer-account-state"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableConsumerAccountStateContent"; file="privacy/desc.md"; from=446; to=448 },
    @{ id="privacy-disable-third-party-suggestions"; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableThirdPartySuggestions"; file="privacy/desc.md"; from=448; to=450 },
    @{ id="privacy-disable-windows-ink-workspace"; hive="HKLM"; path="Software\Policies\Microsoft\WindowsInkWorkspace"; name="AllowWindowsInkWorkspace"; file="privacy/desc.md"; from=126; to=128 },
    @{ id="privacy-disable-suggested-apps-ink-workspace"; hive="HKLM"; path="Software\Policies\Microsoft\WindowsInkWorkspace"; name="AllowSuggestedAppsInWindowsInkWorkspace"; file="privacy/desc.md"; from=127; to=129 },
    @{ id="privacy-disallow-input-personalization"; hive="HKLM"; path="Software\Policies\Microsoft\InputPersonalization"; name="AllowInputPersonalization"; file="privacy/desc.md"; from=128; to=130 },
    @{ id="privacy-disallow-speech-model-update"; hive="HKLM"; path="Software\Policies\Microsoft\Speech"; name="AllowSpeechModelUpdate"; file="privacy/desc.md"; from=129; to=131 },
    @{ id="privacy-turn-off-rsop-logging"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\System"; name="RSoPLogging"; file="privacy/desc.md"; from=134; to=136 }
)

Write-Host "Total in Block 18: $($block18.Count)"

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

# Source line check
$sourceFail = 0
foreach ($b in $block18) {
    $fPath = "C:\Users\User\Documents\win-tweaker\data\source\win-config\" + $b.file
    $lines = Get-Content $fPath
    $sub = $lines[($b.from - 1)..($b.to - 1)] -join "`n"
    $hasName = $sub.IndexOf($b.name, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $hasPath = $sub.IndexOf($b.path, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $hasType = $sub.IndexOf("Type:", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    if (-not $hasName -and -not $hasPath) {
        Write-Host "SOURCE FAIL: $($b.id) neither name nor path found in $($b.file):$($b.from)..$($b.to)" -ForegroundColor Red
        $sourceFail++
    }
    if ($hasType) {
        Write-Host "WARNING 'Type:' found in $($b.id)" -ForegroundColor Yellow
        $sourceFail++
    }
}

if ($sourceFail -eq 0) {
    Write-Host "ALL 25 SOURCE RANGES VALIDATED!" -ForegroundColor Green
}
