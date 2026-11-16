$items = @(
    @{ id="network-directaccess-passive-mode"; file="network/desc.md"; from=994; to=996; path="SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator"; name="PassiveMode" },
    @{ id="network-wifi-autoconnect-hotspots"; file="network/desc.md"; line=1144; from=1143; to=1145; path="Software\Microsoft\wcmsvc\wifinetworkmanager\config"; name="AutoConnectAllowedOEM" },
    @{ id="peripheral-downlevel-browse-printers"; file="peripheral/desc.md"; from=1835; to=1837; path="Software\Policies\Microsoft\Windows NT\Printers"; name="Downlevel Browse" },
    @{ id="visibility-prevent-changing-desktop-icons"; file="visibility/desc.md"; from=1362; to=1364; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="NoDispBackgroundPage" },
    @{ id="visibility-prevent-changing-lock-screen"; file="visibility/desc.md"; from=1363; to=1365; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingLockScreen" },
    @{ id="visibility-prevent-changing-mouse-pointers"; file="visibility/desc.md"; from=1364; to=1366; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingMousePointers" },
    @{ id="visibility-prevent-changing-start-background"; file="visibility/desc.md"; from=1365; to=1367; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingStartMenuBackground" },
    @{ id="visibility-animate-lock-screen-background"; file="visibility/desc.md"; from=1767; to=1769; path="Software\Policies\Microsoft\Windows\Personalization"; name="AnimateLockScreenBackground" },
    @{ id="visibility-disable-wcn-ui"; file="visibility/desc.md"; from=337; to=339; path="Software\Policies\Microsoft\Windows\WCN\UI"; name="DisableWcnUi" },
    @{ id="system-search-auto-lang-detection"; file="system/desc.md"; from=9153; to=9155; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="AlwaysUseAutoLangDetection" },
    @{ id="system-search-web-metered-connections"; file="system/desc.md"; from=9155; to=9157; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="ConnectedSearchUseWebOverMeteredConnections" },
    @{ id="system-search-safesearch"; file="system/desc.md"; from=9157; to=9159; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="ConnectedSearchSafeSearch" },
    @{ id="system-disable-account-notifications"; file="system/desc.md"; from=8402; to=8404; path="SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name="DisableAccountNotifications" },
    @{ id="privacy-disable-enterprise-auth-proxy"; file="privacy/desc.md"; from=113; to=115; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableEnterpriseAuthProxy" },
    @{ id="privacy-allow-device-name-in-telemetry"; file="privacy/desc.md"; from=115; to=117; path="Software\Policies\Microsoft\Windows\DataCollection"; name="AllowDeviceNameInTelemetry" },
    @{ id="privacy-disable-telemetry-optin-settings-ux"; file="privacy/desc.md"; from=116; to=118; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableTelemetryOptInSettingsUx" },
    @{ id="privacy-disable-telemetry-optin-change-notification"; file="privacy/desc.md"; from=117; to=119; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableTelemetryOptInChangeNotification" },
    @{ id="privacy-disable-diagnostic-data-viewer"; file="privacy/desc.md"; from=119; to=121; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableDiagnosticDataViewer" },
    @{ id="privacy-allow-linguistic-data-collection"; file="privacy/desc.md"; from=124; to=126; path="Software\Microsoft\Windows\CurrentVersion\Policies\TextInput"; name="AllowLinguisticDataCollection" },
    @{ id="privacy-turn-off-messenger-ceip"; file="privacy/desc.md"; from=131; to=133; path="Software\Policies\Microsoft\Messenger\Client"; name="CEIP" },
    @{ id="privacy-disable-msdt-query-remote-server"; file="privacy/desc.md"; from=379; to=381; path="SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnosticsProvider\Policy"; name="DisableQueryRemoteServer" },
    @{ id="privacy-troubleshooting-allow-recommendations"; file="privacy/desc.md"; from=382; to=384; path="Software\Policies\Microsoft\Windows\Troubleshooting"; name="TroubleshootingAllowRecommendations" },
    @{ id="privacy-disable-spotlight-collection-desktop"; file="privacy/desc.md"; from=455; to=457; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableSpotlightCollectionOnDesktop" },
    @{ id="privacy-disable-spotlight-action-center"; file="privacy/desc.md"; from=457; to=459; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightOnActionCenter" },
    @{ id="privacy-disable-spotlight-settings"; file="privacy/desc.md"; from=458; to=460; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightOnSettings" }
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
