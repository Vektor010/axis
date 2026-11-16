$path = "C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs"
$raw = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
$norm = $raw.Replace("`r`n", "`n")

# 1. IsSidebarExpanded
$t1 = @"
        public bool IsSidebarExpanded
        {
            get { return this._isSidebarExpanded; }
            set
            {
                this._isSidebarExpanded = value;
                this.SidebarWidth = value ? 200 : 64;
                this.OnPropertyChanged("IsSidebarExpanded");
                this.OnPropertyChanged("IsTweaksSubmenuVisible");
            }
        }
"@.Replace("`r`n", "`n").Trim()

$r1 = @"
        public bool IsSidebarExpanded
        {
            get { return this._isSidebarExpanded; }
            set
            {
                if (this._isSidebarExpanded != value)
                {
                    this._isSidebarExpanded = value;
                    this.SidebarWidth = value ? 200 : 64;
                    if (!this._isScreenshotMode && this._settingsManager != null && this._settingsManager.Settings != null)
                    {
                        this._settingsManager.Settings.IsSidebarExpanded = value;
                        this._settingsManager.Save();
                    }
                    this.OnPropertyChanged("IsSidebarExpanded");
                    this.OnPropertyChanged("IsTweaksSubmenuVisible");
                }
            }
        }
"@.Replace("`r`n", "`n").Trim()

if ($norm.Contains($t1)) {
    $norm = $norm.Replace($t1, $r1)
    Write-Output "Matched t1"
} else {
    Write-Error "t1 not found"
}

# 2. IsTweaksSubmenuExpanded
$t2 = @"
        public bool IsTweaksSubmenuExpanded
        {
            get { return this._isTweaksSubmenuExpanded; }
            set
            {
                if (this._isTweaksSubmenuExpanded != value)
                {
                    this._isTweaksSubmenuExpanded = value;
                    if (this._settingsManager != null && this._settingsManager.Settings != null)
                    {
                        this._settingsManager.Settings.IsTweaksSubmenuExpanded = value;
                        this._settingsManager.Save();
                    }
                    this.OnPropertyChanged("IsTweaksSubmenuExpanded");
                    this.OnPropertyChanged("IsTweaksSubmenuVisible");
                }
            }
        }
"@.Replace("`r`n", "`n").Trim()

$r2 = @"
        public bool IsTweaksSubmenuExpanded
        {
            get { return this._isTweaksSubmenuExpanded; }
            set
            {
                if (this._isTweaksSubmenuExpanded != value)
                {
                    this._isTweaksSubmenuExpanded = value;
                    if (!this._isScreenshotMode && this._settingsManager != null && this._settingsManager.Settings != null)
                    {
                        this._settingsManager.Settings.IsTweaksSubmenuExpanded = value;
                        this._settingsManager.Save();
                    }
                    this.OnPropertyChanged("IsTweaksSubmenuExpanded");
                    this.OnPropertyChanged("IsTweaksSubmenuVisible");
                }
            }
        }
"@.Replace("`r`n", "`n").Trim()

if ($norm.Contains($t2)) {
    $norm = $norm.Replace($t2, $r2)
    Write-Output "Matched t2"
} else {
    Write-Error "t2 not found"
}

# 3. InitializeServices
$t3 = @"
        private void InitializeServices()
        {
            this._settingsManager = new SettingsManager(null, isTestMode: !this._isRealMode);
            this._pingCustomHost = this._settingsManager.Settings != null ? this._settingsManager.Settings.PingCustomHost : "";
            this._isTweaksSubmenuExpanded = this._settingsManager.Settings != null ? this._settingsManager.Settings.IsTweaksSubmenuExpanded : true;
"@.Replace("`r`n", "`n").Trim()

$r3 = @"
        private void InitializeServices()
        {
            this._settingsManager = new SettingsManager(null, isTestMode: !this._isRealMode);
            if (!string.IsNullOrEmpty(this._settingsManager.LastError))
            {
                this.LogSettingsSaveError("при запуске приложения");
            }
            this._pingCustomHost = this._settingsManager.Settings != null ? this._settingsManager.Settings.PingCustomHost : "";
            this._isTweaksSubmenuExpanded = this._settingsManager.Settings != null ? this._settingsManager.Settings.IsTweaksSubmenuExpanded : true;
            this._isSidebarExpanded = this._settingsManager.Settings != null ? this._settingsManager.Settings.IsSidebarExpanded : true;
            this._sidebarWidth = this._isSidebarExpanded ? 200 : 64;
"@.Replace("`r`n", "`n").Trim()

if ($norm.Contains($t3)) {
    $norm = $norm.Replace($t3, $r3)
    Write-Output "Matched t3"
} else {
    Write-Error "t3 not found"
}

# 4. OnToggleTheme
$t4 = @"
        private void OnToggleTheme(object obj)
        {
            this._isDarkTheme = !this._isDarkTheme;
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.IsDarkTheme = this._isDarkTheme;
                this._settingsManager.Save();
            }
            if (this.ThemeChanged != null)
            {
                this.ThemeChanged(this._isDarkTheme);
            }
        }
"@.Replace("`r`n", "`n").Trim()

$r4 = @"
        private void OnToggleTheme(object obj)
        {
            this._isDarkTheme = !this._isDarkTheme;
            if (!this._isScreenshotMode && this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.IsDarkTheme = this._isDarkTheme;
                this._settingsManager.Save();
            }
            if (this.ThemeChanged != null)
            {
                this.ThemeChanged(this._isDarkTheme);
            }
        }
"@.Replace("`r`n", "`n").Trim()

if ($norm.Contains($t4)) {
    $norm = $norm.Replace($t4, $r4)
    Write-Output "Matched t4"
} else {
    Write-Error "t4 not found"
}

# 5. OnSaveSettings & SaveWindowPlacement & LogSettingsSaveError
$t5 = @"
        private void OnSaveSettings(object obj)
        {
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.PingCustomHost = this._pingCustomHost != null ? this._pingCustomHost.Trim() : "";
                this._settingsManager.Save();
                this.SettingsSaveNotification = "Настройки успешно сохранены.";
            }
        }
"@.Replace("`r`n", "`n").Trim()

$r5 = @"
        private void OnSaveSettings(object obj)
        {
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.PingCustomHost = this._pingCustomHost != null ? this._pingCustomHost.Trim() : "";
                bool ok = this._settingsManager.Save();
                if (ok)
                {
                    this.SettingsSaveNotification = "Настройки успешно сохранены.";
                }
                else
                {
                    this.SettingsSaveNotification = string.Format("Не удалось сохранить настройки: {0}", this._settingsManager.LastError);
                }
            }
        }

        public bool SaveWindowPlacement(double left, double top, double width, double height, bool isMaximized)
        {
            if (this._isScreenshotMode)
            {
                return true;
            }

            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.WindowLeft = left;
                this._settingsManager.Settings.WindowTop = top;
                this._settingsManager.Settings.WindowWidth = width;
                this._settingsManager.Settings.WindowHeight = height;
                this._settingsManager.Settings.IsWindowMaximized = isMaximized;
                bool ok = this._settingsManager.Save();
                if (!ok)
                {
                    this.LogSettingsSaveError("при закрытии окна");
                }
                return ok;
            }
            return false;
        }

        private void LogSettingsSaveError(string context)
        {
            string err = (this._settingsManager != null && !string.IsNullOrEmpty(this._settingsManager.LastError))
                ? this._settingsManager.LastError
                : "Неизвестная ошибка";
            string msg = string.Format("Не удалось сохранить настройки ({0}): {1}", context, err);

            if (this._journalManager != null)
            {
                try
                {
                    this._journalManager.Log("ERROR", "", msg);
                    this.RefreshJournal();
                    this.UpdateRecentJournal();
                }
                catch
                {
                }
            }
            this.StatusNotification = msg;
        }
"@.Replace("`r`n", "`n").Trim()

if ($norm.Contains($t5)) {
    $norm = $norm.Replace($t5, $r5)
    Write-Output "Matched t5"
} else {
    Write-Error "t5 not found"
}

$final = $norm.Replace("`n", "`r`n")
[System.IO.File]::WriteAllText($path, $final, [System.Text.Encoding]::UTF8)
