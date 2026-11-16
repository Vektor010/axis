using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock30Tool
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
                // 1. peripheral-usb20-hardware-lpm-override
                new TweakItem
                {
                    id = "peripheral-usb20-hardware-lpm-override",
                    name_ru = "Отключить аппаратное управление питанием USB 2.0 (LPM Override)",
                    description_ru = "Запрещает аппаратное переключение шины USB 2.0 в режимы энергосбережения LPM для исключения задержек пробуждения контроллера.",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 718, line_to = 722 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\usb\Usb20HardwareLpm",
                            name = "Usb20HardwareLpmOverride",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2. peripheral-usb20-hardware-lpm-timeout
                new TweakItem
                {
                    id = "peripheral-usb20-hardware-lpm-timeout",
                    name_ru = "Сбросить тайм-аут перехода шины USB 2.0 в режим сна LPM",
                    description_ru = "Устанавливает нулевой тайм-аут ожидания аппаратного LPM на концентраторах USB 2.0.",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 718, line_to = 722 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\usb\Usb20HardwareLpm",
                            name = "Usb20HardwareLpmTimeout",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3. peripheral-usb-recovery-power-drain
                new TweakItem
                {
                    id = "peripheral-usb-recovery-power-drain",
                    name_ru = "Запретить принудительное отключение USB при выключении дисплея",
                    description_ru = "Предотвращает автоматическую остановку USB устройств при отключении экрана на портативных ПК (AttemptRecoveryFromUsbPowerDrain).",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 723, line_to = 726 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\usb\AutomaticSurpriseRemoval",
                            name = "AttemptRecoveryFromUsbPowerDrain",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4. peripheral-usb-ltm-enable
                new TweakItem
                {
                    id = "peripheral-usb-ltm-enable",
                    name_ru = "Отключить передачу сообщений о допустимой задержке USB (LTM)",
                    description_ru = "Отключает протокол Latency Tolerance Messaging (LTM) для USB шины, снижая транзакционные задержки опроса.",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 733, line_to = 736 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\usb\UsbLtm",
                            name = "UsbLtmEnable",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5. peripheral-usb-disable-cc-detect
                new TweakItem
                {
                    id = "peripheral-usb-disable-cc-detect",
                    name_ru = "Отключить периодическое обнаружение Type-C линий конфигурации (CcDetect)",
                    description_ru = "Предотвращает постоянный циклический опрос служебных линий CC на Type-C портах в стеке usbport.sys.",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 743, line_to = 754 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Services\usb",
                            name = "DisableCcDetect",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6. peripheral-usb-force-hc-d3-no-wake-arm
                new TweakItem
                {
                    id = "peripheral-usb-force-hc-d3-no-wake-arm",
                    name_ru = "Запретить перевод USB хост-контроллера в режим сна D3",
                    description_ru = "Исключает подготовку хост-контроллеров USB к глубокому сну D3 для мгновенной доступности всех портов.",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 743, line_to = 754 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Services\usb",
                            name = "ForceHcD3NoWakeArm",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7. peripheral-usb-enable-dca
                new TweakItem
                {
                    id = "peripheral-usb-enable-dca",
                    name_ru = "Включить прямой доступ к контроллеру USB (Direct Controller Access DCA)",
                    description_ru = "Активирует режим DCA в стеке драйвера USB для прямого взаимодействия с аппаратным контроллером.",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 743, line_to = 754 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Services\usb",
                            name = "EnableDCA",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8. peripheral-usb-force-ports-high-speed
                new TweakItem
                {
                    id = "peripheral-usb-force-ports-high-speed",
                    name_ru = "Принудительно удерживать порты USB 2.0 в режиме High-Speed",
                    description_ru = "Предотвращает ошибочное падение скорости подключенных устройств USB 2.0 до Full-Speed в EHCI стеке.",
                    group = "USB контроллеры",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 743, line_to = 754 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Services\usb",
                            name = "ForcePortsHighSpeed",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9. system-kernel-always-track-io-boosting
                new TweakItem
                {
                    id = "system-kernel-always-track-io-boosting",
                    name_ru = "Отключить принудительное отслеживание ускорения ввода-вывода (IO Boosting)",
                    description_ru = "Отключает постоянный трекинг IO-буста в PsBoostThreadIoEx для минимизации накладных расходов планировщика.",
                    group = "Планировщик ядра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3344, line_to = 3348 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel",
                            name = "AlwaysTrackIoBoosting",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10. system-kernel-disable-lightweight-suspend
                new TweakItem
                {
                    id = "system-kernel-disable-lightweight-suspend",
                    name_ru = "Отключить механизм легковесной приостановки потоков ядра",
                    description_ru = "Блокирует ветку легковесной приостановки в KiSuspendThread, направляя вызовы через стандартный путь APC.",
                    group = "Планировщик ядра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3344, line_to = 3363 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel",
                            name = "DisableLightWeightSuspend",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11. system-kernel-disable-low-qos-timer-resolution
                new TweakItem
                {
                    id = "system-kernel-disable-low-qos-timer-resolution",
                    name_ru = "Запретить деградацию разрешения таймера для низкоприоритетных процессов (Low QoS)",
                    description_ru = "Исключает принудительное загрубление точности системного таймера для фоновых процессов ядра (KeDisableLowQosTimerResolution).",
                    group = "Системные таймеры",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3344, line_to = 3364 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel",
                            name = "DisableLowQosTimerResolution",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12. system-kernel-global-timer-resolution-requests
                new TweakItem
                {
                    id = "system-kernel-global-timer-resolution-requests",
                    name_ru = "Отключить глобальные межпроцессные запросы разрешения таймера",
                    description_ru = "Ограничивает влияние сторонних приложений на глобальное переключение частоты таймера ядра.",
                    group = "Системные таймеры",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3344, line_to = 3395 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel",
                            name = "GlobalTimerResolutionRequests",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13. system-kernel-thread-dpc-enable
                new TweakItem
                {
                    id = "system-kernel-thread-dpc-enable",
                    name_ru = "Включить обработку DPC в контексте специализированных потоков (Threaded DPC)",
                    description_ru = "Активирует выполнение DPC процедур в специализированных системных потоках для предотвращения блокировки аппаратных прерываний.",
                    group = "Прерывания и DPC",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3344, line_to = 3448 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel",
                            name = "ThreadDpcEnable",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14. system-kernel-timer-check-flags
                new TweakItem
                {
                    id = "system-kernel-timer-check-flags",
                    name_ru = "Отключить отладочные проверки очереди таймеров ядра (KeTimerCheckFlags)",
                    description_ru = "Сбрасывает отладочные флаги валидации таймеров ядра, устраняя лишние проверки при обработке тиков.",
                    group = "Системные таймеры",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3344, line_to = 3450 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel",
                            name = "TimerCheckFlags",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15. system-memory-cache-unmap-behind-mb
                new TweakItem
                {
                    id = "system-memory-cache-unmap-behind-mb",
                    name_ru = "Уменьшить буфер упреждающего размонтирования кэша файловой системы",
                    description_ru = "Устанавливает минимальный буфер Unmap-Behind в 1 МБ для мгновенного освобождения неиспользуемых страниц памяти.",
                    group = "Управление памятью",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3534, line_to = 3540 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                            name = "CacheUnmapBehindLengthInMB",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16. system-memory-enable-async-lazywrite
                new TweakItem
                {
                    id = "system-memory-enable-async-lazywrite",
                    name_ru = "Включить асинхронную отложенную запись кэша (Async Lazy Write)",
                    description_ru = "Активирует асинхронный сброс модифицированных страниц кэша на диск в диспетчере кэша (CcEnableAsyncLazywriteOverride).",
                    group = "Управление памятью",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3534, line_to = 3550 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                            name = "EnableAsyncLazywrite",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17. system-memory-enable-async-lazywrite-multi
                new TweakItem
                {
                    id = "system-memory-enable-async-lazywrite-multi",
                    name_ru = "Включить многопоточную асинхронную отложенную запись кэша",
                    description_ru = "Разрешает параллельный сброс модифицированных страниц несколькими потоками Lazy Writer для исключения задержек дискового ввода-вывода.",
                    group = "Управление памятью",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3534, line_to = 3550 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                            name = "EnableAsyncLazywriteMulti",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18. system-memory-enable-per-volume-lazywriter
                new TweakItem
                {
                    id = "system-memory-enable-per-volume-lazywriter",
                    name_ru = "Включить раздельные потоки отложенной записи для каждого тома диска",
                    description_ru = "Активирует независимые потоки сброса кэша на уровне каждого тома файловой системы (CcEnablePerVolumeLazyWriterOverride).",
                    group = "Управление памятью",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3534, line_to = 3550 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                            name = "EnablePerVolumeLazyWriter",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19. system-executive-force-mutant-autoboost
                new TweakItem
                {
                    id = "system-executive-force-mutant-autoboost",
                    name_ru = "Включить автоматическое повышение приоритета при захвате мьютексов (Mutant Autoboost)",
                    description_ru = "Принудительно активирует механизм автоматического буста приоритета потоков-владельцев мьютексов ядра для предотвращения инверсии приоритетов.",
                    group = "Исполнительная подсистема ядра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3631, line_to = 3638 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Executive",
                            name = "ForceEnableMutantAutoboost",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20. system-executive-max-kernel-worker-threads
                new TweakItem
                {
                    id = "system-executive-max-kernel-worker-threads",
                    name_ru = "Оптимизировать максимальный пул системных рабочих потоков ядра",
                    description_ru = "Устанавливает оптимальный лимит пула рабочих потоков исполнительной подсистемы ядра (ExpMaximumKernelWorkerThreads = 2048).",
                    group = "Исполнительная подсистема ядра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3631, line_to = 3640 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Executive",
                            name = "MaximumKernelWorkerThreads",
                            type = "REG_DWORD",
                            apply = 2048,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21. system-executive-worker-creation-timeout
                new TweakItem
                {
                    id = "system-executive-worker-creation-timeout",
                    name_ru = "Уменьшить тайм-аут создания рабочих потоков ядра при росте очереди",
                    description_ru = "Сокращает время реакции фабрики рабочих потоков ядра на накопление задач с 10 до 5 секунд.",
                    group = "Исполнительная подсистема ядра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3631, line_to = 3642 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Executive",
                            name = "WorkerFactoryThreadCreationTimeout",
                            type = "REG_DWORD",
                            apply = 5,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22. system-executive-worker-idle-timeout
                new TweakItem
                {
                    id = "system-executive-worker-idle-timeout",
                    name_ru = "Уменьшить время ожидания простоя неиспользуемых рабочих потоков ядра",
                    description_ru = "Сокращает время удержания в памяти простаивающих рабочих потоков фабрики с 67 до 30 секунд.",
                    group = "Исполнительная подсистема ядра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3631, line_to = 3642 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Executive",
                            name = "WorkerFactoryThreadIdleTimeout",
                            type = "REG_DWORD",
                            apply = 30,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23. system-config-freeze-thaw-timeout
                new TweakItem
                {
                    id = "system-config-freeze-thaw-timeout",
                    name_ru = "Сократить время заморозки реестра при фиксации изменений (Freeze-Thaw)",
                    description_ru = "Уменьшает максимальный интервал блокировки кустов реестра Configuration Manager при сохранении транзакций с 60 до 30 секунд.",
                    group = "Диспетчер конфигурации реестра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3709, line_to = 3721 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Configuration Manager",
                            name = "FreezeThawTimeoutInSeconds",
                            type = "REG_DWORD",
                            apply = 30,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24. system-config-lazy-flush-boot-delay
                new TweakItem
                {
                    id = "system-config-lazy-flush-boot-delay",
                    name_ru = "Уменьшить задержку запуска отложенного сброса реестра при загрузке",
                    description_ru = "Сокращает начальную задержку старта фонового сброса буферов реестра при загрузке системы с 60 до 15 секунд.",
                    group = "Диспетчер конфигурации реестра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3709, line_to = 3721 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Configuration Manager",
                            name = "RegistryLazyFlushBootDelay",
                            type = "REG_DWORD",
                            apply = 15,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25. system-config-lazy-flush-interval
                new TweakItem
                {
                    id = "system-config-lazy-flush-interval",
                    name_ru = "Оптимизировать периодичность сброса изменений реестра на диск",
                    description_ru = "Устанавливает оптимальный регулярный интервал сброса модифицированных страниц реестра Configuration Manager (30 секунд).",
                    group = "Диспетчер конфигурации реестра",
                    category = "system",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3709, line_to = 3721 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Configuration Manager",
                            name = "RegistryLazyFlushInterval",
                            type = "REG_DWORD",
                            apply = 30,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            list.AddRange(newTweaks);
            Console.WriteLine("Новое число твиков: " + list.Count);

            string updatedJson = serializer.Serialize(list);

            // Pretty format JSON for clean git diffs
            StringBuilder formatted = new StringBuilder();
            int indent = 0;
            bool inQuotes = false;
            for (int i = 0; i < updatedJson.Length; i++)
            {
                char ch = updatedJson[i];
                if (ch == '\"' && (i == 0 || updatedJson[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                    formatted.Append(ch);
                }
                else if (!inQuotes)
                {
                    if (ch == '{' || ch == '[')
                    {
                        formatted.Append(ch);
                        formatted.AppendLine();
                        indent += 2;
                        formatted.Append(new string(' ', indent));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        formatted.AppendLine();
                        indent -= 2;
                        formatted.Append(new string(' ', indent));
                        formatted.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        formatted.Append(ch);
                        formatted.AppendLine();
                        formatted.Append(new string(' ', indent));
                    }
                    else if (ch == ':')
                    {
                        formatted.Append(": ");
                    }
                    else if (!char.IsWhiteSpace(ch))
                    {
                        formatted.Append(ch);
                    }
                }
                else
                {
                    formatted.Append(ch);
                }
            }

            File.WriteAllText(jsonPath, formatted.ToString(), Encoding.UTF8);
            Console.WriteLine("[SUCCESS] tweaks.json успешно обновлен до " + list.Count + " твиков!");
        }
    }
}
