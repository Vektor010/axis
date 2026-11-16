$candidates = @(
    # Security
    @{ id="security-pua-protection"; file="security/desc.md"; line=287; hive="HKLM"; path="Software\Policies\Microsoft\Windows Defender"; name="PUAProtection"; apply=1 },
    @{ id="security-block-at-first-seen"; file="security/desc.md"; line=298; hive="HKLM"; path="Software\Policies\Microsoft\Windows Defender\MpEngine"; name="DisableBlockAtFirstSeen"; apply=0 },
    @{ id="security-enable-network-protection"; file="security/desc.md"; line=304; hive="HKLM"; path="Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection"; name="EnableNetworkProtection"; apply=1 },
    @{ id="security-controlled-folder-access"; file="security/desc.md"; line=305; hive="HKLM"; path="Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access"; name="EnableControlledFolderAccess"; apply=1 },
    @{ id="security-no-auto-update"; file="security/desc.md"; line=494; hive="HKLM"; path="Software\Policies\Microsoft\Windows\WindowsUpdate\AU"; name="NoAutoUpdate"; apply=1 },
    @{ id="security-allow-optional-content"; file="security/desc.md"; line=504; hive="HKLM"; path="Software\Policies\Microsoft\Windows\WindowsUpdate"; name="SetAllowOptionalContent"; apply=0 },

    # Privacy
    @{ id="privacy-disable-device-delete"; file="privacy/desc.md"; line=119; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableDeviceDelete"; apply=0 },
    @{ id="privacy-limit-diagnostic-log-collection"; file="privacy/desc.md"; line=121; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="LimitDiagnosticLogCollection"; apply=1 },
    @{ id="privacy-limit-dump-collection"; file="privacy/desc.md"; line=122; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="LimitDumpCollection"; apply=1 },
    @{ id="privacy-disable-send-generic-driver-wer"; file="privacy/desc.md"; line=331; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DeviceInstall\Settings"; name="DisableSendGenericDriverNotFoundToWER"; apply=1 },
    @{ id="privacy-disable-send-request-additional-software-wer"; file="privacy/desc.md"; line=332; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DeviceInstall\Settings"; name="DisableSendRequestAdditionalSoftwareToWER"; apply=1 },
    @{ id="privacy-troubleshooting-enabled-execution"; file="privacy/desc.md"; line=384; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\ScheduledDiagnostics"; name="EnabledExecution"; apply=0 },
    @{ id="privacy-wdi-scenario-execution"; file="privacy/desc.md"; line=385; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\WDI\{9c5a40da-b965-4fc3-8781-88dd50a6299d}"; name="ScenarioExecutionEnabled"; apply=0 },
    @{ id="privacy-allow-online-tips"; file="privacy/desc.md"; line=450; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="AllowOnlineTips"; apply=0 },
    @{ id="privacy-disable-search-box-suggestions"; file="privacy/desc.md"; line=453; hive="HKCU"; path="Software\Policies\Microsoft\Windows\Explorer"; name="DisableSearchBoxSuggestions"; apply=1 },
    @{ id="privacy-disable-spotlight-welcome-experience"; file="privacy/desc.md"; line=460; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightWindowsWelcomeExperience"; apply=1 },
    @{ id="privacy-allow-untriggered-maps-traffic"; file="privacy/desc.md"; line=524; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Maps"; name="AllowUntriggeredNetworkTrafficOnSettingsPage"; apply=0 },
    @{ id="privacy-allow-cross-device-clipboard"; file="privacy/desc.md"; line=737; hive="HKLM"; path="Software\Policies\Microsoft\Windows\System"; name="AllowCrossDeviceClipboard"; apply=0 },
    @{ id="privacy-allow-clipboard-history"; file="privacy/desc.md"; line=738; hive="HKLM"; path="Software\Policies\Microsoft\Windows\System"; name="AllowClipboardHistory"; apply=1 },
    @{ id="privacy-allow-recall-enablement"; file="privacy/desc.md"; line=847; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows AI"; name="AllowRecallEnablement"; apply=0 },
    @{ id="privacy-allow-game-dvr"; file="privacy/desc.md"; line=922; hive="HKLM"; path="Software\Policies\Microsoft\Windows\GameDVR"; name="AllowGameDVR"; apply=0 },
    @{ id="privacy-disable-windows-location-provider"; file="privacy/desc.md"; line=953; hive="HKLM"; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableWindowsLocationProvider"; apply=1 },
    @{ id="privacy-disable-sensors"; file="privacy/desc.md"; line=954; hive="HKLM"; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableSensors"; apply=1 },
    @{ id="privacy-disable-location"; file="privacy/desc.md"; line=955; hive="HKLM"; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableLocation"; apply=1 },
    @{ id="privacy-disable-location-scripting"; file="privacy/desc.md"; line=956; hive="HKLM"; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableLocationScripting"; apply=1 }
)

Write-Host "Block 20 candidate count: $($candidates.Count)"

$srcDir = "C:\Users\User\Documents\win-tweaker\data\source\win-config"
$errors = 0

foreach ($c in $candidates) {
    $fullPath = Join-Path $srcDir $c.file
    if (-not (Test-Path $fullPath)) {
        Write-Host "FILE NOT FOUND: $($c.file)" -ForegroundColor Red
        $errors++
        continue
    }
    $lines = Get-Content $fullPath
    $targetLine = $lines[$c.line - 1]
    
    $hasName = $targetLine.IndexOf($c.name, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $hasPath = $targetLine.IndexOf($c.path, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    $hasType = $targetLine.IndexOf("Type:", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    
    if (-not $hasName -and -not $hasPath) {
        Write-Host "FAIL NAME/PATH: $($c.name) in line $($c.line) of $($c.file): $targetLine" -ForegroundColor Red
        $errors++
    }
    if ($hasType) {
        Write-Host "WARNING Type: in line $($c.line)" -ForegroundColor Yellow
        $errors++
    }
    if (($hasName -or $hasPath) -and (-not $hasType)) {
        Write-Host "OK: $($c.name) at $($c.file):$($c.line)" -ForegroundColor Green
    }
}

Write-Host "Total errors: $errors"
