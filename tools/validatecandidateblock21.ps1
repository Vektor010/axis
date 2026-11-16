$srcDir = "C:\Users\User\Documents\win-tweaker\data\source\win-config"
$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json

$existingIds = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
$existingRegKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($t in $tweaks) {
    $existingIds.Add($t.id) | Out-Null
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $existingRegKeys.Add("$($r.hive)\$($r.path)\$($r.name)") | Out-Null
        }
    }
}

$candidates = @(
    # Game Mode (system/desc.md)
    @{ id = "system-game-mode-allow-auto"; file = "system/desc.md"; from = 4440; to = 4443; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\GameDVR"; name = "AllowAutoGameMode"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Отключить принудительную автоматическую активацию Game Mode"; cat = "system" },
    @{ id = "system-game-mode-gpu-yield"; file = "system/desc.md"; from = 4434; to = 4439; hive = "HKCU"; path = "Software\Microsoft\GameBar"; name = "GpuYieldPercentage"; apply = 2; revMode = "delete"; revVal = $null; name_ru = "Минимальная задержка планировщика GPU Game Mode (GpuYieldPercentage 2%)"; cat = "system" },
    @{ id = "system-game-mode-disallow-system-cpu-sets"; file = "system/desc.md"; from = 4434; to = 4440; hive = "HKCU"; path = "Software\Microsoft\GameBar"; name = "DisallowSystemAllowedCpuSets"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Запретить системные ограничения CPU Sets для Game Mode"; cat = "system" },

    # Privacy (privacy/desc.md)
    @{ id = "privacy-limit-enhanced-diagnostic-data"; file = "privacy/desc.md"; from = 114; to = 116; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\DataCollection"; name = "LimitEnhancedDiagnosticDataWindowsAnalytics"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Ограничить расширенные диагностические данные для Desktop Analytics"; cat = "privacy" },
    @{ id = "privacy-disable-kms-avs-validation"; file = "privacy/desc.md"; from = 135; to = 137; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows NT\CurrentVersion\Software Protection Platform"; name = "NoGenTicket"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Отключить онлайн-валидацию AVS для клиента KMS"; cat = "privacy" },
    @{ id = "privacy-auto-approve-os-dumps"; file = "privacy/desc.md"; from = 334; to = 336; hive = "HKCU"; path = "SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"; name = "AutoApproveOSDumps"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Запретить автоматическую отправку дампов памяти ОС в Microsoft"; cat = "privacy" },
    @{ id = "privacy-enable-device-health-attestation"; file = "privacy/desc.md"; from = 336; to = 338; hive = "HKLM"; path = "Software\Policies\Microsoft\DeviceHealthAttestationService"; name = "EnableDeviceHealthAttestationService"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Отключить службу мониторинга и отчетов аттестации работоспособности устройств"; cat = "privacy" },
    @{ id = "privacy-persistent-timestamp"; file = "privacy/desc.md"; from = 337; to = 339; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows NT\Reliability"; name = "TimeStampEnabled"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Отключить постоянную временную метку монитора надежности (Reliability)"; cat = "privacy" },
    @{ id = "privacy-disable-scripted-diagnostics"; file = "privacy/desc.md"; from = 380; to = 382; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnostics"; name = "EnableDiagnostics"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Запретить запуск мастеров скриптовой диагностики Windows"; cat = "privacy" },
    @{ id = "privacy-disable-remote-troubleshooting-server"; file = "privacy/desc.md"; from = 381; to = 383; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnosticsProvider\Policy"; name = "EnableQueryRemoteServer"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Запретить доступ мастера устранения неполадок к онлайн-серверам Microsoft (WOTS)"; cat = "privacy" },
    @{ id = "privacy-disable-spotlight-features"; file = "privacy/desc.md"; from = 454; to = 456; hive = "HKCU"; path = "Software\Policies\Microsoft\Windows\CloudContent"; name = "DisableWindowsSpotlightFeatures"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Отключить все функции Windows Spotlight (Интересное в Windows)"; cat = "privacy" },
    @{ id = "privacy-disable-phone-pc-linking"; file = "privacy/desc.md"; from = 608; to = 610; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "EnableMmx"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Отключить интеграцию и связывание смартфона с ПК (Phone Link)"; cat = "privacy" },
    @{ id = "privacy-disable-app-uri-handlers"; file = "privacy/desc.md"; from = 722; to = 724; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\System"; name = "EnableAppUriHandlers"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Отключить перенаправление веб-ссылок в приложения (Web-to-App URI Handlers)"; cat = "privacy" },
    @{ id = "privacy-disable-sandbox-clipboard-redirection"; file = "privacy/desc.md"; from = 739; to = 741; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Sandbox"; name = "AllowClipboardRedirection"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Запретить общий буфер обмена с песочницей Windows Sandbox"; cat = "privacy" },
    @{ id = "privacy-disable-apps-camera-access"; file = "privacy/desc.md"; from = 1063; to = 1065; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessCamera"; apply = 2; revMode = "delete"; revVal = $null; name_ru = "Запретить приложениям Windows доступ к веб-камере"; cat = "privacy" },
    @{ id = "privacy-disable-apps-screenshot-border"; file = "privacy/desc.md"; from = 1068; to = 1070; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessGraphicsCaptureWithoutBorder"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Разрешить приложениям отключать желтую рамку захвата экрана"; cat = "privacy" },
    @{ id = "privacy-disable-apps-notifications-access"; file = "privacy/desc.md"; from = 1074; to = 1076; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessNotifications"; apply = 2; revMode = "delete"; revVal = $null; name_ru = "Запретить приложениям Windows доступ к системным уведомлениям"; cat = "privacy" },
    @{ id = "privacy-disable-apps-phone-call-access"; file = "privacy/desc.md"; from = 1075; to = 1077; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\AppPrivacy"; name = "LetAppsAccessPhone"; apply = 2; revMode = "delete"; revVal = $null; name_ru = "Запретить приложениям Windows доступ к совершению телефонных звонков"; cat = "privacy" },
    @{ id = "privacy-disable-sync-personalization"; file = "privacy/desc.md"; from = 1167; to = 1169; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\SettingSync"; name = "DisablePersonalizationSettingSync"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Отключить облачную синхронизацию настроек персонализации Windows"; cat = "privacy" },
    @{ id = "privacy-disable-sync-desktop-theme"; file = "privacy/desc.md"; from = 1170; to = 1172; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\SettingSync"; name = "DisableDesktopThemeSettingSync"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Отключить облачную синхронизацию темы оформления рабочего стола"; cat = "privacy" },
    @{ id = "privacy-disable-sync-browser"; file = "privacy/desc.md"; from = 1171; to = 1173; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\SettingSync"; name = "DisableWebBrowserSettingSync"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Отключить облачную синхронизацию данных и закладок браузера"; cat = "privacy" },
    @{ id = "privacy-disable-sync-paid-network"; file = "privacy/desc.md"; from = 1172; to = 1174; hive = "HKLM"; path = "Software\Policies\Microsoft\Windows\SettingSync"; name = "DisableSyncOnPaidNetwork"; apply = 1; revMode = "delete"; revVal = $null; name_ru = "Отключить синхронизацию параметров в сетях с оплатой за трафик"; cat = "privacy" },
    @{ id = "privacy-disable-search-cloud"; file = "privacy/desc.md"; from = 1228; to = 1230; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name = "AllowCloudSearch"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Отключить облачный поиск Microsoft в меню поиска Windows"; cat = "privacy" },
    @{ id = "privacy-disable-search-location"; file = "privacy/desc.md"; from = 1227; to = 1229; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name = "AllowSearchToUseLocation"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Запретить службе поиска Windows использовать данные о местоположении"; cat = "privacy" },
    @{ id = "privacy-disable-cortana-above-lock"; file = "privacy/desc.md"; from = 1226; to = 1228; hive = "HKLM"; path = "SOFTWARE\Policies\Microsoft\Windows\Windows Search"; name = "AllowCortanaAboveLock"; apply = 0; revMode = "delete"; revVal = $null; name_ru = "Запретить запуск голосового помощника на экране блокировки"; cat = "privacy" }
)

Write-Host "Total candidates: $($candidates.Count)"

$errors = 0
foreach ($c in $candidates) {
    if ($existingIds.Contains($c.id)) {
        Write-Host "FAIL: Duplicate id: $($c.id)" -ForegroundColor Red
        $errors++
    }

    $regKey = "$($c.hive)\$($c.path)\$($c.name)"
    if ($existingRegKeys.Contains($regKey)) {
        Write-Host "FAIL: Duplicate registry key: $regKey" -ForegroundColor Red
        $errors++
    }

    $fullPath = Join-Path $srcDir $c.file
    if (-not (Test-Path $fullPath)) {
        Write-Host "FAIL: Source file does not exist: $fullPath" -ForegroundColor Red
        $errors++
        continue
    }

    $lines = Get-Content $fullPath
    $sub = $lines[($c.from - 1)..($c.to - 1)] -join "`n"

    $hasNameOrPath = ($sub.IndexOf($c.name, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) -or ($sub.IndexOf($c.path, [System.StringComparison]::OrdinalIgnoreCase) -ge 0)
    if (-not $hasNameOrPath) {
        Write-Host "FAIL: Neither name nor path found in range $($c.file):$($c.from)..$($c.to) for tweak $($c.id)" -ForegroundColor Red
        $errors++
    }

    $hasType = $sub.IndexOf("Type:", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    if ($hasType) {
        Write-Host "WARNING: Range contains Type: line for $($c.id)" -ForegroundColor Yellow
        $errors++
    }
}

if ($errors -eq 0) {
    Write-Host "ALL 25 CANDIDATES ARE 100% VALID! ZERO ERRORS!" -ForegroundColor Green
} else {
    Write-Host "ERRORS FOUND: $errors" -ForegroundColor Red
}
