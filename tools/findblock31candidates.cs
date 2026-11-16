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
        static void Main(string[] args)
        {
            string sourceRoot = @"C:\Users\User\Documents\win-tweaker\data\source\win-config";
            string tweaksJson = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksJson, Encoding.UTF8));

            HashSet<string> existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in list)
            {
                var regList = t.ContainsKey("registry") ? t["registry"] as System.Collections.ArrayList : null;
                if (regList != null)
                {
                    foreach (Dictionary<string, object> r in regList)
                    {
                        string path = r.ContainsKey("path") ? r["path"] as string : "";
                        string name = r.ContainsKey("name") ? r["name"] as string : "";
                        existingKeys.Add(path + "\\" + name);
                    }
                }
            }

            Console.WriteLine("Существующих ключей в базе: " + existingKeys.Count);

            string[] files = Directory.GetFiles(sourceRoot, "desc.md", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                string relPath = f.Substring(sourceRoot.Length).TrimStart('\\').Replace('\\', '/');
                string[] lines = File.ReadAllLines(f);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    // Ищем строчки реестра вида "Name" = dword:00000001 или "Name"=hex: или ветки
                    if (line.StartsWith("### ") || line.StartsWith("## "))
                    {
                        // Заголовок секции
                    }
                }
            }
        }
    }
}
