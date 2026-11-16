param(
    [string]$Revision = "HEAD"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
Set-Location $rootDir

# Список файлов, измененных в рабочей копии или в коммите
$changedFiles = git status --porcelain | Where-Object { $_ -notmatch '^\?\?\s+(scratch|tests/out)' } | ForEach-Object {
    if ($_.Length -ge 4) {
        $_.Substring(3).Trim()
    }
}

if (-not $changedFiles -or $changedFiles.Count -eq 0) {
    $changedFiles = git diff --name-only HEAD~1 HEAD
}

$descriptions = @{
    "tools/list-tweaks.ps1" = "Вывод 10 колонок параметров и сырых строк источника для всех твиков tweaks.json"
    "tools/scan-candidates.ps1" = "Механический сканер 31 кандидатной группы visibility и security с вычислением include/exclude"
    "App.xaml.cs" = "Точка входа Main, замер времени старта и памяти, аргументы --diag-assemblies, --checkpoints, --screenshot"
    "MainWindow.xaml.cs" = "Окно WPF: замер ContentRenderedUtc, флаг --diag-mem, отписка RegistryWatcher"
    "Tweaker.csproj" = "Проект WPF: App.xaml как Page для явного Main(), ресурсы и манифест"
    "data/tweaks.json" = "База из 26 проверенных твиков среза (25 visibility + 1 security), 809 строк"
    "src/Common/NativeMethods.cs" = "Win32 P/Invoke: RegNotifyChangeKeyValue, DwmSetWindowAttribute, проверка HRESULT"
    "src/Engine/RestorePointManager.cs" = "Управление точками восстановления: TestModeProcessRunner, проверка DisableSR, кулдаун 24ч"
    "src/Engine/StateManager.cs" = "Состояние приложения: персистентное сохранение LastRestorePointUtc, защита от записи в тестах"
    "src/Engine/TweakEngine.cs" = "Движок применения твиков: интеграция вызова точки восстановления перед Apply"
    "src/ViewModels/TweakCardViewModel.cs" = "Карточка твика: заглушка 'Описание в источнике отсутствует'"
    "Tests/Program.cs" = "Автоматические тесты (575 тестов): валидация источника, 5 сценариев DisableSR, изоляция"
    "Tests/baseline/App.xaml.cs" = "Точка входа Main в Baseline, замеры памяти и задержек"
    "Tests/baseline/BaselineWindow.xaml.cs" = "Окно Baseline: флаг --keep-open, замер WorkingSet64, дамп baseline-modules.txt, флаг --diag-mem"
    "Tests/dump-source.ps1" = "Скрипт дампа точных диапазонов строк источника win-config для 13 твиков среза"
    "Tests/files-table.ps1" = "Скрипт вывода таблицы измененных файлов этапа, их размеров и SHA-256"
    "Tests/sample-ws.ps1" = "Замер памяти и счетчиков процесса снаружи каждые 0.5 с до 20 с (3 прогона Tweaker и Baseline)"
    "Tests/negative.ps1" = "11 негативных сценариев мутаций с diff и проверкой точного равенства множеств сбоев"
    "Tests/benchmark_memory.ps1" = "Удаленный дубликат скрипта замера памяти (удален через git rm)"
    "docs/freshness.md" = "Анализ свежести данных: удалены упоминания Cloudflare, заменены на нейтральные описания сервера"
    "docs/known-issues.md" = "Реестр дефектов Д1-Д9: нейтральное описание ответов веб-сервера"
    "docs/retractions.md" = "Официальный реестр опровержений и исправлений неточных утверждений (записи 1-59)"
    "docs/prototype/screenshot-dark.png" = "Скриншот приложения: темная тема"
    "docs/prototype/screenshot-light.png" = "Скриншот приложения: светлая тема"
    "docs/prototype/screenshot-search.png" = "Скриншот приложения: фильтрация поиска"
    "docs/prototype/screenshot-settings.png" = "Скриншот приложения: вкладка настроек"
    "docs/prototype/screenshot-journal.png" = "Скриншот приложения: вкладка журнала"
}

$rows = @()
foreach ($relPath in $changedFiles) {
    if ([string]::IsNullOrEmpty($relPath)) { continue }
    $cleanPath = $relPath -replace '/', '\'
    $fullPath = Join-Path $rootDir $cleanPath
    $desc = if ($descriptions.ContainsKey($relPath)) { $descriptions[$relPath] } else { "Файл этапа 3.4" }

    if (Test-Path $fullPath) {
        $fi = Get-Item $fullPath
        $hash = (Get-FileHash $fullPath -Algorithm SHA256).Hash
        $rows += [PSCustomObject]@{
            "Файл" = $relPath
            "Размер (байт)" = $fi.Length
            "SHA-256" = $hash
            "Назначение" = $desc
        }
    } else {
        $rows += [PSCustomObject]@{
            "Файл" = $relPath
            "Размер (байт)" = "Удален"
            "SHA-256" = "-"
            "Назначение" = $desc
        }
    }
}

$rows | Format-Table -AutoSize
