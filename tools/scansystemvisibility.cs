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

        string[] mdFiles = new[] { 
            Path.Combine(srcDir, @"security\desc.md"), 
            Path.Combine(srcDir, @"power\desc.md"),
            Path.Combine(srcDir, @"network\desc.md"),
            Path.Combine(srcDir, @"peripheral\desc.md")
        };

        var tablePattern = new Regex(@"\|\s*(.*?)\s*\|\s*`?(HKLM|HKCU)\\(.*?)`?\s*\|\s*(.*)", RegexOptions.IgnoreCase);

        int count = 0;
        foreach (var file in mdFiles)
        {
            string relFile = file.Substring(srcDir.Length).TrimStart('\\').Replace('\\', '/');
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!line.StartsWith("|")) continue;

                var m = tablePattern.Match(line);
                if (m.Success)
                {
                    string rawTitle = m.Groups[1].Value.Trim();
                    string hive = m.Groups[2].Value.ToUpperInvariant();
                    string rawPath = m.Groups[3].Value.Trim();
                    string rawVal = m.Groups[4].Value.Trim();

                    string[] paths = rawPath.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] vals = rawVal.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

                    var linkMatch = Regex.Match(rawTitle, @"\[([^\]]+)\]");
                    string title = linkMatch.Success ? linkMatch.Groups[1].Value : rawTitle;

                    foreach (var p in paths)
                    {
                        string cleanPath = p.Replace("`", "").Replace("|", "").Trim();
                        if (cleanPath.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase)) cleanPath = cleanPath.Substring(5);
                        if (cleanPath.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase)) cleanPath = cleanPath.Substring(5);

                        foreach (var v in vals)
                        {
                            string cleanVal = v.Replace("`", "").Replace("|", "").Trim();
                            if (string.IsNullOrEmpty(cleanVal) || cleanVal.Equals("Value Name", StringComparison.OrdinalIgnoreCase) || cleanVal.Contains("---") || cleanVal.Contains("<") || cleanVal.Contains(">")) continue;

                            string fullReg = hive + "\\" + cleanPath + "\\" + cleanVal;
                            if (!existingRegKeys.Contains(fullReg))
                            {
                                count++;
                                Console.WriteLine("CANDIDATE " + count + ":");
                                Console.WriteLine("  File: " + relFile + " (line " + (i + 1) + ")");
                                Console.WriteLine("  Title: " + title);
                                Console.WriteLine("  Hive: " + hive);
                                Console.WriteLine("  Path: " + cleanPath);
                                Console.WriteLine("  Name: " + cleanVal);
                                Console.WriteLine("  Full: " + fullReg);
                                Console.WriteLine();
                                existingRegKeys.Add(fullReg);
                                if (count >= 25) return;
                            }
                        }
                    }
                }
            }
        }
    }
}
