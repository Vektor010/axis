$path = 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
$content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
$content = $content.Replace("`r`n", "`n")

# 1. Properties
$target1 = "public string JournalCountDisplay`n        {`n            get { return string.Format(""{0} записей"", this._journalEntries.Count); }`n        }"
$idx1 = $content.IndexOf($target1)
Write-Host "Index 1: $idx1"

# 2. PollLiveMetrics
$target2 = "SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);`n                Application.Current.Dispatcher.BeginInvoke(new Action(() =>`n                {`n                    this.UpdateLiveMetricsUI(m);`n                }));"
$idx2 = $content.IndexOf($target2)
Write-Host "Index 2: $idx2"

# 3. Method
$target3 = "this.UpdateSummary();`n            this.UpdateRecentJournal();`n        }"
$idx3 = $content.IndexOf($target3)
Write-Host "Index 3: $idx3"

if ($idx1 -ge 0 -and $idx2 -ge 0 -and $idx3 -ge 0) {
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
"@.Replace("`r`n", "`n")

    $content = $content.Replace($target1, $propInsert)

    $pollInsert = @"
SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);
                GameModeState gm = this._gameModeWnfService.QueryState();
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.UpdateLiveMetricsUI(m);
                    this.UpdateGameModeUI(gm);
                }));
"@.Replace("`r`n", "`n")

    $content = $content.Replace($target2, $pollInsert)

    $methodInsert = @"
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
"@.Replace("`r`n", "`n")

    $content = $content.Replace($target3, $methodInsert)

    # restore CRLF
    $content = $content.Replace("`n", "`r`n")
    [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
    Write-Host "MainViewModel.cs successfully updated with CRLF!" -ForegroundColor Green
} else {
    Write-Host "Some targets not found" -ForegroundColor Red
}
