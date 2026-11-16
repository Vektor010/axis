using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace WinTweaker.Engine
{
    public class UserSettings
    {
        public bool IsDarkTheme { get; set; }
        public bool RealModePromptShown { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public string PingCustomHost { get; set; }
        public string TextRenderingMode { get; set; }
        public bool IsGrouped { get; set; }
        public int SortMode { get; set; }
        public Dictionary<string, bool> GroupExpandedStates { get; set; }

        public UserSettings()
        {
            this.IsDarkTheme = true;
            this.RealModePromptShown = false;
            this.WindowWidth = 1060;
            this.WindowHeight = 700;
            this.PingCustomHost = "";
            this.TextRenderingMode = "ClearType";
            this.IsGrouped = true;
            this.SortMode = 0;
            this.GroupExpandedStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            this.GroupExpandedStates["Основное"] = true;
        }
    }

    public class SettingsManager
    {
        private readonly string _filePath;
        private readonly bool _isTestMode;
        private UserSettings _settings;
        private readonly JavaScriptSerializer _serializer;

        public SettingsManager(string customPath = null, bool isTestMode = false)
        {
            this._isTestMode = isTestMode;
            this._serializer = new JavaScriptSerializer();

            if (string.IsNullOrEmpty(customPath))
            {
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string baseDir = Path.Combine(programData, "Tweaker");
                this._filePath = Path.Combine(baseDir, "settings.json");
            }
            else
            {
                this._filePath = customPath;
            }

            this.Load();
        }

        public UserSettings Settings
        {
            get { return this._settings; }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(this._filePath))
                {
                    string json = File.ReadAllText(this._filePath);
                    this._settings = this._serializer.Deserialize<UserSettings>(json);
                }
            }
            catch
            {
                this._settings = null;
            }

            if (this._settings == null)
            {
                this._settings = new UserSettings();
            }
            else
            {
                if (this._settings.GroupExpandedStates == null)
                {
                    this._settings.GroupExpandedStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                    this._settings.GroupExpandedStates["Основное"] = true;
                }
                if (string.IsNullOrEmpty(this._settings.TextRenderingMode))
                {
                    this._settings.TextRenderingMode = "ClearType";
                }
            }
        }

        public void Save()
        {
            if (this._isTestMode && StateManager.IsProgramDataPath(this._filePath))
            {
                throw new InvalidOperationException("В тестовом режиме запись в %ProgramData%\\Tweaker запрещена на уровне кода.");
            }

            try
            {
                string dir = Path.GetDirectoryName(this._filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string json = this._serializer.Serialize(this._settings);
                File.WriteAllText(this._filePath, json);
            }
            catch
            {
            }
        }
    }
}
