using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock21Tool
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
                // 1. system-game-mode-allow-auto
                new TweakItem
                {
                    id = "system-game-mode-allow-auto",
                    name_ru = "Отключить принудительную автоматическую активацию Game Mode",
                    description_ru = "Отключает фоновую регистрацию приложений в службе Game Mode через групповые политики Windows (GameDVR).",
                    group = "Игровой режим",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "advanced" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 4440, line_to = 4443 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\GameDVR",
                            name = "AllowAutoGameMode",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. system-game-mode-gpu-yield
                new TweakItem
                {
                    id = "system-game-mode-gpu-yield",
                    name_ru = "Минимальная задержка планировщика GPU Game Mode (GpuYieldPercentage 2%)",
                    description_ru = "Устанавливает минимальный процент квантов GPU (2%), уступаемых графическим планировщиком D3DKMT фоновым процессам во время работы игры.",
                    group = "Игровой режим",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "gaming", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 4434, line_to = 4439 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\GameBar",
                            name = "GpuYieldPercentage",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. system-game-mode-disallow-system-cpu-sets
                new TweakItem
                {
                    id = "system-game-mode-disallow-system-cpu-sets",
                    name_ru = "Запретить системные ограничения CPU Sets для Game Mode",
                    description_ru = "Запрещает Game Mode навязывать системную маску допустимых ядер CPU Sets, позволяя игре использовать все доступные вычислительные ядра процессора.",
                    group = "Игровой режим",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "gaming", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 4434, line_to = 4440 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\GameBar",
                            name = "DisallowSystemAllowedCpuSets",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. privacy-limit-enhanced-diagnostic-data
                new TweakItem
                {
                    id = "privacy-limit-enhanced-diagnostic-data",
                    name_ru = "Ограничить расширенные диагностические данные для Desktop Analytics",
                    description_ru = "Ограничивает сбор и отправку дополнительных данных телеметрии и аналитики рабочего стола в службу Microsoft Desktop Analytics.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 114, line_to = 116 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "LimitEnhancedDiagnosticDataWindowsAnalytics",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. privacy-disable-kms-avs-validation
                new TweakItem
                {
                    id = "privacy-disable-kms-avs-validation",
                    name_ru = "Отключить онлайн-валидацию AVS для клиента KMS",
                    description_ru = "Отключает отправку сетевых запросов валидации лицензии KMS в службу онлайн-проверки подлинности Microsoft (AVS).",
                    group = "Активация и лицензирование",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 135, line_to = 137 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows NT\CurrentVersion\Software Protection Platform",
                            name = "NoGenTicket",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. privacy-auto-approve-os-dumps
                new TweakItem
                {
                    id = "privacy-auto-approve-os-dumps",
                    name_ru = "Запретить автоматическую отправку дампов памяти ОС в Microsoft",
                    description_ru = "Отключает автоматическое одобрение и скрытую передачу дампов оперативной памяти при сбоях операционной системы в службу отчетов об ошибках WER.",
                    group = "Отчеты об ошибках",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 334, line_to = 336 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting",
                            name = "AutoApproveOSDumps",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-enable-device-health-attestation
                new TweakItem
                {
                    id = "privacy-enable-device-health-attestation",
                    name_ru = "Отключить службу мониторинга и отчетов аттестации работоспособности устройств",
                    description_ru = "Отключает службу Device Health Attestation, периодически передающую аппаратные хэши TPM и замеры безопасности на удаленные серверы.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 336, line_to = 338 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\DeviceHealthAttestationService",
                            name = "EnableDeviceHealthAttestationService",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. privacy-persistent-timestamp
                new TweakItem
                {
                    id = "privacy-persistent-timestamp",
                    name_ru = "Отключить постоянную временную метку монитора надежности (Reliability)",
                    description_ru = "Отключает периодическую запись меток времени в реестр для отслеживания неожиданных завершений работы монитором надежности Windows.",
                    group = "Диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 337, line_to = 339 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows NT\Reliability",
                            name = "TimeStampEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. privacy-disable-scripted-diagnostics
                new TweakItem
                {
                    id = "privacy-disable-scripted-diagnostics",
                    name_ru = "Запретить запуск мастеров скриптовой диагностики Windows",
                    description_ru = "Запрещает запуск встроенных скриптовых средств устранения неполадок MSDT, снижая поверхность атак и фоновую активность.",
                    group = "Диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 380, line_to = 382 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnostics",
                            name = "EnableDiagnostics",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. privacy-disable-remote-troubleshooting-server
                new TweakItem
                {
                    id = "privacy-disable-remote-troubleshooting-server",
                    name_ru = "Запретить доступ мастера устранения неполадок к онлайн-серверам Microsoft (WOTS)",
                    description_ru = "Отключает обращение службы Scripted Diagnostics к онлайн-сервису Windows Online Troubleshooting Service через Интернет.",
                    group = "Диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 381, line_to = 383 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnosticsProvider\Policy",
                            name = "EnableQueryRemoteServer",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. privacy-disable-spotlight-features
                new TweakItem
                {
                    id = "privacy-disable-spotlight-features",
                    name_ru = "Отключить все функции Windows Spotlight (Интересное в Windows)",
                    description_ru = "Отключает фоновую загрузку рекламных обоев, советов, викторин и контента Windows Spotlight на экране блокировки и рабочем столе.",
                    group = "Windows Spotlight",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 454, line_to = 456 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableWindowsSpotlightFeatures",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. privacy-disable-phone-pc-linking
                new TweakItem
                {
                    id = "privacy-disable-phone-pc-linking",
                    name_ru = "Отключить интеграцию и связывание смартфона с ПК (Phone Link)",
                    description_ru = "Отключает компонент связывания мобильных устройств с Windows (Связь с телефоном / Phone-PC linking), устраняя фоновую синхронизацию.",
                    group = "Связанные устройства",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 608, line_to = 610 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "EnableMmx",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. privacy-disable-app-uri-handlers
                new TweakItem
                {
                    id = "privacy-disable-app-uri-handlers",
                    name_ru = "Отключить перенаправление веб-ссылок в приложения (Web-to-App URI Handlers)",
                    description_ru = "Запрещает браузеру и системе перехватывать URL-адреса сайтов для принудительного открытия установленных UWP-приложений.",
                    group = "Интеграция приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 722, line_to = 724 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "EnableAppUriHandlers",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-disable-sandbox-clipboard-redirection
                new TweakItem
                {
                    id = "privacy-disable-sandbox-clipboard-redirection",
                    name_ru = "Запретить общий буфер обмена с песочницей Windows Sandbox",
                    description_ru = "Изолирует буфер обмена хост-системы от виртуального окружения Windows Sandbox для предотвращения утечки конфиденциальных данных.",
                    group = "Буфер обмена",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 739, line_to = 741 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Sandbox",
                            name = "AllowClipboardRedirection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-disable-apps-camera-access
                new TweakItem
                {
                    id = "privacy-disable-apps-camera-access",
                    name_ru = "Запретить приложениям Windows доступ к веб-камере",
                    description_ru = "Блокирует доступ UWP и системных приложений Windows к физической веб-камере через групповую политику конфиденциальности.",
                    group = "Конфиденциальность оборудования",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1063, line_to = 1065 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessCamera",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. privacy-disable-apps-screenshot-border
                new TweakItem
                {
                    id = "privacy-disable-apps-screenshot-border",
                    name_ru = "Разрешить приложениям отключать желтую рамку захвата экрана",
                    description_ru = "Разрешает API графического захвата (Graphics Capture) отключать желтую рамку подсветки вокруг захватываемого окна или экрана.",
                    group = "Захват экрана",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1068, line_to = 1070 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessGraphicsCaptureWithoutBorder",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. privacy-disable-apps-notifications-access
                new TweakItem
                {
                    id = "privacy-disable-apps-notifications-access",
                    name_ru = "Запретить приложениям Windows доступ к системным уведомлениям",
                    description_ru = "Блокирует доступ сторонних приложений к чтению содержимого push-уведомлений и системных оповещений других программ.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1074, line_to = 1076 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessNotifications",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-disable-apps-phone-call-access
                new TweakItem
                {
                    id = "privacy-disable-apps-phone-call-access",
                    name_ru = "Запретить приложениям Windows доступ к совершению телефонных звонков",
                    description_ru = "Запрещает приложениям использовать телефонный модуль устройства для совершения исходящих вызовов без явного подтверждения.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1075, line_to = 1077 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessPhone",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-disable-sync-personalization
                new TweakItem
                {
                    id = "privacy-disable-sync-personalization",
                    name_ru = "Отключить облачную синхронизацию настроек персонализации Windows",
                    description_ru = "Отключает передачу в облако Microsoft и синхронизацию цветов, тем оформления и визуальных настроек между ПК.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1167, line_to = 1169 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisablePersonalizationSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. privacy-disable-sync-desktop-theme
                new TweakItem
                {
                    id = "privacy-disable-sync-desktop-theme",
                    name_ru = "Отключить облачную синхронизацию темы оформления рабочего стола",
                    description_ru = "Отключает фоновую синхронизацию активной темы, фонового рисунка и звуковой схемы через учетную запись Microsoft.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1170, line_to = 1172 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableDesktopThemeSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. privacy-disable-sync-browser
                new TweakItem
                {
                    id = "privacy-disable-sync-browser",
                    name_ru = "Отключить облачную синхронизацию данных и закладок браузера",
                    description_ru = "Запрещает передачу журнала, закладок и сохраненных учетных данных браузера Edge/IE на серверы учетной записи Microsoft.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1171, line_to = 1173 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableWebBrowserSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. privacy-disable-sync-paid-network
                new TweakItem
                {
                    id = "privacy-disable-sync-paid-network",
                    name_ru = "Отключить синхронизацию параметров в сетях с оплатой за трафик",
                    description_ru = "Запрещает операционной системе выполнять фоновую передачу параметров синхронизации при подключении к лимитным сетям.",
                    group = "Облачная синхронизация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1172, line_to = 1174 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableSyncOnPaidNetwork",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. privacy-disable-search-cloud
                new TweakItem
                {
                    id = "privacy-disable-search-cloud",
                    name_ru = "Отключить облачный поиск Microsoft в меню поиска Windows",
                    description_ru = "Отключает отправку поисковых запросов пользователя в облачные сервисы Microsoft (Bing/Office 365) при поиске на панели задач.",
                    group = "Поиск Windows",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1228, line_to = 1230 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                            name = "AllowCloudSearch",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. privacy-disable-search-location
                new TweakItem
                {
                    id = "privacy-disable-search-location",
                    name_ru = "Запретить службе поиска Windows использовать данные о местоположении",
                    description_ru = "Запрещает поисковой системе Windows запрашивать географические координаты пользователя для геотаргетированных результатов.",
                    group = "Поиск Windows",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1227, line_to = 1229 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                            name = "AllowSearchToUseLocation",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. privacy-disable-cortana-above-lock
                new TweakItem
                {
                    id = "privacy-disable-cortana-above-lock",
                    name_ru = "Запретить запуск голосового помощника на экране блокировки",
                    description_ru = "Блокирует работу голосового ассистента Cortana при заблокированном экране, исключая несанкционированный доступ к устройству.",
                    group = "Экран блокировки",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1226, line_to = 1228 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                            name = "AllowCortanaAboveLock",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            foreach (var t in newTweaks)
            {
                list.Add(t);
            }

            Console.WriteLine("Новое число твиков после добавления Блока 21: " + list.Count);

            string jsonFormatted = FormatJson(serializer.Serialize(list));
            File.WriteAllText(jsonPath, jsonFormatted, Encoding.UTF8);

            Console.WriteLine("tweaks.json успешно сохранен!");
        }

        private static string FormatJson(string json)
        {
            var sb = new StringBuilder();
            int indent = 0;
            bool inQuote = false;

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (ch == '\"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inQuote = !inQuote;
                    sb.Append(ch);
                }
                else if (!inQuote)
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
                        if (indent < 0) indent = 0;
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
