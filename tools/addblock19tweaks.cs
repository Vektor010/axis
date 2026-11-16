using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock19Tool
{
    public class RevertObj
    {
        public string mode { get; set; }
        public object value { get; set; }
    }

    public class RegItem
    {
        public string hive { get; set; }
        public string path { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public object apply { get; set; }
        public RevertObj revert { get; set; }
    }

    public class SourceObj
    {
        public string file { get; set; }
        public int line_from { get; set; }
        public int line_to { get; set; }
    }

    public class TweakItem
    {
        public string id { get; set; }
        public string name_ru { get; set; }
        public string description_ru { get; set; }
        public string group { get; set; }
        public string category { get; set; }
        public string type { get; set; }
        public string risk { get; set; }
        public List<string> presets { get; set; }
        public int? min_build { get; set; }
        public bool needs_review { get; set; }
        public SourceObj source { get; set; }
        public List<RegItem> registry { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(jsonPath, Encoding.UTF8);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<TweakItem>>(raw);

            Console.WriteLine("Текущее число твиков: " + list.Count);

            var newTweaks = new List<TweakItem>
            {
                // 1. network-directaccess-passive-mode
                new TweakItem
                {
                    id = "network-directaccess-passive-mode",
                    name_ru = "Пассивный режим DirectAccess",
                    description_ru = "Переводит проверку сетевого подключения DirectAccess в пассивный режим для уменьшения лишнего трафика.",
                    group = "Сетевые службы",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 994, line_to = 996 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator",
                            name = "PassiveMode",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. network-wifi-autoconnect-hotspots
                new TweakItem
                {
                    id = "network-wifi-autoconnect-hotspots",
                    name_ru = "Отключить автоподключение к открытым хот-спотам Wi-Fi",
                    description_ru = "Запрещает Windows автоматически подключаться к предлагаемым открытым и общедоступным сетям Wi-Fi.",
                    group = "Беспроводная сеть",
                    category = "network",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 1143, line_to = 1145 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\wcmsvc\wifinetworkmanager\config",
                            name = "AutoConnectAllowedOEM",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. peripheral-downlevel-browse-printers
                new TweakItem
                {
                    id = "peripheral-downlevel-browse-printers",
                    name_ru = "Отключить поиск сетевых принтеров нижнего уровня",
                    description_ru = "Отключает фоновый широковещательный опрос сети для поиска устаревших принтеров.",
                    group = "Принтеры и печать",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1835, line_to = 1837 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows NT\Printers",
                            name = "Downlevel Browse",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. visibility-prevent-changing-desktop-icons
                new TweakItem
                {
                    id = "visibility-prevent-changing-desktop-icons",
                    name_ru = "Запретить изменение значков рабочего стола",
                    description_ru = "Блокирует случайное скрытие или изменение системных значков «Этот компьютер», «Корзина» и др.",
                    group = "Рабочий стол",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1362, line_to = 1364 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "NoDispBackgroundPage",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. visibility-prevent-changing-lock-screen
                new TweakItem
                {
                    id = "visibility-prevent-changing-lock-screen",
                    name_ru = "Запретить изменение изображения экрана блокировки",
                    description_ru = "Фиксирует изображение экрана блокировки и экрана входа в систему от случайных изменений.",
                    group = "Экран блокировки",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1363, line_to = 1365 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Personalization",
                            name = "NoChangingLockScreen",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. visibility-prevent-changing-mouse-pointers
                new TweakItem
                {
                    id = "visibility-prevent-changing-mouse-pointers",
                    name_ru = "Запретить темам менять указатели мыши",
                    description_ru = "Предотвращает автоматическую замену схемы курсоров мыши при применении тем Windows.",
                    group = "Курсор мыши",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1364, line_to = 1366 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Personalization",
                            name = "NoChangingMousePointers",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. visibility-prevent-changing-start-background
                new TweakItem
                {
                    id = "visibility-prevent-changing-start-background",
                    name_ru = "Запретить изменение фона меню «Пуск»",
                    description_ru = "Блокирует переопределение цвета фона и акцентов меню «Пуск» сторонними программами.",
                    group = "Меню «Пуск»",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1365, line_to = 1367 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Personalization",
                            name = "NoChangingStartMenuBackground",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. visibility-animate-lock-screen-background
                new TweakItem
                {
                    id = "visibility-animate-lock-screen-background",
                    name_ru = "Отключить анимацию фона экрана блокировки",
                    description_ru = "Отключает эффект движения и параллакса фона экрана блокировки для ускорения отклика.",
                    group = "Экран блокировки",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1767, line_to = 1769 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Personalization",
                            name = "AnimateLockScreenBackground",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. visibility-disable-wcn-ui
                new TweakItem
                {
                    id = "visibility-disable-wcn-ui",
                    name_ru = "Отключить мастер Windows Connect Now",
                    description_ru = "Отключает всплывающие мастера настройки беспроводной сети Windows Connect Now (WCN UI).",
                    group = "Интерфейс",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 337, line_to = 339 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WCN\UI",
                            name = "DisableWcnUi",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. system-search-auto-lang-detection
                new TweakItem
                {
                    id = "system-search-auto-lang-detection",
                    name_ru = "Отключить автоопределение языка в службе поиска",
                    description_ru = "Отключает фоновые языковые парсеры службы индексирования для снижения нагрузки на процессор.",
                    group = "Служба поиска",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9153, line_to = 9155 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                            name = "AlwaysUseAutoLangDetection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. system-search-web-metered-connections
                new TweakItem
                {
                    id = "system-search-web-metered-connections",
                    name_ru = "Запретить веб-поиск при лимитном подключении",
                    description_ru = "Блокирует фоновый поиск в Интернете через панель поиска при активных лимитных сетевых подключениях.",
                    group = "Служба поиска",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9155, line_to = 9157 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                            name = "ConnectedSearchUseWebOverMeteredConnections",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. system-search-safesearch
                new TweakItem
                {
                    id = "system-search-safesearch",
                    name_ru = "Отключить фильтрацию SafeSearch в поиске Windows",
                    description_ru = "Отключает фильтрацию безопасного поиска через серверы Microsoft в поисковой строке Windows.",
                    group = "Служба поиска",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9157, line_to = 9159 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                            name = "ConnectedSearchSafeSearch",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. system-disable-account-notifications
                new TweakItem
                {
                    id = "system-disable-account-notifications",
                    name_ru = "Отключить системные уведомления учетной записи",
                    description_ru = "Отключает всплывающие напоминания о привязке учетной записи Microsoft и резервных копиях.",
                    group = "Уведомления",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8402, line_to = 8404 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications",
                            name = "DisableAccountNotifications",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-disable-enterprise-auth-proxy
                new TweakItem
                {
                    id = "privacy-disable-enterprise-auth-proxy",
                    name_ru = "Отключить аутентификацию прокси для телеметрии",
                    description_ru = "Запрещает службе телеметрии использовать сохраненные учетные данные прокси-сервера для отправки данных.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 113, line_to = 115 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "DisableEnterpriseAuthProxy",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-allow-device-name-in-telemetry
                new TweakItem
                {
                    id = "privacy-allow-device-name-in-telemetry",
                    name_ru = "Запретить отправку имени устройства в диагностических данных",
                    description_ru = "Исключает имя компьютера и сетевой идентификатор из диагностических отчетов Windows.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 115, line_to = 117 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "AllowDeviceNameInTelemetry",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. privacy-disable-telemetry-optin-settings-ux
                new TweakItem
                {
                    id = "privacy-disable-telemetry-optin-settings-ux",
                    name_ru = "Заблокировать интерфейс настройки телеметрии",
                    description_ru = "Фиксирует уровень телеметрии и запрещает его случайное повышение через графические параметры Windows.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 116, line_to = 118 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "DisableTelemetryOptInSettingsUx",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. privacy-disable-telemetry-optin-change-notification
                new TweakItem
                {
                    id = "privacy-disable-telemetry-optin-change-notification",
                    name_ru = "Отключить уведомления об изменении уровня сбора данных",
                    description_ru = "Подавляет всплывающие системные уведомления об изменении уровня диагностических данных.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 117, line_to = 119 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "DisableTelemetryOptInChangeNotification",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-disable-diagnostic-data-viewer
                new TweakItem
                {
                    id = "privacy-disable-diagnostic-data-viewer",
                    name_ru = "Отключить просмотрщик диагностических данных",
                    description_ru = "Отключает фоновые службы локального кэширования журнала диагностических данных.",
                    group = "Диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 119, line_to = 121 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "DisableDiagnosticDataViewer",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-allow-linguistic-data-collection
                new TweakItem
                {
                    id = "privacy-allow-linguistic-data-collection",
                    name_ru = "Отключить сбор языковых данных ввода",
                    description_ru = "Запрещает отправку в Microsoft образцов рукописного и клавиатурного ввода для улучшения распознавания.",
                    group = "Сбор данных ввода",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 124, line_to = 126 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\TextInput",
                            name = "AllowLinguisticDataCollection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. privacy-turn-off-messenger-ceip
                new TweakItem
                {
                    id = "privacy-turn-off-messenger-ceip",
                    name_ru = "Отключить программу улучшения качества Windows Messenger (CEIP)",
                    description_ru = "Отключает встроенные компоненты сбора статистики использования Windows Messenger CEIP.",
                    group = "Программа улучшения качества (CEIP)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 131, line_to = 133 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Messenger\Client",
                            name = "CEIP",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. privacy-disable-msdt-query-remote-server
                new TweakItem
                {
                    id = "privacy-disable-msdt-query-remote-server",
                    name_ru = "Отключить сетевые запросы средства диагностики поддержки (MSDT)",
                    description_ru = "Блокирует автоматическое обращение средства устранения неполадок MSDT к внешним серверам Microsoft.",
                    group = "Диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 379, line_to = 381 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnosticsProvider\Policy",
                            name = "DisableQueryRemoteServer",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. privacy-troubleshooting-allow-recommendations
                new TweakItem
                {
                    id = "privacy-troubleshooting-allow-recommendations",
                    name_ru = "Отключить рекомендуемое устранение неполадок",
                    description_ru = "Отключает автоматическую фоновую диагностику и навязчивые рекомендации устранения неполадок.",
                    group = "Диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 382, line_to = 384 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Troubleshooting",
                            name = "TroubleshootingAllowRecommendations",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. privacy-disable-spotlight-collection-desktop
                new TweakItem
                {
                    id = "privacy-disable-spotlight-collection-desktop",
                    name_ru = "Отключить коллекцию Windows Spotlight на рабочем столе",
                    description_ru = "Отключает фоновую загрузку рекламных коллекций изображений Windows Spotlight на рабочий стол.",
                    group = "Windows Spotlight",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 455, line_to = 457 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableSpotlightCollectionOnDesktop",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. privacy-disable-spotlight-action-center
                new TweakItem
                {
                    id = "privacy-disable-spotlight-action-center",
                    name_ru = "Отключить Windows Spotlight в Центре уведомлений",
                    description_ru = "Предотвращает появление рекомендательного промо-контента Windows Spotlight в Центре уведомлений.",
                    group = "Windows Spotlight",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 457, line_to = 459 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableWindowsSpotlightOnActionCenter",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. privacy-disable-spotlight-settings
                new TweakItem
                {
                    id = "privacy-disable-spotlight-settings",
                    name_ru = "Отключить Windows Spotlight в меню параметров",
                    description_ru = "Блокирует отображение советов и предложений Spotlight в окне системных настроек Windows.",
                    group = "Windows Spotlight",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 458, line_to = 460 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableWindowsSpotlightOnSettings",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            list.AddRange(newTweaks);
            Console.WriteLine("Новое число твиков: " + list.Count);

            string updated = serializer.Serialize(list);

            var formatted = FormatJson(updated);
            File.WriteAllText(jsonPath, formatted, Encoding.UTF8);
            Console.WriteLine("tweaks.json успешно обновлен до " + list.Count + " твиков!");
        }

        static string FormatJson(string json)
        {
            var sb = new StringBuilder();
            int indent = 0;
            bool inQuotes = false;
            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (ch == '\"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                    sb.Append(ch);
                }
                else if (!inQuotes)
                {
                    if (ch == '{' || ch == '[')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        indent += 2;
                        sb.Append(new string(' ', indent));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        sb.Append("\r\n");
                        indent -= 2;
                        sb.Append(new string(' ', indent));
                        sb.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent));
                    }
                    else if (ch == ':')
                    {
                        sb.Append(": ");
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
}
