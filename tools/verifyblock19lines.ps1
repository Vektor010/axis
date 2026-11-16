$candidates = @(
    @{ id="network-directaccess-passive-mode"; file="network/desc.md"; line=995; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator"; name="PassiveMode"; apply=1 },
    @{ id="network-wifi-autoconnect-hotspots"; file="network/desc.md"; line=1144; hive="HKLM"; path="Software\Microsoft\wcmsvc\wifinetworkmanager\config"; name="AutoConnectAllowedOEM"; apply=0 },
    @{ id="peripheral-downlevel-browse-printers"; file="peripheral/desc.md"; line=1836; hive="HKCU"; path="Software\Policies\Microsoft\Windows NT\Printers"; name="Downlevel Browse"; apply=0 },
    @{ id="visibility-prevent-changing-desktop-icons"; file="visibility/desc.md"; line=1363; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="NoDispBackgroundPage"; apply=1 },
    @{ id="visibility-prevent-changing-lock-screen"; file="visibility/desc.md"; line=1364; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingLockScreen"; apply=1 },
    @{ id="visibility-prevent-changing-mouse-pointers"; file="visibility/desc.md"; line=1365; hive="HKCU"; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingMousePointers"; apply=1 },
    @{ id="visibility-prevent-changing-start-background"; file="visibility/desc.md"; line=1366; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingStartMenuBackground"; apply=1 },
    @{ id="visibility-animate-lock-screen-background"; file="visibility/desc.md"; line=1768; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Personalization"; name="AnimateLockScreenBackground"; apply=0 },
    @{ id="visibility-disable-wcn-ui"; file="visibility/desc.md"; line=338; hive="HKLM"; path="Software\Policies\Microsoft\Windows\WCN\UI"; name="DisableWcnUi"; apply=1 },
    @{ id="system-search-auto-lang-detection"; file="system/desc.md"; line=9154; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="AlwaysUseAutoLangDetection"; apply=0 },
    @{ id="system-search-web-metered-connections"; file="system/desc.md"; line=9156; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="ConnectedSearchUseWebOverMeteredConnections"; apply=0 },
    @{ id="system-search-safesearch"; file="system/desc.md"; line=9158; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="ConnectedSearchSafeSearch"; apply=0 },
    @{ id="system-disable-account-notifications"; file="system/desc.md"; line=8403; hive="HKCU"; path="SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name="DisableAccountNotifications"; apply=1 },
    @{ id="privacy-disable-enterprise-auth-proxy"; file="privacy/desc.md"; line=114; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableEnterpriseAuthProxy"; apply=1 },
    @{ id="privacy-allow-device-name-in-telemetry"; file="privacy/desc.md"; line=116; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="AllowDeviceNameInTelemetry"; apply=0 },
    @{ id="privacy-disable-telemetry-optin-settings-ux"; file="privacy/desc.md"; line=117; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableTelemetryOptInSettingsUx"; apply=1 },
    @{ id="privacy-disable-telemetry-optin-change-notification"; file="privacy/desc.md"; line=118; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableTelemetryOptInChangeNotification"; apply=1 },
    @{ id="privacy-disable-diagnostic-data-viewer"; file="privacy/desc.md"; line=120; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableDiagnosticDataViewer"; apply=1 },
    @{ id="privacy-allow-linguistic-data-collection"; file="privacy/desc.md"; line=125; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\TextInput"; name="AllowLinguisticDataCollection"; apply=0 },
    @{ id="privacy-turn-off-messenger-ceip"; file="privacy/desc.md"; line=132; hive="HKLM"; path="Software\Policies\Microsoft\Messenger\Client"; name="CEIP"; apply=2 },
    @{ id="privacy-disable-msdt-query-remote-server"; file="privacy/desc.md"; line=380; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnosticsProvider\Policy"; name="DisableQueryRemoteServer"; apply=1 },
    @{ id="privacy-troubleshooting-allow-recommendations"; file="privacy/desc.md"; line=383; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Troubleshooting"; name="TroubleshootingAllowRecommendations"; apply=0 },
    @{ id="privacy-disable-spotlight-collection-desktop"; file="privacy/desc.md"; line=456; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableSpotlightCollectionOnDesktop"; apply=1 },
    @{ id="privacy-disable-spotlight-action-center"; file="privacy/desc.md"; line=458; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightOnActionCenter"; apply=1 },
    @{ id="privacy-disable-spotlight-settings"; file="privacy/desc.md"; line=459; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightOnSettings"; apply=1 }
)

Write-Host "Candidate count: $($candidates.Count)"

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
