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
            // Network (3)
            new Candidate { id = "network-lltd-allow-lltdio-on-domain", file = "network/desc.md", from = 759, to = 761, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\LLTD", name = "AllowLLTDIOOnDomain", apply = 0, revMode = "delete", name_ru = "Запретить работу драйвера сопоставителя топологии LLTDIO в домене", cat = "network" },
            new Candidate { id = "network-lltd-allow-rspndr-on-domain", file = "network/desc.md", from = 760, to = 762, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\LLTD", name = "AllowRspndrOnDomain", apply = 0, revMode = "delete", name_ru = "Запретить работу драйвера ответчика топологии RSPNDR в домене", cat = "network" },
            new Candidate { id = "network-lltd-allow-rspndr-on-public-net", file = "network/desc.md", from = 760, to = 762, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\LLTD", name = "AllowRspndrOnPublicNet", apply = 0, revMode = "delete", name_ru = "Запретить работу драйвера ответчика топологии RSPNDR в публичных сетях", cat = "network" },

            // Peripheral (1)
            new Candidate { id = "peripheral-printers-disable-http-printing", file = "peripheral/desc.md", from = 1829, to = 1831, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows NT\Printers", name = "DisableHTTPPrinting", apply = 1, revMode = "delete", name_ru = "Отключить печать по сетевому протоколу HTTP", cat = "peripheral" },

            // Privacy (14)
            new Candidate { id = "privacy-handwriting-prevent-error-reports", file = "privacy/desc.md", from = 125, to = 127, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\HandwritingErrorReports", name = "PreventHandwritingErrorReports", apply = 1, revMode = "delete", name_ru = "Запретить отправку отчетов об ошибках рукописного ввода", cat = "privacy" },
            new Candidate { id = "privacy-handwriting-prevent-data-sharing", file = "privacy/desc.md", from = 125, to = 127, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\HandwritingErrorReports", name = "PreventHandwritingDataSharing", apply = 1, revMode = "delete", name_ru = "Запретить передачу образцов рукописного текста в Microsoft", cat = "privacy" },
            new Candidate { id = "privacy-sqm-disable-ceip", file = "privacy/desc.md", from = 133, to = 135, hive = "HKLM", path = @"Software\Policies\Microsoft\Internet Explorer\SQM", name = "DisableCustomerImprovementProgram", apply = 1, revMode = "delete", name_ru = "Запретить участие в программе улучшения качества ПО (CEIP Internet Explorer)", cat = "privacy" },
            new Candidate { id = "privacy-wer-auto-approve-os-dumps", file = "privacy/desc.md", from = 335, to = 337, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", name = "AutoApproveOSDumps", apply = 0, revMode = "delete", name_ru = "Запретить автоматическую отправку дампов памяти при системных сбоях", cat = "privacy" },
            new Candidate { id = "privacy-explorer-hide-recommended-section", file = "privacy/desc.md", from = 450, to = 452, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "HideRecommendedSection", apply = 1, revMode = "delete", name_ru = "Скрыть раздел Рекомендуем в меню Пуск", cat = "privacy" },
            new Candidate { id = "privacy-explorer-hide-recommended-personalized-sites", file = "privacy/desc.md", from = 451, to = 453, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "HideRecommendedPersonalizedSites", apply = 1, revMode = "delete", name_ru = "Скрыть персональные рекомендации сайтов в меню Пуск", cat = "privacy" },
            new Candidate { id = "privacy-ai-disable-recall-snapshots", file = "privacy/desc.md", from = 845, to = 847, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", name = "DisableAIDataAnalysis", apply = 1, revMode = "delete", name_ru = "Запретить сохранение снимков экрана для службы Windows Recall", cat = "privacy" },
            new Candidate { id = "privacy-ai-allow-recall-enablement", file = "privacy/desc.md", from = 846, to = 848, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", name = "AllowRecallEnablement", apply = 0, revMode = "delete", name_ru = "Запретить принудительное включение функций Windows Recall", cat = "privacy" },
            new Candidate { id = "privacy-ai-disable-click-to-do", file = "privacy/desc.md", from = 847, to = 849, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", name = "DisableClickToDo", apply = 1, revMode = "delete", name_ru = "Отключить интерактивный контекстный AI-инструмент Click to Do", cat = "privacy" },
            new Candidate { id = "privacy-biometrics-enabled", file = "privacy/desc.md", from = 1003, to = 1005, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Biometrics", name = "Enabled", apply = 0, revMode = "delete", name_ru = "Глобально запретить использование биометрических служб Windows", cat = "privacy" },
            new Candidate { id = "privacy-biometrics-credential-provider-enabled", file = "privacy/desc.md", from = 1004, to = 1006, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider", name = "Enabled", apply = 0, revMode = "delete", name_ru = "Запретить вход в Windows по биометрии (Windows Hello)", cat = "privacy" },
            new Candidate { id = "privacy-settingsync-disable-user-override", file = "privacy/desc.md", from = 1164, to = 1166, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию параметров", cat = "privacy" },
            new Candidate { id = "privacy-settingsync-disable-credentials-override", file = "privacy/desc.md", from = 1166, to = 1168, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableCredentialsSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию паролей", cat = "privacy" },
            new Candidate { id = "privacy-netcache-enabled", file = "privacy/desc.md", from = 1332, to = 1334, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\NetCache", name = "Enabled", apply = 0, revMode = "delete", name_ru = "Полностью отключить подсистему автономных файлов (NetCache Offline Files)", cat = "privacy" },

            // Security (2)
            new Candidate { id = "security-smartscreen-edge-phishing-filter-v9", file = "security/desc.md", from = 278, to = 280, hive = "HKLM", path = @"Software\Policies\Microsoft\MicrosoftEdge\PhishingFilter", name = "EnabledV9", apply = 0, revMode = "delete", name_ru = "Отключить фильтр SmartScreen в Microsoft Edge (v9)", cat = "security" },
            new Candidate { id = "security-smartscreen-edge-enabled", file = "security/desc.md", from = 280, to = 282, hive = "HKLM", path = @"Software\Policies\Microsoft\Edge", name = "SmartScreenEnabled", apply = 0, revMode = "delete", name_ru = "Отключить фильтр SmartScreen на уровне браузерного ядра Edge", cat = "security" },

            // System (4)
            new Candidate { id = "system-start-disable-account-notifications", file = "system/desc.md", from = 8402, to = 8404, hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\AccountNotifications", name = "DisableAccountNotifications", apply = 1, revMode = "delete", name_ru = "Отключить уведомления учетной записи в меню Пуск", cat = "system" },
            new Candidate { id = "system-push-notifications-no-toast", file = "system/desc.md", from = 8414, to = 8416, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\PushNotifications", name = "NoToastApplicationNotification", apply = 1, revMode = "delete", name_ru = "Отключить всплывающие тост-уведомления приложений", cat = "system" },
            new Candidate { id = "privacy-settingsync-disable-personalization-override", file = "privacy/desc.md", from = 1167, to = 1169, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisablePersonalizationSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию персонализации", cat = "privacy" },
            new Candidate { id = "system-start-show-hide-most-used-apps", file = "system/desc.md", from = 9160, to = 9162, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Explorer", name = "ShowOrHideMostUsedApps", apply = 2, revMode = "delete", name_ru = "Скрыть список Часто используемые приложения в меню Пуск", cat = "system" },

            // Visibility (1)
            new Candidate { id = "visibility-explorer-no-recent-docs-history", file = "visibility/desc.md", from = 336, to = 338, hive = "HKLM", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", name = "NoRecentDocsHistory", apply = 1, revMode = "delete", name_ru = "Не сохранять историю недавно открывавшихся документов", cat = "visibility" }
        };

        Console.WriteLine("Total candidates for Block 24: " + candidates.Count);

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
            Console.WriteLine("ALL 25 CANDIDATES FOR BLOCK 24 ARE 100% VALID! ZERO ERRORS!");
        }
        else
        {
            Console.WriteLine("TOTAL ERRORS: " + errors);
        }
    }
}
