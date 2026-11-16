using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace ComprehensiveAudit
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string sourceRoot = @"C:\Users\User\Documents\win-tweaker\data\source\win-config";
            string raw = File.ReadAllText(jsonPath, Encoding.UTF8);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(raw);

            Console.WriteLine("=== ПОЛНЫЙ АУДИТ БАЗЫ ДАННЫХ И ИСТОЧНИКОВ ===");
            Console.WriteLine("Всего твиков: " + list.Count);

            int applyObjectCount = 0;
            int applyEqualRevertCount = 0;
            int missingSourceFileCount = 0;
            int outOfBoundsSourceLineCount = 0;
            int invalidCategoryCount = 0;
            int invalidTypeCount = 0;
            int invalidRiskCount = 0;
            int invalidPathCount = 0;

            var sourceFileLines = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                string id = t.ContainsKey("id") ? t["id"] as string : "unknown";

                // Проверка источника
                var src = t.ContainsKey("source") ? t["source"] as Dictionary<string, object> : null;
                if (src != null)
                {
                    string file = src.ContainsKey("file") ? src["file"] as string : "";
                    int from = src.ContainsKey("line_from") ? Convert.ToInt32(src["line_from"]) : 0;
                    int to = src.ContainsKey("line_to") ? Convert.ToInt32(src["line_to"]) : 0;

                    string fullSrcPath = Path.Combine(sourceRoot, file);
                    if (!File.Exists(fullSrcPath))
                    {
                        missingSourceFileCount++;
                        Console.WriteLine(string.Format("[SRC-NOT-FOUND] {0}: файл '{1}' не найден", id, file));
                    }
                    else
                    {
                        if (!sourceFileLines.ContainsKey(file))
                        {
                            sourceFileLines[file] = File.ReadAllLines(fullSrcPath).Length;
                        }

                        int totalLines = sourceFileLines[file];
                        if (from > totalLines || to > totalLines || from <= 0 || to < from)
                        {
                            outOfBoundsSourceLineCount++;
                            Console.WriteLine(string.Format("[SRC-LINE-OOB] {0}: {1} lines {2}..{3} (total: {4})", id, file, from, to, totalLines));
                        }
                    }
                }

                // Проверка действий реестра
                var regList = t.ContainsKey("registry") ? t["registry"] as ArrayList : null;
                if (regList != null)
                {
                    foreach (Dictionary<string, object> r in regList)
                    {
                        string name = r.ContainsKey("name") ? r["name"] as string : "";
                        string path = r.ContainsKey("path") ? r["path"] as string : "";
                        object apply = r.ContainsKey("apply") ? r["apply"] : null;
                        var revert = r.ContainsKey("revert") ? r["revert"] as Dictionary<string, object> : null;

                        if (string.IsNullOrEmpty(path) || path.StartsWith("\\") || path.EndsWith("\\"))
                        {
                            invalidPathCount++;
                        }

                        if (apply is Dictionary<string, object> || apply is ArrayList)
                        {
                            applyObjectCount++;
                        }
                        else
                        {
                            if (revert != null)
                            {
                                string revMode = revert.ContainsKey("mode") ? revert["mode"] as string : "";
                                object revVal = revert.ContainsKey("value") ? revert["value"] : null;

                                if (revMode == "set" && revVal != null && apply != null && apply.ToString() == revVal.ToString())
                                {
                                    applyEqualRevertCount++;
                                    Console.WriteLine(string.Format("[APPLY==REVERT] {0} ({1}): val={2}", id, name, apply));
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("\n--- ИТОГИ ПРОВЕРКИ ---");
            Console.WriteLine("apply являются объектами: " + applyObjectCount);
            Console.WriteLine("apply равен revert (примитивы): " + applyEqualRevertCount);
            Console.WriteLine("Не найдены файлы источников: " + missingSourceFileCount);
            Console.WriteLine("Строки источников вне диапазона: " + outOfBoundsSourceLineCount);
            Console.WriteLine("Некорректные пути: " + invalidPathCount);
        }
    }
}
