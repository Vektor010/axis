using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Scanner
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceRoot = @"C:\Users\User\Documents\win-tweaker\data\source\win-config";
            string tweaksJson = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJson, Encoding.UTF8));

            HashSet<string> existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var t in list)
            {
                if (t.ContainsKey("id")) existingIds.Add(t["id"] as string);
                var regList = t.ContainsKey("registry") ? t["registry"] as System.Collections.ArrayList : null;
                if (regList != null)
                {
                    foreach (Dictionary<string, object> r in regList)
                    {
                        string name = r.ContainsKey("name") ? r["name"] as string : "";
                        if (!string.IsNullOrEmpty(name)) existingNames.Add(name);
                    }
                }
            }

            Console.WriteLine("Существующих имен параметров в базе: " + existingNames.Count);

            string[] files = Directory.GetFiles(sourceRoot, "desc.md", SearchOption.AllDirectories);

            // Регексы для поиска определений ключей в desc.md:
            // "ValueName" = 1;
            // "ValueName"=dword:00000001
            // ValueName    Type: REG_DWORD, Length: 4, Data: 1
            var rx1 = new Regex(@"\""([A-Za-z0-9_\*]+)\""\s*=\s*([0-9a-zA-Z\?]+);", RegexOptions.Compiled);
            var rx2 = new Regex(@"([A-Za-z0-9_\*]+)\s+Type:\s*(REG_DWORD|REG_SZ),\s*Length:\s*\d+,\s*Data:\s*([0-9a-zA-Z\-_]+)", RegexOptions.Compiled);
            var rx3 = new Regex(@"HK[LM|CU]\\[^\n\r]*\\([A-Za-z0-9_\*]+)\s+Type:\s*(REG_DWORD|REG_SZ)", RegexOptions.Compiled);

            int candidateCount = 0;

            foreach (var f in files)
            {
                string rel = f.Substring(sourceRoot.Length).TrimStart('\\').Replace('\\', '/');
                string[] lines = File.ReadAllLines(f);

                string currentSection = "";
                string currentKeyPath = "";

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.StartsWith("# ") || line.StartsWith("## ") || line.StartsWith("### "))
                    {
                        currentSection = line;
                    }
                    if (line.StartsWith("\"HKLM\\") || line.StartsWith("\"HKCU\\"))
                    {
                        currentKeyPath = line.Trim('"', ';', ' ');
                    }

                    Match m1 = rx1.Match(line);
                    Match m2 = rx2.Match(line);
                    Match m3 = rx3.Match(line);

                    string varName = null;
                    string valStr = null;

                    if (m1.Success)
                    {
                        varName = m1.Groups[1].Value;
                        valStr = m1.Groups[2].Value;
                    }
                    else if (m2.Success)
                    {
                        varName = m2.Groups[1].Value;
                        valStr = m2.Groups[3].Value;
                    }
                    else if (m3.Success)
                    {
                        varName = m3.Groups[1].Value;
                        valStr = "data";
                    }

                    if (!string.IsNullOrEmpty(varName))
                    {
                        if (!existingNames.Contains(varName) && varName.Length > 2 && !varName.StartsWith("http") && valStr != "?")
                        {
                            candidateCount++;
                            Console.WriteLine(string.Format("[{0}:{1}] Var: '{2}' Val: {3} | Sec: {4} | Key: {5}",
                                rel, i + 1, varName, valStr, currentSection, currentKeyPath));
                        }
                    }
                }
            }

            Console.WriteLine("\nВсего найдено уникальных кандидатов: " + candidateCount);
        }
    }
}
