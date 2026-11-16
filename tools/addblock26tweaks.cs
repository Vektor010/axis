using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock26Tool
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
                // 1. peripheral-autorun-no-autorun
                new TweakItem
                {
                    id = "peripheral-autorun-no-autorun",
                    name_ru = "Отключить автозапуск программ (AutoRun) при подключении носителей",
                    description_ru = "Запрещает автоматический запуск исполняемых файлов autorun.inf с внешних USB-накопителей и дисков.",
                    group = "Внешние устройства",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1390, line_to = 1392 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "NoAutorun",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. peripheral-autoplay-no-drivetype-autorun
                new TweakItem
                {
                    id = "peripheral-autoplay-no-drivetype-autorun",
                    name_ru = "Отключить автовоспроизведение (AutoPlay) для всех типов дисков",
                    description_ru = "Отключает службу и всплывающие окна автовоспроизведения для оптических, съемных и сетевых дисков (маска 0xFF).",
                    group = "Внешние устройства",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1391, line_to = 1393 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "NoDriveTypeAutoRun",
                            type = "REG_DWORD",
                            apply = 255,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. peripheral-autoplay-no-autoplay-for-non-volume
                new TweakItem
                {
                    id = "peripheral-autoplay-no-autoplay-for-non-volume",
                    name_ru = "Запретить автозапуск для устройств без файловых томов (камеры, MTP)",
                    description_ru = "Блокирует автозапуск мультимедиа для портативных устройств, смартфонов и камер без стандартной файловой системы.",
                    group = "Внешние устройства",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1392, line_to = 1394 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "NoAutoplayfornonVolume",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. peripheral-autoplay-dont-set-checkbox
                new TweakItem
                {
                    id = "peripheral-autoplay-dont-set-checkbox",
                    name_ru = "Запретить AutoPlay запоминать выбор пользователя",
                    description_ru = "Предотвращает сохранение постоянного действия для подключенных съемных носителей в диалоге автозапуска.",
                    group = "Внешние устройства",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1393, line_to = 1395 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Explorer",
                            name = "DontSetAutoplayCheckbox",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. peripheral-printer-disable-web-pnp-download
                new TweakItem
                {
                    id = "peripheral-printer-disable-web-pnp-download",
                    name_ru = "Отключить загрузку драйверов печати через HTTP",
                    description_ru = "Запрещает автоматическую загрузку сторонних пакетов драйверов принтеров по протоколу HTTP из интернета.",
                    group = "Периферия",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1831, line_to = 1833 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows NT\Printers",
                            name = "DisableWebPnPDownload",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. peripheral-printer-downlevel-browse
                new TweakItem
                {
                    id = "peripheral-printer-downlevel-browse",
                    name_ru = "Отключить сетевой поиск принтеров ранних версий",
                    description_ru = "Отключает широковещательный поиск сетевых принтеров устаревших версий в мастере установки.",
                    group = "Периферия",
                    category = "system",
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
                            path = @"Software\Policies\Microsoft\Windows NT\Printers\Wizard",
                            name = "Downlevel Browse",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. privacy-datacollection-commercial-id
                new TweakItem
                {
                    id = "privacy-datacollection-commercial-id",
                    name_ru = "Очистить коммерческий идентификатор телеметрии Windows",
                    description_ru = "Удаляет привязку устройства к корпоративному коммерческому идентификатору отправки диагностических данных.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 122, line_to = 124 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\DataCollection",
                            name = "CommercialId",
                            type = "REG_SZ",
                            apply = "",
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. privacy-tabletpc-prevent-handwriting-error-reports
                new TweakItem
                {
                    id = "privacy-tabletpc-prevent-handwriting-error-reports",
                    name_ru = "Отключить отправку отчетов об ошибках распознавания рукописного ввода",
                    description_ru = "Запрещает отправку фрагментов рукописного ввода и отчетов об ошибках распознавания в Microsoft.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 125, line_to = 127 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\TabletPC",
                            name = "PreventHandwritingErrorReports",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. privacy-tabletpc-prevent-handwriting-data-sharing
                new TweakItem
                {
                    id = "privacy-tabletpc-prevent-handwriting-data-sharing",
                    name_ru = "Запретить передачу данных рукописного ввода в Microsoft",
                    description_ru = "Блокирует фоновую передачу образцов рукописного текста и данных стилуса для обучения алгоритмов.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 125, line_to = 127 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\TabletPC",
                            name = "PreventHandwritingDataSharing",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. privacy-ie-disable-customer-improvement-program
                new TweakItem
                {
                    id = "privacy-ie-disable-customer-improvement-program",
                    name_ru = "Отключить программу улучшения качества ПО (CEIP) в браузере",
                    description_ru = "Отключает сбор данных об использовании и отправку телеметрии качества в компонентах Internet Explorer.",
                    group = "Телеметрия",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 133, line_to = 135 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Internet Explorer\SQM",
                            name = "DisableCustomerImprovementProgram",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. privacy-wdi-enabled-scenario-execution-level
                new TweakItem
                {
                    id = "privacy-wdi-enabled-scenario-execution-level",
                    name_ru = "Отключить уровень выполнения сценариев диагностики WDI",
                    description_ru = "Отключает автоматическое выполнение фоновых сценариев инфраструктуры диагностики Windows (WDI).",
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
                            path = @"SOFTWARE\Policies\Microsoft\Windows\WDI",
                            name = "EnabledScenarioExecutionLevel",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. privacy-startmenu-hide-recommended-section
                new TweakItem
                {
                    id = "privacy-startmenu-hide-recommended-section",
                    name_ru = "Скрыть раздел «Рекомендуем» в меню Пуск Windows 11",
                    description_ru = "Удаляет блок рекомендуемых приложений, недавних файлов и ссылок из меню Пуск.",
                    group = "Меню Пуск",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 22000,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 450, line_to = 452 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "HideRecommendedSection",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. privacy-startmenu-hide-recommended-personalized-sites
                new TweakItem
                {
                    id = "privacy-startmenu-hide-recommended-personalized-sites",
                    name_ru = "Отключить показ рекомендованных сайтов в меню Пуск",
                    description_ru = "Предотвращает появление персонализированных рекомендаций веб-сайтов в блоке рекомендаций меню Пуск.",
                    group = "Меню Пуск",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 22000,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 451, line_to = 453 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "HideRecommendedPersonalizedSites",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. privacy-remoteassistance-allow-full-control
                new TweakItem
                {
                    id = "privacy-remoteassistance-allow-full-control",
                    name_ru = "Запретить полный удаленный контроль при запросе удаленного помощника",
                    description_ru = "Ограничивает сеансы запрашиваемой удаленной помощи только просмотром экрана, запрещая перехват мыши и клавиатуры.",
                    group = "Удаленный доступ",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1030, line_to = 1032 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\policies\Microsoft\Windows NT\Terminal Services",
                            name = "fAllowFullControl",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. privacy-remoteassistance-allow-unsolicited-full-control
                new TweakItem
                {
                    id = "privacy-remoteassistance-allow-unsolicited-full-control",
                    name_ru = "Запретить несанкционированное удаленное управление в помощнике",
                    description_ru = "Блокирует возможность удаленного управления компьютером без явного локального запроса со стороны пользователя.",
                    group = "Удаленный доступ",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 1031, line_to = 1033 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\policies\Microsoft\Windows NT\Terminal Services",
                            name = "fAllowUnsolicitedFullControl",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. security-credui-disable-password-reveal
                new TweakItem
                {
                    id = "security-credui-disable-password-reveal",
                    name_ru = "Скрыть кнопку отображения пароля (глазок) в окнах входа",
                    description_ru = "Удаляет кнопку с глазом для просмотра введенного пароля в открытом виде в системных диалогах аутентификации.",
                    group = "Безопасность входа",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1163, line_to = 1165 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CredUI",
                            name = "DisablePasswordReveal",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. security-removable-storage-deny-write
                new TweakItem
                {
                    id = "security-removable-storage-deny-write",
                    name_ru = "Запретить запись на съемные диски и флеш-накопители",
                    description_ru = "Переводит подключенные съемные накопители в режим только для чтения для защиты от утечки данных.",
                    group = "Контроль устройств",
                    category = "security",
                    type = "toggle",
                    risk = "medium",
                    presets = new List<string>(),
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1175, line_to = 1177 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\RemovableStorageDevices\{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}",
                            name = "Deny_Write",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. security-powershell-enable-module-logging
                new TweakItem
                {
                    id = "security-powershell-enable-module-logging",
                    name_ru = "Включить аудит и журнал загрузки модулей PowerShell",
                    description_ru = "Активирует запись событий выполнения конвейера и загрузки модулей PowerShell в журнал событий безопасности.",
                    group = "PowerShell",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 670, line_to = 672 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging",
                            name = "EnableModuleLogging",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. security-powershell-enable-transcripting
                new TweakItem
                {
                    id = "security-powershell-enable-transcripting",
                    name_ru = "Включить транскрибирование и протоколирование сеансов PowerShell",
                    description_ru = "Автоматически записывает введенные команды и результаты вывода всех сеансов PowerShell в текстовые протоколы.",
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
                            name = "EnableTranscripting",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. security-powershell-enable-scriptblock-logging
                new TweakItem
                {
                    id = "security-powershell-enable-scriptblock-logging",
                    name_ru = "Включить глубокое логирование блоков скриптов PowerShell",
                    description_ru = "Включает детальное протоколирование содержимого всех выполняемых блоков сценариев PowerShell для выявления вредоносного кода.",
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
                            name = "EnableScriptBlockLogging",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. system-explorer-disable-notification-center
                new TweakItem
                {
                    id = "system-explorer-disable-notification-center",
                    name_ru = "Отключить центр уведомлений и всплывающие оповещения",
                    description_ru = "Полностью скрывает значок центра уведомлений на панели задач и подавляет системные всплывающие тосты.",
                    group = "Проводник",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 8405, line_to = 8407 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "DisableNotificationCenter",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. system-startmenu-no-mfu-programs-list
                new TweakItem
                {
                    id = "system-startmenu-no-mfu-programs-list",
                    name_ru = "Отключить отслеживание списка часто используемых программ",
                    description_ru = "Запрещает Windows вести историю и отображать список часто запускаемых программ в меню Пуск.",
                    group = "Меню Пуск",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9161, line_to = 9163 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "NoStartMenuMFUprogramsList",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. system-startmenu-hide-recently-added-apps
                new TweakItem
                {
                    id = "system-startmenu-hide-recently-added-apps",
                    name_ru = "Скрыть список недавно добавленных программ в меню Пуск",
                    description_ru = "Удаляет блок «Недавно добавленные» из списка приложений меню Пуск.",
                    group = "Меню Пуск",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9163, line_to = 9165 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "HideRecentlyAddedApps",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. visibility-taskbar-hide-taskview-button
                new TweakItem
                {
                    id = "visibility-taskbar-hide-taskview-button",
                    name_ru = "Скрыть кнопку «Представление задач» на панели задач",
                    description_ru = "Удаляет кнопку переключения виртуальных рабочих столов и Task View с панели задач Windows.",
                    group = "Панель задач",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1300, line_to = 1302 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "HideTaskViewButton",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. visibility-multitasking-alt-tab-filter
                new TweakItem
                {
                    id = "visibility-multitasking-alt-tab-filter",
                    name_ru = "Отключить отображение вкладок браузера в окне переключения Alt+Tab",
                    description_ru = "Настраивает переключатель Alt+Tab на отображение исключительно открытых окон приложений, исключая вкладки Edge.",
                    group = "Многозадачность",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1646, line_to = 1648 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "MultiTaskingAltTabFilter",
                            type = "REG_DWORD",
                            apply = 3,
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
            Console.WriteLine("Успешно добавлено 25 твиков Блока 26 в data/tweaks.json!");
        }

        static int to_line(int line) { return line; }
    }
}
