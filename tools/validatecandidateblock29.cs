using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace ValidateCandidateBlock29
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
            string sourceBase = @"C:\Users\User\Documents\win-tweaker\data\source\win-config";
            string existingJson = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var existing = serializer.Deserialize<List<TweakItem>>(File.ReadAllText(existingJson, Encoding.UTF8));

            var existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in existing)
            {
                existingIds.Add(t.id);
                foreach (var r in t.registry)
                {
                    existingKeys.Add(r.hive + @"\" + r.path + @"\" + r.name);
                }
            }

            var candidates = new List<TweakItem>
            {
                // 1
                new TweakItem
                {
                    id = "power-allow-audio-execution-power-requests",
                    name_ru = "Запретить аудиопотокам блокировать переход процессора в энергосберегающий режим",
                    description_ru = "Отключает автоматическое требование активного режима выполнения (ExecutionRequired) при воспроизведении звука.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1357 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "AllowAudioToEnableExecutionRequiredPowerRequests",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 2
                new TweakItem
                {
                    id = "power-allow-system-required-power-requests",
                    name_ru = "Запретить преобразование системных запросов питания в активный режим CPU",
                    description_ru = "Предотвращает трансляцию запросов SystemRequired в ExecutionRequired, снижая пробуждения ядер из глубоких C-состояний.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1359 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "AllowSystemRequiredPowerRequests",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 3
                new TweakItem
                {
                    id = "power-checkpoint-system-sleep",
                    name_ru = "Отключить контрольные точки (чекпоинты) сна системы",
                    description_ru = "Отключает механизм создания контрольных точек состояния системы при переходах в спящий режим.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1362 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "CheckpointSystemSleep",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 4
                new TweakItem
                {
                    id = "power-disable-display-burst-power-change",
                    name_ru = "Отключить импульсную задержку дисплея при смене источника питания",
                    description_ru = "Предотвращает задержки обновления дисплея (Display Burst) при переключении между питанием от сети и батареей.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1377 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "DisableDisplayBurstOnPowerSourceChange",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 5
                new TweakItem
                {
                    id = "power-disable-vsync-latency-update",
                    name_ru = "Отключить фоновое обновление задержки VSync в диспетчере питания (PPM)",
                    description_ru = "Отключает периодический опрос задержек вертикальной синхронизации в модуле управления питанием процессора.",
                    group = "Процессор",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1380 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "DisableVsyncLatencyUpdate",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 6
                new TweakItem
                {
                    id = "power-enforce-austerity-mode",
                    name_ru = "Отключить принудительный режим строгой экономии энергии (Austerity Mode)",
                    description_ru = "Предотвращает активацию режима ограничения производительности Austerity Mode для фоновых системных служб.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1393 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "EnforceAusterityMode",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 7
                new TweakItem
                {
                    id = "power-enforce-console-lock-timeout",
                    name_ru = "Отключить принудительный тайм-аут экрана блокировки консоли",
                    description_ru = "Сбрасывает принудительное отключение дисплея при блокировке пользовательской сессии.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1394 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "EnforceConsoleLockScreenTimeout",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 8
                new TweakItem
                {
                    id = "power-enforce-disconnected-standby",
                    name_ru = "Отключить принудительный режим Disconnected Standby",
                    description_ru = "Запрещает принудительный перевод сетевых подключений в отключенное состояние в режиме ожидания.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1395 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "EnforceDisconnectedStandby",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 9
                new TweakItem
                {
                    id = "power-event-processor-enable",
                    name_ru = "Отключить процессор событий управления электропитанием",
                    description_ru = "Отключает внутреннюю очередь обработки телеметрии и событий переключения профилей питания.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1396 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "EventProcessorEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 10
                new TweakItem
                {
                    id = "power-exit-latency-check",
                    name_ru = "Отключить проверку задержки выхода из C-состояний (Exit Latency Check)",
                    description_ru = "Отключает постоянную перепроверку времени выхода ядер из состояний глубокого простоя в PPM.",
                    group = "Процессор",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1397 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "ExitLatencyCheckEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 11
                new TweakItem
                {
                    id = "power-experimental-cluster-idle-mitigation",
                    name_ru = "Отключить экспериментальное сглаживание простоя кластеров ядер",
                    description_ru = "Отключает экспериментальный опрос состояния простоя кластеров процессора для снижения DPC-задержек.",
                    group = "Процессор",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1398 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "ExperimentalClusterIdleMitigation",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 12
                new TweakItem
                {
                    id = "power-hetero-hgsplus-disable",
                    name_ru = "Отключить распределение потоков HGS+ в гетерогенном планировщике",
                    description_ru = "Отключает планирование потоков Intel HGS+ в планировщике питания гибридных процессоров.",
                    group = "Процессор",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1404 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "HeteroHgsPlusDisabled",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 13
                new TweakItem
                {
                    id = "power-hibernate-boot-optimization",
                    name_ru = "Отключить кэширование оптимизации быстрой загрузки гибернации",
                    description_ru = "Отключает сохранение предварительного снимка драйверов для быстрого запуска (HiberBootOptimization).",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1413 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "HibernateBootOptimizationEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 14
                new TweakItem
                {
                    id = "power-ignore-cs-compliance-check",
                    name_ru = "Игнорировать проверку совместимости Connected Standby",
                    description_ru = "Отключает системную проверку соответствия подключенного оборудования требованиям Connected Standby.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1423 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "IgnoreCsComplianceCheck",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 15
                new TweakItem
                {
                    id = "power-ipi-last-clock-owner-disable",
                    name_ru = "Отключить межпроцессорные прерывания (IPI) последнему владельцу таймера",
                    description_ru = "Предотвращает генерацию IPI к ядру, являющемуся последним владельцем системного таймера.",
                    group = "Процессор",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1425 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "IpiLastClockOwnerDisable",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 16
                new TweakItem
                {
                    id = "power-promote-hibernate-to-shutdown",
                    name_ru = "Перенаправлять команды гибернации в полное завершение работы",
                    description_ru = "Автоматически транслирует вызовы перехода в гибернацию в чистое выключение питания системы (S5).",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1453 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "PromoteHibernateToShutdown",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 17
                new TweakItem
                {
                    id = "power-user-battery-charge-estimator",
                    name_ru = "Отключить оценку времени зарядки батареи (Battery Charge Estimator)",
                    description_ru = "Очищает и блокирует фоновое вычисление прогноза зарядки батареи в WNF_PO_CHARGE_ESTIMATE.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1470 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "UserBatteryChargeEstimator",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 18
                new TweakItem
                {
                    id = "power-sleepstudy-accounting",
                    name_ru = "Отключить фоновый учет энергопотребления SleepStudy",
                    description_ru = "Отключает постоянный сбор статистики и блокираторов энергопотребления модулем SleepStudy.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1457 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "SleepstudyAccountingEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 19
                new TweakItem
                {
                    id = "power-thermal-polling-mode",
                    name_ru = "Отключить периодический опрос тепловых зон ACPI (Thermal Polling)",
                    description_ru = "Отключает циклическое прерывание ядер для проверки температурных зон, снижая DPC-задержки.",
                    group = "Процессор",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1466 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "ThermalPollingMode",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 20
                new TweakItem
                {
                    id = "power-thermal-telemetry-verbosity",
                    name_ru = "Отключить сбор телеметрии теплового состояния (Thermal Telemetry)",
                    description_ru = "Устанавливает уровень детализации событий терморегулирования в 0, отключая генерацию ETW-событий.",
                    group = "Управление питанием",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1467 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "ThermalTelemetryVerbosity",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 21
                new TweakItem
                {
                    id = "power-ttm-enable",
                    name_ru = "Отключить диспетчер переходного терморегулирования (TTM)",
                    description_ru = "Отключает модуль Transient Temperature Mitigation для исключения фоновых накладных расходов планировщика.",
                    group = "Процессор",
                    category = "power",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "power/desc.md", line_from = 1352, line_to = 1469 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"SYSTEM\CurrentControlSet\Control\Power",
                            name = "TtmEnabled",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 22
                new TweakItem
                {
                    id = "peripheral-audio-disable-spatial-global",
                    name_ru = "Отключить пространственный звук на глобальном уровне (Spatial Audio)",
                    description_ru = "Отключает глобальные шлюзы реестра для обработки Spatial Audio в AudioSrv.",
                    group = "Аудио",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 450, line_to = 454 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                            name = "DisableSpatialAudioGlobal",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 23
                new TweakItem
                {
                    id = "peripheral-audio-spatial-hrtf-default",
                    name_ru = "Отключить стандартную активацию HRTF-фильтрации пространственного звука",
                    description_ru = "Запрещает автоматическое включение HRTF (Head-Related Transfer Function) для аудиоустройств.",
                    group = "Аудио",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 450, line_to = 457 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                            name = "SpatialAudioHrtfOnByDefault",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 24
                new TweakItem
                {
                    id = "peripheral-audio-disable-spatial-combo",
                    name_ru = "Отключить пространственный звук на комбинированных аудиоразъемах",
                    description_ru = "Блокирует автоматическое включение Spatial Audio для комбинированных наушников/гарнитур.",
                    group = "Аудио",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 450, line_to = 460 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                            name = "DisableSpatialOnComboEndpoints",
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                },

                // 25
                new TweakItem
                {
                    id = "peripheral-audio-spatial-license-user-context",
                    name_ru = "Отключить проверку лицензии пространственного звука в контексте пользователя",
                    description_ru = "Отключает вызов проверки лицензии Atmos/DTS в пользовательском профиле при инициализации аудиопотока.",
                    group = "Аудио",
                    category = "peripheral",
                    type = "toggle",
                    risk = "low",
                    presets = new List<string> { "safe", "maximum" },
                    min_build = 19041,
                    needs_review = false,
                    source = new SourceObj { file = "peripheral/desc.md", line_from = 460, line_to = 467 },
                    registry = new List<RegItem>
                    {
                        new RegItem
                        {
                            hive = "HKLM",
                            path = @"Software\Microsoft\Windows\CurrentVersion\Audio\Policy\Spatial",
                            name = "SpatialAudioLicenseCheckRequiresUserContext",
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertObj { mode = "delete" }
                        }
                    }
                }
            };

            Console.WriteLine("Кандидатов: " + candidates.Count);

            int errors = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                var cand = candidates[i];
                if (existingIds.Contains(cand.id))
                {
                    Console.WriteLine("[ERROR] Коллизия ID: " + cand.id);
                    errors++;
                }

                foreach (var r in cand.registry)
                {
                    string fullKey = r.hive + @"\" + r.path + @"\" + r.name;
                    if (existingKeys.Contains(fullKey))
                    {
                        Console.WriteLine("[ERROR] Коллизия реестрового ключа: " + fullKey + " в твике " + cand.id);
                        errors++;
                    }
                }

                string fullSrc = Path.Combine(sourceBase, cand.source.file.Replace('/', '\\'));
                if (!File.Exists(fullSrc))
                {
                    Console.WriteLine("[ERROR] Файл источника не существует: " + fullSrc);
                    errors++;
                    continue;
                }

                string[] lines = File.ReadAllLines(fullSrc);
                if (cand.source.line_from < 1 || cand.source.line_to > lines.Length || cand.source.line_from > cand.source.line_to)
                {
                    Console.WriteLine("[ERROR] Неверный диапазон строк: " + cand.source.line_from + ".." + cand.source.line_to);
                    errors++;
                    continue;
                }

                StringBuilder sb = new StringBuilder();
                for (int l = cand.source.line_from - 1; l < cand.source.line_to; l++)
                {
                    sb.AppendLine(lines[l]);
                }
                string rangeContent = sb.ToString();

                foreach (var r in cand.registry)
                {
                    bool found = rangeContent.IndexOf(r.name, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!found)
                    {
                        Console.WriteLine("[ERROR] Имя параметра '" + r.name + "' не найдено в диапазоне " + cand.source.line_from + ".." + cand.source.line_to + " файла " + cand.source.file);
                        errors++;
                    }
                }
            }

            if (errors == 0)
            {
                Console.WriteLine("[SUCCESS] Все 25 кандидатов Блока 29 полностью валидны и не имеют коллизий!");
            }
            else
            {
                Console.WriteLine("[FAILURE] Ошибок найдено: " + errors);
            }
        }
    }
}
