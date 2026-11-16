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

$ids = $block19 | ForEach-Object { $_.id }
$keys = $block19 | ForEach-Object { "$($_.hive):$($_.path):$($_.name)".ToLowerInvariant() }

Write-Host "Total IDs: $($ids.Count)"
Write-Host "Unique IDs: $(($ids | Select-Object -Unique).Count)"
Write-Host "Total Keys: $($keys.Count)"
Write-Host "Unique Keys: $(($keys | Select-Object -Unique).Count)"

$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$existingIds = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
$existingKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($t in $tweaks) {
    $existingIds.Add($t.id) | Out-Null
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $existingKeys.Add("$($r.hive):$($r.path):$($r.name)".ToLowerInvariant()) | Out-Null
        }
    }
}

$idCollisions = @()
$keyCollisions = @()

foreach ($id in $ids) {
    if ($existingIds.Contains($id)) { $idCollisions += $id }
}

foreach ($k in $keys) {
    if ($existingKeys.Contains($k)) { $keyCollisions += $k }
}

Write-Host "Collisions with existing IDs: $($idCollisions.Count)"
Write-Host "Collisions with existing Keys: $($keyCollisions.Count)"
