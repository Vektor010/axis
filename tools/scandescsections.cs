using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

class Program
{
    static void Main()
    {
        string root = @"C:\Users\User\Documents\win-tweaker";
        string srcDir = Path.Combine(root, @"data\source\win-config");
        string tweaksPath = Path.Combine(root, @"data\tweaks.json");

        var serializer = new JavaScriptSerializer();
        var existingTweaks = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksPath, Encoding.UTF8));

        var existingRegKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in existingTweaks)
        {
            if (t.ContainsKey("registry") && t["registry"] is System.Collections.ArrayList)
            {
                var regList = t["registry"] as System.Collections.ArrayList;
                foreach (Dictionary<string, object> r in regList)
                {
                    existingRegKeys.Add(r["hive"] + "\\" + r["path"] + "\\" + r["name"]);
                }
            }
        }

        string[] mdFiles = Directory.GetFiles(srcDir, "desc.md", SearchOption.AllDirectories);

        // Pattern for table rows with | Title | Path | Value |
        // or code blocks with [HKEY_LOCAL_MACHINE\...] or "ValueName"=dword:...
        foreach (var file in mdFiles)
        {
            string relFile = file.Substring(srcDir.Length).TrimStart('\\').Replace('\\', '/');
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            Console.WriteLine("=== File: " + relFile + " (lines: " + lines.Length + ") ===");

            int found = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Check for HKLM or HKCU in table row
                if (line.StartsWith("|") && (line.IndexOf("HKLM\\", StringComparison.OrdinalIgnoreCase) >= 0 || line.IndexOf("HKCU\\", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4)
                    {
                        string title = parts[1].Trim();
                        string rawPath = parts[2].Trim().Replace("`", "");
                        string rawVal = parts[3].Trim().Replace("`", "");

                        // Extract hive
                        string hive = "HKLM";
                        string cleanPath = rawPath;
                        if (rawPath.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase))
                        {
                            hive = "HKCU";
                            cleanPath = rawPath.Substring(5);
                        }
                        else if (rawPath.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase))
                        {
                            hive = "HKLM";
                            cleanPath = rawPath.Substring(5);
                        }

                        // Check if value is simple name
                        if (!rawVal.Contains("<") && !rawVal.Contains(">") && !rawVal.Contains(" ") && rawVal.Length > 1 && !rawVal.Contains("-"))
                        {
                            string key = hive + "\\" + cleanPath + "\\" + rawVal;
                            if (!existingRegKeys.Contains(key))
                            {
                                found++;
                                if (found <= 20)
                                {
                                    Console.WriteLine(string.Format("  Line {0}: [{1}] {2} -> {3}\\{4} : {5}", i + 1, relFile, title, hive, cleanPath, rawVal));
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("  Total unused in " + relFile + ": " + found);
        }
    }
}
