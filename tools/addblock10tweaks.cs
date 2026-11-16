using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock10Tool
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
            string tweaksPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string json = File.ReadAllText(tweaksPath, Encoding.UTF8);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            List<TweakItem> list = serializer.Deserialize<List<TweakItem>>(json);

            Console.WriteLine("Current tweaks count: " + list.Count);

            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in list) existingIds.Add(t.id);

            List<TweakItem> newTweaks = new List<TweakItem>();

            // 1. power-throttling-off
            newTweaks.Add(new TweakItem {
                id = "power-throttling-off",
                name_ru = "Отключить Power Throttling (PowerThrottlingOff)",
                description_ru = "Отключает глобальный механизм троттлинга процессов и фоновых потоков Windows для стабилизации фреймтайма.",
                group = "Управление питанием CPU",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "power/desc.md", line_from = 1516, line_to = 1518 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling",
                        name = "PowerThrottlingOff",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 2. power-modern-sleep-enabled-actions
            newTweaks.Add(new TweakItem {
                id = "power-modern-sleep-enabled-actions",
                name_ru = "Отключить агрессивные действия Modern Sleep (EnabledActions)",
                description_ru = "Отключает агрессивные фоновые действия энергосбережения при переходе в режим ожидания (PopAggressiveStandbyActionsRegValue).",
                group = "Управление питанием CPU",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "power/desc.md", line_from = 1512, line_to = 1514 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Power\ModernSleep",
                        name = "EnabledActions",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 3. power-modern-sleep-ds-net-refresh
            newTweaks.Add(new TweakItem {
                id = "power-modern-sleep-ds-net-refresh",
                name_ru = "Отключить сетевое обновление Modern Sleep (EnableDsNetRefresh)",
                description_ru = "Запрещает фоновое периодическое пробуждение сетевых адаптеров для обновления пакетов в режиме глубокого сна.",
                group = "Управление питанием CPU",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "power/desc.md", line_from = 1512, line_to = 1515 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Power\ModernSleep",
                        name = "EnableDsNetRefresh",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 4. power-always-compute-qos-hints
            newTweaks.Add(new TweakItem {
                id = "power-always-compute-qos-hints",
                name_ru = "Отключить вычисление QoS хинтов (AlwaysComputeQosHints)",
                description_ru = "Отключает постоянный пересчет эвристик качества обслуживания планировщика питания, снижая накладные расходы ядра.",
                group = "Управление питанием CPU",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "power/desc.md", line_from = 1744, line_to = 1750 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Power",
                        name = "AlwaysComputeQosHints",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 5. power-nic-device-sleep-on-disconnect
            newTweaks.Add(new TweakItem {
                id = "power-nic-device-sleep-on-disconnect",
                name_ru = "Запретить сон сетевой карты при отключении кабеля (*DeviceSleepOnDisconnect)",
                description_ru = "Предотвращает перевод сетевой карты в режим глубокого сна D3 при временном отключении кабеля или потере линка.",
                group = "Энергосбережение периферии",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "power/desc.md", line_from = 2038, line_to = 2041 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "*DeviceSleepOnDisconnect",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 6. power-nic-dynamic-power-gating
            newTweaks.Add(new TweakItem {
                id = "power-nic-dynamic-power-gating",
                name_ru = "Отключить динамический гейтинг питания сетевой карты (*EnableDynamicPowerGating)",
                description_ru = "Отключает динамическое отключение неиспользуемых блоков сетевого чипа, снижая пинг и исключая задержку пробуждения PHY.",
                group = "Энергосбережение периферии",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "power/desc.md", line_from = 2038, line_to = 2042 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "*EnableDynamicPowerGating",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 7. power-nic-dma-coalescing
            newTweaks.Add(new TweakItem {
                id = "power-nic-dma-coalescing",
                name_ru = "Отключить группировку DMA сетевой карты (DMACoalescing)",
                description_ru = "Запрещает сетевому контроллеру откладывать пакетные DMA-транзакции в память для экономии энергии, снижая задержку приёма пакетов.",
                group = "Энергосбережение периферии",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "power/desc.md", line_from = 2038, line_to = 2044 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "DMACoalescing",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 8. power-nic-energy-efficient-ethernet
            newTweaks.Add(new TweakItem {
                id = "power-nic-energy-efficient-ethernet",
                name_ru = "Отключить энергоэффективный Ethernet (*EEE)",
                description_ru = "Отключает стандарт IEEE 802.3az Energy-Efficient Ethernet, предотвращая переход сетевого линка в состояние низкого энергопотребления (LPI).",
                group = "Энергосбережение периферии",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "power/desc.md", line_from = 2056, line_to = 2061 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "*EEE",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 9. power-nic-selective-suspend
            newTweaks.Add(new TweakItem {
                id = "power-nic-selective-suspend",
                name_ru = "Отключить селективное усыпление сетевого адаптера (*SelectiveSuspend)",
                description_ru = "Запрещает NDIS отправлять сетевой адаптер в выборочный спящий режим при кратковременном отсутствии трафика.",
                group = "Энергосбережение периферии",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "power/desc.md", line_from = 2063, line_to = 2066 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                        name = "*SelectiveSuspend",
                        type = "REG_SZ",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", "0" } },
                        revert = new RevertObj { mode = "set", value = "1" }
                    }
                }
            });

            // 10. power-usb-audio-idle-timeout
            newTweaks.Add(new TweakItem {
                id = "power-usb-audio-idle-timeout",
                name_ru = "Отключить таймаут простоя USB-аудио (PerformanceIdleTime)",
                description_ru = "Отключает таймер засыпания звукового USB-устройства (PerformanceIdleTime=0), исключая щелчки, проглатывание первого звука и задержки инициализации ЦАП.",
                group = "Энергосбережение периферии",
                category = "power",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = true,
                source = new SourceObj { file = "power/desc.md", line_from = 1923, line_to = 1928 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}\00xx\PowerSettings",
                        name = "PerformanceIdleTime",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 30 }
                    }
                }
            });

            // 11. system-dxg-foreground-priority-boost
            newTweaks.Add(new TweakItem {
                id = "system-dxg-foreground-priority-boost",
                name_ru = "Повысить приоритет активного графического процесса (ForegroundPriorityBoost)",
                description_ru = "Активирует приоритетное планирование графических квантов GPU для окна на переднем плане, снижая задержки рендеринга и микрофризы.",
                group = "Графическая подсистема (DirectX / DWM)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 5456, line_to = 5458 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler",
                        name = "ForegroundPriorityBoost",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 12. system-dxg-adjust-worker-thread
            newTweaks.Add(new TweakItem {
                id = "system-dxg-adjust-worker-thread",
                name_ru = "Адаптивный приоритет рабочих потоков GPU (AdjustWorkerThreadPriority)",
                description_ru = "Разрешает планировщику VidSchi динамически повышать приоритет потоков-обработчиков пакетов команд для уменьшения задержки конвейера.",
                group = "Графическая подсистема (DirectX / DWM)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 5445, line_to = 5447 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler",
                        name = "AdjustWorkerThreadPriority",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 13. system-dxg-disable-overlays-mpo
            newTweaks.Add(new TweakItem {
                id = "system-dxg-disable-overlays-mpo",
                name_ru = "Аппаратная композиция DirectFlip / MPO (DisableOverlays)",
                description_ru = "Гарантирует включение аппаратных оверлеев Multi-Plane Overlay (MPO) для прямого вывода кадров без накладных расходов композитора DWM.",
                group = "Графическая подсистема (DirectX / DWM)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 5259, line_to = 5261 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                        name = "DisableOverlays",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 14. system-dwm-overlay-test-mode
            newTweaks.Add(new TweakItem {
                id = "system-dwm-overlay-test-mode",
                name_ru = "Разрешить режим независимых оверлеев DWM (OverlayTestMode)",
                description_ru = "Устанавливает штатный режим проверки аппаратных плоскостей оверлеев MPO в композиторе DWM (0 = штатный, 5 = отключение оверлеев).",
                group = "Графическая подсистема (DirectX / DWM)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 2786, line_to = 2788 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Microsoft\Windows\Dwm",
                        name = "OverlayTestMode",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 5 }
                    }
                }
            });

            // 15. system-dwm-disable-independent-flip
            newTweaks.Add(new TweakItem {
                id = "system-dwm-disable-independent-flip",
                name_ru = "Режим Independent Flip в DWM (DisableIndependentFlip)",
                description_ru = "Обеспечивает работу режима независимого переключения буферов (Independent Flip / iFlip) в играх без оконного инпут-лага.",
                group = "Графическая подсистема (DirectX / DWM)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 2779, line_to = 2781 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Microsoft\Windows\Dwm",
                        name = "DisableIndependentFlip",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 16. system-dwm-disable-advanced-direct-flip
            newTweaks.Add(new TweakItem {
                id = "system-dwm-disable-advanced-direct-flip",
                name_ru = "Прямой вывод Advanced DirectFlip (DisableAdvancedDirectFlip)",
                description_ru = "Разрешает использование Advanced DirectFlip для оптимизации цепочки показа кадров между видеокартой и дисплеем.",
                group = "Графическая подсистема (DirectX / DWM)",
                category = "system",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "system/desc.md", line_from = 2778, line_to = 2780 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Microsoft\Windows\Dwm",
                        name = "DisableAdvancedDirectFlip",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 17. nvidia-hide-xgpu-tray-icon
            newTweaks.Add(new TweakItem {
                id = "nvidia-hide-xgpu-tray-icon",
                name_ru = "Скрыть иконку активности GPU в трее (HideXGpuTrayIcon)",
                description_ru = "Отключает отображение дополнительной иконки активности видеокарты в системном трее Windows.",
                group = "Интерфейс и телеметрия",
                category = "nvidia",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "nvidia/desc.md", line_from = 579, line_to = 582 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak",
                        name = "HideXGpuTrayIcon",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 18. nvidia-context-ui-policy
            newTweaks.Add(new TweakItem {
                id = "nvidia-context-ui-policy",
                name_ru = "Удалить пункт NVIDIA из контекстного меню Рабочего стола (ContextUIPolicy)",
                description_ru = "Удаляет пункт вызова Панели управления NVIDIA из контекстного меню рабочего стола, ускоряя его открытие по правому клику.",
                group = "Интерфейс и телеметрия",
                category = "nvidia",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "nvidia/desc.md", line_from = 594, line_to = 598 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKCU",
                        path = @"Software\NVIDIA Corporation\Global\NvCplApi\Policies",
                        name = "ContextUIPolicy",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 19. nvidia-show-dlss-indicator
            newTweaks.Add(new TweakItem {
                id = "nvidia-show-dlss-indicator",
                name_ru = "Отключить экранный индикатор DLSS (ShowDlssIndicator)",
                description_ru = "Отключает отладочный водяной знак DLSS в углу экрана во время игры.",
                group = "Интерфейс и телеметрия",
                category = "nvidia",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "nvidia/desc.md", line_from = 620, line_to = 624 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\NVIDIA Corporation\Global\NGXCore",
                        name = "ShowDlssIndicator",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 20. nvidia-telemetry-client-optout
            newTweaks.Add(new TweakItem {
                id = "nvidia-telemetry-client-optout",
                name_ru = "Отключить сбор телеметрии NVCPL (OptInOrOutPreference)",
                description_ru = "Запрещает клиенту Панели управления NVIDIA отправку пользовательских отчетов и диагностических данных.",
                group = "Интерфейс и телеметрия",
                category = "nvidia",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "nvidia/desc.md", line_from = 668, line_to = 671 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\NVIDIA Corporation\NVControlPanel2\Client",
                        name = "OptInOrOutPreference",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 21. privacy-disable-background-apps
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-background-apps",
                name_ru = "Запретить фоновую работу UWP-приложений (LetAppsRunInBackground)",
                description_ru = "Глобально запрещает приложениям UWP выполняться в фоне, потреблять такты процессора и оперативную память.",
                group = "Фоновые службы и телеметрия",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 682, line_to = 685 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\AppPrivacy",
                        name = "LetAppsRunInBackground",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 2 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 22. privacy-disable-maps-autodownload
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-maps-autodownload",
                name_ru = "Отключить автозагрузку и обновление карт (AutoDownloadAndUpdateMapData)",
                description_ru = "Отключает автоматическое скачивание и обновление картографических данных службой фонового диспетчера карт.",
                group = "Фоновые службы и телеметрия",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 522, line_to = 525 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\Maps",
                        name = "AutoDownloadAndUpdateMapData",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 1 } },
                        revert = new RevertObj { mode = "set", value = 0 }
                    }
                }
            });

            // 23. privacy-disable-cross-device-cdp
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-cross-device-cdp",
                name_ru = "Отключить кросс-девайсный обмен данными (EnableCdp)",
                description_ru = "Отключает платформу подключенных устройств Connected Devices Platform (CDP) и функцию Share Across Devices.",
                group = "Фоновые службы и телеметрия",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 583, line_to = 585 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\System",
                        name = "EnableCdp",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 24. privacy-disable-activity-feed
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-activity-feed",
                name_ru = "Отключить журнал действий пользователя (EnableActivityFeed)",
                description_ru = "Отключает сбор, хранение и синхронизацию журнала действий пользователя (Activity Feed) с облаком Microsoft.",
                group = "Фоновые службы и телеметрия",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1185, line_to = 1187 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"Software\Policies\Microsoft\Windows\System",
                        name = "EnableActivityFeed",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            // 25. privacy-disable-cortana-voice
            newTweaks.Add(new TweakItem {
                id = "privacy-disable-cortana-voice",
                name_ru = "Отключить голосовой ассистент Cortana (AllowCortana)",
                description_ru = "Полностью отключает голосовой помощник Cortana в системе и при поиске.",
                group = "Фоновые службы и телеметрия",
                category = "privacy",
                type = "toggle",
                risk = "low",
                presets = new List<string> { "recommended" },
                needs_review = false,
                source = new SourceObj { file = "privacy/desc.md", line_from = 1225, line_to = 1227 },
                registry = new List<RegItem> {
                    new RegItem {
                        hive = "HKLM",
                        path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                        name = "AllowCortana",
                        type = "REG_DWORD",
                        apply = new Dictionary<string, object> { { "mode", "set" }, { "value", 0 } },
                        revert = new RevertObj { mode = "set", value = 1 }
                    }
                }
            });

            int added = 0;
            foreach (var tweak in newTweaks)
            {
                if (!existingIds.Contains(tweak.id))
                {
                    list.Add(tweak);
                    existingIds.Add(tweak.id);
                    added++;
                    Console.WriteLine("Added: " + tweak.id);
                }
                else
                {
                    Console.WriteLine("Skipping already existing: " + tweak.id);
                }
            }

            Console.WriteLine("Total added: " + added);
            Console.WriteLine("New total: " + list.Count);

            string updatedJson = FormatJson(serializer.Serialize(list));
            File.WriteAllText(tweaksPath, updatedJson, new UTF8Encoding(false));
            Console.WriteLine("Updated " + tweaksPath + " successfully.");
        }

        private static string FormatJson(string json)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
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
