using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace WinTweaker.Tools
{
    class TweakDef
    {
        public string id;
        public string name;
        public string desc;
        public string group;
        public string category;
        public string risk;
        public string preset;
        public string hive;
        public string path;
        public string nameVal;
        public string type;
        public object apply;
        public string revMode;
        public object revVal;
        public string srcFile;
        public int from;
        public int to;
        public bool needsReview;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(jsonPath);
            var serializer = new JavaScriptSerializer();
            var tweaks = serializer.Deserialize<List<Dictionary<string, object>>>(raw);

            var existingIds = new HashSet<string>();
            foreach (var t in tweaks)
            {
                if (t.ContainsKey("id") && t["id"] != null)
                {
                    existingIds.Add(t["id"].ToString());
                }
            }

            var defs = new List<TweakDef>
            {
                // ==================== PERIPHERAL ====================
                new TweakDef {
                    id = "peripheral-raw-mouse-throttle-disable",
                    name = "Отключить троттлинг сырого ввода мыши (RawMouseThrottleEnabled)",
                    desc = "Отключает системное троттлирование необработанного ввода (Raw Input) мыши в фоновых и оконных процессах, снижая задержку мыши.",
                    group = "Мышь и сенсоры", category = "peripheral", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PrecisionTouchPad", nameVal = "RawMouseThrottleEnabled", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "peripheral/desc.md", from = 10, to = 25, needsReview = true
                },
                new TweakDef {
                    id = "peripheral-disable-mouse-acceleration",
                    name = "Отключить акселерацию мыши (MouseSpeed)",
                    desc = "Отключает встроенное программное ускорение мыши для обеспечения линейного соотношения 1:1 перемещения сенсора и курсора.",
                    group = "Мышь и сенсоры", category = "peripheral", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Control Panel\Mouse", nameVal = "MouseSpeed", type = "REG_SZ",
                    apply = "0", revMode = "set", revVal = "1", srcFile = "peripheral/desc.md", from = 232, to = 244, needsReview = true
                },
                new TweakDef {
                    id = "peripheral-mouse-threshold1",
                    name = "Линейный порог мыши 1 (MouseThreshold1)",
                    desc = "Обнуляет первый порог динамического ускорения курсора для точного позиционирования прицела в шутерах без рывков.",
                    group = "Мышь и сенсоры", category = "peripheral", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Control Panel\Mouse", nameVal = "MouseThreshold1", type = "REG_SZ",
                    apply = "0", revMode = "set", revVal = "6", srcFile = "peripheral/desc.md", from = 232, to = 244, needsReview = true
                },
                new TweakDef {
                    id = "peripheral-mouse-threshold2",
                    name = "Линейный порог мыши 2 (MouseThreshold2)",
                    desc = "Обнуляет второй порог ускорения курсора при резких фликах и быстрых движениях манипулятора.",
                    group = "Мышь и сенсоры", category = "peripheral", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Control Panel\Mouse", nameVal = "MouseThreshold2", type = "REG_SZ",
                    apply = "0", revMode = "set", revVal = "10", srcFile = "peripheral/desc.md", from = 232, to = 244, needsReview = true
                },
                new TweakDef {
                    id = "peripheral-disable-selective-suspend",
                    name = "Запретить выборочную приостановку USB (DisableSelectiveSuspend)",
                    desc = "Предотвращает переход контроллеров и концентраторов USB в энергосберегающий спящий режим, устраняя задержку первого клика.",
                    group = "USB и порты", category = "peripheral", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\USB", nameVal = "DisableSelectiveSuspend", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "peripheral/desc.md", from = 735, to = 755, needsReview = true
                },
                new TweakDef {
                    id = "peripheral-disable-audio-ducking",
                    name = "Отключить приглушение звука при связи (UserDuckingPreference)",
                    desc = "Отключает автоматическое снижение громкости музыки и игр Windows при входящих звонках (0 dB / Do nothing).",
                    group = "Звук и аудио", category = "peripheral", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Multimedia\Audio", nameVal = "UserDuckingPreference", type = "REG_DWORD",
                    apply = 3, revMode = "set", revVal = 1, srcFile = "peripheral/desc.md", from = 1268, to = 1276, needsReview = true
                },
                new TweakDef {
                    id = "peripheral-disable-dynamic-lighting",
                    name = "Отключить динамическое освещение подсветки (AmbientLightingEnabled)",
                    desc = "Отключает службу фоновой синхронизации подсветки RGB-периферии Windows Dynamic Lighting, экономя такты процессора.",
                    group = "Подсветка и устройства", category = "peripheral", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Lighting", nameVal = "AmbientLightingEnabled", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "peripheral/desc.md", from = 1678, to = 1686, needsReview = true
                },

                // ==================== POWER ====================
                new TweakDef {
                    id = "power-disable-hibernation",
                    name = "Отключить режим гибернации (HibernateEnabled)",
                    desc = "Полностью отключает режим гибернации и удаляет файл hiberfil.sys, освобождая десятки гигабайт на диске.",
                    group = "Спящий режим и запуск", category = "power", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", nameVal = "HibernateEnabled", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "power/desc.md", from = 1414, to = 1420, needsReview = true
                },
                new TweakDef {
                    id = "power-disable-fast-startup",
                    name = "Отключить быстрый запуск (HiberbootEnabled)",
                    desc = "Отключает гибридный быстрый запуск Windows, предотвращая накопление фрагментации оперативной памяти и ошибок драйверов.",
                    group = "Спящий режим и запуск", category = "power", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", nameVal = "HiberbootEnabled", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "power/desc.md", from = 1407, to = 1413, needsReview = true
                },
                new TweakDef {
                    id = "power-disable-timer-coalescing",
                    name = "Отключить группировку таймеров (CoalescingTimerInterval)",
                    desc = "Отключает фоновое объединение системных таймеров для обеспечения минимального джиттера и непрерывной отзывчивости ядра.",
                    group = "Таймеры и планировщик", category = "power", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", nameVal = "CoalescingTimerInterval", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1500, srcFile = "power/desc.md", from = 1363, to = 1368, needsReview = true
                },
                new TweakDef {
                    id = "power-disable-deep-io-coalescing",
                    name = "Отключить глубокое объединение дискового ввода-вывода (DeepIoCoalescingEnabled)",
                    desc = "Запрещает диспетчеру питания откладывать дисковые I/O операции ради энергосбережения, минимизируя латентность NVMe/SSD.",
                    group = "Диск и ввод-вывод", category = "power", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", nameVal = "DeepIoCoalescingEnabled", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 0, srcFile = "power/desc.md", from = 1364, to = 1369, needsReview = true
                },
                new TweakDef {
                    id = "power-disable-energy-estimation",
                    name = "Отключить фоновую оценку энергопотребления (EnergyEstimationEnabled)",
                    desc = "Отключает постоянный фоновый опрос и расчет энергопотребления процессов для телеметрии аккумулятора.",
                    group = "Таймеры и планировщик", category = "power", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", nameVal = "EnergyEstimationEnabled", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "power/desc.md", from = 1390, to = 1395, needsReview = true
                },
                new TweakDef {
                    id = "power-disable-fx-accounting-telemetry",
                    name = "Отключить телеметрию Fx Accounting (FxAccountingTelemetryDisabled)",
                    desc = "Отключает внутреннюю телеметрию Power Management Framework по учету времени переключения фаз питания устройств.",
                    group = "Энергосбережение и телеметрия", category = "power", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", nameVal = "FxAccountingTelemetryDisabled", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "power/desc.md", from = 1398, to = 1403, needsReview = true
                },
                new TweakDef {
                    id = "power-disable-idle-states-boot",
                    name = "Запретить состояния простоя ядер при старте (DisableIdleStatesAtBoot)",
                    desc = "Предотвращает ранний перевод ядер процессора в глубокие состояния сна (C-states) при инициализации подсистем ядра.",
                    group = "Процессор и ядра", category = "power", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Power", nameVal = "DisableIdleStatesAtBoot", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "power/desc.md", from = 1374, to = 1380, needsReview = true
                }
            };

            int addedCount = 0;
            foreach (var d in defs)
            {
                if (existingIds.Contains(d.id))
                {
                    Console.WriteLine("Skip existing: " + d.id);
                    continue;
                }

                var t = new Dictionary<string, object>();
                t["id"] = d.id;
                t["name_ru"] = d.name;
                t["description_ru"] = d.desc;
                t["group"] = d.group;
                t["category"] = d.category;
                t["type"] = "toggle";
                t["risk"] = d.risk;
                t["presets"] = new List<string> { d.preset };
                t["min_build"] = null;
                t["needs_review"] = d.needsReview;

                var src = new Dictionary<string, object>();
                src["file"] = d.srcFile;
                src["line_from"] = d.from;
                src["line_to"] = d.to;
                t["source"] = src;

                var regItem = new Dictionary<string, object>();
                regItem["hive"] = d.hive;
                regItem["path"] = d.path;
                regItem["name"] = d.nameVal;
                regItem["type"] = d.type;

                regItem["apply"] = d.apply;

                var revObj = new Dictionary<string, object>();
                revObj["mode"] = d.revMode;
                revObj["value"] = d.revVal;
                regItem["revert"] = revObj;

                t["registry"] = new List<Dictionary<string, object>> { regItem };

                tweaks.Add(t);
                existingIds.Add(d.id);
                addedCount++;
                Console.WriteLine("Added tweak: " + d.id);
            }

            string updatedJson = serializer.Serialize(tweaks);
            File.WriteAllText(jsonPath, updatedJson, System.Text.Encoding.UTF8);
            Console.WriteLine("Done! Added {0} tweaks. Total tweaks: {1}", addedCount, tweaks.Count);
        }
    }
}
