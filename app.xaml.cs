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
                MainWindow window = new MainWindow(true, isDark);
                window.Width = 1100;
                window.Height = 740;

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
                if (w <= 0) w = 1100;
                if (h <= 0) h = 740;

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

        public void ApplyTheme(bool isDark)
        {
            ResourceDictionary res = this.Resources;

            if (isDark)
            {
                // Dark Theme Palette (Tachyon: Deep violet-tinted dark)
                res["WindowBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B0B12"));
                res["SidebarBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F17"));
                res["SidebarBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242434"));
                res["CardBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12121B"));
                res["CardBgHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#181826"));
                res["CardBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242434"));
                res["CardBorderHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#363650"));

                res["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8E8F2"));
                res["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8BA3"));

                res["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B7CF6"));
                res["AccentFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#268B7CF6"));
                res["AccentBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B7CF6"));

                res["SuccessBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                res["SuccessFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2622C55E"));
                res["SuccessBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));

                res["DangerBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                res["DangerFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26EF4444"));
                res["DangerBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));

                res["WarningBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                res["WarningFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26F59E0B"));
                res["WarningBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));

                res["ToggleBgOff"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242434"));
                res["ToggleThumbOff"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8BA3"));
                res["ToggleBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#35354D"));
                res["ToggleBgHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E2E42"));
                res["ToggleBorderHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B4B69"));

                res["ButtonBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#181824"));
                res["ButtonBgHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222233"));
                res["ButtonBgPressed"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14141F"));
                res["ButtonBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E2E42"));

                res["NavHoverBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A26"));
                res["NavSelectedBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232334"));
                res["SearchBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141420"));
                res["SearchBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28283C"));
                res["ScrollThumbBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B3B52"));
                res["BadgeBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A28"));
                res["HeaderBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#101018"));
            }
            else
            {
                // Light Theme Palette (Tachyon: Clean airy light)
                res["WindowBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F8"));
                res["SidebarBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAF2"));
                res["SidebarBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1E1EC"));
                res["CardBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                res["CardBgHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6F6FC"));
                res["CardBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1E1EC"));
                res["CardBorderHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CECEDD"));

                res["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B1B28"));
                res["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B6B80"));

                res["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B7CF6"));
                res["AccentFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#268B7CF6"));
                res["AccentBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B7CF6"));

                res["SuccessBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                res["SuccessFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2622C55E"));
                res["SuccessBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));

                res["DangerBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                res["DangerFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26EF4444"));
                res["DangerBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));

                res["WarningBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                res["WarningFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26F59E0B"));
                res["WarningBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));

                res["ToggleBgOff"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E2ED"));
                res["ToggleThumbOff"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B6B80"));
                res["ToggleBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0D0DE"));
                res["ToggleBgHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8D8E5"));
                res["ToggleBorderHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B8B8CC"));

                res["ButtonBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                res["ButtonBgHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F7"));
                res["ButtonBgPressed"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4E4EE"));
                res["ButtonBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D5D5E2"));

                res["NavHoverBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DFDFEC"));
                res["NavSelectedBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4D4E4"));
                res["SearchBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                res["SearchBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D2D2E0"));
                res["ScrollThumbBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C4C4D4"));
                res["BadgeBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8E8F2"));
                res["HeaderBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8FC"));
            }
        }
    }
}
