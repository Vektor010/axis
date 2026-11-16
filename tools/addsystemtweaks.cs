using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace AddSystemTweaks
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
        public int from;
        public int to;
        public bool needsReview;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string json = File.ReadAllText(jsonPath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            List<Dictionary<string, object>> tweaks = serializer.Deserialize<List<Dictionary<string, object>>>(json);

            // Filter out newly added ones
            var filtered = new List<Dictionary<string, object>>();
            var newlyAddedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "system-win32-priority-separation",
                "system-mmcss-network-throttling",
                "system-mmcss-responsiveness",
                "system-mmcss-games-scheduling",
                "system-svchost-split-threshold",
                "system-disable-page-combining",
                "system-ntfs-disable-8dot3",
                "system-ntfs-disable-last-access",
                "system-long-paths-enabled",
                "system-game-mode-enabled",
                "system-no-low-disk-space-checks",
                "system-disable-storage-sense-cleanup",
                "system-disable-storage-sense-temp",
                "system-disable-storage-sense-global",
                "system-disable-wsearch-service",
                "system-disable-diagtrack-service",
                "system-disable-dmwappushservice",
                "system-disable-mapsbroker",
                "system-detailed-status"
            };

            for (int i = 0; i < tweaks.Count; i++)
            {
                string id = (string)tweaks[i]["id"];
                if (!newlyAddedIds.Contains(id))
                {
                    filtered.Add(tweaks[i]);
                }
            }
            tweaks = filtered;

            var defs = new List<TweakDef>
            {
                new TweakDef {
                    id = "system-win32-priority-separation",
                    name = "Приоритет активного приложения (Win32PrioritySeparation)",
                    desc = "Оптимизирует распределение квантов процессора, выделяя активному окну и играм в 3 раза больше процессорного времени (3:1).",
                    group = "Планировщик и кванты", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\PriorityControl", nameVal = "Win32PrioritySeparation", type = "REG_DWORD",
                    apply = 38, revMode = "set", revVal = 2, from = 1, to = 15, needsReview = true
                },
                new TweakDef {
                    id = "system-mmcss-network-throttling",
                    name = "Отключить сетевой троттлинг MMCSS (NetworkThrottlingIndex)",
                    desc = "Снимает встроенное ограничение Windows на обработку сетевых пакетов при воспроизведении медиа, устраняя лаги в онлайн-играх.",
                    group = "Планировщик и кванты", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", nameVal = "NetworkThrottlingIndex", type = "REG_DWORD",
                    apply = 4294967295L, revMode = "set", revVal = 10, from = 1195, to = 1220, needsReview = true
                },
                new TweakDef {
                    id = "system-mmcss-responsiveness",
                    name = "Максимальная отзывчивость системы (SystemResponsiveness)",
                    desc = "Отключает резервирование 20% ресурсов процессора под низкоприоритетные задачи, передавая 100% мощности активному процессу.",
                    group = "Планировщик и кванты", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", nameVal = "SystemResponsiveness", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 20, from = 1195, to = 1220, needsReview = true
                },
                new TweakDef {
                    id = "system-mmcss-games-scheduling",
                    name = "Высокий приоритет игр в MMCSS (Games Scheduling Category)",
                    desc = "Переводит профиль MMCSS Games в категорию High с наивысшим приоритетом планировщика для стабильного времени кадра.",
                    group = "Планировщик и кванты", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", nameVal = "Scheduling Category", type = "REG_SZ",
                    apply = "High", revMode = "set", revVal = "Medium", from = 1450, to = 1520, needsReview = true
                },
                new TweakDef {
                    id = "system-svchost-split-threshold",
                    name = "Группировка служб svchost (SvcHostSplitThresholdInKB)",
                    desc = "Объединяет сотни разрозненных процессов svchost в общие группы, снижая количество фоновых процессов, дескрипторов и переключений контекста CPU.",
                    group = "Память и процессы", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control", nameVal = "SvcHostSplitThresholdInKB", type = "REG_DWORD",
                    apply = 4294967295L, revMode = "set", revVal = 3670016, from = 8575, to = 8610, needsReview = true
                },
                new TweakDef {
                    id = "system-disable-page-combining",
                    name = "Отключить дедупликацию памяти (DisablePageCombining)",
                    desc = "Отключает периодическое фоновое сканирование RAM на дубликаты страниц, устраняя микрозадержки планировщика памяти.",
                    group = "Память и процессы", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", nameVal = "DisablePageCombining", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, from = 3540, to = 3550, needsReview = true
                },
                new TweakDef {
                    id = "system-ntfs-disable-8dot3",
                    name = "Отключить создание коротких имён 8.3 (NtfsDisable8dot3NameCreation)",
                    desc = "Отключает генерацию устаревших коротких имён MS-DOS в NTFS, ускоряя создание файлов и работу с каталогами на SSD.",
                    group = "Файловая система и ввод-вывод", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\FileSystem", nameVal = "NtfsDisable8dot3NameCreation", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, from = 8510, to = 8530, needsReview = true
                },
                new TweakDef {
                    id = "system-ntfs-disable-last-access",
                    name = "Отключить обновление времени последнего доступа (NtfsDisableLastAccessUpdate)",
                    desc = "Отключает запись времени доступа на диск при каждом чтении файлов, снижая нагрузку метаданных на SSD и продлевая его ресурс.",
                    group = "Файловая система и ввод-вывод", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\FileSystem", nameVal = "NtfsDisableLastAccessUpdate", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, from = 8510, to = 8530, needsReview = true
                },
                new TweakDef {
                    id = "system-long-paths-enabled",
                    name = "Поддержка длинных путей файлов (LongPathsEnabled)",
                    desc = "Снимает устаревшее ограничение длины пути в 260 символов (MAX_PATH), предотвращая сбои при копировании глубоко вложенных файлов.",
                    group = "Файловая система и ввод-вывод", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\FileSystem", nameVal = "LongPathsEnabled", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, from = 8510, to = 8525, needsReview = true
                },
                new TweakDef {
                    id = "system-game-mode-enabled",
                    name = "Игровой режим Windows (AutoGameModeEnabled)",
                    desc = "Активирует Resource Manager игрового режима Windows для приоритетной диспетчеризации ресурсов GPU и CPU при запуске игр.",
                    group = "Игровой режим", category = "system", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\GameBar", nameVal = "AutoGameModeEnabled", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, from = 4430, to = 4445, needsReview = true
                },
                new TweakDef {
                    id = "system-disable-storage-sense-global",
                    name = "Отключить фоновую службу контроля памяти (Storage Sense)",
                    desc = "Отключает глобальную фоновую службу контроля и автоочистки накопителей через групповую политику.",
                    group = "Диск и уведомления", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\StorageSense", nameVal = "AllowStorageSenseGlobal", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, from = 8673, to = 8682, needsReview = true
                },
                new TweakDef {
                    id = "system-disable-storage-sense-temp",
                    name = "Отключить автоудаление временных файлов (Storage Sense Temp)",
                    desc = "Запрещает политике Storage Sense автоматически очищать временные файлы программ и системы.",
                    group = "Диск и уведомления", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\StorageSense", nameVal = "AllowStorageSenseTemporaryFilesCleanup", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, from = 8673, to = 8682, needsReview = true
                },
                new TweakDef {
                    id = "system-disable-wsearch-service",
                    name = "Отключить службу индексации поиска Windows (WSearch)",
                    desc = "Останавливает фоновую дисковую индексацию SearchIndexer.exe, устраняя периодическую фоновую нагрузку на накопитель.",
                    group = "Службы и процессы", category = "system", risk = "medium", preset = "",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\WSearch", nameVal = "Start", type = "REG_DWORD",
                    apply = 4, revMode = "set", revVal = 2, from = 6736, to = 7200, needsReview = true
                },
                new TweakDef {
                    id = "system-disable-diagtrack-service",
                    name = "Отключить службу телеметрии DiagTrack",
                    desc = "Отключает службу Connected User Experiences and Telemetry, устраняя скрытый фоновый сбор диагностических отчётов.",
                    group = "Службы и процессы", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\DiagTrack", nameVal = "Start", type = "REG_DWORD",
                    apply = 4, revMode = "set", revVal = 2, from = 6736, to = 7100, needsReview = true
                },
                new TweakDef {
                    id = "system-disable-dmwappushservice",
                    name = "Отключить службу WAP Push телеметрии (dmwappushservice)",
                    desc = "Отключает фоновый транспорт телеметрических сообщений WAP Push в Windows.",
                    group = "Службы и процессы", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\dmwappushservice", nameVal = "Start", type = "REG_DWORD",
                    apply = 4, revMode = "set", revVal = 3, from = 6736, to = 7100, needsReview = true
                },
                new TweakDef {
                    id = "system-disable-mapsbroker",
                    name = "Отключить службу загруженных карт (MapsBroker)",
                    desc = "Отключает неиспользуемую службу менеджера карт Windows для настольных и игровых систем.",
                    group = "Службы и процессы", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\MapsBroker", nameVal = "Start", type = "REG_DWORD",
                    apply = 4, revMode = "set", revVal = 2, from = 6736, to = 6960, needsReview = true
                },
                new TweakDef {
                    id = "system-detailed-status",
                    name = "Подробные сообщения загрузки и выключения (VerboseStatus)",
                    desc = "Отображает конкретные шаги запуска и остановки служб Windows вместо зависающей стандартной анимации.",
                    group = "Информативность системы", category = "system", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", nameVal = "VerboseStatus", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, from = 9400, to = 9415, needsReview = true
                }
            };

            foreach (var d in defs)
            {
                var t = new Dictionary<string, object>();
                t["id"] = d.id;
                t["name_ru"] = d.name;
                t["description_ru"] = d.desc;
                t["group"] = d.group;
                t["category"] = d.category;
                t["type"] = "toggle";
                t["risk"] = d.risk;
                var presets = new List<string>();
                if (!string.IsNullOrEmpty(d.preset)) presets.Add(d.preset);
                t["presets"] = presets;
                t["min_build"] = null;
                t["needs_review"] = d.needsReview;

                var src = new Dictionary<string, object>();
                src["file"] = "system/desc.md";
                src["line_from"] = d.from;
                src["line_to"] = d.to;
                t["source"] = src;

                var regList = new List<Dictionary<string, object>>();
                var reg = new Dictionary<string, object>();
                reg["hive"] = d.hive;
                reg["path"] = d.path;
                reg["name"] = d.nameVal;
                reg["type"] = d.type;
                reg["apply"] = d.apply;
                reg["apply_block"] = "Enabled";

                var rev = new Dictionary<string, object>();
                rev["mode"] = d.revMode;
                rev["value"] = d.revVal;
                reg["revert"] = rev;

                regList.Add(reg);
                t["registry"] = regList;

                tweaks.Add(t);
                Console.WriteLine("Added tweak: " + d.id);
            }

            string updatedJson = serializer.Serialize(tweaks);
            string formatted = FormatJson(updatedJson);
            File.WriteAllText(jsonPath, formatted, Encoding.UTF8);
            Console.WriteLine("Done! Total tweaks: " + tweaks.Count);
        }

        static string FormatJson(string json)
        {
            var sb = new StringBuilder();
            int indent = 0;
            bool inQuotes = false;
            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                if (ch == '\\' && inQuotes)
                {
                    sb.Append(ch);
                    if (i + 1 < json.Length) sb.Append(json[++i]);
                    continue;
                }
                if (ch == '"') inQuotes = !inQuotes;
                if (!inQuotes)
                {
                    if (ch == '{' || ch == '[')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        indent += 2;
                        sb.Append(new string(' ', indent));
                        continue;
                    }
                    if (ch == '}' || ch == ']')
                    {
                        sb.Append("\r\n");
                        indent -= 2;
                        if (indent < 0) indent = 0;
                        sb.Append(new string(' ', indent));
                        sb.Append(ch);
                        continue;
                    }
                    if (ch == ',')
                    {
                        sb.Append(ch);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent));
                        continue;
                    }
                    if (ch == ':')
                    {
                        sb.Append(": ");
                        continue;
                    }
                    if (char.IsWhiteSpace(ch)) continue;
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
