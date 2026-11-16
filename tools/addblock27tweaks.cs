using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock27Tool
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
                // 1. network-nca-passive-mode
                new TweakItem
                {
                    id = "network-nca-passive-mode",
                    name_ru = "Включить пассивный режим помощника сетевого подключения (NCA)",
                    description_ru = "Переводит Network Connectivity Assistant в пассивный режим для минимизации фонового сетевого зондирования.",
                    group = "Сетевой стек",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityAssistant",
                            name = "PassiveMode",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. privacy-datacollection-user-allow-telemetry
                new TweakItem
                {
                    id = "privacy-datacollection-user-allow-telemetry",
                    name_ru = "Отключить передачу диагностических данных пользователя (AllowTelemetry HKCU)",
                    description_ru = "Отключает отправку телеметрии и диагностических данных на уровне текущего профиля пользователя.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 112, line_to = 114 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "AllowTelemetry",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. privacy-appprivacy-background-apps-user-control
                new TweakItem
                {
                    id = "privacy-appprivacy-background-apps-user-control",
                    name_ru = "Запретить фоновую работу приложений по выбору пользователя",
                    description_ru = "Блокирует переопределение пользователем политик запрета фоновой активности UWP-приложений.",
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
                            name = "LetAppsRunInBackground_UserInControlOfTheseApps",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. security-deviceguard-systemguard-launch
                new TweakItem
                {
                    id = "security-deviceguard-systemguard-launch",
                    name_ru = "Отключить принудительный запуск System Guard при загрузке",
                    description_ru = "Отключает аппаратную проверку целостности прошивки и загрузчика System Guard Secure Launch.",
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
                            name = "ConfigureSystemGuardLaunch",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. privacy-mdm-application-id
                new TweakItem
                {
                    id = "privacy-mdm-application-id",
                    name_ru = "Очистить идентификатор корпоративного приложения регистрации MDM",
                    description_ru = "Удаляет привязку к внешнему приложению управления мобильными устройствами (MDM) в Azure AD.",
                    group = "MDM и телеметрия",
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
                            name = "MDMApplicationId",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. privacy-netcache-background-sync-period
                new TweakItem
                {
                    id = "privacy-netcache-background-sync-period",
                    name_ru = "Отключить периодическую фоновую синхронизацию автономных файлов (NetCache)",
                    description_ru = "Устанавливает интервал фоновой синхронизации автономных сетевых папок в 0 для исключения фонового I/O.",
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
                            name = "BackgroundSyncPeriodMin",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-netcache-background-sync-max-start
                new TweakItem
                {
                    id = "privacy-netcache-background-sync-max-start",
                    name_ru = "Отключить случайную задержку запуска синхронизации автономных файлов",
                    description_ru = "Сбрасывает максимальное время отсрочки фоновой синхронизации кэша автономных файлов.",
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
                            name = "BackgroundSyncMaxStartMin",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. privacy-netcache-background-sync-ignore-blockout
                new TweakItem
                {
                    id = "privacy-netcache-background-sync-ignore-blockout",
                    name_ru = "Отключить принудительную синхронизацию автономных файлов после блокировки",
                    description_ru = "Запрещает запуск сетевой синхронизации автономных файлов при истечении времени ожидания.",
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
                            name = "BackgroundSyncIgnoreBlockOutAfterMin",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. privacy-netcache-background-sync-forced-offline
                new TweakItem
                {
                    id = "privacy-netcache-background-sync-forced-offline",
                    name_ru = "Отключить синхронизацию автономных файлов в принудительном автономном режиме",
                    description_ru = "Запрещает фоновые сетевые попытки синхронизации, когда режим работы с сетью переведен в автономный.",
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
                            name = "BackgroundSyncEnabledForForcedOffline",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. security-wu-scheduled-install-every-week
                new TweakItem
                {
                    id = "security-wu-scheduled-install-every-week",
                    name_ru = "Установить еженедельное расписание автоматического обновления Windows",
                    description_ru = "Фиксирует расписание проверки и установки обновлений на еженедельную периодичность.",
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
                            name = "ScheduledInstallEveryWeek",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. security-wu-scheduled-install-first-week
                new TweakItem
                {
                    id = "security-wu-scheduled-install-first-week",
                    name_ru = "Отключить принудительную установку обновлений в первую неделю месяца",
                    description_ru = "Запрещает установку накопительных пакетов обновлений в первую неделю месяца.",
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
                            name = "ScheduledInstallFirstWeek",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. security-wu-scheduled-install-second-week
                new TweakItem
                {
                    id = "security-wu-scheduled-install-second-week",
                    name_ru = "Отключить принудительную установку обновлений во вторую неделю месяца (Patch Tuesday)",
                    description_ru = "Предотвращает автоматическую перезагрузку в день выпуска обновлений во второй вторник месяца.",
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
                            name = "ScheduledInstallSecondWeek",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. security-wu-fill-empty-content-urls
                new TweakItem
                {
                    id = "security-wu-fill-empty-content-urls",
                    name_ru = "Запретить автоматическое заполнение пустых URL-адресов сервера обновлений",
                    description_ru = "Блокирует перенаправление запросов содержимого обновлений при ненастроенном внутреннем сервере.",
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
                            name = "FillEmptyContentUrls",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. security-wu-donot-enforce-cert-pinning
                new TweakItem
                {
                    id = "security-wu-donot-enforce-cert-pinning",
                    name_ru = "Включить обязательную привязку TLS-сертификатов (Cert Pinning) для обновлений",
                    description_ru = "Требует строгую валидацию корпоративных сертификатов TLS для предотвращения MITM-атак на обновления.",
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
                            name = "DoNotEnforceEnterpriseTLSCertPinningForUpdateDetection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. security-wu-proxy-behavior
                new TweakItem
                {
                    id = "security-wu-proxy-behavior",
                    name_ru = "Отключить использование системного прокси для обнаружения обновлений",
                    description_ru = "Направляет проверку обновлений напрямую, минуя возможные сторонние прокси-серверы.",
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
                            name = "SetProxyBehaviorForUpdateDetection",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. security-wu-target-release-version-info
                new TweakItem
                {
                    id = "security-wu-target-release-version-info",
                    name_ru = "Сбросить целевую версию функционального обновления (TargetReleaseVersionInfo)",
                    description_ru = "Очищает принудительную фиксацию конкретного билда Windows (например, 22H2/23H2) при обновлениях.",
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
                            name = "TargetReleaseVersionInfo",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. security-wu-branch-readiness-level
                new TweakItem
                {
                    id = "security-wu-branch-readiness-level",
                    name_ru = "Установить уровень готовности ветки обновлений на Semi-Annual Channel (16)",
                    description_ru = "Переводит получение обновлений в канал стабильных релизов для корпоративных систем.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 497, line_to = 499 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "BranchReadinessLevel",
                            type = "REG_DWORD",
                            apply = 16,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. security-wu-defer-quality-updates-days
                new TweakItem
                {
                    id = "security-wu-defer-quality-updates-days",
                    name_ru = "Сбросить задержку получения качественных обновлений (0 дней)",
                    description_ru = "Устраняет искусственную задержку в днях перед получением исправлений безопасности.",
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
                            name = "DeferQualityUpdatesPeriodInDays",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. security-powershell-machine-enable-scripts
                new TweakItem
                {
                    id = "security-powershell-machine-enable-scripts",
                    name_ru = "Разрешить выполнение подписанных сценариев PowerShell (Machine)",
                    description_ru = "Активирует контролируемое выполнение административных скриптов на уровне локального компьютера.",
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
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell",
                            name = "EnableScripts",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. security-powershell-machine-execution-policy
                new TweakItem
                {
                    id = "security-powershell-machine-execution-policy",
                    name_ru = "Установить политику выполнения PowerShell RemoteSigned (Machine)",
                    description_ru = "Требует наличия доверенной цифровой подписи для загружаемых из сети скриптов PowerShell.",
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
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell",
                            name = "ExecutionPolicy",
                            type = "REG_SZ",
                            apply = "RemoteSigned",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. security-powershell-user-execution-policy
                new TweakItem
                {
                    id = "security-powershell-user-execution-policy",
                    name_ru = "Установить политику выполнения PowerShell RemoteSigned (User)",
                    description_ru = "Фиксирует безопасный режим выполнения удаленно загруженных скриптов для текущего пользователя.",
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
                            name = "ExecutionPolicy",
                            type = "REG_SZ",
                            apply = "RemoteSigned",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. security-powershell-invocation-header
                new TweakItem
                {
                    id = "security-powershell-invocation-header",
                    name_ru = "Включить заголовок вызова команд в транскриптах PowerShell",
                    description_ru = "Добавляет временную метку и контекст запуска в начало каждой записи протокола PowerShell.",
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
                            name = "EnableInvocationHeader",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. security-powershell-scriptblock-invocation-logging
                new TweakItem
                {
                    id = "security-powershell-scriptblock-invocation-logging",
                    name_ru = "Включить протоколирование событий вызова блоков сценариев PowerShell",
                    description_ru = "Записывает событие начала и завершения выполнения каждого блока кода в журнал безопасности.",
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
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging",
                            name = "EnableScriptBlockInvocationLogging",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. security-deviceguard-lsa-cfg-flags
                new TweakItem
                {
                    id = "security-deviceguard-lsa-cfg-flags",
                    name_ru = "Отключить изоляцию учетных данных LSA в Credential Guard (LsaCfgFlags)",
                    description_ru = "Отключает принудительную виртуализацию процесса LSASS, снижая задержки переключения контекста.",
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
                            name = "LsaCfgFlags",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. visibility-dwm-disallow-animations-machine
                new TweakItem
                {
                    id = "visibility-dwm-disallow-animations-machine",
                    name_ru = "Отключить все анимации окон DWM на уровне системы (Machine)",
                    description_ru = "Полностью отключает системные анимации окон менеджера рабочего стола (DWM) для максимального быстродействия.",
                    group = "Анимации DWM",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1080, line_to = 1082 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\DWM",
                            name = "DisallowAnimations",
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
            Console.WriteLine("Успешно добавлено 25 твиков Блока 27 в data/tweaks.json!");
        }
    }
}
