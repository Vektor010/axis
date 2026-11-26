using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.Win32;
using WinTweaker.Models;

namespace WinTweaker.Engine
{
    /// <summary>
    /// Handles non-trivial real-OS execution for hardware enumeration (NIC/Audio/GPU/PCI affinities),
    /// BCDEdit, Netsh TCP congestion provider, PowerCfg, NVIDIA DRS profile, WinSxS/Cleanup actions,
    /// REG_BINARY/QWORD serialization, and immediate live service/shell refresh.
    /// </summary>
    public static class SpecialTweakExecutor
    {
        private const string NicClassPrefix = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}";
        private const string AudioClassPrefix = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e96c-e325-11ce-bfc1-08002be10318}";
        private const string DisplayClassPrefix = @"System\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
        private const string PciPlaceholderPrefix = @"SYSTEM\CurrentControlSet\Enum\PCI\...\";

        private static readonly object _cacheLock = new object();
        private static string _bcdCacheOutput = null;
        private static DateTime _bcdCacheTime = DateTime.MinValue;
        private static string _tcpCacheProvider = null;
        private static DateTime _tcpCacheTime = DateTime.MinValue;
        private static bool? _ipv6BindingAnyEnabledCache = null;
        private static DateTime _ipv6BindingCacheTime = DateTime.MinValue;
        private static List<string> _nicKeysCache = null;
        private static DateTime _nicKeysCacheTime = DateTime.MinValue;
        private static List<string> _audioKeysCache = null;
        private static DateTime _audioKeysCacheTime = DateTime.MinValue;
        private static List<string> _nvKeysCache = null;
        private static DateTime _nvKeysCacheTime = DateTime.MinValue;
        private static readonly Dictionary<string, List<string>> _pciKeysCache = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        private static DateTime _pciKeysCacheTime = DateTime.MinValue;
        private static Dictionary<string, bool> _scheduledTaskCache = null;
        private static DateTime _scheduledTaskCacheTime = DateTime.MinValue;

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        private static extern int DnsFlushResolverCache();

        [DllImport("shell32.dll")]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        public static void InvalidateCaches()
        {
            lock (_cacheLock)
            {
                _bcdCacheOutput = null;
                _bcdCacheTime = DateTime.MinValue;
                _tcpCacheProvider = null;
                _tcpCacheTime = DateTime.MinValue;
                _ipv6BindingAnyEnabledCache = null;
                _ipv6BindingCacheTime = DateTime.MinValue;
                _nicKeysCache = null;
                _nicKeysCacheTime = DateTime.MinValue;
                _audioKeysCache = null;
                _audioKeysCacheTime = DateTime.MinValue;
                _nvKeysCache = null;
                _nvKeysCacheTime = DateTime.MinValue;
                _pciKeysCache.Clear();
                _scheduledTaskCache = null;
                _scheduledTaskCacheTime = DateTime.MinValue;
                _pciKeysCacheTime = DateTime.MinValue;
            }
        }

        // =====================================================================
        // 1. Registry Type Parsing & Formatting Helpers (C# 5.0)
        // =====================================================================
        public static byte[] ParseHexBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return new byte[0];
            string cleaned = hex.Trim()
                .Replace("0x", "")
                .Replace("0X", "")
                .Replace(",", "")
                .Replace("-", "")
                .Replace(" ", "");
            if (cleaned.Length == 0) return new byte[0];
            if (cleaned.Length % 2 != 0) cleaned = "0" + cleaned;

            int len = cleaned.Length / 2;
            byte[] data = new byte[len];
            for (int i = 0; i < len; i++)
            {
                byte b;
                if (!byte.TryParse(cleaned.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out b))
                {
                    b = 0;
                }
                data[i] = b;
            }
            return data;
        }

        public static string FormatHexBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static object ConvertToRegistryValue(string regType, object rawValue, out RegistryValueKind kind)
        {
            kind = RegistryValueKind.DWord;
            if (rawValue == null)
            {
                if (regType == "REG_SZ" || regType == "REG_EXPAND_SZ")
                {
                    kind = (regType == "REG_EXPAND_SZ") ? RegistryValueKind.ExpandString : RegistryValueKind.String;
                    return "";
                }
                if (regType == "REG_BINARY")
                {
                    kind = RegistryValueKind.Binary;
                    return new byte[0];
                }
                if (regType == "REG_QWORD")
                {
                    kind = RegistryValueKind.QWord;
                    return 0L;
                }
                if (regType == "REG_MULTI_SZ")
                {
                    kind = RegistryValueKind.MultiString;
                    return new string[0];
                }
                return 0;
            }

            if (regType == "REG_DWORD")
            {
                kind = RegistryValueKind.DWord;
                return unchecked((int)Convert.ToInt64(rawValue));
            }
            if (regType == "REG_QWORD")
            {
                kind = RegistryValueKind.QWord;
                return Convert.ToInt64(rawValue);
            }
            if (regType == "REG_SZ")
            {
                kind = RegistryValueKind.String;
                return rawValue.ToString();
            }
            if (regType == "REG_EXPAND_SZ")
            {
                kind = RegistryValueKind.ExpandString;
                return rawValue.ToString();
            }
            if (regType == "REG_MULTI_SZ")
            {
                kind = RegistryValueKind.MultiString;
                string[] arr = rawValue as string[];
                if (arr != null) return arr;
                return rawValue.ToString().Split(new char[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (regType == "REG_BINARY")
            {
                kind = RegistryValueKind.Binary;
                byte[] existing = rawValue as byte[];
                if (existing != null) return existing;
                return ParseHexBytes(rawValue.ToString());
            }
            return rawValue;
        }

        public static bool RegistryValuesEqual(string regType, object currentVal, object expectedVal)
        {
            if (currentVal == null || expectedVal == null) return false;

            if (regType == "REG_DWORD")
            {
                try
                {
                    long cur = Convert.ToInt64(currentVal) & 0xFFFFFFFFL;
                    long exp = Convert.ToInt64(expectedVal) & 0xFFFFFFFFL;
                    return cur == exp;
                }
                catch { return false; }
            }
            if (regType == "REG_QWORD")
            {
                try { return Convert.ToInt64(currentVal) == Convert.ToInt64(expectedVal); }
                catch { return false; }
            }
            if (regType == "REG_BINARY")
            {
                byte[] curBytes = currentVal as byte[];
                if (curBytes == null) curBytes = ParseHexBytes(currentVal.ToString());
                byte[] expBytes = expectedVal as byte[];
                if (expBytes == null) expBytes = ParseHexBytes(expectedVal.ToString());
                if (curBytes.Length != expBytes.Length) return false;
                for (int i = 0; i < curBytes.Length; i++)
                {
                    if (curBytes[i] != expBytes[i]) return false;
                }
                return true;
            }
            if (regType == "REG_MULTI_SZ")
            {
                string[] curArr = currentVal as string[];
                string curJoined = curArr != null ? string.Join("\n", curArr) : currentVal.ToString();
                string[] expArr = expectedVal as string[];
                string expJoined = expArr != null ? string.Join("\n", expArr) : expectedVal.ToString();
                return string.Equals(curJoined, expJoined, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(currentVal.ToString(), expectedVal.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        // =====================================================================
        // 2. Identification & Routing for Real-OS / Special Tweaks
        // =====================================================================
        public static bool IsCleanupAction(TweakItem tweak)
        {
            if (tweak == null) return false;
            return string.Equals(tweak.category, "cleanup", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tweak.type, "action", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCustomRealExecutorTweak(TweakItem tweak)
        {
            if (tweak == null || string.IsNullOrEmpty(tweak.id)) return false;
            string id = tweak.id;
            if (id.StartsWith("system-bcd-", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "system-timer-force-tsc", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "system-timer-watchdog-disable", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "system-timer-disable-dynamic-tick", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "system-disable-memory-compression", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "system-ntfs-disable-last-access", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "network-congestion-provider-bbr", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "network-disable-ipv6-components", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "network-interrupt-moderation-low", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "privacy-disable-sleep-study", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "misc-spotify-optimize", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "misc-vscode-disable-telemetry", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "misc-lghub-toggle-services", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "peripheral-steelseries-disable-sonar", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "power-disable-hibernation", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "power-usb-audio-idle-timeout", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-noverse-minimal-profile", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-on-demand-nvcpl", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-powermizer-maximum-performance", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-telemetry-client-optout", StringComparison.OrdinalIgnoreCase)) return true;
            if (id.StartsWith("steam-vdf-", StringComparison.OrdinalIgnoreCase)) return true;
            if (ScheduledTaskGroupSpecs.ContainsKey(id)) return true;
            if (IsCleanupAction(tweak)) return true;
            return false;
        }

        private static readonly Dictionary<string, string[]> ScheduledTaskGroupSpecs = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "system-tasks-disable-ceip", new[] {
                @"\Microsoft\Windows\Autochk\Proxy",
                @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
                @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
                @"\Microsoft\Windows\Customer Experience Improvement Program\Uploader",
                @"\Microsoft\Windows\Customer Experience Improvement Program\Server\ServerCeipAssistant",
                @"\Microsoft\Windows\Customer Experience Improvement Program\Server\ServerRoleUsageCollector"
            }},
            { "system-tasks-disable-error-reporting", new[] {
                @"\Microsoft\Windows\Windows Error Reporting\QueueReporting"
            }},
            { "system-tasks-disable-feedback", new[] {
                @"\Microsoft\Windows\Feedback\Siuf\DmClient",
                @"\Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload"
            }},
            { "system-tasks-disable-app-experience", new[] {
                @"\Microsoft\Windows\Application Experience\MareBackup",
                @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
                @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser Exp",
                @"\Microsoft\Windows\Application Experience\PcaPatchDbTask",
                @"\Microsoft\Windows\Application Experience\SdbinstMergeDbTask",
                @"\Microsoft\Windows\Application Experience\StartupAppTask"
            }},
            { "system-tasks-disable-census", new[] {
                @"\Microsoft\Windows\Device Information\Device",
                @"\Microsoft\Windows\Device Information\Device User"
            }},
            { "system-tasks-disable-maps", new[] {
                @"\Microsoft\Windows\Maps\MapsUpdateTask",
                @"\Microsoft\Windows\Maps\MapsToastTask"
            }},
            { "system-tasks-disable-sleep-study", new[] {
                @"\Microsoft\Windows\Power Efficiency Diagnostics\AnalyzeSystem"
            }},
            { "system-tasks-disable-diag-telemetry", new[] {
                @"\Microsoft\Windows\Diagnosis\RecommendedTroubleshootingScanner",
                @"\Microsoft\Windows\Diagnosis\Scheduled",
                @"\Microsoft\Windows\Diagnosis\UnexpectedCodePath",
                @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector",
                @"\Microsoft\Windows\Flighting\OneSettings\RefreshCache",
                @"\Microsoft\Windows\AccountHealth\RecoverabilityToastTask",
                @"\Microsoft\Windows\Location\WindowsActionDialog",
                @"\Microsoft\Windows\RetailDemo\CleanupOfflineContent",
                @"\Microsoft\Windows\Speech\SpeechModelDownloadTask",
                @"\Microsoft\Windows\RemoteAssistance\RemoteAssistanceTask"
            }},
            { "system-tasks-disable-windows-ai", new[] {
                @"\Microsoft\Windows\WindowsAI\Recall\InitialConfiguration",
                @"\Microsoft\Windows\WindowsAI\Recall\PolicyConfiguration",
                @"\Microsoft\Windows\WindowsAI\Settings\InitialConfiguration"
            }},
            { "system-tasks-disable-office-telemetry", new[] {
                @"\Microsoft\Office\OfficeTelemetryAgentFallBack",
                @"\Microsoft\Office\OfficeTelemetryAgentFallBack2016",
                @"\Microsoft\Office\OfficeTelemetryAgentLogOn",
                @"\Microsoft\Office\OfficeTelemetryAgentLogOn2016"
            }},
            { "system-tasks-disable-defrag", new[] {
                @"\Microsoft\Windows\Defrag\ScheduledDefrag"
            }},
            { "system-tasks-disable-maintenance-misc", new[] {
                @"\Microsoft\Windows\ApplicationData\DsSvcCleanup",
                @"\Microsoft\Windows\CloudExperienceHost\CreateObjectTask",
                @"\Microsoft\Windows\DiskCleanup\SilentCleanup",
                @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticResolver",
                @"\Microsoft\Windows\DiskFootprint\Diagnostics",
                @"\Microsoft\Windows\DiskFootprint\StorageSense",
                @"\Microsoft\Windows\UNP\RunUpdateNotificationMgr",
                @"\Microsoft\Windows\BrokerInfrastructure\BgTaskRegistrationMaintenanceTask",
                @"\Microsoft\Windows\capabilityaccessmanager\maintenancetasks",
                @"\Microsoft\Windows\Registry\RegIdleBackup",
                @"\Microsoft\Windows\MemoryDiagnostic\AutomaticOfflineMemoryDiagnostic",
                @"\Microsoft\Windows\MemoryDiagnostic\ProcessMemoryDiagnosticEvents",
                @"\Microsoft\Windows\MemoryDiagnostic\RunFullMemoryDiagnostic",
                @"\Microsoft\Windows\Chkdsk\ProactiveScan",
                @"\Microsoft\Windows\Chkdsk\SyspartRepair"
            }},
            { "system-tasks-disable-offline-files-workfolders", new[] {
                @"\Microsoft\Windows\Offline Files\Background Synchronization",
                @"\Microsoft\Windows\Offline Files\Logon Synchronization",
                @"\Microsoft\Windows\Work Folders\Work Folders Logon Synchronization",
                @"\Microsoft\Windows\Work Folders\Work Folders Maintenance Work"
            }},
            { "system-tasks-disable-printing-wifi-tasks", new[] {
                @"\Microsoft\Windows\Printing\PrintJobCleanupTask",
                @"\Microsoft\Windows\Printing\PrinterCleanupTask",
                @"\Microsoft\Windows\Printing\EduPrintProv",
                @"\Microsoft\Windows\WCM\WiFiTask",
                @"\Microsoft\Windows\WlanSvc\CDSSync",
                @"\Microsoft\Windows\WwanSvc\NotificationTask",
                @"\Microsoft\Windows\WwanSvc\OobeDiscovery",
                @"\Microsoft\Windows\WlanSvc\MoProfileManagement"
            }},
            { "system-tasks-disable-localization-timesync", new[] {
                @"\Microsoft\Windows\International\Synchronize Language Settings",
                @"\Microsoft\Windows\LanguageComponentsInstaller\Installation",
                @"\Microsoft\Windows\LanguageComponentsInstaller\ReconcileLanguageResources",
                @"\Microsoft\Windows\LanguageComponentsInstaller\Uninstallation",
                @"\Microsoft\Windows\Time Synchronization\ForceSynchronizeTime",
                @"\Microsoft\Windows\Time Synchronization\SynchronizeTime"
            }},
            { "system-tasks-disable-wu-store-tasks", new[] {
                @"\Microsoft\Windows\InstallService\ScanForUpdates",
                @"\Microsoft\Windows\InstallService\ScanForUpdatesAsUser",
                @"\Microsoft\Windows\InstallService\SmartRetry",
                @"\Microsoft\Windows\InstallService\WakeUpAndContinueUpdates",
                @"\Microsoft\Windows\InstallService\WakeUpAndScanForUpdates",
                @"\Microsoft\Windows\UpdateOrchestrator\Report policies",
                @"\Microsoft\Windows\UpdateOrchestrator\Schedule Maintenance Work",
                @"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan",
                @"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan Static Task",
                @"\Microsoft\Windows\UpdateOrchestrator\Schedule Wake To Work",
                @"\Microsoft\Windows\UpdateOrchestrator\Schedule Work",
                @"\Microsoft\Windows\UpdateOrchestrator\Start Oobe Expedite Work",
                @"\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScan_LicenseAccepted",
                @"\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScanAfterUpdate",
                @"\Microsoft\Windows\UpdateOrchestrator\USO_UxBroker",
                @"\Microsoft\Windows\UpdateOrchestrator\UUS Failover Task",
                @"\Microsoft\Windows\WindowsUpdate\Scheduled Start",
                @"\Microsoft\Windows\WindowsUpdate\Refresh Group Policy Cache",
                @"\Microsoft\Windows\BitLocker\BitLocker Encrypt All Drives",
                @"\Microsoft\Windows\BitLocker\BitLocker MDM Policy Refresh"
            }},
            { "system-tasks-disable-system-restore", new[] {
                @"\Microsoft\Windows\SystemRestore\SR"
            }},
            { "security-tasks-disable-defender", new[] {
                @"\Microsoft\Windows\Windows Defender\Windows Defender Cache Maintenance",
                @"\Microsoft\Windows\Windows Defender\Windows Defender Cleanup",
                @"\Microsoft\Windows\Windows Defender\Windows Defender Scheduled Scan",
                @"\Microsoft\Windows\Windows Defender\Windows Defender Verification"
            }}
        };

        private class SteamVdfSpec
        {
            public string Section;
            public string Key;
            public string ApplyValue;
            public string RevertValue;
        }

        private static bool TryGetSteamVdfSpec(string id, out SteamVdfSpec spec)
        {
            spec = null;
            if (string.IsNullOrEmpty(id)) return false;
            switch (id.ToLowerInvariant())
            {
                case "steam-vdf-disable-game-recording":
                    spec = new SteamVdfSpec { Section = "GameRecording", Key = "BackgroundRecordMode", ApplyValue = "0", RevertValue = "1" };
                    return true;
                case "steam-vdf-disable-remote-play":
                    spec = new SteamVdfSpec { Section = "streaming_v2", Key = "EnableStreaming", ApplyValue = "0", RevertValue = "1" };
                    return true;
                case "steam-vdf-library-low-perf":
                    spec = new SteamVdfSpec { Section = "root", Key = "LibraryLowPerfMode", ApplyValue = "1", RevertValue = "0" };
                    return true;
                case "steam-vdf-library-low-bandwidth":
                    spec = new SteamVdfSpec { Section = "root", Key = "LibraryLowBandwidthMode", ApplyValue = "1", RevertValue = "0" };
                    return true;
                case "steam-vdf-disable-community-content":
                    spec = new SteamVdfSpec { Section = "root", Key = "LibraryDisableCommunityContent", ApplyValue = "1", RevertValue = "0" };
                    return true;
                case "steam-vdf-disable-broadcast":
                    spec = new SteamVdfSpec { Section = "Broadcast", Key = "Permissions", ApplyValue = "0", RevertValue = "2" };
                    return true;
                case "steam-vdf-disable-friends-autologin":
                    spec = new SteamVdfSpec { Section = "friends", Key = "SignIntoFriends", ApplyValue = "0", RevertValue = "1" };
                    return true;
                case "steam-vdf-reduce-motion":
                    spec = new SteamVdfSpec { Section = "Accessibility", Key = "ReduceMotion", ApplyValue = "1", RevertValue = "0" };
                    return true;
                case "steam-vdf-disable-overlay-browser-restore":
                    spec = new SteamVdfSpec { Section = "system", Key = "InGameOverlayRestoreBrowserTabs", ApplyValue = "0", RevertValue = "1" };
                    return true;
                case "steam-vdf-disable-news-popup":
                    spec = new SteamVdfSpec { Section = "news", Key = "NotifyAvailableGames", ApplyValue = "0", RevertValue = "1" };
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasUnresolvedPlaceholder(TweakItem tweak)
        {
            if (tweak == null || tweak.registry == null) return false;
            for (int i = 0; i < tweak.registry.Count; i++)
            {
                string p = tweak.registry[i].path ?? "";
                if (p.IndexOf("...", StringComparison.Ordinal) >= 0 ||
                    p.IndexOf("00XX", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    p.IndexOf("{default}", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        // =====================================================================
        // 3. Dynamic Hardware Path Resolution (NIC / Audio / GPU / PCI Affinity)
        // =====================================================================
        public static List<string> ResolveTargetPaths(TweakItem tweak, RegistryActionItem action, bool isRealMode)
        {
            List<string> results = new List<string>();
            if (action == null || string.IsNullOrEmpty(action.path)) return results;

            string rawPath = action.path;
            if (!isRealMode)
            {
                results.Add(rawPath);
                return results;
            }

            // A) Network adapter class placeholder (\00XX or \...)
            if (rawPath.StartsWith(NicClassPrefix + @"\", StringComparison.OrdinalIgnoreCase))
            {
                string tail = rawPath.Substring(NicClassPrefix.Length + 1);
                if (tail.Equals("00XX", StringComparison.OrdinalIgnoreCase) || tail.Equals("...", StringComparison.Ordinal))
                {
                    List<string> nicSubkeys = EnumerateRealPhysicalNicSubkeys();
                    for (int i = 0; i < nicSubkeys.Count; i++)
                    {
                        results.Add(NicClassPrefix + @"\" + nicSubkeys[i]);
                    }
                    return results;
                }
            }

            // B) Audio class placeholder (\00xx\PowerSettings)
            if (rawPath.StartsWith(AudioClassPrefix + @"\", StringComparison.OrdinalIgnoreCase) &&
                rawPath.IndexOf(@"\00xx\", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                List<string> audioSubkeys = EnumerateAudioPowerSettingsSubkeys();
                for (int i = 0; i < audioSubkeys.Count; i++)
                {
                    results.Add(AudioClassPrefix + @"\" + audioSubkeys[i] + @"\PowerSettings");
                }
                return results;
            }

            // C) Display adapter class for NVIDIA tweaks (\0000 -> actual NVIDIA subkey)
            if (tweak != null && string.Equals(tweak.category, "nvidia", StringComparison.OrdinalIgnoreCase) &&
                rawPath.StartsWith(DisplayClassPrefix + @"\0000", StringComparison.OrdinalIgnoreCase))
            {
                List<string> nvSubkeys = EnumerateNvidiaDisplaySubkeys();
                for (int i = 0; i < nvSubkeys.Count; i++)
                {
                    results.Add(DisplayClassPrefix + @"\" + nvSubkeys[i]);
                }
                return results;
            }

            // D) PCI device interrupt/affinity placeholders
            if (rawPath.StartsWith(PciPlaceholderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string relativeSuffix = rawPath.Substring(PciPlaceholderPrefix.Length);
                List<string> pciInstances = EnumerateTargetPciDevicePaths(tweak != null ? tweak.id : "");
                for (int i = 0; i < pciInstances.Count; i++)
                {
                    results.Add(pciInstances[i] + @"\" + relativeSuffix);
                }
                return results;
            }

            // E) Реальные службы и драйверы Windows/сторонних приложений: проверяем наличие Start в реестре,
            // чтобы отсутствующие на данном ПК службы (VirtualBox, SteelSeries, Epic, удалённые компоненты) были Unavailable
            const string servicesPrefix = @"SYSTEM\CurrentControlSet\Services\";
            if (string.Equals(action.name, "Start", StringComparison.OrdinalIgnoreCase) &&
                rawPath.StartsWith(servicesPrefix, StringComparison.OrdinalIgnoreCase) &&
                rawPath.LastIndexOf('\\') == servicesPrefix.Length - 1)
            {
                try
                {
                    using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    using (RegistryKey sk = hklm.OpenSubKey(rawPath, false))
                    {
                        if (sk == null || sk.GetValue("Start") == null)
                        {
                            return results;
                        }
                    }
                }
                catch { }
            }

            results.Add(rawPath);
            return results;
        }

        private static List<string> EnumerateRealPhysicalNicSubkeys()
        {
            lock (_cacheLock)
            {
                if (_nicKeysCache != null && (DateTime.UtcNow - _nicKeysCacheTime).TotalSeconds < 5.0)
                {
                    return new List<string>(_nicKeysCache);
                }
            }
            List<string> keys = new List<string>();
            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey cls = hklm.OpenSubKey(NicClassPrefix, false))
                {
                    if (cls == null) return keys;
                    string[] subs = cls.GetSubKeyNames();
                    List<string> pciKeys = new List<string>();
                    List<string> usbKeys = new List<string>();
                    for (int i = 0; i < subs.Length; i++)
                    {
                        string subName = subs[i];
                        if (!IsFourDigits(subName)) continue;
                        using (RegistryKey sk = cls.OpenSubKey(subName, false))
                        {
                            if (sk == null) continue;
                            string cid = (sk.GetValue("ComponentId") as string) ?? "";
                            string mid = (sk.GetValue("MatchingDeviceId") as string) ?? "";
                            string desc = (sk.GetValue("DriverDesc") as string) ?? "";
                            string idCheck = (cid + " " + mid).ToLowerInvariant();

                            if (desc.IndexOf("VirtualBox", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                desc.IndexOf("VMware", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                desc.IndexOf("WAN Miniport", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                desc.IndexOf("TAP-", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                continue;
                            }
                            if (idCheck.Contains(@"pci\")) pciKeys.Add(subName);
                            else if (idCheck.Contains(@"usb\")) usbKeys.Add(subName);
                        }
                    }
                    keys.AddRange(pciKeys);
                    if (keys.Count == 0) keys.AddRange(usbKeys);
                }
            }
            catch { }
            lock (_cacheLock)
            {
                _nicKeysCache = new List<string>(keys);
                _nicKeysCacheTime = DateTime.UtcNow;
            }
            return keys;
        }

        private static List<string> EnumerateAudioPowerSettingsSubkeys()
        {
            lock (_cacheLock)
            {
                if (_audioKeysCache != null && (DateTime.UtcNow - _audioKeysCacheTime).TotalSeconds < 5.0)
                {
                    return new List<string>(_audioKeysCache);
                }
            }
            List<string> keys = new List<string>();
            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey cls = hklm.OpenSubKey(AudioClassPrefix, false))
                {
                    if (cls == null) return keys;
                    string[] subs = cls.GetSubKeyNames();
                    for (int i = 0; i < subs.Length; i++)
                    {
                        string subName = subs[i];
                        if (!IsFourDigits(subName)) continue;
                        using (RegistryKey ps = cls.OpenSubKey(subName + @"\PowerSettings", false))
                        {
                            if (ps != null) keys.Add(subName);
                        }
                    }
                }
            }
            catch { }
            lock (_cacheLock)
            {
                _audioKeysCache = new List<string>(keys);
                _audioKeysCacheTime = DateTime.UtcNow;
            }
            return keys;
        }

        private static List<string> EnumerateNvidiaDisplaySubkeys()
        {
            lock (_cacheLock)
            {
                if (_nvKeysCache != null && (DateTime.UtcNow - _nvKeysCacheTime).TotalSeconds < 5.0)
                {
                    return new List<string>(_nvKeysCache);
                }
            }
            List<string> keys = new List<string>();
            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey cls = hklm.OpenSubKey(DisplayClassPrefix, false))
                {
                    if (cls == null) return keys;
                    string[] subs = cls.GetSubKeyNames();
                    for (int i = 0; i < subs.Length; i++)
                    {
                        string subName = subs[i];
                        if (!IsFourDigits(subName)) continue;
                        using (RegistryKey sk = cls.OpenSubKey(subName, false))
                        {
                            if (sk == null) continue;
                            string desc = (sk.GetValue("DriverDesc") as string) ?? "";
                            string prov = (sk.GetValue("ProviderName") as string) ?? "";
                            if (desc.IndexOf("NVIDIA", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                prov.IndexOf("NVIDIA", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                keys.Add(subName);
                            }
                        }
                    }
                }
            }
            catch { }
            lock (_cacheLock)
            {
                _nvKeysCache = new List<string>(keys);
                _nvKeysCacheTime = DateTime.UtcNow;
            }
            return keys;
        }

        private static List<string> EnumerateTargetPciDevicePaths(string tweakId)
        {
            bool wantDisplay = tweakId != null && (tweakId.Contains("-gpu-") || tweakId.Contains("msi-mode-gpu") || tweakId.Contains("device-policy-spread"));
            bool wantNet = tweakId != null && (tweakId.Contains("-nic-") || tweakId.Contains("device-policy-spread"));
            bool wantUsbXhci = tweakId != null && tweakId.Contains("device-policy-spread");
            string cacheKey = string.Format("{0}_{1}_{2}", wantDisplay ? 1 : 0, wantNet ? 1 : 0, wantUsbXhci ? 1 : 0);
            lock (_cacheLock)
            {
                List<string> cached;
                if ((DateTime.UtcNow - _pciKeysCacheTime).TotalSeconds < 5.0 && _pciKeysCache.TryGetValue(cacheKey, out cached))
                {
                    return new List<string>(cached);
                }
            }
            List<string> paths = new List<string>();

            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey pciRoot = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\PCI", false))
                {
                    if (pciRoot == null) return paths;
                    string[] venDevs = pciRoot.GetSubKeyNames();
                    for (int i = 0; i < venDevs.Length; i++)
                    {
                        using (RegistryKey devKey = pciRoot.OpenSubKey(venDevs[i], false))
                        {
                            if (devKey == null) continue;
                            string[] instances = devKey.GetSubKeyNames();
                            for (int j = 0; j < instances.Length; j++)
                            {
                                using (RegistryKey instKey = devKey.OpenSubKey(instances[j], false))
                                {
                                    if (instKey == null) continue;
                                    string cls = (instKey.GetValue("Class") as string) ?? "";
                                    string clsGuid = (instKey.GetValue("ClassGUID") as string) ?? "";
                                    string svc = (instKey.GetValue("Service") as string) ?? "";
                                    string desc = (instKey.GetValue("DeviceDesc") as string) ?? "";

                                    bool isDisplay = cls.Equals("Display", StringComparison.OrdinalIgnoreCase) ||
                                                     clsGuid.Equals("{4d36e968-e325-11ce-bfc1-08002be10318}", StringComparison.OrdinalIgnoreCase);
                                    bool isNet = cls.Equals("Net", StringComparison.OrdinalIgnoreCase) ||
                                                 clsGuid.Equals("{4d36e972-e325-11ce-bfc1-08002be10318}", StringComparison.OrdinalIgnoreCase);
                                    bool isXhci = svc.Equals("USBXHCI", StringComparison.OrdinalIgnoreCase);

                                    if (isDisplay && desc.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        continue;
                                    }

                                    bool match = false;
                                    if (wantDisplay && isDisplay) match = true;
                                    if (wantNet && isNet) match = true;
                                    if (wantUsbXhci && isXhci) match = true;

                                    if (match)
                                    {
                                        using (RegistryKey im = instKey.OpenSubKey(@"Device Parameters\Interrupt Management", false))
                                        {
                                            if (im != null)
                                            {
                                                paths.Add(@"SYSTEM\CurrentControlSet\Enum\PCI\" + venDevs[i] + @"\" + instances[j]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            lock (_cacheLock)
            {
                _pciKeysCache[cacheKey] = new List<string>(paths);
                _pciKeysCacheTime = DateTime.UtcNow;
            }
            return paths;
        }

        private static bool IsFourDigits(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length != 4) return false;
            for (int i = 0; i < 4; i++)
            {
                if (name[i] < '0' || name[i] > '9') return false;
            }
            return true;
        }

        public static byte[] ComputeOptimalGpuAffinityMask()
        {
            int procCount = Environment.ProcessorCount;
            ulong mask;
            if (procCount >= 10)
            {
                // Keep exact Noverse default mask on 10+ logical CPU systems (CPU 8 & 9 = 0x0300 little endian)
                mask = (1UL << 8) | (1UL << 9);
            }
            else if (procCount >= 4)
            {
                mask = (1UL << (procCount - 2)) | (1UL << (procCount - 1));
            }
            else
            {
                mask = (1UL << Math.Max(0, procCount - 1));
            }
            return BitConverter.GetBytes(mask);
        }

        // =====================================================================
        // 4. Custom CheckStatus / Apply / Revert / LiveText Implementations
        // =====================================================================
        public static TweakStatus CheckSpecialStatus(TweakItem tweak, IRegistryStore store, StateManager stateManager, bool isRealMode)
        {
            string id = tweak.id;

            // Cleanup / Action tweaks
            if (IsCleanupAction(tweak))
            {
                if (!isRealMode && tweak.registry != null && tweak.registry.Count > 0)
                {
                    RegistryActionItem act = tweak.registry[0];
                    if (store != null && store.ValueExists(act.hive, act.path, act.name) &&
                        RegistryValuesEqual(act.type, store.GetValue(act.hive, act.path, act.name), act.apply))
                    {
                        return TweakStatus.Enabled;
                    }
                }
                return (stateManager != null && stateManager.HasSnapshot(id)) ? TweakStatus.Enabled : TweakStatus.Disabled;
            }

            if (!isRealMode) return TweakStatus.Unknown;

            // BCD & Timer tweaks
            if (id.StartsWith("system-bcd-", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-force-tsc", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-watchdog-disable", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-disable-dynamic-tick", StringComparison.OrdinalIgnoreCase))
            {
                return CheckBcdStatus(id);
            }

            // Memory Compression (MMAgent / Superfetch)
            if (string.Equals(id, "system-disable-memory-compression", StringComparison.OrdinalIgnoreCase))
            {
                return CheckMemoryCompressionDisabled(store);
            }

            // Steam localconfig.vdf settings
            SteamVdfSpec vdfSpec;
            if (TryGetSteamVdfSpec(id, out vdfSpec))
            {
                if (SteamConfigHelper.GetLocalConfigVdfPaths().Length == 0) return TweakStatus.Unavailable;
                string cur = SteamConfigHelper.GetLocalConfigValue(vdfSpec.Section, vdfSpec.Key);
                if (string.IsNullOrEmpty(cur)) return TweakStatus.Disabled;
                return string.Equals(cur, vdfSpec.ApplyValue, StringComparison.OrdinalIgnoreCase) ? TweakStatus.Enabled : TweakStatus.Disabled;
            }

            // NIC Interrupt Moderation (*InterruptModeration / ITR)
            if (string.Equals(id, "network-interrupt-moderation-low", StringComparison.OrdinalIgnoreCase))
            {
                List<string> nics = EnumerateRealPhysicalNicSubkeys();
                if (nics.Count == 0) return TweakStatus.Unavailable;
                int matched = 0;
                for (int i = 0; i < nics.Count; i++)
                {
                    string path = NicClassPrefix + @"\" + nics[i];
                    object stdVal = store.GetValue("HKLM", path, "*InterruptModeration");
                    object itrVal = store.GetValue("HKLM", path, "ITR");
                    if ((stdVal != null && stdVal.ToString() == "0") || (itrVal != null && itrVal.ToString() == "0"))
                    {
                        matched++;
                    }
                }
                if (matched == nics.Count) return TweakStatus.Enabled;
                return matched > 0 ? TweakStatus.PartiallyEnabled : TweakStatus.Disabled;
            }

            // Noverse High Performance Power Plan (SCHEME_MIN or cloned Noverse High Performance)
            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase))
            {
                string activeGuid = GetActivePowerSchemeGuid();
                if (string.IsNullOrEmpty(activeGuid)) return TweakStatus.Unknown;
                if (string.Equals(activeGuid, "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", StringComparison.OrdinalIgnoreCase))
                {
                    return TweakStatus.Enabled;
                }
                string friendly = GetPowerSchemeFriendlyName(store, activeGuid);
                if (!string.IsNullOrEmpty(friendly) &&
                    friendly.IndexOf("Noverse High Performance", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return TweakStatus.Enabled;
                }
                return TweakStatus.Disabled;
            }

            // NVIDIA Scheduled Tasks
            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase))
            {
                if (EnumerateNvidiaDisplaySubkeys().Count == 0) return TweakStatus.Unavailable;
                return CheckNvidiaTasksDisabledStatus();
            }

            // Noverse Windows Scheduled Tasks Groups (COM Schedule.Service)
            string[] taskPaths;
            if (ScheduledTaskGroupSpecs.TryGetValue(id, out taskPaths))
            {
                return CheckScheduledTaskGroupStatus(taskPaths);
            }

            // NVIDIA Telemetry Opt-Out (both HKLM and HKCU)
            if (string.Equals(id, "nvidia-telemetry-client-optout", StringComparison.OrdinalIgnoreCase))
            {
                object vLm = store.GetValue("HKLM", @"Software\NVIDIA Corporation\NVControlPanel2\Client", "OptInOrOutPreference");
                object vCu = store.GetValue("HKCU", @"Software\NVIDIA Corporation\NVControlPanel2\Client", "OptInOrOutPreference");
                if (vLm != null && Convert.ToInt64(vLm) == 0 && vCu != null && Convert.ToInt64(vCu) == 0) return TweakStatus.Enabled;
                if ((vLm != null && Convert.ToInt64(vLm) == 0) || (vCu != null && Convert.ToInt64(vCu) == 0)) return TweakStatus.PartiallyEnabled;
                return TweakStatus.Unknown;
            }

            // NVIDIA PowerMizer Maximum Performance (real Display key)
            if (string.Equals(id, "nvidia-powermizer-maximum-performance", StringComparison.OrdinalIgnoreCase))
            {
                List<string> nvKeys = EnumerateNvidiaDisplaySubkeys();
                if (nvKeys.Count == 0) return TweakStatus.Unavailable;
                string path = DisplayClassPrefix + @"\" + nvKeys[0];
                object en = store.GetValue("HKLM", path, "PowerMizerEnable");
                object lv = store.GetValue("HKLM", path, "PowerMizerLevel");
                if (en != null && Convert.ToInt64(en) == 1 && lv != null && Convert.ToInt64(lv) == 1) return TweakStatus.Enabled;
                if (en != null && Convert.ToInt64(en) == 1) return TweakStatus.PartiallyEnabled;
                return (en == null) ? TweakStatus.Unknown : TweakStatus.Disabled;
            }

            // TCP Congestion Provider (BBR2)
            if (string.Equals(id, "network-congestion-provider-bbr", StringComparison.OrdinalIgnoreCase))
            {
                string prov = QueryTcpCongestionProvider();
                if (string.Equals(prov, "Unavailable", StringComparison.OrdinalIgnoreCase)) return TweakStatus.Unknown;
                return string.Equals(prov, "BBR2", StringComparison.OrdinalIgnoreCase) ? TweakStatus.Enabled : TweakStatus.Disabled;
            }

            // Power Hibernation
            if (string.Equals(id, "power-disable-hibernation", StringComparison.OrdinalIgnoreCase))
            {
                object val = store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled");
                if (val == null) return TweakStatus.Unknown;
                return Convert.ToInt64(val) == 0 ? TweakStatus.Enabled : TweakStatus.Disabled;
            }

            // USB/Audio PowerSettings Idle Timeout (REG_BINARY len 4)
            if (string.Equals(id, "power-usb-audio-idle-timeout", StringComparison.OrdinalIgnoreCase))
            {
                List<string> audioKeys = EnumerateAudioPowerSettingsSubkeys();
                if (audioKeys.Count == 0) return TweakStatus.Unavailable;
                int enabledCount = 0;
                for (int i = 0; i < audioKeys.Count; i++)
                {
                    string path = AudioClassPrefix + @"\" + audioKeys[i] + @"\PowerSettings";
                    byte[] perf = store.GetValue("HKLM", path, "PerformanceIdleTime") as byte[];
                    byte[] csPerf = store.GetValue("HKLM", path, "CSPerformanceIdleTime") as byte[];
                    byte[] cons = store.GetValue("HKLM", path, "ConservationIdleTime") as byte[];
                    bool perfZero = perf != null && perf.Length == 4 && BitConverter.ToUInt32(perf, 0) == 0;
                    bool consZero = cons != null && cons.Length == 4 && BitConverter.ToUInt32(cons, 0) == 0;
                    bool csOk = (csPerf == null) || (csPerf.Length == 4 && BitConverter.ToUInt32(csPerf, 0) == 0);
                    if (perfZero && consZero && csOk) enabledCount++;
                }
                if (enabledCount == audioKeys.Count) return TweakStatus.Enabled;
                return enabledCount > 0 ? TweakStatus.PartiallyEnabled : TweakStatus.Disabled;
            }

            // NTFS Last Access Update (Windows 10/11 stores 0x80000000 flags alongside mode bits)
            if (string.Equals(id, "system-ntfs-disable-last-access", StringComparison.OrdinalIgnoreCase))
            {
                object val = store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate");
                if (val == null) return TweakStatus.Unknown;
                try
                {
                    long raw = Convert.ToInt64(val) & 0x3L;
                    return (raw == 1L || raw == 3L) ? TweakStatus.Enabled : TweakStatus.Disabled;
                }
                catch { return TweakStatus.Unknown; }
            }

            // NVIDIA DRS Minimal Profile (some deprecated IDs may be unsupported by specific driver versions)
            if (string.Equals(id, "nvidia-noverse-minimal-profile", StringComparison.OrdinalIgnoreCase))
            {
                int matched, total;
                string summary;
                if (!NvDrsService.TryQueryMinimalProfile(out matched, out total, out summary))
                {
                    return TweakStatus.Unavailable;
                }
                if (matched == total || (total >= 20 && matched >= total - 4)) return TweakStatus.Enabled;
                if (matched > 0) return TweakStatus.PartiallyEnabled;
                return TweakStatus.Disabled;
            }

            // NVIDIA On-Demand NVCPL
            if (string.Equals(id, "nvidia-on-demand-nvcpl", StringComparison.OrdinalIgnoreCase))
            {
                if (EnumerateNvidiaDisplaySubkeys().Count == 0) return TweakStatus.Unavailable;
                object startVal = store.GetValue("HKLM", @"System\CurrentControlSet\Services\NVDisplay.ContainerLocalSystem", "Start");
                bool svcDisabled = startVal != null && Convert.ToInt64(startVal) == 4;
                string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Nvcpl.lnk");
                string wrapperPs1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Optimizer\NV-nvcpl.ps1");
                if (svcDisabled && (File.Exists(desktopShortcut) || File.Exists(wrapperPs1))) return TweakStatus.Enabled;
                if (svcDisabled) return TweakStatus.PartiallyEnabled;
                return TweakStatus.Disabled;
            }

            // IPv6 Full Disable (0xFF + ms_tcpip6)
            if (string.Equals(id, "network-disable-ipv6-components", StringComparison.OrdinalIgnoreCase))
            {
                return CheckIpv6FullDisabledStatus(store);
            }

            // Sleep Study Full Disable
            if (string.Equals(id, "privacy-disable-sleep-study", StringComparison.OrdinalIgnoreCase))
            {
                object ssDis = store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "SleepStudyDisabled");
                object ssAcc = store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Power", "SleepstudyAccountingEnabled");
                bool disOk = ssDis != null && Convert.ToInt64(ssDis) == 1;
                bool accOk = ssAcc != null && Convert.ToInt64(ssAcc) == 0;
                if (disOk && accOk) return TweakStatus.Enabled;
                if (disOk || accOk) return TweakStatus.PartiallyEnabled;
                return TweakStatus.Disabled;
            }

            // App Guide: Spotify
            if (string.Equals(id, "misc-spotify-optimize", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsSpotifyInstalled()) return TweakStatus.Unavailable;
                return CheckSpotifyConfigInDir(GetSpotifyAppDataDir());
            }

            // App Guide: VS Code / VSCodium
            if (string.Equals(id, "misc-vscode-disable-telemetry", StringComparison.OrdinalIgnoreCase))
            {
                string[] dirs = GetInstalledVSCodeUserDirs();
                if (dirs.Length == 0) return TweakStatus.Unavailable;
                int enabledCount = 0;
                for (int i = 0; i < dirs.Length; i++)
                {
                    if (CheckVSCodeConfigInDir(dirs[i]) == TweakStatus.Enabled) enabledCount++;
                }
                if (enabledCount == dirs.Length) return TweakStatus.Enabled;
                return enabledCount > 0 ? TweakStatus.PartiallyEnabled : TweakStatus.Disabled;
            }

            // App Guide: Logitech G HUB Toggle
            if (string.Equals(id, "misc-lghub-toggle-services", StringComparison.OrdinalIgnoreCase))
            {
                return CheckLghubToggleStatus(store);
            }

            // App Guide: SteelSeries GG / Sonar
            if (string.Equals(id, "peripheral-steelseries-disable-sonar", StringComparison.OrdinalIgnoreCase))
            {
                return CheckSteelSeriesStatus(store);
            }

            return TweakStatus.Unknown;
        }

        public static bool ApplySpecial(TweakItem tweak, IRegistryStore store, StateManager stateManager, JournalManager journal, bool isRealMode)
        {
            string id = tweak.id;

            // 1) Cleanup actions
            if (IsCleanupAction(tweak))
            {
                if (isRealMode)
                {
                    ExecuteCleanupAction(id, journal);
                    CleanupLegacyFakeRegistryKeys(id, store);
                }
                else if (tweak.registry != null && tweak.registry.Count > 0)
                {
                    RegistryValueKind k;
                    object val = ConvertToRegistryValue(tweak.registry[0].type, tweak.registry[0].apply, out k);
                    store.SetValue(tweak.registry[0].hive, tweak.registry[0].path, tweak.registry[0].name, val, k);
                }

                TweakSnapshot snap = new TweakSnapshot();
                snap.TweakId = id;
                snap.AppliedAt = DateTime.UtcNow.ToString("o");
                stateManager.SaveSnapshotIfAbsent(id, snap);
                return true;
            }

            if (!isRealMode) return false;

            // 2) BCD & Timer tweaks
            if (id.StartsWith("system-bcd-", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-force-tsc", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-watchdog-disable", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-disable-dynamic-tick", StringComparison.OrdinalIgnoreCase))
            {
                bool ok = ApplyBcdTweak(id, stateManager, journal);
                InvalidateCaches();
                return ok;
            }

            // 2b) Memory Compression (MMAgent)
            if (string.Equals(id, "system-disable-memory-compression", StringComparison.OrdinalIgnoreCase))
            {
                if (IsSysMainServiceDisabled(store))
                {
                    SaveCustomSnapshot(stateManager, id, "MemCompression", "SysMainDisabled");
                    journal.Log("APPLY", id, "Сжатие памяти (MMAgent) уже неактивно — служба SysMain отключена в системе");
                    return true;
                }
                int rc = RunProcess("powershell.exe", "-NoProfile -Command \"Disable-MMAgent -MemoryCompression\"", 12000);
                SaveCustomSnapshot(stateManager, id, "MemCompression", "Enabled");
                journal.Log(rc == 0 ? "APPLY" : "ERROR", id, "Выполнен Disable-MMAgent -MemoryCompression (exit=" + rc + ")");
                return rc == 0;
            }

            // 2b2) NTFS DisableLastAccessUpdate via fsutil (sets 0x80000001 persistently across Windows 10/11)
            if (string.Equals(id, "system-ntfs-disable-last-access", StringComparison.OrdinalIgnoreCase))
            {
                const string fsPath = @"SYSTEM\CurrentControlSet\Control\FileSystem";
                object prev = store.ValueExists("HKLM", fsPath, "NtfsDisableLastAccessUpdate")
                    ? store.GetValue("HKLM", fsPath, "NtfsDisableLastAccessUpdate")
                    : null;
                SaveCustomSnapshot(stateManager, id, "PrevNtfsLastAccess", prev != null ? prev.ToString() : "0");
                int rc = RunProcess("fsutil.exe", "behavior set disablelastaccess 1", 6000);
                if (rc != 0)
                {
                    store.SetValue("HKLM", fsPath, "NtfsDisableLastAccessUpdate", 1, RegistryValueKind.DWord);
                }
                journal.Log("APPLY", id, string.Format("Отключено обновление времени последнего доступа NTFS (fsutil exit={0})", rc));
                return true;
            }

            // 2c) Steam localconfig.vdf settings
            SteamVdfSpec vdfSpec;
            if (TryGetSteamVdfSpec(id, out vdfSpec))
            {
                string prev = SteamConfigHelper.GetLocalConfigValue(vdfSpec.Section, vdfSpec.Key);
                SaveCustomSnapshot(stateManager, id, "vdf_prev", prev ?? "");
                bool ok = SteamConfigHelper.SetLocalConfigValue(vdfSpec.Section, vdfSpec.Key, vdfSpec.ApplyValue);
                journal.Log(ok ? "APPLY" : "WARN", id,
                    ok ? string.Format("Steam VDF [{0}\\{1}] = {2} (перезапустите Steam, если открыт)", vdfSpec.Section, vdfSpec.Key, vdfSpec.ApplyValue)
                       : "Файл localconfig.vdf Steam не найден");
                return ok;
            }

            // 2d) NIC Interrupt Moderation (*InterruptModeration + ITR)
            if (string.Equals(id, "network-interrupt-moderation-low", StringComparison.OrdinalIgnoreCase))
            {
                List<string> nics = EnumerateRealPhysicalNicSubkeys();
                if (nics.Count == 0) return false;
                TweakSnapshot snap = new TweakSnapshot();
                snap.TweakId = id;
                for (int i = 0; i < nics.Count; i++)
                {
                    string path = NicClassPrefix + @"\" + nics[i];
                    string[] keys = new string[] { "*InterruptModeration", "ITR" };
                    for (int k = 0; k < keys.Length; k++)
                    {
                        bool exists = store.ValueExists("HKLM", path, keys[k]);
                        if (keys[k] == "ITR" && !exists) continue;
                        object cur = exists ? store.GetValue("HKLM", path, keys[k]) : null;
                        snap.Values.Add(new RegistryValueSnapshot { Hive = "HKLM", Path = path, Name = keys[k], Existed = exists, Kind = "REG_SZ", Value = cur != null ? cur.ToString() : null });
                        store.SetValue("HKLM", path, keys[k], "0", RegistryValueKind.String);
                    }
                }
                stateManager.SaveSnapshotIfAbsent(id, snap);
                journal.Log("APPLY", id, string.Format("Отключена модерация прерываний (*InterruptModeration=0) для {0} NIC", nics.Count));
                return true;
            }

            // 2e) Noverse High Performance Power Plan (with custom-OS fallback when SCHEME_MIN is stripped)
            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase))
            {
                string prevGuid = GetActivePowerSchemeGuid();
                SaveCustomSnapshot(stateManager, id, "PrevPowerScheme", prevGuid ?? "");
                int rc = RunProcess("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", 8000);
                string activatedDesc = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c (High Performance)";
                if (rc != 0)
                {
                    RunProcess("powercfg.exe", "/duplicatescheme 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", 8000);
                    rc = RunProcess("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", 8000);
                }
                if (rc != 0)
                {
                    string baseGuid = !string.IsNullOrEmpty(prevGuid) ? prevGuid : "381b4222-f694-41f0-9685-ff5bb260df2e";
                    string dupOut;
                    int dupRc = RunProcessCapture("powercfg.exe", "/duplicatescheme " + baseGuid, 8000, out dupOut);
                    if (dupRc == 0 && !string.IsNullOrEmpty(dupOut))
                    {
                        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(
                            dupOut, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
                        if (m.Success)
                        {
                            string newGuid = m.Value.ToLowerInvariant();
                            RunProcess("powercfg.exe", string.Format("/changename {0} \"Noverse High Performance\" \"Noverse low-latency performance profile\"", newGuid), 4000);
                            rc = RunProcess("powercfg.exe", "/setactive " + newGuid, 8000);
                            if (rc == 0)
                            {
                                activatedDesc = newGuid + " (Noverse High Performance)";
                            }
                        }
                    }
                    if (rc != 0 && !string.IsNullOrEmpty(prevGuid))
                    {
                        rc = RunProcess("powercfg.exe", "/setactive SCHEME_CURRENT", 4000);
                        activatedDesc = "SCHEME_CURRENT (применены индексы Noverse)";
                    }
                }
                // Apply Noverse High Perf indexes: processor min=100, USB selective suspend=0, PCIe ASPM=0, disk idle=0
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100", 4000);
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0", 4000);
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT SUB_PCIEXPRESS ASPM 0", 4000);
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT SUB_DISK DISKIDLE 0", 4000);
                RunProcess("powercfg.exe", "/setactive SCHEME_CURRENT", 4000);
                InvalidateCaches();
                journal.Log(rc == 0 ? "APPLY" : "ERROR", id, "Активирована схема питания " + activatedDesc);
                return rc == 0;
            }

            // 2f) NVIDIA Scheduled Tasks
            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase))
            {
                bool ok = SetNvidiaTasksEnabledViaCom(false);
                if (!ok)
                {
                    int rc = RunProcess("powershell.exe", "-NoProfile -Command \"Get-ScheduledTask -TaskName 'NvTmRep*','NvTmMon*' -ErrorAction SilentlyContinue | Disable-ScheduledTask | Out-Null\"", 10000);
                    ok = (rc == 0);
                }
                SaveCustomSnapshot(stateManager, id, "NvTasks", "Disabled");
                journal.Log("APPLY", id, "Задачи телеметрии NVIDIA (NvTmRep*/NvTmMon*) отключены");
                return ok;
            }

            // 2f-bis) Noverse Scheduled Tasks Groups (COM Schedule.Service + Snapshot + schtasks fallback)
            string[] applyTaskPaths;
            if (ScheduledTaskGroupSpecs.TryGetValue(id, out applyTaskPaths))
            {
                return ApplyScheduledTaskGroup(id, applyTaskPaths, stateManager, journal);
            }

            // 2g) NVIDIA Telemetry Opt-Out (HKLM + HKCU)
            if (string.Equals(id, "nvidia-telemetry-client-optout", StringComparison.OrdinalIgnoreCase))
            {
                TweakSnapshot snap = new TweakSnapshot();
                snap.TweakId = id;
                string[] hives = new string[] { "HKLM", "HKCU" };
                string clientPath = @"Software\NVIDIA Corporation\NVControlPanel2\Client";
                for (int i = 0; i < hives.Length; i++)
                {
                    bool ex = store.ValueExists(hives[i], clientPath, "OptInOrOutPreference");
                    object cur = ex ? store.GetValue(hives[i], clientPath, "OptInOrOutPreference") : null;
                    snap.Values.Add(new RegistryValueSnapshot { Hive = hives[i], Path = clientPath, Name = "OptInOrOutPreference", Existed = ex, Kind = "REG_DWORD", Value = cur });
                    store.SetValue(hives[i], clientPath, "OptInOrOutPreference", 0, RegistryValueKind.DWord);
                }
                stateManager.SaveSnapshotIfAbsent(id, snap);
                journal.Log("APPLY", id, "Отключена телеметрия NVCPL (OptInOrOutPreference=0 в HKLM и HKCU)");
                return true;
            }

            // 2h) NVIDIA PowerMizer Maximum Performance (3 ключа на реальном Display адаптере)
            if (string.Equals(id, "nvidia-powermizer-maximum-performance", StringComparison.OrdinalIgnoreCase))
            {
                List<string> nvKeys = EnumerateNvidiaDisplaySubkeys();
                if (nvKeys.Count == 0) return false;
                TweakSnapshot snap = new TweakSnapshot();
                snap.TweakId = id;
                string[] names = new string[] { "PowerMizerEnable", "PowerMizerLevel", "PowerMizerLevelAC" };
                for (int i = 0; i < nvKeys.Count; i++)
                {
                    string path = DisplayClassPrefix + @"\" + nvKeys[i];
                    for (int n = 0; n < names.Length; n++)
                    {
                        bool ex = store.ValueExists("HKLM", path, names[n]);
                        object cur = ex ? store.GetValue("HKLM", path, names[n]) : null;
                        snap.Values.Add(new RegistryValueSnapshot { Hive = "HKLM", Path = path, Name = names[n], Existed = ex, Kind = "REG_DWORD", Value = cur });
                        store.SetValue("HKLM", path, names[n], 1, RegistryValueKind.DWord);
                    }
                }
                stateManager.SaveSnapshotIfAbsent(id, snap);
                journal.Log("APPLY", id, "Включен режим PowerMizer Max Performance (Enable/Level/LevelAC=1)");
                return true;
            }

            // 3) TCP Congestion Provider (BBR2 via netsh + disable loopback large MTU per Noverse)
            if (string.Equals(id, "network-congestion-provider-bbr", StringComparison.OrdinalIgnoreCase))
            {
                string prev = QueryTcpCongestionProvider();
                SaveCustomSnapshot(stateManager, id, "CongestionProvider", prev);
                int rc1 = RunProcess("netsh.exe", "int tcp set supplemental template=internet congestionprovider=bbr2", 8000);
                int rc2 = RunProcess("netsh.exe", "int tcp set supplemental template=internetcustom congestionprovider=bbr2", 8000);
                RunProcess("netsh.exe", "int ipv4 set global loopbacklargemtu=disabled", 5000);
                RunProcess("netsh.exe", "int ipv6 set global loopbacklargemtu=disabled", 5000);
                InvalidateCaches();
                if (rc1 == 0 || rc2 == 0)
                {
                    journal.Log("APPLY", id, "Установлен BBR2 + loopbacklargemtu=disabled (фикс Noverse для Steam/127.0.0.1)");
                    return true;
                }
                journal.Log("ERROR", id, string.Format("netsh вернул коды {0}/{1}", rc1, rc2));
                return false;
            }

            // 4) Hibernation (powercfg /hibernate off)
            if (string.Equals(id, "power-disable-hibernation", StringComparison.OrdinalIgnoreCase))
            {
                object prev = store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled");
                SaveCustomSnapshot(stateManager, id, "HibernateEnabled", prev != null ? prev.ToString() : "1");
                int rc = RunProcess("powercfg.exe", "/hibernate off", 10000);
                if (rc == 0)
                {
                    journal.Log("APPLY", id, "Гибернация отключена через powercfg /hibernate off");
                    return true;
                }
                journal.Log("ERROR", id, "powercfg /hibernate off завершился с кодом " + rc);
                return false;
            }

            // 5) Audio Idle Timeout (REG_BINARY 4-byte)
            if (string.Equals(id, "power-usb-audio-idle-timeout", StringComparison.OrdinalIgnoreCase))
            {
                List<string> audioKeys = EnumerateAudioPowerSettingsSubkeys();
                if (audioKeys.Count == 0)
                {
                    journal.Log("WARN", id, "Не найдено аудиоустройств с веткой PowerSettings");
                    return false;
                }
                TweakSnapshot snap = new TweakSnapshot();
                snap.TweakId = id;
                byte[] zero4 = new byte[] { 0, 0, 0, 0 };
                for (int i = 0; i < audioKeys.Count; i++)
                {
                    string path = AudioClassPrefix + @"\" + audioKeys[i] + @"\PowerSettings";
                    string[] names = new string[] { "PerformanceIdleTime", "ConservationIdleTime", "CSPerformanceIdleTime" };
                    for (int n = 0; n < names.Length; n++)
                    {
                        bool exists = store.ValueExists("HKLM", path, names[n]);
                        object cur = exists ? store.GetValue("HKLM", path, names[n]) : null;
                        snap.Values.Add(new RegistryValueSnapshot
                        {
                            Hive = "HKLM",
                            Path = path,
                            Name = names[n],
                            Existed = exists,
                            Kind = "REG_BINARY",
                            Value = cur is byte[] ? FormatHexBytes((byte[])cur) : (cur != null ? cur.ToString() : null)
                        });
                        store.SetValue("HKLM", path, names[n], zero4, RegistryValueKind.Binary);
                    }
                }
                DeleteSubkeyIfBogus(AudioClassPrefix + @"\00xx");
                stateManager.SaveSnapshotIfAbsent(id, snap);
                journal.Log("APPLY", id, string.Format("Отключен idle timeout (REG_BINARY 00000000) для {0} аудиоустройств", audioKeys.Count));
                return true;
            }

            // 6) NVIDIA Noverse Minimal DRS Profile
            if (string.Equals(id, "nvidia-noverse-minimal-profile", StringComparison.OrdinalIgnoreCase))
            {
                string serialized, detail;
                bool ok = NvDrsService.ApplyMinimalProfile(out serialized, out detail);
                if (ok)
                {
                    SaveCustomSnapshot(stateManager, id, "NvDrsSnapshot", serialized);
                    journal.Log("APPLY", id, detail);
                }
                else
                {
                    journal.Log("ERROR", id, detail);
                }
                return ok;
            }

            // 7) NVIDIA On-Demand NVCPL
            if (string.Equals(id, "nvidia-on-demand-nvcpl", StringComparison.OrdinalIgnoreCase))
            {
                return ExecuteOnDemandNvcpl(true, store, stateManager, journal);
            }

            // 8) IPv6 Full Disable (0xFF + ms_tcpip6)
            if (string.Equals(id, "network-disable-ipv6-components", StringComparison.OrdinalIgnoreCase))
            {
                return ApplyIpv6FullDisable(store, stateManager, journal);
            }

            // 9) Sleep Study Full Disable
            if (string.Equals(id, "privacy-disable-sleep-study", StringComparison.OrdinalIgnoreCase))
            {
                return ApplySleepStudyDisable(store, stateManager, journal);
            }

            // 10) App Guides (Spotify / VS Code / LGHUB / SteelSeries)
            if (string.Equals(id, "misc-spotify-optimize", StringComparison.OrdinalIgnoreCase))
            {
                string detail;
                bool ok = ApplySpotifyConfigToDir(GetSpotifyAppDataDir(), out detail);
                journal.Log(ok ? "APPLY" : "WARN", id, detail);
                return ok;
            }

            if (string.Equals(id, "misc-vscode-disable-telemetry", StringComparison.OrdinalIgnoreCase))
            {
                return ApplyVSCodeConfigAll(journal, id);
            }

            if (string.Equals(id, "misc-lghub-toggle-services", StringComparison.OrdinalIgnoreCase))
            {
                return ApplyLghubToggle(true, store, stateManager, journal, id);
            }

            if (string.Equals(id, "peripheral-steelseries-disable-sonar", StringComparison.OrdinalIgnoreCase))
            {
                return ApplySteelSeriesSonar(true, store, stateManager, journal, id);
            }

            return false;
        }

        public static bool RevertSpecial(TweakItem tweak, IRegistryStore store, StateManager stateManager, JournalManager journal, bool isRealMode)
        {
            string id = tweak.id;
            if (IsCleanupAction(tweak))
            {
                stateManager.RemoveSnapshot(id);
                return true;
            }
            if (!isRealMode) return false;

            if (id.StartsWith("system-bcd-", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-force-tsc", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-watchdog-disable", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-disable-dynamic-tick", StringComparison.OrdinalIgnoreCase))
            {
                bool ok = RevertBcdTweak(id, stateManager, journal);
                InvalidateCaches();
                return ok;
            }

            if (string.Equals(id, "system-disable-memory-compression", StringComparison.OrdinalIgnoreCase))
            {
                int rc = RunProcess("powershell.exe", "-NoProfile -Command \"Enable-MMAgent -MemoryCompression\"", 12000);
                stateManager.RemoveSnapshot(id);
                journal.Log(rc == 0 ? "REVERT" : "ERROR", id, "Выполнен Enable-MMAgent -MemoryCompression (exit=" + rc + ")");
                return rc == 0;
            }

            if (string.Equals(id, "system-ntfs-disable-last-access", StringComparison.OrdinalIgnoreCase))
            {
                string prevStr = GetCustomSnapshotValue(stateManager, id, "PrevNtfsLastAccess");
                long prevVal = 0;
                if (!long.TryParse(prevStr, out prevVal)) prevVal = 0;
                int fsutilMode = (int)(prevVal & 0x3L);
                if (fsutilMode == 1 || fsutilMode == 3) fsutilMode = 0;
                RunProcess("fsutil.exe", "behavior set disablelastaccess " + fsutilMode, 6000);
                store.SetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate", unchecked((int)prevVal), RegistryValueKind.DWord);
                stateManager.RemoveSnapshot(id);
                journal.Log("REVERT", id, "Восстановлен режим NtfsDisableLastAccessUpdate: " + prevStr);
                return true;
            }

            SteamVdfSpec vdfSpec;
            if (TryGetSteamVdfSpec(id, out vdfSpec))
            {
                string prev = GetCustomSnapshotValue(stateManager, id, "vdf_prev");
                string restore = !string.IsNullOrEmpty(prev) ? prev : vdfSpec.RevertValue;
                bool ok = SteamConfigHelper.SetLocalConfigValue(vdfSpec.Section, vdfSpec.Key, restore);
                stateManager.RemoveSnapshot(id);
                journal.Log(ok ? "REVERT" : "WARN", id, string.Format("Steam VDF [{0}\\{1}] восстановлен в {2}", vdfSpec.Section, vdfSpec.Key, restore));
                return ok;
            }

            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase))
            {
                string prevGuid = GetCustomSnapshotValue(stateManager, id, "PrevPowerScheme");
                if (string.IsNullOrEmpty(prevGuid)) prevGuid = "381b4222-f694-41f0-9685-ff5bb260df2e"; // Balanced default
                int rc = RunProcess("powercfg.exe", "/setactive " + prevGuid, 8000);
                stateManager.RemoveSnapshot(id);
                journal.Log(rc == 0 ? "REVERT" : "ERROR", id, "Восстановлена схема питания: " + prevGuid);
                return rc == 0;
            }

            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase))
            {
                bool ok = SetNvidiaTasksEnabledViaCom(true);
                if (!ok)
                {
                    int rc = RunProcess("powershell.exe", "-NoProfile -Command \"Get-ScheduledTask -TaskName 'NvTmRep*','NvTmMon*' -ErrorAction SilentlyContinue | Enable-ScheduledTask | Out-Null\"", 10000);
                    ok = (rc == 0);
                }
                stateManager.RemoveSnapshot(id);
                journal.Log("REVERT", id, "Задачи NVIDIA повторно включены (при наличии в системе)");
                return ok;
            }

            string[] revertTaskPaths;
            if (ScheduledTaskGroupSpecs.TryGetValue(id, out revertTaskPaths))
            {
                return RevertScheduledTaskGroup(id, revertTaskPaths, stateManager, journal);
            }

            if (string.Equals(id, "network-interrupt-moderation-low", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "nvidia-telemetry-client-optout", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "nvidia-powermizer-maximum-performance", StringComparison.OrdinalIgnoreCase))
            {
                TweakSnapshot snap = stateManager.GetSnapshot(id);
                if (snap != null && snap.Values != null && snap.Values.Count > 0)
                {
                    for (int i = 0; i < snap.Values.Count; i++)
                    {
                        var v = snap.Values[i];
                        if (v.Existed && v.Value != null)
                        {
                            RegistryValueKind rk = (string.Equals(v.Kind, "REG_SZ", StringComparison.OrdinalIgnoreCase) || v.Kind == "String")
                                ? RegistryValueKind.String : RegistryValueKind.DWord;
                            object wval = (rk == RegistryValueKind.DWord) ? (object)unchecked((int)Convert.ToInt64(v.Value)) : v.Value.ToString();
                            store.SetValue(v.Hive, v.Path, v.Name, wval, rk);
                        }
                        else if (!v.Existed)
                        {
                            store.DeleteValue(v.Hive, v.Path, v.Name);
                        }
                    }
                    stateManager.RemoveSnapshot(id);
                    journal.Log("REVERT", id, "Значения восстановлены из снимка");
                    return true;
                }
                return false;
            }

            if (string.Equals(id, "network-congestion-provider-bbr", StringComparison.OrdinalIgnoreCase))
            {
                string prev = GetCustomSnapshotValue(stateManager, id, "CongestionProvider");
                if (string.IsNullOrEmpty(prev) || string.Equals(prev, "BBR2", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(prev, "Unavailable", StringComparison.OrdinalIgnoreCase))
                {
                    prev = "cubic";
                }
                string low = prev.ToLowerInvariant();
                int rc1 = RunProcess("netsh.exe", "int tcp set supplemental template=internet congestionprovider=" + low, 8000);
                int rc2 = RunProcess("netsh.exe", "int tcp set supplemental template=internetcustom congestionprovider=" + low, 8000);
                RunProcess("netsh.exe", "int ipv4 set global loopbacklargemtu=enabled", 5000);
                RunProcess("netsh.exe", "int ipv6 set global loopbacklargemtu=enabled", 5000);
                stateManager.RemoveSnapshot(id);
                InvalidateCaches();
                if (rc1 == 0 || rc2 == 0)
                {
                    journal.Log("REVERT", id, "TCP CongestionProvider возвращён на " + prev + " + loopbacklargemtu=enabled");
                    return true;
                }
                return false;
            }

            if (string.Equals(id, "power-disable-hibernation", StringComparison.OrdinalIgnoreCase))
            {
                int rc = RunProcess("powercfg.exe", "/hibernate on", 10000);
                stateManager.RemoveSnapshot(id);
                if (rc == 0)
                {
                    journal.Log("REVERT", id, "Гибернация включена через powercfg /hibernate on");
                    return true;
                }
                return false;
            }

            if (string.Equals(id, "power-usb-audio-idle-timeout", StringComparison.OrdinalIgnoreCase))
            {
                TweakSnapshot snap = stateManager.GetSnapshot(id);
                if (snap != null && snap.Values != null && snap.Values.Count > 0)
                {
                    for (int i = 0; i < snap.Values.Count; i++)
                    {
                        var v = snap.Values[i];
                        if (v.Existed && v.Value != null)
                        {
                            store.SetValue(v.Hive, v.Path, v.Name, ParseHexBytes(v.Value.ToString()), RegistryValueKind.Binary);
                        }
                        else if (!v.Existed)
                        {
                            store.DeleteValue(v.Hive, v.Path, v.Name);
                        }
                    }
                }
                else
                {
                    List<string> audioKeys = EnumerateAudioPowerSettingsSubkeys();
                    byte[] def30 = new byte[] { 0x1E, 0, 0, 0 };
                    for (int i = 0; i < audioKeys.Count; i++)
                    {
                        string path = AudioClassPrefix + @"\" + audioKeys[i] + @"\PowerSettings";
                        store.SetValue("HKLM", path, "ConservationIdleTime", def30, RegistryValueKind.Binary);
                    }
                }
                stateManager.RemoveSnapshot(id);
                journal.Log("REVERT", id, "Восстановлены настройки таймаута простоя аудиоустройств");
                return true;
            }

            if (string.Equals(id, "nvidia-noverse-minimal-profile", StringComparison.OrdinalIgnoreCase))
            {
                string snap = GetCustomSnapshotValue(stateManager, id, "NvDrsSnapshot");
                string detail;
                bool ok = NvDrsService.RevertMinimalProfile(snap, out detail);
                if (ok) stateManager.RemoveSnapshot(id);
                journal.Log(ok ? "REVERT" : "ERROR", id, detail);
                return ok;
            }

            if (string.Equals(id, "nvidia-on-demand-nvcpl", StringComparison.OrdinalIgnoreCase))
            {
                return ExecuteOnDemandNvcpl(false, store, stateManager, journal);
            }

            if (string.Equals(id, "network-disable-ipv6-components", StringComparison.OrdinalIgnoreCase))
            {
                return RevertIpv6FullDisable(store, stateManager, journal);
            }

            if (string.Equals(id, "privacy-disable-sleep-study", StringComparison.OrdinalIgnoreCase))
            {
                return RevertSleepStudyDisable(store, stateManager, journal);
            }

            if (string.Equals(id, "misc-spotify-optimize", StringComparison.OrdinalIgnoreCase))
            {
                string detail;
                bool ok = RevertSpotifyConfigInDir(GetSpotifyAppDataDir(), out detail);
                journal.Log(ok ? "REVERT" : "WARN", id, detail);
                return ok;
            }

            if (string.Equals(id, "misc-vscode-disable-telemetry", StringComparison.OrdinalIgnoreCase))
            {
                return RevertVSCodeConfigAll(journal, id);
            }

            if (string.Equals(id, "misc-lghub-toggle-services", StringComparison.OrdinalIgnoreCase))
            {
                return ApplyLghubToggle(false, store, stateManager, journal, id);
            }

            if (string.Equals(id, "peripheral-steelseries-disable-sonar", StringComparison.OrdinalIgnoreCase))
            {
                return ApplySteelSeriesSonar(false, store, stateManager, journal, id);
            }

            return false;
        }

        public static string GetLiveDisplayValue(TweakItem tweak, IRegistryStore store, bool isRealMode)
        {
            if (tweak == null) return "Нет данных";
            if (IsCleanupAction(tweak)) return "Разовая очистка";
            string id = tweak.id;

            if (!isRealMode)
            {
                if (tweak.registry != null && tweak.registry.Count > 0)
                {
                    var act = tweak.registry[0];
                    if (store != null && store.ValueExists(act.hive, act.path, act.name))
                    {
                        return FormatDisplayObject(store.GetValue(act.hive, act.path, act.name));
                    }
                }
                return "Не задано (тестовый режим)";
            }

            if (id.StartsWith("system-bcd-", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-force-tsc", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-watchdog-disable", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(id, "system-timer-disable-dynamic-tick", StringComparison.OrdinalIgnoreCase))
            {
                string rawVal;
                CheckBcdStatus(id, out rawVal);
                return rawVal;
            }

            if (string.Equals(id, "system-disable-memory-compression", StringComparison.OrdinalIgnoreCase))
            {
                TweakStatus mc = CheckMemoryCompressionDisabled(store);
                return (mc == TweakStatus.Enabled) ? "Отключено (MMAgent)" : "Включено (Windows)";
            }

            if (string.Equals(id, "system-ntfs-disable-last-access", StringComparison.OrdinalIgnoreCase))
            {
                object v = store != null ? store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate") : null;
                if (v == null) return "Не задано";
                try
                {
                    uint raw = unchecked((uint)Convert.ToInt32(v));
                    uint mode = raw & 0x3U;
                    return (mode == 1U || mode == 3U) ? string.Format("Отключено (0x{0:X})", raw) : string.Format("Включено (0x{0:X})", raw);
                }
                catch { return v.ToString(); }
            }

            SteamVdfSpec vdfSpec;
            if (TryGetSteamVdfSpec(id, out vdfSpec))
            {
                string v = SteamConfigHelper.GetLocalConfigValue(vdfSpec.Section, vdfSpec.Key);
                return !string.IsNullOrEmpty(v) ? string.Format("{0} ({1})", v, vdfSpec.Key) : "Не задано (по умолчанию Steam)";
            }

            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase))
            {
                string g = GetActivePowerSchemeGuid();
                if (string.IsNullOrEmpty(g)) return "Неизвестно";
                return string.Equals(g, "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", StringComparison.OrdinalIgnoreCase)
                    ? "High Performance (8c5e7fda)" : ("Схема: " + g);
            }

            if (string.Equals(id, "network-interrupt-moderation-low", StringComparison.OrdinalIgnoreCase))
            {
                List<string> nics = EnumerateRealPhysicalNicSubkeys();
                if (nics.Count == 0) return "Устройство не обнаружено";
                for (int i = 0; i < nics.Count; i++)
                {
                    string path = NicClassPrefix + @"\" + nics[i];
                    object stdVal = store != null ? store.GetValue("HKLM", path, "*InterruptModeration") : null;
                    if (stdVal != null) return "*InterruptModeration=" + stdVal.ToString();
                    object itrVal = store != null ? store.GetValue("HKLM", path, "ITR") : null;
                    if (itrVal != null) return "ITR=" + itrVal.ToString();
                }
            }

            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase))
            {
                TweakStatus ts = CheckNvidiaTasksDisabledStatus();
                return (ts == TweakStatus.Enabled) ? "Задачи отключены" : "Активны (или не установлены)";
            }

            string[] dispTaskPaths;
            if (ScheduledTaskGroupSpecs.TryGetValue(id, out dispTaskPaths))
            {
                int foundCount, disabledCount;
                TweakStatus ts = CheckScheduledTaskGroupStatus(dispTaskPaths, out foundCount, out disabledCount);
                if (foundCount == 0 || ts == TweakStatus.Unavailable) return "Задачи не обнаружены в этой ОС";
                return string.Format("Отключено {0} из {1} задач", disabledCount, foundCount);
            }

            if (string.Equals(id, "network-congestion-provider-bbr", StringComparison.OrdinalIgnoreCase))
            {
                return QueryTcpCongestionProvider();
            }

            if (string.Equals(id, "nvidia-noverse-minimal-profile", StringComparison.OrdinalIgnoreCase))
            {
                int matched, total;
                string summary;
                NvDrsService.TryQueryMinimalProfile(out matched, out total, out summary);
                return summary;
            }

            if (string.Equals(id, "network-disable-ipv6-components", StringComparison.OrdinalIgnoreCase))
            {
                return GetIpv6LiveDisplayValue(store);
            }

            if (string.Equals(id, "privacy-disable-sleep-study", StringComparison.OrdinalIgnoreCase))
            {
                object v = store != null ? store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "SleepStudyDisabled") : null;
                return (v != null && Convert.ToInt64(v) == 1) ? "Отключен (SleepStudyDisabled=1)" : "Активен (по умолчанию)";
            }

            if (string.Equals(id, "misc-spotify-optimize", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsSpotifyInstalled()) return "Spotify не установлен";
                return CheckSpotifyConfigInDir(GetSpotifyAppDataDir()) == TweakStatus.Enabled ? "Оптимизирован (prefs)" : "Стандартный профиль";
            }

            if (string.Equals(id, "misc-vscode-disable-telemetry", StringComparison.OrdinalIgnoreCase))
            {
                string[] dirs = GetInstalledVSCodeUserDirs();
                if (dirs.Length == 0) return "VS Code / VSCodium не установлен";
                return CheckVSCodeConfigInDir(dirs[0]) == TweakStatus.Enabled ? "Телеметрия отключена (settings.json)" : "Настройки по умолчанию";
            }

            if (string.Equals(id, "misc-lghub-toggle-services", StringComparison.OrdinalIgnoreCase))
            {
                TweakStatus s = CheckLghubToggleStatus(store);
                if (s == TweakStatus.Unavailable) return "Logitech G HUB не установлен";
                return s == TweakStatus.Enabled ? "Фоновые службы и драйверы отключены" : "Службы G HUB активны";
            }

            if (string.Equals(id, "peripheral-steelseries-disable-sonar", StringComparison.OrdinalIgnoreCase))
            {
                TweakStatus s = CheckSteelSeriesStatus(store);
                if (s == TweakStatus.Unavailable) return "SteelSeries GG не установлен";
                return s == TweakStatus.Enabled ? "Sonar и фоновые службы отключены" : "Службы Sonar активны";
            }

            if (tweak.registry != null && tweak.registry.Count > 0 && store != null)
            {
                var act = tweak.registry[0];
                List<string> paths = ResolveTargetPaths(tweak, act, isRealMode);
                if (paths.Count == 0)
                {
                    return "Устройство не обнаружено";
                }
                for (int i = 0; i < paths.Count; i++)
                {
                    if (store.ValueExists(act.hive, paths[i], act.name))
                    {
                        return FormatDisplayObject(store.GetValue(act.hive, paths[i], act.name));
                    }
                }
            }

            if (string.Equals(tweak.category, "steam", StringComparison.OrdinalIgnoreCase))
            {
                return "Не задано (по умолчанию Steam)";
            }
            return "Не задано (по умолчанию Windows)";
        }

        private static string FormatDisplayObject(object val)
        {
            if (val == null) return "null";
            byte[] bytes = val as byte[];
            if (bytes != null) return FormatHexBytes(bytes);
            string[] arr = val as string[];
            if (arr != null) return string.Join(", ", arr);
            return val.ToString();
        }

        // =====================================================================
        // 5. BCDEdit Helper
        // =====================================================================
        private static string GetBcdCurrentEnum()
        {
            lock (_cacheLock)
            {
                if (_bcdCacheOutput != null && (DateTime.UtcNow - _bcdCacheTime).TotalSeconds < 3.0)
                {
                    return _bcdCacheOutput;
                }
            }

            string output;
            int rc = RunProcessCapture("bcdedit.exe", "/enum {current}", 6000, out output);
            if (rc == 0 && !string.IsNullOrEmpty(output))
            {
                lock (_cacheLock)
                {
                    _bcdCacheOutput = output;
                    _bcdCacheTime = DateTime.UtcNow;
                }
            }
            return output ?? "";
        }

        private static string FindBcdLineValue(string enumText, params string[] keys)
        {
            if (string.IsNullOrEmpty(enumText)) return null;
            string[] lines = enumText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                for (int k = 0; k < keys.Length; k++)
                {
                    if (line.StartsWith(keys[k], StringComparison.OrdinalIgnoreCase) &&
                        line.Length > keys[k].Length && char.IsWhiteSpace(line[keys[k].Length]))
                    {
                        return line.Substring(keys[k].Length).Trim();
                    }
                }
            }
            return null;
        }

        private static TweakStatus CheckBcdStatus(string id)
        {
            string dummy;
            return CheckBcdStatus(id, out dummy);
        }

        private static TweakStatus CheckBcdStatus(string id, out string liveDisplay)
        {
            string enumText = GetBcdCurrentEnum();
            liveDisplay = "По умолчанию Windows";
            if (string.IsNullOrEmpty(enumText)) return TweakStatus.Unknown;

            if (id == "system-bcd-bootux-disabled")
            {
                string v = FindBcdLineValue(enumText, "bootuxdisabled", "custom:1600006C");
                if (v != null) liveDisplay = v;
                return (v != null && (v.Equals("Yes", StringComparison.OrdinalIgnoreCase) || v.Equals("True", StringComparison.OrdinalIgnoreCase)))
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            if (id == "system-bcd-bootmenupolicy-legacy")
            {
                string v = FindBcdLineValue(enumText, "bootmenupolicy");
                if (v != null) liveDisplay = v;
                return (v != null && v.Equals("Legacy", StringComparison.OrdinalIgnoreCase))
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            if (id == "system-bcd-highestmode")
            {
                string v = FindBcdLineValue(enumText, "highestmode", "custom:16000054");
                if (v != null) liveDisplay = v;
                return (v != null && (v.Equals("Yes", StringComparison.OrdinalIgnoreCase) || v.Equals("True", StringComparison.OrdinalIgnoreCase)))
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            if (id == "system-bcd-disable-loading-circle")
            {
                string v = FindBcdLineValue(enumText, "nobootuxprogress", "custom:16000069", "custom:0x16000069");
                if (v != null) liveDisplay = v;
                return (v != null && (v.Equals("Yes", StringComparison.OrdinalIgnoreCase) || v.Equals("True", StringComparison.OrdinalIgnoreCase) || v == "1"))
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            if (id == "system-bcd-disable-boot-logo")
            {
                string v = FindBcdLineValue(enumText, "custom:16000067", "custom:0x16000067", "quietboot");
                if (v != null) liveDisplay = v;
                return (v != null && (v.Equals("Yes", StringComparison.OrdinalIgnoreCase) || v.Equals("True", StringComparison.OrdinalIgnoreCase) || v == "1"))
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            if (id == "system-timer-watchdog-disable")
            {
                string v = FindBcdLineValue(enumText, "loadoptions");
                if (v != null) liveDisplay = v;
                return (v != null && v.IndexOf("SYSTEMWATCHDOGPOLICY=DISABLED", StringComparison.OrdinalIgnoreCase) >= 0)
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            if (id == "system-timer-force-tsc")
            {
                string v = FindBcdLineValue(enumText, "useplatformclock");
                liveDisplay = v != null ? ("useplatformclock=" + v) : "TSC (по умолчанию)";
                return (v == null || v.Equals("No", StringComparison.OrdinalIgnoreCase) || v.Equals("False", StringComparison.OrdinalIgnoreCase))
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            if (id == "system-timer-disable-dynamic-tick")
            {
                string v = FindBcdLineValue(enumText, "disabledynamictick");
                if (v != null) liveDisplay = "disabledynamictick=" + v;
                return (v != null && (v.Equals("Yes", StringComparison.OrdinalIgnoreCase) || v.Equals("True", StringComparison.OrdinalIgnoreCase)))
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }
            return TweakStatus.Unknown;
        }

        private static bool ApplyBcdTweak(string id, StateManager stateManager, JournalManager journal)
        {
            string enumText = GetBcdCurrentEnum();
            string curLoadOpts = FindBcdLineValue(enumText, "loadoptions") ?? "";
            SaveCustomSnapshot(stateManager, id, "bcd_prev", FindBcdSnapshotForId(enumText, id));

            int rc = -1;
            if (id == "system-bcd-bootux-disabled")
            {
                rc = RunProcess("bcdedit.exe", "/set {current} bootuxdisabled yes", 6000);
            }
            else if (id == "system-bcd-bootmenupolicy-legacy")
            {
                rc = RunProcess("bcdedit.exe", "/set {current} bootmenupolicy legacy", 6000);
            }
            else if (id == "system-bcd-highestmode")
            {
                rc = RunProcess("bcdedit.exe", "/set {current} highestmode yes", 6000);
            }
            else if (id == "system-bcd-disable-loading-circle")
            {
                rc = RunProcess("bcdedit.exe", "/set {current} custom:16000069 yes", 6000);
            }
            else if (id == "system-bcd-disable-boot-logo")
            {
                rc = RunProcess("bcdedit.exe", "/set {current} custom:16000067 yes", 6000);
            }
            else if (id == "system-timer-watchdog-disable")
            {
                string newOpts = curLoadOpts;
                if (newOpts.IndexOf("SYSTEMWATCHDOGPOLICY=DISABLED", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    newOpts = string.IsNullOrEmpty(newOpts) ? "SYSTEMWATCHDOGPOLICY=DISABLED" : (newOpts.Trim() + " SYSTEMWATCHDOGPOLICY=DISABLED");
                }
                rc = RunProcess("bcdedit.exe", string.Format("/set {{current}} loadoptions \"{0}\"", newOpts), 6000);
            }
            else if (id == "system-timer-force-tsc")
            {
                rc = RunProcess("bcdedit.exe", "/set {current} useplatformclock no", 6000);
            }
            else if (id == "system-timer-disable-dynamic-tick")
            {
                rc = RunProcess("bcdedit.exe", "/set {current} disabledynamictick yes", 6000);
            }

            journal.Log(rc == 0 ? "APPLY" : "ERROR", id, string.Format("Выполнение bcdedit для {0} (exit={1})", id, rc));
            return rc == 0;
        }

        private static bool RevertBcdTweak(string id, StateManager stateManager, JournalManager journal)
        {
            string prev = GetCustomSnapshotValue(stateManager, id, "bcd_prev");
            int rc = -1;

            if (id == "system-bcd-bootux-disabled")
            {
                rc = RunProcess("bcdedit.exe", string.IsNullOrEmpty(prev) ? "/deletevalue {current} bootuxdisabled" : "/set {current} bootuxdisabled " + prev, 6000);
                if (rc != 0) rc = RunProcess("bcdedit.exe", "/set {current} bootuxdisabled no", 6000);
            }
            else if (id == "system-bcd-bootmenupolicy-legacy")
            {
                string target = string.IsNullOrEmpty(prev) ? "standard" : prev.ToLowerInvariant();
                rc = RunProcess("bcdedit.exe", "/set {current} bootmenupolicy " + target, 6000);
            }
            else if (id == "system-bcd-highestmode")
            {
                rc = RunProcess("bcdedit.exe", string.IsNullOrEmpty(prev) ? "/deletevalue {current} highestmode" : "/set {current} highestmode " + prev, 6000);
                if (rc != 0) rc = RunProcess("bcdedit.exe", "/set {current} highestmode no", 6000);
            }
            else if (id == "system-bcd-disable-loading-circle")
            {
                rc = RunProcess("bcdedit.exe", "/deletevalue {current} custom:16000069", 6000);
            }
            else if (id == "system-bcd-disable-boot-logo")
            {
                rc = RunProcess("bcdedit.exe", "/deletevalue {current} custom:16000067", 6000);
            }
            else if (id == "system-timer-watchdog-disable")
            {
                string enumText = GetBcdCurrentEnum();
                string cur = FindBcdLineValue(enumText, "loadoptions") ?? "";
                string stripped = cur.Replace("SYSTEMWATCHDOGPOLICY=DISABLED", "").Replace("  ", " ").Trim();
                if (string.IsNullOrEmpty(stripped))
                {
                    rc = RunProcess("bcdedit.exe", "/deletevalue {current} loadoptions", 6000);
                }
                else
                {
                    rc = RunProcess("bcdedit.exe", string.Format("/set {{current}} loadoptions \"{0}\"", stripped), 6000);
                }
            }
            else if (id == "system-timer-force-tsc")
            {
                if (!string.IsNullOrEmpty(prev) && prev.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    rc = RunProcess("bcdedit.exe", "/set {current} useplatformclock yes", 6000);
                }
                else
                {
                    rc = RunProcess("bcdedit.exe", "/deletevalue {current} useplatformclock", 6000);
                    if (rc != 0) rc = 0; // already absent
                }
            }
            else if (id == "system-timer-disable-dynamic-tick")
            {
                rc = RunProcess("bcdedit.exe", "/deletevalue {current} disabledynamictick", 6000);
                if (rc != 0) rc = RunProcess("bcdedit.exe", "/set {current} disabledynamictick no", 6000);
            }

            if (rc == 0) stateManager.RemoveSnapshot(id);
            journal.Log(rc == 0 ? "REVERT" : "ERROR", id, string.Format("Откат bcdedit для {0} (exit={1})", id, rc));
            return rc == 0;
        }

        private static string FindBcdSnapshotForId(string enumText, string id)
        {
            if (id == "system-bcd-bootux-disabled") return FindBcdLineValue(enumText, "bootuxdisabled") ?? "";
            if (id == "system-bcd-bootmenupolicy-legacy") return FindBcdLineValue(enumText, "bootmenupolicy") ?? "Standard";
            if (id == "system-bcd-highestmode") return FindBcdLineValue(enumText, "highestmode") ?? "";
            if (id == "system-bcd-disable-loading-circle") return FindBcdLineValue(enumText, "nobootuxprogress", "custom:16000069") ?? "";
            if (id == "system-timer-force-tsc") return FindBcdLineValue(enumText, "useplatformclock") ?? "";
            if (id == "system-timer-disable-dynamic-tick") return FindBcdLineValue(enumText, "disabledynamictick") ?? "";
            if (id == "system-timer-watchdog-disable") return FindBcdLineValue(enumText, "loadoptions") ?? "";
            return "";
        }

        // =====================================================================
        // 6. Netsh TCP Congestion Provider Helper
        // =====================================================================
        private static string QueryTcpCongestionProvider()
        {
            lock (_cacheLock)
            {
                if (_tcpCacheProvider != null && (DateTime.UtcNow - _tcpCacheTime).TotalSeconds < 3.0)
                {
                    return _tcpCacheProvider;
                }
            }

            string output;
            int rc = RunProcessCapture("netsh.exe", "int tcp show supplemental", 5000, out output);
            string detected = "Unavailable";
            if (rc == 0 && !string.IsNullOrEmpty(output))
            {
                string[] lines = output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.IndexOf("bbr2", StringComparison.OrdinalIgnoreCase) >= 0) { detected = "BBR2"; break; }
                    if (line.IndexOf("cubic", StringComparison.OrdinalIgnoreCase) >= 0) { detected = "CUBIC"; }
                    else if (line.IndexOf("ctcp", StringComparison.OrdinalIgnoreCase) >= 0 && detected == "Unavailable") { detected = "CTCP"; }
                    else if (line.IndexOf("newreno", StringComparison.OrdinalIgnoreCase) >= 0 && detected == "Unavailable") { detected = "NewReno"; }
                }
            }

            lock (_cacheLock)
            {
                _tcpCacheProvider = detected;
                _tcpCacheTime = DateTime.UtcNow;
            }
            return detected;
        }

        // =====================================================================
        // 7. On-Demand NVCPL Helper (Fixed + Standalone Fallback)
        // =====================================================================
        private static bool ExecuteOnDemandNvcpl(bool enable, IRegistryStore store, StateManager stateManager, JournalManager journal)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(baseDir, @"scripts\nvcpl.ps1");
                if (!File.Exists(scriptPath))
                {
                    DirectoryInfo parent = Directory.GetParent(baseDir);
                    if (parent != null) scriptPath = Path.Combine(parent.FullName, @"scripts\nvcpl.ps1");
                }

                if (File.Exists(scriptPath))
                {
                    string actionArg = enable ? "Create" : "Restore";
                    int rc = RunProcess("powershell.exe", string.Format("-NoProfile -ExecutionPolicy Bypass -File \"{0}\" -Action {1}", scriptPath, actionArg), 20000);
                    if (rc == 0)
                    {
                        if (enable) SaveCustomSnapshot(stateManager, "nvidia-on-demand-nvcpl", "nvcpl_mode", "Disabled");
                        else stateManager.RemoveSnapshot("nvidia-on-demand-nvcpl");
                        journal.Log(enable ? "APPLY" : "REVERT", "nvidia-on-demand-nvcpl", "Выполнен nvcpl.ps1 -Action " + actionArg);
                        return true;
                    }
                }

                // Standalone portable fallback when scripts/ directory is not next to Axis.exe
                string[] nvServices = new string[] { "NVDisplay.ContainerLocalSystem", "NvContainerLocalSystem", "NvTelemetryContainer" };
                string nvcplDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Optimizer");
                string wrapperPs1 = Path.Combine(nvcplDir, "NV-nvcpl.ps1");
                string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Nvcpl.lnk");

                if (enable)
                {
                    string exePath = FindNvcplExe();
                    for (int i = 0; i < nvServices.Length; i++)
                    {
                        SetServiceStartAndControl(nvServices[i], 4, false, store);
                    }
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        Directory.CreateDirectory(nvcplDir);
                        string wrapperContent = string.Join(Environment.NewLine, new string[]
                        {
                            "$ErrorActionPreference = 'SilentlyContinue'",
                            "$nvsrv = @('NVDisplay.ContainerLocalSystem', 'NvContainerLocalSystem')",
                            "foreach ($s in $nvsrv) { if (Get-Service $s) { Set-Service $s -StartupType Manual; if ((Get-Service $s).Status -ne 'Running') { Start-Service $s } } }",
                            string.Format("Start-Process -FilePath '{0}' -Wait", exePath.Replace("'", "''")),
                            "foreach ($s in $nvsrv) { if (Get-Service $s) { if ((Get-Service $s).Status -eq 'Running') { Stop-Service $s -Force }; Set-Service $s -StartupType Disabled } }"
                        });
                        File.WriteAllText(wrapperPs1, wrapperContent, Encoding.ASCII);

                        string psCreateLnk = string.Format(
                            "$w = New-Object -ComObject WScript.Shell; $s = $w.CreateShortcut('{0}'); $s.TargetPath = 'powershell.exe'; $s.Arguments = '-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{1}\"'; $s.WorkingDirectory = '{2}'; $s.IconLocation = '{3}'; $s.Save()",
                            shortcutPath.Replace("'", "''"),
                            wrapperPs1.Replace("'", "''"),
                            nvcplDir.Replace("'", "''"),
                            exePath.Replace("'", "''"));
                        string encCmd = Convert.ToBase64String(Encoding.Unicode.GetBytes(psCreateLnk));
                        RunProcess("powershell.exe", "-NoProfile -EncodedCommand " + encCmd, 10000);
                    }
                    SaveCustomSnapshot(stateManager, "nvidia-on-demand-nvcpl", "nvcpl_mode", "Disabled");
                    journal.Log("APPLY", "nvidia-on-demand-nvcpl", "Службы NVIDIA переведены в Disabled и создан ярлык Nvcpl.lnk");
                    return true;
                }
                else
                {
                    if (File.Exists(shortcutPath)) try { File.Delete(shortcutPath); } catch { }
                    if (File.Exists(wrapperPs1)) try { File.Delete(wrapperPs1); } catch { }
                    SetServiceStartAndControl("NVDisplay.ContainerLocalSystem", 2, true, store);
                    SetServiceStartAndControl("NvContainerLocalSystem", 2, true, store);
                    stateManager.RemoveSnapshot("nvidia-on-demand-nvcpl");
                    journal.Log("REVERT", "nvidia-on-demand-nvcpl", "Службы NVIDIA Container возвращены в Automatic");
                    return true;
                }
            }
            catch (Exception ex)
            {
                journal.Log("ERROR", "nvidia-on-demand-nvcpl", "Ошибка NVCPL On-Demand: " + ex.Message);
                return false;
            }
        }

        private static string FindNvcplExe()
        {
            string std = @"C:\Program Files\NVIDIA Corporation\Control Panel Client\nvcplui.exe";
            if (File.Exists(std)) return std;
            try
            {
                string wa = @"C:\Program Files\WindowsApps";
                if (Directory.Exists(wa))
                {
                    string[] dirs = Directory.GetDirectories(wa, "NVIDIACorp.NVIDIAControlPanel_*");
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        string cand = Path.Combine(dirs[i], "nvcplui.exe");
                        if (File.Exists(cand)) return cand;
                    }
                }
            }
            catch { }
            return null;
        }

        // =====================================================================
        // 8. Live Service Runtime Control
        // =====================================================================
        public static void ApplyServiceRuntimeState(string registryPath, int targetStart)
        {
            if (string.IsNullOrEmpty(registryPath)) return;
            const string prefix = @"SYSTEM\CurrentControlSet\Services\";
            if (!registryPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
            string serviceName = registryPath.Substring(prefix.Length);
            if (serviceName.IndexOf('\\') >= 0 || string.IsNullOrEmpty(serviceName)) return;

            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (targetStart == 4)
                    {
                        if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending)
                        {
                            try { sc.Stop(); sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(1500)); } catch { }
                        }
                    }
                    else if (targetStart == 2)
                    {
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            try { sc.Start(); sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(1500)); } catch { }
                        }
                    }
                }
            }
            catch { }
        }

        private static void SetServiceStartAndControl(string serviceName, int startValue, bool startIfAuto, IRegistryStore store)
        {
            string path = @"System\CurrentControlSet\Services\" + serviceName;
            if (store.KeyExists("HKLM", path))
            {
                store.SetValue("HKLM", path, "Start", startValue, RegistryValueKind.DWord);
                ApplyServiceRuntimeState(@"SYSTEM\CurrentControlSet\Services\" + serviceName, startIfAuto ? 2 : startValue);
            }
        }

        // =====================================================================
        // 9. Complete Cleanup Executor (All 16 Noverse Cleanup Tasks)
        // =====================================================================
        public static void ExecuteCleanupAction(string tweakId, JournalManager journal)
        {
            if (string.IsNullOrEmpty(tweakId)) return;
            string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            try
            {
                if (tweakId.Contains("dns-cache"))
                {
                    try { DnsFlushResolverCache(); } catch { }
                    RunProcess("ipconfig.exe", "/flushdns", 5000);
                }
                else if (tweakId.Contains("recycle-bin"))
                {
                    try { SHEmptyRecycleBin(IntPtr.Zero, null, 7); } catch { }
                }
                else if (tweakId.Contains("temporary-files"))
                {
                    CleanDirectorySafe(Path.GetTempPath());
                    CleanDirectorySafe(Path.Combine(winDir, "Temp"));
                }
                else if (tweakId.Contains("temporary-internet-files"))
                {
                    CleanDirectorySafe(Path.Combine(localApp, @"Microsoft\Windows\INetCache"));
                }
                else if (tweakId.Contains("directx-shader-cache"))
                {
                    CleanDirectorySafe(Path.Combine(localApp, "D3DSCache"));
                    CleanDirectorySafe(Path.Combine(localApp, @"NVIDIA\DXCache"));
                    CleanDirectorySafe(Path.Combine(localApp, @"NVIDIA\GLCache"));
                    CleanDirectorySafe(Path.Combine(localApp, @"NVIDIA Corporation\NV_Cache"));
                    CleanDirectorySafe(Path.Combine(localApp, @"AMD\DxCache"));
                }
                else if (tweakId.Contains("bsod-dumps"))
                {
                    CleanDirectorySafe(Path.Combine(winDir, "Minidump"));
                    CleanDirectorySafe(Path.Combine(winDir, "LiveKernelReports"));
                    try
                    {
                        string memDmp = Path.Combine(winDir, "MEMORY.DMP");
                        if (File.Exists(memDmp)) File.Delete(memDmp);
                    }
                    catch { }
                }
                else if (tweakId.Contains("wer-files"))
                {
                    CleanDirectorySafe(Path.Combine(localApp, @"Microsoft\Windows\WER"));
                    CleanDirectorySafe(Path.Combine(progData, @"Microsoft\Windows\WER\ReportArchive"));
                    CleanDirectorySafe(Path.Combine(progData, @"Microsoft\Windows\WER\ReportQueue"));
                    CleanDirectorySafe(Path.Combine(progData, @"Microsoft\Windows\WER\Temp"));
                }
                else if (tweakId.Contains("windows-update-cache"))
                {
                    bool wuWasRunning = TryStopServiceForCleanup("wuauserv");
                    bool bitsWasRunning = TryStopServiceForCleanup("bits");
                    CleanDirectorySafe(Path.Combine(winDir, @"SoftwareDistribution\Download"));
                    if (bitsWasRunning) TryStartServiceAfterCleanup("bits");
                    if (wuWasRunning) TryStartServiceAfterCleanup("wuauserv");
                }
                else if (tweakId.Contains("delivery-optimization"))
                {
                    bool doWasRunning = TryStopServiceForCleanup("dosvc");
                    CleanDirectorySafe(Path.Combine(winDir, @"SoftwareDistribution\DeliveryOptimization"));
                    CleanDirectorySafe(Path.Combine(progData, @"Microsoft\Windows\DeliveryOptimization\Cache"));
                    if (doWasRunning) TryStartServiceAfterCleanup("dosvc");
                }
                else if (tweakId.Contains("system-logs"))
                {
                    RunProcess("powershell.exe", "-NoProfile -Command \"wevtutil el | ForEach-Object { wevtutil cl $_ 2>$null }\"", 30000);
                    CleanDirectorySafe(Path.Combine(winDir, "Panther"));
                }
                else if (tweakId.Contains("thumbnail-cache"))
                {
                    string explorerDir = Path.Combine(localApp, @"Microsoft\Windows\Explorer");
                    CleanFilesByPatternSafe(explorerDir, "thumbcache_*.db");
                    CleanFilesByPatternSafe(explorerDir, "iconcache_*.db");
                }
                else if (tweakId.Contains("font-cache"))
                {
                    bool fcWasRunning = TryStopServiceForCleanup("FontCache");
                    CleanDirectorySafe(Path.Combine(winDir, @"ServiceProfiles\LocalService\AppData\Local\FontCache"));
                    if (fcWasRunning) TryStartServiceAfterCleanup("FontCache");
                }
                else if (tweakId.Contains("mui-cache"))
                {
                    ClearSubkeyValuesSafe(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache");
                }
                else if (tweakId.Contains("background-history"))
                {
                    try
                    {
                        using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", true))
                        {
                            if (k != null)
                            {
                                for (int i = 0; i <= 4; i++)
                                {
                                    k.DeleteValue("BackgroundHistoryPath" + i, false);
                                }
                            }
                        }
                    }
                    catch { }
                }
                else if (tweakId.Contains("prefetch-files"))
                {
                    CleanFilesByPatternSafe(Path.Combine(winDir, "Prefetch"), "*.pf");
                }
                else if (tweakId.Contains("winsxs-resetbase"))
                {
                    RunProcess("dism.exe", "/Online /Cleanup-Image /StartComponentCleanup /ResetBase", 120000);
                }

                if (journal != null)
                {
                    journal.Log("APPLY", tweakId, "Очистка системы успешно выполнена: " + tweakId);
                }
            }
            catch (Exception ex)
            {
                if (journal != null)
                {
                    journal.Log("ERROR", tweakId, "Ошибка очистки: " + ex.Message);
                }
            }
        }

        private static void CleanupLegacyFakeRegistryKeys(string tweakId, IRegistryStore store)
        {
            try
            {
                if (tweakId == "cleanup-directx-shader-cache") store.DeleteValue("HKCU", @"Software\Microsoft\Direct3D", "D3DSCache");
                else if (tweakId == "cleanup-dns-cache") store.DeleteValue("HKLM", @"SYSTEM\CurrentControlSet\Services\Dnscache", "DnsClientCache");
                else if (tweakId == "cleanup-windows-update-cache") store.DeleteValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "SoftwareDistribution");
                else if (tweakId == "cleanup-temporary-files") store.DeleteValue("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Temp", "Temp");
                else if (tweakId == "cleanup-temporary-internet-files") store.DeleteValue("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Cache", "INetCache");
                else if (tweakId == "cleanup-delivery-optimization") store.DeleteValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DeliveryOptimization");
                else if (tweakId == "cleanup-bsod-dumps") store.DeleteValue("HKLM", @"SYSTEM\CurrentControlSet\Control\CrashControl", "LiveKernelReports");
                else if (tweakId == "cleanup-wer-files") store.DeleteValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "WER");
                else if (tweakId == "cleanup-system-logs") store.DeleteValue("HKLM", @"SYSTEM\CurrentControlSet\Control\WMI", "Logs");
                else if (tweakId == "cleanup-recycle-bin") store.DeleteValue("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\BitBucket", "RecycleBin");
                else if (tweakId == "cleanup-thumbnail-cache") store.DeleteValue("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Thumbnail");
                else if (tweakId == "cleanup-font-cache") store.DeleteValue("HKLM", @"SYSTEM\CurrentControlSet\Services\FontCache", "FontCache");
                else if (tweakId == "cleanup-prefetch-files") store.DeleteValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "Prefetch");
            }
            catch { }
        }

        private static bool TryStopServiceForCleanup(string name)
        {
            try
            {
                using (ServiceController sc = new ServiceController(name))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static void TryStartServiceAfterCleanup(string name)
        {
            try
            {
                using (ServiceController sc = new ServiceController(name))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped) sc.Start();
                }
            }
            catch { }
        }

        private static void CleanDirectorySafe(string dir)
        {
            try
            {
                if (!Directory.Exists(dir)) return;
                DirectoryInfo di = new DirectoryInfo(dir);
                FileInfo[] files = di.GetFiles();
                for (int i = 0; i < files.Length; i++) { try { files[i].Delete(); } catch { } }
                DirectoryInfo[] dirs = di.GetDirectories();
                for (int i = 0; i < dirs.Length; i++) { try { dirs[i].Delete(true); } catch { } }
            }
            catch { }
        }

        private static void CleanFilesByPatternSafe(string dir, string pattern)
        {
            try
            {
                if (!Directory.Exists(dir)) return;
                string[] files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++) { try { File.Delete(files[i]); } catch { } }
            }
            catch { }
        }

        private static void ClearSubkeyValuesSafe(RegistryKey root, string subkey)
        {
            try
            {
                using (RegistryKey k = root.OpenSubKey(subkey, true))
                {
                    if (k == null) return;
                    string[] names = k.GetValueNames();
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (!string.Equals(names[i], "LangID", StringComparison.OrdinalIgnoreCase))
                        {
                            try { k.DeleteValue(names[i], false); } catch { }
                        }
                    }
                }
            }
            catch { }
        }

        private static void DeleteSubkeyIfBogus(string subkey)
        {
            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    hklm.DeleteSubKeyTree(subkey, false);
                }
            }
            catch { }
        }

        // =====================================================================
        // 10. Process & Snapshot Utilities
        // =====================================================================
        private static int RunProcess(string fileName, string args, int timeoutMs)
        {
            string dummy;
            return RunProcessCapture(fileName, args, timeoutMs, out dummy);
        }

        private static int RunProcessCapture(string fileName, string args, int timeoutMs, out string stdout)
        {
            stdout = "";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, args);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return -1;
                    stdout = p.StandardOutput.ReadToEnd();
                    if (!p.WaitForExit(timeoutMs))
                    {
                        try { p.Kill(); } catch { }
                        return -1;
                    }
                    return p.ExitCode;
                }
            }
            catch
            {
                return -1;
            }
        }

        private static void SaveCustomSnapshot(StateManager stateManager, string tweakId, string name, string val)
        {
            if (stateManager == null) return;
            TweakSnapshot snap = new TweakSnapshot();
            snap.TweakId = tweakId;
            snap.Values.Add(new RegistryValueSnapshot
            {
                Hive = "CUSTOM",
                Path = tweakId,
                Name = name,
                Existed = !string.IsNullOrEmpty(val),
                Kind = "REG_SZ",
                Value = val
            });
            stateManager.SaveSnapshotIfAbsent(tweakId, snap);
        }

        private static string GetCustomSnapshotValue(StateManager stateManager, string tweakId, string name)
        {
            if (stateManager == null) return null;
            TweakSnapshot snap = stateManager.GetSnapshot(tweakId);
            if (snap == null || snap.Values == null) return null;
            for (int i = 0; i < snap.Values.Count; i++)
            {
                if (string.Equals(snap.Values[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return snap.Values[i].Value != null ? snap.Values[i].Value.ToString() : null;
                }
            }
            return null;
        }

        private static bool IsSysMainServiceDisabled(IRegistryStore store)
        {
            try
            {
                object st = store != null
                    ? store.GetValue("HKLM", @"SYSTEM\CurrentControlSet\Services\SysMain", "Start")
                    : null;
                if (st == null)
                {
                    using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    using (RegistryKey k = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SysMain", false))
                    {
                        st = k != null ? k.GetValue("Start") : null;
                    }
                }
                if (st != null && Convert.ToInt32(st) == 4) return true;
            }
            catch { }
            return false;
        }

        private static TweakStatus CheckMemoryCompressionDisabled(IRegistryStore store)
        {
            try
            {
                const string sfKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Superfetch";
                object disObj = store != null ? store.GetValue("HKLM", sfKey, "AdminDisable") : null;
                object enObj = store != null ? store.GetValue("HKLM", sfKey, "AdminEnable") : null;
                int dis = disObj != null ? Convert.ToInt32(disObj) : 0;
                int en = enObj != null ? Convert.ToInt32(enObj) : 0;
                if ((dis & 0x200) != 0) return TweakStatus.Enabled;
                if (IsSysMainServiceDisabled(store)) return TweakStatus.Enabled;
                if ((en & 0x200) != 0) return TweakStatus.Disabled;
            }
            catch { }
            return TweakStatus.Unknown;
        }

        private static string GetPowerSchemeFriendlyName(IRegistryStore store, string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            try
            {
                string path = @"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\" + guid;
                object fn = store != null ? store.GetValue("HKLM", path, "FriendlyName") : null;
                if (fn == null)
                {
                    using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    using (RegistryKey k = hklm.OpenSubKey(path, false))
                    {
                        fn = k != null ? k.GetValue("FriendlyName") : null;
                    }
                }
                return fn != null ? fn.ToString() : null;
            }
            catch { return null; }
        }

        private static string GetActivePowerSchemeGuid()
        {
            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey k = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", false))
                {
                    object val = k != null ? k.GetValue("ActivePowerScheme") : null;
                    if (val != null)
                    {
                        string s = val.ToString().Trim('{', '}').ToLowerInvariant();
                        if (s.Length == 36) return s;
                    }
                }
            }
            catch { }

            string stdout;
            int rc = RunProcessCapture("powercfg.exe", "/getactivescheme", 4000, out stdout);
            if (rc == 0 && !string.IsNullOrEmpty(stdout))
            {
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(
                    stdout, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
                if (m.Success) return m.Value.ToLowerInvariant();
            }
            return null;
        }

        private static bool SetNvidiaTasksEnabledViaCom(bool enabled)
        {
            try
            {
                Type comType = Type.GetTypeFromProgID("Schedule.Service");
                if (comType == null) return false;
                object sched = Activator.CreateInstance(comType);
                comType.InvokeMember("Connect", System.Reflection.BindingFlags.InvokeMethod, null, sched, null);
                object root = comType.InvokeMember("GetFolder", System.Reflection.BindingFlags.InvokeMethod, null, sched, new object[] { @"\" });
                object tasks = root.GetType().InvokeMember("GetTasks", System.Reflection.BindingFlags.InvokeMethod, null, root, new object[] { 1 });
                System.Collections.IEnumerable enumerable = tasks as System.Collections.IEnumerable;
                if (enumerable == null) return false;
                foreach (object t in enumerable)
                {
                    if (t == null) continue;
                    Type tt = t.GetType();
                    string name = tt.InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, null, t, null) as string;
                    if (!string.IsNullOrEmpty(name) &&
                        (name.StartsWith("NvTmRep", StringComparison.OrdinalIgnoreCase) ||
                         name.StartsWith("NvTmMon", StringComparison.OrdinalIgnoreCase)))
                    {
                        tt.InvokeMember("Enabled", System.Reflection.BindingFlags.SetProperty, null, t, new object[] { enabled });
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static TweakStatus CheckNvidiaTasksDisabledStatus()
        {
            try
            {
                Type comType = Type.GetTypeFromProgID("Schedule.Service");
                if (comType != null)
                {
                    object sched = Activator.CreateInstance(comType);
                    comType.InvokeMember("Connect", System.Reflection.BindingFlags.InvokeMethod, null, sched, null);
                    object root = comType.InvokeMember("GetFolder", System.Reflection.BindingFlags.InvokeMethod, null, sched, new object[] { @"\" });
                    object tasks = root.GetType().InvokeMember("GetTasks", System.Reflection.BindingFlags.InvokeMethod, null, root, new object[] { 1 });
                    System.Collections.IEnumerable enumerable = tasks as System.Collections.IEnumerable;
                    if (enumerable != null)
                    {
                        int found = 0;
                        int disabled = 0;
                        foreach (object t in enumerable)
                        {
                            if (t == null) continue;
                            Type tt = t.GetType();
                            string name = tt.InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, null, t, null) as string;
                            if (!string.IsNullOrEmpty(name) &&
                                (name.StartsWith("NvTmRep", StringComparison.OrdinalIgnoreCase) ||
                                 name.StartsWith("NvTmMon", StringComparison.OrdinalIgnoreCase)))
                            {
                                found++;
                                object enObj = tt.InvokeMember("Enabled", System.Reflection.BindingFlags.GetProperty, null, t, null);
                                if (enObj is bool && !((bool)enObj)) disabled++;
                            }
                        }
                        if (found == 0 || disabled == found) return TweakStatus.Enabled;
                        return TweakStatus.Disabled;
                    }
                }
            }
            catch { }

            string stdout;
            int rc = RunProcessCapture("powershell.exe",
                "-NoProfile -Command \"$t = Get-ScheduledTask -TaskName 'NvTmRep*','NvTmMon*' -ErrorAction SilentlyContinue; if (-not $t) { 'NONE' } elseif (($t | Where-Object { $_.State -ne 'Disabled' }).Count -eq 0) { 'DISABLED' } else { 'ENABLED' }\"",
                6000, out stdout);
            if (rc == 0 && !string.IsNullOrEmpty(stdout))
            {
                string s = stdout.Trim();
                if (s.IndexOf("DISABLED", StringComparison.OrdinalIgnoreCase) >= 0) return TweakStatus.Enabled;
                if (s.IndexOf("NONE", StringComparison.OrdinalIgnoreCase) >= 0) return TweakStatus.Enabled;
                if (s.IndexOf("ENABLED", StringComparison.OrdinalIgnoreCase) >= 0) return TweakStatus.Disabled;
            }
            return TweakStatus.Unknown;
        }

        private static Dictionary<string, bool> GetScheduledTaskEnabledMap()
        {
            lock (_cacheLock)
            {
                if (_scheduledTaskCache != null && (DateTime.UtcNow - _scheduledTaskCacheTime).TotalSeconds < 4.0)
                {
                    return _scheduledTaskCache;
                }
            }

            Dictionary<string, bool> map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            try
            {
                Type comType = Type.GetTypeFromProgID("Schedule.Service");
                if (comType != null)
                {
                    object sched = Activator.CreateInstance(comType);
                    comType.InvokeMember("Connect", System.Reflection.BindingFlags.InvokeMethod, null, sched, null);
                    object root = comType.InvokeMember("GetFolder", System.Reflection.BindingFlags.InvokeMethod, null, sched, new object[] { @"\" });
                    TraverseTaskFolder(root, map);
                }
            }
            catch { }

            lock (_cacheLock)
            {
                _scheduledTaskCache = map;
                _scheduledTaskCacheTime = DateTime.UtcNow;
            }
            return map;
        }

        private static void TraverseTaskFolder(object folder, Dictionary<string, bool> map)
        {
            if (folder == null) return;
            Type ft = folder.GetType();
            try
            {
                object tasks = ft.InvokeMember("GetTasks", System.Reflection.BindingFlags.InvokeMethod, null, folder, new object[] { 1 });
                System.Collections.IEnumerable enumTasks = tasks as System.Collections.IEnumerable;
                if (enumTasks != null)
                {
                    foreach (object t in enumTasks)
                    {
                        if (t == null) continue;
                        Type tt = t.GetType();
                        string path = tt.InvokeMember("Path", System.Reflection.BindingFlags.GetProperty, null, t, null) as string;
                        object enObj = tt.InvokeMember("Enabled", System.Reflection.BindingFlags.GetProperty, null, t, null);
                        if (!string.IsNullOrEmpty(path) && enObj is bool)
                        {
                            map[path] = (bool)enObj;
                        }
                    }
                }
            }
            catch { }

            try
            {
                object subFolders = ft.InvokeMember("GetFolders", System.Reflection.BindingFlags.InvokeMethod, null, folder, new object[] { 0 });
                System.Collections.IEnumerable enumFolders = subFolders as System.Collections.IEnumerable;
                if (enumFolders != null)
                {
                    foreach (object sf in enumFolders)
                    {
                        TraverseTaskFolder(sf, map);
                    }
                }
            }
            catch { }
        }

        private static TweakStatus CheckScheduledTaskGroupStatus(string[] taskPaths)
        {
            int found, disabled;
            return CheckScheduledTaskGroupStatus(taskPaths, out found, out disabled);
        }

        private static TweakStatus CheckScheduledTaskGroupStatus(string[] taskPaths, out int found, out int disabled)
        {
            found = 0;
            disabled = 0;
            if (taskPaths == null || taskPaths.Length == 0) return TweakStatus.Unavailable;

            Dictionary<string, bool> map = GetScheduledTaskEnabledMap();
            for (int i = 0; i < taskPaths.Length; i++)
            {
                bool enabled;
                if (map.TryGetValue(taskPaths[i], out enabled))
                {
                    found++;
                    if (!enabled) disabled++;
                }
            }

            if (found == 0) return TweakStatus.Unavailable;
            if (disabled == found) return TweakStatus.Enabled;
            if (disabled > 0) return TweakStatus.PartiallyEnabled;
            return TweakStatus.Disabled;
        }

        private static bool SetScheduledTaskEnabled(string fullTaskPath, bool enabled)
        {
            if (string.IsNullOrEmpty(fullTaskPath)) return false;
            try
            {
                Type comType = Type.GetTypeFromProgID("Schedule.Service");
                if (comType != null)
                {
                    object sched = Activator.CreateInstance(comType);
                    comType.InvokeMember("Connect", System.Reflection.BindingFlags.InvokeMethod, null, sched, null);
                    int idx = fullTaskPath.LastIndexOf('\\');
                    string folderPath = idx > 0 ? fullTaskPath.Substring(0, idx) : @"\";
                    string taskName = idx >= 0 ? fullTaskPath.Substring(idx + 1) : fullTaskPath;
                    object folder = comType.InvokeMember("GetFolder", System.Reflection.BindingFlags.InvokeMethod, null, sched, new object[] { folderPath });
                    object task = folder.GetType().InvokeMember("GetTask", System.Reflection.BindingFlags.InvokeMethod, null, folder, new object[] { taskName });
                    task.GetType().InvokeMember("Enabled", System.Reflection.BindingFlags.SetProperty, null, task, new object[] { enabled });
                    return true;
                }
            }
            catch { }

            string flag = enabled ? "/ENABLE" : "/DISABLE";
            int rc = RunProcess("schtasks.exe", string.Format("/Change /TN \"{0}\" {1}", fullTaskPath, flag), 4000);
            return rc == 0;
        }

        private static bool ApplyScheduledTaskGroup(string id, string[] taskPaths, StateManager stateManager, JournalManager journal)
        {
            Dictionary<string, bool> map = GetScheduledTaskEnabledMap();
            TweakSnapshot snap = new TweakSnapshot();
            snap.TweakId = id;

            int modified = 0;
            int found = 0;
            for (int i = 0; i < taskPaths.Length; i++)
            {
                string tp = taskPaths[i];
                bool curEnabled;
                if (!map.TryGetValue(tp, out curEnabled)) continue;
                found++;
                snap.Values.Add(new RegistryValueSnapshot
                {
                    Hive = "TASK",
                    Path = tp,
                    Name = "Enabled",
                    Existed = true,
                    Kind = "DWord",
                    Value = curEnabled ? 1 : 0
                });
                if (!curEnabled || SetScheduledTaskEnabled(tp, false))
                {
                    modified++;
                }
            }

            if (found == 0)
            {
                if (journal != null) journal.Log("WARN", id, "Задачи планировщика из этой группы отсутствуют в установленной редакции Windows");
                return false;
            }

            if (stateManager != null) stateManager.SaveSnapshotIfAbsent(id, snap);
            InvalidateCaches();
            if (journal != null)
            {
                journal.Log("APPLY", id, string.Format("Отключены задачи планировщика ({0} из {1})", modified, found));
            }
            return modified > 0;
        }

        private static bool RevertScheduledTaskGroup(string id, string[] taskPaths, StateManager stateManager, JournalManager journal)
        {
            TweakSnapshot snap = stateManager != null ? stateManager.GetSnapshot(id) : null;
            int restored = 0;
            if (snap != null && snap.Values != null && snap.Values.Count > 0)
            {
                for (int i = 0; i < snap.Values.Count; i++)
                {
                    var v = snap.Values[i];
                    if (string.Equals(v.Hive, "TASK", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(v.Path))
                    {
                        bool origEnabled = v.Value == null || Convert.ToInt64(v.Value) != 0;
                        if (origEnabled && SetScheduledTaskEnabled(v.Path, true)) restored++;
                    }
                }
            }
            else if (taskPaths != null)
            {
                Dictionary<string, bool> map = GetScheduledTaskEnabledMap();
                for (int i = 0; i < taskPaths.Length; i++)
                {
                    if (map.ContainsKey(taskPaths[i]) && SetScheduledTaskEnabled(taskPaths[i], true)) restored++;
                }
            }
            if (stateManager != null) stateManager.RemoveSnapshot(id);
            InvalidateCaches();
            if (journal != null)
            {
                journal.Log("REVERT", id, string.Format("Задачи планировщика восстановлены ({0})", restored));
            }
            return true;
        }

        // =====================================================================
        // 6. IPv6 Full Disable (0xFF + ms_tcpip6 binding)
        // =====================================================================
        private static bool? IsAnyIpv6BindingEnabled()
        {
            lock (_cacheLock)
            {
                if ((DateTime.UtcNow - _ipv6BindingCacheTime).TotalSeconds < 5.0)
                {
                    return _ipv6BindingAnyEnabledCache;
                }
            }

            bool? result = null;
            try
            {
                NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
                bool anyEnabled = false;
                for (int i = 0; i < ifaces.Length; i++)
                {
                    NetworkInterface ni = ifaces[i];
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        continue;
                    }
                    if (ni.Supports(NetworkInterfaceComponent.IPv6))
                    {
                        anyEnabled = true;
                        break;
                    }
                }
                result = anyEnabled;
            }
            catch
            {
                string stdout;
                int rc = RunProcessCapture("powershell.exe",
                    "-NoProfile -Command \"$b = Get-NetAdapterBinding -ComponentID ms_tcpip6 -ErrorAction SilentlyContinue; if (-not $b) { 'NONE' } elseif (($b | Where-Object { $_.Enabled }).Count -gt 0) { 'ENABLED' } else { 'DISABLED' }\"",
                    5000, out stdout);
                if (rc == 0 && !string.IsNullOrEmpty(stdout))
                {
                    string s = stdout.Trim();
                    if (s.IndexOf("ENABLED", StringComparison.OrdinalIgnoreCase) >= 0) result = true;
                    else if (s.IndexOf("DISABLED", StringComparison.OrdinalIgnoreCase) >= 0) result = false;
                }
            }

            lock (_cacheLock)
            {
                _ipv6BindingAnyEnabledCache = result;
                _ipv6BindingCacheTime = DateTime.UtcNow;
            }
            return result;
        }

        private static TweakStatus CheckIpv6FullDisabledStatus(IRegistryStore store)
        {
            const string path = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters";
            object val = store != null ? store.GetValue("HKLM", path, "DisabledComponents") : null;
            bool regDisabled = (val != null && Convert.ToInt64(val) == 255);
            bool? anyBinding = IsAnyIpv6BindingEnabled();

            if (regDisabled && (!anyBinding.HasValue || !anyBinding.Value)) return TweakStatus.Enabled;
            if (regDisabled || (anyBinding.HasValue && !anyBinding.Value)) return TweakStatus.PartiallyEnabled;
            return TweakStatus.Disabled;
        }

        private static string GetIpv6LiveDisplayValue(IRegistryStore store)
        {
            const string path = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters";
            object val = store != null ? store.GetValue("HKLM", path, "DisabledComponents") : null;
            long num = val != null ? Convert.ToInt64(val) : -1;
            string regText = num == 255 ? "0xFF (отключен)" : (num == 32 ? "0x20 (Prefer IPv4)" : (num >= 0 ? num.ToString() : "Не задано"));
            bool? anyBinding = IsAnyIpv6BindingEnabled();
            string bindText = !anyBinding.HasValue ? "адаптеры н/д" : (anyBinding.Value ? "ms_tcpip6: вкл." : "ms_tcpip6: откл.");
            return regText + " | " + bindText;
        }

        private static bool ApplyIpv6FullDisable(IRegistryStore store, StateManager stateManager, JournalManager journal)
        {
            const string regPath = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters";
            TweakSnapshot snap = new TweakSnapshot();
            snap.TweakId = "network-disable-ipv6-components";

            bool existed = store.ValueExists("HKLM", regPath, "DisabledComponents");
            object prev = existed ? store.GetValue("HKLM", regPath, "DisabledComponents") : null;
            snap.Values.Add(new RegistryValueSnapshot
            {
                Hive = "HKLM",
                Path = regPath,
                Name = "DisabledComponents",
                Existed = existed,
                Kind = "REG_DWORD",
                Value = prev != null ? prev.ToString() : null
            });

            string enabledList;
            int qRc = RunProcessCapture("powershell.exe",
                "-NoProfile -Command \"((Get-NetAdapterBinding -ComponentID ms_tcpip6 -ErrorAction SilentlyContinue | Where-Object { $_.Enabled } | ForEach-Object { $_.Name }) -join '|')\"",
                5000, out enabledList);
            if (qRc == 0 && !string.IsNullOrEmpty(enabledList))
            {
                snap.Values.Add(new RegistryValueSnapshot
                {
                    Hive = "CUSTOM",
                    Path = "network-disable-ipv6-components",
                    Name = "Ipv6EnabledAdapters",
                    Existed = true,
                    Kind = "REG_SZ",
                    Value = enabledList.Trim()
                });
            }

            stateManager.SaveSnapshotIfAbsent("network-disable-ipv6-components", snap);
            store.SetValue("HKLM", regPath, "DisabledComponents", 255, RegistryValueKind.DWord);
            RunProcess("powershell.exe",
                "-NoProfile -Command \"Disable-NetAdapterBinding -Name '*' -ComponentID ms_tcpip6 -Confirm:$false -ErrorAction SilentlyContinue\"",
                8000);
            InvalidateCaches();
            if (journal != null)
            {
                journal.Log("APPLY", "network-disable-ipv6-components", "IPv6 полностью отключен: DisabledComponents=0xFF + ms_tcpip6 отключен на адаптерах");
            }
            return true;
        }

        private static bool RevertIpv6FullDisable(IRegistryStore store, StateManager stateManager, JournalManager journal)
        {
            const string regPath = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters";
            TweakSnapshot snap = stateManager.GetSnapshot("network-disable-ipv6-components");
            string savedAdapters = null;
            bool restoredReg = false;

            if (snap != null && snap.Values != null)
            {
                for (int i = 0; i < snap.Values.Count; i++)
                {
                    RegistryValueSnapshot v = snap.Values[i];
                    if (string.Equals(v.Hive, "CUSTOM", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(v.Name, "Ipv6EnabledAdapters", StringComparison.OrdinalIgnoreCase))
                    {
                        savedAdapters = v.Value != null ? v.Value.ToString() : null;
                    }
                    else if (string.Equals(v.Name, "DisabledComponents", StringComparison.OrdinalIgnoreCase))
                    {
                        if (v.Existed && v.Value != null)
                        {
                            int prevVal;
                            if (int.TryParse(v.Value.ToString(), out prevVal))
                            {
                                store.SetValue("HKLM", regPath, "DisabledComponents", prevVal, RegistryValueKind.DWord);
                            }
                            else
                            {
                                store.SetValue("HKLM", regPath, "DisabledComponents", 0, RegistryValueKind.DWord);
                            }
                        }
                        else
                        {
                            store.SetValue("HKLM", regPath, "DisabledComponents", 0, RegistryValueKind.DWord);
                        }
                        restoredReg = true;
                    }
                }
            }

            if (!restoredReg)
            {
                store.SetValue("HKLM", regPath, "DisabledComponents", 0, RegistryValueKind.DWord);
            }

            if (!string.IsNullOrEmpty(savedAdapters))
            {
                string[] names = savedAdapters.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < names.Length; i++)
                {
                    string safeName = names[i].Replace("'", "''");
                    RunProcess("powershell.exe",
                        "-NoProfile -Command \"Enable-NetAdapterBinding -Name '" + safeName + "' -ComponentID ms_tcpip6 -Confirm:$false -ErrorAction SilentlyContinue\"",
                        6000);
                }
            }
            else
            {
                RunProcess("powershell.exe",
                    "-NoProfile -Command \"Enable-NetAdapterBinding -Name '*' -ComponentID ms_tcpip6 -Confirm:$false -ErrorAction SilentlyContinue\"",
                    8000);
            }

            stateManager.RemoveSnapshot("network-disable-ipv6-components");
            InvalidateCaches();
            if (journal != null)
            {
                journal.Log("REVERT", "network-disable-ipv6-components", "Настройки IPv6 (DisabledComponents и привязка ms_tcpip6) восстановлены");
            }
            return true;
        }

        // =====================================================================
        // 7. Sleep Study Full Disable (Registry + Diagnostic Channels + Task)
        // =====================================================================
        private static readonly string[] SleepStudyChannels = new string[]
        {
            "Microsoft-Windows-SleepStudy/Diagnostic",
            "Microsoft-Windows-Kernel-Processor-Power/Diagnostic",
            "Microsoft-Windows-UserModePowerService/Diagnostic"
        };

        private static bool ApplySleepStudyDisable(IRegistryStore store, StateManager stateManager, JournalManager journal)
        {
            const string smPower = @"SYSTEM\CurrentControlSet\Control\Session Manager\Power";
            const string ctrlPower = @"SYSTEM\CurrentControlSet\Control\Power";
            const string id = "privacy-disable-sleep-study";

            TweakSnapshot snap = new TweakSnapshot();
            snap.TweakId = id;
            SaveRegDwordToSnap(snap, store, "HKLM", smPower, "SleepStudyDisabled");
            SaveRegDwordToSnap(snap, store, "HKLM", ctrlPower, "SleepstudyAccountingEnabled");
            stateManager.SaveSnapshotIfAbsent(id, snap);

            store.SetValue("HKLM", smPower, "SleepStudyDisabled", 1, RegistryValueKind.DWord);
            store.SetValue("HKLM", ctrlPower, "SleepstudyAccountingEnabled", 0, RegistryValueKind.DWord);

            for (int i = 0; i < SleepStudyChannels.Length; i++)
            {
                RunProcess("wevtutil.exe", "sl \"" + SleepStudyChannels[i] + "\" /e:false", 4000);
            }
            RunProcess("schtasks.exe", "/Change /TN \"\\Microsoft\\Windows\\Power Efficiency Diagnostics\\AnalyzeSystem\" /DISABLE", 4000);

            if (journal != null)
            {
                journal.Log("APPLY", id, "Sleep Study отключен (SleepStudyDisabled=1, каналы WINEVT и AnalyzeSystem)");
            }
            return true;
        }

        private static bool RevertSleepStudyDisable(IRegistryStore store, StateManager stateManager, JournalManager journal)
        {
            const string id = "privacy-disable-sleep-study";
            TweakSnapshot snap = stateManager.GetSnapshot(id);
            if (snap != null && snap.Values != null && snap.Values.Count > 0)
            {
                for (int i = 0; i < snap.Values.Count; i++)
                {
                    RestoreSnapshotEntry(store, snap.Values[i]);
                }
            }
            else
            {
                store.SetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "SleepStudyDisabled", 0, RegistryValueKind.DWord);
                store.SetValue("HKLM", @"SYSTEM\CurrentControlSet\Control\Power", "SleepstudyAccountingEnabled", 1, RegistryValueKind.DWord);
            }
            RunProcess("schtasks.exe", "/Change /TN \"\\Microsoft\\Windows\\Power Efficiency Diagnostics\\AnalyzeSystem\" /ENABLE", 4000);
            stateManager.RemoveSnapshot(id);
            if (journal != null)
            {
                journal.Log("REVERT", id, "Учёт Sleep Study возвращён к состоянию до применения");
            }
            return true;
        }

        private static void SaveRegDwordToSnap(TweakSnapshot snap, IRegistryStore store, string hive, string path, string name)
        {
            bool exists = store.ValueExists(hive, path, name);
            object val = exists ? store.GetValue(hive, path, name) : null;
            snap.Values.Add(new RegistryValueSnapshot
            {
                Hive = hive,
                Path = path,
                Name = name,
                Existed = exists,
                Kind = "REG_DWORD",
                Value = val != null ? val.ToString() : null
            });
        }

        private static void RestoreSnapshotEntry(IRegistryStore store, RegistryValueSnapshot v)
        {
            if (v == null || string.Equals(v.Hive, "CUSTOM", StringComparison.OrdinalIgnoreCase)) return;
            if (!v.Existed)
            {
                store.DeleteValue(v.Hive, v.Path, v.Name);
                return;
            }
            if (string.Equals(v.Kind, "REG_SZ", StringComparison.OrdinalIgnoreCase))
            {
                store.SetValue(v.Hive, v.Path, v.Name, v.Value != null ? v.Value.ToString() : "", RegistryValueKind.String);
            }
            else
            {
                int num;
                if (v.Value != null && int.TryParse(v.Value.ToString(), out num))
                {
                    store.SetValue(v.Hive, v.Path, v.Name, num, RegistryValueKind.DWord);
                }
            }
        }

        // =====================================================================
        // 8. Noverse App Guides: Spotify / VS Code / LGHUB / SteelSeries
        // =====================================================================
        private static string GetSpotifyAppDataDir()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spotify");
        }

        public static bool IsSpotifyInstalled()
        {
            try
            {
                string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Directory.Exists(GetSpotifyAppDataDir())
                    || Directory.Exists(Path.Combine(pf, "Spotify"))
                    || (!string.IsNullOrEmpty(pf86) && Directory.Exists(Path.Combine(pf86, "Spotify")))
                    || File.Exists(Path.Combine(localApp, @"Microsoft\WindowsApps\Spotify.exe"));
            }
            catch { return false; }
        }

        private static readonly string[,] SpotifyGlobalPrefs = new string[,]
        {
            { "app.autostart-configured", "true" },
            { "app.autostart-mode", "\"off\"" },
            { "network.proxy.mode", "1" },
            { "ui.hardware_acceleration", "false" }
        };

        private static readonly string[,] SpotifyUserPrefs = new string[,]
        {
            { "audio.play_bitrate_non_metered_enumeration", "0" },
            { "audio.play_bitrate_enumeration", "0" },
            { "audio.sync_bitrate_enumeration", "0" },
            { "audio.allow_downgrade", "true" },
            { "audio.normalize_v2", "false" },
            { "ui.hide_hpto", "true" },
            { "ui.system_media_controls_enabled", "false" },
            { "ui.right_panel_content", "0" },
            { "audio.crossfade_v2", "false" },
            { "audio.automix", "true" },
            { "audio.downmixer_v2", "false" },
            { "audio.silence_trimmer_v2", "false" },
            { "ui.minimize_to_tray", "false" }
        };

        public static TweakStatus CheckSpotifyConfigInDir(string spotifyDir)
        {
            if (string.IsNullOrEmpty(spotifyDir) || !Directory.Exists(spotifyDir)) return TweakStatus.Disabled;
            string globalPrefs = Path.Combine(spotifyDir, "prefs");
            if (!File.Exists(globalPrefs)) return TweakStatus.Disabled;

            Dictionary<string, string> parsed = ReadPrefsMap(globalPrefs);
            int matched = 0;
            int total = SpotifyGlobalPrefs.GetLength(0);
            for (int i = 0; i < total; i++)
            {
                string val;
                if (parsed.TryGetValue(SpotifyGlobalPrefs[i, 0], out val) &&
                    string.Equals(val, SpotifyGlobalPrefs[i, 1], StringComparison.OrdinalIgnoreCase))
                {
                    matched++;
                }
            }
            if (matched == total) return TweakStatus.Enabled;
            return matched > 0 ? TweakStatus.PartiallyEnabled : TweakStatus.Disabled;
        }

        public static bool ApplySpotifyConfigToDir(string spotifyDir, out string detail)
        {
            detail = "";
            if (string.IsNullOrEmpty(spotifyDir))
            {
                detail = "Не указан каталог Spotify";
                return false;
            }
            try
            {
                StopProcessesSafe(new string[] { "Spotify" });
                Directory.CreateDirectory(spotifyDir);

                string globalPrefs = Path.Combine(spotifyDir, "prefs");
                WritePrefsWithBackup(globalPrefs, SpotifyGlobalPrefs);

                string usersDir = Path.Combine(spotifyDir, "Users");
                int userProfilesUpdated = 0;
                if (Directory.Exists(usersDir))
                {
                    string[] subdirs = Directory.GetDirectories(usersDir);
                    for (int i = 0; i < subdirs.Length; i++)
                    {
                        string userPrefs = Path.Combine(subdirs[i], "prefs");
                        if (File.Exists(userPrefs))
                        {
                            WritePrefsWithBackup(userPrefs, SpotifyUserPrefs);
                            userProfilesUpdated++;
                        }
                    }
                }
                detail = string.Format("Spotify prefs обновлены (профилей: {0})", userProfilesUpdated);
                return true;
            }
            catch (Exception ex)
            {
                detail = "Ошибка настройки Spotify prefs: " + ex.Message;
                return false;
            }
        }

        public static bool RevertSpotifyConfigInDir(string spotifyDir, out string detail)
        {
            detail = "";
            if (string.IsNullOrEmpty(spotifyDir) || !Directory.Exists(spotifyDir))
            {
                detail = "Каталог Spotify не найден";
                return false;
            }
            try
            {
                StopProcessesSafe(new string[] { "Spotify" });
                int restoredCount = 0;
                if (RestoreFromAxisBackup(Path.Combine(spotifyDir, "prefs"))) restoredCount++;

                string usersDir = Path.Combine(spotifyDir, "Users");
                if (Directory.Exists(usersDir))
                {
                    string[] subdirs = Directory.GetDirectories(usersDir);
                    for (int i = 0; i < subdirs.Length; i++)
                    {
                        if (RestoreFromAxisBackup(Path.Combine(subdirs[i], "prefs"))) restoredCount++;
                    }
                }
                detail = string.Format("Конфигурация Spotify prefs восстановлена ({0} файл(ов))", restoredCount);
                return true;
            }
            catch (Exception ex)
            {
                detail = "Ошибка отката Spotify prefs: " + ex.Message;
                return false;
            }
        }

        private static Dictionary<string, string> ReadPrefsMap(string path)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path)) return map;
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string k = line.Substring(0, eq).Trim();
                string v = line.Substring(eq + 1).Trim();
                map[k] = v;
            }
            return map;
        }

        private static void WritePrefsWithBackup(string path, string[,] pairs)
        {
            string bak = path + ".axis.bak";
            if (!File.Exists(bak) && File.Exists(path))
            {
                File.Copy(path, bak, true);
            }

            List<string> lines = new List<string>();
            if (File.Exists(path))
            {
                string[] existing = File.ReadAllLines(path, Encoding.UTF8);
                for (int i = 0; i < existing.Length; i++)
                {
                    if (!string.IsNullOrEmpty(existing[i])) lines.Add(existing[i]);
                }
            }

            int count = pairs.GetLength(0);
            for (int i = 0; i < count; i++)
            {
                string key = pairs[i, 0];
                string val = pairs[i, 1];
                string entry = key + "=" + val;
                int foundIdx = -1;
                for (int j = 0; j < lines.Count; j++)
                {
                    if (lines[j].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                    {
                        foundIdx = j;
                        break;
                    }
                }
                if (foundIdx >= 0) lines[foundIdx] = entry;
                else lines.Add(entry);
            }
            File.WriteAllLines(path, lines.ToArray(), new UTF8Encoding(false));
        }

        private static bool RestoreFromAxisBackup(string targetPath)
        {
            string bak = targetPath + ".axis.bak";
            if (!File.Exists(bak)) return false;
            File.Copy(bak, targetPath, true);
            try { File.Delete(bak); } catch { }
            return true;
        }

        public static string[] GetInstalledVSCodeUserDirs()
        {
            List<string> dirs = new List<string>();
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                string codeUser = Path.Combine(appData, @"Code\User");
                bool hasCode = Directory.Exists(Path.Combine(appData, "Code"))
                    || Directory.Exists(Path.Combine(localApp, @"Programs\Microsoft VS Code"))
                    || Directory.Exists(Path.Combine(pf, "Microsoft VS Code"))
                    || (!string.IsNullOrEmpty(pf86) && Directory.Exists(Path.Combine(pf86, "Microsoft VS Code")));
                if (hasCode) dirs.Add(codeUser);

                string codiumUser = Path.Combine(appData, @"VSCodium\User");
                if (Directory.Exists(Path.Combine(appData, "VSCodium"))) dirs.Add(codiumUser);

                string insidersUser = Path.Combine(appData, @"Code - Insiders\User");
                if (Directory.Exists(Path.Combine(appData, "Code - Insiders"))) dirs.Add(insidersUser);
            }
            catch { }
            return dirs.ToArray();
        }

        private static readonly KeyValuePair<string, object>[] VSCodeTelemetrySettings = new KeyValuePair<string, object>[]
        {
            new KeyValuePair<string, object>("telemetry.telemetryLevel", "off"),
            new KeyValuePair<string, object>("telemetry.enableCrashReporter", false),
            new KeyValuePair<string, object>("telemetry.enableTelemetry", false),
            new KeyValuePair<string, object>("telemetry.feedback.enabled", false),
            new KeyValuePair<string, object>("workbench.enableExperiments", false),
            new KeyValuePair<string, object>("update.mode", "manual"),
            new KeyValuePair<string, object>("update.showReleaseNotes", false),
            new KeyValuePair<string, object>("extensions.ignoreRecommendations", true),
            new KeyValuePair<string, object>("git.autofetch", false),
            new KeyValuePair<string, object>("npm.fetchOnlinePackageInfo", false)
        };

        public static TweakStatus CheckVSCodeConfigInDir(string userDir)
        {
            if (string.IsNullOrEmpty(userDir)) return TweakStatus.Disabled;
            string settingsPath = Path.Combine(userDir, "settings.json");
            if (!File.Exists(settingsPath)) return TweakStatus.Disabled;

            try
            {
                string raw = File.ReadAllText(settingsPath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(raw)) return TweakStatus.Disabled;
                JavaScriptSerializer js = new JavaScriptSerializer();
                Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(raw);
                if (dict == null) return TweakStatus.Disabled;

                int matched = 0;
                for (int i = 0; i < VSCodeTelemetrySettings.Length; i++)
                {
                    object cur;
                    if (dict.TryGetValue(VSCodeTelemetrySettings[i].Key, out cur) && cur != null &&
                        string.Equals(cur.ToString(), VSCodeTelemetrySettings[i].Value.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        matched++;
                    }
                }
                if (matched == VSCodeTelemetrySettings.Length) return TweakStatus.Enabled;
                return matched > 0 ? TweakStatus.PartiallyEnabled : TweakStatus.Disabled;
            }
            catch
            {
                return TweakStatus.Unknown;
            }
        }

        public static bool ApplyVSCodeConfigToDir(string userDir, out string detail)
        {
            detail = "";
            if (string.IsNullOrEmpty(userDir))
            {
                detail = "Не указан путь профиля VS Code";
                return false;
            }
            try
            {
                Directory.CreateDirectory(userDir);
                string settingsPath = Path.Combine(userDir, "settings.json");
                string bakPath = settingsPath + ".axis.bak";
                if (!File.Exists(bakPath) && File.Exists(settingsPath))
                {
                    File.Copy(settingsPath, bakPath, true);
                }

                Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.Ordinal);
                if (File.Exists(settingsPath))
                {
                    string raw = File.ReadAllText(settingsPath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        Dictionary<string, object> existing = js.Deserialize<Dictionary<string, object>>(raw);
                        if (existing != null) dict = existing;
                    }
                }

                for (int i = 0; i < VSCodeTelemetrySettings.Length; i++)
                {
                    dict[VSCodeTelemetrySettings[i].Key] = VSCodeTelemetrySettings[i].Value;
                }

                string serialized = FormatJsonDictionary(dict);
                File.WriteAllText(settingsPath, serialized, new UTF8Encoding(false));
                detail = "Обновлён файл " + settingsPath;
                return true;
            }
            catch (Exception ex)
            {
                detail = "Ошибка записи settings.json: " + ex.Message;
                return false;
            }
        }

        public static bool RevertVSCodeConfigInDir(string userDir, out string detail)
        {
            detail = "";
            if (string.IsNullOrEmpty(userDir))
            {
                detail = "Не указан путь профиля VS Code";
                return false;
            }
            try
            {
                string settingsPath = Path.Combine(userDir, "settings.json");
                if (RestoreFromAxisBackup(settingsPath))
                {
                    detail = "Восстановлен settings.json из резервной копии";
                    return true;
                }
                if (File.Exists(settingsPath))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(File.ReadAllText(settingsPath, Encoding.UTF8));
                    if (dict != null)
                    {
                        for (int i = 0; i < VSCodeTelemetrySettings.Length; i++)
                        {
                            dict.Remove(VSCodeTelemetrySettings[i].Key);
                        }
                        File.WriteAllText(settingsPath, FormatJsonDictionary(dict), new UTF8Encoding(false));
                    }
                }
                detail = "Удалены добавленные параметры телеметрии VS Code";
                return true;
            }
            catch (Exception ex)
            {
                detail = "Ошибка отката settings.json: " + ex.Message;
                return false;
            }
        }

        private static bool ApplyVSCodeConfigAll(JournalManager journal, string id)
        {
            string[] dirs = GetInstalledVSCodeUserDirs();
            if (dirs.Length == 0)
            {
                if (journal != null) journal.Log("WARN", id, "VS Code / VSCodium не установлен");
                return false;
            }
            bool anyOk = false;
            for (int i = 0; i < dirs.Length; i++)
            {
                string detail;
                if (ApplyVSCodeConfigToDir(dirs[i], out detail)) anyOk = true;
                if (journal != null) journal.Log(anyOk ? "APPLY" : "WARN", id, detail);
            }
            return anyOk;
        }

        private static bool RevertVSCodeConfigAll(JournalManager journal, string id)
        {
            string[] dirs = GetInstalledVSCodeUserDirs();
            if (dirs.Length == 0) return false;
            bool anyOk = false;
            for (int i = 0; i < dirs.Length; i++)
            {
                string detail;
                if (RevertVSCodeConfigInDir(dirs[i], out detail)) anyOk = true;
                if (journal != null) journal.Log("REVERT", id, detail);
            }
            return anyOk;
        }

        private static string FormatJsonDictionary(Dictionary<string, object> dict)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            int idx = 0;
            int total = dict.Count;
            JavaScriptSerializer js = new JavaScriptSerializer();
            foreach (KeyValuePair<string, object> kv in dict)
            {
                idx++;
                string comma = (idx < total) ? "," : "";
                sb.AppendLine("  " + js.Serialize(kv.Key) + ": " + js.Serialize(kv.Value) + comma);
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static readonly string[] LghubServices = new string[] { "LGHUBUpdaterService", "logi_joy_bus_enum", "logi_joy_vir_hid" };
        private static readonly string[] SteelSeriesServices = new string[] { "SteelSeriesSonar", "SteelSeriesGGClient", "SteelSeriesAudioService" };

        private static bool IsAppInstalledByDirsOrServices(string[] services, string[] dirs, IRegistryStore store)
        {
            for (int i = 0; i < services.Length; i++)
            {
                if (store != null && store.ValueExists("HKLM", @"SYSTEM\CurrentControlSet\Services\" + services[i], "Start"))
                {
                    return true;
                }
            }
            for (int i = 0; i < dirs.Length; i++)
            {
                if (!string.IsNullOrEmpty(dirs[i]) && Directory.Exists(dirs[i])) return true;
            }
            return false;
        }

        private static TweakStatus CheckLghubToggleStatus(IRegistryStore store)
        {
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] dirs = new string[]
            {
                Path.Combine(pf, "LGHUB"),
                !string.IsNullOrEmpty(pf86) ? Path.Combine(pf86, "LGHUB") : "",
                Path.Combine(localApp, "LGHUB")
            };
            if (!IsAppInstalledByDirsOrServices(LghubServices, dirs, store)) return TweakStatus.Unavailable;
            return CheckServicesDisabledStatus(LghubServices, "LGHUB", store);
        }

        private static TweakStatus CheckSteelSeriesStatus(IRegistryStore store)
        {
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string[] dirs = new string[]
            {
                Path.Combine(pf, "SteelSeries"),
                !string.IsNullOrEmpty(pf86) ? Path.Combine(pf86, "SteelSeries") : "",
                Path.Combine(localApp, "SteelSeries"),
                Path.Combine(progData, "SteelSeries")
            };
            if (!IsAppInstalledByDirsOrServices(SteelSeriesServices, dirs, store)) return TweakStatus.Unavailable;
            return CheckServicesDisabledStatus(SteelSeriesServices, "SteelSeriesGG", store);
        }

        private static TweakStatus CheckServicesDisabledStatus(string[] services, string runKeyName, IRegistryStore store)
        {
            int found = 0;
            int disabled = 0;
            for (int i = 0; i < services.Length; i++)
            {
                string svcPath = @"SYSTEM\CurrentControlSet\Services\" + services[i];
                if (store.ValueExists("HKLM", svcPath, "Start"))
                {
                    found++;
                    object st = store.GetValue("HKLM", svcPath, "Start");
                    if (st != null && Convert.ToInt64(st) == 4) disabled++;
                }
            }
            bool hasAutostart = store.ValueExists("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Run", runKeyName);
            if (found > 0 && disabled == found && !hasAutostart) return TweakStatus.Enabled;
            if (disabled > 0) return TweakStatus.PartiallyEnabled;
            return TweakStatus.Disabled;
        }

        private static bool ApplyLghubToggle(bool disable, IRegistryStore store, StateManager stateManager, JournalManager journal, string id)
        {
            return ApplyOrRevertAppServices(
                disable,
                LghubServices,
                new string[] { "lghub", "lghub_agent", "lghub_updater", "logi_lamparray_service" },
                new string[] { "LGHUB" },
                new int[] { 3, 1, 3 },
                store, stateManager, journal, id, "Logitech G HUB");
        }

        private static bool ApplySteelSeriesSonar(bool disable, IRegistryStore store, StateManager stateManager, JournalManager journal, string id)
        {
            return ApplyOrRevertAppServices(
                disable,
                SteelSeriesServices,
                new string[] { "SteelSeriesGG", "SteelSeriesGGClient", "SteelSeriesSonar", "SteelSeriesEngine" },
                new string[] { "SteelSeriesGG", "SteelSeriesGGClient", "SteelSeries Sonar" },
                new int[] { 2, 2, 2 },
                store, stateManager, journal, id, "SteelSeries GG / Sonar");
        }

        private static bool ApplyOrRevertAppServices(
            bool disable,
            string[] services,
            string[] procNames,
            string[] runNames,
            int[] fallbackStartModes,
            IRegistryStore store,
            StateManager stateManager,
            JournalManager journal,
            string id,
            string appLabel)
        {
            const string hkcuRun = @"Software\Microsoft\Windows\CurrentVersion\Run";
            if (disable)
            {
                StopProcessesSafe(procNames);
                TweakSnapshot snap = new TweakSnapshot();
                snap.TweakId = id;

                for (int i = 0; i < services.Length; i++)
                {
                    string svcPath = @"SYSTEM\CurrentControlSet\Services\" + services[i];
                    if (store.ValueExists("HKLM", svcPath, "Start"))
                    {
                        SaveRegDwordToSnap(snap, store, "HKLM", svcPath, "Start");
                        TryStopServiceForCleanup(services[i]);
                        store.SetValue("HKLM", svcPath, "Start", 4, RegistryValueKind.DWord);
                    }
                }
                for (int i = 0; i < runNames.Length; i++)
                {
                    if (store.ValueExists("HKCU", hkcuRun, runNames[i]))
                    {
                        object runVal = store.GetValue("HKCU", hkcuRun, runNames[i]);
                        snap.Values.Add(new RegistryValueSnapshot
                        {
                            Hive = "HKCU",
                            Path = hkcuRun,
                            Name = runNames[i],
                            Existed = true,
                            Kind = "REG_SZ",
                            Value = runVal != null ? runVal.ToString() : ""
                        });
                        store.DeleteValue("HKCU", hkcuRun, runNames[i]);
                    }
                }
                stateManager.SaveSnapshotIfAbsent(id, snap);
                if (journal != null) journal.Log("APPLY", id, appLabel + ": службы отключены (Start=4) и автозапуск удалён");
                return true;
            }
            else
            {
                TweakSnapshot snap = stateManager.GetSnapshot(id);
                if (snap != null && snap.Values != null && snap.Values.Count > 0)
                {
                    for (int i = 0; i < snap.Values.Count; i++)
                    {
                        RestoreSnapshotEntry(store, snap.Values[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < services.Length; i++)
                    {
                        string svcPath = @"SYSTEM\CurrentControlSet\Services\" + services[i];
                        if (store.ValueExists("HKLM", svcPath, "Start"))
                        {
                            int mode = i < fallbackStartModes.Length ? fallbackStartModes[i] : 3;
                            store.SetValue("HKLM", svcPath, "Start", mode, RegistryValueKind.DWord);
                        }
                    }
                }
                stateManager.RemoveSnapshot(id);
                if (journal != null) journal.Log("REVERT", id, appLabel + ": параметры служб и автозапуска восстановлены");
                return true;
            }
        }

        private static void StopProcessesSafe(string[] procNames)
        {
            if (procNames == null) return;
            for (int i = 0; i < procNames.Length; i++)
            {
                try
                {
                    Process[] procs = Process.GetProcessesByName(procNames[i]);
                    for (int j = 0; j < procs.Length; j++)
                    {
                        try { procs[j].Kill(); procs[j].WaitForExit(1500); } catch { }
                        try { procs[j].Dispose(); } catch { }
                    }
                }
                catch { }
            }
        }
    }
}
