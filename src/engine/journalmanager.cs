using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WinTweaker.Engine
{
    public class JournalEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } // "INFO", "WARN", "ERROR", "APPLY", "REVERT"
        public string TweakId { get; set; }
        public string Message { get; set; }

        public string FormattedTime
        {
            get { return this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public override string ToString()
        {
            return string.Format("[{0}] [{1}] {2}{3}",
                this.FormattedTime,
                this.Level,
                string.IsNullOrEmpty(this.TweakId) ? "" : ("[" + this.TweakId + "] "),
                this.Message);
        }
    }

    public class JournalManager
    {
        private readonly string _logFilePath;
        private readonly string _backupDir;
        private readonly bool _isTestMode;
        private readonly List<JournalEntry> _entries;
        private readonly object _batchLock = new object();
        private readonly List<string> _pendingLines = new List<string>();
        private int _batchDepth;

        public event Action<JournalEntry> EntryAdded;

        public JournalManager(string customLogPath = null, bool isTestMode = false)
        {
            this._isTestMode = isTestMode;
            this._entries = new List<JournalEntry>();

            if (string.IsNullOrEmpty(customLogPath))
            {
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string baseDir = Path.Combine(programData, "Tweaker");
                this._logFilePath = Path.Combine(baseDir, "journal.log");
                this._backupDir = Path.Combine(baseDir, "backups");
            }
            else
            {
                this._logFilePath = customLogPath;
                string dir = Path.GetDirectoryName(customLogPath);
                this._backupDir = Path.Combine(string.IsNullOrEmpty(dir) ? Path.GetTempPath() : dir, "backups");
            }

            if (!isTestMode || !StateManager.IsProgramDataPath(this._logFilePath))
            {
                this.EnsureDirectories();
            }
            this.LoadExistingEntries();
        }

        private void EnsureDirectories()
        {
            try
            {
                string dir = Path.GetDirectoryName(this._logFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                if (!Directory.Exists(this._backupDir))
                {
                    Directory.CreateDirectory(this._backupDir);
                }
            }
            catch
            {
            }
        }

        private void LoadExistingEntries()
        {
            try
            {
                if (File.Exists(this._logFilePath))
                {
                    string[] lines = File.ReadAllLines(this._logFilePath, Encoding.UTF8);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        JournalEntry entry = new JournalEntry();
                        entry.Timestamp = DateTime.Now;
                        entry.Level = "INFO";
                        entry.Message = line;
                        this._entries.Add(entry);
                    }
                }
            }
            catch
            {
            }
        }

        public List<JournalEntry> Entries
        {
            get { return this._entries; }
        }

        public void Clear()
        {
            this._entries.Clear();
            try
            {
                if (File.Exists(this._logFilePath))
                {
                    File.WriteAllText(this._logFilePath, string.Empty);
                }
            }
            catch { }
        }

        public void BeginBatch()
        {
            lock (this._batchLock)
            {
                this._batchDepth++;
            }
        }

        public void EndBatch()
        {
            List<string> linesToFlush = null;
            lock (this._batchLock)
            {
                if (this._batchDepth > 0) this._batchDepth--;
                if (this._batchDepth == 0 && this._pendingLines.Count > 0)
                {
                    linesToFlush = new List<string>(this._pendingLines);
                    this._pendingLines.Clear();
                }
            }

            if (linesToFlush != null && linesToFlush.Count > 0)
            {
                try
                {
                    this.EnsureDirectories();
                    using (StreamWriter sw = new StreamWriter(this._logFilePath, true, Encoding.UTF8))
                    {
                        for (int i = 0; i < linesToFlush.Count; i++)
                        {
                            sw.WriteLine(linesToFlush[i]);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public void Log(string level, string tweakId, string message)
        {
            if (this._isTestMode && StateManager.IsProgramDataPath(this._logFilePath))
            {
                throw new InvalidOperationException("В тестовом режиме запись в %ProgramData%\\Tweaker запрещена на уровне кода.");
            }

            JournalEntry entry = new JournalEntry();
            entry.Timestamp = DateTime.Now;
            entry.Level = level;
            entry.TweakId = tweakId;
            entry.Message = message;

            bool notifyNow = true;
            lock (this._batchLock)
            {
                this._entries.Add(entry);
                if (this._batchDepth > 0)
                {
                    this._pendingLines.Add(entry.ToString());
                    notifyNow = false;
                }
            }

            if (notifyNow)
            {
                try
                {
                    this.EnsureDirectories();
                    using (StreamWriter sw = new StreamWriter(this._logFilePath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(entry.ToString());
                    }
                }
                catch
                {
                }

                if (this.EntryAdded != null)
                {
                    this.EntryAdded(entry);
                }
            }
        }

        public string ExportRegBackup(string tweakId, string hive, string subkey, string valueName, object currentValue, string kind)
        {
            try
            {
                this.EnsureDirectories();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = string.Format("backup_{0}_{1}.reg", tweakId, timestamp);
                string filePath = Path.Combine(this._backupDir, fileName);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Windows Registry Editor Version 5.00");
                sb.AppendLine();

                string fullKey = string.Format("[{0}\\{1}]", hive.ToUpperInvariant(), subkey);
                sb.AppendLine(fullKey);

                if (currentValue == null)
                {
                    sb.AppendLine(string.Format("\"{0}\"=-", valueName));
                }
                else if (kind == "REG_DWORD" || kind == "DWord")
                {
                    uint val = Convert.ToUInt32(currentValue);
                    sb.AppendLine(string.Format("\"{0}\"=dword:{1:x8}", valueName, val));
                }
                else if (kind == "REG_QWORD" || kind == "QWord")
                {
                    ulong val64 = Convert.ToUInt64(currentValue);
                    byte[] qBytes = BitConverter.GetBytes(val64);
                    sb.AppendLine(string.Format("\"{0}\"=hex(b):{1}", valueName, FormatRegHexList(qBytes)));
                }
                else if (kind == "REG_BINARY" || kind == "Binary" || currentValue is byte[])
                {
                    byte[] binBytes = currentValue as byte[];
                    if (binBytes == null)
                    {
                        binBytes = SpecialTweakExecutor.ParseHexBytes(currentValue.ToString());
                    }
                    sb.AppendLine(string.Format("\"{0}\"=hex:{1}", valueName, FormatRegHexList(binBytes)));
                }
                else if (kind == "REG_MULTI_SZ" || kind == "MultiString" || currentValue is string[])
                {
                    string[] lines = currentValue as string[];
                    string joined = lines != null ? string.Join(" ", lines) : currentValue.ToString();
                    sb.AppendLine(string.Format("\"{0}\"=\"{1}\"", valueName, joined.Replace("\\", "\\\\").Replace("\"", "\\\"")));
                }
                else
                {
                    sb.AppendLine(string.Format("\"{0}\"=\"{1}\"", valueName, currentValue.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")));
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.Unicode);
                this.Log("INFO", tweakId, "Резервная копия реестра экспортирована в: " + filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                this.Log("WARN", tweakId, "Ошибка при создании .reg резервной копии: " + ex.Message);
                return null;
            }
        }

        private static string FormatRegHexList(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            StringBuilder sb = new StringBuilder(bytes.Length * 3);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
