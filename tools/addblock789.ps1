[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$jsonPath = "C:\Users\User\Documents\win-tweaker\data\tweaks.json"
$content = Get-Content -Path $jsonPath -Raw -Encoding UTF8
$items = [System.Collections.ArrayList]@($content | ConvertFrom-Json)
Write-Host "Current tweaks count: $($items.Count)"

function New-TweakItem {
    param(
        [string]$id,
        [string]$nameRu,
        [string]$descRu,
        [string]$category,
        [string]$group,
        [string]$sourceFile,
        [int]$lineFrom,
        [int]$lineTo,
        [string]$hive,
        [string]$path,
        [string]$name,
        [string]$type,
        $applyVal,
        $revertVal,
        [string[]]$presets
    )

    $obj = [PSCustomObject]@{
        id = $id
        name_ru = $nameRu
        description_ru = $descRu
        group = $group
        category = $category
        type = "toggle"
        risk = "low"
        presets = @($presets)
        min_build = $null
        needs_review = $true
        source = [PSCustomObject]@{
            file = $sourceFile
            line_from = $lineFrom
            line_to = $lineTo
        }
        registry = @(
            [PSCustomObject]@{
                hive = $hive
                path = $path
                name = $name
                type = $type
                apply = [PSCustomObject]@{
                    mode = "set"
                    value = $applyVal
                }
                revert = [PSCustomObject]@{
                    mode = "set"
                    value = $revertVal
                }
            }
        )
    }
    return $obj
}

# 1. system-disable-paging-executive
$items.Add((New-TweakItem `
    "system-disable-paging-executive" `
    "Запретить сброс ядра в файл подкачки (DisablePagingExecutive)" `
    "Удерживает исполняемый код ntoskrnl и драйверы ядра строго в физической оперативной памяти (DisablePagingExecutive = 1), полностью устраняя задержки при системных вызовах." `
    "system" `
    "Управление памятью и кэшем" `
    "system/desc.md" `
    3543 3545 `
    "HKLM" "SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" "DisablePagingExecutive" "REG_DWORD" 1 0 `
    @("recommended")))

# 2. system-disable-page-combining
$items.Add((New-TweakItem `
    "system-disable-page-combining" `
    "Отключить дедупликацию памяти (DisablePageCombining)" `
    "Отключает фоновое сканирование одинаковых страниц оперативной памяти процессором (DisablePageCombining = 1), снижая непроизводительную нагрузку на ядра CPU." `
    "system" `
    "Управление памятью и кэшем" `
    "system/desc.md" `
    3542 3544 `
    "HKLM" "SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" "DisablePageCombining" "REG_DWORD" 1 0 `
    @("recommended")))

# 3. system-disable-cache-telemetry
$items.Add((New-TweakItem `
    "system-disable-cache-telemetry" `
    "Отключить телеметрию кэша ФС (DisableCacheTelemetry)" `
    "Отключает сбор внутренней диагностической телеметрии кэша системных файлов (CcDisableTelemetryRegKeyAtInit = 1), уменьшая фоновую активность накопителя." `
    "system" `
    "Управление памятью и кэшем" `
    "system/desc.md" `
    3541 3543 `
    "HKLM" "SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" "DisableCacheTelemetry" "REG_DWORD" 1 0 `
    @("recommended")))

# 4. system-bcd-bootux-disabled
$items.Add((New-TweakItem `
    "system-bcd-bootux-disabled" `
    "Отключить графический экран загрузки (bootuxdisabled)" `
    "Отключает экран приветствия и полосу загрузки при старте операционной системы (bootuxdisabled = true), ускоряя инициализацию Windows на быстрых SSD." `
    "system" `
    "Загрузка и BCD" `
    "system/desc.md" `
    7805 7807 `
    "HKLM" "BCD00000000\Objects\{default}\Elements\1600006C" "Element" "REG_BINARY" "01" "00" `
    @("recommended")))

# 5. system-bcd-bootmenupolicy-legacy
$items.Add((New-TweakItem `
    "system-bcd-bootmenupolicy-legacy" `
    "Классическое меню загрузки F8 (bootmenupolicy = Legacy)" `
    "Возвращает классический текстовый загрузчик Windows с вызовом безопасного режима по клавише F8 без перехода в тяжелый графический интерфейс WinRE." `
    "system" `
    "Загрузка и BCD" `
    "system/desc.md" `
    7852 7854 `
    "HKLM" "BCD00000000\Objects\{default}\Elements\25000008" "Element" "REG_BINARY" "0000000000000000" "0100000000000000" `
    @("recommended")))

# 6. system-bcd-highestmode
$items.Add((New-TweakItem `
    "system-bcd-highestmode" `
    "Максимальное графическое разрешение UEFI (highestmode)" `
    "Принудительно устанавливает наивысшее доступное экранное разрешение видеоадаптера на этапе ранней загрузки UEFI (highestmode = true)." `
    "system" `
    "Загрузка и BCD" `
    "system/desc.md" `
    7809 7811 `
    "HKLM" "BCD00000000\Objects\{default}\Elements\16000054" "Element" "REG_BINARY" "01" "00" `
    @("recommended")))

# 7. system-bcd-disable-loading-circle
$items.Add((New-TweakItem `
    "system-bcd-disable-loading-circle" `
    "Отключить вращающийся круг загрузки (Custom 16000069)" `
    "Отключает циклическую анимацию точек при загрузке ядра Windows (custom:16000069 = true), исключая микрозадержки видеодрайвера при инициализации." `
    "system" `
    "Загрузка и BCD" `
    "system/desc.md" `
    7806 7808 `
    "HKLM" "BCD00000000\Objects\{default}\Elements\16000069" "Element" "REG_BINARY" "01" "00" `
    @("recommended")))

# 8. system-bcd-disable-boot-logo
$items.Add((New-TweakItem `
    "system-bcd-disable-boot-logo" `
    "Отключить логотип Windows при старте (Custom 16000067)" `
    "Отключает отображение системного логотипа при инициализации ядра (custom:16000067 = true), делая запуск бесшовным и быстрым." `
    "system" `
    "Загрузка и BCD" `
    "system/desc.md" `
    7807 7809 `
    "HKLM" "BCD00000000\Objects\{default}\Elements\16000067" "Element" "REG_BINARY" "01" "00" `
    @("recommended")))

# 9. system-timer-watchdog-disable
$items.Add((New-TweakItem `
    "system-timer-watchdog-disable" `
    "Отключить аппаратный сторожевой таймер (SYSTEMWATCHDOGPOLICY=DISABLED)" `
    "Отключает прерывания сторожевого таймера HAL (HalpTimerWatchdogDisable = 1), предотвращая регулярные всплески задержек DPC в соревновательных играх." `
    "system" `
    "Системные таймеры и тактовая частота" `
    "system/desc.md" `
    8068 8072 `
    "HKLM" "SYSTEM\CurrentControlSet\Control\Session Manager\kernel" "SYSTEMWATCHDOGPOLICY" "REG_SZ" "DISABLED" "ENABLED" `
    @("recommended")))

# 10. system-timer-force-tsc
$items.Add((New-TweakItem `
    "system-timer-force-tsc" `
    "Использовать быстрый процессорный счетчик TSC (USEPLATFORMCLOCK=No)" `
    "Запрещает принудительное чтение медленного внешнего таймера HPET системной платы, переключая систему на прямой аппаратный счетчик тактов TSC." `
    "system" `
    "Системные таймеры и тактовая частота" `
    "system/desc.md" `
    8087 8090 `
    "HKLM" "SYSTEM\CurrentControlSet\Control\Session Manager\kernel" "USEPLATFORMCLOCK" "REG_SZ" "No" "Yes" `
    @("recommended")))

# 11. affinities-msi-mode-gpu
$items.Add((New-TweakItem `
    "affinities-msi-mode-gpu" `
    "Включить режим MSI (Message Signaled Interrupts) для видеокарты" `
    "Переводит графический процессор в режим векторных прерываний MSI (MSISupported = 1), устраняя конфликты общих линий IRQ и стабилизируя фреймтайм." `
    "affinities" `
    "Режим MSI и прерывания" `
    "power/desc.md" `
    1154 1157 `
    "HKLM" "SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties" "MSISupported" "REG_DWORD" 1 0 `
    @("recommended")))

# 12. affinities-gpu-device-priority
$items.Add((New-TweakItem `
    "affinities-gpu-device-priority" `
    "Высокий приоритет прерываний видеокарты (DevicePriority = High)" `
    "Устанавливает приоритет обработки прерываний видеокарты на высокий уровень (DevicePriority = 3 / High), сокращая инпут-лаг при рендеринге кадров." `
    "affinities" `
    "Режим MSI и прерывания" `
    "power/desc.md" `
    1164 1168 `
    "HKLM" "SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy" "DevicePriority" "REG_DWORD" 3 0 `
    @("recommended")))

# 13. affinities-nic-device-priority
$items.Add((New-TweakItem `
    "affinities-nic-device-priority" `
    "Высокий приоритет прерываний сетевой карты (NIC DevicePriority)" `
    "Задает высокий приоритет прерываний для сетевого адаптера (DevicePriority = 3 / High), ускоряя цикл обработки сетевых пакетов на уровне ядра." `
    "affinities" `
    "Режим MSI и прерывания" `
    "power/desc.md" `
    1164 1168 `
    "HKLM" "SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy" "DevicePriority" "REG_DWORD" 3 0 `
    @("recommended")))

# 14. affinities-device-policy-spread
$items.Add((New-TweakItem `
    "affinities-device-policy-spread" `
    "Многоядерное распределение MSI-X (DevicePolicy = Spread)" `
    "Включает равномерное распределение векторов прерываний контроллеров по всем доступным ядрам CPU (DevicePolicy = 5), исключая троттлинг Core 0." `
    "affinities" `
    "Политика распределения прерываний" `
    "power/desc.md" `
    1164 1168 `
    "HKLM" "SYSTEM\CurrentControlSet\Enum\PCI\...\Device Parameters\Interrupt Management\Affinity Policy" "DevicePolicy" "REG_DWORD" 5 0 `
    @("recommended")))

# 15. misc-disable-onedrive-file-sync
$items.Add((New-TweakItem `
    "misc-disable-onedrive-file-sync" `
    "Запретить использование OneDrive для хранения файлов" `
    "Полностью отключает интеграцию облака OneDrive в Windows (DisableFileSyncNGSC = 1), предотвращая фоновую синхронизацию и дисковую нагрузку." `
    "misc" `
    "OneDrive и облачные службы" `
    "misc/desc.md" `
    356 358 `
    "HKLM" "Software\Policies\Microsoft\Windows\OneDrive" "DisableFileSyncNGSC" "REG_DWORD" 1 0 `
    @("recommended")))

# 16. misc-disable-onedrive-network-traffic
$items.Add((New-TweakItem `
    "misc-disable-onedrive-network-traffic" `
    "Запретить сетевой трафик OneDrive до входа" `
    "Блокирует отправку предварительных сетевых запросов службой OneDrive до авторизации пользователя (PreventNetworkTrafficPreUserSignIn = 1)." `
    "misc" `
    "OneDrive и облачные службы" `
    "misc/desc.md" `
    357 359 `
    "HKLM" "SOFTWARE\Microsoft\OneDrive" "PreventNetworkTrafficPreUserSignIn" "REG_DWORD" 1 0 `
    @("recommended")))

# 17. misc-7zip-cascaded-menu
$items.Add((New-TweakItem `
    "misc-7zip-cascaded-menu" `
    "Каскадное контекстное меню 7-Zip (CascadedMenu)" `
    "Объединяет все операции архиватора 7-Zip в компактное каскадное подменю Проводника, очищая контекстное меню от визуального мусора." `
    "misc" `
    "Архиваторы и контекстное меню" `
    "misc/desc.md" `
    304 306 `
    "HKCU" "Software\7-Zip\Options" "CascadedMenu" "REG_DWORD" 1 0 `
    @("recommended")))

# 18. misc-7zip-eliminate-root-folder-duplication
$items.Add((New-TweakItem `
    "misc-7zip-eliminate-root-folder-duplication" `
    "Устранять дублирование корневых папок при распаковке 7-Zip" `
    "Предотвращает создание вложенных одноименных папок при извлечении архивов 7-Zip (ElimDupExtract = 1), наводя порядок в файлах." `
    "misc" `
    "Архиваторы и контекстное меню" `
    "misc/desc.md" `
    304 306 `
    "HKCU" "Software\7-Zip\Options" "ElimDupExtract" "REG_DWORD" 1 0 `
    @("recommended")))

# 19. misc-7zip-menu-icons
$items.Add((New-TweakItem `
    "misc-7zip-menu-icons" `
    "Отображать значки 7-Zip в контекстном меню" `
    "Включает наглядные миниатюрные иконки рядом с пунктами 7-Zip в контекстном меню Проводника (MenuIcons = 1)." `
    "misc" `
    "Архиваторы и контекстное меню" `
    "misc/desc.md" `
    304 306 `
    "HKCU" "Software\7-Zip\Options" "MenuIcons" "REG_DWORD" 1 0 `
    @("recommended")))

# 20. misc-startallback-noxaml-menus
$items.Add((New-TweakItem `
    "misc-startallback-noxaml-menus" `
    "Отключить медленные WinUI/XAML меню в StartAllBack (NoXAMLMenus)" `
    "Отключает тяжелый рендеринг XAML в контекстных меню Проводника и панели задач, возвращая мгновенный Win32 отклик." `
    "misc" `
    "Интерфейсные утилиты (StartAllBack)" `
    "misc/desc.md" `
    63 65 `
    "HKCU" "Software\StartIsBack" "NoXAMLMenus" "REG_DWORD" 1 0 `
    @("recommended")))

# 21. misc-startallback-undead-control-panel
$items.Add((New-TweakItem `
    "misc-startallback-undead-control-panel" `
    "Классическая Панель управления в StartAllBack (UndeadControlPanel)" `
    "Сохраняет доступ к классической панели управления Windows без редиректа в громоздкое UWP-приложение Параметры." `
    "misc" `
    "Интерфейсные утилиты (StartAllBack)" `
    "misc/desc.md" `
    63 65 `
    "HKCU" "Software\StartIsBack" "UndeadControlPanel" "REG_DWORD" 1 0 `
    @("recommended")))

# 22. misc-startallback-navbar-glass
$items.Add((New-TweakItem `
    "misc-startallback-navbar-glass" `
    "Эффект Mica/Acrylic для навигационной панели (NavBarGlass)" `
    "Включает аппаратный эффект прозрачности Mica/Glass для адресной строки и панели управления Проводника (NavBarGlass = 1)." `
    "misc" `
    "Интерфейсные утилиты (StartAllBack)" `
    "misc/desc.md" `
    63 65 `
    "HKCU" "Software\StartIsBack" "NavBarGlass" "REG_DWORD" 1 0 `
    @("recommended")))

Write-Host "New tweaks count: $($items.Count)"

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$jsonText = $items | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText($jsonPath, $jsonText, $utf8NoBom)
Write-Host "File successfully written to $jsonPath"
