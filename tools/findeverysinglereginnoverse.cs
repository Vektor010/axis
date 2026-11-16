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
        serializer.MaxJsonLength = int.MaxValue;
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

        string[] mdFiles = Directory.GetFiles(srcDir, "*.md", SearchOption.AllDirectories);

        var unusedCandidates = new List<string>();

        foreach (var file in mdFiles)
        {
            string relFile = file.Substring(srcDir.Length).TrimStart('\\').Replace('\\', '/');
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith("|") && (line.IndexOf("HKLM", StringComparison.OrdinalIgnoreCase) >= 0 || line.IndexOf("HKCU", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var cols = line.Split('|');
                    if (cols.Length >= 4)
                    {
                        string title = cols[1].Trim();
                        string p = cols[2].Replace("`", "").Trim();
                        string v = cols[3].Replace("`", "").Trim();

                        string[] pArr = p.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                        string[] vArr = v.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var pathItem in pArr)
                        {
                            string cleanP = pathItem.Trim();
                            string hive = cleanP.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase) ? "HKCU" : "HKLM";
                            if (cleanP.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase)) cleanP = cleanP.Substring(5);
                            if (cleanP.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase)) cleanP = cleanP.Substring(5);

                            foreach (var valItem in vArr)
                            {
                                string cleanV = valItem.Trim();
                                if (cleanV.Contains("<") || cleanV.Contains(">") || cleanV.Contains("---") || cleanV.Length < 2) continue;
                                string full = hive + "\\" + cleanP + "\\" + cleanV;
                                if (!existingRegKeys.Contains(full))
                                {
                                    unusedCandidates.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", relFile, i + 1, title, hive, cleanP, cleanV));
                                    existingRegKeys.Add(full);
                                }
                            }
                        }
                    }
                }
            }
        }

        File.WriteAllLines(@"C:\Users\User\Documents\win-tweaker\tools\all_unused_candidates.txt", unusedCandidates, Encoding.UTF8);
        Console.WriteLine("Written " + unusedCandidates.Count + " candidates to all_unused_candidates.txt");
    }
}
