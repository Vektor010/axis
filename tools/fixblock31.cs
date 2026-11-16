using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace FixBlock31
{
    class Program
    {
        static void Main(string[] args)
        {
            string tweaksPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(tweaksPath, Encoding.UTF8);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(raw);

            var smbIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "network-smb-disable-bandwidth-throttling",
                "network-smb-disable-large-mtu",
                "network-smb-disable-multichannel",
                "network-smb-disable-leasing"
            };

            int updated = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                string id = t.ContainsKey("id") ? t["id"] as string : "";
                if (smbIds.Contains(id))
                {
                    Console.WriteLine("Setting needs_review = true for: " + id + " (was " + t["needs_review"] + ")");
                    t["needs_review"] = true;
                    updated++;
                }
            }

            Console.WriteLine("Всего обновлено: " + updated);

            // Сохраняем в UTF-8 БЕЗ BOM через правильный сериализатор
            string formatted = FormatJson(list);
            File.WriteAllText(tweaksPath, formatted, new UTF8Encoding(false));
            Console.WriteLine("Файл tweaks.json успешно перезаписан в UTF-8 без BOM!");
        }

        static string FormatJson(object obj)
        {
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            string raw = serializer.Serialize(obj);

            var sb = new StringBuilder();
            int indent = 0;
            bool inQuote = false;

            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c == '\"' && (i == 0 || raw[i - 1] != '\\'))
                {
                    inQuote = !inQuote;
                    sb.Append(c);
                }
                else if (!inQuote)
                {
                    if (c == '{' || c == '[')
                    {
                        sb.Append(c);
                        sb.Append("\r\n");
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (c == '}' || c == ']')
                    {
                        sb.Append("\r\n");
                        indent--;
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(c);
                    }
                    else if (c == ',')
                    {
                        sb.Append(c);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (c == ':')
                    {
                        sb.Append(": ");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
