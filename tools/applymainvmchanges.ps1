$path = 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
$lines = [System.Collections.Generic.List[string]](Get-Content $path)

# 1. Find line with HasRecentJournalActions (around 840)
$propIndex = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i].Trim() -eq 'public bool HasRecentJournalActions') {
        $propIndex = $i
        break
    }
}

$propCode = @(
    "        // Game Mode & WNF Properties (Noverse gm_effects)",
    "        public GameModeState GameModeState",
    "        {",
    "            get { return this._gameModeState; }",
    "            set { this._gameModeState = value; this.OnPropertyChanged(""GameModeState""); }",
    "        }",
    "",
    "        public string GameModeStatusSummary",
    "        {",
    "            get { return this._gameModeState != null ? this._gameModeState.StatusSummary : ""Game Mode: Ожидание""; }",
    "        }",
    "",
    "        public string GameModeActiveGameDisplay",
    "        {",
    "            get { return this._gameModeState != null ? this._gameModeState.ActiveGameProcessName : ""Отсутствует""; }",
    "        }",
    "",
    "        public string GameModePowerProfileDisplay",
    "        {",
    "            get",
    "            {",
    "                if (this._gameModeState == null) return ""—"";",
    "                if (this._gameModeState.PowerProfileLowValue == 3) return ""3 (Активен)"";",
    "                if (this._gameModeState.PowerProfileLowValue == 1) return ""1 (Неактивен)"";",
    "                return this._gameModeState.PowerProfileLowValue.ToString();",
    "            }",
    "        }",
    "",
    "        public string GameModeCpuSetsDisplay",
    "        {",
    "            get { return this._gameModeState != null ? this._gameModeState.CpuSetIdsDisplay : ""—""; }",
    "        }",
    "",
    "        public string GameModeForegroundDisplay",
    "        {",
    "            get { return this._gameModeState != null ? this._gameModeState.ForegroundProcessName : ""—""; }",
    "        }",
    "",
    "        public bool IsGameModeRunning",
    "        {",
    "            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }",
    "        }",
    ""
)

if ($propIndex -gt 0) {
    $lines.InsertRange($propIndex, $propCode)
    Write-Host "Properties inserted at $propIndex"
} else {
    Write-Host "HasRecentJournalActions not found" -ForegroundColor Red
}

# 2. Update PollLiveMetrics to call gm query
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i].Contains("SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);")) {
        $lines.Insert($i + 1, "                GameModeState gm = this._gameModeWnfService.QueryState();")
        Write-Host "gm query inserted at $($i+1)"
        break
    }
}

for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i].Contains("this.UpdateLiveMetricsUI(m);")) {
        $lines.Insert($i + 1, "                    this.UpdateGameModeUI(gm);")
        Write-Host "UpdateGameModeUI call inserted at $($i+1)"
        break
    }
}

# 3. Add UpdateGameModeUI method right after CreateSparklinePoints
$sparkIndex = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i].Trim() -eq 'private PointCollection CreateSparklinePoints(List<double> history)') {
        $sparkIndex = $i
        break
    }
}

$methodCode = @(
    "        private void UpdateGameModeUI(GameModeState gm)",
    "        {",
    "            this._gameModeState = gm;",
    "            this.OnPropertyChanged(""GameModeState"");",
    "            this.OnPropertyChanged(""GameModeStatusSummary"");",
    "            this.OnPropertyChanged(""GameModeActiveGameDisplay"");",
    "            this.OnPropertyChanged(""GameModePowerProfileDisplay"");",
    "            this.OnPropertyChanged(""GameModeCpuSetsDisplay"");",
    "            this.OnPropertyChanged(""GameModeForegroundDisplay"");",
    "            this.OnPropertyChanged(""IsGameModeRunning"");",
    "        }",
    ""
)

if ($sparkIndex -gt 0) {
    $lines.InsertRange($sparkIndex, $methodCode)
    Write-Host "UpdateGameModeUI method inserted at $sparkIndex"
} else {
    Write-Host "CreateSparklinePoints not found" -ForegroundColor Red
}

Set-Content -Path $path -Value $lines -Encoding UTF8
Write-Host "MainViewModel.cs completely saved!" -ForegroundColor Green
