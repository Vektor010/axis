using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace WinTweaker
{
    public partial class App : Application
    {
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

            // Подписка на AssemblyLoad
            AppDomain.CurrentDomain.AssemblyLoad += delegate(object sender, AssemblyLoadEventArgs aArgs)
            {
                double ms = (DateTime.UtcNow - ProcStartUtc).TotalMilliseconds;
                string loc = "";
                try { loc = aArgs.LoadedAssembly.Location; } catch { }
                AssemblyLoadLog.Add(string.Format("{0:F1} ms | {1} | {2}", ms, aArgs.LoadedAssembly.GetName().Name, loc));
            };

            WinTweaker.App app = new WinTweaker.App();
            app.InitializeComponent();
            MemAppCtorWs = Process.GetCurrentProcess().WorkingSet64;
            MemAppCtorPm = Process.GetCurrentProcess().PrivateMemorySize64;

            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool enableProfileOpt = true;
            if (e.Args != null)
            {
                for (int i = 0; i < e.Args.Length; i++)
                {
                    if (e.Args[i] == "--no-profile-opt") enableProfileOpt = false;
                }
            }

            if (enableProfileOpt)
            {
                try
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string profileDir = System.IO.Path.Combine(localAppData, "Tweaker");
                    if (!System.IO.Directory.Exists(profileDir))
                    {
                        System.IO.Directory.CreateDirectory(profileDir);
                    }
                    System.Runtime.ProfileOptimization.SetProfileRoot(profileDir);
                    System.Runtime.ProfileOptimization.StartProfile("Startup.profile");
                }
                catch { }
            }

            base.OnStartup(e);

            bool isDark = true;
            bool isReal = true;
            string screenshotPath = null;

            if (e.Args != null)
            {
                for (int i = 0; i < e.Args.Length; i++)
                {
                    if (e.Args[i] == "--light" || e.Args[i] == "-light")
                    {
                        isDark = false;
                    }
                    else if (e.Args[i] == "--mock" || e.Args[i] == "--test" || e.Args[i] == "-test")
                    {
                        isReal = false;
                    }
                    else if (e.Args[i] == "--screenshot" && i + 1 < e.Args.Length)
                    {
                        screenshotPath = e.Args[i + 1];
                    }
                }
            }

            this.ApplyTheme(isDark);

            if (!string.IsNullOrEmpty(screenshotPath))
            {
                this.SaveWindowScreenshot(screenshotPath, isDark, e.Args);
                this.Shutdown(0);
                return;
            }

            MainWindow mainWin = new MainWindow(isReal, isDark);
            MemWinCtorWs = Process.GetCurrentProcess().WorkingSet64;
            MemWinCtorPm = Process.GetCurrentProcess().PrivateMemorySize64;
            mainWin.Show();
        }

        private void SaveWindowScreenshot(string path, bool isDark, string[] args)
        {
            try
            {
                bool isReal = true;
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "--mock" || args[i] == "--test" || args[i] == "-test")
                        {
                            isReal = false;
                        }
                    }
                }

                MainWindow window = new MainWindow(isReal, isDark);
                window.Width = 1180;
                window.Height = 800;

                string tabToSelect = null;
                string searchQuery = null;
                bool expandFirstCard = false;
                string fontMode = null;
                bool history60s = false;
                string categoryToSelect = null;
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "--tab" && i + 1 < args.Length)
                        {
                            tabToSelect = args[i + 1];
                        }
                        else if (args[i] == "--category" && i + 1 < args.Length)
                        {
                            categoryToSelect = args[i + 1];
                        }
                        else if (args[i] == "--search" && i + 1 < args.Length)
                        {
                            searchQuery = args[i + 1];
                        }
                        else if (args[i] == "--expand-card")
                        {
                            expandFirstCard = true;
                        }
                        else if (args[i] == "--font-mode" && i + 1 < args.Length)
                        {
                            fontMode = args[i + 1];
                        }
                        else if (args[i] == "--history-60s")
                        {
                            history60s = true;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(fontMode))
                {
                    if (string.Equals(fontMode, "grayscale", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Windows.Media.TextOptions.SetTextRenderingMode(window, System.Windows.Media.TextRenderingMode.Grayscale);
                    }
                    else if (string.Equals(fontMode, "cleartype", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Windows.Media.TextOptions.SetTextRenderingMode(window, System.Windows.Media.TextRenderingMode.ClearType);
                    }
                }

                WinTweaker.ViewModels.MainViewModel vm = window.DataContext as WinTweaker.ViewModels.MainViewModel;
                if (vm != null)
                {
                    if (history60s)
                    {
                        vm.Populate60SecondsHistory();
                    }

                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        vm.SearchQuery = searchQuery;
                    }

                    if (!string.IsNullOrEmpty(tabToSelect))
                    {
                        for (int n = 0; n < vm.NavItems.Count; n++)
                        {
                            if (string.Equals(vm.NavItems[n].Id, tabToSelect, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(vm.NavItems[n].Name, tabToSelect, StringComparison.OrdinalIgnoreCase))
                            {
                                vm.SelectedNavItem = vm.NavItems[n];
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(categoryToSelect))
                    {
                        for (int c = 0; c < vm.Categories.Count; c++)
                        {
                            if (string.Equals(vm.Categories[c].Id, categoryToSelect, StringComparison.OrdinalIgnoreCase))
                            {
                                vm.SelectedCategory = vm.Categories[c];
                                break;
                            }
                        }
                    }

                    if (expandFirstCard && vm.FilteredTweaks.Count > 0)
                    {
                        vm.FilteredTweaks[0].IsExpanded = true;
                    }
                }

                window.Show();
                window.UpdateLayout();

                int w = (int)window.ActualWidth;
                int h = (int)window.ActualHeight;
                if (w <= 0) w = 1180;
                if (h <= 0) h = 800;

                System.Windows.Media.Imaging.RenderTargetBitmap rtb =
                    new System.Windows.Media.Imaging.RenderTargetBitmap(w, h, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                rtb.Render(window);

                string dir = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
                    encoder.Save(fs);
                }

                window.Close();
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.WriteAllText(path + ".err.txt", ex.ToString());
                }
                catch { }
            }
        }

        private static SolidColorBrush CreateFrozenBrush(string hex)
        {
            SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        private static LinearGradientBrush CreateFrozenGradient(string hexStart, string hexEnd)
        {
            LinearGradientBrush brush = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString(hexStart),
                (Color)ColorConverter.ConvertFromString(hexEnd),
                new Point(0, 0), new Point(1, 1));
            brush.Freeze();
            return brush;
        }

        private static System.Collections.Generic.Dictionary<string, Brush> BuildDarkPalette()
        {
            var d = new System.Collections.Generic.Dictionary<string, Brush>(35);
            d["WindowBg"] = CreateFrozenBrush("#0B0B12");
            d["SidebarBg"] = CreateFrozenBrush("#0F0F17");
            d["SidebarBorder"] = CreateFrozenBrush("#242434");
            d["CardBg"] = CreateFrozenBrush("#12121B");
            d["CardBgHover"] = CreateFrozenBrush("#181826");
            d["CardBorder"] = CreateFrozenBrush("#242434");
            d["CardBorderHover"] = CreateFrozenBrush("#363650");

            d["TextPrimary"] = CreateFrozenBrush("#E8E8F2");
            d["TextSecondary"] = CreateFrozenBrush("#8B8BA3");

            d["AccentBrush"] = CreateFrozenBrush("#8B7CF6");
            d["AccentFill"] = CreateFrozenBrush("#268B7CF6");
            d["AccentBorder"] = CreateFrozenBrush("#8B7CF6");

            d["CyanAccentBrush"] = CreateFrozenBrush("#22D3EE");
            d["CyanFill"] = CreateFrozenBrush("#2222D3EE");
            d["AccentGradient"] = CreateFrozenGradient("#7C3AED", "#6366F1");
            d["HeroCardBg"] = CreateFrozenGradient("#17142B", "#11111C");

            d["SuccessBrush"] = CreateFrozenBrush("#22C55E");
            d["SuccessFill"] = CreateFrozenBrush("#2622C55E");
            d["SuccessBorder"] = CreateFrozenBrush("#22C55E");

            d["DangerBrush"] = CreateFrozenBrush("#EF4444");
            d["DangerFill"] = CreateFrozenBrush("#26EF4444");
            d["DangerBorder"] = CreateFrozenBrush("#EF4444");

            d["WarningBrush"] = CreateFrozenBrush("#F59E0B");
            d["WarningFill"] = CreateFrozenBrush("#26F59E0B");
            d["WarningBorder"] = CreateFrozenBrush("#F59E0B");

            d["ToggleBgOff"] = CreateFrozenBrush("#242434");
            d["ToggleThumbOff"] = CreateFrozenBrush("#8B8BA3");
            d["ToggleBorder"] = CreateFrozenBrush("#35354D");
            d["ToggleBgHover"] = CreateFrozenBrush("#2E2E42");
            d["ToggleBorderHover"] = CreateFrozenBrush("#4B4B69");

            d["ButtonBg"] = CreateFrozenBrush("#181824");
            d["ButtonBgHover"] = CreateFrozenBrush("#222233");
            d["ButtonBgPressed"] = CreateFrozenBrush("#14141F");
            d["ButtonBorder"] = CreateFrozenBrush("#2E2E42");

            d["NavHoverBg"] = CreateFrozenBrush("#1A1A26");
            d["NavSelectedBg"] = CreateFrozenBrush("#232334");
            d["SearchBg"] = CreateFrozenBrush("#141420");
            d["SearchBorder"] = CreateFrozenBrush("#28283C");
            d["ScrollThumbBg"] = CreateFrozenBrush("#3B3B52");
            d["BadgeBg"] = CreateFrozenBrush("#1A1A28");
            d["HeaderBg"] = CreateFrozenBrush("#101018");
            return d;
        }

        private static System.Collections.Generic.Dictionary<string, Brush> BuildLightPalette()
        {
            var d = new System.Collections.Generic.Dictionary<string, Brush>(35);
            d["WindowBg"] = CreateFrozenBrush("#F3F3F8");
            d["SidebarBg"] = CreateFrozenBrush("#EAEAF2");
            d["SidebarBorder"] = CreateFrozenBrush("#E1E1EC");
            d["CardBg"] = CreateFrozenBrush("#FFFFFF");
            d["CardBgHover"] = CreateFrozenBrush("#F6F6FC");
            d["CardBorder"] = CreateFrozenBrush("#E1E1EC");
            d["CardBorderHover"] = CreateFrozenBrush("#CECEDD");

            d["TextPrimary"] = CreateFrozenBrush("#1B1B28");
            d["TextSecondary"] = CreateFrozenBrush("#6B6B80");

            d["AccentBrush"] = CreateFrozenBrush("#7C3AED");
            d["AccentFill"] = CreateFrozenBrush("#207C3AED");
            d["AccentBorder"] = CreateFrozenBrush("#7C3AED");

            d["CyanAccentBrush"] = CreateFrozenBrush("#0891B2");
            d["CyanFill"] = CreateFrozenBrush("#200891B2");
            d["AccentGradient"] = CreateFrozenGradient("#7C3AED", "#4F46E5");
            d["HeroCardBg"] = CreateFrozenGradient("#F5F3FF", "#FFFFFF");

            d["SuccessBrush"] = CreateFrozenBrush("#22C55E");
            d["SuccessFill"] = CreateFrozenBrush("#2622C55E");
            d["SuccessBorder"] = CreateFrozenBrush("#22C55E");

            d["DangerBrush"] = CreateFrozenBrush("#EF4444");
            d["DangerFill"] = CreateFrozenBrush("#26EF4444");
            d["DangerBorder"] = CreateFrozenBrush("#EF4444");

            d["WarningBrush"] = CreateFrozenBrush("#F59E0B");
            d["WarningFill"] = CreateFrozenBrush("#26F59E0B");
            d["WarningBorder"] = CreateFrozenBrush("#F59E0B");

            d["ToggleBgOff"] = CreateFrozenBrush("#E2E2ED");
            d["ToggleThumbOff"] = CreateFrozenBrush("#6B6B80");
            d["ToggleBorder"] = CreateFrozenBrush("#D0D0DE");
            d["ToggleBgHover"] = CreateFrozenBrush("#D8D8E5");
            d["ToggleBorderHover"] = CreateFrozenBrush("#B8B8CC");

            d["ButtonBg"] = CreateFrozenBrush("#FFFFFF");
            d["ButtonBgHover"] = CreateFrozenBrush("#F0F0F7");
            d["ButtonBgPressed"] = CreateFrozenBrush("#E4E4EE");
            d["ButtonBorder"] = CreateFrozenBrush("#D5D5E2");

            d["NavHoverBg"] = CreateFrozenBrush("#DFDFEC");
            d["NavSelectedBg"] = CreateFrozenBrush("#D4D4E4");
            d["SearchBg"] = CreateFrozenBrush("#FFFFFF");
            d["SearchBorder"] = CreateFrozenBrush("#D2D2E0");
            d["ScrollThumbBg"] = CreateFrozenBrush("#C4C4D4");
            d["BadgeBg"] = CreateFrozenBrush("#E8E8F2");
            d["HeaderBg"] = CreateFrozenBrush("#F8F8FC");
            return d;
        }

        private static readonly System.Collections.Generic.Dictionary<string, Brush> _darkPalette = BuildDarkPalette();
        private static readonly System.Collections.Generic.Dictionary<string, Brush> _lightPalette = BuildLightPalette();

        public void ApplyTheme(bool isDark)
        {
            ResourceDictionary res = this.Resources;
            var palette = isDark ? _darkPalette : _lightPalette;
            foreach (System.Collections.Generic.KeyValuePair<string, Brush> kvp in palette)
            {
                res[kvp.Key] = kvp.Value;
            }
        }
    }
}
