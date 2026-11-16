using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

class Program
{
    static void Main()
    {
        string root = @"C:\Users\User\Documents\win-tweaker";
        string tweaksPath = Path.Combine(root, @"data\tweaks.json");

        var serializer = new JavaScriptSerializer();
        var existingTweaks = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(tweaksPath, Encoding.UTF8));

        var testIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "network-directaccess-passive-mode",
            "peripheral-printers-disable-webpnp-download",
            "privacy-troubleshooting-allow-recommendations",
            "system-push-notifications-no-cloud",
            "system-start-no-mfu-programs-list",
            "system-start-hide-recently-added-apps",
            "visibility-dwm-disallow-animations"
        };

        foreach (var t in existingTweaks)
        {
            string id = t["id"].ToString();
            if (testIds.Contains(id))
            {
                Console.WriteLine("FOUND ID: " + id);
                Console.WriteLine("Name: " + t["name_ru"]);
                if (t.ContainsKey("registry") && t["registry"] is System.Collections.ArrayList)
                {
                    var regList = t["registry"] as System.Collections.ArrayList;
                    foreach (Dictionary<string, object> r in regList)
                    {
                        Console.WriteLine("Reg: " + r["hive"] + "\\" + r["path"] + "\\" + r["name"] + " = " + r["apply"]);
                    }
                }
                Console.WriteLine("---------------------------------------------");
            }
        }
    }
}
