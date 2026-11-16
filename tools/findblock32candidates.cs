using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace CandidateFinder
{
    class Program
    {
        class Candidate
        {
            public string SourceFile;
            public int LineFrom;
            public int LineTo;
            public string Section;
            public string SubSection;
            public string Root;
            public string Path;
            public string Name;
            public string Type;
            public object ApplyVal;
            public object RevertVal;
            public string RawSnippet;
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

            foreach (var t in list)
            {
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

            Console.WriteLine("Существующих ключей: " + existingKeys.Count);

            string[] files = Directory.GetFiles(sourceRoot, "desc.md", SearchOption.AllDirectories);

            List<Candidate> found = new List<Candidate>();

            foreach (var f in files)
            {
                string rel = f.Substring(sourceRoot.Length).TrimStart('\\').Replace('\\', '/');
                string[] lines = File.ReadAllLines(f);

                // Ищем секции с "#### " или "### "
                for (int i = 0; i < lines.Length; i++)
                {
                    string l = lines[i].Trim();
                    // Ищем маркеры реестра Type: REG_DWORD или "Name" = 1
                    if (l.Contains("Type: REG_DWORD") || l.Contains("Type: REG_SZ"))
                    {
                        // Поднимаемся вверх, чтобы найти путь реестра и заголовок
                        int searchStart = Math.Max(0, i - 25);
                        int searchEnd = Math.Min(lines.Length - 1, i + 25);

                        string keyPath = null;
                        string keyRoot = "HKLM";

                        for (int k = i; k >= searchStart; k--)
                        {
                            string kl = lines[k].Trim();
                            if (kl.StartsWith("\"HKLM\\") || kl.StartsWith("HKLM\\"))
                            {
                                keyRoot = "HKLM";
                                keyPath = kl.Trim('"', ';', ' ').Substring(5);
                                break;
                            }
                            else if (kl.StartsWith("\"HKCU\\") || kl.StartsWith("HKCU\\"))
                            {
                                keyRoot = "HKCU";
                                keyPath = kl.Trim('"', ';', ' ').Substring(5);
                                break;
                            }
                        }

                        // Имя параметра
                        string paramName = null;
                        var mParam = Regex.Match(l, @"^([A-Za-z0-9_\*]+)\s+Type:\s*(REG_DWORD|REG_SZ)");
                        if (!mParam.Success)
                        {
                            mParam = Regex.Match(l, @"HK[LM|CU]\\[^\n\r]*\\([A-Za-z0-9_\*]+)\s+Type:\s*(REG_DWORD|REG_SZ)");
                        }

                        if (mParam.Success)
                        {
                            paramName = mParam.Groups[1].Value;
                        }

                        if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(keyPath))
                        {
                            string fullKey = keyPath + "\\" + paramName;
                            if (!existingKeys.Contains(fullKey) && !existingNames.Contains(paramName))
                            {
                                // Ищем границы секции
                                int secStart = i;
                                while (secStart > 0 && !lines[secStart].StartsWith("### ") && !lines[secStart].StartsWith("#### "))
                                {
                                    secStart--;
                                }
                                int secEnd = i;
                                while (secEnd < lines.Length - 1 && !lines[secEnd + 1].StartsWith("### ") && !lines[secEnd + 1].StartsWith("## "))
                                {
                                    secEnd++;
                                    if (lines[secEnd].StartsWith("### ") || lines[secEnd].StartsWith("## "))
                                    {
                                        secEnd--;
                                        break;
                                    }
                                }

                                var c = new Candidate();
                                c.SourceFile = rel;
                                c.LineFrom = secStart + 1; // 1-based
                                c.LineTo = secEnd + 1;
                                c.Root = keyRoot;
                                c.Path = keyPath;
                                c.Name = paramName;
                                c.Type = mParam.Groups[2].Value;
                                c.Section = lines[secStart];
                                found.Add(c);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Найдено кандидатов Type: REG_*: " + found.Count);

            // Также ищем синтаксис "Name" = 1; в desc.md
            foreach (var f in files)
            {
                string rel = f.Substring(sourceRoot.Length).TrimStart('\\').Replace('\\', '/');
                string[] lines = File.ReadAllLines(f);

                for (int i = 0; i < lines.Length; i++)
                {
                    string l = lines[i].Trim();
                    var m = Regex.Match(l, @"^\""([A-Za-z0-9_\*]+)\""\s*=\s*([0-9a-zA-Z\-_]+);");
                    if (m.Success)
                    {
                        string paramName = m.Groups[1].Value;
                        string valStr = m.Groups[2].Value;

                        // Ищем ключ выше
                        string keyPath = null;
                        string keyRoot = "HKLM";
                        for (int k = i; k >= Math.Max(0, i - 20); k--)
                        {
                            string kl = lines[k].Trim();
                            if (kl.StartsWith("\"HKLM\\") || kl.StartsWith("HKLM\\"))
                            {
                                keyRoot = "HKLM";
                                keyPath = kl.Trim('"', ';', ' ').Substring(5);
                                break;
                            }
                            else if (kl.StartsWith("\"HKCU\\") || kl.StartsWith("HKCU\\"))
                            {
                                keyRoot = "HKCU";
                                keyPath = kl.Trim('"', ';', ' ').Substring(5);
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(keyPath))
                        {
                            string fullKey = keyPath + "\\" + paramName;
                            if (!existingKeys.Contains(fullKey) && !existingNames.Contains(paramName))
                            {
                                int secStart = i;
                                while (secStart > 0 && !lines[secStart].StartsWith("### ") && !lines[secStart].StartsWith("#### "))
                                {
                                    secStart--;
                                }
                                int secEnd = i;
                                while (secEnd < lines.Length - 1 && !lines[secEnd + 1].StartsWith("### ") && !lines[secEnd + 1].StartsWith("## "))
                                {
                                    secEnd++;
                                    if (lines[secEnd].StartsWith("### ") || lines[secEnd].StartsWith("## "))
                                    {
                                        secEnd--;
                                        break;
                                    }
                                }

                                var c = new Candidate();
                                c.SourceFile = rel;
                                c.LineFrom = secStart + 1;
                                c.LineTo = secEnd + 1;
                                c.Root = keyRoot;
                                c.Path = keyPath;
                                c.Name = paramName;
                                c.Type = "REG_DWORD";
                                c.Section = lines[secStart];
                                found.Add(c);
                            }
                        }
                    }
                }
            }

            // Убираем дубли
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Candidate> unique = new List<Candidate>();
            foreach (var c in found)
            {
                string key = c.SourceFile + ":" + c.Path + "\\" + c.Name;
                if (!seen.Contains(key))
                {
                    seen.Add(key);
                    unique.Add(c);
                }
            }

            Console.WriteLine("Всего уникальных неиспользованных кандидатов: " + unique.Count);

            // Выводим кандидатов сгруппированных по файлам
            foreach (var c in unique)
            {
                Console.WriteLine(string.Format("{0} | lines {1}..{2} | {3}\\{4} | {5} | {6}",
                    c.SourceFile, c.LineFrom, c.LineTo, c.Root, c.Path + "\\" + c.Name, c.Type, c.Section));
            }
        }
    }
}
