using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace WinTweaker.Engine
{
    public class RegistryValueSnapshot
    {
        public string Hive { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public bool Existed { get; set; }
        public object Value { get; set; }
        public string Kind { get; set; }

        public RegistryValueSnapshot()
        {
        }
    }

    public class TweakSnapshot
    {
        public string TweakId { get; set; }
        public string AppliedAt { get; set; }
        public List<RegistryValueSnapshot> Values { get; set; }

        public TweakSnapshot()
        {
            this.Values = new List<RegistryValueSnapshot>();
        }
    }

    public class StateData
    {
        public string Version { get; set; }
        public string LastUpdated { get; set; }
        public string LastRestorePointUtc { get; set; }
        public Dictionary<string, TweakSnapshot> Snapshots { get; set; }

        public StateData()
        {
            this.Version = "1.0";
            this.Snapshots = new Dictionary<string, TweakSnapshot>();
        }
    }

    public class StateManager
    {
        private readonly string _filePath;
        private readonly bool _isTestMode;
        private StateData _data;
        private readonly JavaScriptSerializer _serializer;

        public StateManager(string customPath = null, bool isTestMode = false)
        {
            this._isTestMode = isTestMode;
            this._serializer = new JavaScriptSerializer();
            if (string.IsNullOrEmpty(customPath))
            {
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string tweakerDir = Path.Combine(programData, "Tweaker");
                if (!isTestMode && !Directory.Exists(tweakerDir))
                {
                    try { Directory.CreateDirectory(tweakerDir); } catch { }
                }
                this._filePath = Path.Combine(tweakerDir, "state.json");
            }
            else
            {
                this._filePath = customPath;
            }

            this.Load();
        }

        public static bool IsProgramDataPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string tweakerDir = Path.GetFullPath(Path.Combine(programData, "Tweaker"));
            string full = Path.GetFullPath(path);
            return full.StartsWith(tweakerDir, StringComparison.OrdinalIgnoreCase);
        }

        public string FilePath
        {
            get { return this._filePath; }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(this._filePath))
                {
                    string json = File.ReadAllText(this._filePath);
                    this._data = this._serializer.Deserialize<StateData>(json);
                }
            }
            catch
            {
                this._data = null;
            }

            if (this._data == null)
            {
                this._data = new StateData();
            }
            if (this._data.Snapshots == null)
            {
                this._data.Snapshots = new Dictionary<string, TweakSnapshot>();
            }
        }

        public void Save()
        {
            if (this._isTestMode && IsProgramDataPath(this._filePath))
            {
                throw new InvalidOperationException("В тестовом режиме запись в %ProgramData%\\Tweaker запрещена на уровне кода.");
            }

            try
            {
                this._data.LastUpdated = DateTime.Now.ToString("o");
                string dir = Path.GetDirectoryName(this._filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string json = this._serializer.Serialize(this._data);
                File.WriteAllText(this._filePath, json);
            }
            catch
            {
            }
        }

        public bool HasSnapshot(string tweakId)
        {
            return this._data != null && this._data.Snapshots.ContainsKey(tweakId);
        }

        public TweakSnapshot GetSnapshot(string tweakId)
        {
            if (this.HasSnapshot(tweakId))
            {
                return this._data.Snapshots[tweakId];
            }
            return null;
        }

        public void SaveSnapshot(string tweakId, TweakSnapshot snapshot)
        {
            if (this._data == null) this._data = new StateData();
            snapshot.TweakId = tweakId;
            snapshot.AppliedAt = DateTime.Now.ToString("o");
            this._data.Snapshots[tweakId] = snapshot;
            this.Save();
        }

        public void RemoveSnapshot(string tweakId)
        {
            if (this._data != null && this._data.Snapshots.ContainsKey(tweakId))
            {
                this._data.Snapshots.Remove(tweakId);
                this.Save();
            }
        }

        public DateTime? GetLastRestorePointUtc()
        {
            if (this._data != null && !string.IsNullOrEmpty(this._data.LastRestorePointUtc))
            {
                DateTime dt;
                if (DateTime.TryParse(this._data.LastRestorePointUtc, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dt))
                {
                    return dt.ToUniversalTime();
                }
            }
            return null;
        }

        public void SetLastRestorePointUtc(DateTime dt)
        {
            if (this._data == null) this._data = new StateData();
            this._data.LastRestorePointUtc = dt.ToUniversalTime().ToString("o");
            this.Save();
        }

        public List<string> GetAppliedTweakIds()
        {
            if (this._data != null && this._data.Snapshots != null)
            {
                return new List<string>(this._data.Snapshots.Keys);
            }
            return new List<string>();
        }
    }
}
