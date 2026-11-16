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
                new TweakDef {
                    id = "cleanup-directx-shader-cache",
                    name = "Очистить кэш шейдеров DirectX и NVIDIA (D3DSCache)",
                    desc = "Очищает скомпилированные кэши шейдеров DirectX и видеокарт NVIDIA (DXCache, GLCache, NV_Cache) для устранения микрофризов после смены драйвера.",
                    group = "Кэши и шейдеры", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Direct3D", nameVal = "D3DSCache", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 68, to = 84, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-dns-cache",
                    name = "Сброс и очистка DNS-кэша (DnsClientCache)",
                    desc = "Сбрасывает локальный кэш сопоставления IP-адресов службы DNS Client для устранения сетевых задержек и недоступности сайтов.",
                    group = "Сеть и DNS", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\Dnscache", nameVal = "DnsClientCache", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 208, to = 216, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-windows-update-cache",
                    name = "Очистить кэш обновлений Windows (SoftwareDistribution)",
                    desc = "Очищает загруженные поврежденные и устаревшие установочные пакеты из каталога SoftwareDistribution\\Download, освобождая гигабайты места.",
                    group = "Системный диск", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", nameVal = "SoftwareDistribution", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 246, to = 261, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-temporary-files",
                    name = "Очистить временные файлы (Temp)",
                    desc = "Удаляет временный мусор сторонних программ, установочные логи и кэшированные остатки файлов из пользовательской и системной папок Temp.",
                    group = "Системный диск", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Temp", nameVal = "Temp", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 185, to = 194, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-temporary-internet-files",
                    name = "Очистить временные файлы интернета (INetCache)",
                    desc = "Удаляет устаревшие кэшированные файлы WinINet и встроенных веб-компонентов из каталога INetCache.",
                    group = "Сеть и DNS", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Cache", nameVal = "INetCache", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 162, to = 173, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-delivery-optimization",
                    name = "Очистить файлы оптимизации доставки (DeliveryOptimization)",
                    desc = "Удаляет накопленные фрагменты фоновых обновлений службы Delivery Optimization (DoSvc), освобождая гигабайты свободного места.",
                    group = "Системный диск", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", nameVal = "DeliveryOptimization", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 174, to = 184, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-bsod-dumps",
                    name = "Очистить аварийные дампы ядра BSoD (LiveKernelReports)",
                    desc = "Удаляет аварийные дампы памяти ядра (MEMORY.DMP, Minidump) и журналы сбоев драйверов LiveKernelReports.",
                    group = "Журналы и дампы", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\CrashControl", nameVal = "LiveKernelReports", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 280, to = 292, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-wer-files",
                    name = "Очистить отчеты об ошибках Windows (WER)",
                    desc = "Удаляет накопленные архивы отчетов об ошибках и телеметрии падений приложений службы Windows Error Reporting.",
                    group = "Журналы и дампы", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", nameVal = "WER", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 217, to = 226, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-system-logs",
                    name = "Очистить системные журналы и логи (Logs)",
                    desc = "Очищает накопленные устаревшие лог-файлы CBS, DISM, NetSetup и диагностики WMI.",
                    group = "Журналы и дампы", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\WMI", nameVal = "Logs", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 334, to = 345, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-recycle-bin",
                    name = "Очистить корзину всех дисков (RecycleBin)",
                    desc = "Выполняет принудительную очистку системных корзин $Recycle.Bin на всех смонтированных томах.",
                    group = "Системный диск", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\BitBucket", nameVal = "RecycleBin", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 85, to = 96, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-thumbnail-cache",
                    name = "Сброс и очистка кэша эскизов (Thumbnail)",
                    desc = "Сбрасывает поврежденные файлы кэша миниатюр Проводника (*.db), освобождая память и исправляя отображение иконок.",
                    group = "Проводник и оболочка", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", nameVal = "Thumbnail", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 262, to = 270, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-font-cache",
                    name = "Сброс кэша шрифтов Windows (FontCache)",
                    desc = "Очищает поврежденный кэш FNTCACHE.DAT подсистемы шрифтов, устраняя артефакты и сбои отрисовки текста в Windows.",
                    group = "Проводник и оболочка", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Services\FontCache", nameVal = "FontCache", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 152, to = 161, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-mui-cache",
                    name = "Очистить кэш интерфейсных строк (MuiCache)",
                    desc = "Удаляет кэшированные имена и пути запускавшихся приложений из разделов Shell MuiCache реестра.",
                    group = "Проводник и оболочка", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell", nameVal = "MuiCache", type = "REG_SZ",
                    apply = "", revMode = "set", revVal = "", srcFile = "cleanup/desc.md", from = 142, to = 151, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-background-history",
                    name = "Очистить историю обоев рабочего стола (BackgroundHistoryPath0)",
                    desc = "Очищает историю последних пяти обоев рабочего стола в окне персонализации Windows.",
                    group = "Проводник и оболочка", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", nameVal = "BackgroundHistoryPath0", type = "REG_SZ",
                    apply = "", revMode = "set", revVal = "", srcFile = "cleanup/desc.md", from = 112, to = 124, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-prefetch-files",
                    name = "Очистить файлы упреждающего чтения (Prefetch)",
                    desc = "Очищает устаревшие файлы трассировки запуска приложений из папки C:\\Windows\\Prefetch на скоростных NVMe SSD.",
                    group = "Системный диск", category = "cleanup", risk = "low", preset = "recommended",
                    hive = "HKLM", path = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", nameVal = "Prefetch", type = "REG_DWORD",
                    apply = 1, revMode = "set", revVal = 0, srcFile = "cleanup/desc.md", from = 271, to = 279, needsReview = true
                },
                new TweakDef {
                    id = "cleanup-winsxs-resetbase",
                    name = "Очистка базы компонентов WinSxS (DisableResetbase)",
                    desc = "Разрешает удаление всех замененных и устаревших версий компонентов в WinSxS через DISM /ResetBase, освобождая гигабайты диска.",
                    group = "Системный диск", category = "cleanup", risk = "medium", preset = "recommended",
                    hive = "HKLM", path = @"Software\Microsoft\Windows\CurrentVersion\SideBySide\Configuration", nameVal = "DisableResetbase", type = "REG_DWORD",
                    apply = 0, revMode = "set", revVal = 1, srcFile = "cleanup/desc.md", from = 38, to = 44, needsReview = true
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
