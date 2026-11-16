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
    static void Main()
    {
        string root = @"C:\Users\User\Documents\win-tweaker";
        string srcDir = Path.Combine(root, @"data\source\win-config");
        string tweaksPath = Path.Combine(root, @"data\tweaks.json");

        var serializer = new JavaScriptSerializer();
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
            // Security
            new Candidate { id = "security-defender-allow-network-protection-server", file = "security/desc.md", from = 295, to = 297, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection", name = "AllowNetworkProtectionOnWinServer", apply = 1, revMode = "delete", name_ru = "Разрешить защиту сети Exploit Guard на Windows Server", cat = "security" },
            new Candidate { id = "security-defender-local-override-spynet", file = "security/desc.md", from = 298, to = 300, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Spynet", name = "LocalSettingOverrideSpynetReporting", apply = 0, revMode = "delete", name_ru = "Запретить локальное переопределение отчетов Spynet", cat = "security" },
            new Candidate { id = "security-defender-center-hide-all-notifications", file = "security/desc.md", from = 305, to = 307, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", name = "DisableNotifications", apply = 1, revMode = "delete", name_ru = "Скрыть все уведомления центра безопасности Защитника", cat = "security" },
            new Candidate { id = "security-defender-disable-antispyware", file = "security/desc.md", from = 308, to = 310, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows Defender", name = "DisableAntiSpyware", apply = 0, revMode = "delete", name_ru = "Включить модуль защиты от шпионских программ (AntiSpyware)", cat = "security" },
            new Candidate { id = "security-wu-disable-windows-update-access", file = "security/desc.md", from = 488, to = 490, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "DisableWindowsUpdateAccess", apply = 1, revMode = "delete", name_ru = "Заблокировать доступ ко всем функциям Центра обновления Windows", cat = "security" },
            new Candidate { id = "visibility-lockscreen-overlays-disabled", file = "visibility/desc.md", from = 1770, to = 1772, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Personalization", name = "LockScreenOverlaysDisabled", apply = 1, revMode = "delete", name_ru = "Отключить наложения и подсказки на экране блокировки", cat = "visibility" },
            new Candidate { id = "visibility-search-on-taskbar-mode", file = "visibility/desc.md", from = 1301, to = 1303, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Windows Search", name = "SearchOnTaskbarMode", apply = 0, revMode = "delete", name_ru = "Скрыть поле и значок поиска на панели задач", cat = "visibility" },
            new Candidate { id = "security-wu-allow-optional-content", file = "security/desc.md", from = 503, to = 505, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "AllowOptionalContent", apply = 0, revMode = "delete", name_ru = "Запретить автоматическую установку необязательного контента обновлений", cat = "security" },
            new Candidate { id = "security-defender-disable-block-at-first-seen", file = "security/desc.md", from = 297, to = 299, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Spynet", name = "DisableBlockAtFirstSeen", apply = 1, revMode = "delete", name_ru = "Отключить облачную блокировку при первом появлении (Block at First Sight)", cat = "security" },
            new Candidate { id = "security-filesystem-ntfs-disable-encryption", file = "security/desc.md", from = 1067, to = 1069, hive = "HKLM", path = @"System\CurrentControlSet\Policies", name = "NtfsDisableEncryption", apply = 1, revMode = "delete", name_ru = "Запретить шифрование файлов EFS на всех томах NTFS", cat = "security" },
            new Candidate { id = "security-defender-center-hide-non-critical-notifications", file = "system/desc.md", from = 8411, to = 8413, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", name = "DisableEnhancedNotifications", apply = 1, revMode = "delete", name_ru = "Скрыть некритические уведомления Центра безопасности Защитника", cat = "security" },

            // Privacy
            new Candidate { id = "privacy-configure-windows-spotlight", file = "privacy/desc.md", from = 453, to = 455, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\CloudContent", name = "ConfigureWindowsSpotlight", apply = 2, revMode = "delete", name_ru = "Отключить Windows Spotlight на экране блокировки", cat = "privacy" },
            new Candidate { id = "privacy-display-last-logon-info", file = "privacy/desc.md", from = 639, to = 641, hive = "HKLM", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System", name = "DisplayLastLogonInfo", apply = 1, revMode = "delete", name_ru = "Отображать информацию о предыдущих входах пользователя в систему", cat = "privacy" },
            new Candidate { id = "privacy-dont-display-logon-hours-warnings", file = "privacy/desc.md", from = 640, to = 642, hive = "HKCU", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\System", name = "DontDisplayLogonHoursWarnings", apply = 1, revMode = "delete", name_ru = "Скрыть предупреждения об окончании разрешенных часов входа", cat = "privacy" },
            new Candidate { id = "privacy-allow-camera-global", file = "privacy/desc.md", from = 1153, to = 1155, hive = "HKLM", path = @"software\Policies\Microsoft\Camera", name = "AllowCamera", apply = 0, revMode = "delete", name_ru = "Глобально запретить использование веб-камеры в Windows", cat = "privacy" },
            new Candidate { id = "privacy-sync-disable-appsync-sync", file = "privacy/desc.md", from = 1168, to = 1170, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableAppSyncSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию установленных приложений", cat = "privacy" },
            new Candidate { id = "privacy-sync-disable-windows-setting-sync", file = "privacy/desc.md", from = 1169, to = 1171, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableWindowsSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию других системных настроек Windows", cat = "privacy" },
            new Candidate { id = "privacy-netcache-work-offline-disabled", file = "privacy/desc.md", from = 1337, to = 1339, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "WorkOfflineDisabled", apply = 1, revMode = "delete", name_ru = "Удалить команду Автономная работа (NetCache)", cat = "privacy" },
            new Candidate { id = "privacy-mdm-disable-auto-enroll", file = "privacy/desc.md", from = 1213, to = 1215, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\CurrentVersion\MDM", name = "AutoEnrollMDM", apply = 0, revMode = "delete", name_ru = "Отключить автоматическое зачисление в MDM через Azure AD", cat = "privacy" },
            new Candidate { id = "privacy-cortana-disable-aad-oobe", file = "privacy/desc.md", from = 1229, to = 1231, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCortanaInAAD", name = "AllowCortanaInAADPathOOBE", apply = 0, revMode = "delete", name_ru = "Отключить страницу Cortana при первичной настройке системы (OOBE)", cat = "privacy" },
            new Candidate { id = "privacy-wmdrm-disable-online", file = "privacy/desc.md", from = 1247, to = 1249, hive = "HKLM", path = @"Software\Policies\Microsoft\WMDRM", name = "DisableOnline", apply = 1, revMode = "delete", name_ru = "Запретить сетевой доступ Windows Media DRM к Интернету", cat = "privacy" },
            new Candidate { id = "privacy-netcache-no-reminders", file = "privacy/desc.md", from = 1333, to = 1335, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "NoReminders", apply = 1, revMode = "delete", name_ru = "Отключить всплывающие напоминания службы автономных файлов (NetCache)", cat = "privacy" },
            new Candidate { id = "privacy-netcache-sync-at-logoff", file = "privacy/desc.md", from = 1334, to = 1336, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "SyncAtLogoff", apply = 0, revMode = "delete", name_ru = "Отключить принудительную синхронизацию автономных файлов при выходе из системы", cat = "privacy" },
            new Candidate { id = "privacy-netcache-sync-at-logon", file = "privacy/desc.md", from = 1335, to = 1337, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "SyncAtLogon", apply = 0, revMode = "delete", name_ru = "Отключить принудительную синхронизацию автономных файлов при входе в систему", cat = "privacy" },
            new Candidate { id = "privacy-netcache-disable-background-sync", file = "privacy/desc.md", from = 1336, to = 1338, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "BackgroundSyncEnabled", apply = 0, revMode = "delete", name_ru = "Отключить фоновую периодическую синхронизацию автономных файлов", cat = "privacy" }
        };

        Console.WriteLine("Total candidates for Block 23: " + candidates.Count);

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
                Console.WriteLine("FAIL: Duplicate registry key: " + regKey);
                errors++;
            }

            string fullSourcePath = Path.Combine(srcDir, c.file.Replace('/', '\\'));
            if (!File.Exists(fullSourcePath))
            {
                Console.WriteLine("FAIL: Source file missing: " + fullSourcePath);
                errors++;
                continue;
            }

            string[] lines = File.ReadAllLines(fullSourcePath, Encoding.UTF8);
            if (c.from < 1 || c.to > lines.Length || c.from > c.to)
            {
                Console.WriteLine("FAIL: Invalid line range: " + c.from + ".." + c.to + " (total " + lines.Length + ") for " + c.id);
                errors++;
                continue;
            }

            var sb = new StringBuilder();
            for (int l = c.from - 1; l < c.to; l++)
            {
                sb.AppendLine(lines[l]);
            }
            string sub = sb.ToString();

            bool hasName = sub.IndexOf(c.name, StringComparison.OrdinalIgnoreCase) >= 0;
            bool hasPath = sub.IndexOf(c.path, StringComparison.OrdinalIgnoreCase) >= 0;

            if (!hasName && !hasPath)
            {
                Console.WriteLine("FAIL: Neither name nor path found in " + c.file + ":" + c.from + ".." + c.to + " for " + c.id);
                errors++;
            }
            else
            {
                Console.WriteLine("PASS: " + c.id + " [" + c.name + "]");
            }
        }

        if (errors == 0)
        {
            Console.WriteLine("ALL 25 CANDIDATES FOR BLOCK 23 ARE 100% VALID! ZERO ERRORS!");
        }
        else
        {
            Console.WriteLine("TOTAL ERRORS: " + errors);
        }
    }
}
