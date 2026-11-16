using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace CandidateFinder34
{
    class Program
    {
        class Candidate
        {
            public string SourceFile;
            public int Line;
            public string Root;
            public string Path;
            public string Name;
            public string Type;
            public string DefaultVal;
            public string Context;
        }

        static void Main(string[] args)
        {
            string sourceRoot = @"C:\Users\User\Documents\win-tweaker\data\source\win-config";
            string tweaksJson = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJson, Encoding.UTF8));

            HashSet<string> existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                        string p = r.ContainsKey("path") ? r["path"] as string : "";
                        string n = r.ContainsKey("name") ? r["name"] as string : "";
                        if (!string.IsNullOrEmpty(n))
                        {
                            existingNames.Add(n);
                            existingKeys.Add(p + "\\" + n);
                        }
                    }
                }
            }

            Console.WriteLine("Существующих ключей: " + existingKeys.Count + " (твиков: " + list.Count + ")");

            string[] files = Directory.GetFiles(sourceRoot, "desc.md", SearchOption.AllDirectories);
            List<Candidate> candidates = new List<Candidate>();

            var rx1 = new Regex(@"\""([A-Za-z0-9_\*]+)\""\s*=\s*([0-9a-zA-Z\-_]+);", RegexOptions.Compiled);
            var rx2 = new Regex(@"([A-Za-z0-9_\*]+)\s+Type:\s*(REG_DWORD|REG_SZ)", RegexOptions.Compiled);

            foreach (var f in files)
            {
                string rel = f.Substring(sourceRoot.Length).TrimStart('\\').Replace('\\', '/');
                string[] lines = File.ReadAllLines(f);

                string currentKeyPath = null;
                string currentHive = "HKLM";

                for (int i = 0; i < lines.Length; i++)
                {
                    string l = lines[i].Trim();
                    if (l.StartsWith("\"HKLM\\") || l.StartsWith("HKLM\\"))
                    {
                        currentHive = "HKLM";
                        currentKeyPath = l.Trim('"', ';', ' ').Substring(5);
                    }
                    else if (l.StartsWith("\"HKCU\\") || l.StartsWith("HKCU\\"))
                    {
                        currentHive = "HKCU";
                        currentKeyPath = l.Trim('"', ';', ' ').Substring(5);
                    }

                    Match m1 = rx1.Match(l);
                    Match m2 = rx2.Match(l);

                    string varName = null;
                    string varType = "REG_DWORD";
                    string varVal = null;

                    if (m1.Success)
                    {
                        varName = m1.Groups[1].Value;
                        varVal = m1.Groups[2].Value;
                    }
                    else if (m2.Success)
                    {
                        varName = m2.Groups[1].Value;
                        varType = m2.Groups[2].Value;
                    }

                    if (!string.IsNullOrEmpty(varName) && !string.IsNullOrEmpty(currentKeyPath))
                    {
                        string fullKey = currentKeyPath + "\\" + varName;
                        if (!existingKeys.Contains(fullKey) && !existingNames.Contains(varName) && varName.Length > 2 && !varName.StartsWith("http"))
                        {
                            var c = new Candidate();
                            c.SourceFile = rel;
                            c.Line = i + 1;
                            c.Root = currentHive;
                            c.Path = currentKeyPath;
                            c.Name = varName;
                            c.Type = varType;
                            c.DefaultVal = varVal;
                            c.Context = l;
                            candidates.Add(c);
                        }
                    }
                }
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Candidate> unique = new List<Candidate>();
            foreach (var c in candidates)
            {
                string k = c.SourceFile + ":" + c.Path + "\\" + c.Name;
                if (!seen.Contains(k))
                {
                    seen.Add(k);
                    unique.Add(c);
                }
            }

            Console.WriteLine("Найдено уникальных кандидатов: " + unique.Count);
            foreach (var c in unique)
            {
                Console.WriteLine(string.Format("{0}:{1} | {2}\\{3} | {4} | {5} | {6}",
                    c.SourceFile, c.Line, c.Root, c.Path + "\\" + c.Name, c.Type, c.DefaultVal, c.Context));
            }
        }
    }
}
