using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using WinTweaker.Common;
using WinTweaker.Engine;
using WinTweaker.ViewModels;

namespace WinTweaker
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _disableMica = false;
        public static DateTime MainStartUtc = DateTime.UtcNow;
        public DateTime WindowCreatedUtc = DateTime.UtcNow;
        public DateTime ContentRenderedUtc;

        public MainWindow() : this(true, true)
        {
        }

        public MainWindow(bool isRealMode, bool isDarkTheme)
        {
            InitializeComponent();
            this._viewModel = new MainViewModel(isRealMode, isDarkTheme);

            string[] args = Environment.GetCommandLineArgs();
            string tabToSelect = null;
            string tweakToApply = null;

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--no-mica")
                    {
                        this._disableMica = true;
                    }
                    else if (args[i] == "--tab" && i + 1 < args.Length)
                    {
                        tabToSelect = args[i + 1];
                    }
                    else if (args[i] == "--apply-tweak" && i + 1 < args.Length)
                    {
                        tweakToApply = args[i + 1];
                    }
                }
            }

            if (!string.IsNullOrEmpty(tweakToApply))
            {
                for (int t = 0; t < this._viewModel.FilteredTweaks.Count; t++)
                {
                    var card = this._viewModel.FilteredTweaks[t];
                    if (card.Id == tweakToApply)
                    {
                        card.ToggleCommand.Execute(null);
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(tabToSelect))
            {
                for (int n = 0; n < this._viewModel.NavItems.Count; n++)
                {
                    if (string.Equals(this._viewModel.NavItems[n].Id, tabToSelect, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(this._viewModel.NavItems[n].Name, tabToSelect, StringComparison.OrdinalIgnoreCase))
                    {
                        this._viewModel.SelectedNavItem = this._viewModel.NavItems[n];
                        break;
                    }
                }
            }

            this._viewModel.ThemeChanged += this.OnThemeChanged;
            this.DataContext = this._viewModel;
            this.WindowCreatedUtc = DateTime.UtcNow;

            this.PreviewKeyDown += delegate(object sender, System.Windows.Input.KeyEventArgs kArgs)
            {
                bool ctrl = (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control;
                if (ctrl && (kArgs.Key == System.Windows.Input.Key.F || kArgs.Key == System.Windows.Input.Key.K))
                {
                    if (this.SearchBoxControl != null)
                    {
                        this.SearchBoxControl.Focus();
                        this.SearchBoxControl.SelectAll();
                        kArgs.Handled = true;
                    }
                }
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero && !this._disableMica)
            {
                HwndSource source = HwndSource.FromHwnd(hwnd);
                if (source != null && source.CompositionTarget != null)
                {
                    source.CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;
                }
            }
            bool isDark = this._viewModel != null ? this._viewModel.IsDarkTheme : true;
            this.ApplyWindowStyle(isDark);
            if (this._viewModel != null && this._viewModel.IsTweaksSelected)
            {
                this.SubscribeRegistryWatchers();
            }
            this.IsVisibleChanged += delegate
            {
                if (this.IsVisible && this._viewModel != null && this._viewModel.IsTweaksSelected) this.SubscribeRegistryWatchers();
                else this.UnsubscribeRegistryWatchers();
            };

            if (this._viewModel != null)
            {
                this._viewModel.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs pArgs)
                {
                    if (pArgs.PropertyName == "SelectedNavItem" || pArgs.PropertyName == "SelectedCategory")
                    {
                        if (this.IsVisible && this._viewModel.IsTweaksSelected)
                        {
                            this.SubscribeRegistryWatchers();
                        }
                        else
                        {
                            this.UnsubscribeRegistryWatchers();
                        }
                    }
                };
            }
        }

        private readonly System.Collections.Generic.List<RegistryWatcher> _watchers = new System.Collections.Generic.List<RegistryWatcher>();

        private void SubscribeRegistryWatchers()
        {
            this.UnsubscribeRegistryWatchers();
            if (this._viewModel == null || !this._viewModel.IsTweaksSelected || this._viewModel.FilteredTweaks == null) return;

            var watchedKeys = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < this._viewModel.FilteredTweaks.Count; i++)
            {
                var card = this._viewModel.FilteredTweaks[i];
                if (card.Model != null && card.Model.registry != null)
                {
                    for (int r = 0; r < card.Model.registry.Count; r++)
                    {
                        var reg = card.Model.registry[r];
                        if (string.IsNullOrEmpty(reg.path)) continue;
                        string hive = string.IsNullOrEmpty(reg.hive) ? "HKCU" : reg.hive;
                        string fullKey = hive + "\\" + reg.path;
                        if (!watchedKeys.Contains(fullKey))
                        {
                            watchedKeys.Add(fullKey);
                            try
                            {
                                var watcher = new RegistryWatcher(hive, reg.path, this.Dispatcher);
                                string targetKey = fullKey;
                                watcher.Changed += delegate
                                {
                                    for (int k = 0; k < this._viewModel.FilteredTweaks.Count; k++)
                                    {
                                        var c = this._viewModel.FilteredTweaks[k];
                                        if (c.Model != null && c.Model.registry != null)
                                        {
                                            for (int kr = 0; kr < c.Model.registry.Count; kr++)
                                            {
                                                var kreg = c.Model.registry[kr];
                                                string khive = string.IsNullOrEmpty(kreg.hive) ? "HKCU" : kreg.hive;
                                                if (string.Equals(khive + "\\" + kreg.path, targetKey, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    c.RefreshStatus();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                };
                                this._watchers.Add(watcher);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void UnsubscribeRegistryWatchers()
        {
            for (int i = 0; i < this._watchers.Count; i++)
            {
                try { this._watchers[i].Dispose(); } catch { }
            }
            this._watchers.Clear();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.UnsubscribeRegistryWatchers();
            if (this._viewModel != null)
            {
                this._viewModel.ThemeChanged -= this.OnThemeChanged;
                this._viewModel.Dispose();
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            this.ContentRenderedUtc = DateTime.UtcNow;
            long memRenderedWs = Process.GetCurrentProcess().WorkingSet64;
            long memRenderedPm = Process.GetCurrentProcess().PrivateMemorySize64;

            try
            {
                DateTime processStart = Process.GetCurrentProcess().StartTime.ToUniversalTime();
                string metricsPath = Path.Combine(Path.GetTempPath(), "tweaker_first_render.txt");
                string content = string.Format(
                    "ProcessStartTimeUtc={0:o}\nMainStartUtc={1:o}\nWindowCreatedUtc={2:o}\nContentRenderedUtc={3:o}\nPreMainMs={4}\nMainToRenderMs={5}\nTotalMs={6}\nWorkingSet64={7}\nPrivateMemorySize64={8}\n",
                    processStart,
                    App.MainStartUtc,
                    this.WindowCreatedUtc,
                    this.ContentRenderedUtc,
                    (App.MainStartUtc - processStart).TotalMilliseconds,
                    (this.ContentRenderedUtc - App.MainStartUtc).TotalMilliseconds,
                    (this.ContentRenderedUtc - processStart).TotalMilliseconds,
                    memRenderedWs,
                    memRenderedPm);
                File.WriteAllText(metricsPath, content);
            }
            catch { }

            string[] args = Environment.GetCommandLineArgs();
            bool isBenchmark = false;
            bool isIdleBenchmark = false;
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--benchmark") isBenchmark = true;
                    if (args[i] == "--idle-benchmark") isIdleBenchmark = true;
                }
            }

            if (isBenchmark)
            {
                Application.Current.Shutdown(0);
            }
            else if (isIdleBenchmark)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Tick += delegate
                {
                    timer.Stop();
                    try
                    {
                        Process p = Process.GetCurrentProcess();
                        string memPath = Path.Combine(Path.GetTempPath(), "tweaker_memory.txt");
                        string memContent = string.Format("WorkingSet64={0}\nPrivateMemorySize64={1}\n",
                            p.WorkingSet64, p.PrivateMemorySize64);
                        File.WriteAllText(memPath, memContent);
                    }
                    catch { }
                    Application.Current.Shutdown(0);
                };
                timer.Start();
            }
        }

        private void OnThemeChanged(bool isDark)
        {
            App app = Application.Current as App;
            if (app != null)
            {
                app.ApplyTheme(isDark);
            }
            this.ApplyWindowStyle(isDark);
        }

        private void ApplyWindowStyle(bool isDark)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.ApplyModernWindowStyles(hwnd, isDark, null, this._disableMica);
                NativeMethods.ApplyWindowFrameColors(hwnd, isDark);
            }
        }

        private void GroupExpander_Loaded(object sender, RoutedEventArgs e)
        {
            var expander = sender as System.Windows.Controls.Expander;
            if (expander == null) return;
            var group = expander.DataContext as System.Windows.Data.CollectionViewGroup;
            if (group != null && this._viewModel != null)
            {
                string groupName = group.Name as string;
                if (!string.IsNullOrEmpty(groupName))
                {
                    bool target = this._viewModel.IsGroupExpanded(groupName);
                    if (expander.IsExpanded != target)
                    {
                        expander.IsExpanded = target;
                    }
                }
            }
        }

        private void GroupExpander_Expanded(object sender, RoutedEventArgs e)
        {
            var expander = sender as System.Windows.Controls.Expander;
            if (expander == null) return;
            var group = expander.DataContext as System.Windows.Data.CollectionViewGroup;
            if (group != null && this._viewModel != null)
            {
                string groupName = group.Name as string;
                if (!string.IsNullOrEmpty(groupName))
                {
                    this._viewModel.SetGroupExpanded(groupName, true);
                }
            }
        }

        private void GroupExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            var expander = sender as System.Windows.Controls.Expander;
            if (expander == null) return;
            var group = expander.DataContext as System.Windows.Data.CollectionViewGroup;
            if (group != null && this._viewModel != null)
            {
                string groupName = group.Name as string;
                if (!string.IsNullOrEmpty(groupName))
                {
                    this._viewModel.SetGroupExpanded(groupName, false);
                }
            }
        }

        private int _lastHoverSampleIdx = -1;
        private string _lastHoverSeries = "";

        private void ChartPlotGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this._viewModel != null && e.NewSize.Width > 0)
            {
                this._viewModel.SetChartDimensions(e.NewSize.Width, e.NewSize.Height);
            }
        }

        private void ChartPlotGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this._viewModel == null || this.ChartPlotGrid == null || this.ChartHoverCanvas == null) return;
            Point pos = e.GetPosition(this.ChartPlotGrid);
            int sampleIdx;
            string seriesKey;
            double snapX, snapY;
            string readout;
            if (!this._viewModel.TrySampleChartAtX(pos.X, pos.Y, out sampleIdx, out seriesKey, out snapX, out snapY, out readout))
            {
                if (this.ChartHoverCanvas.Visibility == Visibility.Visible)
                {
                    this.ChartHoverCanvas.Visibility = Visibility.Collapsed;
                    this._lastHoverSampleIdx = -1;
                    this._lastHoverSeries = "";
                }
                return;
            }

            if (sampleIdx == this._lastHoverSampleIdx && seriesKey == this._lastHoverSeries && this.ChartHoverCanvas.Visibility == Visibility.Visible)
            {
                return;
            }
            this._lastHoverSampleIdx = sampleIdx;
            this._lastHoverSeries = seriesKey;

            string brushKey = seriesKey == "ram" ? "SuccessBrush" : (seriesKey == "gpu" ? "WarningBrush" : "AccentBrush");
            var seriesBrush = this.TryFindResource(brushKey) as System.Windows.Media.Brush;
            if (seriesBrush != null)
            {
                this.ChartHoverDot.Fill = seriesBrush;
                this.ChartHoverBadge.BorderBrush = seriesBrush;
            }

            this.ChartCrosshairLine.X1 = snapX;
            this.ChartCrosshairLine.X2 = snapX;

            System.Windows.Controls.Canvas.SetLeft(this.ChartHoverDot, snapX - 4.0);
            System.Windows.Controls.Canvas.SetTop(this.ChartHoverDot, snapY - 4.0);

            this.ChartHoverReadoutText.Text = readout;
            this.ChartHoverBadge.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double badgeW = this.ChartHoverBadge.DesiredSize.Width;
            double gridW = this.ChartPlotGrid.ActualWidth > 100 ? this.ChartPlotGrid.ActualWidth : 520.0;
            double badgeLeft = snapX + 8.0;
            if (badgeLeft + badgeW > gridW - 6.0)
            {
                badgeLeft = Math.Max(36.0, snapX - badgeW - 8.0);
            }
            double badgeTop = snapY - 26.0;
            if (badgeTop < 4.0) badgeTop = Math.Min(72.0, snapY + 8.0);
            System.Windows.Controls.Canvas.SetLeft(this.ChartHoverBadge, badgeLeft);
            System.Windows.Controls.Canvas.SetTop(this.ChartHoverBadge, badgeTop);
            this.ChartHoverCanvas.Visibility = Visibility.Visible;
        }

        private void ChartPlotGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this._lastHoverSampleIdx = -1;
            this._lastHoverSeries = "";
            if (this.ChartHoverCanvas != null)
            {
                this.ChartHoverCanvas.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchBorderContainer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.SearchBoxControl != null)
            {
                this.SearchBoxControl.Focus();
                System.Windows.Input.Keyboard.Focus(this.SearchBoxControl);
            }
        }
    }
}
