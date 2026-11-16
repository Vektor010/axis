$path = 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
$content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)

# 1. Add properties after JournalCountDisplay
$propInsert = @"
        public string JournalCountDisplay
        {
            get { return string.Format("{0} записей", this._journalEntries.Count); }
        }

        // Game Mode & WNF Properties (Noverse gm_effects)
        public GameModeState GameModeState
        {
            get { return this._gameModeState; }
            set { this._gameModeState = value; this.OnPropertyChanged("GameModeState"); }
        }

        public string GameModeStatusSummary
        {
            get { return this._gameModeState != null ? this._gameModeState.StatusSummary : "Game Mode: Ожидание"; }
        }

        public string GameModeActiveGameDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.ActiveGameProcessName : "Отсутствует"; }
        }

        public string GameModePowerProfileDisplay
        {
            get
            {
                if (this._gameModeState == null) return "—";
                if (this._gameModeState.PowerProfileLowValue == 3) return "3 (Активен)";
                if (this._gameModeState.PowerProfileLowValue == 1) return "1 (Неактивен)";
                return this._gameModeState.PowerProfileLowValue.ToString();
            }
        }

        public string GameModeCpuSetsDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.CpuSetIdsDisplay : "—"; }
        }

        public string GameModeForegroundDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.ForegroundProcessName : "—"; }
        }

        public bool IsGameModeRunning
        {
            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }
        }
"@

$targetProp = @"
        public string JournalCountDisplay
        {
            get { return string.Format("{0} записей", this._journalEntries.Count); }
        }
"@

if ($content.Contains($targetProp)) {
    $content = $content.Replace($targetProp, $propInsert)
    Write-Host "Properties injected successfully."
} else {
    Write-Host "TargetProp not found!" -ForegroundColor Red
}

# 2. Update PollLiveMetrics to query GameMode
$pollTarget = @"
                SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.UpdateLiveMetricsUI(m);
                }));
"@

$pollReplace = @"
                SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);
                GameModeState gm = this._gameModeWnfService.QueryState();
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.UpdateLiveMetricsUI(m);
                    this.UpdateGameModeUI(gm);
                }));
"@

if ($content.Contains($pollTarget)) {
    $content = $content.Replace($pollTarget, $pollReplace)
    Write-Host "PollLiveMetrics updated successfully."
} else {
    Write-Host "PollTarget not found!" -ForegroundColor Red
}

# 3. Add UpdateGameModeUI method right after UpdateLiveMetricsUI
$methodTarget = @"
            this.UpdateSummary();
            this.UpdateRecentJournal();
        }
"@

$methodReplace = @"
            this.UpdateSummary();
            this.UpdateRecentJournal();
        }

        private void UpdateGameModeUI(GameModeState gm)
        {
            this._gameModeState = gm;
            this.OnPropertyChanged("GameModeState");
            this.OnPropertyChanged("GameModeStatusSummary");
            this.OnPropertyChanged("GameModeActiveGameDisplay");
            this.OnPropertyChanged("GameModePowerProfileDisplay");
            this.OnPropertyChanged("GameModeCpuSetsDisplay");
            this.OnPropertyChanged("GameModeForegroundDisplay");
            this.OnPropertyChanged("IsGameModeRunning");
        }
"@

if ($content.Contains($methodTarget)) {
    $content = $content.Replace($methodTarget, $methodReplace)
    Write-Host "UpdateGameModeUI method injected successfully."
} else {
    Write-Host "MethodTarget not found!" -ForegroundColor Red
}

[System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
Write-Host "MainViewModel.cs updated!"
