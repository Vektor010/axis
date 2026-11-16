using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Baseline
{
    public partial class BaselineWindow : Window
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        public BaselineWindow()
        {
            App.SwWinCtorMs = App.Sw.ElapsedMilliseconds;
            InitializeComponent();
            App.SwInitCompMs = App.Sw.ElapsedMilliseconds;

            this.Loaded += delegate
            {
                App.SwLoadedMs = App.Sw.ElapsedMilliseconds;
            };
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            App.SwRenderedMs = App.Sw.ElapsedMilliseconds;
            long memRenderedWs = Process.GetCurrentProcess().WorkingSet64;
            long memRenderedPm = Process.GetCurrentProcess().PrivateMemorySize64;

            try
            {
                DateTime processStart = Process.GetCurrentProcess().StartTime.ToUniversalTime();
                DateTime mainStart = DateTime.UtcNow - TimeSpan.FromMilliseconds(App.SwRenderedMs);
                DateTime contentRendered = DateTime.UtcNow;

                int tier = RenderCapability.Tier >> 16;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== BASELINE CHECKPOINTS ===");
                sb.AppendLine(string.Format("Main: {0} ms", App.SwMainMs));
                sb.AppendLine(string.Format("App ctor: {0} ms", App.SwAppCtorMs));
                sb.AppendLine(string.Format("Window ctor: {0} ms", App.SwWinCtorMs));
                sb.AppendLine(string.Format("After InitializeComponent: {0} ms", App.SwInitCompMs));
                sb.AppendLine(string.Format("Loaded: {0} ms", App.SwLoadedMs));
                sb.AppendLine(string.Format("ContentRendered: {0} ms", App.SwRenderedMs));
                sb.AppendLine(string.Format("RenderTier: {0}", tier));
                sb.AppendLine(string.Format("PreMainMs={0:F2}", (mainStart - processStart).TotalMilliseconds));
                sb.AppendLine(string.Format("MainToRenderMs={0:F2}", (double)App.SwRenderedMs));
                sb.AppendLine(string.Format("TotalMs={0:F2}", (contentRendered - processStart).TotalMilliseconds));
                Process current = Process.GetCurrentProcess();
                sb.AppendLine(string.Format("WorkingSet64={0}", current.WorkingSet64));
                sb.AppendLine(string.Format("PrivateMemorySize64={0}", current.PrivateMemorySize64));

                // Полный дамп Process.Modules Baseline после первого кадра в tests\out\baseline-modules.txt
                try
                {
                    string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tests", "out");
                    if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                    string bModFile = Path.Combine(outDir, "baseline-modules.txt");
                    StringBuilder sbFullMod = new StringBuilder();
                    foreach (ProcessModule m in current.Modules)
                    {
                        sbFullMod.AppendLine(string.Format("{0} | {1} | {2}", m.ModuleName, m.ModuleMemorySize, m.FileName));
                    }
                    File.WriteAllText(bModFile, sbFullMod.ToString(), Encoding.UTF8);
                }
                catch { }

                string metricsPath = Path.Combine(Path.GetTempPath(), "baseline_first_render.txt");
                File.WriteAllText(metricsPath, sb.ToString(), Encoding.UTF8);

                AttachConsole(-1);
                Console.WriteLine(sb.ToString());
            }
            catch { }

            string[] args = Environment.GetCommandLineArgs();
            bool isBenchmark = false;
            bool isIdleBenchmark = false;
            bool isDiagAssemblies = false;
            bool isCheckpoints = false;
            bool isDiagMem = false;
            bool isKeepOpen = false;

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--benchmark") isBenchmark = true;
                    if (args[i] == "--idle-benchmark") isIdleBenchmark = true;
                    if (args[i] == "--diag-assemblies") isDiagAssemblies = true;
                    if (args[i] == "--checkpoints") isCheckpoints = true;
                    if (args[i] == "--diag-mem") isDiagMem = true;
                    if (args[i] == "--keep-open") isKeepOpen = true;
                }
            }

            if (isKeepOpen)
            {
                return;
            }

            if (isDiagAssemblies)
            {
                try
                {
                    string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tests", "out");
                    if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                    string asmFile = Path.Combine(outDir, "baseline-assemblies.txt");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("=== СБОРКИ НА МОМЕНТ ВХОДА В MAIN ===");
                    for (int a = 0; a < App.AssembliesAtMain.Count; a++)
                    {
                        sb.AppendLine(App.AssembliesAtMain[a]);
                    }
                    sb.AppendLine("\n=== ЖУРНАЛ ПОДПИСКИ AppDomain.AssemblyLoad ===");
                    sb.AppendLine("мс от Process.StartTime | имя сборки | путь");
                    for (int a = 0; a < App.AssemblyLoadLog.Count; a++)
                    {
                        sb.AppendLine(App.AssemblyLoadLog[a]);
                    }
                    File.WriteAllText(asmFile, sb.ToString(), Encoding.UTF8);

                    string modFile = Path.Combine(outDir, "baseline-modules.txt");
                    StringBuilder sbMod = new StringBuilder();
                    foreach (ProcessModule m in Process.GetCurrentProcess().Modules)
                    {
                        sbMod.AppendLine(string.Format("{0} | {1} | {2}", m.ModuleName, m.ModuleMemorySize, m.FileName));
                    }
                    File.WriteAllText(modFile, sbMod.ToString(), Encoding.UTF8);
                }
                catch { }
                Application.Current.Shutdown(0);
                return;
            }

            if (isCheckpoints)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Tick += delegate
                {
                    timer.Stop();
                    try
                    {
                        Process p = Process.GetCurrentProcess();
                        long mem10sWs = p.WorkingSet64;
                        long mem10sPm = p.PrivateMemorySize64;

                        string cpFile = Path.Combine(Path.GetTempPath(), "baseline_checkpoints.txt");
                        string cpContent = string.Format(
                            "MainWs={0}\nMainPm={1}\nAppCtorWs={2}\nAppCtorPm={3}\nWinCtorWs={4}\nWinCtorPm={5}\nRenderedWs={6}\nRenderedPm={7}\nIdle10sWs={8}\nIdle10sPm={9}\n",
                            App.MemMainWs, App.MemMainPm,
                            App.MemAppCtorWs, App.MemAppCtorPm,
                            App.MemWinCtorWs, App.MemWinCtorPm,
                            memRenderedWs, memRenderedPm,
                            mem10sWs, mem10sPm);
                        File.WriteAllText(cpFile, cpContent, Encoding.UTF8);

                        string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tests", "out");
                        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                        string modFile = Path.Combine(outDir, "baseline-modules.txt");
                        StringBuilder sbMod = new StringBuilder();
                        foreach (ProcessModule m in p.Modules)
                        {
                            sbMod.AppendLine(string.Format("{0} | {1} | {2}", m.ModuleName, m.ModuleMemorySize, m.FileName));
                        }
                        File.WriteAllText(modFile, sbMod.ToString(), Encoding.UTF8);
                    }
                    catch { }
                    Application.Current.Shutdown(0);
                };
                timer.Start();
                return;
            }

            if (isBenchmark && isDiagMem)
            {
                int second = 0;
                StringBuilder diagSb = new StringBuilder();
                diagSb.AppendLine("Second|WorkingSet64|WorkingSetPrivate|PrivateBytes|GCTotalMemory|GC0|GC1|GC2|Threads|Handles");

                Action recordSample = delegate
                {
                    try
                    {
                        Process p = Process.GetCurrentProcess();
                        long ws = p.WorkingSet64;
                        long pb = p.PrivateMemorySize64;
                        long wsPriv = 0;
                        try
                        {
                            using (PerformanceCounter pc = new PerformanceCounter("Process", "Working Set - Private", p.ProcessName))
                            {
                                wsPriv = pc.RawValue;
                            }
                        }
                        catch { }

                        long gcMem = GC.GetTotalMemory(false);
                        int gc0 = GC.CollectionCount(0);
                        int gc1 = GC.CollectionCount(1);
                        int gc2 = GC.CollectionCount(2);
                        int thCount = p.Threads.Count;
                        int hCount = p.HandleCount;

                        diagSb.AppendLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                            second, ws, wsPriv, pb, gcMem, gc0, gc1, gc2, thCount, hCount));
                    }
                    catch { }
                };

                recordSample();

                DispatcherTimer diagTimer = new DispatcherTimer();
                diagTimer.Interval = TimeSpan.FromSeconds(1);
                diagTimer.Tick += delegate
                {
                    second++;
                    recordSample();

                    if (second >= 15)
                    {
                        diagTimer.Stop();
                        try
                        {
                            string outPath = Path.Combine(Path.GetTempPath(), "baseline_diag_mem.txt");
                            File.WriteAllText(outPath, diagSb.ToString(), Encoding.UTF8);
                        }
                        catch { }
                        Application.Current.Shutdown(0);
                    }
                };
                diagTimer.Start();
                return;
            }

            if (isIdleBenchmark)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Tick += delegate
                {
                    timer.Stop();
                    try
                    {
                        Process p = Process.GetCurrentProcess();
                        long wsPrivate = 0;
                        try
                        {
                            using (PerformanceCounter pc = new PerformanceCounter("Process", "Working Set - Private", p.ProcessName))
                            {
                                wsPrivate = pc.RawValue;
                            }
                        }
                        catch { }

                        string memPath = Path.Combine(Path.GetTempPath(), "baseline_memory.txt");
                        string memContent = string.Format("WorkingSet64={0}\nPrivateMemorySize64={1}\nWorkingSetPrivate={2}\n",
                            p.WorkingSet64, p.PrivateMemorySize64, wsPrivate);
                        File.WriteAllText(memPath, memContent);

                        string outDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tests", "out");
                        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                        string modFile = Path.Combine(outDir, "baseline-modules.txt");
                        StringBuilder sbMod = new StringBuilder();
                        foreach (ProcessModule m in p.Modules)
                        {
                            sbMod.AppendLine(string.Format("{0} | {1} | {2}", m.ModuleName, m.ModuleMemorySize, m.FileName));
                        }
                        File.WriteAllText(modFile, sbMod.ToString(), Encoding.UTF8);
                    }
                    catch { }
                    Application.Current.Shutdown(0);
                };
                timer.Start();
            }
            else
            {
                Application.Current.Shutdown(0);
            }
        }
    }
}
