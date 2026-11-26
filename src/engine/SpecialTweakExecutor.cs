using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
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
            if (string.Equals(id, "network-congestion-provider-bbr", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "network-interrupt-moderation-low", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "power-disable-hibernation", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "power-usb-audio-idle-timeout", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-noverse-minimal-profile", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-on-demand-nvcpl", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-powermizer-maximum-performance", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(id, "nvidia-telemetry-client-optout", StringComparison.OrdinalIgnoreCase)) return true;
            if (id.StartsWith("steam-vdf-", StringComparison.OrdinalIgnoreCase)) return true;
            if (IsCleanupAction(tweak)) return true;
            return false;
        }

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

            results.Add(rawPath);
            return results;
        }

        private static List<string> EnumerateRealPhysicalNicSubkeys()
        {
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
            return keys;
        }

        private static List<string> EnumerateAudioPowerSettingsSubkeys()
        {
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
            return keys;
        }

        private static List<string> EnumerateNvidiaDisplaySubkeys()
        {
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
            return keys;
        }

        private static List<string> EnumerateTargetPciDevicePaths(string tweakId)
        {
            List<string> paths = new List<string>();
            bool wantDisplay = tweakId != null && (tweakId.Contains("-gpu-") || tweakId.Contains("msi-mode-gpu") || tweakId.Contains("device-policy-spread"));
            bool wantNet = tweakId != null && (tweakId.Contains("-nic-") || tweakId.Contains("device-policy-spread"));
            bool wantUsbXhci = tweakId != null && tweakId.Contains("device-policy-spread");

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

            // Noverse High Performance Power Plan (SCHEME_MIN)
            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase))
            {
                string activeGuid = GetActivePowerSchemeGuid();
                if (string.IsNullOrEmpty(activeGuid)) return TweakStatus.Unknown;
                return string.Equals(activeGuid, "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", StringComparison.OrdinalIgnoreCase)
                    ? TweakStatus.Enabled : TweakStatus.Disabled;
            }

            // NVIDIA Scheduled Tasks
            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase))
            {
                if (EnumerateNvidiaDisplaySubkeys().Count == 0) return TweakStatus.Unavailable;
                return CheckNvidiaTasksDisabledStatus();
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

            // NVIDIA DRS Minimal Profile
            if (string.Equals(id, "nvidia-noverse-minimal-profile", StringComparison.OrdinalIgnoreCase))
            {
                int matched, total;
                string summary;
                if (!NvDrsService.TryQueryMinimalProfile(out matched, out total, out summary))
                {
                    return TweakStatus.Unavailable;
                }
                if (matched == total) return TweakStatus.Enabled;
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
                if (svcDisabled && File.Exists(desktopShortcut)) return TweakStatus.Enabled;
                if (svcDisabled) return TweakStatus.PartiallyEnabled;
                return TweakStatus.Disabled;
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
                stateManager.SaveSnapshot(id, snap);
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
                int rc = RunProcess("powershell.exe", "-NoProfile -Command \"Disable-MMAgent -MemoryCompression\"", 12000);
                SaveCustomSnapshot(stateManager, id, "MemCompression", "Enabled");
                journal.Log(rc == 0 ? "APPLY" : "ERROR", id, "Выполнен Disable-MMAgent -MemoryCompression (exit=" + rc + ")");
                return rc == 0;
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
                stateManager.SaveSnapshot(id, snap);
                journal.Log("APPLY", id, string.Format("Отключена модерация прерываний (*InterruptModeration=0) для {0} NIC", nics.Count));
                return true;
            }

            // 2e) Noverse High Performance Power Plan
            if (string.Equals(id, "power-noverse-high-performance-plan", StringComparison.OrdinalIgnoreCase))
            {
                string prevGuid = GetActivePowerSchemeGuid();
                SaveCustomSnapshot(stateManager, id, "PrevPowerScheme", prevGuid ?? "");
                int rc = RunProcess("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", 8000);
                if (rc != 0)
                {
                    RunProcess("powercfg.exe", "/duplicatescheme 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", 8000);
                    rc = RunProcess("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", 8000);
                }
                // Apply Noverse High Perf indexes: processor min=100, USB selective suspend=0, PCIe ASPM=0, disk idle=0
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100", 4000);
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0", 4000);
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT SUB_PCIEXPRESS ASPM 0", 4000);
                RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT SUB_DISK DISKIDLE 0", 4000);
                RunProcess("powercfg.exe", "/setactive SCHEME_CURRENT", 4000);
                journal.Log(rc == 0 ? "APPLY" : "ERROR", id, "Активирована схема High Performance (Noverse PowerPlan, exit=" + rc + ")");
                return rc == 0;
            }

            // 2f) NVIDIA Scheduled Tasks
            if (string.Equals(id, "nvidia-disable-scheduled-tasks", StringComparison.OrdinalIgnoreCase))
            {
                int rc = RunProcess("powershell.exe", "-NoProfile -Command \"Get-ScheduledTask -TaskName 'NvTmRep*','NvTmMon*' -ErrorAction SilentlyContinue | Disable-ScheduledTask | Out-Null\"", 10000);
                SaveCustomSnapshot(stateManager, id, "NvTasks", "Disabled");
                journal.Log("APPLY", id, "Задачи телеметрии NVIDIA (NvTmRep*/NvTmMon*) отключены");
                return rc == 0;
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
                stateManager.SaveSnapshot(id, snap);
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
                stateManager.SaveSnapshot(id, snap);
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
                stateManager.SaveSnapshot(id, snap);
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
                int rc = RunProcess("powershell.exe", "-NoProfile -Command \"Get-ScheduledTask -TaskName 'NvTmRep*','NvTmMon*' -ErrorAction SilentlyContinue | Enable-ScheduledTask | Out-Null\"", 10000);
                stateManager.RemoveSnapshot(id);
                journal.Log("REVERT", id, "Задачи NVIDIA повторно включены (при наличии в системе)");
                return rc == 0;
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
                string v = FindBcdLineValue(enumText, "custom:16000069", "custom:0x16000069");
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
                        RunProcess("powershell.exe", "-NoProfile -Command \"" + psCreateLnk.Replace("\"", "\\\"") + "\"", 10000);
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
                            try { sc.Stop(); sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(4)); } catch { }
                        }
                    }
                    else if (targetStart == 2)
                    {
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            try { sc.Start(); sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(4)); } catch { }
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
            stateManager.SaveSnapshot(tweakId, snap);
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
                if ((en & 0x200) != 0) return TweakStatus.Disabled;
            }
            catch { }
            return TweakStatus.Unknown;
        }

        private static string GetActivePowerSchemeGuid()
        {
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

        private static TweakStatus CheckNvidiaTasksDisabledStatus()
        {
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
    }
}
