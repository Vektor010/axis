using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        // 1. Add properties before HasRecentJournalActions
        string target1 = "public bool HasRecentJournalActions";
        string props = @"// Game Mode & WNF Properties (Noverse gm_effects)
        public GameModeState GameModeState
        {
            get { return this._gameModeState; }
            set { this._gameModeState = value; this.OnPropertyChanged(""GameModeState""); }
        }

        public string GameModeStatusSummary
        {
            get { return this._gameModeState != null ? this._gameModeState.StatusSummary : ""Game Mode: Ожидание""; }
        }

        public string GameModeActiveGameDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.ActiveGameProcessName : ""Отсутствует""; }
        }

        public string GameModePowerProfileDisplay
        {
            get
            {
                if (this._gameModeState == null) return ""—"";
                if (this._gameModeState.PowerProfileLowValue == 3) return ""3 (Активен)"";
                if (this._gameModeState.PowerProfileLowValue == 1) return ""1 (Неактивен)"";
                return this._gameModeState.PowerProfileLowValue.ToString();
            }
        }

        public string GameModeCpuSetsDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.CpuSetIdsDisplay : ""—""; }
        }

        public string GameModeForegroundDisplay
        {
            get { return this._gameModeState != null ? this._gameModeState.ForegroundProcessName : ""—""; }
        }

        public bool IsGameModeRunning
        {
            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }
        }

        public bool HasRecentJournalActions";

        if (!content.Contains(props))
        {
            content = content.Replace(target1, props);
            Console.WriteLine("Properties injected.");
        }

        // 2. PollLiveMetrics
        string targetPoll = "SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);";
        string replacePoll = "SystemMetrics m = this._systemInfoService.CollectLiveMetrics(host);\r\n                GameModeState gm = this._gameModeWnfService.QueryState();";
        if (content.Contains(targetPoll) && !content.Contains("GameModeState gm = this._gameModeWnfService.QueryState();"))
        {
            content = content.Replace(targetPoll, replacePoll);
            Console.WriteLine("Poll query injected.");
        }

        string targetUi = "this.UpdateLiveMetricsUI(m);";
        string replaceUi = "this.UpdateLiveMetricsUI(m);\r\n                    this.UpdateGameModeUI(gm);";
        if (content.Contains(targetUi) && !content.Contains("this.UpdateGameModeUI(gm);"))
        {
            content = content.Replace(targetUi, replaceUi);
            Console.WriteLine("Poll UI update injected.");
        }

        // 3. UpdateGameModeUI method
        string targetMethod = "private PointCollection CreateSparklinePoints(List<double> history)";
        string methodCode = @"private void UpdateGameModeUI(GameModeState gm)
        {
            this._gameModeState = gm;
            this.OnPropertyChanged(""GameModeState"");
            this.OnPropertyChanged(""GameModeStatusSummary"");
            this.OnPropertyChanged(""GameModeActiveGameDisplay"");
            this.OnPropertyChanged(""GameModePowerProfileDisplay"");
            this.OnPropertyChanged(""GameModeCpuSetsDisplay"");
            this.OnPropertyChanged(""GameModeForegroundDisplay"");
            this.OnPropertyChanged(""IsGameModeRunning"");
        }

        private PointCollection CreateSparklinePoints(List<double> history)";

        if (!content.Contains("private void UpdateGameModeUI(GameModeState gm)"))
        {
            content = content.Replace(targetMethod, methodCode);
            Console.WriteLine("UpdateGameModeUI method injected.");
        }

        File.WriteAllText(path, content, Encoding.UTF8);
        Console.WriteLine("Done!");
    }
}
