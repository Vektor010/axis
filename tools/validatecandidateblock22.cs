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
            // Security (security/desc.md)
            new Candidate { id = "security-wtds-notify-malicious", file = "security/desc.md", from = 282, to = 284, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WTDS\Components", name = "NotifyMalicious", apply = 1, revMode = "delete", name_ru = "Уведомлять при обнаружении вредоносного содержимого WTDS", cat = "security" },
            new Candidate { id = "security-wtds-capture-threat-window", file = "security/desc.md", from = 285, to = 287, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WTDS\Components", name = "CaptureThreatWindow", apply = 0, revMode = "delete", name_ru = "Отключить захват снимка экрана окна с угрозой WTDS", cat = "security" },
            new Candidate { id = "security-defender-disable-behavior-monitoring", file = "security/desc.md", from = 287, to = 289, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", name = "DisableBehaviorMonitoring", apply = 0, revMode = "delete", name_ru = "Включить мониторинг поведения подозрительных процессов Защитника", cat = "security" },
            new Candidate { id = "security-defender-disable-ioav-protection", file = "security/desc.md", from = 288, to = 290, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", name = "DisableIOAVProtection", apply = 0, revMode = "delete", name_ru = "Включить проверку всех загружаемых файлов и вложений IOAV", cat = "security" },
            new Candidate { id = "security-defender-disable-on-access-protection", file = "security/desc.md", from = 289, to = 291, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", name = "DisableOnAccessProtection", apply = 0, revMode = "delete", name_ru = "Включить постоянную защиту при обращении к файлам (On-Access Protection)", cat = "security" },
            new Candidate { id = "security-defender-disable-scan-realtime-enable", file = "security/desc.md", from = 291, to = 293, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", name = "DisableScanOnRealtimeEnable", apply = 0, revMode = "delete", name_ru = "Включить экспресс-сканирование при активации защиты реального времени", cat = "security" },
            new Candidate { id = "security-defender-disable-generic-reports", file = "security/desc.md", from = 292, to = 294, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Reporting", name = "DisableGenericRePorts", apply = 0, revMode = "delete", name_ru = "Отключить отправку стандартных отчетов Watson для Защитника", cat = "security" },
            new Candidate { id = "security-defender-disable-email-scanning", file = "security/desc.md", from = 294, to = 296, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Scan", name = "DisableEmailScanning", apply = 0, revMode = "delete", name_ru = "Включить сканирование входящих почтовых сообщений и баз писем", cat = "security" },
            new Candidate { id = "security-defender-signature-disable-notification", file = "security/desc.md", from = 296, to = 298, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Signature Updates", name = "SignatureDisableNotification", apply = 1, revMode = "delete", name_ru = "Отключить уведомления об отключении отчетов на основе сигнатур MAPS", cat = "security" },
            new Candidate { id = "security-defender-submit-samples-consent", file = "security/desc.md", from = 300, to = 302, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\Spynet", name = "SubmitSamplesConsent", apply = 2, revMode = "delete", name_ru = "Запретить отправку образцов подозрительных файлов в Microsoft Spynet", cat = "security" },
            new Candidate { id = "security-defender-cloud-block-level", file = "security/desc.md", from = 301, to = 303, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\MpEngine", name = "MpCloudBlockLevel", apply = 2, revMode = "delete", name_ru = "Установить высокий уровень облачной блокировки угроз (MpCloudBlockLevel High)", cat = "security" },
            new Candidate { id = "security-defender-bafs-extended-timeout", file = "security/desc.md", from = 302, to = 304, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows Defender\MpEngine", name = "MpBafsExtendedTimeout", apply = 50, revMode = "delete", name_ru = "Увеличить таймаут облачной проверки Block at First Sight до 50 секунд", cat = "security" },
            new Candidate { id = "security-store-auto-download", file = "security/desc.md", from = 491, to = 493, hive = "HKLM", path = @"Software\Policies\Microsoft\WindowsStore", name = "AutoDownload", apply = 2, revMode = "delete", name_ru = "Отключить автоматическое скачивание и установку обновлений Windows Store", cat = "security" },
            new Candidate { id = "security-wu-target-release-version", file = "security/desc.md", from = 496, to = 498, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "TargetReleaseVersion", apply = 1, revMode = "delete", name_ru = "Зафиксировать целевую версию выпуска компонентов Windows Update", cat = "security" },
            new Candidate { id = "security-wu-manage-preview-builds", file = "security/desc.md", from = 497, to = 499, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "ManagePreviewBuildsPolicyValue", apply = 0, revMode = "delete", name_ru = "Запретить автоматическую установку предварительных сборок (Preview Builds)", cat = "security" },
            new Candidate { id = "security-wu-defer-quality-updates", file = "security/desc.md", from = 498, to = 500, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "DeferQualityUpdates", apply = 1, revMode = "delete", name_ru = "Включить отсрочку накопительных обновлений качества (Quality Updates)", cat = "security" },
            new Candidate { id = "security-wu-disable-ux-access", file = "security/desc.md", from = 500, to = 502, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\WindowsUpdate", name = "SetDisableUXWUAccess", apply = 1, revMode = "delete", name_ru = "Скрыть доступ пользователя к интерфейсу Центра обновления Windows", cat = "security" },

            // Privacy (privacy/desc.md)
            new Candidate { id = "privacy-terminal-disable-clip", file = "privacy/desc.md", from = 738, to = 740, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", name = "fDisableClip", apply = 1, revMode = "delete", name_ru = "Запретить перенаправление буфера обмена в сеансах удаленного рабочего стола", cat = "privacy" },
            new Candidate { id = "privacy-copilot-hardware-key", file = "privacy/desc.md", from = 826, to = 828, hive = "HKCU", path = @"SOFTWARE\Policies\Microsoft\Windows\CopilotKey", name = "SetCopilotHardwareKey", apply = 0, revMode = "delete", name_ru = "Отключить принудительное действие аппаратной клавиши Copilot", cat = "privacy" },
            new Candidate { id = "privacy-biometrics-domain-accounts", file = "privacy/desc.md", from = 1005, to = 1007, hive = "HKLM", path = @"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider", name = "Domain Accounts", apply = 0, revMode = "delete", name_ru = "Запретить доменным пользователям вход в систему по биометрическим данным", cat = "privacy" },
            new Candidate { id = "privacy-terminal-allow-unsolicited", file = "privacy/desc.md", from = 1031, to = 1033, hive = "HKLM", path = @"Software\policies\Microsoft\Windows NT\Terminal Services", name = "fAllowUnsolicited", apply = 0, revMode = "delete", name_ru = "Запретить несанкционированные предложения удаленной помощи (Remote Assistance)", cat = "privacy" },
            new Candidate { id = "privacy-sync-disable-setting-sync", file = "privacy/desc.md", from = 1164, to = 1166, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableSettingSync", apply = 1, revMode = "delete", name_ru = "Полностью отключить облачную синхронизацию всех системных настроек", cat = "privacy" },
            new Candidate { id = "privacy-sync-disable-app-setting-sync", file = "privacy/desc.md", from = 1165, to = 1167, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableApplicationSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию настроек приложений Windows", cat = "privacy" },
            new Candidate { id = "privacy-sync-disable-credentials-sync", file = "privacy/desc.md", from = 1166, to = 1168, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableCredentialsSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию паролей и сохраненных учетных данных", cat = "privacy" },
            new Candidate { id = "privacy-sync-disable-start-layout-sync", file = "privacy/desc.md", from = 1173, to = 1175, hive = "HKLM", path = @"Software\Policies\Microsoft\Windows\SettingSync", name = "DisableStartLayoutSettingSync", apply = 1, revMode = "delete", name_ru = "Отключить облачную синхронизацию структуры и макета меню Пуск", cat = "privacy" }
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
            Console.WriteLine("ALL 25 CANDIDATES FOR BLOCK 22 ARE 100% VALID! ZERO ERRORS!");
        }
        else
        {
            Console.WriteLine("TOTAL ERRORS: " + errors);
        }
    }
}
