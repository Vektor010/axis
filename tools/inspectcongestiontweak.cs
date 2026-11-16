using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Inspector
{
    class Program
    {
        static void Main()
        {
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var list = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(@"C:\Users\User\Documents\win-tweaker\data\tweaks.json", Encoding.UTF8));
            foreach (var t in list)
            {
                if ((t["id"] as string) == "network-congestion-provider-bbr")
                {
                    Console.WriteLine(ser.Serialize(t));
                }
            }
        }
    }
}
