Set-Location C:\Users\User\Documents\win-tweaker

[xml]$sitemap = Get-Content docs/sitemap-0.xml
$urls = $sitemap.urlset.url.loc

$sources = @()

# 1. URLs from website (269 URLs)
foreach ($u in $urls) {
    $kind = "site-page"
    $category = "root"
    $classification = "reference"
    $engineTarget = "none"
    $notes = ""
    
    if ($u -match 'noverse\.dev/docs/win-config/([^/]+)/([^/]+)/') {
        $category = "win-config/" + $matches[1]
        $classification = "tweak-source"
        $engineTarget = "v1-tweak-engine"
        $notes = "Страница твика Windows в категории " + $matches[1]
    } elseif ($u -match 'noverse\.dev/docs/win-config/([^/]+)/') {
        $category = "win-config/" + $matches[1]
        $classification = "tweak-source"
        $engineTarget = "v1-metadata"
        $notes = "Категорийная страница раздела " + $matches[1]
    } elseif ($u -match 'noverse\.dev/docs/win-config/') {
        $category = "win-config"
        $classification = "tweak-source"
        $engineTarget = "v1-metadata"
        $notes = "Главный индекс документации win-config"
    } elseif ($u -match 'noverse\.dev/docs/app-guides/([^/]+)/') {
        $category = "app-guides"
        $classification = "app-config"
        $engineTarget = "v2-app-guide"
        $notes = "Руководство по настройке стороннего приложения: " + $matches[1]
    } elseif ($u -match 'noverse\.dev/docs/app-guides/') {
        $category = "app-guides"
        $classification = "app-config"
        $engineTarget = "v2-app-guide"
        $notes = "Индекс раздела руководств по сторонним приложениям"
    } elseif ($u -match 'noverse\.dev/docs/regkit/') {
        $category = "regkit"
        $classification = "reference"
        $engineTarget = "none"
        $notes = "Документация и справочник архитектуры реестра RegKit"
    } elseif ($u -match 'noverse\.dev/docs/windbg-notes/') {
        $category = "windbg-notes"
        $classification = "reference"
        $engineTarget = "none"
        $notes = "Справочные заметки по отладке ядра Windows и WinDbg"
    } elseif ($u -match 'noverse\.dev/(diff|policies|tools|terminal)/') {
        $category = "web-tools"
        $classification = "tool"
        $engineTarget = "none"
        $notes = "Интерактивный веб-инструмент Nohuto: " + $matches[1]
    } else {
        $category = "overview"
        $classification = "reference"
        $engineTarget = "none"
        $notes = "Информационная страница сайта: " + $u
    }
    
    $sources += [PSCustomObject]@{
        Identifier = $u
        Type = "Website URL"
        Category = $category
        Classification = $classification
        EngineTarget = $engineTarget
        License = "AGPL-3.0 / GPL-3.0"
        Description = $notes
    }
}

# 2. GitHub Repositories (28 repos)
$ghRepos = @(
    @{ Name="win-config"; Clf="tweak-source"; Target="v1-tweak-engine"; Lic="AGPL-3.0"; Desc="Основной репозиторий документации твиков Windows (12 категорий, 796 файлов)" },
    @{ Name="regkit"; Clf="tool"; Target="developer-tool"; Lic="AGPL-3.0"; Desc="Высокопроизводительный редактор реестра и библиотека твиков" },
    @{ Name="win32-ui"; Clf="reference"; Target="none"; Lic="AGPL-3.0"; Desc="Общая библиотека компонентов пользовательского интерфейса Win32" },
    @{ Name="thread-sim"; Clf="reference"; Target="none"; Lic="AGPL-3.0"; Desc="Эксперименты и симуляция планировщика потоков Windows NT" },
    @{ Name="utils"; Clf="tool"; Target="developer-tool"; Lic="AGPL-3.0"; Desc="Набор CLI утилит для демонстрации параметров в win-config" },
    @{ Name="windbg-notes"; Clf="reference"; Target="none"; Lic="AGPL-3.0"; Desc="Заметки по низкоуровневой отладке ядра, прерываниям и DPC" },
    @{ Name="app-guides"; Clf="app-config"; Target="v2-app-guide"; Lic="GPL-3.0"; Desc="Конфигурации и скрипты оптимизации 10 сторонних приложений (Steam, Spotify, LGHUB и др.)" },
    @{ Name="userscripts"; Clf="reference"; Target="none"; Lic="GPL-3.0"; Desc="Пользовательские скрипты для браузеров (YouTube, Twitch)" },
    @{ Name="decompiled-pseudocode"; Clf="reference"; Target="none"; Lic="None"; Desc="Декомпилированный псевдокод функций из разных сборок Windows" },
    @{ Name="type-layouts"; Clf="reference"; Target="none"; Lic="AGPL-3.0"; Desc="Структуры и разметка типов данных из системных модулей Windows" },
    @{ Name="globals"; Clf="reference"; Target="none"; Lic="AGPL-3.0"; Desc="Извлеченные глобальные переменные ядра (RVA, типы, секции)" },
    @{ Name="admx-parser"; Clf="tool"; Target="developer-tool"; Lic="AGPL-3.0"; Desc="Парсер файлов PolicyDefinitions (ADMX/ADML) в JSON/YAML" },
    @{ Name="aes-cbc"; Clf="tool"; Target="developer-tool"; Lic="GPL-3.0"; Desc="PowerShell реализация шифрования AES CBC 256-bit" },
    @{ Name="minifier"; Clf="tool"; Target="developer-tool"; Lic="GPL-3.0"; Desc="Минификатор PowerShell скриптов (удаление пробелов, алиасы)" },
    @{ Name="void"; Clf="tool"; Target="none"; Lic="GPL-3.0"; Desc="Обфускатор PowerShell скриптов пробельными символами" },
    @{ Name="b64-char"; Clf="tool"; Target="none"; Lic="GPL-3.0"; Desc="Конвертер строк в символьные массивы для обфускации" },
    @{ Name="b64rev"; Clf="tool"; Target="none"; Lic="GPL-3.0"; Desc="Инструмент кодирования и декодирования Base64" },
    @{ Name="ps12bat"; Clf="tool"; Target="developer-tool"; Lic="GPL-3.0"; Desc="Конвертер .ps1 в автономные .bat файлы через Base64" },
    @{ Name="katzenfreunde.bb"; Clf="reference"; Target="none"; Lic="AGPL-3.0"; Desc="Сайт некоммерческой организации волонтеров" },
    @{ Name="nvfetch"; Clf="tool"; Target="developer-tool"; Lic="GPL-3.0"; Desc="CLI утилита вывода системной информации OS/HW (CIMV2/NVIDIA)" },
    @{ Name="bitmask-calc"; Clf="tool"; Target="developer-tool"; Lic="GPL-3.0"; Desc="Калькулятор битовых масок для реестра драйвера NVIDIA" },
    @{ Name="game-tools"; Clf="tool"; Target="v2-app-guide"; Lic="GPL-3.0"; Desc="Инструменты автоконфигурации киберспортивных игр (VALORANT, CS2, Fortnite)" },
    @{ Name="nvapi-cli"; Clf="tool"; Target="developer-tool"; Lic="AGPL-3.0"; Desc="CLI интерфейс к недокументированным функциям NvAPI (~400 вызовов)" },
    @{ Name="dism-wsim"; Clf="reference"; Target="none"; Lic="GPL-3.0"; Desc="Руководство по созданию кастомных образов Windows через DISM и WSIM" },
    @{ Name="reg2bat"; Clf="tool"; Target="developer-tool"; Lic="GPL-3.0"; Desc="Конвертер .reg файлов в пакетные команды reg.exe (.bat)" },
    @{ Name="pbo2-uv"; Clf="reference"; Target="none"; Lic="GPL-3.0"; Desc="Руководство по андервольтингу процессоров AMD через PBO Curve Optimizer" },
    @{ Name="gpu-oc-uv"; Clf="reference"; Target="none"; Lic="GPL-3.0"; Desc="Руководство по разгону и андервольтингу GPU в MSI Afterburner" },
    @{ Name="noverse.dev"; Clf="reference"; Target="none"; Lic="GPL-3.0"; Desc="Исходный код веб-сайта noverse.dev, веб-инструментов и документации" }
)

foreach ($repo in $ghRepos) {
    $sources += [PSCustomObject]@{
        Identifier = "https://github.com/nohuto/" + $repo.Name
        Type = "GitHub Repository"
        Category = "github/nohuto"
        Classification = $repo.Clf
        EngineTarget = $repo.Target
        License = $repo.Lic
        Description = $repo.Desc
    }
}

Write-Output "Total sources: $($sources.Count) (269 URLs + 28 Repos = $($sources.Count))"

# Export to CSV
$csvPath = "docs/sources.csv"
$sources | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8
Write-Output "Exported to $csvPath"
