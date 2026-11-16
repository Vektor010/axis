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
            // 1-4. Privacy: AppPrivacy & NetCache
            new Candidate { id = "privacy-appprivacy-force-allow-apps", file = "privacy/desc.md", from = 683, to = 685, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppPrivacy", name = "LetAppsRunInBackground_ForceAllowTheseApps", apply = "", revMode = "delete", name_ru = "Очистить список принудительно разрешенных фоновых приложений", cat = "privacy" },
            new Candidate { id = "privacy-appprivacy-force-deny-apps", file = "privacy/desc.md", from = 683, to = 685, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppPrivacy", name = "LetAppsRunInBackground_ForceDenyTheseApps", apply = "", revMode = "delete", name_ru = "Сбросить список принудительно заблокированных фоновых приложений", cat = "privacy" },
            new Candidate { id = "privacy-netcache-blockout-start-time", file = "privacy/desc.md", from = 1336, to = 1338, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "BackgroundSyncBlockOutStartTime", apply = 0, revMode = "delete", name_ru = "Сбросить время начала периода блокировки фоновой синхронизации", cat = "privacy" },
            new Candidate { id = "privacy-netcache-blockout-duration", file = "privacy/desc.md", from = 1336, to = 1338, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "BackgroundSyncBlockOutDurationMin", apply = 0, revMode = "delete", name_ru = "Сбросить длительность блокировки фоновой синхронизации", cat = "privacy" },

            // 5-18. Security: Windows Update Policies
            new Candidate { id = "security-wu-scheduled-install-third-week", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "ScheduledInstallThirdWeek", apply = 0, revMode = "delete", name_ru = "Отключить принудительную установку обновлений в третью неделю месяца", cat = "security" },
            new Candidate { id = "security-wu-scheduled-install-fourth-week", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "ScheduledInstallFourthWeek", apply = 0, revMode = "delete", name_ru = "Отключить принудительную установку обновлений в четвертую неделю месяца", cat = "security" },
            new Candidate { id = "security-wu-wuserver-policy", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "WUServer", apply = "", revMode = "delete", name_ru = "Сбросить адрес интранет-сервера служб обновлений WSUS (Machine)", cat = "security" },
            new Candidate { id = "security-wu-wustatus-server-policy", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "WUStatusServer", apply = "", revMode = "delete", name_ru = "Сбросить адрес сервера отчетов о статусе обновлений (WUStatusServer)", cat = "security" },
            new Candidate { id = "security-wu-update-service-alternate", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "UpdateServiceUrlAlternate", apply = "", revMode = "delete", name_ru = "Сбросить альтернативный адрес сервера службы обновлений (Alternate URL)", cat = "security" },
            new Candidate { id = "security-wu-au-wuserver", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "WUServer", apply = "", revMode = "delete", name_ru = "Сбросить URL интранет-сервера обновлений в службе AU", cat = "security" },
            new Candidate { id = "security-wu-au-wustatus-server", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "WUStatusServer", apply = "", revMode = "delete", name_ru = "Сбросить URL сервера статистики обновлений в службе AU", cat = "security" },
            new Candidate { id = "security-wu-au-update-service-alternate", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "UpdateServiceUrlAlternate", apply = "", revMode = "delete", name_ru = "Сбросить альтернативный URL сервера обновлений в службе AU", cat = "security" },
            new Candidate { id = "security-wu-au-fill-empty-content-urls", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "FillEmptyContentUrls", apply = 0, revMode = "delete", name_ru = "Запретить заполнение пустых URL контента обновлений в службе AU", cat = "security" },
            new Candidate { id = "security-wu-au-donot-enforce-cert-pinning", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "DoNotEnforceEnterpriseTLSCertPinningForUpdateDetection", apply = 0, revMode = "delete", name_ru = "Включить привязку TLS-сертификатов (Cert Pinning) в службе AU", cat = "security" },
            new Candidate { id = "security-wu-au-proxy-behavior", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "SetProxyBehaviorForUpdateDetection", apply = 0, revMode = "delete", name_ru = "Отключить прокси-сервер при обнаружении обновлений в службе AU", cat = "security" },
            new Candidate { id = "security-wu-au-use-wuserver", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "UseWUServer", apply = 0, revMode = "delete", name_ru = "Запретить принудительное перенаправление службы AU на сервер WSUS", cat = "security" },
            new Candidate { id = "security-wu-product-version", file = "security/desc.md", from = 496, to = 498, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "ProductVersion", apply = "", revMode = "delete", name_ru = "Сбросить принудительную версию продукта функциональных обновлений (ProductVersion)", cat = "security" },
            new Candidate { id = "security-wu-pause-quality-updates-start-time", file = "security/desc.md", from = 498, to = 500, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "PauseQualityUpdatesStartTime", apply = "", revMode = "delete", name_ru = "Сбросить время начала приостановки качественных обновлений", cat = "security" },

            // 19-23. Security: PowerShell User & Transcription Policies
            new Candidate { id = "security-powershell-user-enable-scripts", file = "security/desc.md", from = 669, to = 671, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell", name = "EnableScripts", apply = 1, revMode = "delete", name_ru = "Разрешить выполнение сценариев PowerShell для текущего пользователя", cat = "security" },
            new Candidate { id = "security-powershell-machine-transcription-output-dir", file = "security/desc.md", from = 671, to = 673, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription", name = "OutputDirectory", apply = "", revMode = "delete", name_ru = "Сбросить каталог сохранения транскриптов PowerShell (Machine)", cat = "security" },
            new Candidate { id = "security-powershell-user-transcription-output-dir", file = "security/desc.md", from = 671, to = 673, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription", name = "OutputDirectory", apply = "", revMode = "delete", name_ru = "Сбросить каталог сохранения транскриптов PowerShell (User)", cat = "security" },
            new Candidate { id = "security-powershell-user-invocation-header", file = "security/desc.md", from = 671, to = 673, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription", name = "EnableInvocationHeader", apply = 1, revMode = "delete", name_ru = "Включить заголовок вызова команд в транскриптах PowerShell (User)", cat = "security" },
            new Candidate { id = "security-powershell-user-scriptblock-invocation-logging", file = "security/desc.md", from = 672, to = 674, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging", name = "EnableScriptBlockInvocationLogging", apply = 1, revMode = "delete", name_ru = "Включить логирование вызова блоков сценариев PowerShell (User)", cat = "security" },

            // 24. Security: DeviceGuard Kernel Shadow Stacks
            new Candidate { id = "security-deviceguard-shadow-stacks-launch", file = "security/desc.md", from = 1026, to = 1028, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", name = "ConfigureKernelShadowStacksLaunch", apply = 0, revMode = "delete", name_ru = "Отключить принудительный запуск стеков теневого вызова ядра (Kernel Shadow Stacks)", cat = "security" },

            // 25. Visibility: Wallpaper Reset
            new Candidate { id = "visibility-desktop-wallpaper-clear", file = "visibility/desc.md", from = 576, to = 578, hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System", name = "Wallpaper", apply = "", revMode = "delete", name_ru = "Сбросить принудительную блокировку пути фонового рисунка рабочего стола", cat = "visibility" }
        };

        Console.WriteLine("Total candidates for Block 28: " + candidates.Count);

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
            Console.WriteLine("ALL 25 CANDIDATES FOR BLOCK 28 VALIDATED PERFECTLY! Zero errors, zero collisions.");
            return 0;
        }
        else
        {
            Console.WriteLine("Total validation errors: " + errors);
            return 1;
        }
    }
}
