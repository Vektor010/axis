using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Baseline
{
    public partial class App : Application
    {
        public static Stopwatch Sw = new Stopwatch();
        public static long SwMainMs = 0;
        public static long SwAppCtorMs = 0;
        public static long SwWinCtorMs = 0;
        public static long SwInitCompMs = 0;
        public static long SwLoadedMs = 0;
        public static long SwRenderedMs = 0;

        public static DateTime ProcStartUtc;
        public static DateTime MainStartUtc;
        public static long MemMainWs;
        public static long MemMainPm;
        public static long MemAppCtorWs;
        public static long MemAppCtorPm;
        public static long MemWinCtorWs;
        public static long MemWinCtorPm;

        public static System.Collections.Generic.List<string> AssemblyLoadLog = new System.Collections.Generic.List<string>();
        public static System.Collections.Generic.List<string> AssembliesAtMain = new System.Collections.Generic.List<string>();

        public App()
        {
            SwAppCtorMs = Sw.ElapsedMilliseconds;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            ProcStartUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime();
            MainStartUtc = DateTime.UtcNow;
            MemMainWs = Process.GetCurrentProcess().WorkingSet64;
            MemMainPm = Process.GetCurrentProcess().PrivateMemorySize64;

            // Сборки на момент входа в Main
            System.Reflection.Assembly[] initialAsms = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < initialAsms.Length; i++)
            {
                string loc = "";
                try { loc = initialAsms[i].Location; } catch { }
                AssembliesAtMain.Add(string.Format("{0} | {1}", initialAsms[i].GetName().Name, loc));
            }

            // Подписка на AssemblyLoad в первой строке Main
            AppDomain.CurrentDomain.AssemblyLoad += delegate(object sender, AssemblyLoadEventArgs aArgs)
            {
                double ms = (DateTime.UtcNow - ProcStartUtc).TotalMilliseconds;
                string loc = "";
                try { loc = aArgs.LoadedAssembly.Location; } catch { }
                AssemblyLoadLog.Add(string.Format("{0:F1} ms | {1} | {2}", ms, aArgs.LoadedAssembly.GetName().Name, loc));
            };

            Sw.Start();
            SwMainMs = Sw.ElapsedMilliseconds;

            if (args != null && Array.IndexOf(args, "--profile-opt") >= 0)
            {
                try
                {
                    string profileDir = Path.Combine(Path.GetTempPath(), "BaselineProfile");
                    if (!Directory.Exists(profileDir)) Directory.CreateDirectory(profileDir);
                    System.Runtime.ProfileOptimization.SetProfileRoot(profileDir);
                    System.Runtime.ProfileOptimization.StartProfile("Baseline.profile");
                }
                catch { }
            }

            App app = new App();
            app.InitializeComponent();
            MemAppCtorWs = Process.GetCurrentProcess().WorkingSet64;
            MemAppCtorPm = Process.GetCurrentProcess().PrivateMemorySize64;

            BaselineWindow win = new BaselineWindow();
            MemWinCtorWs = Process.GetCurrentProcess().WorkingSet64;
            MemWinCtorPm = Process.GetCurrentProcess().PrivateMemorySize64;

            app.Run(win);
        }
    }
}
