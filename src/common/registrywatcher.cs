using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;

namespace WinTweaker.Common
{
    public class RegistryWatcher : IDisposable
    {
        private const int KEY_NOTIFY = 0x0010;
        private const int REG_NOTIFY_CHANGE_NAME = 1;
        private const int REG_NOTIFY_CHANGE_LAST_SET = 4;

        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegNotifyChangeKeyValue(
            IntPtr hKey,
            bool bWatchSubtree,
            int dwNotifyFilter,
            IntPtr hEvent,
            bool fAsynchronous);

        private readonly string _hiveName;
        private readonly string _subKey;
        private readonly Dispatcher _dispatcher;
        private readonly ManualResetEvent _stopEvent;
        private readonly AutoResetEvent _changeEvent;
        private Thread _workerThread;
        private IntPtr _hKey;
        private bool _disposed;

        public event EventHandler Changed;

        public RegistryWatcher(string hiveName, string subKey, Dispatcher dispatcher = null)
        {
            this._hiveName = hiveName;
            this._subKey = subKey;
            this._dispatcher = dispatcher;
            this._stopEvent = new ManualResetEvent(false);
            this._changeEvent = new AutoResetEvent(false);
            this._hKey = IntPtr.Zero;
            this._disposed = false;

            this.Start();
        }

        private IntPtr GetHiveHandle(string hive)
        {
            if (string.Equals(hive, "HKCU", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hive, "HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
            {
                return HKEY_CURRENT_USER;
            }
            if (string.Equals(hive, "HKLM", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hive, "HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
            {
                return HKEY_LOCAL_MACHINE;
            }
            return HKEY_CURRENT_USER;
        }

        private void Start()
        {
            IntPtr root = this.GetHiveHandle(this._hiveName);
            int ret = RegOpenKeyEx(root, this._subKey, 0, KEY_NOTIFY, out this._hKey);
            if (ret != 0 || this._hKey == IntPtr.Zero)
            {
                // Key might not exist yet; watcher will remain idle
                return;
            }

            this._workerThread = new Thread(this.WorkerLoop)
            {
                IsBackground = true,
                Name = "RegistryWatcher_" + this._subKey
            };
            this._workerThread.Start();
        }

        private void WorkerLoop()
        {
            WaitHandle[] handles = new WaitHandle[] { this._stopEvent, this._changeEvent };

            while (!this._disposed)
            {
                int filter = REG_NOTIFY_CHANGE_LAST_SET | REG_NOTIFY_CHANGE_NAME;
                int notifyRes = RegNotifyChangeKeyValue(this._hKey, true, filter, this._changeEvent.SafeWaitHandle.DangerousGetHandle(), true);
                if (notifyRes != 0)
                {
                    break;
                }

                int waitIdx = WaitHandle.WaitAny(handles);
                if (waitIdx == 0 || this._disposed)
                {
                    break;
                }

                // Event triggered
                this.FireChanged();
            }
        }

        private void FireChanged()
        {
            EventHandler handler = this.Changed;
            if (handler != null)
            {
                if (this._dispatcher != null && !this._dispatcher.HasShutdownStarted)
                {
                    try
                    {
                        this._dispatcher.BeginInvoke(new Action(delegate { handler(this, EventArgs.Empty); }));
                    }
                    catch
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
                else
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;

            try
            {
                this._stopEvent.Set();
                if (this._workerThread != null && this._workerThread.IsAlive)
                {
                    this._workerThread.Join(500);
                }
            }
            catch { }

            if (this._hKey != IntPtr.Zero)
            {
                try
                {
                    RegCloseKey(this._hKey);
                }
                catch { }
                this._hKey = IntPtr.Zero;
            }

            try { this._changeEvent.Close(); } catch { }
            try { this._stopEvent.Close(); } catch { }
        }
    }
}
