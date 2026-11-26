using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WinTweaker.Engine
{
    public static class SteamConfigHelper
    {
        public static string GetSteamPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("SteamPath");
                        if (val != null && !string.IsNullOrEmpty(val.ToString()))
                        {
                            return val.ToString().Replace('/', '\\');
                        }
                    }
                }
            }
            catch { }

            string defaultPath = @"C:\Program Files (x86)\Steam";
            if (Directory.Exists(defaultPath))
            {
                return defaultPath;
            }

            return null;
        }

        public static bool IsSteamAutoRunDisabled()
        {
            try
            {
                // 1. Проверяем нативный флаг Valve SuppressAutoRun в HKCU\Software\Valve\Steam
                using (RegistryKey valveKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (valveKey != null)
                    {
                        object suppress = valveKey.GetValue("SuppressAutoRun");
                        if (suppress != null)
                        {
                            int suppressInt;
                            if (int.TryParse(suppress.ToString(), out suppressInt) && suppressInt == 1)
                            {
                                return true;
                            }
                        }
                    }
                }

                // 2. Проверяем наличие ключа автозапуска в HKCU\Software\Microsoft\Windows\CurrentVersion\Run
                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (runKey == null || runKey.GetValue("Steam") == null)
                    {
                        return true;
                    }
                }

                // 3. Проверяем состояние автозагрузки в Диспетчере задач Windows (StartupApproved\Run)
                using (RegistryKey appKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run"))
                {
                    if (appKey != null)
                    {
                        byte[] bytes = appKey.GetValue("Steam") as byte[];
                        if (bytes != null && bytes.Length > 0)
                        {
                            // В Windows 10/11:
                            // 0x02 = Включено (Enabled)
                            // 0x03 или 0x01 = Отключено пользователем через Диспетчер задач / Параметры
                            if (bytes[0] != 2)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool SetSteamAutoRunDisabled(bool disable)
        {
            try
            {
                // 1. Устанавливаем SuppressAutoRun в HKCU\Software\Valve\Steam
                using (RegistryKey valveKey = Registry.CurrentUser.CreateSubKey(@"Software\Valve\Steam"))
                {
                    if (valveKey != null)
                    {
                        valveKey.SetValue("SuppressAutoRun", disable ? 1 : 0, RegistryValueKind.DWord);
                    }
                }

                // 2. Синхронизируем Windows StartupApproved\Run
                using (RegistryKey appKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run"))
                {
                    if (appKey != null)
                    {
                        byte[] bytes = appKey.GetValue("Steam") as byte[];
                        if (bytes != null && bytes.Length > 0)
                        {
                            bytes[0] = (byte)(disable ? 0x03 : 0x02);
                            appKey.SetValue("Steam", bytes, RegistryValueKind.Binary);
                        }
                        else
                        {
                            byte[] defaultBytes = new byte[12];
                            defaultBytes[0] = (byte)(disable ? 0x03 : 0x02);
                            appKey.SetValue("Steam", defaultBytes, RegistryValueKind.Binary);
                        }
                    }
                }

                // 3. Если включаем автозапуск (disable == false), восстанавливаем Run\Steam при отсутствии
                if (!disable)
                {
                    using (RegistryKey runKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                    {
                        if (runKey != null && runKey.GetValue("Steam") == null)
                        {
                            string steamPath = GetSteamPath();
                            if (!string.IsNullOrEmpty(steamPath))
                            {
                                string exePath = Path.Combine(steamPath, "steam.exe");
                                if (File.Exists(exePath))
                                {
                                    runKey.SetValue("Steam", string.Format("\"{0}\" -silent", exePath), RegistryValueKind.String);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string[] GetLocalConfigVdfPaths()
        {
            try
            {
                string steamPath = GetSteamPath();
                if (string.IsNullOrEmpty(steamPath))
                {
                    return new string[0];
                }

                string userDataDir = Path.Combine(steamPath, "userdata");
                if (!Directory.Exists(userDataDir))
                {
                    return new string[0];
                }

                string[] userDirs = Directory.GetDirectories(userDataDir);
                List<string> list = new List<string>();
                for (int i = 0; i < userDirs.Length; i++)
                {
                    string vdfPath = Path.Combine(userDirs[i], @"config\localconfig.vdf");
                    if (File.Exists(vdfPath))
                    {
                        list.Add(vdfPath);
                    }
                }
                return list.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        public static int? GetGameOverlaySetting()
        {
            try
            {
                string[] vdfFiles = GetLocalConfigVdfPaths();
                if (vdfFiles.Length == 0)
                {
                    return null;
                }

                for (int i = 0; i < vdfFiles.Length; i++)
                {
                    string text = File.ReadAllText(vdfFiles[i]);
                    Match m = Regex.Match(text, "\"EnableGameOverlay\"\\s+\"([0-9]+)\"");
                    if (m.Success)
                    {
                        int res;
                        if (int.TryParse(m.Groups[1].Value, out res))
                        {
                            return res;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public static bool SetGameOverlaySetting(int value)
        {
            try
            {
                string[] vdfFiles = GetLocalConfigVdfPaths();
                if (vdfFiles.Length == 0)
                {
                    return false;
                }

                bool modified = false;
                for (int i = 0; i < vdfFiles.Length; i++)
                {
                    string path = vdfFiles[i];
                    string text = File.ReadAllText(path);
                    Match m = Regex.Match(text, "\"EnableGameOverlay\"\\s+\"([0-9]+)\"");
                    if (m.Success)
                    {
                        // Резервная копия .bak
                        string bak = path + ".bak";
                        if (!File.Exists(bak))
                        {
                            File.Copy(path, bak, true);
                        }

                        string replaced = Regex.Replace(text, "\"EnableGameOverlay\"\\s+\"[0-9]+\"", string.Format("\"EnableGameOverlay\"\t\t\"{0}\"", value));
                        File.WriteAllText(path, replaced);
                        modified = true;
                    }
                }
                return modified;
            }
            catch
            {
                return false;
            }
        }

        public static string GetLocalConfigValue(string sectionName, string keyName)
        {
            return GetLocalConfigValueFromFiles(GetLocalConfigVdfPaths(), sectionName, keyName);
        }

        public static string GetLocalConfigValueFromFiles(string[] vdfFiles, string sectionName, string keyName)
        {
            try
            {
                if (vdfFiles == null || vdfFiles.Length == 0 || string.IsNullOrEmpty(keyName)) return null;
                for (int i = 0; i < vdfFiles.Length; i++)
                {
                    if (!File.Exists(vdfFiles[i])) continue;
                    string text = File.ReadAllText(vdfFiles[i], Encoding.UTF8);
                    string val = ReadVdfKeyValue(text, sectionName, keyName);
                    if (val != null) return val;
                }
            }
            catch { }
            return null;
        }

        public static bool SetLocalConfigValue(string sectionName, string keyName, string value)
        {
            return SetLocalConfigValueInFiles(GetLocalConfigVdfPaths(), sectionName, keyName, value);
        }

        public static bool SetLocalConfigValueInFiles(string[] vdfFiles, string sectionName, string keyName, string value)
        {
            try
            {
                if (vdfFiles == null || vdfFiles.Length == 0 || string.IsNullOrEmpty(keyName)) return false;
                bool anyModified = false;
                for (int i = 0; i < vdfFiles.Length; i++)
                {
                    string path = vdfFiles[i];
                    if (!File.Exists(path)) continue;
                    string text = File.ReadAllText(path, Encoding.UTF8);
                    string bak = path + ".bak";
                    if (!File.Exists(bak))
                    {
                        File.Copy(path, bak, true);
                    }
                    string updated = WriteVdfKeyValue(text, sectionName, keyName, value);
                    if (updated != null)
                    {
                        File.WriteAllText(path, updated, new UTF8Encoding(false));
                        anyModified = true;
                    }
                }
                return anyModified;
            }
            catch
            {
                return false;
            }
        }

        private static string ReadVdfKeyValue(string text, string sectionName, string keyName)
        {
            if (string.IsNullOrEmpty(text)) return null;
            bool isRoot = string.IsNullOrEmpty(sectionName) || string.Equals(sectionName, "root", StringComparison.OrdinalIgnoreCase);
            int blockStart, blockEnd;
            if (!FindVdfSectionRange(text, isRoot ? null : sectionName, out blockStart, out blockEnd)) return null;

            string slice = text.Substring(blockStart, blockEnd - blockStart);
            if (isRoot)
            {
                // For root, only match keys outside deeper braces
                string shallow = StripNestedVdfBlocks(slice);
                Match mr = Regex.Match(shallow, "\"" + Regex.Escape(keyName) + "\"\\s+\"([^\"]*)\"", RegexOptions.IgnoreCase);
                return mr.Success ? mr.Groups[1].Value : null;
            }

            Match m = Regex.Match(slice, "\"" + Regex.Escape(keyName) + "\"\\s+\"([^\"]*)\"", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : null;
        }

        private static string WriteVdfKeyValue(string text, string sectionName, string keyName, string value)
        {
            if (string.IsNullOrEmpty(text)) return null;
            int blockStart, blockEnd;
            bool isRoot = string.IsNullOrEmpty(sectionName) || string.Equals(sectionName, "root", StringComparison.OrdinalIgnoreCase);

            if (!FindVdfSectionRange(text, isRoot ? null : sectionName, out blockStart, out blockEnd))
            {
                if (isRoot) return null;
                int rootStart, rootEnd;
                if (!FindVdfSectionRange(text, null, out rootStart, out rootEnd)) return null;
                string newSection = string.Format("\r\n\t\"{0}\"\r\n\t{{\r\n\t\t\"{1}\"\t\t\"{2}\"\r\n\t}}\r\n", sectionName, keyName, value);
                return text.Insert(rootEnd, newSection);
            }

            string inner = text.Substring(blockStart, blockEnd - blockStart);
            Regex keyRx = new Regex("(\"" + Regex.Escape(keyName) + "\"\\s+\")[^\"]*(\")", RegexOptions.IgnoreCase);
            if (!isRoot && keyRx.IsMatch(inner))
            {
                string replacedInner = keyRx.Replace(inner, "${1}" + value + "${2}", 1);
                return text.Substring(0, blockStart) + replacedInner + text.Substring(blockEnd);
            }

            if (isRoot)
            {
                // Replace only top-level key if present in root depth
                string replacedRoot;
                if (TryReplaceShallowVdfKey(inner, keyName, value, out replacedRoot))
                {
                    return text.Substring(0, blockStart) + replacedRoot + text.Substring(blockEnd);
                }
            }

            string indent = isRoot ? "\t" : "\t\t";
            string insertion = string.Format("\r\n{0}\"{1}\"\t\t\"{2}\"\r\n", indent, keyName, value);
            return text.Insert(blockEnd, insertion);
        }

        private static bool FindVdfSectionRange(string text, string sectionName, out int contentStart, out int contentEnd)
        {
            contentStart = 0;
            contentEnd = 0;
            if (string.IsNullOrEmpty(sectionName))
            {
                int firstBrace = text.IndexOf('{');
                if (firstBrace < 0) return false;
                int close = FindMatchingBrace(text, firstBrace);
                if (close < 0) return false;
                contentStart = firstBrace + 1;
                contentEnd = close;
                return true;
            }

            Match m = Regex.Match(text, "\"" + Regex.Escape(sectionName) + "\"\\s*\\{", RegexOptions.IgnoreCase);
            if (!m.Success) return false;
            int openBrace = text.IndexOf('{', m.Index);
            int closeBrace = FindMatchingBrace(text, openBrace);
            if (closeBrace < 0) return false;
            contentStart = openBrace + 1;
            contentEnd = closeBrace;
            return true;
        }

        private static int FindMatchingBrace(string text, int openIndex)
        {
            int depth = 0;
            bool inQuotes = false;
            for (int i = openIndex; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\\' && inQuotes) { i++; continue; }
                if (c == '"') { inQuotes = !inQuotes; continue; }
                if (!inQuotes)
                {
                    if (c == '{') depth++;
                    else if (c == '}')
                    {
                        depth--;
                        if (depth == 0) return i;
                    }
                }
            }
            return -1;
        }

        private static string StripNestedVdfBlocks(string slice)
        {
            StringBuilder sb = new StringBuilder(slice.Length);
            int depth = 0;
            bool inQuotes = false;
            for (int i = 0; i < slice.Length; i++)
            {
                char c = slice[i];
                if (c == '\\' && inQuotes && i + 1 < slice.Length) { if (depth == 0) sb.Append(c).Append(slice[i + 1]); i++; continue; }
                if (c == '"') { inQuotes = !inQuotes; if (depth == 0) sb.Append(c); continue; }
                if (!inQuotes)
                {
                    if (c == '{') { depth++; continue; }
                    if (c == '}') { if (depth > 0) depth--; continue; }
                }
                if (depth == 0) sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool TryReplaceShallowVdfKey(string inner, string keyName, string newValue, out string updated)
        {
            updated = inner;
            int depth = 0;
            bool inQuotes = false;
            for (int i = 0; i < inner.Length; i++)
            {
                char c = inner[i];
                if (c == '\\' && inQuotes) { i++; continue; }
                if (c == '"')
                {
                    if (!inQuotes && depth == 0)
                    {
                        string sub = inner.Substring(i);
                        Match m = Regex.Match(sub, "^\"" + Regex.Escape(keyName) + "\"(\\s+)\"([^\"]*)\"", RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            string replacement = string.Format("\"{0}\"{1}\"{2}\"", keyName, m.Groups[1].Value, newValue);
                            updated = inner.Substring(0, i) + replacement + inner.Substring(i + m.Length);
                            return true;
                        }
                    }
                    inQuotes = !inQuotes;
                    continue;
                }
                if (!inQuotes)
                {
                    if (c == '{') depth++;
                    else if (c == '}' && depth > 0) depth--;
                }
            }
            return false;
        }
    }
}
