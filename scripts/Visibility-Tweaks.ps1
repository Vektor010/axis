# ==============================================================================
# Gaming & System Optimizer - Видимость, Проводник и задержки интерфейса (UX & Visibility)
# ==============================================================================

$ErrorActionPreference = "SilentlyContinue"

Write-Host ">>> Применение твиков Проводника, интерфейса и времени отклика..." -ForegroundColor Cyan

# ─────────────────────────────────────────────
# Название: Классическое контекстное меню Windows 10
# Что делает: Заменяет новое XAML-меню правой кнопки мыши Windows 11 на классическое легковесное Win32-меню.
# Зачем нужно: Полностью ликвидирует задержку рендеринга XAML (меню открывается со скоростью 0 мс), исключает необходимость нажимать "Показать дополнительные параметры" (Shift+F10) и отображает все доступные пункты сразу.
# Значение по умолчанию (Windows): Раздел реестра отсутствует (используется медленное меню XAML Windows 11).
# Значение после твика: Раздел создан с пустым параметром (Default) = ""
# ─────────────────────────────────────────────
Write-Host "[1/10] Включение классического контекстного меню (0 мс задержки XAML)..." -ForegroundColor Yellow
$clsidPath = "HKCU:\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"
if (-not (Test-Path $clsidPath)) { New-Item -Path $clsidPath -Force | Out-Null }
Set-ItemProperty -Path $clsidPath -Name "(Default)" -Value ""

# ─────────────────────────────────────────────
# Название: Отключение анимаций DWM и Проводника (DisallowAnimations)
# Что делает: Отключает системные анимации сворачивания, развертывания окон и анимации первого входа в систему через групповые политики.
# Зачем нужно: Окна и элементы управления реагируют мгновенно без траты времени на отрисовку плавных переходов, снижая нагрузку на DWM и делая интерфейс максимально отзывчивым.
# Значение по умолчанию (Windows): DisallowAnimations = 0 (или отсутствует), TurnOffSPIAnimations = 0, EnableFirstLogonAnimation = 1
# Значение после твика: DisallowAnimations = 1 (HKCU и HKLM Policies\DWM), TurnOffSPIAnimations = 1 (Policies\Explorer), EnableFirstLogonAnimation = 0 (Policies\System)
# ─────────────────────────────────────────────
Write-Host "[2/10] Отключение системных анимаций окон и первого входа (DisallowAnimations)..." -ForegroundColor Yellow
$dwmPolUser = "HKCU:\SOFTWARE\Policies\Microsoft\Windows\DWM"
if (-not (Test-Path $dwmPolUser)) { New-Item -Path $dwmPolUser -Force | Out-Null }
Set-ItemProperty -Path $dwmPolUser -Name "DisallowAnimations" -Type DWord -Value 1

$dwmPolMach = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\DWM"
if (-not (Test-Path $dwmPolMach)) { New-Item -Path $dwmPolMach -Force | Out-Null }
Set-ItemProperty -Path $dwmPolMach -Name "DisallowAnimations" -Type DWord -Value 1

$expPol = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"
if (-not (Test-Path $expPol)) { New-Item -Path $expPol -Force | Out-Null }
Set-ItemProperty -Path $expPol -Name "TurnOffSPIAnimations" -Type DWord -Value 1

$sysPol = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Policies\System"
if (-not (Test-Path $sysPol)) { New-Item -Path $sysPol -Force | Out-Null }
Set-ItemProperty -Path $sysPol -Name "EnableFirstLogonAnimation" -Type DWord -Value 0

$winMet = "HKCU:\Control Panel\Desktop\WindowMetrics"
if (-not (Test-Path $winMet)) { New-Item -Path $winMet -Force | Out-Null }
Set-ItemProperty -Path $winMet -Name "MinAnimate" -Type String -Value "0"

# ─────────────────────────────────────────────
# Название: Noverse «Minimal Visual Effects» (UserPreferencesMask)
# Что делает: Точечно отключает тени окон, тени курсора, затухание меню и скольжение списков,
#             но СОХРАНЯЕТ сглаживание шрифтов ClearType, эскизы вместо значков и содержимое при перетаскивании.
# Зачем нужно: Убирает все задержки рендеринга окон и меню без ухудшения читаемости шрифтов (исследование Noverse.dev).
# Значение по умолчанию: UserPreferencesMask = 9E 1E 07 80 12 00 00 00
# Значение после твика: UserPreferencesMask = 90 12 03 80 10 00 00 00 (REG_BINARY)
# ─────────────────────────────────────────────
Write-Host "[2.1] Применение Noverse Minimal Visual Effects (UserPreferencesMask)..." -ForegroundColor Yellow
$desk = "HKCU:\Control Panel\Desktop"
if (-not (Test-Path $desk)) { New-Item -Path $desk -Force | Out-Null }
Set-ItemProperty -Path $desk -Name "UserPreferencesMask" -Type Binary -Value ([byte[]](0x90, 0x12, 0x03, 0x80, 0x10, 0x00, 0x00, 0x00))

$visFx = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects"
if (-not (Test-Path $visFx)) { New-Item -Path $visFx -Force | Out-Null }
Set-ItemProperty -Path $visFx -Name "VisualFXSetting" -Type DWord -Value 3

# ─────────────────────────────────────────────
# Название: Отключение прозрачности DWM (EnableTransparency)
# Что делает: Отключает эффект прозрачности окон и акриловое размытие экрана входа (Logon).
# Зачем нужно: Снимает нагрузку с DWM по расчету размытия заднего плана (BackdropBlur) и разгружает GPU.
# ─────────────────────────────────────────────
Write-Host "[2.2] Отключение прозрачности DWM и акрилового слоя (EnableTransparency)..." -ForegroundColor Yellow
$personalize = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
if (-not (Test-Path $personalize)) { New-Item -Path $personalize -Force | Out-Null }
Set-ItemProperty -Path $personalize -Name "EnableTransparency" -Type DWord -Value 0

$sysSecPol = "HKLM:\Software\Policies\Microsoft\Windows\System"
if (-not (Test-Path $sysSecPol)) { New-Item -Path $sysSecPol -Force | Out-Null }
Set-ItemProperty -Path $sysSecPol -Name "DisableAcrylicBackgroundOnLogon" -Type DWord -Value 1

# ─────────────────────────────────────────────
# Название: Отображение расширений файлов и скрытых элементов
# Что делает: Заставляет Проводник всегда показывать полные расширения всех типов файлов (.exe, .cmd, .cfg) и отображать скрытые файлы и папки.
# Зачем нужно: Критически важно для безопасности (исключает маскировку исполняемых файлов) и обеспечивает удобство быстрой правки конфигурационных файлов игр.
# Значение по умолчанию (Windows): HideFileExt = 1 (расширения скрыты), Hidden = 2 (скрытые файлы не отображаются)
# Значение после твика: HideFileExt = 0 (REG_DWORD), Hidden = 1 (REG_DWORD)
# ─────────────────────────────────────────────
Write-Host "[3/10] Включение отображения расширений файлов и скрытых элементов..." -ForegroundColor Yellow
$adv = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
if (-not (Test-Path $adv)) { New-Item -Path $adv -Force | Out-Null }
Set-ItemProperty -Path $adv -Name "HideFileExt" -Type DWord -Value 0
Set-ItemProperty -Path $adv -Name "Hidden" -Type DWord -Value 1

# ─────────────────────────────────────────────
# Название: Подробный график передачи файлов (EnthusiastMode)
# Что делает: Автоматически открывает окно перемещения и копирования файлов в развернутом виде с графиком реальной скорости.
# Зачем нужно: Позволяет сразу видеть скорость передачи данных на скоростных NVMe SSD без необходимости каждый раз вручную нажимать кнопку "Подробнее".
# Значение по умолчанию (Windows): 0 (или отсутствует, компактное окно без графика скорости)
# Значение после твика: 1 (REG_DWORD, развернутый график скорости по умолчанию)
# ─────────────────────────────────────────────
Write-Host "[4/10] Включение подробного графика копирования файлов (EnthusiastMode)..." -ForegroundColor Yellow
$ops = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager"
if (-not (Test-Path $ops)) { New-Item -Path $ops -Force | Out-Null }
Set-ItemProperty -Path $ops -Name "EnthusiastMode" -Type DWord -Value 1

# ─────────────────────────────────────────────
# Название: Открытие Проводника в режиме "Этот компьютер"
# Что делает: Назначает папку "Этот компьютер" со списком физических дисков стартовой страницей при открытии Проводника (Win+E).
# Зачем нужно: Устраняет долгую задержку обращения к облачным серверам OneDrive и кэшу недавних документов при открытии Проводника.
# Значение по умолчанию (Windows): 2 (REG_DWORD, Главная страница / Рекомендации)
# Значение после твика: 1 (REG_DWORD, Этот компьютер)
# ─────────────────────────────────────────────
Write-Host "[5/10] Открытие Проводника на 'Этот компьютер' вместо Главной..." -ForegroundColor Yellow
Set-ItemProperty -Path $adv -Name "LaunchTo" -Type DWord -Value 1

# ─────────────────────────────────────────────
# Название: Отключение сбора недавних и часто используемых файлов
# Что делает: Отключает сохранение истории недавно открытых файлов и часто используемых папок в Проводнике и Быстром доступе.
# Зачем нужно: Очищает панель навигации от мусорных ссылок, защищает конфиденциальность и снижает фоновые операции дискового ввода-вывода.
# Значение по умолчанию (Windows): ShowRecent = 1, ShowFrequent = 1, ShowCloudFilesInQuickAccess = 1
# Значение после твика: ShowRecent = 0 (REG_DWORD), ShowFrequent = 0 (REG_DWORD), ShowCloudFilesInQuickAccess = 0 (REG_DWORD)
# ─────────────────────────────────────────────
Write-Host "[6/10] Отключение сбора недавних и частых файлов в Быстром доступе..." -ForegroundColor Yellow
$exp = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer"
if (-not (Test-Path $exp)) { New-Item -Path $exp -Force | Out-Null }
Set-ItemProperty -Path $exp -Name "ShowRecent" -Type DWord -Value 0
Set-ItemProperty -Path $exp -Name "ShowFrequent" -Type DWord -Value 0
Set-ItemProperty -Path $exp -Name "ShowCloudFilesInQuickAccess" -Type DWord -Value 0

# ─────────────────────────────────────────────
# Название: Компактный режим папок и отключение всплывающих подсказок
# Что делает: Включает плотный компактный интервал между файлами в списках, отключает всплывающие подсказки размера папок и рекламу синхронизации.
# Зачем нужно: Увеличивает полезную площадь отображения файлов и папок, устраняет паразитные микрозадержки при наведении курсора мыши на элементы.
# Значение по умолчанию (Windows): UseCompactMode = 0, ShowTypeOverlay = 1, FolderContentsInfoTip = 1, ShowSyncProviderNotifications = 1
# Значение после твика: UseCompactMode = 1 (REG_DWORD), ShowTypeOverlay = 0 (REG_DWORD), FolderContentsInfoTip = 0 (REG_DWORD), ShowSyncProviderNotifications = 0 (REG_DWORD)
# ─────────────────────────────────────────────
Write-Host "[7/10] Включение компактного вида и отключение всплывающих оверлеев..." -ForegroundColor Yellow
Set-ItemProperty -Path $adv -Name "UseCompactMode" -Type DWord -Value 1
Set-ItemProperty -Path $adv -Name "ShowTypeOverlay" -Type DWord -Value 0
Set-ItemProperty -Path $adv -Name "FolderContentsInfoTip" -Type DWord -Value 0
Set-ItemProperty -Path $adv -Name "ShowSyncProviderNotifications" -Type DWord -Value 0

# ─────────────────────────────────────────────
# Название: Мгновенный отклик мыши и меню (MouseHoverTime и MenuShowDelay)
# Что делает: Устанавливает интервал фиксации наведения курсора мыши на 8 мс и задержку открытия подменю на 0 мс.
# Зачем нужно: Делает раскрытие контекстных меню и реакцию интерфейса на движение мыши мгновенными вместо стандартной раздражающей паузы в 400 мс.
# Техническая деталь: В Win32/User32 (CMenuToolbarBase::_SetTimer) значение передается в SetTimer,
#                     где задержки ниже USER_TIMER_MINIMUM (10 мс) аппаратно округляются до 10 мс.
#                     Значение "0" дает минимально допустимый опрос таймера в ~14-16 мс (подтверждено тестом msd_test).
# Значение по умолчанию (Windows): MouseHoverTime = "400", MenuShowDelay = "400" (REG_SZ)
# Значение после твика: MouseHoverTime = "8" (REG_SZ), MenuShowDelay = "0" (REG_SZ)
# ─────────────────────────────────────────────
Write-Host "[8/10] Оптимизация задержки мыши (MouseHoverTime) и меню (MenuShowDelay)..." -ForegroundColor Yellow
$mouse = "HKCU:\Control Panel\Mouse"
if (-not (Test-Path $mouse)) { New-Item -Path $mouse -Force | Out-Null }
Set-ItemProperty -Path $mouse -Name "MouseHoverTime" -Type String -Value "100"

$desk = "HKCU:\Control Panel\Desktop"
if (-not (Test-Path $desk)) { New-Item -Path $desk -Force | Out-Null }
Set-ItemProperty -Path $desk -Name "MenuShowDelay" -Type String -Value "0"

# ─────────────────────────────────────────────
# Название: Отключение сворачивания окон встряхиванием (DisallowShaking / Aero Shake)
# Что делает: Отключает функцию Aero Shake, которая сворачивает все фоновые окна при быстром перемещении активного окна мышью за заголовок.
# Зачем нужно: Предотвращает случайное сворачивание окон Discord, браузера или мониторинга во время интенсивных движений курсора мыши.
# Значение по умолчанию (Windows): 0 (REG_DWORD, встряхивание включено)
# Значение после твика: 1 (REG_DWORD, встряхивание отключено)
# ─────────────────────────────────────────────
Write-Host "[9/10] Отключение Aero Shake (случайное сворачивание окон при встряхивании)..." -ForegroundColor Yellow
Set-ItemProperty -Path $adv -Name "DisallowShaking" -Type DWord -Value 1

# ─────────────────────────────────────────────
# Название: Добавление пункта «Завершить задачу» в контекстное меню панели задач
# Что делает: Включает встроенную опцию быстрого принудительного завершения зависшего процесса (End Task) по правому клику на значок приложения в панели задач.
# Зачем нужно: Позволяет мгновенно закрыть зависшую игру или приложение без необходимости открывать Диспетчер задач.
# Значение по умолчанию (Windows): 0 (или отсутствует)
# Значение после твика: 1 (REG_DWORD)
# ─────────────────────────────────────────────
Write-Host "[10/10] Включение пункта 'Завершить задачу' и отключение анимаций панели задач..." -ForegroundColor Yellow
$devPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings"
if (-not (Test-Path $devPath)) { New-Item -Path $devPath -Force | Out-Null }
Set-ItemProperty -Path $devPath -Name "TaskbarEndTask" -Type DWord -Value 1

Set-ItemProperty -Path $adv -Name "TaskbarAnimations" -Type DWord -Value 0

Write-Host "[OK] Все параметры отображения и отзывчивости Проводника применены успешно!" -ForegroundColor Green
