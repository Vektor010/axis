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

        var regPathRegex = new Regex(@"\[(HKEY_LOCAL_MACHINE|HKEY_CURRENT_USER)\\(.*?)\]", RegexOptions.IgnoreCase);
        var regValRegex = new Regex(@"""([^""]+)""\s*=\s*(dword:[0-9a-fA-F]+|""[^""]*""|hex:[0-9a-fA-F,]+)", RegexOptions.IgnoreCase);

        foreach (var file in mdFiles)
        {
            string relFile = file.Substring(srcDir.Length).TrimStart('\\').Replace('\\', '/');
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            string currentHeading = "";
            int headingLine = 0;
            string currentPath = "";
            string currentHive = "";

            int fileFound = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith("#"))
                {
                    currentHeading = line.Trim();
                    headingLine = i + 1;
                    currentPath = "";
                    currentHive = "";
                }

                var pathMatch = regPathRegex.Match(line);
                if (pathMatch.Success)
                {
                    currentHive = pathMatch.Groups[1].Value.Equals("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase) ? "HKLM" : "HKCU";
                    currentPath = pathMatch.Groups[2].Value.Trim();
                }

                if (!string.IsNullOrEmpty(currentPath))
                {
                    var valMatch = regValRegex.Match(line);
                    if (valMatch.Success)
                    {
                        string valName = valMatch.Groups[1].Value.Trim();
                        string valData = valMatch.Groups[2].Value.Trim();
                        string fullKey = currentHive + "\\" + currentPath + "\\" + valName;

                        if (!existingRegKeys.Contains(fullKey))
                        {
                            fileFound++;
                            if (fileFound <= 15)
                            {
                                Console.WriteLine(string.Format("File: {0} | Heading: {1} (line {2}) | {3} | {4} = {5}",
                                    relFile, currentHeading, headingLine, fullKey, valName, valData));
                            }
                            existingRegKeys.Add(fullKey); // avoid reporting same in this file multiple times
                        }
                    }
                }
            }
            Console.WriteLine(string.Format("=== {0}: {1} unused reg entries ===", relFile, fileFound));
        }
    }
}
