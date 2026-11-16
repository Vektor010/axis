using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace WinTweaker.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(jsonPath, Encoding.UTF8);
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var tweaks = serializer.Deserialize<List<Dictionary<string, object>>>(raw);

            var fixes = new Dictionary<string, Tuple<int, int>>
            {
                { "privacy-disable-wer", Tuple.Create(330, 335) },
                { "privacy-disable-recall", Tuple.Create(844, 848) },
                { "privacy-disable-click-to-do", Tuple.Create(846, 850) },
                { "privacy-disable-gamedvr-app-capture", Tuple.Create(861, 865) },
                { "security-disable-driver-searching", Tuple.Create(486, 490) },
                { "security-defender-disable-realtime", Tuple.Create(289, 293) }
            };

            int fixedCount = 0;
            foreach (var t in tweaks)
            {
                if (t.ContainsKey("id") && t["id"] != null)
                {
                    string id = t["id"].ToString();
                    if (fixes.ContainsKey(id))
                    {
                        var tuple = fixes[id];
                        var src = t["source"] as Dictionary<string, object>;
                        if (src != null)
                        {
                            src["line_from"] = tuple.Item1;
                            src["line_to"] = tuple.Item2;
                            fixedCount++;
                            Console.WriteLine("Fixed range for " + id + ": " + tuple.Item1 + ".." + tuple.Item2);
                        }
                    }
                }
            }

            string updated = serializer.Serialize(tweaks);
            File.WriteAllText(jsonPath, updated, Encoding.UTF8);
            Console.WriteLine("Done! Fixed {0} tweaks.", fixedCount);
        }
    }
}
