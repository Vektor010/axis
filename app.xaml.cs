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
                window.Width = 1200;
                window.Height = 840;

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
                if (w <= 0) w = 1200;
                if (h <= 0) h = 840;

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
            d["WindowBg"] = CreateFrozenBrush("#07090E");
            d["SidebarBg"] = CreateFrozenBrush("#0B0F17");
            d["SidebarBorder"] = CreateFrozenBrush("#1D263B");
            d["CardBg"] = CreateFrozenBrush("#111622");
            d["CardBgHover"] = CreateFrozenBrush("#171F30");
            d["CardBorder"] = CreateFrozenBrush("#1F2A40");
            d["CardBorderHover"] = CreateFrozenBrush("#32456B");

            d["TextPrimary"] = CreateFrozenBrush("#ECF2FF");
            d["TextSecondary"] = CreateFrozenBrush("#8B99B5");

            d["AccentBrush"] = CreateFrozenBrush("#3B82F6");
            d["AccentFill"] = CreateFrozenBrush("#243B82F6");
            d["AccentBorder"] = CreateFrozenBrush("#3B82F6");

            d["CyanAccentBrush"] = CreateFrozenBrush("#38BDF8");
            d["CyanFill"] = CreateFrozenBrush("#2038BDF8");
            d["AccentGradient"] = CreateFrozenGradient("#2563EB", "#3B82F6");
            d["HeroCardBg"] = CreateFrozenGradient("#0E1729", "#0B0F1A");

            d["SuccessBrush"] = CreateFrozenBrush("#22C55E");
            d["SuccessFill"] = CreateFrozenBrush("#2422C55E");
            d["SuccessBorder"] = CreateFrozenBrush("#22C55E");

            d["DangerBrush"] = CreateFrozenBrush("#EF4444");
            d["DangerFill"] = CreateFrozenBrush("#24EF4444");
            d["DangerBorder"] = CreateFrozenBrush("#EF4444");

            d["WarningBrush"] = CreateFrozenBrush("#F59E0B");
            d["WarningFill"] = CreateFrozenBrush("#24F59E0B");
            d["WarningBorder"] = CreateFrozenBrush("#F59E0B");

            d["ToggleBgOff"] = CreateFrozenBrush("#1E283D");
            d["ToggleThumbOff"] = CreateFrozenBrush("#8B99B5");
            d["ToggleBorder"] = CreateFrozenBrush("#2D3B57");
            d["ToggleBgHover"] = CreateFrozenBrush("#26334D");
            d["ToggleBorderHover"] = CreateFrozenBrush("#41567F");

            d["ButtonBg"] = CreateFrozenBrush("#151C2C");
            d["ButtonBgHover"] = CreateFrozenBrush("#1E283E");
            d["ButtonBgPressed"] = CreateFrozenBrush("#101624");
            d["ButtonBorder"] = CreateFrozenBrush("#293754");

            d["NavHoverBg"] = CreateFrozenBrush("#151D2E");
            d["NavSelectedBg"] = CreateFrozenBrush("#1B263B");
            d["SearchBg"] = CreateFrozenBrush("#0E131D");
            d["SearchBorder"] = CreateFrozenBrush("#232F46");
            d["ScrollThumbBg"] = CreateFrozenBrush("#334466");
            d["BadgeBg"] = CreateFrozenBrush("#161F30");
            d["HeaderBg"] = CreateFrozenBrush("#0D121C");
            return d;
        }

        private static System.Collections.Generic.Dictionary<string, Brush> BuildLightPalette()
        {
            var d = new System.Collections.Generic.Dictionary<string, Brush>(35);
            d["WindowBg"] = CreateFrozenBrush("#F1F5F9");
            d["SidebarBg"] = CreateFrozenBrush("#E8EEF7");
            d["SidebarBorder"] = CreateFrozenBrush("#D3DCEB");
            d["CardBg"] = CreateFrozenBrush("#FFFFFF");
            d["CardBgHover"] = CreateFrozenBrush("#F6F9FE");
            d["CardBorder"] = CreateFrozenBrush("#D8E1EF");
            d["CardBorderHover"] = CreateFrozenBrush("#B6C6DF");

            d["TextPrimary"] = CreateFrozenBrush("#0F172A");
            d["TextSecondary"] = CreateFrozenBrush("#5B6B86");

            d["AccentBrush"] = CreateFrozenBrush("#2563EB");
            d["AccentFill"] = CreateFrozenBrush("#1F2563EB");
            d["AccentBorder"] = CreateFrozenBrush("#2563EB");

            d["CyanAccentBrush"] = CreateFrozenBrush("#0284C7");
            d["CyanFill"] = CreateFrozenBrush("#1F0284C7");
            d["AccentGradient"] = CreateFrozenGradient("#2563EB", "#1D4ED8");
            d["HeroCardBg"] = CreateFrozenGradient("#EFF6FF", "#FFFFFF");

            d["SuccessBrush"] = CreateFrozenBrush("#16A34A");
            d["SuccessFill"] = CreateFrozenBrush("#2216A34A");
            d["SuccessBorder"] = CreateFrozenBrush("#16A34A");

            d["DangerBrush"] = CreateFrozenBrush("#DC2626");
            d["DangerFill"] = CreateFrozenBrush("#22DC2626");
            d["DangerBorder"] = CreateFrozenBrush("#DC2626");

            d["WarningBrush"] = CreateFrozenBrush("#D97706");
            d["WarningFill"] = CreateFrozenBrush("#22D97706");
            d["WarningBorder"] = CreateFrozenBrush("#D97706");

            d["ToggleBgOff"] = CreateFrozenBrush("#DCE4F0");
            d["ToggleThumbOff"] = CreateFrozenBrush("#64748B");
            d["ToggleBorder"] = CreateFrozenBrush("#C4D1E4");
            d["ToggleBgHover"] = CreateFrozenBrush("#CED8E8");
            d["ToggleBorderHover"] = CreateFrozenBrush("#A4B6D3");

            d["ButtonBg"] = CreateFrozenBrush("#FFFFFF");
            d["ButtonBgHover"] = CreateFrozenBrush("#EEF3FB");
            d["ButtonBgPressed"] = CreateFrozenBrush("#E0E8F5");
            d["ButtonBorder"] = CreateFrozenBrush("#CBD5E1");

            d["NavHoverBg"] = CreateFrozenBrush("#DDE6F3");
            d["NavSelectedBg"] = CreateFrozenBrush("#CFDCF0");
            d["SearchBg"] = CreateFrozenBrush("#FFFFFF");
            d["SearchBorder"] = CreateFrozenBrush("#CBD5E1");
            d["ScrollThumbBg"] = CreateFrozenBrush("#B5C4DB");
            d["BadgeBg"] = CreateFrozenBrush("#E7EEF9");
            d["HeaderBg"] = CreateFrozenBrush("#F8FAFC");
            return d;
        }

        private static readonly System.Collections.Generic.Dictionary<string, Brush> _darkPalette = BuildDarkPalette();
        private static readonly System.Collections.Generic.Dictionary<string, Brush> _lightPalette = BuildLightPalette();

        private static ResourceDictionary BuildThemeDictionary(System.Collections.Generic.Dictionary<string, Brush> palette)
        {
            ResourceDictionary dict = new ResourceDictionary();
            foreach (var kvp in palette)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }

        private static readonly ResourceDictionary _darkThemeDict = BuildThemeDictionary(_darkPalette);
        private static readonly ResourceDictionary _lightThemeDict = BuildThemeDictionary(_lightPalette);

        public void ApplyTheme(bool isDark)
        {
            var targetDict = isDark ? _darkThemeDict : _lightThemeDict;

            // 1. Atomic dictionary swap in MergedDictionaries: triggers ONE single unified WPF resource invalidation pass
            if (this.Resources.MergedDictionaries.Count > 0)
            {
                this.Resources.MergedDictionaries[0] = targetDict;
            }
            else
            {
                this.Resources.MergedDictionaries.Add(targetDict);
            }

            // 2. Keep direct keys updated for any components reading Application.Current.Resources[key]
            var palette = isDark ? _darkPalette : _lightPalette;
            foreach (System.Collections.Generic.KeyValuePair<string, Brush> kvp in palette)
            {
                this.Resources[kvp.Key] = kvp.Value;
            }
        }
    }
}
