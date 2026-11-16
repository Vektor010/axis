using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock16Tool
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
                // 1. privacy-app-access-account-info
                new TweakItem
                {
                    id = "privacy-app-access-account-info",
                    name_ru = "Запретить приложениям доступ к данным учетной записи",
                    description_ru = "Блокирует UWP-приложениям доступ к имени пользователя, аватару и информации об учетной записи Windows.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1060, line_to = 1062 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessAccountInfo",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. privacy-app-access-calendar
                new TweakItem
                {
                    id = "privacy-app-access-calendar",
                    name_ru = "Запретить приложениям доступ к календарю",
                    description_ru = "Блокирует приложениям чтение системного календаря и событий расписания.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1061, line_to = 1063 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessCalendar",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. privacy-app-access-call-history
                new TweakItem
                {
                    id = "privacy-app-access-call-history",
                    name_ru = "Запретить приложениям доступ к журналу звонков",
                    description_ru = "Запрещает установленным приложениям просматривать историю телефонных и голосовых вызовов.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1062, line_to = 1064 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessCallHistory",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. privacy-app-access-contacts
                new TweakItem
                {
                    id = "privacy-app-access-contacts",
                    name_ru = "Запретить приложениям доступ к контактам",
                    description_ru = "Блокирует доступ сторонних приложений к адресной книге и контактам Windows.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1064, line_to = 1066 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessContacts",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. privacy-app-access-email
                new TweakItem
                {
                    id = "privacy-app-access-email",
                    name_ru = "Запретить приложениям доступ к электронной почте",
                    description_ru = "Запрещает приложениям читать входящую почту и отправлять сообщения от имени пользователя.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1065, line_to = 1067 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessEmail",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. privacy-app-access-system-ai-models
                new TweakItem
                {
                    id = "privacy-app-access-system-ai-models",
                    name_ru = "Запретить приложениям доступ к локальным ИИ-моделям Windows",
                    description_ru = "Блокирует приложениям доступ к встроенным генеративным моделям текста и изображений Windows Copilot/AI.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 26100,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1066, line_to = 1068 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessSystemAIModels",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-app-access-graphics-capture
                new TweakItem
                {
                    id = "privacy-app-access-graphics-capture",
                    name_ru = "Запретить приложениям автоматический захват экрана",
                    description_ru = "Блокирует API Graphics Capture от создания фоновых скриншотов окон и рабочих столов без подтверждения.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1067, line_to = 1069 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessGraphicsCaptureProgrammatic",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. privacy-app-access-human-presence
                new TweakItem
                {
                    id = "privacy-app-access-human-presence",
                    name_ru = "Запретить приложениям доступ к датчикам присутствия человека",
                    description_ru = "Блокирует опрос встроенных сенсоров присутствия и приближения пользователя перед экраном.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1069, line_to = 1071 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessHumanPresence",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. privacy-app-access-messaging
                new TweakItem
                {
                    id = "privacy-app-access-messaging",
                    name_ru = "Запретить приложениям доступ к SMS и чатам",
                    description_ru = "Блокирует приложениям доступ к системным текстовым сообщениям и службам обмена сообщениями.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1071, line_to = 1073 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessMessaging",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. privacy-app-access-motion
                new TweakItem
                {
                    id = "privacy-app-access-motion",
                    name_ru = "Запретить приложениям доступ к датчикам движения",
                    description_ru = "Запрещает приложениям опрашивать данные акселерометра, гироскопа и шагомера.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1073, line_to = 1075 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessMotion",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. privacy-app-access-radios
                new TweakItem
                {
                    id = "privacy-app-access-radios",
                    name_ru = "Запретить приложениям управлять радиомодулями (Bluetooth/Wi-Fi)",
                    description_ru = "Блокирует сторонним приложениям возможность программно включать и отключать Bluetooth и беспроводные адаптеры.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1076, line_to = 1078 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessRadios",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. privacy-app-access-sync-devices
                new TweakItem
                {
                    id = "privacy-app-access-sync-devices",
                    name_ru = "Запретить приложениям связь с несопряженными устройствами",
                    description_ru = "Запрещает приложениям автоматически обмениваться данными с маяками и несопряженными внешними устройствами.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1077, line_to = 1079 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsSyncWithDevices",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. privacy-app-access-tasks
                new TweakItem
                {
                    id = "privacy-app-access-tasks",
                    name_ru = "Запретить приложениям доступ к системным задачам (Tasks)",
                    description_ru = "Блокирует доступ сторонних приложений к спискам задач и напоминаниям пользователя.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1078, line_to = 1080 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessTasks",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-app-access-trusted-devices
                new TweakItem
                {
                    id = "privacy-app-access-trusted-devices",
                    name_ru = "Запретить приложениям доступ к доверенным устройствам",
                    description_ru = "Запрещает приложениям напрямую открывать каналы связи с доверенными периферийными устройствами.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1079, line_to = 1081 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessTrustedDevices",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-app-access-diagnostic-info
                new TweakItem
                {
                    id = "privacy-app-access-diagnostic-info",
                    name_ru = "Запретить приложениям сбор диагностики о других программах",
                    description_ru = "Блокирует сторонним приложениям опрос диагностических данных и телеметрии о работе других установленных программ.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1080, line_to = 1082 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsGetDiagnosticInfo",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. privacy-app-access-gaze-input
                new TweakItem
                {
                    id = "privacy-app-access-gaze-input",
                    name_ru = "Запретить приложениям доступ к устройствам отслеживания взгляда (Eye Tracker)",
                    description_ru = "Блокирует приложениям доступ к датчикам айтрекинга и координатам взгляда пользователя.",
                    group = "Разрешения приложений",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1081, line_to = 1083 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                            name = "LetAppsAccessGazeInput",
                            type = "REG_DWORD",
                            apply = 2,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. privacy-terminal-services-logging
                new TweakItem
                {
                    id = "privacy-terminal-services-logging",
                    name_ru = "Отключить журналирование сеансов удаленной помощи (RA Logging)",
                    description_ru = "Запрещает ведение журнала подключений и протоколирование сеансов Remote Assistance.",
                    group = "Удаленный доступ и терминалы",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1032, line_to = 1034 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\policies\Microsoft\Windows NT\Terminal Services",
                            name = "LoggingEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. security-connect-require-pin-pairing
                new TweakItem
                {
                    id = "security-connect-require-pin-pairing",
                    name_ru = "Требовать ввод PIN-кода для сопряжения беспроводного экрана (Miracast)",
                    description_ru = "Обязывает вводить PIN-код при беспроводном проецировании экрана на ПК для предотвращения несанкционированного подключения.",
                    group = "Безопасность беспроводных соединений",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1036, line_to = 1038 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Connect",
                            name = "RequirePinForPairing",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. system-fs-trim-delete-notification
                new TweakItem
                {
                    id = "system-fs-trim-delete-notification",
                    name_ru = "Включить отправку команд TRIM/UNMAP для SSD (DisableDeleteNotification 0)",
                    description_ru = "Гарантирует, что команды TRIM немедленно уведомляют SSD-накопитель об освобождении блоков, поддерживая высокую скорость записи.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8512, line_to = 8514 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "DisableDeleteNotification",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 20. system-fs-dont-verify-random-drivers
                new TweakItem
                {
                    id = "system-fs-dont-verify-random-drivers",
                    name_ru = "Запретить случайный выбор драйверов в Driver Verifier",
                    description_ru = "Исключает случайную верификацию системных драйверов при загрузке, предотвращая задержки DPC и ложные синие экраны.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8513, line_to = 8515 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "DontVerifyRandomDrivers",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 21. system-fs-ntfs-bugcheck-on-corrupt
                new TweakItem
                {
                    id = "system-fs-ntfs-bugcheck-on-corrupt",
                    name_ru = "Предотвращать повреждение данных NTFS (BugcheckOnCorrupt)",
                    description_ru = "Вызывает аварийный перезапуск при обнаружении критического повреждения тома NTFS, исключая скрытую потерю данных.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8516, line_to = 8518 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "NtfsBugcheckOnCorrupt",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 22. system-fs-ntfs-disable-spot-corruption
                new TweakItem
                {
                    id = "system-fs-ntfs-disable-spot-corruption",
                    name_ru = "Отключить фоновую обработку повреждений NTFS (Spot Corruption Handling)",
                    description_ru = "Отключает фоновые попытки самовосстановления тома в реальном времени, устраняя микрофризы дисковой подсистемы.",
                    group = "Файловая система и диски",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8522, line_to = 8524 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\FileSystem",
                            name = "NtfsDisableSpotCorruptionHandling",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 23. peripheral-touchpad-tap-and-drag
                new TweakItem
                {
                    id = "peripheral-touchpad-tap-and-drag",
                    name_ru = "Включить перетаскивание двойным касанием на тачпаде (Tap and Drag)",
                    description_ru = "Позволяет выделять и перетаскивать окна двойным быстрым касанием сенсорной панели без физического нажатия.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1438, line_to = 1440 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                            name = "TapAndDrag",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 24. peripheral-touchpad-pan-enabled
                new TweakItem
                {
                    id = "peripheral-touchpad-pan-enabled",
                    name_ru = "Включить прокрутку двумя пальцами (Two-Finger Panning)",
                    description_ru = "Активирует плавную двухпальцевую прокрутку содержимого окон в обоих направлениях.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1441, line_to = 1443 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\PrecisionTouchPad",
                            name = "PanEnabled",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 25. peripheral-pen-sys-latency-optimal
                new TweakItem
                {
                    id = "peripheral-pen-sys-latency-optimal",
                    name_ru = "Минимальная задержка опроса пера и сенсора (Pen Latency 8 ms)",
                    description_ru = "Устанавливает аппаратный опрос пера и дигитайзера Wisp на минимальное время выборки 8 мс для плавного ввода.",
                    group = "Сенсорная панель и устройства ввода",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1457, line_to = 1459 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Wisp\Pen\SysEventParameters",
                            name = "Latency",
                            type = "REG_DWORD",
                            apply = 8,
                            revert = new RevertObj { mode = "set", value = 16 }
                        }
                    }
                }
            };

            Console.WriteLine("Добавляем новых твиков: " + newTweaks.Count);
            list.AddRange(newTweaks);
            Console.WriteLine("Новое суммарное число твиков: " + list.Count);

            string serialized = serializer.Serialize(list);
            string formatted = FormatJson(serialized);

            File.WriteAllText(jsonPath, formatted, new UTF8Encoding(false));
            Console.WriteLine("Файл tweaks.json успешно сохранен в кодировке UTF-8 без BOM!");
        }

        static string FormatJson(string json)
        {
            var sb = new StringBuilder();
            bool quoted = false;
            int indent = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', ++indent * 2));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', --indent * 2));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 2));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
