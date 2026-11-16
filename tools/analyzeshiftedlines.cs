using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Analyzer
{
    class Program
    {
        static void Main()
        {
            string rootDir = @"C:\Users\User\Documents\win-tweaker";
            string tweaksJsonPath = Path.Combine(rootDir, @"data\tweaks.json");
            string sourceBaseDir = Path.Combine(rootDir, @"data\source\win-config");

            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var tweaks = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJsonPath, Encoding.UTF8));

            int mismatchCount = 0;
            int autoFixedCount = 0;

            foreach (var tweak in tweaks)
            {
                var src = tweak["source"] as Dictionary<string, object>;
                if (src == null) continue;

                string fileRel = src["file"] as string;
                int lineFrom = (int)src["line_from"];
                int lineTo = (int)src["line_to"];

                string fullPath = Path.Combine(sourceBaseDir, fileRel.Replace('/', '\\'));
                if (!File.Exists(fullPath)) continue;

                var lines = File.ReadAllLines(fullPath);

                var regList = tweak["registry"] as System.Collections.ArrayList;
                if (regList == null || regList.Count == 0) continue;

                var firstReg = regList[0] as Dictionary<string, object>;
                string paramName = firstReg.ContainsKey("name") ? firstReg["name"] as string : "";
                string path = firstReg.ContainsKey("path") ? firstReg["path"] as string : "";

                // Проверяем, есть ли имя в текущем диапазоне
                bool foundInCurrent = false;
                for (int l = Math.Max(0, lineFrom - 1); l < Math.Min(lines.Length, lineTo); l++)
                {
                    if (lines[l].Contains(paramName))
                    {
                        foundInCurrent = true;
                        break;
                    }
                }

                if (!foundInCurrent)
                {
                    mismatchCount++;
                    // Ищем параметр в файле в окне +- 200 строк
                    int bestLine = -1;
                    int minDistance = int.MaxValue;

                    for (int l = 0; l < lines.Length; l++)
                    {
                        if (lines[l].Contains(paramName))
                        {
                            int dist = Math.Abs(l - (lineFrom - 1));
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestLine = l + 1; // 1-based
                            }
                        }
                    }

                    if (bestLine != -1)
                    {
                        int span = lineTo - lineFrom;
                        int shift = bestLine - lineFrom;
                        Console.WriteLine(string.Format("[SHIFT] {0} ({1}): {2}..{3} -> shift {4} (найден на {5})",
                            tweak["id"], paramName, lineFrom, lineTo, shift, bestLine));

                        // Автоматически вычисляем правильный диапазон
                        // Находим начало и конец блока
                        int newFrom = bestLine;
                        while (newFrom > 1 && !lines[newFrom - 1].StartsWith("#") && !lines[newFrom - 1].StartsWith("//"))
                        {
                            newFrom--;
                        }
                        int newTo = bestLine;
                        while (newTo < lines.Length && !lines[newTo].StartsWith("#") && !lines[newTo].StartsWith("//"))
                        {
                            newTo++;
                        }

                        // Сохраняем скорректированный диапазон
                        src["line_from"] = Math.Max(1, lineFrom + shift);
                        src["line_to"] = Math.Max((int)src["line_from"], lineTo + shift);
                        autoFixedCount++;
                    }
                    else
                    {
                        Console.WriteLine(string.Format("[NOT_FOUND] {0} ({1}) не найден в {2}", tweak["id"], paramName, fileRel));
                    }
                }
            }

            Console.WriteLine(string.Format("\nВсего несоответствий строк: {0}, Автоматически найдено смещений: {1}", mismatchCount, autoFixedCount));

            if (autoFixedCount > 0)
            {
                // Записываем обновленный tweaks.json
                string jsonOutput = ser.Serialize(tweaks);

                StringBuilder sb = new StringBuilder();
                int indent = 0;
                bool inQuote = false;
                for (int i = 0; i < jsonOutput.Length; i++)
                {
                    char ch = jsonOutput[i];
                    if (ch == '"' && (i == 0 || jsonOutput[i - 1] != '\\'))
                    {
                        inQuote = !inQuote;
                        sb.Append(ch);
                    }
                    else if (!inQuote)
                    {
                        if (ch == '{' || ch == '[')
                        {
                            sb.Append(ch);
                            sb.Append("\r\n");
                            indent++;
                            sb.Append(new string(' ', indent * 2));
                        }
                        else if (ch == '}' || ch == ']')
                        {
                            sb.Append("\r\n");
                            indent--;
                            sb.Append(new string(' ', indent * 2));
                            sb.Append(ch);
                        }
                        else if (ch == ',')
                        {
                            sb.Append(ch);
                            sb.Append("\r\n");
                            sb.Append(new string(' ', indent * 2));
                        }
                        else if (ch == ':')
                        {
                            sb.Append(": ");
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                File.WriteAllText(tweaksJsonPath, sb.ToString(), new UTF8Encoding(false));
                Console.WriteLine("tweaks.json успешно обновлен новыми номерами строк!");
            }
        }
    }
}
