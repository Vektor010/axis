$path = "C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs"
$content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)

# 1. Field _isScreenshotMode
$content = $content.Replace("private bool _isRealMode;", "private bool _isRealMode;`r`n        private readonly bool _isScreenshotMode;")

# 2. Properties SettingsManager and IsScreenshotMode
$tProp = "public bool IsAboutSelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == `"about`"; } }"
$rProp = @"
public bool IsAboutSelected { get { return this._selectedNavItem != null && this._selectedNavItem.Id == `"about`"; } }

        public SettingsManager SettingsManager
        {
            get { return this._settingsManager; }
        }

        public bool IsScreenshotMode
        {
            get { return this._isScreenshotMode; }
        }
"@
$content = $content.Replace($tProp, $rProp.Trim())

# 3. IsSidebarExpanded setter
$tSidebar = @"
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
"@
$rSidebar = @"
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
"@
$content = $content.Replace($tSidebar.Trim().Replace("`r`n", "`n"), $rSidebar.Trim().Replace("`r`n", "`n").Replace("`n", "`r`n"))
# Also try direct CRLF replace
$content = $content.Replace($tSidebar.Trim(), $rSidebar.Trim())

# 4. IsTweaksSubmenuExpanded setter
$tTweaks = @"
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
"@
$rTweaks = @"
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
"@
$content = $content.Replace($tTweaks.Trim().Replace("`r`n", "`n"), $rTweaks.Trim().Replace("`r`n", "`n").Replace("`n", "`r`n"))
$content = $content.Replace($tTweaks.Trim(), $rTweaks.Trim())

# 5. Constructor
$tCtor = "public MainViewModel(bool isRealMode = false, bool isDarkTheme = true)`r`n        {"
$rCtor = "public MainViewModel(bool isRealMode = false, bool isDarkTheme = true, bool isScreenshotMode = false)`r`n        {`r`n            this._isScreenshotMode = isScreenshotMode;"
$content = $content.Replace($tCtor, $rCtor)

# 6. InitializeServices
$tInit = @"
            this._settingsManager = new SettingsManager(null, isTestMode: !this._isRealMode);
            this._pingCustomHost = this._settingsManager.Settings != null ? this._settingsManager.Settings.PingCustomHost : "";
            this._isTweaksSubmenuExpanded = this._settingsManager.Settings != null ? this._settingsManager.Settings.IsTweaksSubmenuExpanded : true;
"@
$rInit = @"
            this._settingsManager = new SettingsManager(null, isTestMode: !this._isRealMode);
            if (!string.IsNullOrEmpty(this._settingsManager.LastError))
            {
                this.LogSettingsSaveError("при запуске приложения");
            }
            this._pingCustomHost = this._settingsManager.Settings != null ? this._settingsManager.Settings.PingCustomHost : "";
            this._isTweaksSubmenuExpanded = this._settingsManager.Settings != null ? this._settingsManager.Settings.IsTweaksSubmenuExpanded : true;
            this._isSidebarExpanded = this._settingsManager.Settings != null ? this._settingsManager.Settings.IsSidebarExpanded : true;
            this._sidebarWidth = this._isSidebarExpanded ? 200 : 64;
"@
$content = $content.Replace($tInit.Trim().Replace("`r`n", "`n"), $rInit.Trim().Replace("`r`n", "`n").Replace("`n", "`r`n"))
$content = $content.Replace($tInit.Trim(), $rInit.Trim())

# 7. OnToggleTheme
$tTheme = @"
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
"@
$rTheme = @"
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
"@
$content = $content.Replace($tTheme.Trim().Replace("`r`n", "`n"), $rTheme.Trim().Replace("`r`n", "`n").Replace("`n", "`r`n"))
$content = $content.Replace($tTheme.Trim(), $rTheme.Trim())

# 8. OnSaveSettings & SaveWindowPlacement & LogSettingsSaveError
$tSave = @"
        private void OnSaveSettings(object obj)
        {
            if (this._settingsManager != null && this._settingsManager.Settings != null)
            {
                this._settingsManager.Settings.PingCustomHost = this._pingCustomHost != null ? this._pingCustomHost.Trim() : "";
                this._settingsManager.Save();
                this.SettingsSaveNotification = "Настройки успешно сохранены.";
            }
        }
"@
$rSave = @"
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
"@
$content = $content.Replace($tSave.Trim().Replace("`r`n", "`n"), $rSave.Trim().Replace("`r`n", "`n").Replace("`n", "`r`n"))
$content = $content.Replace($tSave.Trim(), $rSave.Trim())

[System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
