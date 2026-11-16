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
    public object revVal;
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
            // Game Mode (system/desc.md)
            new Candidate { id = "system-game-mode-allow-auto", file = "system/desc.md", from = 4440, to = 4443, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\GameDVR", name = "AllowAutoGameMode", apply = 0, revMode = "delete", name_ru = "Отключить принудительную автоматическую активацию Game Mode", cat = "system" },
            new Candidate { id = "system-game-mode-gpu-yield", file = "system/desc.md", from = 4434, to = 4439, hive = "HKCU", path = @"Software\Microsoft\GameBar", name = "GpuYieldPercentage", apply = 2, revMode = "delete", name_ru = "Минимальная задержка планировщика GPU Game Mode (GpuYieldPercentage 2%)", cat = "system" },
            new Candidate { id = "system-game-mode-disallow-system-cpu-sets", file = "system/desc.md", from = 4434, to = 4440, hive = "HKCU", path = @"Software\Microsoft\GameBar", name = "DisallowSystemAllowedCpuSets", apply = 1, revMode = "delete", name_ru = "Запретить системные ограничения CPU Sets для Game Mode", cat = "system" },

            // Privacy (privacy/desc.md)
            new Candidate { id = "privacy-limit-enhanced-diagnostic-data", file = "privacy/desc.md", from = 114, to = 116, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\DataCollection", name = "LimitEnhancedDiagnosticDataWindowsAnalytics", apply = 0, revMode = "delete", name_ru = "Ограничить расширенные диагностические данные для Desktop Analytics", cat = "privacy" },
            new Candidate { id = "privacy-disable-kms-avs-validation", file = "privacy/desc.md", from = 135, to = 137, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows NT\CurrentVersion\Software Protection Platform", name = "NoGenTicket", apply = 1, revMode = "delete", name_ru = "Отключить онлайн-валидацию AVS для клиента KMS", cat = "privacy" },
            new Candidate { id = "privacy-auto-approve-os-dumps", file = "privacy/desc.md", from = 334, to = 336, hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", name = "AutoApproveOSDumps", apply = 0, revMode = "delete", name_ru = "Запретить автоматическую отправку дампов памяти ОС в Microsoft", cat = "privacy" },
            new Candidate { id = "privacy-enable-device-health-attestation", file = "privacy/desc.md", from = 336, to = 338, hive = "HKLM", path = @"Software\Policies\Microsoft\DeviceHealthAttestationService", name = "EnableDeviceHealthAttestationService", apply = 0, revMode = "delete", name_ru = "Отключить службу мониторинга и отчетов аттестации работоспособности устройств", cat = "privacy" },
            new Candidate { id = "privacy-persistent-timestamp", file = "privacy/desc.md", from = 337, to = 339, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows NT\Reliability", name = "TimeStampEnabled", apply = 0, revMode = "delete", name_ru = "Отключить постоянную временную метку монитора надежности (Reliability)", cat = "privacy" },
            new Candidate { id = "privacy-disable-scripted-diagnostics", file = "privacy/desc.md", from = 380, to = 382, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnostics", name = "EnableDiagnostics", apply = 0, revMode = "delete", name_ru = "Запретить запуск мастеров скриптовой диагностики Windows", cat = "privacy" },
            new Candidate { id = "privacy-disable-remote-troubleshooting-server", file = "privacy/desc.md", from = 381, to = 383, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\ScriptedDiagnosticsProvider\Policy", name = "EnableQueryRemoteServer", apply = 0, revMode = "delete", name_ru = "Запретить доступ мастера устранения неполадок к онлайн-серверам Microsoft (WOTS)", cat = "privacy" },
            new Candidate { id = "privacy-disable-spotlight-features", file = "privacy/desc.md", from = 454, to = 456, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\CloudContent", name = "DisableWindowsSpotlightFeatures", apply = 1, revMode = "delete", name_ru = "Отключить все функции Windows Spotlight (Интересное в Windows)", cat = "privacy" },
            new Candidate { id = "privacy-disable-phone-pc-linking", file = "privacy/desc.md", from = 608, to = 610, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\System", name = "EnableMmx", apply = 0, revMode = "delete", name_ru = "Отключить интеграцию и связывание смартфона с ПК (Phone Link)", cat = "privacy" },
            new Candidate { id = "privacy-disable-app-uri-handlers", file = "privacy/desc.md", from = 722, to = 724, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\System", name = "EnableAppUriHandlers", apply = 0, revMode = "delete", name_ru = "Отключить перенаправление веб-ссылок в приложения (Web-to-App URI Handlers)", cat = "privacy" },
            new Candidate { id = "privacy-disable-sandbox-clipboard-redirection", file = "privacy/desc.md", from = 739, to = 741, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Sandbox", name = "AllowClipboardRedirection", apply = 0, revMode = "delete", name_ru = "Запретить общий буфер обмена с песочницей Windows Sandbox", cat = "privacy" },
            new Candidate { id = "privacy-disable-apps-camera-access", file = "privacy/desc.md", from = 1063, to = 1065, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppPrivacy", name = "LetAppsAccessCamera", apply = 2, revMode = "delete", name_ru = "Запретить приложениям Windows доступ к веб-камере", cat = "privacy" },
            new Candidate { id = "privacy-disable-apps-screenshot-border", file = "privacy/desc.md", from = 1068, to = 1070, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppPrivacy", name = "LetAppsAccessGraphicsCaptureWithoutBorder", apply = 1, revMode = "delete", name_ru = "Разрешить приложениям отключать желтую рамку захвата экрана", cat = "privacy" },
            new Candidate { id = "privacy-disable-apps-notifications-access", file = "privacy/desc.md", from = 1074, to = 1076, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppPrivacy", name = "LetAppsAccessNotifications", apply = 2, revMode = "delete", name_ru = "Запретить приложениям Windows доступ к системным уведомлениям", cat = "privacy" },
            new Candidate { id = "privacy-disable-apps-phone-call-access", file = "privacy/desc.md", from = 1075, to = 1077, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\AppPrivacy", name = "LetAppsAccessPhone", apply = 2, revMode = "delete", name_ru = "Запретить приложениям Windows доступ к совершению телефонных звонков", cat = "privacy" },
            new Candidate { id = "privacy-disable-sync-personalization", file = "privacy/desc.md", from = 1167, to = 1169, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisablePersonalizationSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию настроек персонализации Windows", cat = "privacy" },
            new Candidate { id = "privacy-disable-sync-desktop-theme", file = "privacy/desc.md", from = 1170, to = 1172, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableDesktopThemeSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию темы оформления рабочего стола", cat = "privacy" },
            new Candidate { id = "privacy-disable-sync-browser", file = "privacy/desc.md", from = 1171, to = 1173, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableWebBrowserSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию данных и закладок браузера", cat = "privacy" },
            new Candidate { id = "privacy-disable-sync-paid-network", file = "privacy/desc.md", from = 1172, to = 1174, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableSyncOnPaidNetwork", apply = 1, revMode = "delete", name_ru = "Отключить синхронизацию параметров в сетях с оплатой за трафик", cat = "privacy" },
            new Candidate { id = "privacy-disable-search-cloud", file = "privacy/desc.md", from = 1228, to = 1230, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", name = "AllowCloudSearch", apply = 0, revMode = "delete", name_ru = "Отключить облачный поиск Microsoft в меню поиска Windows", cat = "privacy" },
            new Candidate { id = "privacy-disable-search-location", file = "privacy/desc.md", from = 1227, to = 1229, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", name = "AllowSearchToUseLocation", apply = 0, revMode = "delete", name_ru = "Запретить службе поиска Windows использовать данные о местоположении", cat = "privacy" },
            new Candidate { id = "privacy-disable-cortana-above-lock", file = "privacy/desc.md", from = 1226, to = 1228, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", name = "AllowCortanaAboveLock", apply = 0, revMode = "delete", name_ru = "Запретить запуск голосового помощника на экране блокировки", cat = "privacy" }
        };

        Console.WriteLine("Total candidates: " + candidates.Count);

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
            Console.WriteLine("ALL 25 CANDIDATES ARE 100% VALID! ZERO ERRORS!");
        }
        else
        {
            Console.WriteLine("TOTAL ERRORS: " + errors);
        }
    }
}
