using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock18Tool
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
                // 1. security-disable-system-restore-config
                new TweakItem
                {
                    id = "security-disable-system-restore-config",
                    name_ru = "Запретить настройку восстановления системы",
                    description_ru = "Блокирует изменение параметров конфигурации и отключение защиты системы в апплете восстановления.",
                    group = "Защита системы",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1111, line_to = 1115 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows NT\SystemRestore",
                            name = "DisableConfig",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. security-block-domain-picture-password
                new TweakItem
                {
                    id = "security-block-domain-picture-password",
                    name_ru = "Запретить графический пароль для доменных учетных записей",
                    description_ru = "Отключает возможность создания и входа с помощью графического пароля для защиты учетных записей.",
                    group = "Учетные записи",
                    category = "security",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "security/desc.md", line_from = 1161, line_to = 1165 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "BlockDomainPicturePassword",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. peripheral-no-autorun
                new TweakItem
                {
                    id = "peripheral-no-autorun",
                    name_ru = "Отключить автозапуск AutoRun",
                    description_ru = "Отключает автоматическое выполнение сценариев autorun.inf со съемных носителей и дисков.",
                    group = "Автозапуск",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1389, line_to = 1395 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "NoAutorun",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. peripheral-dont-set-autoplay-checkbox
                new TweakItem
                {
                    id = "peripheral-dont-set-autoplay-checkbox",
                    name_ru = "Запретить автозапуску сохранять выбор пользователя",
                    description_ru = "Предотвращает автоматическое запоминание действия по умолчанию для подключаемых внешних устройств.",
                    group = "Автозапуск",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1389, line_to = 1395 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Explorer",
                            name = "DontSetAutoplayCheckbox",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. peripheral-wisp-panning-disabled
                new TweakItem
                {
                    id = "peripheral-wisp-panning-disabled",
                    name_ru = "Отключить сенсорное панорамирование Wisp",
                    description_ru = "Отключает подсистему кинетического панорамирования Wisp Touch для устранения лишних задержек ввода.",
                    group = "Сенсорный ввод",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1413, line_to = 1427 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Wisp\Touch",
                            name = "PanningDisabled",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 0 }
                        }
                    }
                },

                // 6. peripheral-wisp-touch-gate
                new TweakItem
                {
                    id = "peripheral-wisp-touch-gate",
                    name_ru = "Отключить фильтрацию сенсорного шлюза Wisp",
                    description_ru = "Отключает фильтрацию TouchGate для устранения буферизации и ускорения отклика сенсорного ввода.",
                    group = "Сенсорный ввод",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1413, line_to = 1427 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Wisp\Touch",
                            name = "TouchGate",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 7. peripheral-pen-flick-mode
                new TweakItem
                {
                    id = "peripheral-pen-flick-mode",
                    name_ru = "Отключить жесты росчерка пера",
                    description_ru = "Отключает обработчик быстрых росчерков стилуса FlickMode для устранения задержек при рисовании.",
                    group = "Стилус и перо",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1449, line_to = 1465 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Wisp\Pen\SysEventParameters",
                            name = "FlickMode",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "set", value = 1 }
                        }
                    }
                },

                // 8. peripheral-pen-sample-time
                new TweakItem
                {
                    id = "peripheral-pen-sample-time",
                    name_ru = "Минимальное время выборки событий пера",
                    description_ru = "Уменьшает время выборки событий пера SampleTime до 1 мс для мгновенного отклика стилуса.",
                    group = "Стилус и перо",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 1449, line_to = 1465 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Wisp\Pen\SysEventParameters",
                            name = "SampleTime",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "set", value = 8 }
                        }
                    }
                },

                // 9. power-hiberboot-enabled
                new TweakItem
                {
                    id = "power-hiberboot-enabled",
                    name_ru = "Отключить требование быстрой загрузки Hiberboot",
                    description_ru = "Запрещает принудительное включение гибридной быстрой загрузки для стабильности системных драйверов.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 2370, line_to = 2373 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "HiberbootEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. power-hide-lock-option
                new TweakItem
                {
                    id = "power-hide-lock-option",
                    name_ru = "Скрыть пункт блокировки в меню пользователя",
                    description_ru = "Убирает пункт «Заблокировать» из всплывающего меню аватара текущего пользователя.",
                    group = "Меню питания",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 2396, line_to = 2401 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Explorer",
                            name = "ShowLockOption",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. power-user-battery-discharge-estimator
                new TweakItem
                {
                    id = "power-user-battery-discharge-estimator",
                    name_ru = "Отключить оценку скорости разряда аккумулятора",
                    description_ru = "Отключает фоновую телеметрию расхода батареи PopBatteryWorker в ядре операционной системы.",
                    group = "Энергосбережение",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 2406, line_to = 2411 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"System\CurrentControlSet\Control\Power",
                            name = "UserBatteryDischargeEstimator",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. visibility-prevent-color-appearance-change
                new TweakItem
                {
                    id = "visibility-prevent-color-appearance-change",
                    name_ru = "Запретить изменение цвета и оформления",
                    description_ru = "Блокирует случайную смену цветов оформления и контрастных тем в параметрах Windows.",
                    group = "Интерфейс",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1359, line_to = 1368 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                            name = "NoDispAppearancePage",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. visibility-prevent-changing-wallpaper
                new TweakItem
                {
                    id = "visibility-prevent-changing-wallpaper",
                    name_ru = "Запретить изменение фона рабочего стола",
                    description_ru = "Запрещает изменение пользователями или сторонними программами обоев рабочего стола.",
                    group = "Рабочий стол",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1359, line_to = 1368 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop",
                            name = "NoChangingWallPaper",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. visibility-prevent-changing-theme
                new TweakItem
                {
                    id = "visibility-prevent-changing-theme",
                    name_ru = "Запретить смену темы оформления",
                    description_ru = "Отключает смену сохраненных тем персонализации Windows для сохранения стабильного вида.",
                    group = "Интерфейс",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1359, line_to = 1368 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "NoThemesTab",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. visibility-use-default-tile
                new TweakItem
                {
                    id = "visibility-use-default-tile",
                    name_ru = "Использовать стандартный аватар для всех пользователей",
                    description_ru = "Устанавливает единый стандартный значок учетной записи для экрана входа и меню «Пуск».",
                    group = "Учетные записи",
                    category = "visibility",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "visibility/desc.md", line_from = 1385, line_to = 1388 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                            name = "UseDefaultTile",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. system-allow-automatic-app-archiving
                new TweakItem
                {
                    id = "system-allow-automatic-app-archiving",
                    name_ru = "Отключить автоматическую архивацию неиспользуемых приложений",
                    description_ru = "Отключает периодическую выгрузку и удаление двоичных файлов редко используемых приложений.",
                    group = "Приложения",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9456, line_to = 9459 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\Appx",
                            name = "AllowAutomaticAppArchiving",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. system-disable-search-ui
                new TweakItem
                {
                    id = "system-disable-search-ui",
                    name_ru = "Полное отключение интерфейса поиска Windows",
                    description_ru = "Отключает всплывающее графическое окно поиска Search UI для разгрузки ОЗУ и проводника.",
                    group = "Служба поиска",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 9159, line_to = 9161 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                            name = "DisableSearch",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. privacy-disable-cloud-optimized-content
                new TweakItem
                {
                    id = "privacy-disable-cloud-optimized-content",
                    name_ru = "Отключить оптимизированный облачный контент",
                    description_ru = "Отключает загрузку и отображение облачного рекомендательного контента в интерфейсе Windows.",
                    group = "Облачный контент",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 445, line_to = 447 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableCloudOptimizedContent",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. privacy-disable-consumer-account-state
                new TweakItem
                {
                    id = "privacy-disable-consumer-account-state",
                    name_ru = "Отключить контент состояния учетной записи потребителя",
                    description_ru = "Блокирует отслеживание состояния аккаунта и связанные с ним подсказки в меню системы.",
                    group = "Облачный контент",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 446, line_to = 448 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableConsumerAccountStateContent",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. privacy-disable-third-party-suggestions
                new TweakItem
                {
                    id = "privacy-disable-third-party-suggestions",
                    name_ru = "Запретить предложения стороннего контента в Windows Spotlight",
                    description_ru = "Блокирует рекомендации сторонних приложений и промо-материалов в компоненте Windows Spotlight.",
                    group = "Облачный контент",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 448, line_to = 450 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKCU",
                            path = @"Software\Policies\Microsoft\Windows\CloudContent",
                            name = "DisableThirdPartySuggestions",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. privacy-disable-windows-ink-workspace
                new TweakItem
                {
                    id = "privacy-disable-windows-ink-workspace",
                    name_ru = "Отключить рабочую область Windows Ink Workspace",
                    description_ru = "Отключает запуск и фоновые процессы панели рисования и заметок Windows Ink Workspace.",
                    group = "Рукописный ввод",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 126, line_to = 128 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\WindowsInkWorkspace",
                            name = "AllowWindowsInkWorkspace",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. privacy-disable-suggested-apps-ink-workspace
                new TweakItem
                {
                    id = "privacy-disable-suggested-apps-ink-workspace",
                    name_ru = "Отключить рекомендации приложений в Windows Ink Workspace",
                    description_ru = "Блокирует загрузку списка рекомендуемых приложений магазина в рабочей области Windows Ink.",
                    group = "Рукописный ввод",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 127, line_to = 129 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\WindowsInkWorkspace",
                            name = "AllowSuggestedAppsInWindowsInkWorkspace",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. privacy-disallow-input-personalization
                new TweakItem
                {
                    id = "privacy-disallow-input-personalization",
                    name_ru = "Запретить онлайн-распознавание речи и персонализацию ввода",
                    description_ru = "Блокирует отправку шаблонов рукописного ввода и образцов голоса на серверы корпорации Майкрософт.",
                    group = "Персонализация ввода",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 128, line_to = 130 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\InputPersonalization",
                            name = "AllowInputPersonalization",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. privacy-disallow-speech-model-update
                new TweakItem
                {
                    id = "privacy-disallow-speech-model-update",
                    name_ru = "Запретить автоматическое обновление голосовых моделей",
                    description_ru = "Отключает автоматическую фоновую загрузку речевых моделей и языковых пакетов распознавания.",
                    group = "Голосовые службы",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 129, line_to = 131 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Speech",
                            name = "AllowSpeechModelUpdate",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. privacy-turn-off-rsop-logging
                new TweakItem
                {
                    id = "privacy-turn-off-rsop-logging",
                    name_ru = "Отключить ведение журнала результирующей политики RSoP",
                    description_ru = "Отключает создание подробных журналов RSoP при каждом обновлении групповых политик Windows.",
                    group = "Диагностика",
                    category = "privacy",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "privacy/desc.md", line_from = 134, line_to = 136 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Policies\Microsoft\Windows\System",
                            name = "RSoPLogging",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            list.AddRange(newTweaks);
            Console.WriteLine("Новое число твиков: " + list.Count);

            string updated = serializer.Serialize(list);

            // Форматирование JSON
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
