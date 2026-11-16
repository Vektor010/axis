using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace FixTweak
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string json = File.ReadAllText(path, Encoding.UTF8);
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(json);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i]["id"].ToString() == "visibility-taskbar-end-task")
                {
                    var item = new Dictionary<string, object>();
                    item["id"] = "visibility-classic-control-panel";
                    item["name_ru"] = "Всегда открывать классическую панель управления (ForceClassicControlPanel)";
                    item["description_ru"] = "Принудительно открывает классический вид всех элементов панели управления (крупные значки) вместо категорий.";
                    item["group"] = "Проводник и рабочий стол";
                    item["category"] = "visibility";
                    item["type"] = "toggle";
                    item["risk"] = "low";
                    item["presets"] = new List<string> { "recommended" };
                    item["min_build"] = null;
                    item["needs_review"] = false;

                    var src = new Dictionary<string, object>();
                    src["file"] = "visibility/desc.md";
                    src["line_from"] = 1841;
                    src["line_to"] = 1844;
                    item["source"] = src;

                    var regList = new List<Dictionary<string, object>>();
                    var reg = new Dictionary<string, object>();
                    reg["hive"] = "HKCU";
                    reg["path"] = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";
                    reg["name"] = "ForceClassicControlPanel";
                    reg["type"] = "REG_DWORD";

                    var appObj = new Dictionary<string, object>();
                    appObj["mode"] = "set";
                    appObj["value"] = 1;
                    reg["apply"] = appObj;

                    var revObj = new Dictionary<string, object>();
                    revObj["mode"] = "set";
                    revObj["value"] = 0;
                    reg["revert"] = revObj;

                    regList.Add(reg);
                    item["registry"] = regList;

                    list[i] = item;
                    Console.WriteLine("Replaced visibility-taskbar-end-task at index " + i);
                    break;
                }
            }

            string updated = FormatJson(serializer.Serialize(list));
            File.WriteAllText(path, updated, new UTF8Encoding(false));
            Console.WriteLine("Done saving tweaks.json. Total count: " + list.Count);
        }

        private static string FormatJson(string json)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', ++indent * 2));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', --indent * 2));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 2));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
