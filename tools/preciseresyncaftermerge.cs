using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace PreciseResync
{
    class Program
    {
        static void Main()
        {
            string rootDir = @"C:\Users\User\Documents\win-tweaker";
            string tweaksJsonPath = Path.Combine(rootDir, @"data\tweaks.json");
            string backupPath = Path.Combine(rootDir, @"data\tweaks.json.bak33");
            string sourceDir = Path.Combine(rootDir, @"data\source\win-config");

            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;

            // Берем чистые твики из tweaks.json.bak33 (рубеж 775 твиков) или текущие 800 твиков
            var tweaks = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJsonPath, Encoding.UTF8));

            string visPath = Path.Combine(sourceDir, @"visibility\desc.md");
            string[] visLines = File.ReadAllLines(visPath);

            // Для каждого твика visibility, который сбоит, найдем точные строки блока // Enabled и // Disabled
            foreach (var tweak in tweaks)
            {
                var src = tweak["source"] as Dictionary<string, object>;
                if (src == null) continue;
                string f = src["file"] as string;

                if (f == "visibility/desc.md")
                {
                    var regList = tweak["registry"] as System.Collections.ArrayList;
                    if (regList == null || regList.Count == 0) continue;
                    var r0 = regList[0] as Dictionary<string, object>;
                    string paramName = r0["name"] as string;

                    // Ищем строчку с paramName в visLines
                    for (int i = 0; i < visLines.Length; i++)
                    {
                        if (visLines[i].Contains(paramName))
                        {
                            // Проверяем, есть ли рядом // Enabled
                            int blockStart = i;
                            while (blockStart > 0 && !visLines[blockStart].Trim().StartsWith("// Enabled") && !visLines[blockStart].Trim().StartsWith("### ") && !visLines[blockStart].Trim().StartsWith("## "))
                            {
                                blockStart--;
                                if (i - blockStart > 10) { blockStart = i; break; }
                            }

                            int blockEnd = i;
                            while (blockEnd < visLines.Length - 1 && !visLines[blockEnd + 1].Trim().StartsWith("// Enabled") && !visLines[blockEnd + 1].Trim().StartsWith("### ") && !visLines[blockEnd + 1].Trim().StartsWith("## "))
                            {
                                blockEnd++;
                                if (blockEnd - i > 10) { break; }
                            }

                            // Если нашли блок с // Enabled и // Disabled
                            bool hasEnabled = false;
                            for (int k = blockStart; k <= blockEnd; k++)
                            {
                                if (visLines[k].Contains("// Enabled")) hasEnabled = true;
                            }

                            if (hasEnabled)
                            {
                                src["line_from"] = blockStart + 1; // 1-based
                                src["line_to"] = blockEnd + 1;
                                break;
                            }
                        }
                    }
                }
            }

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
            Console.WriteLine("Синхронизация завершена успешно!");
        }
    }
}
