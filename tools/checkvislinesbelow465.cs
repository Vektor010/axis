using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace CheckerVis
{
    class Program
    {
        static void Main()
        {
            var ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            var list = ser.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(@"C:\Users\User\Documents\win-tweaker\data\tweaks.json", Encoding.UTF8));
            int countBelow465 = 0;
            foreach (var t in list)
            {
                var src = t["source"] as Dictionary<string, object>;
                if (src != null && (src["file"] as string) == "visibility/desc.md")
                {
                    int lineFrom = (int)src["line_from"];
                    if (lineFrom >= 465)
                    {
                        countBelow465++;
                        Console.WriteLine(string.Format("{0}: lines {1}..{2}", t["id"], lineFrom, src["line_to"]));
                    }
                }
            }
            Console.WriteLine("Всего твиков visibility ниже строки 465: " + countBelow465);
        }
    }
}
