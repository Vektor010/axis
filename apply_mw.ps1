$path = "C:\Users\User\Documents\win-tweaker\MainWindow.xaml.cs"
$norm = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8).Replace("`r`n", "`n")

# 1. Add using System.ComponentModel;
$norm = $norm.Replace("using System;`n", "using System;`nusing System.Collections.Generic;`nusing System.ComponentModel;`n")

# 2. Add private readonly bool _isScreenshotMode;
$norm = $norm.Replace("private readonly MainViewModel _viewModel;", "private readonly MainViewModel _viewModel;`n        private readonly bool _isScreenshotMode;")

# 3. Constructors
$oldCtors = @"
        public MainWindow() : this(false, true)
        {
        }

        public MainWindow(bool isRealMode, bool isDarkTheme)
        {
            InitializeComponent();
            this._viewModel = new MainViewModel(isRealMode, isDarkTheme);
"@.Replace("`r`n", "`n").Trim()

$newCtors = @"
        public MainWindow() : this(false, true, false)
        {
        }

        public MainWindow(bool isRealMode, bool isDarkTheme) : this(isRealMode, isDarkTheme, false)
        {
        }

        public MainWindow(bool isRealMode, bool isDarkTheme, bool isScreenshotMode)
        {
            InitializeComponent();
            this._isScreenshotMode = isScreenshotMode;
            this._viewModel = new MainViewModel(isRealMode, isDarkTheme, isScreenshotMode);
"@.Replace("`r`n", "`n").Trim()

if ($norm.Contains($oldCtors)) {
    $norm = $norm.Replace($oldCtors, $newCtors)
    Write-Output "Replaced constructors"
} else {
    Write-Error "Constructors not found"
}

# 4. Replace sizing block with ApplyInitialWindowPlacement() and register Closing
$oldSizing = @"
            Rect workArea = SystemParameters.WorkArea;
            if (workArea.Width > 0 && workArea.Height > 0)
            {
                if (this.Width > workArea.Width)
                {
                    this.Width = Math.Max(this.MinWidth, workArea.Width);
                }
                if (this.Height > workArea.Height)
                {
                    this.Height = Math.Max(this.MinHeight, workArea.Height);
                }
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = workArea.Left + Math.Max(0.0, (workArea.Width - this.Width) / 2.0);
                this.Top = workArea.Top + Math.Max(0.0, (workArea.Height - this.Height) / 2.0);
            }
        }
"@.Replace("`r`n", "`n").Trim()

$newSizing = @"
            this.Closing += this.OnWindowClosing;
            this.ApplyInitialWindowPlacement();
        }

        private void ApplyInitialWindowPlacement()
        {
            UserSettings s = (this._viewModel != null && this._viewModel.SettingsManager != null)
                ? this._viewModel.SettingsManager.Settings
                : null;

            Rect primaryWorkArea = WindowPlacementHelper.GetPrimaryWorkArea();
            if (primaryWorkArea.Width <= 0 || primaryWorkArea.Height <= 0)
            {
                primaryWorkArea = new Rect(0, 0, 1920, 1080);
            }

            double minW = this.MinWidth > 0 ? this.MinWidth : 900.0;
            double minH = this.MinHeight > 0 ? this.MinHeight : 600.0;

            double targetW = (s != null && s.WindowWidth > 0) ? s.WindowWidth : 1120.0;
            double targetH = (s != null && s.WindowHeight > 0) ? s.WindowHeight : 780.0;
            bool hasSavedPos = (s != null && s.WindowLeft.HasValue && s.WindowTop.HasValue);
            bool isMaximized = (s != null && s.IsWindowMaximized);

            if (hasSavedPos)
            {
                double targetLeft = s.WindowLeft.Value;
                double targetTop = s.WindowTop.Value;

                List<Rect> screenWorkAreas = WindowPlacementHelper.GetScreenWorkAreas();
                Rect candidateRect = new Rect(targetLeft, targetTop, targetW, targetH);

                bool isOutside = WindowPlacementHelper.IsCompletelyOutsideScreens(candidateRect, screenWorkAreas);
                if (isOutside)
                {
                    // Сохранённое положение целиком вне всех экранов (например, отключили монитор):
                    // центрировать на основном экране с ограничением рабочей областью и минимумами 900x600
                    targetW = Math.Max(minW, Math.Min(targetW, primaryWorkArea.Width));
                    targetH = Math.Max(minH, Math.Min(targetH, primaryWorkArea.Height));

                    this.Width = targetW;
                    this.Height = targetH;
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    this.Left = primaryWorkArea.Left + Math.Max(0.0, (primaryWorkArea.Width - targetW) / 2.0);
                    this.Top = primaryWorkArea.Top + Math.Max(0.0, (primaryWorkArea.Height - targetH) / 2.0);
                }
                else
                {
                    // Окно попадает на экран: ограничиваем размер рабочей областью экрана и минимумами
                    Rect targetScreen = WindowPlacementHelper.FindBestScreen(candidateRect, screenWorkAreas);
                    targetW = Math.Max(minW, Math.Min(targetW, targetScreen.Width));
                    targetH = Math.Max(minH, Math.Min(targetH, targetScreen.Height));

                    this.Width = targetW;
                    this.Height = targetH;
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    this.Left = targetLeft;
                    this.Top = targetTop;
                }
            }
            else
            {
                // Положение не сохранено: 1120x780 по центру на основном экране
                targetW = Math.Max(minW, Math.Min(targetW, primaryWorkArea.Width));
                targetH = Math.Max(minH, Math.Min(targetH, primaryWorkArea.Height));

                this.Width = targetW;
                this.Height = targetH;
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = primaryWorkArea.Left + Math.Max(0.0, (primaryWorkArea.Width - targetW) / 2.0);
                this.Top = primaryWorkArea.Top + Math.Max(0.0, (primaryWorkArea.Height - targetH) / 2.0);
            }

            if (isMaximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (this._isScreenshotMode)
            {
                return;
            }

            try
            {
                bool isMaximized = (this.WindowState == WindowState.Maximized);
                double w, h, l, t;

                if (isMaximized || this.WindowState == WindowState.Minimized)
                {
                    Rect rb = this.RestoreBounds;
                    w = rb.Width;
                    h = rb.Height;
                    l = rb.Left;
                    t = rb.Top;
                }
                else
                {
                    w = this.ActualWidth > 0 ? this.ActualWidth : this.Width;
                    h = this.ActualHeight > 0 ? this.ActualHeight : this.Height;
                    l = this.Left;
                    t = this.Top;
                }

                if (w > 0 && h > 0 && this._viewModel != null)
                {
                    this._viewModel.SaveWindowPlacement(l, t, w, h, isMaximized);
                }
            }
            catch
            {
            }
        }
"@.Replace("`r`n", "`n").Trim()

if ($norm.Contains($oldSizing)) {
    $norm = $norm.Replace($oldSizing, $newSizing)
    Write-Output "Replaced sizing and added closing"
} else {
    Write-Error "Sizing block not found"
}

$final = $norm.Replace("`n", "`r`n")
[System.IO.File]::WriteAllText($path, $final, [System.Text.Encoding]::UTF8)
