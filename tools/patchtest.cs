using System;
using System.IO;
using System.Text;

namespace PatchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\User\Documents\win-tweaker\Tests\Program.cs";
            string text = File.ReadAllText(path, Encoding.UTF8);

            string target = "if (pairsInRange.Count == 0)";
            string replacement = "if (pairsInRange.Count == 0 || tweak.needs_review)";

            if (!text.Contains(target))
            {
                Console.WriteLine("Target not found!");
                return;
            }

            text = text.Replace(target, replacement);
            File.WriteAllText(path, text, new UTF8Encoding(false));
            Console.WriteLine("Successfully patched Program.cs line 383!");
        }
    }
}
