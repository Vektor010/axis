using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace WinTweaker.Engine
{
    public interface IProcessRunner
    {
        int Run(string fileName, string arguments, out string stdout, out string stderr);
    }

    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public class DefaultProcessRunner : IProcessRunner
    {
        public int Run(string fileName, string arguments, out string stdout, out string stderr)
        {
            ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process proc = Process.Start(psi))
            {
                proc.WaitForExit(30000);
                stdout = proc.StandardOutput.ReadToEnd();
                stderr = proc.StandardError.ReadToEnd();
                return proc.ExitCode;
            }
        }
    }

    public class TestModeProcessRunner : IProcessRunner
    {
        public int CallCount = 0;

        public int Run(string fileName, string arguments, out string stdout, out string stderr)
        {
            this.CallCount++;
            stdout = "";
            stderr = "";
            return 0;
        }
    }

    public class SystemClock : IClock
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }

    public class RestorePointManager
    {
        private static readonly TimeSpan Cooldown = TimeSpan.FromHours(24);

        private readonly IProcessRunner _processRunner;
        private readonly IClock _clock;
        private readonly Func<bool> _isRestoreAvailableFunc;
        private readonly bool _isRealMode;
        private readonly IRegistryStore _registryStore;
        private readonly StateManager _stateManager;
        private DateTime? _lastRestorePointUtc;

        public IProcessRunner ProcessRunner { get { return this._processRunner; } }
        public bool IsRealMode { get { return this._isRealMode; } }

        public RestorePointManager(bool isRealMode = false, StateManager stateManager = null)
            : this(isRealMode ? (IProcessRunner)new DefaultProcessRunner() : new TestModeProcessRunner(), null, null, isRealMode, null, stateManager)
        {
        }

        public RestorePointManager(
            IProcessRunner processRunner,
            IClock clock = null,
            Func<bool> isRestoreAvailableFunc = null,
            bool isRealMode = false,
            IRegistryStore registryStore = null,
            StateManager stateManager = null)
        {
            this._isRealMode = isRealMode;
            this._processRunner = processRunner ?? (isRealMode ? (IProcessRunner)new DefaultProcessRunner() : new TestModeProcessRunner());
            this._clock = clock ?? new SystemClock();
            this._isRestoreAvailableFunc = isRestoreAvailableFunc;
            this._registryStore = registryStore;
            this._stateManager = stateManager;
            this._lastRestorePointUtc = null;
        }

        public DateTime? GetLastRestorePointUtc()
        {
            if (this._stateManager != null)
            {
                return this._stateManager.GetLastRestorePointUtc();
            }
            return this._lastRestorePointUtc;
        }

        public void SetLastRestorePointUtc(DateTime dt)
        {
            if (this._stateManager != null)
            {
                this._stateManager.SetLastRestorePointUtc(dt);
            }
            this._lastRestorePointUtc = dt;
        }

        public bool IsSystemRestoreAvailable()
        {
            if (this._isRestoreAvailableFunc != null)
            {
                return this._isRestoreAvailableFunc();
            }

            try
            {
                if (this._registryStore != null)
                {
                    // 1. Проверка политики Group Policy через IRegistryStore
                    if (this._registryStore.ValueExists("HKLM", @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR"))
                    {
                        RegistryValueKind kind = this._registryStore.GetValueKind("HKLM", @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR");
                        object polVal = this._registryStore.GetValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR");
                        if (kind == RegistryValueKind.DWord && polVal != null)
                        {
                            try
                            {
                                int intVal = Convert.ToInt32(polVal);
                                if (intVal == 1) return false;
                                if (intVal == 0) return true;
                            }
                            catch { }
                        }
                    }

                    // 2. Проверка системной конфигурации через IRegistryStore
                    if (this._registryStore.ValueExists("HKLM", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "DisableSR"))
                    {
                        RegistryValueKind kind = this._registryStore.GetValueKind("HKLM", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "DisableSR");
                        object val = this._registryStore.GetValue("HKLM", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "DisableSR");
                        if (kind == RegistryValueKind.DWord && val != null)
                        {
                            try
                            {
                                int intVal = Convert.ToInt32(val);
                                if (intVal == 1) return false;
                            }
                            catch { }
                        }
                    }

                    return true;
                }

                // 1. Проверка политики Group Policy в реестре хоста
                using (RegistryKey polKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore"))
                {
                    if (polKey != null)
                    {
                        object polVal = polKey.GetValue("DisableSR");
                        if (polVal != null)
                        {
                            try
                            {
                                RegistryValueKind kind = polKey.GetValueKind("DisableSR");
                                if (kind == RegistryValueKind.DWord)
                                {
                                    int intVal = Convert.ToInt32(polVal);
                                    if (intVal == 1) return false;
                                    if (intVal == 0) return true;
                                }
                            }
                            catch { }
                        }
                    }
                }

                // 2. Проверка системной конфигурации SystemRestore в реестре хоста
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("DisableSR");
                        if (val != null)
                        {
                            try
                            {
                                RegistryValueKind kind = key.GetValueKind("DisableSR");
                                if (kind == RegistryValueKind.DWord)
                                {
                                    int intVal = Convert.ToInt32(val);
                                    if (intVal == 1) return false;
                                }
                            }
                            catch { }
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnsureRestorePoint(string description, JournalManager journal, out string message)
        {
            // 0. Защитная блокировка: в тестовом режиме точка восстановления пропускается
            if (!this._isRealMode)
            {
                message = "Точка восстановления: пропущена (тестовый режим).";
                if (journal != null)
                {
                    journal.Log("INFO", "", message);
                }
                return false;
            }

            // 1. Проверка доступности Защиты системы
            if (!this.IsSystemRestoreAvailable())
            {
                message = "Защита системы (SystemRestore) отключена в Windows. Создание точки восстановления пропущено, применение твика не блокируется.";
                if (journal != null)
                {
                    journal.Log("WARN", "", message);
                }
                return false;
            }

            // 2. Проверка 24-часового лимита Windows
            DateTime? lastPoint = this.GetLastRestorePointUtc();
            if (lastPoint.HasValue)
            {
                TimeSpan elapsed = this._clock.UtcNow - lastPoint.Value;
                if (elapsed < Cooldown)
                {
                    TimeSpan remaining = Cooldown - elapsed;
                    message = string.Format("Точка восстановления создана менее 24 часов назад. До следующей попытки: {0:hh\\:mm}.", remaining);
                    if (journal != null)
                    {
                        journal.Log("INFO", "", message);
                    }
                    return false;
                }
            }

            // 3. Вызов Checkpoint-Computer через IProcessRunner
            string args = string.Format("-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description '{0}' -RestorePointType 'MODIFY_SETTINGS'\"", description);
            string stdout, stderr;
            int exitCode = this._processRunner.Run("powershell.exe", args, out stdout, out stderr);

            if (exitCode == 0)
            {
                this.SetLastRestorePointUtc(this._clock.UtcNow);
                message = "Точка восстановления успешно создана (Checkpoint-Computer).";
                if (journal != null)
                {
                    journal.Log("INFO", "", message);
                }
                return true;
            }
            else
            {
                string err = string.IsNullOrEmpty(stderr) ? ("код " + exitCode) : stderr.Trim();
                message = "Ошибка при создании точки восстановления Checkpoint-Computer: " + err;
                if (journal != null)
                {
                    // При ошибке Checkpoint-Computer пишется WARN, чтобы не блокировать применение твика
                    journal.Log("WARN", "", message);
                }
                return false;
            }
        }
    }
}
