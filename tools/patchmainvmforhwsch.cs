using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        // 1. Add fields
        string targetFields = "private GameModeState _gameModeState;\r\n";
        string newFields = "private GameModeState _gameModeState;\r\n        private GpuHwschService _gpuHwschService;\r\n        private GpuHwschState _gpuHwschState;\r\n";
        if (!content.Contains(targetFields))
        {
            targetFields = "private GameModeState _gameModeState;\n";
            newFields = "private GameModeState _gameModeState;\n        private GpuHwschService _gpuHwschService;\n        private GpuHwschState _gpuHwschState;\n";
        }

        // 2. Add constructor init
        string targetCtor = "this._gameModeState = new GameModeState();\r\n";
        string newCtor = "this._gameModeState = new GameModeState();\r\n            this._gpuHwschService = new GpuHwschService();\r\n            this._gpuHwschState = this._gpuHwschService.QueryPrimaryGpuState();\r\n";
        if (!content.Contains(targetCtor))
        {
            targetCtor = "this._gameModeState = new GameModeState();\n";
            newCtor = "this._gameModeState = new GameModeState();\n            this._gpuHwschService = new GpuHwschService();\n            this._gpuHwschState = this._gpuHwschService.QueryPrimaryGpuState();\n";
        }

        // 3. Add properties
        string targetProps = "public bool IsGameModeRunning\r\n        {\r\n            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }\r\n        }\r\n";
        string newProps = "public bool IsGameModeRunning\r\n        {\r\n            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }\r\n        }\r\n\r\n        public GpuHwschState GpuHwschState\r\n        {\r\n            get { return this._gpuHwschState; }\r\n        }\r\n\r\n        public string GpuHagsDisplay\r\n        {\r\n            get { return this._gpuHwschState != null ? this._gpuHwschState.HagsStatusDisplay : \"Не поддерживается\"; }\r\n        }\r\n\r\n        public string GpuFlipQueueDisplay\r\n        {\r\n            get { return this._gpuHwschState != null ? this._gpuHwschState.FlipQueueStatusDisplay : \"Не поддерживается\"; }\r\n        }\r\n\r\n        public bool IsHagsActive\r\n        {\r\n            get { return this._gpuHwschState != null && this._gpuHwschState.IsHagsEnabled; }\r\n        }\r\n\r\n        public bool IsFlipQueueActive\r\n        {\r\n            get { return this._gpuHwschState != null && this._gpuHwschState.IsFlipQueueEnabled; }\r\n        }\r\n\r\n        public string GpuHwschSummary\r\n        {\r\n            get { return this._gpuHwschState != null ? this._gpuHwschState.SummaryDisplay : \"Нет данных\"; }\r\n        }\r\n";
        if (!content.Contains(targetProps))
        {
            targetProps = "public bool IsGameModeRunning\n        {\n            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }\n        }\n";
            newProps = "public bool IsGameModeRunning\n        {\n            get { return this._gameModeState != null && this._gameModeState.IsGameRunning; }\n        }\n\n        public GpuHwschState GpuHwschState\n        {\n            get { return this._gpuHwschState; }\n        }\n\n        public string GpuHagsDisplay\n        {\n            get { return this._gpuHwschState != null ? this._gpuHwschState.HagsStatusDisplay : \"Не поддерживается\"; }\n        }\n\n        public string GpuFlipQueueDisplay\n        {\n            get { return this._gpuHwschState != null ? this._gpuHwschState.FlipQueueStatusDisplay : \"Не поддерживается\"; }\n        }\n\n        public bool IsHagsActive\n        {\n            get { return this._gpuHwschState != null && this._gpuHwschState.IsHagsEnabled; }\n        }\n\n        public bool IsFlipQueueActive\n        {\n            get { return this._gpuHwschState != null && this._gpuHwschState.IsFlipQueueEnabled; }\n        }\n\n        public string GpuHwschSummary\n        {\n            get { return this._gpuHwschState != null ? this._gpuHwschState.SummaryDisplay : \"Нет данных\"; }\n        }\n";
        }

        if (content.Contains(targetFields) && !content.Contains("_gpuHwschService"))
        {
            content = content.Replace(targetFields, newFields);
            content = content.Replace(targetCtor, newCtor);
            content = content.Replace(targetProps, newProps);

            File.WriteAllText(path, content, Encoding.UTF8);
            Console.WriteLine("MainViewModel.cs patched with GpuHwsch properties successfully!");
        }
        else
        {
            Console.WriteLine("Target not found or already patched.");
        }
    }
}
