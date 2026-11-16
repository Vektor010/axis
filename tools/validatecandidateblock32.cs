using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Validator
{
    class Program
    {
        class CandidateSpec
        {
            public string Id;
            public string NameRu;
            public string DescRu;
            public string Category;
            public string Subcategory;
            public string SourceRel;
            public int LineFrom;
            public int LineTo;
            public string Root;
            public string Path;
            public string Name;
            public string Type;
            public object Apply;
            public string RevertMode;
            public object RevertValue;
        }

        static void Main(string[] args)
        {
            string rootDir = @"C:\Users\User\Documents\win-tweaker";
            string tweaksJsonPath = Path.Combine(rootDir, @"data\tweaks.json");
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var currentTweaks = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJsonPath, Encoding.UTF8));

            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> existingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var t in currentTweaks)
            {
                existingIds.Add(t["id"] as string);
                var regList = t["registry"] as System.Collections.ArrayList;
                if (regList != null)
                {
                    foreach (Dictionary<string, object> r in regList)
                    {
                        string p = r.ContainsKey("path") ? r["path"] as string : "";
                        string n = r.ContainsKey("name") ? r["name"] as string : "";
                        existingPaths.Add(p + "\\" + n);
                    }
                }
            }

            var candidates = GetCandidates();
            Console.WriteLine("Кандидатов на проверку: " + candidates.Count);

            bool allPass = true;
            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];
                Console.WriteLine(string.Format("[{0}] Проверка: {1} ({2})", i + 1, c.Id, c.Name));

                // 1. Проверка уникальности ID
                if (existingIds.Contains(c.Id))
                {
                    Console.WriteLine("  [FAIL] Дубликат ID: " + c.Id);
                    allPass = false;
                }

                // 2. Проверка коллизий пути
                string fullKey = c.Path + "\\" + c.Name;
                if (existingPaths.Contains(fullKey))
                {
                    Console.WriteLine("  [FAIL] Коллизия ключа реестра: " + fullKey);
                    allPass = false;
                }

                // 3. Проверка типа Apply
                if (c.Type == "REG_DWORD" && !(c.Apply is int))
                {
                    Console.WriteLine("  [FAIL] REG_DWORD Apply должен быть int: " + c.Apply);
                    allPass = false;
                }
                if (c.Type == "REG_SZ" && !(c.Apply is string))
                {
                    Console.WriteLine("  [FAIL] REG_SZ Apply должен быть string: " + c.Apply);
                    allPass = false;
                }

                // 4. Проверка Revert
                if (c.RevertMode == "set")
                {
                    if (c.RevertValue == null)
                    {
                        Console.WriteLine("  [FAIL] mode=set требует revert.value!");
                        allPass = false;
                    }
                    if (c.Apply.Equals(c.RevertValue))
                    {
                        Console.WriteLine("  [FAIL] apply == revert.value: " + c.Apply);
                        allPass = false;
                    }
                }

                // 5. Проверка существования файла источника и диапазона строк
                string fullSourcePath = Path.Combine(rootDir, @"data\source\win-config", c.SourceRel.Replace('/', '\\'));
                if (!File.Exists(fullSourcePath))
                {
                    Console.WriteLine("  [FAIL] Источник не найден: " + fullSourcePath);
                    allPass = false;
                }
                else
                {
                    var lines = File.ReadAllLines(fullSourcePath);
                    if (c.LineFrom < 1 || c.LineTo > lines.Length || c.LineFrom > c.LineTo)
                    {
                        Console.WriteLine(string.Format("  [FAIL] Неверный диапазон строк: {0}..{1} (всего {2})", c.LineFrom, c.LineTo, lines.Length));
                        allPass = false;
                    }
                    else
                    {
                        // Проверяем, что имя параметра упоминается в этом диапазоне строк
                        bool foundName = false;
                        for (int lineIdx = c.LineFrom - 1; lineIdx < c.LineTo; lineIdx++)
                        {
                            if (lines[lineIdx].Contains(c.Name))
                            {
                                foundName = true;
                                break;
                            }
                        }
                        if (!foundName)
                        {
                            Console.WriteLine(string.Format("  [FAIL] Имя параметра '{0}' не найдено в строках {1}..{2} файла {3}", c.Name, c.LineFrom, c.LineTo, c.SourceRel));
                            allPass = false;
                        }
                    }
                }
            }

            if (allPass)
            {
                Console.WriteLine("\n[SUCCESS] ВСЕ 25 КАНДИДАТОВ УСПЕШНО ПРОШЛИ ПРЕДВАРИТЕЛЬНУЮ ВАЛИДАЦИЮ!");
            }
            else
            {
                Console.WriteLine("\n[ERROR] ОБНАРУЖЕНЫ ОШИБКИ В КАНДИДАТАХ!");
            }
        }

        static List<CandidateSpec> GetCandidates()
        {
            var list = new List<CandidateSpec>();

            // 1. MMCSS
            list.Add(new CandidateSpec {
                Id = "system-mmcss-no-lazy-mode",
                NameRu = "Отключить ленивый режим MMCSS (NoLazyMode)",
                DescRu = "Отключает переход планировщика MMCSS (Multimedia Class Scheduler Service) в энергосберегающий режим ожидания (Lazy Mode), предотвращая задержки при возобновлении обработки аудиопотока и мультимедийных пакетов.",
                Category = "system",
                Subcategory = "MMCSS",
                SourceRel = "system/desc.md",
                LineFrom = 1196,
                LineTo = 1212,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                Name = "NoLazyMode",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 2. MMCSS
            list.Add(new CandidateSpec {
                Id = "system-mmcss-idle-detection-cycles",
                NameRu = "Максимальные циклы детекции простоя MMCSS (IdleDetectionCycles)",
                DescRu = "Устанавливает максимальное количество циклов планировщика MMCSS перед определением потока как простаивающего (31 цикл вместо стандартных 2), продлевая окно гарантированного квантования времени.",
                Category = "system",
                Subcategory = "MMCSS",
                SourceRel = "system/desc.md",
                LineFrom = 1196,
                LineTo = 1212,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                Name = "IdleDetectionCycles",
                Type = "REG_DWORD",
                Apply = 31,
                RevertMode = "delete"
            });

            // 3. MMCSS
            list.Add(new CandidateSpec {
                Id = "system-mmcss-lazy-mode-timeout",
                NameRu = "Увеличить таймаут ленивого режима MMCSS (LazyModeTimeout)",
                DescRu = "Задает предельный таймаут активности планировщика перед активацией ленивого режима MMCSS в микросекундах (10000000 мкс), снижая вероятность просадки приоритетов при прерывистом рендеринге.",
                Category = "system",
                Subcategory = "MMCSS",
                SourceRel = "system/desc.md",
                LineFrom = 1196,
                LineTo = 1212,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                Name = "LazyModeTimeout",
                Type = "REG_DWORD",
                Apply = 10000000,
                RevertMode = "delete"
            });

            // 4. MMCSS
            list.Add(new CandidateSpec {
                Id = "system-mmcss-scheduler-period",
                NameRu = "Минимальный период планировщика MMCSS (SchedulerPeriod)",
                DescRu = "Устанавливает минимально допустимый период планирования мультимедийных задач MMCSS (50000 сотых долей микросекунды / 5 мс вместо дефолтных 10 мс), повышая отзывчивость планирования времени.",
                Category = "system",
                Subcategory = "MMCSS",
                SourceRel = "system/desc.md",
                LineFrom = 1196,
                LineTo = 1212,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                Name = "SchedulerPeriod",
                Type = "REG_DWORD",
                Apply = 50000,
                RevertMode = "delete"
            });

            // 5. MMCSS
            list.Add(new CandidateSpec {
                Id = "system-mmcss-max-threads-process",
                NameRu = "Увеличить лимит потоков MMCSS на процесс (MaxThreadsPerProcess)",
                DescRu = "Повышает максимальное количество регистрируемых мультимедийных потоков AvSetMmThreadCharacteristics на один процесс до аппаратного предела драйвера (128 вместо дефолтных 32).",
                Category = "system",
                Subcategory = "MMCSS",
                SourceRel = "system/desc.md",
                LineFrom = 1196,
                LineTo = 1212,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                Name = "MaxThreadsPerProcess",
                Type = "REG_DWORD",
                Apply = 128,
                RevertMode = "delete"
            });

            // 6. MMCSS
            list.Add(new CandidateSpec {
                Id = "system-mmcss-max-threads-total",
                NameRu = "Увеличить общий пул потоков MMCSS (MaxThreadsTotal)",
                DescRu = "Увеличивает суммарный пул активных потоков планировщика MMCSS для всех приложений в системе до 65535 (дефолт 256), предотвращая отказ в регистрации мультимедийных задач при высокой нагрузке.",
                Category = "system",
                Subcategory = "MMCSS",
                SourceRel = "system/desc.md",
                LineFrom = 1196,
                LineTo = 1212,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                Name = "MaxThreadsTotal",
                Type = "REG_DWORD",
                Apply = 65535,
                RevertMode = "delete"
            });

            // 7. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-disable-draw-list-caching",
                NameRu = "Отключить кэширование списков отрисовки DWM (DisableDrawListCaching)",
                DescRu = "Отключает кэширование буферов DrawList в DWM, принуждая немедленную передачу отрисованных примитивов в конвейер композиции без задержек ожидания инвалидации кэша.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2800,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "DisableDrawListCaching",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 8. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-disable-projected-shadows",
                NameRu = "Отключить проецируемые тени окон DWM (DisableProjectedShadows)",
                DescRu = "Отключает генерацию сцен CompositionProjectedShadow в DWM, экономя ресурсы шейдерных блоков GPU при перемещении окон.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2800,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "DisableProjectedShadows",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 9. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-backdrop-blur-caching",
                NameRu = "Включить оптимизированный кэш размытия фона DWM (EnableBackdropBlurCaching)",
                DescRu = "Активирует повторное использование вычисленного размытия заднего фона (Backdrop Blur) в dwmcore, исключая избыточный рендеринг гауссова размытия на каждом кадре.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2800,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnableBackdropBlurCaching",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 10. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-enable-effect-caching",
                NameRu = "Включить кэширование визуальных эффектов DWM (EnableEffectCaching)",
                DescRu = "Включает внутреннее кэширование эффектов композитора Windows для ускорения отрисовки статических элементов интерфейса.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2726,
                LineTo = 2800,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "EnableEffectCaching",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 11. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-force-disable-mode-change-animation",
                NameRu = "Отключить анимацию смены видеорежима дисплея (ForceDisableModeChangeAnimation)",
                DescRu = "Отключает визуальную задержку затемнения и анимацию перехода при смене разрешения, герцовки или топологии мониторов в Windows 11.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2800,
                LineTo = 2850,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "ForceDisableModeChangeAnimation",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 12. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-disable-hologram-compositor",
                NameRu = "Отключить композитор голографических дисплеев (DisableHologramCompositor)",
                DescRu = "Пропускает регистрацию слушателя голографических устройств и специализированных шлемов в uDWM, исключая лишние системные вызовы.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2820,
                LineTo = 2850,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "DisableHologramCompositor",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 13. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-child-window-dpi-isolation",
                NameRu = "Изоляция масштабирования DPI дочерних окон (ChildWindowDpiIsolation)",
                DescRu = "Активирует изоляцию контекста DPI для дочерних окон в подсистеме win32kfull, предотвращая визуальные артефакты и пересчет координат в смешанных DPI-средах.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2830,
                LineTo = 2860,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "ChildWindowDpiIsolation",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 14. DWM
            list.Add(new CandidateSpec {
                Id = "system-dwm-disable-device-bitmaps",
                NameRu = "Отключить устаревшие битовые карты устройств DWM (DisableDeviceBitmaps)",
                DescRu = "Отключает использование Device Bitmaps в win32kfull, форсируя работу композиции исключительно через современные поверхности DirectComposition.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2830,
                LineTo = 2860,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm",
                Name = "DisableDeviceBitmaps",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 15. DWM Scene
            list.Add(new CandidateSpec {
                Id = "system-dwm-scene-enable-shadow",
                NameRu = "Отключить тени сцен композитора (Scene EnableShadow)",
                DescRu = "Отключает отрисовку теней в подсистеме Dwm\\Scene, снижая нагрузку на пиксельные шейдеры рабочего стола.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2875,
                LineTo = 2900,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                Name = "EnableShadow",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 16. DWM Scene
            list.Add(new CandidateSpec {
                Id = "system-dwm-scene-image-processing-8bit",
                NameRu = "Форсировать 8-битную обработку изображений сцены (ImageProcessing8bit)",
                DescRu = "Ограничивает внутреннюю точность обработки промежуточных растров сцены DWM до 8 бит на канал, снижая пропускную способность памяти GPU.",
                Category = "system",
                Subcategory = "DWM",
                SourceRel = "system/desc.md",
                LineFrom = 2875,
                LineTo = 2900,
                Root = "HKLM",
                Path = @"SOFTWARE\Microsoft\Windows\Dwm\Scene",
                Name = "ImageProcessing8bit",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 17. Peripheral Audio
            list.Add(new CandidateSpec {
                Id = "peripheral-audio-skip-rt-heap",
                NameRu = "Использовать выделенную кучу реального времени звука (SkipRTHeap)",
                DescRu = "Переключает подсистему audiodg на выделенную кучу реального времени (RT Heap) вместо стандартной кучи C++, снижая вариативность задержек и джиттер при выделении аудиопамяти.",
                Category = "peripheral",
                Subcategory = "Audio",
                SourceRel = "peripheral/desc.md",
                LineFrom = 948,
                LineTo = 985,
                Root = "HKLM",
                Path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                Name = "SkipRTHeap",
                Type = "REG_DWORD",
                Apply = 0,
                RevertMode = "delete"
            });

            // 18. Peripheral Audio
            list.Add(new CandidateSpec {
                Id = "peripheral-audio-suppress-glitch-logging",
                NameRu = "Подавить логирование глитчей аудиомоста (SuppressBridgeTargetGlitchLogging)",
                DescRu = "Отключает запись в журнал событий при возникновении микроглитчей в аудиомосте CEndpointInstance, предотвращая всплески дискового I/O в моменты задержек звука.",
                Category = "peripheral",
                Subcategory = "Audio",
                SourceRel = "peripheral/desc.md",
                LineFrom = 948,
                LineTo = 985,
                Root = "HKLM",
                Path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                Name = "SuppressBridgeTargetGlitchLogging",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 19. Peripheral Audio
            list.Add(new CandidateSpec {
                Id = "peripheral-audio-max-cpu-percent",
                NameRu = "Максимальный лимит CPU для звуковой помпы (AudioDGCPUPercentMax)",
                DescRu = "Увеличивает предельный бюджет процессорного времени для потока помпы CAudioPump с 40% до 90% периода, исключая выпадения звука (DPC dropouts) под высокой игровой нагрузкой.",
                Category = "peripheral",
                Subcategory = "Audio",
                SourceRel = "peripheral/desc.md",
                LineFrom = 977,
                LineTo = 985,
                Root = "HKLM",
                Path = @"Software\Microsoft\Windows\CurrentVersion\Audio\Parameters",
                Name = "AudioDGCPUPercentMax",
                Type = "REG_DWORD",
                Apply = 90,
                RevertMode = "delete"
            });

            // 20. Peripheral Audio
            list.Add(new CandidateSpec {
                Id = "peripheral-audio-allow-classic-offload",
                NameRu = "Разрешить классический аппаратный оффлоад аудио (AllowClassicOffload)",
                DescRu = "Разрешает прямой аппаратный оффлоад аудиопотоков на звуковой процессор через менеджер политик AudioSrvPolicyManager.",
                Category = "peripheral",
                Subcategory = "Audio",
                SourceRel = "peripheral/desc.md",
                LineFrom = 988,
                LineTo = 995,
                Root = "HKLM",
                Path = @"Software\Microsoft\Windows\CurrentVersion\Audio",
                Name = "AllowClassicOffload",
                Type = "REG_DWORD",
                Apply = 1,
                RevertMode = "delete"
            });

            // 21. Network WoL
            list.Add(new CandidateSpec {
                Id = "network-wol-disable-magic-packet",
                NameRu = "Отключить пробуждение по Magic Packet (*WakeOnMagicPacket)",
                DescRu = "Отключает обработку специальных пакетов Wake-on-LAN (Magic Packet) сетевым адаптером, предотвращая фоновые пробуждения ПК и снижая нагрузку на контроллер.",
                Category = "network",
                Subcategory = "Ethernet",
                SourceRel = "network/desc.md",
                LineFrom = 1160,
                LineTo = 1171,
                Root = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*WakeOnMagicPacket",
                Type = "REG_SZ",
                Apply = "0",
                RevertMode = "set",
                RevertValue = "1"
            });

            // 22. Network WoL
            list.Add(new CandidateSpec {
                Id = "network-wol-disable-pattern-match",
                NameRu = "Отключить пробуждение по совпадению сетевых шаблонов (*WakeOnPattern)",
                DescRu = "Отключает фильтрацию сетевых кадров на совпадение с зарегистрированными ОС шаблонами пробуждения, исключая ложные выходы из энергосбережения.",
                Category = "network",
                Subcategory = "Ethernet",
                SourceRel = "network/desc.md",
                LineFrom = 1160,
                LineTo = 1171,
                Root = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*WakeOnPattern",
                Type = "REG_SZ",
                Apply = "0",
                RevertMode = "set",
                RevertValue = "1"
            });

            // 23. Network WoL
            list.Add(new CandidateSpec {
                Id = "network-wol-disable-wake-from-s5",
                NameRu = "Отключить пробуждение сетевой карты из выключенного состояния S5 (WakeFromS5)",
                DescRu = "Блокирует подачу дежурного питания на чип сетевой карты для пробуждения из мягкого отключения S5 Soft Off.",
                Category = "network",
                Subcategory = "Ethernet",
                SourceRel = "network/desc.md",
                LineFrom = 1160,
                LineTo = 1171,
                Root = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "WakeFromS5",
                Type = "REG_SZ",
                Apply = "0",
                RevertMode = "set",
                RevertValue = "2"
            });

            // 24. Network VMQ
            list.Add(new CandidateSpec {
                Id = "network-vmq-disable-vlan-filtering",
                NameRu = "Отключить фильтрацию VLAN для очередей VMQ (*VMQVlanFiltering)",
                DescRu = "Отключает аппаратную фильтрацию тегов VLAN в заголовках Ethernet-пакетов для VMQ, экономя такты контроллера.",
                Category = "network",
                Subcategory = "Virtualization",
                SourceRel = "network/desc.md",
                LineFrom = 1573,
                LineTo = 1597,
                Root = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*VMQVlanFiltering",
                Type = "REG_SZ",
                Apply = "0",
                RevertMode = "set",
                RevertValue = "1"
            });

            // 25. Network VMQ
            list.Add(new CandidateSpec {
                Id = "network-vmq-disable-lookahead-split",
                NameRu = "Отключить разделение буферов приема VMQ (*VMQLookaheadSplit)",
                DescRu = "Отключает разделение заголовков и данных пакетов на опережающие буферы VMQ Lookahead Split, снижая фрагментацию памяти сетевого адаптера.",
                Category = "network",
                Subcategory = "Virtualization",
                SourceRel = "network/desc.md",
                LineFrom = 1573,
                LineTo = 1597,
                Root = "HKLM",
                Path = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\00XX",
                Name = "*VMQLookaheadSplit",
                Type = "REG_SZ",
                Apply = "0",
                RevertMode = "set",
                RevertValue = "1"
            });

            return list;
        }
    }
}
