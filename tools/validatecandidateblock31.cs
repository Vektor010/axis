using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace CandidateValidator
{
    public class Candidate
    {
        public string id { get; set; }
        public string name_ru { get; set; }
        public string description_ru { get; set; }
        public string category { get; set; }
        public string group { get; set; }
        public string hive { get; set; }
        public string path { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public object apply { get; set; }
        public string revMode { get; set; }
        public object revVal { get; set; }
        public string file { get; set; }
        public int from { get; set; }
        public int to { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string sourceRoot = @"C:\Users\User\Documents\win-tweaker\data\source\win-config";
            string tweaksJson = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJson, Encoding.UTF8));

            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var t in list)
            {
                if (t.ContainsKey("id")) existingIds.Add(t["id"] as string);
                var regList = t.ContainsKey("registry") ? t["registry"] as System.Collections.ArrayList : null;
                if (regList != null)
                {
                    foreach (Dictionary<string, object> r in regList)
                    {
                        string path = r.ContainsKey("path") ? r["path"] as string : "";
                        string name = r.ContainsKey("name") ? r["name"] as string : "";
                        existingKeys.Add(path + "\\" + name);
                    }
                }
            }

            var candidates = GetCandidates();
            Console.WriteLine("Проверяем кандидатов Блока 31: " + candidates.Count);

            int errCount = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];
                if (existingIds.Contains(c.id))
                {
                    Console.WriteLine("[ERR] Дубликат id: " + c.id);
                    errCount++;
                }

                string keyComb = c.path + "\\" + c.name;
                if (existingKeys.Contains(keyComb))
                {
                    Console.WriteLine("[ERR] Ключ реестра уже существует: " + keyComb);
                    errCount++;
                }

                string fullFile = Path.Combine(sourceRoot, c.file);
                if (!File.Exists(fullFile))
                {
                    Console.WriteLine(string.Format("[ERR] Файл источника не найден: {0} ({1})", c.file, c.id));
                    errCount++;
                }
                else
                {
                    string[] fileLines = File.ReadAllLines(fullFile);
                    if (c.from <= 0 || c.to > fileLines.Length || c.from > c.to)
                    {
                        Console.WriteLine(string.Format("[ERR] Диапазон строк {0}..{1} вне файла {2} (длина {3}) для {4}",
                            c.from, c.to, c.file, fileLines.Length, c.id));
                        errCount++;
                    }
                    else
                    {
                        bool found = false;
                        for (int lineIdx = c.from - 1; lineIdx < c.to; lineIdx++)
                        {
                            if (fileLines[lineIdx].IndexOf(c.name, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            Console.WriteLine(string.Format("[ERR] Имя параметра '{0}' не найдено в строках {1}..{2} файла {3} для {4}",
                                c.name, c.from, c.to, c.file, c.id));
                            errCount++;
                        }
                    }
                }

                if (c.type == "REG_DWORD" && !(c.apply is int || c.apply is long))
                {
                    Console.WriteLine(string.Format("[ERR] REG_DWORD не число: {0} ({1})", c.apply, c.id));
                    errCount++;
                }
                if (c.type == "REG_SZ" && !(c.apply is string))
                {
                    Console.WriteLine(string.Format("[ERR] REG_SZ не строка: {0} ({1})", c.apply, c.id));
                    errCount++;
                }
            }

            Console.WriteLine(string.Format("\nИтог валидации кандидатов Блока 31: ОШИБОК: {0}", errCount));
        }

        static List<Candidate> GetCandidates()
        {
            return new List<Candidate>
            {
                // 1. system-dwm-mousewheel-animation-duration
                new Candidate {
                    id = "system-dwm-mousewheel-animation-duration",
                    name_ru = "Отключить анимацию прокрутки колесика мыши DWM (MousewheelAnimationDurationMs)",
                    description_ru = "Сбрасывает длительность анимации скроллинга DWM с 250 мс до 0 для мгновенной прокрутки в современных приложениях.",
                    category = "system", group = "Диспетчер окон (DWM)",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "MousewheelAnimationDurationMs", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 2755, to = 2765
                },
                // 2. system-dwm-telemetry-frames-report-period
                new Candidate {
                    id = "system-dwm-telemetry-frames-report-period",
                    name_ru = "Отключить периодический отчет телеметрии кадров DWM (TelemetryFramesReportPeriodMilliseconds)",
                    description_ru = "Отключает фоновый 5-минутный отчет статистики рендеринга кадров DWM, снижая накладные расходы планировщика.",
                    category = "system", group = "Диспетчер окон (DWM)",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "TelemetryFramesReportPeriodMilliseconds", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 2765, to = 2775
                },
                // 3. system-dwm-telemetry-sequence-idle-interval
                new Candidate {
                    id = "system-dwm-telemetry-sequence-idle-interval",
                    name_ru = "Отключить интервал телеметрии простоя последовательности кадров DWM",
                    description_ru = "Сбрасывает интервал фиксации простоя кадров DWM (TelemetryFramesSequenceIdleIntervalMilliseconds) в ноль.",
                    category = "system", group = "Диспетчер окон (DWM)",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "TelemetryFramesSequenceIdleIntervalMilliseconds", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 2765, to = 2775
                },
                // 4. system-dwm-backdrop-blur-caching-throttle
                new Candidate {
                    id = "system-dwm-backdrop-blur-caching-throttle",
                    name_ru = "Снять ограничение троттлинга кэширования размытия фона DWM",
                    description_ru = "Сбрасывает 25-мс задержку обновления кэша эффектов размытия фона в dwmcore для мгновенной перерисовки интерфейса.",
                    category = "system", group = "Диспетчер окон (DWM)",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "BackdropBlurCachingThrottleMs", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 2772, to = 2780
                },
                // 5. system-dwm-disable-locking-memory
                new Candidate {
                    id = "system-dwm-disable-locking-memory",
                    name_ru = "Запретить блокировку рабочего набора памяти DWM (DisableLockingMemory)",
                    description_ru = "Активирует флаг предпочтений CSettingsManager в DWM, запрещая принудительное закрепление страниц в RAM.",
                    category = "system", group = "Диспетчер окон (DWM)",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "DisableLockingMemory", type = "REG_DWORD",
                    apply = 1, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 2790, to = 2800
                },
                // 6. system-dwm-telemetry-sequence-max-period
                new Candidate {
                    id = "system-dwm-telemetry-sequence-max-period",
                    name_ru = "Отключить максимальный интервал телеметрии кадров DWM",
                    description_ru = "Сбрасывает тайм-аут сбора последовательностей кадров DWM (TelemetryFramesSequenceMaximumPeriodMilliseconds) в ноль.",
                    category = "system", group = "Диспетчер окон (DWM)",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "TelemetryFramesSequenceMaximumPeriodMilliseconds", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 2765, to = 2775
                },
                // 7. network-smb-disable-bandwidth-throttling
                new Candidate {
                    id = "network-smb-disable-bandwidth-throttling",
                    name_ru = "Отключить троттлинг пропускной способности SMB (DisableBandwidthThrottling)",
                    description_ru = "Снимает системное ограничение скорости передачи файлов по протоколу SMB для максимальной утилизации локальной сети.",
                    category = "network", group = "Протокол SMB",
                    hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters", name = "DisableBandwidthThrottling", type = "REG_DWORD",
                    apply = 1, revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 570, to = 577
                },
                // 8. network-smb-disable-large-mtu
                new Candidate {
                    id = "network-smb-disable-large-mtu",
                    name_ru = "Разрешить передачу больших пакетов SMB (DisableLargeMtu)",
                    description_ru = "Включает Large MTU в клиенте LanmanWorkstation для ускорения копирования объемных файлов в гигабитных сетях.",
                    category = "network", group = "Протокол SMB",
                    hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters", name = "DisableLargeMtu", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 578, to = 586
                },
                // 9. network-smb-disable-multichannel
                new Candidate {
                    id = "network-smb-disable-multichannel",
                    name_ru = "Разрешить многоканальное подключение SMB (DisableMultiChannel)",
                    description_ru = "Активирует SMB Multichannel для параллельной передачи сетевого трафика по нескольким каналам и адаптерам.",
                    category = "network", group = "Протокол SMB",
                    hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanWorkstation\Parameters", name = "DisableMultiChannel", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 587, to = 605
                },
                // 10. network-smb-disable-leasing
                new Candidate {
                    id = "network-smb-disable-leasing",
                    name_ru = "Отключить сетевой лизинг дескрипторов файлов SMB (DisableLeasing)",
                    description_ru = "Устраняет задержки открытия и блокировки общих файлов в LanmanServer при совместной работе в локальной сети.",
                    category = "network", group = "Протокол SMB",
                    hive = "HKLM", path = @"System\CurrentControlSet\Services\LanmanServer\Parameters", name = "DisableLeasing", type = "REG_DWORD",
                    apply = 1, revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 606, to = 614
                },
                // 11. network-nic-lso-v2-ipv6
                new Candidate {
                    id = "network-nic-lso-v2-ipv6",
                    name_ru = "Отключить выгрузку сегментации больших пакетов IPv6 (LSOv2)",
                    description_ru = "Запрещает сетевому адаптеру разделять крупные IPv6 пакеты для исключения микрозадержек и джиттера в соревновательных играх.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*LsoV2IPv6", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 300, to = 315
                },
                // 12. network-nic-uso-ipv4
                new Candidate {
                    id = "network-nic-uso-ipv4",
                    name_ru = "Отключить аппаратную сегментацию UDP пакетов IPv4 (USO)",
                    description_ru = "Запрещает адаптеру объединять и фрагментировать UDP пакеты IPv4, гарантируя немедленную отправку игровых тиков.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*UsoIPv4", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 305, to = 315
                },
                // 13. network-nic-uso-ipv6
                new Candidate {
                    id = "network-nic-uso-ipv6",
                    name_ru = "Отключить аппаратную сегментацию UDP пакетов IPv6 (USO)",
                    description_ru = "Запрещает аппаратную сегментацию UDP датаграмм IPv6 на уровне драйвера сетевой карты.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*UsoIPv6", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 305, to = 315
                },
                // 14. network-nic-rsc-ipv6
                new Candidate {
                    id = "network-nic-rsc-ipv6",
                    name_ru = "Отключить коалесценцию входящих сегментов IPv6 (RSC)",
                    description_ru = "Отключает ожидание и склеивание входящих сегментов IPv6 для мгновенной обработки трафика сетевым стеком.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*RscIPv6", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 310, to = 320
                },
                // 15. network-nic-udp-rsc
                new Candidate {
                    id = "network-nic-udp-rsc",
                    name_ru = "Отключить коалесценцию входящих UDP пакетов (URO)",
                    description_ru = "Блокирует склеивание входящих UDP датаграмм на поддерживаемых картах в NDIS 6.89 для исключения задержек сетевого ввода.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*UdpRsc", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 310, to = 320
                },
                // 16. network-nic-pm-arp-offload
                new Candidate {
                    id = "network-nic-pm-arp-offload",
                    name_ru = "Отключить энергосберегающую выгрузку протокола ARP (*PMARPOffload)",
                    description_ru = "Запрещает перевод сетевой карты в режим сна с автономным ответом на ARP запросы.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*PMARPOffload", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 325, to = 335
                },
                // 17. network-nic-pm-ns-offload
                new Candidate {
                    id = "network-nic-pm-ns-offload",
                    name_ru = "Отключить энергосберегающую выгрузку Neighbor Solicitation (*PMNSOffload)",
                    description_ru = "Исключает автономную обработку IPv6 соседских запросов сетевой картой в состоянии сна.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*PMNSOffload", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 325, to = 335
                },
                // 18. network-nic-pm-wifi-rekey-offload
                new Candidate {
                    id = "network-nic-pm-wifi-rekey-offload",
                    name_ru = "Отключить энергосберегающий перевыпуск ключей Wi-Fi (*PMWiFiRekeyOffload)",
                    description_ru = "Запрещает адаптеру беспроводной связи выполнять автономный перевыпуск ключей GTK при низком энергопотреблении.",
                    category = "network", group = "Аппаратная разгрузка адаптера",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\...", name = "*PMWiFiRekeyOffload", type = "REG_SZ",
                    apply = "0", revMode = "delete", revVal = null,
                    file = "network/desc.md", from = 325, to = 335
                },
                // 19. power-directed-drips-override
                new Candidate {
                    id = "power-directed-drips-override",
                    name_ru = "Отключить принудительный режим глубокого сна компонентов (DirectedDripsOverride)",
                    description_ru = "Предотвращает принудительное погружение периферийных шин в глубокий сон Directed DRIPS.",
                    category = "power", group = "Управление питанием устройств",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", name = "DirectedDripsOverride", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "power/desc.md", from = 1368, to = 1375
                },
                // 20. power-check-power-source-after-rtc-wake
                new Candidate {
                    id = "power-check-power-source-after-rtc-wake",
                    name_ru = "Отключить проверку источника питания после пробуждения по таймеру RTC",
                    description_ru = "Исключает служебную задержку на опрос состояния батареи диспетчером питания после выхода из сна по RTC.",
                    category = "power", group = "Управление питанием устройств",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", name = "CheckPowerSourceAfterRtcWakeTime", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "power/desc.md", from = 1360, to = 1366
                },
                // 21. power-directed-drips-debounce-interval
                new Candidate {
                    id = "power-directed-drips-debounce-interval",
                    name_ru = "Сбросить защитный интервал стабилизации Directed DRIPS",
                    description_ru = "Устанавливает нулевой интервал стабилизации переходов энергосбережения Directed DRIPS Debounce Interval.",
                    category = "power", group = "Управление питанием устройств",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", name = "DirectedDripsDebounceInterval", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "power/desc.md", from = 1368, to = 1375
                },
                // 22. peripheral-usb-en-pm-debug
                new Candidate {
                    id = "peripheral-usb-en-pm-debug",
                    name_ru = "Отключить отладку управления питанием контроллеров USB (EnPMDebug)",
                    description_ru = "Отключает фоновую служебную отладку переходов питания USB в стеке драйвера usbport.sys.",
                    category = "peripheral", group = "USB контроллеры",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\usb", name = "EnPMDebug", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "peripheral/desc.md", from = 743, to = 754
                },
                // 23. peripheral-usb-debug-log-enable
                new Candidate {
                    id = "peripheral-usb-debug-log-enable",
                    name_ru = "Отключить сбор отладочных логов драйвера USB (debuglogenable)",
                    description_ru = "Сбрасывает флаг записи отладочных логов usbport.sys для устранения накладных расходов стека USB.",
                    category = "peripheral", group = "USB контроллеры",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\usb", name = "debuglogenable", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "peripheral/desc.md", from = 743, to = 754
                },
                // 24. system-kernel-dpc-watchdog-period
                new Candidate {
                    id = "system-kernel-dpc-watchdog-period",
                    name_ru = "Отключить таймер сторожевого контроля DPC (DpcWatchdogPeriod)",
                    description_ru = "Отключает периодический опрос сторожевого таймера DPC ядра, устраняя паразитные прерывания при непрерывном рендеринге.",
                    category = "system", group = "Прерывания и DPC",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel", name = "DpcWatchdogPeriod", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 3344, to = 3390
                },
                // 25. system-dwm-animations-shift-key
                new Candidate {
                    id = "system-dwm-animations-shift-key",
                    name_ru = "Отключить замедленные анимации по клавише Shift в DWM (AnimationsShiftKey)",
                    description_ru = "Отключает скрытую функцию замедления анимации окон при удерживании Shift в DWM CSettingsManager.",
                    category = "system", group = "Диспетчер окон (DWM)",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\DWM", name = "AnimationsShiftKey", type = "REG_DWORD",
                    apply = 0, revMode = "delete", revVal = null,
                    file = "system/desc.md", from = 2790, to = 2800
                }
            };
        }
    }
}
