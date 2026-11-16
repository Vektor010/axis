using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock28Tool
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
                // 1. privacy-appprivacy-force-allow-apps
                new TweakItem
                {
                    id = "privacy-appprivacy-force-allow-apps",
                    name_ru = "Очистить список принудительно разрешенных фоновых приложений",
                    description_ru = "Сбрасывает системный перечень приложений, которым безусловно разрешена работа в фоновом режиме.",
                    group = "Фоновые приложения",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 683, line_to = 685 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsRunInBackground_ForceAllowTheseApps",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. privacy-appprivacy-force-deny-apps
                new TweakItem
                {
                    id = "privacy-appprivacy-force-deny-apps",
                    name_ru = "Сбросить список принудительно заблокированных фоновых приложений",
                    description_ru = "Очищает перечень принудительно запрещенных приложений, возвращая стандартное управление фоновым режимом.",
                    group = "Фоновые приложения",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 683, line_to = 685 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsRunInBackground_ForceDenyTheseApps",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. privacy-netcache-blockout-start-time
                new TweakItem
                {
                    id = "privacy-netcache-blockout-start-time",
                    name_ru = "Сбросить время начала периода блокировки фоновой синхронизации",
                    description_ru = "Отключает расписание временных окон блокировки синхронизации автономных сетевых папок.",
                    group = "Автономные файлы",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1336, line_to = 1338 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\NetCache",
                            name = "BackgroundSyncBlockOutStartTime",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. privacy-netcache-blockout-duration
                new TweakItem
                {
                    id = "privacy-netcache-blockout-duration",
                    name_ru = "Сбросить длительность блокировки фоновой синхронизации",
                    description_ru = "Устанавливает в 0 минут длительность периода запрета синхронизации автономных файлов.",
                    group = "Автономные файлы",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1336, line_to = 1338 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\NetCache",
                            name = "BackgroundSyncBlockOutDurationMin",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. security-wu-scheduled-install-third-week
                new TweakItem
                {
                    id = "security-wu-scheduled-install-third-week",
                    name_ru = "Отключить принудительную установку обновлений в третью неделю месяца",
                    description_ru = "Предотвращает автоматическую установку пакетов обновлений в третью неделю календарного месяца.",
                    group = "Центр обновления",
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
                            name = "ScheduledInstallThirdWeek",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. security-wu-scheduled-install-fourth-week
                new TweakItem
                {
                    id = "security-wu-scheduled-install-fourth-week",
                    name_ru = "Отключить принудительную установку обновлений в четвертую неделю месяца",
                    description_ru = "Предотвращает автоматическую установку пакетов обновлений в четвертую неделю календарного месяца.",
                    group = "Центр обновления",
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
                            name = "ScheduledInstallFourthWeek",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. security-wu-wuserver-policy
                new TweakItem
                {
                    id = "security-wu-wuserver-policy",
                    name_ru = "Сбросить адрес интранет-сервера служб обновлений WSUS (Machine)",
                    description_ru = "Очищает принудительный адрес локального сервера обновлений WUServer для проверки через официальные серверы.",
                    group = "Центр обновления",
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
                            name = "WUServer",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. security-wu-wustatus-server-policy
                new TweakItem
                {
                    id = "security-wu-wustatus-server-policy",
                    name_ru = "Сбросить адрес сервера отчетов о статусе обновлений (WUStatusServer)",
                    description_ru = "Удаляет адрес сервера интрасети для отправки корпоративной статистики обновлений.",
                    group = "Центр обновления",
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
                            name = "WUStatusServer",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. security-wu-update-service-alternate
                new TweakItem
                {
                    id = "security-wu-update-service-alternate",
                    name_ru = "Сбросить альтернативный адрес сервера службы обновлений (Alternate URL)",
                    description_ru = "Очищает резервный URL интранет-сервера обновлений Windows.",
                    group = "Центр обновления",
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
                            name = "UpdateServiceUrlAlternate",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. security-wu-au-wuserver
                new TweakItem
                {
                    id = "security-wu-au-wuserver",
                    name_ru = "Сбросить URL интранет-сервера обновлений в службе AU",
                    description_ru = "Очищает адрес локального сервера обновлений в конфигурации службы автоматического обновления.",
                    group = "Центр обновления",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "WUServer",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. security-wu-au-wustatus-server
                new TweakItem
                {
                    id = "security-wu-au-wustatus-server",
                    name_ru = "Сбросить URL сервера статистики обновлений в службе AU",
                    description_ru = "Удаляет адрес сервера отчетов о статусе в конфигурации автоматического обновления AU.",
                    group = "Центр обновления",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "WUStatusServer",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. security-wu-au-update-service-alternate
                new TweakItem
                {
                    id = "security-wu-au-update-service-alternate",
                    name_ru = "Сбросить альтернативный URL сервера обновлений в службе AU",
                    description_ru = "Очищает резервный адрес сервера обновлений в ветке AU.",
                    group = "Центр обновления",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "UpdateServiceUrlAlternate",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. security-wu-au-fill-empty-content-urls
                new TweakItem
                {
                    id = "security-wu-au-fill-empty-content-urls",
                    name_ru = "Запретить заполнение пустых URL контента обновлений в службе AU",
                    description_ru = "Блокирует перенаправление запросов содержимого обновлений в конфигурации AU.",
                    group = "Центр обновления",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "FillEmptyContentUrls",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. security-wu-au-donot-enforce-cert-pinning
                new TweakItem
                {
                    id = "security-wu-au-donot-enforce-cert-pinning",
                    name_ru = "Включить привязку TLS-сертификатов (Cert Pinning) в службе AU",
                    description_ru = "Требует валидацию сертификатов TLS в службе автоматического обновления для защиты от подмены пакетов.",
                    group = "Центр обновления",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "DoNotEnforceEnterpriseTLSCertPinningForUpdateDetection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. security-wu-au-proxy-behavior
                new TweakItem
                {
                    id = "security-wu-au-proxy-behavior",
                    name_ru = "Отключить прокси-сервер при обнаружении обновлений в службе AU",
                    description_ru = "Исключает использование промежуточных прокси-серверов службой автоматического обновления.",
                    group = "Центр обновления",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "SetProxyBehaviorForUpdateDetection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. security-wu-au-use-wuserver
                new TweakItem
                {
                    id = "security-wu-au-use-wuserver",
                    name_ru = "Запретить принудительное перенаправление службы AU на сервер WSUS",
                    description_ru = "Отключает принудительное использование внутреннего сервера обновлений в службе автоматического обновления.",
                    group = "Центр обновления",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "UseWUServer",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. security-wu-product-version
                new TweakItem
                {
                    id = "security-wu-product-version",
                    name_ru = "Сбросить принудительную версию продукта функциональных обновлений (ProductVersion)",
                    description_ru = "Очищает жесткую привязку версии продукта (например, Windows 10/11) при получении обновлений.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 496, line_to = 498 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "ProductVersion",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. security-wu-pause-quality-updates-start-time
                new TweakItem
                {
                    id = "security-wu-pause-quality-updates-start-time",
                    name_ru = "Сбросить время начала приостановки качественных обновлений",
                    description_ru = "Удаляет сохраненную временную метку паузы накопительных обновлений Windows.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 498, line_to = 500 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "PauseQualityUpdatesStartTime",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. security-powershell-user-enable-scripts
                new TweakItem
                {
                    id = "security-powershell-user-enable-scripts",
                    name_ru = "Разрешить выполнение сценариев PowerShell для текущего пользователя",
                    description_ru = "Включает контролируемое выполнение скриптов PowerShell в пользовательском профиле.",
                    group = "PowerShell",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 669, line_to = 671 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell",
                            name = "EnableScripts",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. security-powershell-machine-transcription-output-dir
                new TweakItem
                {
                    id = "security-powershell-machine-transcription-output-dir",
                    name_ru = "Сбросить каталог сохранения транскриптов PowerShell (Machine)",
                    description_ru = "Очищает принудительную директорию сохранения текстовых логов консоли PowerShell.",
                    group = "PowerShell",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 671, line_to = 673 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription",
                            name = "OutputDirectory",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. security-powershell-user-transcription-output-dir
                new TweakItem
                {
                    id = "security-powershell-user-transcription-output-dir",
                    name_ru = "Сбросить каталог сохранения транскриптов PowerShell (User)",
                    description_ru = "Удаляет пользовательскую директорию протоколирования сеансов PowerShell.",
                    group = "PowerShell",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 671, line_to = 673 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription",
                            name = "OutputDirectory",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. security-powershell-user-invocation-header
                new TweakItem
                {
                    id = "security-powershell-user-invocation-header",
                    name_ru = "Включить заголовок вызова команд в транскриптах PowerShell (User)",
                    description_ru = "Активирует запись метаданных и времени вызова в пользовательские транскрипты PowerShell.",
                    group = "PowerShell",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 671, line_to = 673 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription",
                            name = "EnableInvocationHeader",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. security-powershell-user-scriptblock-invocation-logging
                new TweakItem
                {
                    id = "security-powershell-user-scriptblock-invocation-logging",
                    name_ru = "Включить логирование вызова блоков сценариев PowerShell (User)",
                    description_ru = "Активирует запись событий старта блоков скриптов в журнал событий пользователя.",
                    group = "PowerShell",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 672, line_to = 674 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging",
                            name = "EnableScriptBlockInvocationLogging",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. security-deviceguard-shadow-stacks-launch
                new TweakItem
                {
                    id = "security-deviceguard-shadow-stacks-launch",
                    name_ru = "Отключить принудительный запуск стеков теневого вызова ядра (Kernel Shadow Stacks)",
                    description_ru = "Отключает аппаратную защиту стека возврата ядра при загрузке для снижения аппаратных накладных расходов.",
                    group = "Безопасность ядра",
                    category = "security",
                    type = "toggle",
                    risk = "medium",
                    presets = new List<string>(),
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1026, line_to = 1028 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard",
                            name = "ConfigureKernelShadowStacksLaunch",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. visibility-desktop-wallpaper-clear
                new TweakItem
                {
                    id = "visibility-desktop-wallpaper-clear",
                    name_ru = "Сбросить принудительную блокировку пути фонового рисунка рабочего стола",
                    description_ru = "Очищает политику принудительного назначения изображения рабочего стола, возвращая выбор пользователю.",
                    group = "Персонализация",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 576, line_to = 578 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "Wallpaper",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            list.AddRange(newTweaks);
            Console.WriteLine("Новое число твиков: " + list.Count);

            string updated = serializer.Serialize(list);

            // Pretty format json
            StringBuilder sb = new StringBuilder();
            int indent = 0;
            bool inQuotes = false;

            for (int i = 0; i < updated.Length; i++)
            {
                char ch = updated[i];
                if (ch == '"' && (i == 0 || updated[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                    sb.Append(ch);
                }
                else if (!inQuotes)
                {
                    if (ch == '{' || ch == '[')
                    {
                        sb.Append(ch);
                        sb.AppendLine();
                        indent += 2;
                        sb.Append(new string(' ', indent));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        sb.AppendLine();
                        indent -= 2;
                        sb.Append(new string(' ', indent));
                        sb.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.AppendLine();
                        sb.Append(new string(' ', indent));
                    }
                    else if (ch == ':')
                    {
                        sb.Append(": ");
                    }
                    else if (!char.IsWhiteSpace(ch))
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }

            File.WriteAllText(jsonPath, sb.ToString(), Encoding.UTF8);
            Console.WriteLine("Успешно добавлено 25 твиков Блока 28 в data/tweaks.json!");
        }
    }
}
