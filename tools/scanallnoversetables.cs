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
        var existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

        string[] mdFiles = Directory.GetFiles(srcDir, "*.md", SearchOption.AllDirectories);

        var rowRegex = new Regex(@"^\|\s*(.*?)\s*\|\s*`?(HKLM|HKCU)\\(.*?)`?\s*\|\s*`?([A-Za-z0-9_ -]+)`?\s*\|", RegexOptions.IgnoreCase);

        int count = 0;
        foreach (var file in mdFiles)
        {
            string relFile = file.Substring(srcDir.Length).TrimStart('\\').Replace('\\', '/');
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!line.StartsWith("|")) continue;

                var m = rowRegex.Match(line);
                if (m.Success)
                {
                    string rawTitle = m.Groups[1].Value;
                    string hive = m.Groups[2].Value.ToUpperInvariant();
                    string path = m.Groups[3].Value.Trim().Trim('`');
                    string name = m.Groups[4].Value.Trim().Trim('`');

                    // Clean title if markdown link
                    var linkMatch = Regex.Match(rawTitle, @"\[([^\]]+)\]");
                    string title = linkMatch.Success ? linkMatch.Groups[1].Value : rawTitle.Trim();

                    // Remove possible <br> from path
                    if (path.Contains("<br>"))
                    {
                        path = path.Split(new[] { "<br>" }, StringSplitOptions.None)[0].Trim().Trim('`');
                    }
                    if (name.Contains("<br>"))
                    {
                        name = name.Split(new[] { "<br>" }, StringSplitOptions.None)[0].Trim().Trim('`');
                    }

                    string fullKey = hive + "\\" + path + "\\" + name;
                    if (!existingRegKeys.Contains(fullKey) && !string.IsNullOrEmpty(name) && !name.Equals("Value Name", StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                        if (count >= 25 && count <= 65)
                        {
                            Console.WriteLine("CANDIDATE " + count + ":");
                            Console.WriteLine("  File: " + relFile + " (line " + (i + 1) + ")");
                            Console.WriteLine("  Title: " + title);
                            Console.WriteLine("  Hive: " + hive);
                            Console.WriteLine("  Path: " + path);
                            Console.WriteLine("  Name: " + name);
                            Console.WriteLine("  Full: " + fullKey);
                            Console.WriteLine();
                        }
                        existingRegKeys.Add(fullKey);

                        if (count >= 70) return;
                    }
                }
            }
        }
    }
}
