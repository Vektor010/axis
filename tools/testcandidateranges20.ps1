$items = @(
    # Security
    @{ id="security-pua-protection"; file="security/desc.md"; from=286; to=288; path="Software\Policies\Microsoft\Windows Defender"; name="PUAProtection" },
    @{ id="security-block-at-first-seen"; file="security/desc.md"; from=297; to=299; path="Software\Policies\Microsoft\Windows Defender\MpEngine"; name="DisableBlockAtFirstSeen" },
    @{ id="security-enable-network-protection"; file="security/desc.md"; from=303; to=305; path="Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection"; name="EnableNetworkProtection" },
    @{ id="security-controlled-folder-access"; file="security/desc.md"; from=304; to=306; path="Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access"; name="EnableControlledFolderAccess" },
    @{ id="security-no-auto-update"; file="security/desc.md"; from=493; to=495; path="Software\Policies\Microsoft\Windows\WindowsUpdate\AU"; name="NoAutoUpdate" },
    @{ id="security-allow-optional-content"; file="security/desc.md"; from=503; to=505; path="Software\Policies\Microsoft\Windows\WindowsUpdate"; name="SetAllowOptionalContent" },

    # Privacy
    @{ id="privacy-disable-device-delete"; file="privacy/desc.md"; from=118; to=120; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableDeviceDelete" },
    @{ id="privacy-limit-diagnostic-log-collection"; file="privacy/desc.md"; from=120; to=122; path="Software\Policies\Microsoft\Windows\DataCollection"; name="LimitDiagnosticLogCollection" },
    @{ id="privacy-limit-dump-collection"; file="privacy/desc.md"; from=121; to=123; path="Software\Policies\Microsoft\Windows\DataCollection"; name="LimitDumpCollection" },
    @{ id="privacy-disable-send-generic-driver-wer"; file="privacy/desc.md"; from=330; to=332; path="Software\Policies\Microsoft\Windows\DeviceInstall\Settings"; name="DisableSendGenericDriverNotFoundToWER" },
    @{ id="privacy-disable-send-request-additional-software-wer"; file="privacy/desc.md"; from=331; to=333; path="Software\Policies\Microsoft\Windows\DeviceInstall\Settings"; name="DisableSendRequestAdditionalSoftwareToWER" },
    @{ id="privacy-troubleshooting-enabled-execution"; file="privacy/desc.md"; from=383; to=385; path="SOFTWARE\Policies\Microsoft\Windows\ScheduledDiagnostics"; name="EnabledExecution" },
    @{ id="privacy-wdi-scenario-execution"; file="privacy/desc.md"; from=384; to=386; path="SOFTWARE\Policies\Microsoft\Windows\WDI\{9c5a40da-b965-4fc3-8781-88dd50a6299d}"; name="ScenarioExecutionEnabled" },
    @{ id="privacy-allow-online-tips"; file="privacy/desc.md"; from=449; to=451; path="Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"; name="AllowOnlineTips" },
    @{ id="privacy-disable-search-box-suggestions"; file="privacy/desc.md"; from=452; to=454; path="Software\Policies\Microsoft\Windows\Explorer"; name="DisableSearchBoxSuggestions" },
    @{ id="privacy-disable-spotlight-welcome-experience"; file="privacy/desc.md"; from=459; to=461; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightWindowsWelcomeExperience" },
    @{ id="privacy-allow-untriggered-maps-traffic"; file="privacy/desc.md"; from=523; to=525; path="Software\Policies\Microsoft\Windows\Maps"; name="AllowUntriggeredNetworkTrafficOnSettingsPage" },
    @{ id="privacy-allow-cross-device-clipboard"; file="privacy/desc.md"; from=736; to=738; path="Software\Policies\Microsoft\Windows\System"; name="AllowCrossDeviceClipboard" },
    @{ id="privacy-allow-clipboard-history"; file="privacy/desc.md"; from=737; to=739; path="Software\Policies\Microsoft\Windows\System"; name="AllowClipboardHistory" },
    @{ id="privacy-allow-recall-enablement"; file="privacy/desc.md"; from=846; to=848; path="SOFTWARE\Policies\Microsoft\Windows\Windows AI"; name="AllowRecallEnablement" },
    @{ id="privacy-allow-game-dvr"; file="privacy/desc.md"; from=921; to=923; path="Software\Policies\Microsoft\Windows\GameDVR"; name="AllowGameDVR" },
    @{ id="privacy-disable-windows-location-provider"; file="privacy/desc.md"; from=952; to=954; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableWindowsLocationProvider" },
    @{ id="privacy-disable-sensors"; file="privacy/desc.md"; from=953; to=955; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableSensors" },
    @{ id="privacy-disable-location"; file="privacy/desc.md"; from=954; to=956; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableLocation" },
    @{ id="privacy-disable-location-scripting"; file="privacy/desc.md"; from=955; to=957; path="Software\Policies\Microsoft\Windows\LocationAndSensors"; name="DisableLocationScripting" }
)

$srcDir = "C:\Users\User\Documents\win-tweaker\data\source\win-config"
$errors = 0

foreach ($it in $items) {
    $filePath = Join-Path $srcDir $it.file
    $lines = Get-Content $filePath
    $sub = $lines[($it.from - 1)..($it.to - 1)] -join "`n"
    
    $hasNameOrPath = ($sub.IndexOf($it.name, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) -or ($sub.IndexOf($it.path, [System.StringComparison]::OrdinalIgnoreCase) -ge 0)
    $hasType = $sub.IndexOf("Type:", [System.StringComparison]::OrdinalIgnoreCase) -ge 0

    if (-not $hasNameOrPath) {
        Write-Host "FAIL: $($it.name) not found in $($it.file):$($it.from)..$($it.to)" -ForegroundColor Red
        $errors++
    }
    if ($hasType) {
        Write-Host "WARNING Type: found in $($it.name)" -ForegroundColor Yellow
        $errors++
    }
    if ($hasNameOrPath -and (-not $hasType)) {
        Write-Host "PASS: $($it.name) ($($it.file):$($it.from)..$($it.to))" -ForegroundColor Green
    }
}

Write-Host "Total failures: $errors"
