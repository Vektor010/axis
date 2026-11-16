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

        string[] files = new[] { "security/desc.md", "privacy/desc.md", "visibility/desc.md", "system/desc.md" };

        var regPattern = new Regex(@"\|\s*\[([^\]]+)\]\([^\)]+\)\s*\|\s*`([^`]+)`\s*\|\s*`([^`]+)`", RegexOptions.Compiled);

        int found = 0;
        foreach (var f in files)
        {
            string fullPath = Path.Combine(srcDir, f.Replace('/', '\\'));
            if (!File.Exists(fullPath)) continue;

            string[] lines = File.ReadAllLines(fullPath, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                var match = regPattern.Match(lines[i]);
                if (match.Success)
                {
                    string title = match.Groups[1].Value.Trim();
                    string fullKey = match.Groups[2].Value.Trim();
                    string valName = match.Groups[3].Value.Trim();

                    // If multiple lines or complex formatting
                    if (valName.Contains("<br>"))
                    {
                        valName = valName.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Trim('`');
                    }

                    string hive = fullKey.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase) ? "HKLM" :
                                 fullKey.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase) ? "HKCU" : "";
                    if (string.IsNullOrEmpty(hive)) continue;

                    string subPath = fullKey.Substring(5);
                    string fullReg = hive + "\\" + subPath + "\\" + valName;

                    if (!existingRegKeys.Contains(fullReg))
                    {
                        Console.WriteLine("Index: " + (found + 1));
                        Console.WriteLine("File: " + f + ":" + (i + 1));
                        Console.WriteLine("Title: " + title);
                        Console.WriteLine("Hive: " + hive + " | Path: " + subPath + " | Name: " + valName);
                        Console.WriteLine("---------------------------------------------");
                        existingRegKeys.Add(fullReg); // prevent dups in output
                        found++;
                        if (found >= 40) return;
                    }
                }
            }
        }
    }
}
