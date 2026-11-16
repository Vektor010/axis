using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace DeepAudit
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

            Console.WriteLine("=== ГЛУБОКИЙ АУДИТ ТВИКОВ (725) ===");
            Console.WriteLine("Всего записей: " + list.Count);

            HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> regTargets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int errCount = 0;
            int warnCount = 0;

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            string[] validCategories = new string[] {
                "system", "visibility", "peripheral", "power", "privacy",
                "network", "security", "nvidia", "misc", "policies",
                "affinities", "steam", "cleanup"
            };

            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                string id = t.ContainsKey("id") ? t["id"] as string : null;

                if (string.IsNullOrEmpty(id))
                {
                    errors.Add(string.Format("Индекс #{0}: отсутствует или пустой 'id'", i));
                    errCount++;
                    continue;
                }

                if (!seenIds.Add(id))
                {
                    errors.Add(string.Format("Дубликат id: '{0}' (индекс #{1})", id, i));
                    errCount++;
                }

                string nameRu = t.ContainsKey("name_ru") ? t["name_ru"] as string : null;
                if (string.IsNullOrEmpty(nameRu))
                {
                    errors.Add(string.Format("{0}: пустое поле 'name_ru'", id));
                    errCount++;
                }

                string descRu = t.ContainsKey("description_ru") ? t["description_ru"] as string : null;
                if (descRu == null)
                {
                    warnings.Add(string.Format("{0}: отсутствует поле 'description_ru'", id));
                    warnCount++;
                }

                string category = t.ContainsKey("category") ? t["category"] as string : null;
                if (string.IsNullOrEmpty(category) || Array.IndexOf(validCategories, category) < 0)
                {
                    errors.Add(string.Format("{0}: неверная категория '{1}'", id, category));
                    errCount++;
                }

                var regList = t.ContainsKey("registry") ? t["registry"] as ArrayList : null;
                if (regList == null || regList.Count == 0)
                {
                    errors.Add(string.Format("{0}: пустой или отсутствующий массив 'registry'", id));
                    errCount++;
                    continue;
                }

                for (int r = 0; r < regList.Count; r++)
                {
                    var regItem = regList[r] as Dictionary<string, object>;
                    if (regItem == null)
                    {
                        errors.Add(string.Format("{0}[{1}]: запись реестра не является объектом", id, r));
                        errCount++;
                        continue;
                    }

                    string hive = regItem.ContainsKey("hive") ? regItem["hive"] as string : null;
                    string path = regItem.ContainsKey("path") ? regItem["path"] as string : null;
                    string name = regItem.ContainsKey("name") ? regItem["name"] as string : null;
                    string type = regItem.ContainsKey("type") ? regItem["type"] as string : null;
                    object apply = regItem.ContainsKey("apply") ? regItem["apply"] : null;
                    var revert = regItem.ContainsKey("revert") ? regItem["revert"] as Dictionary<string, object> : null;

                    if (hive != "HKLM" && hive != "HKCU")
                    {
                        errors.Add(string.Format("{0}[{1}]: некорректный hive '{2}'", id, r, hive));
                        errCount++;
                    }

                    if (string.IsNullOrEmpty(path) || path.StartsWith("\\") || path.EndsWith("\\"))
                    {
                        errors.Add(string.Format("{0}[{1}]: некорректный path '{2}'", id, r, path));
                        errCount++;
                    }

                    if (name == null)
                    {
                        errors.Add(string.Format("{0}[{1}]: name == null", id, r));
                        errCount++;
                    }

                    // Проверка наложения реестра
                    string targetKey = string.Format("{0}\\{1}::{2}", hive, path, name);
                    if (regTargets.ContainsKey(targetKey))
                    {
                        warnings.Add(string.Format("Коллизия реестра: '{0}' модифицируется в '{1}' и ранее в '{2}'", targetKey, id, regTargets[targetKey]));
                        warnCount++;
                    }
                    else
                    {
                        regTargets[targetKey] = id;
                    }

                    // Проверка apply
                    if (apply is Dictionary<string, object>)
                    {
                        errors.Add(string.Format("{0}[{1}]: apply является объектом Dictionary ({2})", id, r, name));
                        errCount++;
                    }
                    else if (apply is ArrayList)
                    {
                        errors.Add(string.Format("{0}[{1}]: apply является массивом ArrayList ({2})", id, r, name));
                        errCount++;
                    }
                    else if (apply == null)
                    {
                        errors.Add(string.Format("{0}[{1}]: apply == null ({2})", id, r, name));
                        errCount++;
                    }
                    else
                    {
                        if (type == "REG_DWORD")
                        {
                            try
                            {
                                long val = Convert.ToInt64(apply);
                            }
                            catch
                            {
                                errors.Add(string.Format("{0}[{1}]: apply '{2}' не приводится к long для REG_DWORD ({3})", id, r, apply, name));
                                errCount++;
                            }
                        }
                    }

                    // Проверка revert
                    if (revert == null)
                    {
                        errors.Add(string.Format("{0}[{1}]: отсутствует объект revert ({2})", id, r, name));
                        errCount++;
                    }
                    else
                    {
                        string revMode = revert.ContainsKey("mode") ? revert["mode"] as string : null;
                        object revVal = revert.ContainsKey("value") ? revert["value"] : null;

                        if (revMode != "set" && revMode != "delete" && revMode != "snapshot" && revMode != "absent" && revMode != "none")
                        {
                            errors.Add(string.Format("{0}[{1}]: неизвестный режим revert '{2}' ({3})", id, r, revMode, name));
                            errCount++;
                        }

                        if (revMode == "set")
                        {
                            if (revVal == null)
                            {
                                errors.Add(string.Format("{0}[{1}]: revert.mode='set', но revert.value==null ({2})", id, r, name));
                                errCount++;
                            }
                            else if (apply != null && !(apply is Dictionary<string, object>) && apply.ToString() == revVal.ToString())
                            {
                                errors.Add(string.Format("{0}[{1}]: revert.value РАВЕН apply ('{2}') ({3})", id, r, apply, name));
                                errCount++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine(string.Format("\nРезультаты аудита: ОШИБОК: {0}, ПРЕДУПРЕЖДЕНИЙ: {1}", errCount, warnCount));

            if (errors.Count > 0)
            {
                Console.WriteLine("\n--- ПЕРВЫЕ 30 ОШИБОК ---");
                for (int i = 0; i < Math.Min(30, errors.Count); i++)
                {
                    Console.WriteLine(" [ERR] " + errors[i]);
                }
            }

            if (warnings.Count > 0)
            {
                Console.WriteLine("\n--- ПЕРВЫЕ 20 ПРЕДУПРЕЖДЕНИЙ ---");
                for (int i = 0; i < Math.Min(20, warnings.Count); i++)
                {
                    Console.WriteLine(" [WARN] " + warnings[i]);
                }
            }
        }
    }
}
