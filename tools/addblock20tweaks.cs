using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock20Tool
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
                // 1. security-pua-protection
                new TweakItem
                {
                    id = "security-pua-protection",
                    name_ru = "Включить блокировку потенциально нежелательных приложений (PUA)",
                    description_ru = "Блокирует установку нежелательного и рекламного ПО с помощью встроенного антивируса.",
                    group = "Защитник Windows",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 286, line_to = 288 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender",
                            name = "PUAProtection",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. security-block-at-first-seen
                new TweakItem
                {
                    id = "security-block-at-first-seen",
                    name_ru = "Включить мгновенную блокировку неизвестных угроз (Block at First Sight)",
                    description_ru = "Включает проверку новых неизвестных файлов в реальном времени через облако Защитника.",
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
                            path = @"Software\Policies\Microsoft\Windows Defender\MpEngine",
                            name = "DisableBlockAtFirstSeen",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. security-enable-network-protection
                new TweakItem
                {
                    id = "security-enable-network-protection",
                    name_ru = "Включить сетевую защиту Защитника Windows",
                    description_ru = "Предотвращает доступ любых системных приложений к опасным доменам и фишинговым сайтам.",
                    group = "Сетевая безопасность",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 303, line_to = 305 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection",
                            name = "EnableNetworkProtection",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. security-controlled-folder-access
                new TweakItem
                {
                    id = "security-controlled-folder-access",
                    name_ru = "Включить контролируемый доступ к папкам (Защита от шифровальщиков)",
                    description_ru = "Блокирует несанкционированные изменения личных документов и файлов подозрительными программами.",
                    group = "Защита от вымогателей",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 304, line_to = 306 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access",
                            name = "EnableControlledFolderAccess",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. security-no-auto-update
                new TweakItem
                {
                    id = "security-no-auto-update",
                    name_ru = "Отключить автоматическое скачивание и установку обновлений Windows",
                    description_ru = "Переводит центр обновления Windows в ручной режим без фоновых перезагрузок и скачиваний.",
                    group = "Центр обновления",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 493, line_to = 495 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU",
                            name = "NoAutoUpdate",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. security-allow-optional-content
                new TweakItem
                {
                    id = "security-allow-optional-content",
                    name_ru = "Запретить автоматическую загрузку необязательных обновлений",
                    description_ru = "Блокирует фоновую загрузку предварительных версий компонентов и сторонних драйверов.",
                    group = "Центр обновления",
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
                            name = "SetAllowOptionalContent",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-disable-device-delete
                new TweakItem
                {
                    id = "privacy-disable-device-delete",
                    name_ru = "Разрешить удаление диагностических данных с серверов Microsoft",
                    description_ru = "Оставляет активной возможность очистки собранных телеметрических данных устройства.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 118, line_to = 120 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "DisableDeviceDelete",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. privacy-limit-diagnostic-log-collection
                new TweakItem
                {
                    id = "privacy-limit-diagnostic-log-collection",
                    name_ru = "Ограничить сбор диагностических журналов",
                    description_ru = "Запрещает расширенный сбор детальных системных логов при возникновении неполадок.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 120, line_to = 122 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "LimitDiagnosticLogCollection",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. privacy-limit-dump-collection
                new TweakItem
                {
                    id = "privacy-limit-dump-collection",
                    name_ru = "Ограничить сбор аварийных дампов памяти",
                    description_ru = "Ограничивает отправку дампов оперативной памяти при сбоях системы для защиты конфиденциальности.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 121, line_to = 123 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "LimitDumpCollection",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. privacy-disable-send-generic-driver-wer
                new TweakItem
                {
                    id = "privacy-disable-send-generic-driver-wer",
                    name_ru = "Запретить отправку отчетов WER при установке стандартного драйвера",
                    description_ru = "Блокирует отправку телеметрии в Windows Error Reporting при установке совместимых драйверов.",
                    group = "Отчеты об ошибках (WER)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 330, line_to = 332 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DeviceInstall\Settings",
                            name = "DisableSendGenericDriverNotFoundToWER",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. privacy-disable-send-request-additional-software-wer
                new TweakItem
                {
                    id = "privacy-disable-send-request-additional-software-wer",
                    name_ru = "Запретить отправку отчетов WER при запросе драйвером ПО",
                    description_ru = "Предотвращает передачу данных в Microsoft при запросе оборудованием дополнительного ПО.",
                    group = "Отчеты об ошибках (WER)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 331, line_to = 333 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DeviceInstall\Settings",
                            name = "DisableSendRequestAdditionalSoftwareToWER",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. privacy-troubleshooting-enabled-execution
                new TweakItem
                {
                    id = "privacy-troubleshooting-enabled-execution",
                    name_ru = "Отключить выполнение запланированной диагностики",
                    description_ru = "Отключает периодические фоновые проверки системы встроенными мастерами диагностики.",
                    group = "Диагностика",
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
                            name = "EnabledExecution",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. privacy-wdi-scenario-execution
                new TweakItem
                {
                    id = "privacy-wdi-scenario-execution",
                    name_ru = "Отключить сценарии диагностики инфраструктуры WDI",
                    description_ru = "Отключает фоновое выполнение сценариев сбора диагностических данных Windows Diagnostic Infrastructure.",
                    group = "Диагностика",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WDI\{9c5a40da-b965-4fc3-8781-88dd50a6299d}",
                            name = "ScenarioExecutionEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-allow-online-tips
                new TweakItem
                {
                    id = "privacy-allow-online-tips",
                    name_ru = "Отключить интерактивные веб-подсказки в параметрах",
                    description_ru = "Блокирует подгрузку справочных веб-подсказок из интернета в окне настроек Windows.",
                    group = "Интерфейс",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 449, line_to = 451 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "AllowOnlineTips",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-disable-search-box-suggestions
                new TweakItem
                {
                    id = "privacy-disable-search-box-suggestions",
                    name_ru = "Отключить историю поисковых запросов в Проводнике",
                    description_ru = "Запрещает сохранение и показ всплывающего списка предыдущих запросов в строке поиска Проводника.",
                    group = "Проводник",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 452, line_to = 454 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "DisableSearchBoxSuggestions",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. privacy-disable-spotlight-welcome-experience
                new TweakItem
                {
                    id = "privacy-disable-spotlight-welcome-experience",
                    name_ru = "Отключить экран приветствия Windows Spotlight",
                    description_ru = "Отключает полноэкранные промо-сообщения и предложения Spotlight при первом входе в систему.",
                    group = "Windows Spotlight",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 459, line_to = 461 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableWindowsSpotlightWindowsWelcomeExperience",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. privacy-allow-untriggered-maps-traffic
                new TweakItem
                {
                    id = "privacy-allow-untriggered-maps-traffic",
                    name_ru = "Запретить фоновый сетевой трафик автономных карт",
                    description_ru = "Блокирует нежелательный фоновый трафик и проверку обновлений автономных карт без запроса.",
                    group = "Сетевые службы",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 523, line_to = 525 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Maps",
                            name = "AllowUntriggeredNetworkTrafficOnSettingsPage",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-allow-cross-device-clipboard
                new TweakItem
                {
                    id = "privacy-allow-cross-device-clipboard",
                    name_ru = "Отключить синхронизацию буфера обмена между устройствами",
                    description_ru = "Запрещает передачу содержимого скопированного текста и картинок в облако на другие устройства.",
                    group = "Буфер обмена",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 736, line_to = 738 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "AllowCrossDeviceClipboard",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-allow-clipboard-history
                new TweakItem
                {
                    id = "privacy-allow-clipboard-history",
                    name_ru = "Разрешить локальный журнал буфера обмена (Win+V)",
                    description_ru = "Обеспечивает доступность удобного журнала буфера обмена без отправки данных в облако.",
                    group = "Буфер обмена",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 737, line_to = 739 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "AllowClipboardHistory",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. privacy-allow-recall-enablement
                new TweakItem
                {
                    id = "privacy-allow-recall-enablement",
                    name_ru = "Запретить создание моментальных снимков Windows Recall",
                    description_ru = "Полностью отключает фоновую фиксацию экрана и анализ активности компонентом AI Windows Recall.",
                    group = "Искусственный интеллект (AI)",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 846, line_to = 848 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows AI",
                            name = "AllowRecallEnablement",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. privacy-allow-game-dvr
                new TweakItem
                {
                    id = "privacy-allow-game-dvr",
                    name_ru = "Отключить фоновую запись игр (Game DVR)",
                    description_ru = "Отключает постоянную фоновую запись игрового процесса, снижающую FPS и нагружающую видеокарту.",
                    group = "Игровой режим",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 921, line_to = 923 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\GameDVR",
                            name = "AllowGameDVR",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. privacy-disable-windows-location-provider
                new TweakItem
                {
                    id = "privacy-disable-windows-location-provider",
                    name_ru = "Отключить службу определения местоположения Windows",
                    description_ru = "Отключает системный поставщик геолокации Windows Location Provider.",
                    group = "Геолокация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 952, line_to = 954 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LocationAndSensors",
                            name = "DisableWindowsLocationProvider",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. privacy-disable-sensors
                new TweakItem
                {
                    id = "privacy-disable-sensors",
                    name_ru = "Отключить системные датчики и сенсоры",
                    description_ru = "Отключает датчики освещенности, гироскопы и акселерометры для снижения фоновой нагрузки.",
                    group = "Датчики и сенсоры",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 953, line_to = 955 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LocationAndSensors",
                            name = "DisableSensors",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. privacy-disable-location
                new TweakItem
                {
                    id = "privacy-disable-location",
                    name_ru = "Полностью отключить отслеживание местоположения",
                    description_ru = "Блокирует определение координат устройства на системном уровне для всех учетных записей.",
                    group = "Геолокация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 954, line_to = 956 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LocationAndSensors",
                            name = "DisableLocation",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. privacy-disable-location-scripting
                new TweakItem
                {
                    id = "privacy-disable-location-scripting",
                    name_ru = "Запретить сценариям веб-страниц доступ к местоположению",
                    description_ru = "Блокирует веб-скриптам и браузерным расширениям опрос координат пользователя.",
                    group = "Геолокация",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 955, line_to = 957 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\LocationAndSensors",
                            name = "DisableLocationScripting",
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
