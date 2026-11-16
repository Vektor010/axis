<#
.SYNOPSIS
    Настройка запуска Панели управления NVIDIA по требованию (On-Demand NVCPL).
    Основано на скрипте nvcpl.ps1 от Noverse (https://noverse.dev).

.DESCRIPTION
    Отключает службы фоновой телеметрии и контейнеров NVDisplay.ContainerLocalSystem
    и NvTelemetryContainer, высвобождая ресурсы процессора, оперативной памяти и
    устраняя лишние сетевые сокеты и задержки ввода.
    При запуске панели через создаваемый ярлык службы временно активируются,
    а после закрытия окна nvcplui.exe автоматически принудительно выгружаются.

.PARAMETER Action
    Действие: 'Install' (настроить On-Demand), 'Uninstall' (вернуть стандартный автозапуск), 'Status'.
    По умолчанию: 'Install'.

.PARAMETER TargetDir
    Каталог для лончера и вспомогательных файлов.
#>

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [ValidateSet('Install', 'Uninstall', 'Status')]
    [string]$Action = 'Install',

    [Parameter()]
    [string]$TargetDir = "$env:LOCALAPPDATA\WinTweaker\NVCPL"
)

# Проверка прав администратора (UAC Elevation)
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "[!] Запрос прав администратора для настройки служб NVIDIA..." -ForegroundColor Yellow
    $scriptArgs = "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`""
    if ($Action) {
        $scriptArgs += " -Action $Action"
    }
    Start-Process powershell.exe -ArgumentList $scriptArgs -Verb RunAs
    exit
}

$desktop = [Environment]::GetFolderPath('Desktop')
$services = @('NVDisplay.ContainerLocalSystem', 'NvTelemetryContainer')

switch ($Action) {
    'Install' {
        Write-Host "==========================================================" -ForegroundColor Cyan
        Write-Host " [WinTweaker] Настройка On-Demand NVIDIA Control Panel    " -ForegroundColor Cyan
        Write-Host "==========================================================" -ForegroundColor Cyan

        # 1. Поиск исполняемого файла nvcplui.exe
        $foundNvcpl = $null
        $progX86 = ${env:ProgramFiles(x86)}
        $possiblePaths = @(
            "$env:ProgramFiles\NVIDIA Corporation\Control Panel Client\nvcplui.exe",
            "$progX86\NVIDIA Corporation\Control Panel Client\nvcplui.exe",
            "$TargetDir\nvcplui.exe",
            "$env:LOCALAPPDATA\Noverse\nvcplui.exe"
        )

        foreach ($p in $possiblePaths) {
            if (-not [string]::IsNullOrEmpty($p) -and (Test-Path -Path $p)) {
                $foundNvcpl = $p
                break
            }
        }

        # Проверка DCH / UWP пакета
        if (-not $foundNvcpl) {
            try {
                $appx = Get-AppxPackage -Name "*NVIDIACorp.NVIDIAControlPanel*" -ErrorAction SilentlyContinue
                if ($appx -and $appx.InstallLocation) {
                    $cand = Join-Path $appx.InstallLocation "nvcplui.exe"
                    if (Test-Path $cand) {
                        $foundNvcpl = $cand
                    }
                }
            } catch { }
        }

        if (-not (Test-Path $TargetDir)) {
            New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
        }

        $targetExe = Join-Path $TargetDir "nvcplui.exe"
        $targetIco = Join-Path $TargetDir "Nvcpl.ico"

        if (-not $foundNvcpl) {
            Write-Host "[*] Локальный nvcplui.exe не найден, загрузка автономной сборки..." -ForegroundColor Yellow
            try {
                Invoke-WebRequest -Uri "https://github.com/nohuto/Files/releases/download/driver/nvcplui.exe" -OutFile $targetExe -UseBasicParsing -ErrorAction Stop | Out-Null
                $foundNvcpl = $targetExe
            } catch {
                Write-Warning ("Не удалось загрузить автономный nvcplui.exe: {0}" -f $_.Exception.Message)
            }
        } else {
            Write-Host ("[+] Обнаружен nvcplui.exe: {0}" -f $foundNvcpl) -ForegroundColor Green
        }

        # Загрузка иконки
        if (-not (Test-Path $targetIco)) {
            try {
                Invoke-WebRequest -Uri "https://github.com/nohuto/Files/releases/download/driver/Nvcpl.ico" -OutFile $targetIco -UseBasicParsing -ErrorAction SilentlyContinue | Out-Null
            } catch { }
        }

        # 2. Остановка и отключение служб
        foreach ($svcName in $services) {
            try {
                $s = Get-Service -Name $svcName -ErrorAction SilentlyContinue
                if ($s) {
                    if ($s.Status -eq 'Running') {
                        Stop-Service -Name $svcName -Force -ErrorAction SilentlyContinue
                        Write-Host ("[+] Фоновая служба {0} остановлена." -f $svcName) -ForegroundColor Green
                    }
                    Set-Service -Name $svcName -StartupType Disabled -ErrorAction SilentlyContinue
                    Write-Host ("[+] Служба {0} переведена в режим 'Отключена'." -f $svcName) -ForegroundColor Green
                }
            } catch {
                Write-Warning ("Не удалось изменить параметры службы {0}: {1}" -f $svcName, $_.Exception.Message)
            }
        }

        # 3. Генерация wrapper-скрипта запуска
        $launcherScript = Join-Path $TargetDir "NV-nvcpl.ps1"
        $nvcplTarget = if ($foundNvcpl) { $foundNvcpl } else { $targetExe }

        $wrapperContent = @"
`$ErrorActionPreference = 'SilentlyContinue'
`$nvsrv = @('NVDisplay.ContainerLocalSystem', 'NvContainerLocalSystem')
foreach (`$s in `$nvsrv) {
    if (Get-Service -Name `$s -ErrorAction SilentlyContinue) {
        Set-Service -Name `$s -StartupType Manual -ErrorAction SilentlyContinue
        if ((Get-Service -Name `$s).Status -ne 'Running') {
            Start-Service -Name `$s -ErrorAction SilentlyContinue
        }
    }
}
Start-Process -FilePath "$nvcplTarget" -Wait
foreach (`$s in `$nvsrv) {
    if (Get-Service -Name `$s -ErrorAction SilentlyContinue) {
        if ((Get-Service -Name `$s).Status -eq 'Running') {
            Stop-Service -Name `$s -Force -ErrorAction SilentlyContinue
        }
        Set-Service -Name `$s -StartupType Disabled -ErrorAction SilentlyContinue
    }
}
"@

        [System.IO.File]::WriteAllText($launcherScript, $wrapperContent, [System.Text.Encoding]::UTF8)
        Write-Host ("[+] Скрипт On-Demand лончера создан: {0}" -f $launcherScript) -ForegroundColor Green

        # 4. Создание ярлыков на рабочем столе
        try {
            $wsh = New-Object -ComObject WScript.Shell
            $shortcutNames = @("Панель управления NVIDIA.lnk", "Nvcpl.lnk")
            foreach ($sName in $shortcutNames) {
                $sFile = Join-Path $desktop $sName
                $shortcut = $wsh.CreateShortcut($sFile)
                $shortcut.TargetPath = "powershell.exe"
                $shortcut.Arguments = "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File `"$launcherScript`""
                $shortcut.WorkingDirectory = $TargetDir
                if (Test-Path $targetIco) {
                    $shortcut.IconLocation = $targetIco
                } elseif (Test-Path $nvcplTarget) {
                    $shortcut.IconLocation = ("{0},0" -f $nvcplTarget)
                }
                $shortcut.Description = "Панель управления NVIDIA (запуск по требованию)"
                $shortcut.Save()
                Write-Host ("[+] Создан ярлык на рабочем столе: {0}" -f $sFile) -ForegroundColor Green
            }
        } catch {
            Write-Warning ("Не удалось создать ярлык: {0}" -f $_.Exception.Message)
        }

        Write-Host "[OK] Настройка On-Demand NVCPL завершена успешно!" -ForegroundColor Cyan
    }

    'Uninstall' {
        Write-Host "==========================================================" -ForegroundColor Cyan
        Write-Host " [WinTweaker] Откат On-Demand NVCPL к стандартным службам  " -ForegroundColor Cyan
        Write-Host "==========================================================" -ForegroundColor Cyan

        # 1. Восстановление служб в автоматический режим
        foreach ($svcName in $services) {
            try {
                $s = Get-Service -Name $svcName -ErrorAction SilentlyContinue
                if ($s) {
                    Set-Service -Name $svcName -StartupType Automatic -ErrorAction SilentlyContinue
                    Start-Service -Name $svcName -ErrorAction SilentlyContinue
                    Write-Host ("[+] Служба {0} переведена в автозапуск и запущена." -f $svcName) -ForegroundColor Green
                }
            } catch {
                Write-Warning ("Не удалось восстановить службу {0}: {1}" -f $svcName, $_.Exception.Message)
            }
        }

        # 2. Удаление ярлыков
        $shortcutNames = @("Панель управления NVIDIA.lnk", "Nvcpl.lnk")
        foreach ($sName in $shortcutNames) {
            $sFile = Join-Path $desktop $sName
            if (Test-Path $sFile) {
                Remove-Item -Path $sFile -Force -ErrorAction SilentlyContinue
                Write-Host ("[+] Удален ярлык: {0}" -f $sFile) -ForegroundColor Green
            }
        }

        Write-Host "[OK] Стандартный запуск служб NVIDIA восстановлен!" -ForegroundColor Cyan
    }

    'Status' {
        Write-Host "=== Статус служб NVIDIA ===" -ForegroundColor White
        foreach ($svcName in $services) {
            $s = Get-Service -Name $svcName -ErrorAction SilentlyContinue
            if ($s) {
                Write-Host ("  {0}: Статус={1}, Запуск={2}" -f $s.Name, $s.Status, $s.StartType) -ForegroundColor White
            } else {
                Write-Host ("  {0}: Не установлена" -f $svcName) -ForegroundColor DarkGray
            }
        }
    }
}
