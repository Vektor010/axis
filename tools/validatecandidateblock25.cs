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
            // Security - UAC policies (desc.md:522..616)
            new Candidate { id = "security-defender-definition-update-file-shares", file = "security/desc.md", from = 489, to = 491, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Signature Updates", name = "DefinitionUpdateFileSharesSources", apply = "", revMode = "delete", name_ru = "Очистить сетевые папки загрузки обновлений определений Защитника", cat = "security" },
            new Candidate { id = "security-defender-signature-update-fallback-order", file = "security/desc.md", from = 490, to = 492, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Signature Updates", name = "FallbackOrder", apply = "MicrosoftUpdateServer", revMode = "delete", name_ru = "Ограничить источники загрузки сигнатур сервером Microsoft Update", cat = "security" },
            new Candidate { id = "security-uac-enable-installer-detection", file = "security/desc.md", from = 563, to = 571, hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", name = "EnableInstallerDetection", apply = 0, revMode = "delete", name_ru = "Отключить эвристическое обнаружение программ установки UAC", cat = "security" },
            new Candidate { id = "security-uac-validate-admin-code-signatures", file = "security/desc.md", from = 572, to = 580, hive = "HKLM", path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", name = "ValidateAdminCodeSignatures", apply = 1, revMode = "delete", name_ru = "Требовать обязательную проверку цифровой подписи при повышении прав", cat = "security" },
            new Candidate { id = "security-wu-scheduled-install-day", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "ScheduledInstallDay", apply = 0, revMode = "delete", name_ru = "Установить ежедневное расписание проверки обновлений Windows", cat = "security" },
            new Candidate { id = "security-wu-scheduled-install-time", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "ScheduledInstallTime", apply = 3, revMode = "delete", name_ru = "Назначить время запланированной установки обновлений на 03:00", cat = "security" },
            new Candidate { id = "privacy-w32time-ntpclient-enabled", file = "privacy/desc.md", from = 539, to = 541, hive = "HKLM", path = @"Software\Policies\Microsoft\W32Time\TimeProviders\NtpClient", name = "Enabled", apply = 1, revMode = "delete", name_ru = "Включить встроенный клиент сетевого времени NTP (W32Time)", cat = "privacy" },

            // Security - Windows Update & SmartScreen
            new Candidate { id = "security-smartscreen-shell-level", file = "security/desc.md", from = 279, to = 281, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\System", name = "ShellSmartScreenLevel", apply = "Warn", revMode = "delete", name_ru = "Установить уровень предупреждения SmartScreen в проводнике", cat = "security" },
            new Candidate { id = "security-wu-au-options", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "AUOptions", apply = 2, revMode = "delete", name_ru = "Уведомлять о загрузке и установке обновлений Windows (AUOptions 2)", cat = "security" },
            new Candidate { id = "security-wu-automatic-maintenance", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "AutomaticMaintenanceEnabled", apply = 0, revMode = "delete", name_ru = "Отключить автоматическое обслуживание во время установки обновлений", cat = "security" },
            new Candidate { id = "security-wu-allow-mu-service", file = "security/desc.md", from = 493, to = 495, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU", name = "AllowMUUpdateService", apply = 0, revMode = "delete", name_ru = "Отключить получение обновлений для других продуктов Microsoft", cat = "security" },
            new Candidate { id = "security-wu-use-wuserver", file = "security/desc.md", from = 494, to = 496, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "UseWUServer", apply = 0, revMode = "delete", name_ru = "Запретить принудительное перенаправление обновлений на сервер WSUS", cat = "security" },

            // Privacy (SettingSync user overrides & diagnostics)
            new Candidate { id = "privacy-settingsync-disable-appsync-override", file = "privacy/desc.md", from = 1168, to = 1170, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableAppSyncSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию приложений", cat = "privacy" },
            new Candidate { id = "privacy-settingsync-disable-windows-setting-override", file = "privacy/desc.md", from = 1169, to = 1171, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableWindowsSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию параметров Windows", cat = "privacy" },
            new Candidate { id = "privacy-settingsync-disable-desktop-theme-override", file = "privacy/desc.md", from = 1170, to = 1172, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableDesktopThemeSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию темы рабочего стола", cat = "privacy" },
            new Candidate { id = "privacy-settingsync-disable-web-browser-override", file = "privacy/desc.md", from = 1171, to = 1173, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableWebBrowserSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию настроек браузера", cat = "privacy" },
            new Candidate { id = "privacy-settingsync-disable-start-layout-override", file = "privacy/desc.md", from = 1173, to = 1175, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableStartLayoutSettingSyncUserOverride", apply = 1, revMode = "delete", name_ru = "Запретить пользователю включать синхронизацию макета меню Пуск", cat = "privacy" },
            new Candidate { id = "privacy-mdm-use-aad-credential-type", file = "privacy/desc.md", from = 1213, to = 1215, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\CurrentVersion\MDM", name = "UseAADCredentialType", apply = 0, revMode = "delete", name_ru = "Отключить использование учетных данных Azure AD при регистрации MDM", cat = "privacy" },
            new Candidate { id = "privacy-cloudcontent-include-enterprise-spotlight", file = "privacy/desc.md", from = 453, to = 455, hive = "HKCU", path = @"Software\Policies\Microsoft\Windows\CloudContent", name = "IncludeEnterpriseSpotlight", apply = 0, revMode = "delete", name_ru = "Отключить корпоративный контент Windows Spotlight на экране блокировки", cat = "privacy" },
            new Candidate { id = "privacy-wdi-scenario-execution-enabled", file = "privacy/desc.md", from = 384, to = 386, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\WDI", name = "ScenarioExecutionEnabled", apply = 0, revMode = "delete", name_ru = "Отключить автоматическое выполнение сценариев диагностики WDI", cat = "privacy" },
            new Candidate { id = "privacy-scheduled-diagnostics-level", file = "privacy/desc.md", from = 383, to = 385, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows\ScheduledDiagnostics", name = "EnabledExecutionLevel", apply = 0, revMode = "delete", name_ru = "Отключить выполнение запланированных сценариев обслуживания системы", cat = "privacy" },
            new Candidate { id = "privacy-reliability-timestamp-interval", file = "privacy/desc.md", from = 337, to = 339, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows NT\Reliability", name = "TimeStampInterval", apply = 0, revMode = "delete", name_ru = "Отключить периодическую запись системных меток времени надежности", cat = "privacy" },

            // System
            new Candidate { id = "visibility-personalization-lock-screen-image", file = "visibility/desc.md", from = 1770, to = 1772, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\Personalization", name = "LockScreenImage", apply = @"C:\Windows\Web\Screen\img100.jpg", revMode = "delete", name_ru = "Зафиксировать стандартное фоновое изображение экрана блокировки", cat = "visibility" },

            // Visibility
            new Candidate { id = "visibility-desktop-no-desktop", file = "visibility/desc.md", from = 335, to = 337, hive = "HKLM", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", name = "NoDesktop", apply = 0, revMode = "delete", name_ru = "Разрешить отображение значков и элементов на рабочем столе", cat = "visibility" },
            new Candidate { id = "visibility-settings-page-visibility", file = "visibility/desc.md", from = 1920, to = 1922, hive = "HKLM", path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", name = "SettingsPageVisibility", apply = "default", revMode = "delete", name_ru = "Сбросить ограничения видимости страниц приложения Параметры", cat = "visibility" }
        };

        Console.WriteLine("Total candidates for Block 25: " + candidates.Count);

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
            Console.WriteLine("ALL 25 CANDIDATES FOR BLOCK 25 ARE 100% VALID! ZERO ERRORS!");
        }
        else
        {
            Console.WriteLine("TOTAL ERRORS: " + errors);
        }
    }
}
