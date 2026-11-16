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
