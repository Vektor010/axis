using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using WinTweaker.ViewModels;

namespace WinTweaker.Benchmark
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("   AXIS v2.0 THEME TOGGLE PERFORMANCE BENCHMARK   ");
            Console.WriteLine("=================================================");

            // Warm up WPF app and resources
            WinTweaker.App app = new WinTweaker.App();
            app.InitializeComponent();
            app.ApplyTheme(true);

            MainViewModel vm = new MainViewModel(false, true);
            MainWindow win = new MainWindow(false, true);

            // Warm up
            for (int i = 0; i < 5; i++)
            {
                vm.ToggleThemeCommand.Execute(null);
            }

            const int cycles = 30;
            List<double> latencies = new List<double>();
            Stopwatch sw = new Stopwatch();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memBefore = Process.GetCurrentProcess().WorkingSet64;
            long privBefore = Process.GetCurrentProcess().PrivateMemorySize64;

            for (int i = 0; i < cycles; i++)
            {
                sw.Restart();
                vm.ToggleThemeCommand.Execute(null);
                // Dispatch layout pass
                win.Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Render);
                sw.Stop();
                latencies.Add(sw.Elapsed.TotalMilliseconds);
            }

            long memAfter = Process.GetCurrentProcess().WorkingSet64;
            long privAfter = Process.GetCurrentProcess().PrivateMemorySize64;

            latencies.Sort();
            double min = latencies[0];
            double max = latencies[latencies.Count - 1];
            double median = latencies[latencies.Count / 2];
            int p95Index = (int)Math.Ceiling(latencies.Count * 0.95) - 1;
            double p95 = latencies[p95Index];
            double avg = latencies.Average();

            Console.WriteLine(string.Format("Cycles:       {0}", cycles));
            Console.WriteLine(string.Format("Min:          {0:F2} ms", min));
            Console.WriteLine(string.Format("Median (P50): {0:F2} ms", median));
            Console.WriteLine(string.Format("P95:          {0:F2} ms", p95));
            Console.WriteLine(string.Format("Max:          {0:F2} ms", max));
            Console.WriteLine(string.Format("Average:      {0:F2} ms", avg));
            Console.WriteLine(string.Format("WS Delta:     {0:F2} MB", (memAfter - memBefore) / (1024.0 * 1024.0)));
            Console.WriteLine(string.Format("Private Delta:{0:F2} MB", (privAfter - privBefore) / (1024.0 * 1024.0)));

            Console.WriteLine("Individual latencies (ms):");
            for (int i = 0; i < latencies.Count; i++)
            {
                Console.Write(string.Format("{0:F1} ", latencies[i]));
                if ((i + 1) % 10 == 0) Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("=================================================");
        }
    }
}
