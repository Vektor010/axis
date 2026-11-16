using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock23Tool
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
                // 1. security-defender-allow-network-protection-server
                new TweakItem
                {
                    id = "security-defender-allow-network-protection-server",
                    name_ru = "Разрешить защиту сети Exploit Guard на Windows Server",
                    description_ru = "Управляет возможностью перевода компонента Network Protection в режим блокировки или аудита на серверных выпусках Windows.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 295, line_to = 297 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection",
                            name = "AllowNetworkProtectionOnWinServer",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. security-defender-local-override-spynet
                new TweakItem
                {
                    id = "security-defender-local-override-spynet",
                    name_ru = "Запретить локальное переопределение отчетов Spynet",
                    description_ru = "Блокирует возможность изменения настроек отчетов Microsoft MAPS / Spynet через локальный интерфейс пользователя или реестр.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 298, line_to = 300 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Spynet",
                            name = "LocalSettingOverrideSpynetReporting",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. security-defender-center-hide-all-notifications
                new TweakItem
                {
                    id = "security-defender-center-hide-all-notifications",
                    name_ru = "Скрыть все уведомления центра безопасности Защитника",
                    description_ru = "Отключает отображение любых системных всплывающих оповещений службы Windows Defender Security Center.",
                    group = "Центр безопасности",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 305, line_to = 307 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications",
                            name = "DisableNotifications",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. security-defender-disable-antispyware
                new TweakItem
                {
                    id = "security-defender-disable-antispyware",
                    name_ru = "Включить модуль защиты от шпионских программ (AntiSpyware)",
                    description_ru = "Гарантирует постоянную активность встроенной антивирусной службы Windows Defender AntiSpyware.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 308, line_to = 310 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows Defender",
                            name = "DisableAntiSpyware",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. security-wu-disable-windows-update-access
                new TweakItem
                {
                    id = "security-wu-disable-windows-update-access",
                    name_ru = "Заблокировать доступ ко всем функциям Центра обновления Windows",
                    description_ru = "Полностью отключает сканирование, загрузку и установку обновлений Windows через клиент Windows Update.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "medium",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 488, line_to = 490 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "DisableWindowsUpdateAccess",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. visibility-lockscreen-overlays-disabled
                new TweakItem
                {
                    id = "visibility-lockscreen-overlays-disabled",
                    name_ru = "Отключить наложения и подсказки на экране блокировки",
                    description_ru = "Отключает показ развлекательных и рекламных оверлеев, подсказок и фактов поверх обоев экрана блокировки.",
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
                            name = "LockScreenOverlaysDisabled",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. visibility-search-on-taskbar-mode
                new TweakItem
                {
                    id = "visibility-search-on-taskbar-mode",
                    name_ru = "Скрыть поле и значок поиска на панели задач",
                    description_ru = "Убирает поисковую строку и значок лупы с панели задач для освобождения пространства рабочей области.",
                    group = "Панель задач",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1301, line_to = 1303 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Windows Search",
                            name = "SearchOnTaskbarMode",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. security-wu-allow-optional-content
                new TweakItem
                {
                    id = "security-wu-allow-optional-content",
                    name_ru = "Запретить автоматическую установку необязательного контента обновлений",
                    description_ru = "Предотвращает автоматическую загрузку сторонних необязательных компонентов и экспериментальных правок Центра обновления.",
                    group = "Центр обновления Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 503, line_to = 505 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                            name = "AllowOptionalContent",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. security-defender-disable-block-at-first-seen
                new TweakItem
                {
                    id = "security-defender-disable-block-at-first-seen",
                    name_ru = "Отключить облачную блокировку при первом появлении (Block at First Sight)",
                    description_ru = "Отключает принудительную задержку запуска новых неизвестных исполняемых файлов для синхронной облачной проверки.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 297, line_to = 299 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Spynet",
                            name = "DisableBlockAtFirstSeen",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. security-filesystem-ntfs-disable-encryption
                new TweakItem
                {
                    id = "security-filesystem-ntfs-disable-encryption",
                    name_ru = "Запретить шифрование файлов EFS на всех томах NTFS",
                    description_ru = "Отключает встроенное шифрование NTFS (Encrypted File System) для предотвращения случайной блокировки доступа к данным.",
                    group = "Файловая система",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1067, line_to = 1069 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Policies",
                            name = "NtfsDisableEncryption",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. security-defender-center-hide-non-critical-notifications
                new TweakItem
                {
                    id = "security-defender-center-hide-non-critical-notifications",
                    name_ru = "Скрыть некритические уведомления Центра безопасности Защитника",
                    description_ru = "Подавляет регулярные информационные сообщения о завершении сканирования и состоянии безопасности системы.",
                    group = "Центр безопасности",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8411, line_to = 8413 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications",
                            name = "DisableEnhancedNotifications",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. privacy-configure-windows-spotlight
                new TweakItem
                {
                    id = "privacy-configure-windows-spotlight",
                    name_ru = "Отключить Windows Spotlight на экране блокировки",
                    description_ru = "Отключает автоматическую загрузку фоновых изображений и подсказок службы Spotlight на экране блокировки.",
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
                            name = "ConfigureWindowsSpotlight",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. privacy-display-last-logon-info
                new TweakItem
                {
                    id = "privacy-display-last-logon-info",
                    name_ru = "Отображать информацию о предыдущих входах пользователя в систему",
                    description_ru = "Показывает дату и время предыдущего успешного и неудачного входов при авторизации в систему.",
                    group = "Учетные записи",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 639, line_to = 641 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "DisplayLastLogonInfo",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-dont-display-logon-hours-warnings
                new TweakItem
                {
                    id = "privacy-dont-display-logon-hours-warnings",
                    name_ru = "Скрыть предупреждения об окончании разрешенных часов входа",
                    description_ru = "Отключает всплывающие системные диалоговые окна с предупреждениями об ограничении времени работы.",
                    group = "Учетные записи",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 640, line_to = 642 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "DontDisplayLogonHoursWarnings",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-allow-camera-global
                new TweakItem
                {
                    id = "privacy-allow-camera-global",
                    name_ru = "Глобально запретить использование веб-камеры в Windows",
                    description_ru = "Аппаратно блокирует доступ приложений и системных служб к видеокамере через групповую политику.",
                    group = "Конфиденциальность периферии",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1153, line_to = 1155 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"software\Policies\Microsoft\Camera",
                            name = "AllowCamera",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. privacy-sync-disable-appsync-sync
                new TweakItem
                {
                    id = "privacy-sync-disable-appsync-sync",
                    name_ru = "Отключить облачную синхронизацию установленных приложений",
                    description_ru = "Запрещает передачу списка и настроек установленных приложений в облачную учетную запись Microsoft.",
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
                            name = "DisableAppSyncSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. privacy-sync-disable-windows-setting-sync
                new TweakItem
                {
                    id = "privacy-sync-disable-windows-setting-sync",
                    name_ru = "Отключить облачную синхронизацию других системных настроек Windows",
                    description_ru = "Предотвращает передачу персональных настроек рабочего стола и проводника на другие устройства.",
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
                            name = "DisableWindowsSettingSync",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-netcache-work-offline-disabled
                new TweakItem
                {
                    id = "privacy-netcache-work-offline-disabled",
                    name_ru = "Удалить команду Автономная работа (NetCache)",
                    description_ru = "Удаляет пункт автономной работы из проводника и контекстного меню сетевых ресурсов.",
                    group = "Автономные файлы (NetCache)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1337, line_to = 1339 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\NetCache",
                            name = "WorkOfflineDisabled",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-mdm-disable-auto-enroll
                new TweakItem
                {
                    id = "privacy-mdm-disable-auto-enroll",
                    name_ru = "Отключить автоматическое зачисление в MDM через Azure AD",
                    description_ru = "Блокирует автоматическую регистрацию локального компьютера в корпоративной службе MDM при входе.",
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
                            name = "AutoEnrollMDM",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. privacy-cortana-disable-aad-oobe
                new TweakItem
                {
                    id = "privacy-cortana-disable-aad-oobe",
                    name_ru = "Отключить страницу Cortana при первичной настройке системы (OOBE)",
                    description_ru = "Убирает этап настройки голосового помощника Cortana из мастера первоначальной настройки Windows OOBE.",
                    group = "Cortana и голосовой поиск",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1229, line_to = 1231 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCortanaInAAD",
                            name = "AllowCortanaInAADPathOOBE",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. privacy-wmdrm-disable-online
                new TweakItem
                {
                    id = "privacy-wmdrm-disable-online",
                    name_ru = "Запретить сетевой доступ Windows Media DRM к Интернету",
                    description_ru = "Блокирует отправку сетевых запросов и проверку лицензий проигрывателем Windows Media DRM.",
                    group = "Управление цифровыми правами (DRM)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1247, line_to = 1249 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\WMDRM",
                            name = "DisableOnline",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. privacy-netcache-no-reminders
                new TweakItem
                {
                    id = "privacy-netcache-no-reminders",
                    name_ru = "Отключить всплывающие напоминания службы автономных файлов (NetCache)",
                    description_ru = "Подавляет уведомления в трее о переходах в автономный режим и статусе сетевых папок.",
                    group = "Автономные файлы (NetCache)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1333, line_to = 1335 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\NetCache",
                            name = "NoReminders",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. privacy-netcache-sync-at-logoff
                new TweakItem
                {
                    id = "privacy-netcache-sync-at-logoff",
                    name_ru = "Отключить принудительную синхронизацию автономных файлов при выходе из системы",
                    description_ru = "Ускоряет завершение сеанса и выключение ПК за счет отключения синхронизации NetCache.",
                    group = "Автономные файлы (NetCache)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1334, line_to = 1336 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\NetCache",
                            name = "SyncAtLogoff",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. privacy-netcache-sync-at-logon
                new TweakItem
                {
                    id = "privacy-netcache-sync-at-logon",
                    name_ru = "Отключить принудительную синхронизацию автономных файлов при входе в систему",
                    description_ru = "Ускоряет загрузку рабочего стола за счет пропуска проверки сетевых автономных папок при входе.",
                    group = "Автономные файлы (NetCache)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1335, line_to = 1337 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\NetCache",
                            name = "SyncAtLogon",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. privacy-netcache-disable-background-sync
                new TweakItem
                {
                    id = "privacy-netcache-disable-background-sync",
                    name_ru = "Отключить фоновую периодическую синхронизацию автономных файлов",
                    description_ru = "Снижает нагрузку на диск и сеть, запрещая периодическую фоновую сверку автономных файлов.",
                    group = "Автономные файлы (NetCache)",
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
                            name = "BackgroundSyncEnabled",
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

            Console.WriteLine("Новое число твиков после добавления Блока 23: " + list.Count);

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
