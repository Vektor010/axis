using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace AuditTool
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

            Console.WriteLine("Всего твиков на аудите: " + list.Count);

            int objectApplyCount = 0;
            int typeMismatchCount = 0;
            int emptyApplyCount = 0;
            int revertIssuesCount = 0;

            List<string> objectApplyIds = new List<string>();
            List<string> typeMismatchIds = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                string id = t.ContainsKey("id") ? t["id"] as string : "unknown";
                var regList = t.ContainsKey("registry") ? t["registry"] as System.Collections.ArrayList : null;

                if (regList == null) continue;

                foreach (Dictionary<string, object> r in regList)
                {
                    string name = r.ContainsKey("name") ? r["name"] as string : "";
                    string type = r.ContainsKey("type") ? r["type"] as string : "";
                    object apply = r.ContainsKey("apply") ? r["apply"] : null;

                    // 1. Проверка: apply - объект?
                    if (apply is Dictionary<string, object>)
                    {
                        objectApplyCount++;
                        objectApplyIds.Add(id + " [" + name + "]");
                    }
                    else
                    {
                        // 2. Проверка типов
                        if (type == "REG_DWORD")
                        {
                            if (apply is string)
                            {
                                typeMismatchCount++;
                                typeMismatchIds.Add(id + " [" + name + "]: REG_DWORD with string '" + apply + "'");
                            }
                        }
                        if (type == "REG_SZ")
                        {
                            if (!(apply is string))
                            {
                                typeMismatchCount++;
                                typeMismatchIds.Add(id + " [" + name + "]: REG_SZ with non-string " + (apply ?? "null"));
                            }
                        }
                    }

                    // 3. Проверка revert
                    var rev = r.ContainsKey("revert") ? r["revert"] as Dictionary<string, object> : null;
                    if (rev != null)
                    {
                        string mode = rev.ContainsKey("mode") ? rev["mode"] as string : "";
                        object val = rev.ContainsKey("value") ? rev["value"] : null;

                        if (mode == "set")
                        {
                            if (val == null)
                            {
                                revertIssuesCount++;
                            }
                            else if (apply != null && apply.Equals(val))
                            {
                                revertIssuesCount++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("================ АУДИТ БАЗЫ ДАННЫХ ================");
            Console.WriteLine("Твиков с объектом в apply: " + objectApplyCount);
            Console.WriteLine("Твиков с несовпадением типов: " + typeMismatchCount);
            Console.WriteLine("Твиков с проблемами revert: " + revertIssuesCount);

            if (objectApplyIds.Count > 0)
            {
                Console.WriteLine("\nПервые 10 с объектом в apply:");
                for (int i = 0; i < Math.Min(10, objectApplyIds.Count); i++)
                {
                    Console.WriteLine(" - " + objectApplyIds[i]);
                }
            }

            if (typeMismatchIds.Count > 0)
            {
                Console.WriteLine("\nПервые 10 с несовпадением типов:");
                for (int i = 0; i < Math.Min(10, typeMismatchIds.Count); i++)
                {
                    Console.WriteLine(" - " + typeMismatchIds[i]);
                }
            }
        }
    }
}
