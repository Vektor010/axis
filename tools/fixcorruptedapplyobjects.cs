using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace FixTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(jsonPath, Encoding.UTF8);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var list = serializer.Deserialize<List<Dictionary<string, object>>>(raw);

            Console.WriteLine("Всего твиков для нормализации: " + list.Count);

            int fixedApplyCount = 0;
            int fixedRevertCount = 0;

            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                string id = t.ContainsKey("id") ? t["id"] as string : "";

                var regList = t.ContainsKey("registry") ? t["registry"] as ArrayList : null;
                if (regList == null) continue;

                foreach (Dictionary<string, object> r in regList)
                {
                    string name = r.ContainsKey("name") ? r["name"] as string : "";
                    string type = r.ContainsKey("type") ? r["type"] as string : "";
                    object apply = r.ContainsKey("apply") ? r["apply"] : null;
                    var revert = r.ContainsKey("revert") ? r["revert"] as Dictionary<string, object> : null;

                    // 1. Нормализация apply
                    if (apply is Dictionary<string, object>)
                    {
                        var appDict = (Dictionary<string, object>)apply;
                        object innerVal = appDict.ContainsKey("value") ? appDict["value"] : null;

                        if (type == "REG_DWORD")
                        {
                            r["apply"] = innerVal != null ? Convert.ToInt32(innerVal) : 0;
                        }
                        else if (type == "REG_SZ")
                        {
                            r["apply"] = innerVal != null ? innerVal.ToString() : "";
                        }
                        else
                        {
                            r["apply"] = innerVal;
                        }
                        fixedApplyCount++;
                    }

                    // 2. Исправление специфических твиков с коллизией apply == revert
                    if (id == "power-disable-deep-io-coalescing")
                    {
                        r["apply"] = 0;
                        if (revert != null)
                        {
                            revert["mode"] = "delete";
                            revert["value"] = null;
                        }
                        fixedRevertCount++;
                    }
                    else if (id == "security-uac-admin-approval")
                    {
                        // Режим подтверждения встроенного администратора: 1 = включен (безопасный), revert = 0 (дефолт Windows без запросов)
                        r["apply"] = 1;
                        if (revert != null)
                        {
                            revert["mode"] = "set";
                            revert["value"] = 0;
                        }
                        fixedRevertCount++;
                    }
                    else if (id == "cleanup-mui-cache")
                    {
                        t["type"] = "action";
                        r["apply"] = "";
                        if (revert != null)
                        {
                            revert["mode"] = "none";
                            revert["value"] = null;
                        }
                        fixedRevertCount++;
                    }
                    else if (id == "cleanup-background-history")
                    {
                        t["type"] = "action";
                        r["apply"] = "";
                        if (revert != null)
                        {
                            revert["mode"] = "none";
                            revert["value"] = null;
                        }
                        fixedRevertCount++;
                    }
                }
            }

            Console.WriteLine(string.Format("Исправлено объектов apply: {0}", fixedApplyCount));
            Console.WriteLine(string.Format("Скорректировано специфических revert: {0}", fixedRevertCount));

            // Создаем бэкап tweaks.json перед перезаписью
            string backupPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json.bak";
            File.Copy(jsonPath, backupPath, true);
            Console.WriteLine("Бэкап сохранен: " + backupPath);

            // Сохраняем обновленный tweaks.json
            string formatted = FormatJson(list);
            File.WriteAllText(jsonPath, formatted, new UTF8Encoding(false));
            Console.WriteLine("Файл tweaks.json успешно перезаписан!");
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
