using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace FixNtfs
{
    class Program
    {
        static void Main()
        {
            string rootDir = @"C:\Users\User\Documents\win-tweaker";
            string tweaksJsonPath = Path.Combine(rootDir, @"data\tweaks.json");
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var tweaks = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJsonPath, Encoding.UTF8));

            foreach (var t in tweaks)
            {
                if ((t["id"] as string) == "system-ntfs-disable-last-access")
                {
                    var src = t["source"] as Dictionary<string, object>;
                    Console.WriteLine("Source: " + src["file"] + " lines " + src["line_from"] + ".." + src["line_to"]);
                    // В system/desc.md строки 8520..8535
                    src["file"] = "system/desc.md";
                    src["line_from"] = 8520;
                    src["line_to"] = 8535;
                }
            }

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
            Console.WriteLine("system-ntfs-disable-last-access успешно исправлен!");
        }
    }
}
