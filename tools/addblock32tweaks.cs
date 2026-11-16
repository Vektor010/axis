using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace AddBlock32
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootDir = @"C:\Users\User\Documents\win-tweaker";
            string tweaksJsonPath = Path.Combine(rootDir, @"data\tweaks.json");
            string backupPath = Path.Combine(rootDir, @"data\tweaks.json.bak32");

            // Восстанавливаем из бэкапа 750 твиков
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, tweaksJsonPath, true);
                Console.WriteLine("Восстановлен исходный файл из бэкапа: " + backupPath);
            }

            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var list = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJsonPath, Encoding.UTF8));
            Console.WriteLine("Исходное число твиков: " + list.Count);

            var newTweaks = CreateBlock32Tweaks();
            Console.WriteLine("Новых твиков для добавления: " + newTweaks.Count);

            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in list)
            {
                ids.Add(t["id"] as string);
            }

            foreach (var nt in newTweaks)
            {
                string id = nt["id"] as string;
                if (ids.Contains(id))
                {
                    throw new Exception("Дубликат ID обнаружен: " + id);
                }
                ids.Add(id);
                list.Add(nt);
            }

            Console.WriteLine("Итоговое число твиков: " + list.Count);

            string jsonOutput = ser.Serialize(list);

            // Форматируем JSON красиво
            StringBuilder sb = new StringBuilder();
            int indent = 0;
            bool inQuote = false;
            for (int i = 0; i < jsonOutput.Length; i++)
            {
                char ch = jsonOutput[i];
                if (ch == '"' && (i == 0 || jsonOutput[i - 1] != '\\'))
                {
                    inQuote = !inQuote;
                    sb.Append(ch);
                }
                else if (!inQuote)
                {
                    if (ch == '{' || ch == '[')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (ch == '}' || ch == ']')
                    {
                        sb.Append("\r\n");
                        indent--;
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(ch);
                    }
                    else if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent * 2));
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

            File.WriteAllText(tweaksJsonPath, sb.ToString(), new UTF8Encoding(false));
            Console.WriteLine("Успешно записано в tweaks.json (UTF-8 без BOM)");
        }

        static List<Dictionary<string, object>> CreateBlock32Tweaks()
        {
            var res = new List<Dictionary<string, object>>();

            // 1
            res.Add(CreateTweak(
                "system-mmcss-no-lazy-mode",
                "Отключить ленивый режим MMCSS (NoLazyMode)",
                "Отключает переход планировщика MMCSS (Multimedia Class Scheduler Service) в энергосберегающий режим ожидания (Lazy Mode), предотвращая задержки при возобновлении обработки аудиопотока и мультимедийных пакетов.",
                "system",
                "MMCSS",
                new string[] { "mmcss", "latency", "audio", "scheduling", "noverse" },
                new string[] { "latency", "gaming" },
                "system/desc.md",
                1196,
                1212,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "NoLazyMode",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 2
            res.Add(CreateTweak(
                "system-mmcss-idle-detection-cycles",
                "Максимальные циклы детекции простоя MMCSS (IdleDetectionCycles)",
                "Устанавливает максимальное количество циклов планировщика MMCSS перед определением потока как простаивающего (31 цикл вместо стандартных 2), продлевая окно гарантированного квантования времени.",
                "system",
                "MMCSS",
                new string[] { "mmcss", "cycles", "audio", "scheduling", "noverse" },
                new string[] { "latency", "gaming" },
                "system/desc.md",
                1196,
                1212,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "IdleDetectionCycles",
                "REG_DWORD",
                31,
                "delete",
                null
            ));

            // 3
            res.Add(CreateTweak(
                "system-mmcss-lazy-mode-timeout",
                "Увеличить таймаут ленивого режима MMCSS (LazyModeTimeout)",
                "Задает предельный таймаут активности планировщика перед активацией ленивого режима MMCSS в микросекундах (10000000 мкс), снижая вероятность просадки приоритетов при прерывистом рендеринге.",
                "system",
                "MMCSS",
                new string[] { "mmcss", "timeout", "latency", "scheduling", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                1196,
                1212,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "LazyModeTimeout",
                "REG_DWORD",
                10000000,
                "delete",
                null
            ));

            // 4
            res.Add(CreateTweak(
                "system-mmcss-scheduler-period",
                "Минимальный период планировщика MMCSS (SchedulerPeriod)",
                "Устанавливает минимально допустимый период планирования мультимедийных задач MMCSS (50000 сотых долей микросекунды / 5 мс вместо дефолтных 10 мс), повышая отзывчивость планирования времени.",
                "system",
                "MMCSS",
                new string[] { "mmcss", "period", "timer", "scheduling", "noverse" },
                new string[] { "latency", "gaming" },
                "system/desc.md",
                1196,
                1212,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "SchedulerPeriod",
                "REG_DWORD",
                50000,
                "delete",
                null
            ));

            // 5
            res.Add(CreateTweak(
                "system-mmcss-max-threads-process",
                "Увеличить лимит потоков MMCSS на процесс (MaxThreadsPerProcess)",
                "Повышает максимальное количество регистрируемых мультимедийных потоков AvSetMmThreadCharacteristics на один процесс до аппаратного предела драйвера (128 вместо дефолтных 32).",
                "system",
                "MMCSS",
                new string[] { "mmcss", "threads", "avrt", "process", "noverse" },
                new string[] { "latency", "gaming" },
                "system/desc.md",
                1196,
                1212,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "MaxThreadsPerProcess",
                "REG_DWORD",
                128,
                "delete",
                null
            ));

            // 6
            res.Add(CreateTweak(
                "system-mmcss-max-threads-total",
                "Увеличить общий пул потоков MMCSS (MaxThreadsTotal)",
                "Увеличивает суммарный пул активных потоков планировщика MMCSS для всех приложений в системе до 65535 (дефолт 256), предотвращая отказ в регистрации мультимедийных задач при высокой нагрузке.",
                "system",
                "MMCSS",
                new string[] { "mmcss", "pool", "threads", "system", "noverse" },
                new string[] { "latency", "gaming" },
                "system/desc.md",
                1196,
                1212,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                "MaxThreadsTotal",
                "REG_DWORD",
                65535,
                "delete",
                null
            ));

            // 7
            res.Add(CreateTweak(
                "system-dwm-disable-draw-list-caching",
                "Отключить кэширование списков отрисовки DWM (DisableDrawListCaching)",
                "Отключает кэширование буферов DrawList в DWM, принуждая немедленную передачу отрисованных примитивов в конвейер композиции без задержек ожидания инвалидации кэша.",
                "system",
                "DWM",
                new string[] { "dwm", "drawlist", "caching", "composition", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2800,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "DisableDrawListCaching",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 8
            res.Add(CreateTweak(
                "system-dwm-disable-projected-shadows",
                "Отключить проецируемые тени окон DWM (DisableProjectedShadows)",
                "Отключает генерацию сцен CompositionProjectedShadow в DWM, экономя ресурсы шейдерных блоков GPU при перемещении окон.",
                "system",
                "DWM",
                new string[] { "dwm", "shadows", "gpu", "visual", "noverse" },
                new string[] { "gaming" },
                "system/desc.md",
                2726,
                2800,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "DisableProjectedShadows",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 9
            res.Add(CreateTweak(
                "system-dwm-enable-backdrop-blur-caching",
                "Включить оптимизированный кэш размытия фона DWM (EnableBackdropBlurCaching)",
                "Активирует повторное использование вычисленного размытия заднего фона (Backdrop Blur) в dwmcore, исключая избыточный рендеринг гауссова размытия на каждом кадре.",
                "system",
                "DWM",
                new string[] { "dwm", "backdrop", "blur", "caching", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2800,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnableBackdropBlurCaching",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 10
            res.Add(CreateTweak(
                "system-dwm-enable-effect-caching",
                "Включить кэширование визуальных эффектов DWM (EnableEffectCaching)",
                "Включает внутреннее кэширование эффектов композитора Windows для ускорения отрисовки статических элементов интерфейса.",
                "system",
                "DWM",
                new string[] { "dwm", "effects", "caching", "ui", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2726,
                2800,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "EnableEffectCaching",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 11
            res.Add(CreateTweak(
                "system-dwm-force-disable-mode-change-animation",
                "Отключить анимацию смены видеорежима дисплея (ForceDisableModeChangeAnimation)",
                "Отключает визуальную задержку затемнения и анимацию перехода при смене разрешения, герцовки или топологии мониторов в Windows 11.",
                "system",
                "DWM",
                new string[] { "dwm", "animation", "display", "resolution", "noverse" },
                new string[] { "latency", "gaming" },
                "system/desc.md",
                2800,
                2850,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "ForceDisableModeChangeAnimation",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 12
            res.Add(CreateTweak(
                "system-dwm-disable-hologram-compositor",
                "Отключить композитор голографических дисплеев (DisableHologramCompositor)",
                "Пропускает регистрацию слушателя голографических устройств и специализированных шлемов в uDWM, исключая лишние системные вызовы.",
                "system",
                "DWM",
                new string[] { "dwm", "hologram", "compositor", "optimization", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2820,
                2850,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "DisableHologramCompositor",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 13
            res.Add(CreateTweak(
                "system-dwm-child-window-dpi-isolation",
                "Изоляция масштабирования DPI дочерних окон (ChildWindowDpiIsolation)",
                "Активирует изоляцию контекста DPI для дочерних окон в подсистеме win32kfull, предотвращая визуальные артефакты и пересчет координат в смешанных DPI-средах.",
                "system",
                "DWM",
                new string[] { "dwm", "dpi", "scaling", "win32k", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2830,
                2860,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "ChildWindowDpiIsolation",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 14
            res.Add(CreateTweak(
                "system-dwm-disable-device-bitmaps",
                "Отключить устаревшие битовые карты устройств DWM (DisableDeviceBitmaps)",
                "Отключает использование Device Bitmaps в win32kfull, форсируя работу композиции исключительно через современные поверхности DirectComposition.",
                "system",
                "DWM",
                new string[] { "dwm", "bitmaps", "directcomposition", "win32k", "noverse" },
                new string[] { "latency" },
                "system/desc.md",
                2830,
                2860,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm",
                "DisableDeviceBitmaps",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 15
            res.Add(CreateTweak(
                "system-dwm-scene-enable-shadow",
                "Отключить тени сцен композитора (Scene EnableShadow)",
                "Отключает отрисовку теней в подсистеме Dwm\\Scene, снижая нагрузку на пиксельные шейдеры рабочего стола.",
                "system",
                "DWM",
                new string[] { "dwm", "scene", "shadows", "gpu", "noverse" },
                new string[] { "gaming" },
                "system/desc.md",
                2875,
                2900,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                "EnableShadow",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 16
            res.Add(CreateTweak(
                "system-dwm-scene-image-processing-8bit",
                "Форсировать 8-битную обработку изображений сцены (ImageProcessing8bit)",
                "Ограничивает внутреннюю точность обработки промежуточных растров сцены DWM до 8 бит на канал, снижая пропускную способность памяти GPU.",
                "system",
                "DWM",
                new string[] { "dwm", "scene", "color", "bandwidth", "noverse" },
                new string[] { "gaming" },
                "system/desc.md",
                2875,
                2900,
                "HKLM",
                @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                "ImageProcessing8bit",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 17
            res.Add(CreateTweak(
                "peripheral-audio-skip-rt-heap",
                "Использовать выделенную кучу реального времени звука (SkipRTHeap)",
                "Переключает подсистему audiodg на выделенную кучу реального времени (RT Heap) вместо стандартной кучи C++, снижая вариативность задержек и джиттер при выделении аудиопамяти.",
                "peripheral",
                "Аудио",
                new string[] { "audio", "audiodg", "heap", "realtime", "noverse" },
                new string[] { "latency", "gaming" },
                "peripheral/desc.md",
                948,
                985,
                "HKLM",
                @"Software\Microsoft\Windows\CurrentVersion\Audio",
                "SkipRTHeap",
                "REG_DWORD",
                0,
                "delete",
                null
            ));

            // 18
            res.Add(CreateTweak(
                "peripheral-audio-suppress-glitch-logging",
                "Подавить логирование глитчей аудиомоста (SuppressBridgeTargetGlitchLogging)",
                "Отключает запись в журнал событий при возникновении микроглитчей в аудиомосте CEndpointInstance, предотвращая всплески дискового I/O в моменты задержек звука.",
                "peripheral",
                "Аудио",
                new string[] { "audio", "logging", "glitch", "io", "noverse" },
                new string[] { "latency" },
                "peripheral/desc.md",
                948,
                985,
                "HKLM",
                @"Software\Microsoft\Windows\CurrentVersion\Audio",
                "SuppressBridgeTargetGlitchLogging",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 19
            res.Add(CreateTweak(
                "peripheral-audio-max-cpu-percent",
                "Максимальный лимит CPU для звуковой помпы (AudioDGCPUPercentMax)",
                "Увеличивает предельный бюджет процессорного времени для потока помпы CAudioPump с 40% до 90% периода, исключая выпадения звука (DPC dropouts) под высокой игровой нагрузкой.",
                "peripheral",
                "Аудио",
                new string[] { "audio", "audiodg", "cpu", "dpc", "noverse" },
                new string[] { "latency", "gaming" },
                "peripheral/desc.md",
                977,
                985,
                "HKLM",
                @"Software\Microsoft\Windows\CurrentVersion\Audio\Parameters",
                "AudioDGCPUPercentMax",
                "REG_DWORD",
                90,
                "delete",
                null
            ));

            // 20
            res.Add(CreateTweak(
                "peripheral-audio-allow-classic-offload",
                "Разрешить классический аппаратный оффлоад аудио (AllowClassicOffload)",
                "Разрешает прямой аппаратный оффлоад аудиопотоков на звуковой процессор через менеджер политик AudioSrvPolicyManager.",
                "peripheral",
                "Аудио",
                new string[] { "audio", "offload", "hardware", "audiosrv", "noverse" },
                new string[] { "latency" },
                "peripheral/desc.md",
                988,
                995,
                "HKLM",
                @"Software\Microsoft\Windows\CurrentVersion\Audio",
                "AllowClassicOffload",
                "REG_DWORD",
                1,
                "delete",
                null
            ));

            // 21
            res.Add(CreateTweak(
                "network-wol-disable-magic-packet",
                "Отключить пробуждение по Magic Packet (*WakeOnMagicPacket)",
                "Отключает обработку специальных пакетов Wake-on-LAN (Magic Packet) сетевым адаптером, предотвращая фоновые пробуждения ПК и снижая нагрузку на контроллер.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "wol", "magic-packet", "ethernet", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                1160,
                1171,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*WakeOnMagicPacket",
                "REG_SZ",
                "0",
                "set",
                "1"
            ));

            // 22
            res.Add(CreateTweak(
                "network-wol-disable-pattern-match",
                "Отключить пробуждение по совпадению сетевых шаблонов (*WakeOnPattern)",
                "Отключает фильтрацию сетевых кадров на совпадение с зарегистрированными ОС шаблонами пробуждения, исключая ложные выходы из энергосбережения.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "wol", "pattern", "ethernet", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                1160,
                1171,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*WakeOnPattern",
                "REG_SZ",
                "0",
                "set",
                "1"
            ));

            // 23
            res.Add(CreateTweak(
                "network-wol-disable-wake-from-s5",
                "Отключить пробуждение сетевой карты из выключенного состояния S5 (WakeFromS5)",
                "Блокирует подачу дежурного питания на чип сетевой карты для пробуждения из мягкого отключения S5 Soft Off.",
                "network",
                "Сетевой адаптер",
                new string[] { "network", "wol", "s5", "power", "noverse" },
                new string[] { "latency" },
                "network/desc.md",
                1160,
                1171,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "WakeFromS5",
                "REG_SZ",
                "0",
                "set",
                "2"
            ));

            // 24
            res.Add(CreateTweak(
                "network-vmq-disable-vlan-filtering",
                "Отключить фильтрацию VLAN для очередей VMQ (*VMQVlanFiltering)",
                "Отключает аппаратную фильтрацию тегов VLAN в заголовках Ethernet-пакетов для VMQ, экономя такты контроллера.",
                "network",
                "Виртуализация",
                new string[] { "network", "vmq", "vlan", "offload", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                1573,
                1597,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*VMQVlanFiltering",
                "REG_SZ",
                "0",
                "set",
                "1"
            ));

            // 25
            res.Add(CreateTweak(
                "network-vmq-disable-lookahead-split",
                "Отключить разделение буферов приема VMQ (*VMQLookaheadSplit)",
                "Отключает разделение заголовков и данных пакетов на опережающие буферы VMQ Lookahead Split, снижая фрагментацию памяти сетевого адаптера.",
                "network",
                "Виртуализация",
                new string[] { "network", "vmq", "lookahead", "buffers", "noverse" },
                new string[] { "latency", "gaming" },
                "network/desc.md",
                1573,
                1597,
                "HKLM",
                @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                "*VMQLookaheadSplit",
                "REG_SZ",
                "0",
                "set",
                "1"
            ));

            return res;
        }

        static Dictionary<string, object> CreateTweak(
            string id,
            string nameRu,
            string descRu,
            string category,
            string group,
            string[] tags,
            string[] presets,
            string sourceFile,
            int lineFrom,
            int lineTo,
            string hive,
            string path,
            string name,
            string type,
            object applyVal,
            string revertMode,
            object revertVal)
        {
            var tweak = new Dictionary<string, object>();
            tweak["id"] = id;
            tweak["name_ru"] = nameRu;
            tweak["description_ru"] = descRu;
            tweak["category"] = category;
            tweak["group"] = group;
            tweak["type"] = "toggle";
            tweak["risk"] = "low";
            tweak["tags"] = new List<string>(tags);
            tweak["presets"] = new List<string>(presets);
            tweak["min_build"] = null;
            tweak["needs_review"] = true;

            var source = new Dictionary<string, object>();
            source["file"] = sourceFile;
            source["line_from"] = lineFrom;
            source["line_to"] = lineTo;
            tweak["source"] = source;

            var regList = new List<Dictionary<string, object>>();
            var regItem = new Dictionary<string, object>();
            regItem["hive"] = hive;
            regItem["path"] = path;
            regItem["name"] = name;
            regItem["type"] = type;
            regItem["apply"] = applyVal;

            var revert = new Dictionary<string, object>();
            revert["mode"] = revertMode;
            if (revertMode == "set")
            {
                revert["value"] = revertVal;
            }
            regItem["revert"] = revert;

            regList.Add(regItem);
            tweak["registry"] = regList;

            return tweak;
        }
    }
}
