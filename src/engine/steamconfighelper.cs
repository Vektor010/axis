using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
