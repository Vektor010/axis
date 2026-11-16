using System;
using System.Diagnostics;
using System.IO;

namespace Finder
{
    class Program
    {
        static void Main()
        {
            Process p = new Process();
            p.StartInfo.FileName = @"C:\Users\User\Documents\win-tweaker\dist\Tests.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            string line;
            while ((line = p.ReadLineOrNull()) != null)
            {
                if (line.StartsWith("[FAIL]"))
                {
                    Console.WriteLine(line);
                }
            }
            p.WaitForExit();
        }
    }

    static class Ex
    {
        public static string ReadLineOrNull(this Process p)
        {
            return p.StandardOutput.ReadLine();
        }
    }
}
