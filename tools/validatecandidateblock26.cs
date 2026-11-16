using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

class Candidate
{
    public string id;
    public string file;
    public int from;
    public int to;
    public string hive;
    public string path;
    public string name;
    public object apply;
    public string revMode;
    public string name_ru;
    public string cat;
}

class Program
{
    static int Main()
    {
        string root = @"C:\Users\User\Documents\win-tweaker";
        string srcDir = Path.Combine(root, @"data\source\win-config");
        string tweaksPath = Path.Combine(root, @"data\tweaks.json");

        var serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;
        var existingTweaks = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksPath, Encoding.UTF8));

        var existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingRegKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in existingTweaks)
        {
            existingIds.Add(t["id"].ToString());
            if (t.ContainsKey("registry") && t["registry"] is System.Collections.ArrayList)
            {
                var regList = t["registry"] as System.Collections.ArrayList;
                foreach (Dictionary<string, object> r in regList)
                {
                    existingRegKeys.Add(r["hive"] + "\\" + r["path"] + "\\" + r["name"]);
                }
            }
        }

        var candidates = new List<Candidate>
        {
            // Peripheral (AutoPlay & Printers)
            new Candidate { id = "peripheral-autorun-no-autorun", file = "peripheral/desc.md", from = 1390, to = 1392, hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", name = "NoAutorun", apply = 1, revMode = "delete", name_ru = "Отключить автозапуск программ (AutoRun) при подключении носителей", cat = "system" },
            new Candidate { id = "peripheral-autoplay-no-drivetype-autorun", file = "peripheral/desc.md", from = 1391, to = 1393, hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", name = "NoDriveTypeAutoRun", apply = 255, revMode = "delete", name_ru = "Отключить автовоспроизведение (AutoPlay) для всех типов дисков", cat = "system" },
            new Candidate { id = "peripheral-autoplay-no-autoplay-for-non-volume", file = "peripheral/desc.md", from = 1392, to = 1394, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "NoAutoplayfornonVolume", apply = 1, revMode = "delete", name_ru = "Запретить автозапуск для устройств без файловых томов (камеры, MTP)", cat = "system" },
            new Candidate { id = "peripheral-autoplay-dont-set-checkbox", file = "peripheral/desc.md", from = 1393, to = 1395, hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\Explorer", name = "DontSetAutoplayCheckbox", apply = 1, revMode = "delete", name_ru = "Запретить AutoPlay запоминать выбор пользователя", cat = "system" },
            new Candidate { id = "peripheral-printer-disable-web-pnp-download", file = "peripheral/desc.md", from = 1831, to = 1833, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows NT\Printers", name = "DisableWebPnPDownload", apply = 1, revMode = "delete", name_ru = "Отключить загрузку драйверов печати через HTTP", cat = "system" },
            new Candidate { id = "peripheral-printer-downlevel-browse", file = "peripheral/desc.md", from = 1835, to = 1837, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows NT\Printers\Wizard", name = "Downlevel Browse", apply = 0, revMode = "delete", name_ru = "Отключить сетевой поиск принтеров ранних версий", cat = "system" },

            // Privacy & Telemetry
            new Candidate { id = "privacy-datacollection-commercial-id", file = "privacy/desc.md", from = 122, to = 124, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\DataCollection", name = "CommercialId", apply = "", revMode = "delete", name_ru = "Очистить коммерческий идентификатор телеметрии Windows", cat = "privacy" },
            new Candidate { id = "privacy-tabletpc-prevent-handwriting-error-reports", file = "privacy/desc.md", from = 125, to = 127, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\TabletPC", name = "PreventHandwritingErrorReports", apply = 1, revMode = "delete", name_ru = "Отключить отправку отчетов об ошибках распознавания рукописного ввода", cat = "privacy" },
            new Candidate { id = "privacy-tabletpc-prevent-handwriting-data-sharing", file = "privacy/desc.md", from = 125, to = 127, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\TabletPC", name = "PreventHandwritingDataSharing", apply = 1, revMode = "delete", name_ru = "Запретить передачу данных рукописного ввода в Microsoft", cat = "privacy" },
            new Candidate { id = "privacy-ie-disable-customer-improvement-program", file = "privacy/desc.md", from = 133, to = 135, hive = "HKCU", path = @"Software\Policies\Microsoft\Internet Explorer\SQM", name = "DisableCustomerImprovementProgram", apply = 0, revMode = "delete", name_ru = "Отключить программу улучшения качества ПО (CEIP) в браузере", cat = "privacy" },
            new Candidate { id = "privacy-wdi-enabled-scenario-execution-level", file = "privacy/desc.md", from = 384, to = 386, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WDI", name = "EnabledScenarioExecutionLevel", apply = 0, revMode = "delete", name_ru = "Отключить уровень выполнения сценариев диагностики WDI", cat = "privacy" },
            new Candidate { id = "privacy-startmenu-hide-recommended-section", file = "privacy/desc.md", from = 450, to = 452, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "HideRecommendedSection", apply = 1, revMode = "delete", name_ru = "Скрыть раздел «Рекомендуем» в меню Пуск Windows 11", cat = "privacy" },
            new Candidate { id = "privacy-startmenu-hide-recommended-personalized-sites", file = "privacy/desc.md", from = 451, to = 453, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "HideRecommendedPersonalizedSites", apply = 1, revMode = "delete", name_ru = "Отключить показ рекомендованных сайтов в меню Пуск", cat = "privacy" },
            new Candidate { id = "privacy-remoteassistance-allow-full-control", file = "privacy/desc.md", from = 1030, to = 1032, hive = "HKLM", path = @"Software\policies\Microsoft\Windows NT\Terminal Services", name = "fAllowFullControl", apply = 0, revMode = "delete", name_ru = "Запретить полный удаленный контроль при запросе удаленного помощника", cat = "privacy" },
            new Candidate { id = "privacy-remoteassistance-allow-unsolicited-full-control", file = "privacy/desc.md", from = 1031, to = 1033, hive = "HKLM", path = @"Software\policies\Microsoft\Windows NT\Terminal Services", name = "fAllowUnsolicitedFullControl", apply = 0, revMode = "delete", name_ru = "Запретить несанкционированное удаленное управление в помощнике", cat = "privacy" },

            // Security
            new Candidate { id = "security-credui-disable-password-reveal", file = "security/desc.md", from = 1163, to = 1165, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\CredUI", name = "DisablePasswordReveal", apply = 1, revMode = "delete", name_ru = "Скрыть кнопку отображения пароля (глазок) в окнах входа", cat = "security" },
            new Candidate { id = "security-removable-storage-deny-write", file = "security/desc.md", from = 1175, to = 1177, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\RemovableStorageDevices\{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}", name = "Deny_Write", apply = 1, revMode = "delete", name_ru = "Запретить запись на съемные диски и флеш-накопители", cat = "security" },
            new Candidate { id = "security-powershell-enable-module-logging", file = "security/desc.md", from = 670, to = 672, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging", name = "EnableModuleLogging", apply = 1, revMode = "delete", name_ru = "Включить аудит и журнал загрузки модулей PowerShell", cat = "security" },
            new Candidate { id = "security-powershell-enable-transcripting", file = "security/desc.md", from = 671, to = 673, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription", name = "EnableTranscripting", apply = 1, revMode = "delete", name_ru = "Включить транскрибирование и протоколирование сеансов PowerShell", cat = "security" },
            new Candidate { id = "security-powershell-enable-scriptblock-logging", file = "security/desc.md", from = 672, to = 674, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging", name = "EnableScriptBlockLogging", apply = 1, revMode = "delete", name_ru = "Включить глубокое логирование блоков скриптов PowerShell", cat = "security" },

            // System
            new Candidate { id = "system-explorer-disable-notification-center", file = "system/desc.md", from = 8405, to = 8407, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "DisableNotificationCenter", apply = 1, revMode = "delete", name_ru = "Отключить центр уведомлений и всплывающие оповещения", cat = "system" },
            new Candidate { id = "system-startmenu-no-mfu-programs-list", file = "system/desc.md", from = 9161, to = 9163, hive = "HKLM", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", name = "NoStartMenuMFUprogramsList", apply = 1, revMode = "delete", name_ru = "Отключить отслеживание списка часто используемых программ", cat = "system" },
            new Candidate { id = "system-startmenu-hide-recently-added-apps", file = "system/desc.md", from = 9163, to = 9165, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "HideRecentlyAddedApps", apply = 1, revMode = "delete", name_ru = "Скрыть список недавно добавленных программ в меню Пуск", cat = "system" },

            // Visibility
            new Candidate { id = "visibility-taskbar-hide-taskview-button", file = "visibility/desc.md", from = 1300, to = 1302, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "HideTaskViewButton", apply = 1, revMode = "delete", name_ru = "Скрыть кнопку «Представление задач» на панели задач", cat = "visibility" },
            new Candidate { id = "visibility-multitasking-alt-tab-filter", file = "visibility/desc.md", from = 1646, to = 1648, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "MultiTaskingAltTabFilter", apply = 3, revMode = "delete", name_ru = "Отключить отображение вкладок браузера в окне переключения Alt+Tab", cat = "visibility" }
        };

        Console.WriteLine("Total candidates for Block 26: " + candidates.Count);

        int errors = 0;
        foreach (var c in candidates)
        {
            if (existingIds.Contains(c.id))
            {
                Console.WriteLine("FAIL: Duplicate id: " + c.id);
                errors++;
            }

            string regKey = c.hive + "\\" + c.path + "\\" + c.name;
            if (existingRegKeys.Contains(regKey))
            {
                Console.WriteLine("FAIL: Duplicate registry key: " + regKey + " for " + c.id);
                errors++;
            }

            string filePath = Path.Combine(srcDir, c.file.Replace('/', '\\'));
            if (!File.Exists(filePath))
            {
                Console.WriteLine("FAIL: File not found: " + filePath);
                errors++;
                continue;
            }

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            if (c.from < 1 || c.to > lines.Length || c.from > c.to)
            {
                Console.WriteLine("FAIL: Invalid line range: " + c.from + ".." + c.to + " for " + c.id);
                errors++;
                continue;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = c.from - 1; i < c.to; i++)
            {
                sb.AppendLine(lines[i]);
            }
            string text = sb.ToString();

            // Verify action.name is in text
            if (text.IndexOf(c.name, StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.WriteLine(string.Format("FAIL: Name '{0}' not in range {1}..{2} of {3} for {4}", c.name, c.from, c.to, c.file, c.id));
                errors++;
            }

            // Verify action.path or partial path is in text
            string shortPath = c.path.Substring(c.path.LastIndexOf('\\') + 1);
            if (text.IndexOf(shortPath, StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf(c.path, StringComparison.OrdinalIgnoreCase) < 0)
            {
                Console.WriteLine(string.Format("FAIL: Path '{0}' not in range {1}..{2} of {3} for {4}", shortPath, c.from, c.to, c.file, c.id));
                errors++;
            }
        }

        if (errors == 0)
        {
            Console.WriteLine("ALL 25 CANDIDATES VALIDATED PERFECTLY! Zero errors, zero collisions.");
            return 0;
        }
        else
        {
            Console.WriteLine("Total validation errors: " + errors);
            return 1;
        }
    }
}
