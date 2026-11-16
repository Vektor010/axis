using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock25Tool
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
                // 1. security-defender-definition-update-file-shares
                new TweakItem
                {
                    id = "security-defender-definition-update-file-shares",
                    name_ru = "Очистить сетевые папки загрузки обновлений определений Защитника",
                    description_ru = "Запрещает Windows Defender использовать общие сетевые папки в качестве источника антивирусных сигнатур.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 489, line_to = 491 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Signature Updates",
                            name = "DefinitionUpdateFileSharesSources",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. security-defender-signature-update-fallback-order
                new TweakItem
                {
                    id = "security-defender-signature-update-fallback-order",
                    name_ru = "Ограничить источники загрузки сигнатур сервером Microsoft Update",
                    description_ru = "Задает строгий порядок резервных источников обновления Защитника, исключая сторонние сетевые шары.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 490, line_to = 492 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Signature Updates",
                            name = "FallbackOrder",
                            type = "REG_SZ",
                            apply = "MicrosoftUpdateServer",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. security-uac-enable-installer-detection
                new TweakItem
                {
                    id = "security-uac-enable-installer-detection",
                    name_ru = "Отключить эвристическое обнаружение программ установки UAC",
                    description_ru = "Отключает автоматическое сканирование и перехват инсталляторов службой контроля учетных записей (UAC).",
                    group = "Контроль учетных записей (UAC)",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 563, line_to = 571 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "EnableInstallerDetection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. security-uac-validate-admin-code-signatures
                new TweakItem
                {
                    id = "security-uac-validate-admin-code-signatures",
                    name_ru = "Требовать обязательную проверку цифровой подписи при повышении прав",
                    description_ru = "Требует наличия действующей доверенной цифровой подписи перед выполнением любого запроса на повышение привилегий.",
                    group = "Контроль учетных записей (UAC)",
                    category = "security",
                    type = "toggle",
                    risk = "medium",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 572, line_to = 580 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "ValidateAdminCodeSignatures",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. security-wu-scheduled-install-day
                new TweakItem
                {
                    id = "security-wu-scheduled-install-day",
                    name_ru = "Установить ежедневное расписание проверки обновлений Windows",
                    description_ru = "Переводит службу автоматического обновления в предсказуемый ежедневный режим проверки обновлений.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 493, line_to = 495 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "ScheduledInstallDay",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. security-wu-scheduled-install-time
                new TweakItem
                {
                    id = "security-wu-scheduled-install-time",
                    name_ru = "Назначить время запланированной установки обновлений на 03:00",
                    description_ru = "Фиксирует ночное время установки системных обновлений во избежание снижения производительности в рабочие часы.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 493, line_to = 495 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "ScheduledInstallTime",
                            type = "REG_DWORD",
                            apply = 3,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-w32time-ntpclient-enabled
                new TweakItem
                {
                    id = "privacy-w32time-ntpclient-enabled",
                    name_ru = "Включить встроенный клиент сетевого времени NTP (W32Time)",
                    description_ru = "Активирует встроенный клиент точного времени NTP службы W32Time для надежной синхронизации системных часов.",
                    group = "Служба времени W32Time",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 539, line_to = 541 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\W32Time\TimeProviders\NtpClient",
                            name = "Enabled",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. security-smartscreen-shell-level
                new TweakItem
                {
                    id = "security-smartscreen-shell-level",
                    name_ru = "Установить уровень предупреждения SmartScreen в проводнике",
                    description_ru = "Переводит проверку SmartScreen для исполняемых файлов проводника в режим предупреждения вместо жесткой блокировки.",
                    group = "Фильтр SmartScreen",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 279, line_to = 281 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "ShellSmartScreenLevel",
                            type = "REG_SZ",
                            apply = "Warn",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. security-wu-au-options
                new TweakItem
                {
                    id = "security-wu-au-options",
                    name_ru = "Уведомлять о загрузке и установке обновлений Windows (AUOptions 2)",
                    description_ru = "Переводит Центр обновления в режим уведомления перед скачиванием и установкой любых обновлений.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 493, line_to = 495 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "AUOptions",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. security-wu-automatic-maintenance
                new TweakItem
                {
                    id = "security-wu-automatic-maintenance",
                    name_ru = "Отключить автоматическое обслуживание во время установки обновлений",
                    description_ru = "Запрещает Центру обновления Windows инициировать ресурсоемкие задачи автоматического обслуживания.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 493, line_to = 495 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "AutomaticMaintenanceEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. security-wu-allow-mu-service
                new TweakItem
                {
                    id = "security-wu-allow-mu-service",
                    name_ru = "Отключить получение обновлений для других продуктов Microsoft",
                    description_ru = "Ограничивает работу службы обновлений исключительно файлами ОС Windows, исключая Office и другие службы.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 493, line_to = 495 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "AllowMUUpdateService",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. security-wu-use-wuserver
                new TweakItem
                {
                    id = "security-wu-use-wuserver",
                    name_ru = "Запретить принудительное перенаправление обновлений на сервер WSUS",
                    description_ru = "Отключает принудительную переадресацию клиента обновлений на локальный сервер WSUS в корпоративной сети.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 494, line_to = 496 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "UseWUServer",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. privacy-settingsync-disable-appsync-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-appsync-override",
                    name_ru = "Запретить пользователю включать синхронизацию приложений",
                    description_ru = "Блокирует переключатель синхронизации установленных приложений в системных параметрах учетной записи.",
                    group = "Синхронизация параметров",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1168, line_to = 1170 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableAppSyncSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-settingsync-disable-windows-setting-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-windows-setting-override",
                    name_ru = "Запретить пользователю включать синхронизацию параметров Windows",
                    description_ru = "Блокирует переключатель синхронизации дополнительных системных настроек Windows в параметрах.",
                    group = "Синхронизация параметров",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1169, line_to = 1171 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableWindowsSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-settingsync-disable-desktop-theme-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-desktop-theme-override",
                    name_ru = "Запретить пользователю включать синхронизацию темы рабочего стола",
                    description_ru = "Блокирует переключатель синхронизации темы оформления и обоев рабочего стола.",
                    group = "Синхронизация параметров",
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
                            name = "DisableDesktopThemeSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. privacy-settingsync-disable-web-browser-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-web-browser-override",
                    name_ru = "Запретить пользователю включать синхронизацию настроек браузера",
                    description_ru = "Блокирует переключатель синхронизации закладок, истории и настроек браузера в параметрах системы.",
                    group = "Синхронизация параметров",
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
                            name = "DisableWebBrowserSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. privacy-settingsync-disable-start-layout-override
                new TweakItem
                {
                    id = "privacy-settingsync-disable-start-layout-override",
                    name_ru = "Запретить пользователю включать синхронизацию макета меню Пуск",
                    description_ru = "Блокирует переключатель синхронизации закрепленных значков и структуры меню Пуск.",
                    group = "Синхронизация параметров",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1173, line_to = 1175 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\SettingSync",
                            name = "DisableStartLayoutSettingSyncUserOverride",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-mdm-use-aad-credential-type
                new TweakItem
                {
                    id = "privacy-mdm-use-aad-credential-type",
                    name_ru = "Отключить использование учетных данных Azure AD при регистрации MDM",
                    description_ru = "Запрещает автоматическую авторизацию в системах MDM с учетными данными Azure Active Directory.",
                    group = "Управление MDM",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1213, line_to = 1215 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\CurrentVersion\MDM",
                            name = "UseAADCredentialType",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-cloudcontent-include-enterprise-spotlight
                new TweakItem
                {
                    id = "privacy-cloudcontent-include-enterprise-spotlight",
                    name_ru = "Отключить корпоративный контент Windows Spotlight на экране блокировки",
                    description_ru = "Запрещает службе Windows Spotlight загружать корпоративные фоны и рекламные материалы на экране блокировки.",
                    group = "Windows Spotlight",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 453, line_to = 455 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "IncludeEnterpriseSpotlight",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. privacy-wdi-scenario-execution-enabled
                new TweakItem
                {
                    id = "privacy-wdi-scenario-execution-enabled",
                    name_ru = "Отключить автоматическое выполнение сценариев диагностики WDI",
                    description_ru = "Отключает фоновые сценарии диагностической инфраструктуры Windows (WDI) для предотвращения скрытой активности.",
                    group = "Диагностика системы (WDI)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 384, line_to = 386 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WDI",
                            name = "ScenarioExecutionEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. privacy-scheduled-diagnostics-level
                new TweakItem
                {
                    id = "privacy-scheduled-diagnostics-level",
                    name_ru = "Отключить выполнение запланированных сценариев обслуживания системы",
                    description_ru = "Запрещает планировщику Windows запускать регулярные автоматические диагностические тесты производительности.",
                    group = "Диагностика системы (WDI)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 383, line_to = 385 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\ScheduledDiagnostics",
                            name = "EnabledExecutionLevel",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. privacy-reliability-timestamp-interval
                new TweakItem
                {
                    id = "privacy-reliability-timestamp-interval",
                    name_ru = "Отключить периодическую запись системных меток времени надежности",
                    description_ru = "Отключает регулярную запись меток времени службы надежности Windows (Reliability Timestamp) на накопитель.",
                    group = "Монитор стабильности системы",
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
                            name = "TimeStampInterval",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. visibility-personalization-lock-screen-image
                new TweakItem
                {
                    id = "visibility-personalization-lock-screen-image",
                    name_ru = "Зафиксировать стандартное фоновое изображение экрана блокировки",
                    description_ru = "Назначает фиксированное системное фоновое изображение экрана блокировки и отключает его динамическую смену.",
                    group = "Экран блокировки",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1770, line_to = 1772 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Personalization",
                            name = "LockScreenImage",
                            type = "REG_SZ",
                            apply = @"C:\Windows\Web\Screen\img100.jpg",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. visibility-desktop-no-desktop
                new TweakItem
                {
                    id = "visibility-desktop-no-desktop",
                    name_ru = "Разрешить отображение значков и элементов на рабочем столе",
                    description_ru = "Гарантирует доступность всех ярлыков, файлов и системных значков на рабочем столе пользователя.",
                    group = "Проводник и рабочий стол",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 335, line_to = 337 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "NoDesktop",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. visibility-settings-page-visibility
                new TweakItem
                {
                    id = "visibility-settings-page-visibility",
                    name_ru = "Сбросить ограничения видимости страниц приложения Параметры",
                    description_ru = "Снимает групповые политики скрытия отдельных страниц в приложении системных Параметров Windows.",
                    group = "Параметры Windows",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1920, line_to = 1922 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "SettingsPageVisibility",
                            type = "REG_SZ",
                            apply = "default",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            foreach (var t in newTweaks)
            {
                list.Add(t);
            }

            Console.WriteLine("Новое число твиков после добавления Блока 25: " + list.Count);

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
