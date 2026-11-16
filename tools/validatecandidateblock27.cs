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
            // Network
            new Candidate { id = "network-nca-passive-mode", file = "network/desc.md", from = 994, to = 996, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityAssistant", name = "PassiveMode", apply = 1, revMode = "delete", name_ru = "Включить пассивный режим помощника сетевого подключения (NCA)", cat = "network" },

            // Privacy & Telemetry
            new Candidate { id = "privacy-datacollection-user-allow-telemetry", file = "privacy/desc.md", from = 112, to = 114, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\DataCollection", name = "AllowTelemetry", apply = 0, revMode = "delete", name_ru = "Отключить передачу диагностических данных пользователя (AllowTelemetry HKCU)", cat = "privacy" },
            new Candidate { id = "privacy-appprivacy-background-apps-user-control", file = "privacy/desc.md", from = 683, to = 685, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppPrivacy", name = "LetAppsRunInBackground_UserInControlOfTheseApps", apply = 0, revMode = "delete", name_ru = "Запретить фоновую работу приложений по выбору пользователя", cat = "privacy" },
            new Candidate { id = "security-deviceguard-systemguard-launch", file = "security/desc.md", from = 1026, to = 1028, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", name = "ConfigureSystemGuardLaunch", apply = 0, revMode = "delete", name_ru = "Отключить принудительный запуск System Guard при загрузке", cat = "security" },
            new Candidate { id = "privacy-mdm-application-id", file = "privacy/desc.md", from = 1213, to = 1215, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\CurrentVersion\MDM", name = "MDMApplicationId", apply = "", revMode = "delete", name_ru = "Очистить идентификатор корпоративного приложения автоматической регистрации MDM", cat = "privacy" },
            new Candidate { id = "privacy-netcache-background-sync-period", file = "privacy/desc.md", from = 1336, to = 1338, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "BackgroundSyncPeriodMin", apply = 0, revMode = "delete", name_ru = "Отключить периодическую фоновую синхронизацию автономных файлов (NetCache)", cat = "privacy" },
            new Candidate { id = "privacy-netcache-background-sync-max-start", file = "privacy/desc.md", from = 1336, to = 1338, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "BackgroundSyncMaxStartMin", apply = 0, revMode = "delete", name_ru = "Отключить случайную задержку запуска синхронизации автономных файлов", cat = "privacy" },
            new Candidate { id = "privacy-netcache-background-sync-ignore-blockout", file = "privacy/desc.md", from = 1336, to = 1338, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "BackgroundSyncIgnoreBlockOutAfterMin", apply = 0, revMode = "delete", name_ru = "Отключить принудительную синхронизацию автономных файлов после блокировки", cat = "privacy" },
            new Candidate { id = "privacy-netcache-background-sync-forced-offline", file = "privacy/desc.md", from = 1336, to = 1338, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "BackgroundSyncEnabledForForcedOffline", apply = 0, revMode = "delete", name_ru = "Отключить синхронизацию автономных файлов в принудительном автономном режиме", cat = "privacy" },

            // Security - Windows Update
            new Candidate { id = "security-wu-scheduled-install-every-week", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "ScheduledInstallEveryWeek", apply = 1, revMode = "delete", name_ru = "Установить еженедельное расписание автоматического обновления Windows", cat = "security" },
            new Candidate { id = "security-wu-scheduled-install-first-week", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "ScheduledInstallFirstWeek", apply = 0, revMode = "delete", name_ru = "Отключить принудительную установку обновлений в первую неделю месяца", cat = "security" },
            new Candidate { id = "security-wu-scheduled-install-second-week", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "ScheduledInstallSecondWeek", apply = 0, revMode = "delete", name_ru = "Отключить принудительную установку обновлений во вторую неделю месяца (Patch Tuesday)", cat = "security" },
            new Candidate { id = "security-wu-fill-empty-content-urls", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "FillEmptyContentUrls", apply = 0, revMode = "delete", name_ru = "Запретить автоматическое заполнение пустых URL-адресов сервера обновлений", cat = "security" },
            new Candidate { id = "security-wu-donot-enforce-cert-pinning", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "DoNotEnforceEnterpriseTLSCertPinningForUpdateDetection", apply = 0, revMode = "delete", name_ru = "Включить обязательную привязку TLS-сертификатов (Cert Pinning) для обновлений", cat = "security" },
            new Candidate { id = "security-wu-proxy-behavior", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "SetProxyBehaviorForUpdateDetection", apply = 0, revMode = "delete", name_ru = "Отключить использование системного прокси для обнаружения обновлений", cat = "security" },
            new Candidate { id = "security-wu-target-release-version-info", file = "security/desc.md", from = 496, to = 498, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "TargetReleaseVersionInfo", apply = "", revMode = "delete", name_ru = "Сбросить целевую версию функционального обновления (TargetReleaseVersionInfo)", cat = "security" },
            new Candidate { id = "security-wu-branch-readiness-level", file = "security/desc.md", from = 497, to = 499, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "BranchReadinessLevel", apply = 16, revMode = "delete", name_ru = "Установить уровень готовности ветки обновлений на Semi-Annual Channel (16)", cat = "security" },
            new Candidate { id = "security-wu-defer-quality-updates-days", file = "security/desc.md", from = 498, to = 500, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "DeferQualityUpdatesPeriodInDays", apply = 0, revMode = "delete", name_ru = "Сбросить задержку получения качественных обновлений (0 дней)", cat = "security" },

            // Security - PowerShell Policies
            new Candidate { id = "security-powershell-machine-enable-scripts", file = "security/desc.md", from = 669, to = 671, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\PowerShell", name = "EnableScripts", apply = 1, revMode = "delete", name_ru = "Разрешить выполнение подписанных сценариев PowerShell (Machine)", cat = "security" },
            new Candidate { id = "security-powershell-machine-execution-policy", file = "security/desc.md", from = 669, to = 671, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\PowerShell", name = "ExecutionPolicy", apply = "RemoteSigned", revMode = "delete", name_ru = "Установить политику выполнения PowerShell RemoteSigned (Machine)", cat = "security" },
            new Candidate { id = "security-powershell-user-execution-policy", file = "security/desc.md", from = 669, to = 671, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\PowerShell", name = "ExecutionPolicy", apply = "RemoteSigned", revMode = "delete", name_ru = "Установить политику выполнения PowerShell RemoteSigned (User)", cat = "security" },
            new Candidate { id = "security-powershell-invocation-header", file = "security/desc.md", from = 671, to = 673, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\PowerShell\Transcription", name = "EnableInvocationHeader", apply = 1, revMode = "delete", name_ru = "Включить заголовок вызова команд в транскриптах PowerShell", cat = "security" },
            new Candidate { id = "security-powershell-scriptblock-invocation-logging", file = "security/desc.md", from = 672, to = 674, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging", name = "EnableScriptBlockInvocationLogging", apply = 1, revMode = "delete", name_ru = "Включить протоколирование событий вызова блоков сценариев PowerShell", cat = "security" },

            // Security - Device Guard & Virtualization Based Security
            new Candidate { id = "security-deviceguard-lsa-cfg-flags", file = "security/desc.md", from = 1026, to = 1028, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard", name = "LsaCfgFlags", apply = 0, revMode = "delete", name_ru = "Отключить изоляцию учетных данных LSA в Credential Guard (LsaCfgFlags)", cat = "security" },

            // Visibility
            new Candidate { id = "visibility-dwm-disallow-animations-machine", file = "visibility/desc.md", from = 1080, to = 1082, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\DWM", name = "DisallowAnimations", apply = 1, revMode = "delete", name_ru = "Отключить все анимации окон DWM на уровне системы (Machine)", cat = "visibility" }
        };

        Console.WriteLine("Total candidates for Block 27: " + candidates.Count);

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
            Console.WriteLine("ALL 25 CANDIDATES FOR BLOCK 27 VALIDATED PERFECTLY! Zero errors, zero collisions.");
            return 0;
        }
        else
        {
            Console.WriteLine("Total validation errors: " + errors);
            return 1;
        }
    }
}
