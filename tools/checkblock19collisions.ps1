$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json

$block19 = @(
    @{ id="network-directaccess-passive-mode"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator"; name="PassiveMode" },
    @{ id="network-wifi-autoconnect-hotspots"; hive="HKLM"; path="Software\Microsoft\wcmsvc\wifinetworkmanager\config"; name="AutoConnectAllowedOEM" },
    @{ id="peripheral-downlevel-browse-printers"; hive="HKCU"; path="Software\Policies\Microsoft\Windows NT\Printers"; name="Downlevel Browse" },
    @{ id="visibility-prevent-changing-desktop-icons"; hive="HKCU"; path="Software\Microsoft\Windows\CurrentVersion\Policies\System"; name="NoDispBackgroundPage" },
    @{ id="visibility-prevent-changing-lock-screen"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingLockScreen" },
    @{ id="visibility-prevent-changing-mouse-pointers"; hive="HKCU"; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingMousePointers" },
    @{ id="visibility-prevent-changing-start-background"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Personalization"; name="NoChangingStartMenuBackground" },
    @{ id="visibility-animate-lock-screen-background"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Personalization"; name="AnimateLockScreenBackground" },
    @{ id="visibility-disable-wcn-ui"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\WCN\UI"; name="DisableWcnUi" },
    @{ id="system-search-auto-lang-detection"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="AlwaysUseAutoLangDetection" },
    @{ id="system-search-web-metered-connections"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="ConnectedSearchUseWebOverMeteredConnections" },
    @{ id="system-search-safesearch"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name="ConnectedSearchSafeSearch" },
    @{ id="system-disable-account-notifications"; hive="HKCU"; path="SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"; name="DisableAccountNotifications" },
    @{ id="privacy-disable-enterprise-auth-proxy"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableEnterpriseAuthProxy" },
    @{ id="privacy-allow-device-name-in-telemetry"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="AllowDeviceNameInTelemetry" },
    @{ id="privacy-disable-telemetry-optin-settings-ux"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableTelemetryOptInSettingsUx" },
    @{ id="privacy-disable-telemetry-optin-change-notification"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableTelemetryOptInChangeNotification" },
    @{ id="privacy-disable-diagnostic-data-viewer"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\DataCollection"; name="DisableDiagnosticDataViewer" },
    @{ id="privacy-allow-linguistic-data-collection"; hive="HKLM"; path="Software\Microsoft\Windows\CurrentVersion\Policies\TextInput"; name="AllowLinguisticDataCollection" },
    @{ id="privacy-turn-off-messenger-ceip"; hive="HKLM"; path="Software\Policies\Microsoft\Messenger\Client"; name="CEIP" },
    @{ id="privacy-disable-msdt-query-remote-server"; hive="HKLM"; path="SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnosticsProvider\Policy"; name="DisableQueryRemoteServer" },
    @{ id="privacy-troubleshooting-allow-recommendations"; hive="HKLM"; path="Software\Policies\Microsoft\Windows\Troubleshooting"; name="TroubleshootingAllowRecommendations" },
    @{ id="privacy-disable-spotlight-collection-desktop"; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableSpotlightCollectionOnDesktop" },
    @{ id="privacy-disable-spotlight-action-center"; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightOnActionCenter" },
    @{ id="privacy-disable-spotlight-settings"; hive="HKCU"; path="Software\Policies\Microsoft\Windows\CloudContent"; name="DisableWindowsSpotlightOnSettings" }
)

Write-Host "Count: $($block19.Count)"

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

foreach ($b in $block19) {
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

# Check duplicate keys inside block 19
$dupReg = $block19 | Group-Object { "$($_.hive):$($_.path):$($_.name)".ToLowerInvariant() } | Where-Object { $_.Count -gt 1 }
if ($dupReg) {
    Write-Host "INTERNAL REG COLLISION: $($dupReg.Name)" -ForegroundColor Red
    $keyCollision = $true
}

$dupId = $block19 | Group-Object id | Where-Object { $_.Count -gt 1 }
if ($dupId) {
    Write-Host "INTERNAL ID COLLISION: $($dupId.Name)" -ForegroundColor Red
    $idCollision = $true
}

if (-not $idCollision -and -not $keyCollision) {
    Write-Host "ZERO COLLISIONS WITH EXISTING 425 TWEAKS! Ready to proceed." -ForegroundColor Green
}
