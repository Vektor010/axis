using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock31
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

            Console.WriteLine("Текущее число твиков в базе: " + list.Count);

            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in list) existingIds.Add(t.id);

            var newTweaks = new List<TweakItem>
            {
                // 1. system-dwm-mousewheel-animation-duration
                new TweakItem {
                    id = "system-dwm-mousewheel-animation-duration",
                    name_ru = "Отключить анимацию прокрутки колесика мыши DWM (MousewheelAnimationDurationMs)",
                    description_ru = "Сбрасывает длительность анимации скроллинга DWM с 250 мс до 0 для мгновенной прокрутки в современных приложениях.",
                    group = "Диспетчер окон (DWM)", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 2755, line_to = 2765 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "MousewheelAnimationDurationMs", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 2. system-dwm-telemetry-frames-report-period
                new TweakItem {
                    id = "system-dwm-telemetry-frames-report-period",
                    name_ru = "Отключить периодический отчет телеметрии кадров DWM (TelemetryFramesReportPeriodMilliseconds)",
                    description_ru = "Отключает фоновый 5-минутный отчет статистики рендеринга кадров DWM, снижая накладные расходы планировщика.",
                    group = "Диспетчер окон (DWM)", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 2765, line_to = 2775 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "TelemetryFramesReportPeriodMilliseconds", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 3. system-dwm-telemetry-sequence-idle-interval
                new TweakItem {
                    id = "system-dwm-telemetry-sequence-idle-interval",
                    name_ru = "Отключить интервал телеметрии простоя последовательности кадров DWM",
                    description_ru = "Сбрасывает интервал фиксации простоя кадров DWM (TelemetryFramesSequenceIdleIntervalMilliseconds) в ноль.",
                    group = "Диспетчер окон (DWM)", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 2765, line_to = 2775 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "TelemetryFramesSequenceIdleIntervalMilliseconds", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 4. system-dwm-backdrop-blur-caching-throttle
                new TweakItem {
                    id = "system-dwm-backdrop-blur-caching-throttle",
                    name_ru = "Снять ограничение троттлинга кэширования размытия фона DWM",
                    description_ru = "Сбрасывает 25-мс задержку обновления кэша эффектов размытия фона в dwmcore для мгновенной перерисовки интерфейса.",
                    group = "Диспетчер окон (DWM)", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 2772, line_to = 2780 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "BackdropBlurCachingThrottleMs", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 5. system-dwm-disable-locking-memory
                new TweakItem {
                    id = "system-dwm-disable-locking-memory",
                    name_ru = "Запретить блокировку рабочего набора памяти DWM (DisableLockingMemory)",
                    description_ru = "Активирует флаг предпочтений CSettingsManager в DWM, запрещая принудительное закрепление страниц в RAM.",
                    group = "Диспетчер окон (DWM)", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 2790, line_to = 2800 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "DisableLockingMemory", type = "REG_DWORD", apply = 1, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 6. system-dwm-telemetry-sequence-max-period
                new TweakItem {
                    id = "system-dwm-telemetry-sequence-max-period",
                    name_ru = "Отключить максимальный интервал телеметрии кадров DWM",
                    description_ru = "Сбрасывает тайм-аут сбора последовательностей кадров DWM (TelemetryFramesSequenceMaximumPeriodMilliseconds) в ноль.",
                    group = "Диспетчер окон (DWM)", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 2765, line_to = 2775 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "TelemetryFramesSequenceMaximumPeriodMilliseconds", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 7. network-smb-disable-bandwidth-throttling
                new TweakItem {
                    id = "network-smb-disable-bandwidth-throttling",
                    name_ru = "Отключить троттлинг пропускной способности SMB (DisableBandwidthThrottling)",
                    description_ru = "Снимает системное ограничение скорости передачи файлов по протоколу SMB для максимальной утилизации локальной сети.",
                    group = "Протокол SMB", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = true,
                    source = new SourceObj { file = "network/desc.md", line_from = 570, line_to = 577 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters", name = "DisableBandwidthThrottling", type = "REG_DWORD", apply = 1, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 8. network-smb-disable-large-mtu
                new TweakItem {
                    id = "network-smb-disable-large-mtu",
                    name_ru = "Разрешить передачу больших пакетов SMB (DisableLargeMtu)",
                    description_ru = "Включает Large MTU в клиенте LanmanWorkstation для ускорения копирования объемных файлов в гигабитных сетях.",
                    group = "Протокол SMB", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = true,
                    source = new SourceObj { file = "network/desc.md", line_from = 578, line_to = 586 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters", name = "DisableLargeMtu", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 9. network-smb-disable-multichannel
                new TweakItem {
                    id = "network-smb-disable-multichannel",
                    name_ru = "Разрешить многоканальное подключение SMB (DisableMultiChannel)",
                    description_ru = "Активирует SMB Multichannel для параллельной передачи сетевого трафика по нескольким каналам и адаптерам.",
                    group = "Протокол SMB", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = true,
                    source = new SourceObj { file = "network/desc.md", line_from = 587, line_to = 605 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters", name = "DisableMultiChannel", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 10. network-smb-disable-leasing
                new TweakItem {
                    id = "network-smb-disable-leasing",
                    name_ru = "Отключить сетевой лизинг дескрипторов файлов SMB (DisableLeasing)",
                    description_ru = "Устраняет задержки открытия и блокировки общих файлов в LanmanServer при совместной работе в локальной сети.",
                    group = "Протокол SMB", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = true,
                    source = new SourceObj { file = "network/desc.md", line_from = 606, line_to = 614 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanServer\Parameters", name = "DisableLeasing", type = "REG_DWORD", apply = 1, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 11. network-nic-lso-v2-ipv6
                new TweakItem {
                    id = "network-nic-lso-v2-ipv6",
                    name_ru = "Отключить выгрузку сегментации больших пакетов IPv6 (LSOv2)",
                    description_ru = "Запрещает сетевому адаптеру разделять крупные IPv6 пакеты для исключения микрозадержек и джиттера в соревновательных играх.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 300, line_to = 315 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*LsoV2IPv6", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 12. network-nic-uso-ipv4
                new TweakItem {
                    id = "network-nic-uso-ipv4",
                    name_ru = "Отключить аппаратную сегментацию UDP пакетов IPv4 (USO)",
                    description_ru = "Запрещает адаптеру объединять и фрагментировать UDP пакеты IPv4, гарантируя немедленную отправку игровых тиков.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 305, line_to = 320 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*UsoIPv4", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 13. network-nic-uso-ipv6
                new TweakItem {
                    id = "network-nic-uso-ipv6",
                    name_ru = "Отключить аппаратную сегментацию UDP пакетов IPv6 (USO)",
                    description_ru = "Запрещает аппаратную сегментацию UDP датаграмм IPv6 на уровне драйвера сетевой карты.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 305, line_to = 320 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*UsoIPv6", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 14. network-nic-rsc-ipv6
                new TweakItem {
                    id = "network-nic-rsc-ipv6",
                    name_ru = "Отключить коалесценцию входящих сегментов IPv6 (RSC)",
                    description_ru = "Отключает ожидание и склеивание входящих сегментов IPv6 для мгновенной обработки трафика сетевым стеком.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 310, line_to = 320 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*RscIPv6", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 15. network-nic-udp-rsc
                new TweakItem {
                    id = "network-nic-udp-rsc",
                    name_ru = "Отключить коалесценцию входящих UDP пакетов (URO)",
                    description_ru = "Блокирует склеивание входящих UDP датаграмм на поддерживаемых картах в NDIS 6.89 для исключения задержек сетевого ввода.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 310, line_to = 320 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*UdpRsc", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 16. network-nic-pm-arp-offload
                new TweakItem {
                    id = "network-nic-pm-arp-offload",
                    name_ru = "Отключить энергосберегающую выгрузку протокола ARP (*PMARPOffload)",
                    description_ru = "Запрещает перевод сетевой карты в режим сна с автономным ответом на ARP запросы.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 325, line_to = 335 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*PMARPOffload", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 17. network-nic-pm-ns-offload
                new TweakItem {
                    id = "network-nic-pm-ns-offload",
                    name_ru = "Отключить энергосберегающую выгрузку Neighbor Solicitation (*PMNSOffload)",
                    description_ru = "Исключает автономную обработку IPv6 соседских запросов сетевой картой в состоянии сна.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 325, line_to = 335 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*PMNSOffload", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 18. network-nic-pm-wifi-rekey-offload
                new TweakItem {
                    id = "network-nic-pm-wifi-rekey-offload",
                    name_ru = "Отключить энергосберегающий перевыпуск ключей Wi-Fi (*PMWiFiRekeyOffload)",
                    description_ru = "Запрещает адаптеру беспроводной связи выполнять автономный перевыпуск ключей GTK при низком энергопотреблении.",
                    group = "Аппаратная разгрузка адаптера", category = "network", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "network/desc.md", line_from = 325, line_to = 335 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*PMWiFiRekeyOffload", type = "REG_SZ", apply = "0", revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 19. power-directed-drips-override
                new TweakItem {
                    id = "power-directed-drips-override",
                    name_ru = "Отключить принудительный режим глубокого сна компонентов (DirectedDripsOverride)",
                    description_ru = "Предотвращает принудительное погружение периферийных шин в глубокий сон Directed DRIPS.",
                    group = "Управление питанием устройств", category = "power", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1368, line_to = 1375 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", name = "DirectedDripsOverride", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 20. power-check-power-source-after-rtc-wake
                new TweakItem {
                    id = "power-check-power-source-after-rtc-wake",
                    name_ru = "Отключить проверку источника питания после пробуждения по таймеру RTC",
                    description_ru = "Исключает служебную задержку на опрос состояния батареи диспетчером питания после выхода из сна по RTC.",
                    group = "Управление питанием устройств", category = "power", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1360, line_to = 1366 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", name = "CheckPowerSourceAfterRtcWakeTime", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 21. power-directed-drips-debounce-interval
                new TweakItem {
                    id = "power-directed-drips-debounce-interval",
                    name_ru = "Сбросить защитный интервал стабилизации Directed DRIPS",
                    description_ru = "Устанавливает нулевой интервал стабилизации переходов энергосбережения Directed DRIPS Debounce Interval.",
                    group = "Управление питанием устройств", category = "power", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1368, line_to = 1375 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", name = "DirectedDripsDebounceInterval", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 22. peripheral-usb-en-pm-debug
                new TweakItem {
                    id = "peripheral-usb-en-pm-debug",
                    name_ru = "Отключить отладку управления питанием контроллеров USB (EnPMDebug)",
                    description_ru = "Отключает фоновую служебную отладку переходов питания USB в стеке драйвера usbport.sys.",
                    group = "USB контроллеры", category = "peripheral", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 743, line_to = 754 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\usb", name = "EnPMDebug", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 23. peripheral-usb-debug-log-enable
                new TweakItem {
                    id = "peripheral-usb-debug-log-enable",
                    name_ru = "Отключить сбор отладочных логов драйвера USB (debuglogenable)",
                    description_ru = "Сбрасывает флаг записи отладочных логов usbport.sys для устранения накладных расходов стека USB.",
                    group = "USB контроллеры", category = "peripheral", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 743, line_to = 754 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\usb", name = "debuglogenable", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 24. system-kernel-dpc-watchdog-period
                new TweakItem {
                    id = "system-kernel-dpc-watchdog-period",
                    name_ru = "Отключить таймер сторожевого контроля DPC (DpcWatchdogPeriod)",
                    description_ru = "Отключает периодический опрос сторожевого таймера DPC ядра, устраняя паразитные прерывания при непрерывном рендеринге.",
                    group = "Прерывания и DPC", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 3344, line_to = 3390 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel", name = "DpcWatchdogPeriod", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                },
                // 25. system-dwm-animations-shift-key
                new TweakItem {
                    id = "system-dwm-animations-shift-key",
                    name_ru = "Отключить замедленные анимации по клавише Shift в DWM (AnimationsShiftKey)",
                    description_ru = "Отключает скрытую функцию замедления анимации окон при удерживании Shift в DWM CSettingsManager.",
                    group = "Диспетчер окон (DWM)", category = "system", type = "toggle", risk = "low",
                    presets = new List<string> { "safe", "maximum" }, min_build = 19041, needs_review = false,
                    source = new SourceObj { file = "system/desc.md", line_from = 2790, line_to = 2800 },
                    registry = new List<RegItem> {
                        new RegItem { hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "AnimationsShiftKey", type = "REG_DWORD", apply = 0, revert = new RevertObj { mode = "delete" } }
                    }
                }
            };

            int addedCount = 0;
            foreach (var t in newTweaks)
            {
                if (existingIds.Contains(t.id))
                {
                    Console.WriteLine("[SKIP] Уже существует: " + t.id);
                    continue;
                }
                list.Add(t);
                existingIds.Add(t.id);
                addedCount++;
            }

            Console.WriteLine(string.Format("Добавлено новых твиков: {0}. Итого в списке: {1}", addedCount, list.Count));

            // Бэкап перед сохранением
            string backupPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json.bak31";
            File.Copy(jsonPath, backupPath, true);

            // Сохранение
            string formatted = FormatJson(list);
            File.WriteAllText(jsonPath, formatted, new UTF8Encoding(false));
            Console.WriteLine("Файл tweaks.json успешно сохранен!");
        }

        static string FormatJson(object obj)
        {
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            string raw = serializer.Serialize(obj);

            var sb = new StringBuilder();
            int indent = 0;
            bool inQuote = false;

            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c == '\"' && (i == 0 || raw[i - 1] != '\\'))
                {
                    inQuote = !inQuote;
                    sb.Append(c);
                }
                else if (!inQuote)
                {
                    if (c == '{' || c == '[')
                    {
                        sb.Append(c);
                        sb.Append("\r\n");
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (c == '}' || c == ']')
                    {
                        sb.Append("\r\n");
                        indent--;
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(c);
                    }
                    else if (c == ',')
                    {
                        sb.Append(c);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (c == ':')
                    {
                        sb.Append(": ");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
