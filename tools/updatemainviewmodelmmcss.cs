using System;
using System.IO;

class Program
{
    static int Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs";
        string content = File.ReadAllText(path);

        // 1. Add fields
        string target1 = "        private GpuHwschState _gpuHwschState;";
        string replace1 = "        private GpuHwschState _gpuHwschState;\r\n        private MmcssTaskService _mmcssTaskService;\r\n        private MmcssTaskState _mmcssTaskState;";
        if (!content.Contains(target1))
        {
            Console.WriteLine("Error: target1 not found");
            return 1;
        }
        content = content.Replace(target1, replace1);

        // 2. Add constructor init
        string target2 = "this._gpuHwschState = this._gpuHwschService.QueryPrimaryGpuState();";
        string replace2 = "this._gpuHwschState = this._gpuHwschService.QueryPrimaryGpuState();\r\n            this._mmcssTaskService = new MmcssTaskService();\r\n            this._mmcssTaskState = this._mmcssTaskService.QueryState();";
        if (!content.Contains(target2))
        {
            Console.WriteLine("Error: target2 not found");
            return 2;
        }
        content = content.Replace(target2, replace2);

        // 3. Add PollLiveMetrics call
        string target3 = "                GameModeState gm = this._gameModeWnfService.QueryState();\r\n                Application.Current.Dispatcher.BeginInvoke(new Action(() =>\r\n                {\r\n                    this.UpdateLiveMetricsUI(m);\r\n                    this.UpdateGameModeUI(gm);";
        string replace3 = "                GameModeState gm = this._gameModeWnfService.QueryState();\r\n                MmcssTaskState mm = this._mmcssTaskService.QueryState();\r\n                Application.Current.Dispatcher.BeginInvoke(new Action(() =>\r\n                {\r\n                    this.UpdateLiveMetricsUI(m);\r\n                    this.UpdateGameModeUI(gm);\r\n                    this.UpdateMmcssUI(mm);";
        if (!content.Contains(target3))
        {
            // Try with \n only
            target3 = target3.Replace("\r\n", "\n");
            replace3 = replace3.Replace("\r\n", "\n");
            if (!content.Contains(target3))
            {
                Console.WriteLine("Error: target3 not found");
                return 3;
            }
        }
        content = content.Replace(target3, replace3);

        // 4. Add UpdateMmcssUI method after UpdateGameModeUI
        string target4 = "        private void UpdateGameModeUI(GameModeState gm)\r\n        {\r\n            this._gameModeState = gm;\r\n            this.OnPropertyChanged(\"GameModeState\");\r\n            this.OnPropertyChanged(\"GameModeStatusSummary\");\r\n            this.OnPropertyChanged(\"GameModeActiveGameDisplay\");\r\n            this.OnPropertyChanged(\"GameModePowerProfileDisplay\");\r\n            this.OnPropertyChanged(\"GameModeCpuSetsDisplay\");\r\n            this.OnPropertyChanged(\"GameModeForegroundDisplay\");\r\n            this.OnPropertyChanged(\"IsGameModeRunning\");\r\n        }";
        string replace4 = target4 + "\r\n\r\n        private void UpdateMmcssUI(MmcssTaskState state)\r\n        {\r\n            this._mmcssTaskState = state;\r\n            this.OnPropertyChanged(\"MmcssTaskState\");\r\n            this.OnPropertyChanged(\"MmcssSystemResponsivenessDisplay\");\r\n            this.OnPropertyChanged(\"MmcssNetworkThrottlingDisplay\");\r\n            this.OnPropertyChanged(\"MmcssActiveTasksDisplay\");\r\n            this.OnPropertyChanged(\"MmcssStatusSummary\");\r\n            this.OnPropertyChanged(\"IsMmcssOptimized\");\r\n        }";
        if (!content.Contains(target4))
        {
            target4 = target4.Replace("\r\n", "\n");
            replace4 = replace4.Replace("\r\n", "\n");
            if (!content.Contains(target4))
            {
                Console.WriteLine("Error: target4 not found");
                return 4;
            }
        }
        content = content.Replace(target4, replace4);

        // 5. Add MMCSS properties after GpuHwschSummary
        string target5 = "        public string GpuHwschSummary\r\n        {\r\n            get { return this._gpuHwschState != null ? this._gpuHwschState.SummaryDisplay : \"Нет данных\"; }\r\n        }";
        string mmcssProps = @"

        // MMCSS Task Properties (Noverse mmcss_task.c)
        public MmcssTaskState MmcssTaskState
        {
            get { return this._mmcssTaskState; }
        }

        public string MmcssSystemResponsivenessDisplay
        {
            get { return this._mmcssTaskState != null ? this._mmcssTaskState.SystemResponsivenessDisplay : ""—""; }
        }

        public string MmcssNetworkThrottlingDisplay
        {
            get { return this._mmcssTaskState != null ? this._mmcssTaskState.NetworkThrottlingDisplay : ""—""; }
        }

        public string MmcssActiveTasksDisplay
        {
            get
            {
                if (this._mmcssTaskState == null) return ""—"";
                return string.Format(""{0} ({1} задач)"", this._mmcssTaskState.IsAvrtAvailable ? ""Доступно (avrt.dll)"" : ""Недоступно"", this._mmcssTaskState.AvailableTasksCount);
            }
        }

        public string MmcssStatusSummary
        {
            get { return this._mmcssTaskState != null ? this._mmcssTaskState.StatusSummary : ""MMCSS: Ожидание""; }
        }

        public bool IsMmcssOptimized
        {
            get { return this._mmcssTaskState != null && this._mmcssTaskState.IsMmcssOptimized; }
        }";
        string replace5 = target5 + mmcssProps;
        if (!content.Contains(target5))
        {
            target5 = target5.Replace("\r\n", "\n");
            replace5 = replace5.Replace("\r\n", "\n");
            if (!content.Contains(target5))
            {
                Console.WriteLine("Error: target5 not found");
                return 5;
            }
        }
        content = content.Replace(target5, replace5);

        File.WriteAllText(path, content);
        Console.WriteLine("Successfully updated MainViewModel.cs with MMCSS support!");
        return 0;
    }
}
