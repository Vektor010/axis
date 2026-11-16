using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WinTweaker.Engine
{
    public class GameModeState
    {
        public uint ActiveGamePid { get; set; }
        public string ActiveGameProcessName { get; set; }
        public uint PowerProfileLowValue { get; set; }
        public bool IsPowerProfileActive { get; set; }
        public uint ForegroundPid { get; set; }
        public string ForegroundProcessName { get; set; }
        public int CpuSetCount { get; set; }
        public string CpuSetIdsDisplay { get; set; }
        public string StatusSummary { get; set; }
        public bool IsGameRunning { get; set; }

        public GameModeState()
        {
            this.ActiveGamePid = 0;
            this.ActiveGameProcessName = "Отсутствует";
            this.PowerProfileLowValue = 1;
            this.IsPowerProfileActive = false;
            this.ForegroundPid = 0;
            this.ForegroundProcessName = "Нет данных";
            this.CpuSetCount = 0;
            this.CpuSetIdsDisplay = "Не назначены";
            this.StatusSummary = "Game Mode: Ожидание игры";
            this.IsGameRunning = false;
        }
    }

    public class GameModeWnfService
    {
        public const ulong WNF_RM_GAME_MODE_ACTIVE = 0x41C6033FA3BC1075UL;
        public const ulong WNF_SEB_GAME_MODE       = 0x41840B3EA3BDD875UL;

        [DllImport("ntdll.dll")]
        public static extern int NtQueryWnfStateData(
            ref ulong StateName,
            IntPtr TypeId,
            IntPtr ExplicitScope,
            out uint ChangeStamp,
            IntPtr Buffer,
            ref uint BufferSize);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetProcessDefaultCpuSets(
            IntPtr hProcess,
            [Out] uint[] CpuSetIds,
            uint CpuSetIdCount,
            out uint RequiredIdCount);

        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        public static uint QueryWnfU32(ulong stateName, out int ntStatus)
        {
            uint changeStamp = 0;
            uint bufferSize = 8;
            IntPtr buf = Marshal.AllocHGlobal(8);
            try
            {
                ntStatus = NtQueryWnfStateData(ref stateName, IntPtr.Zero, IntPtr.Zero, out changeStamp, buf, ref bufferSize);
                if (ntStatus >= 0 && bufferSize >= 4)
                {
                    return (uint)Marshal.ReadInt32(buf);
                }
                return 0;
            }
            catch
            {
                ntStatus = -1;
                return 0;
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }

        public GameModeState QueryState()
        {
            var state = new GameModeState();

            // 1. Foreground PID
            try
            {
                IntPtr fgHwnd = GetForegroundWindow();
                if (fgHwnd != IntPtr.Zero)
                {
                    uint fgPid = 0;
                    GetWindowThreadProcessId(fgHwnd, out fgPid);
                    state.ForegroundPid = fgPid;
                    if (fgPid > 0)
                    {
                        try
                        {
                            using (var proc = Process.GetProcessById((int)fgPid))
                            {
                                state.ForegroundProcessName = proc.ProcessName + ".exe";
                            }
                        }
                        catch
                        {
                            state.ForegroundProcessName = "PID " + fgPid;
                        }
                    }
                }
            }
            catch { }

            // 2. WNF Game Mode Active PID
            int statusActive = 0;
            uint activePid = QueryWnfU32(WNF_RM_GAME_MODE_ACTIVE, out statusActive);
            state.ActiveGamePid = activePid;

            // 3. WNF Game Mode Power Profile Low Value
            int statusPower = 0;
            uint powerLow = QueryWnfU32(WNF_SEB_GAME_MODE, out statusPower);
            state.PowerProfileLowValue = powerLow;
            state.IsPowerProfileActive = (powerLow == 3);

            // 4. Resolve Active Game Process & CPU Sets
            if (activePid > 0)
            {
                state.IsGameRunning = true;
                try
                {
                    using (var proc = Process.GetProcessById((int)activePid))
                    {
                        state.ActiveGameProcessName = proc.ProcessName + ".exe";
                    }
                }
                catch
                {
                    state.ActiveGameProcessName = "PID " + activePid;
                }

                // Query CPU sets
                IntPtr hProc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, activePid);
                if (hProc != IntPtr.Zero)
                {
                    try
                    {
                        uint[] cpuSets = new uint[256];
                        uint req = 0;
                        if (GetProcessDefaultCpuSets(hProc, cpuSets, (uint)cpuSets.Length, out req))
                        {
                            state.CpuSetCount = (int)req;
                            if (req > 0)
                            {
                                int limit = Math.Min((int)req, cpuSets.Length);
                                StringBuilder sb = new StringBuilder();
                                for (int i = 0; i < limit; i++)
                                {
                                    if (i > 0) sb.Append(", ");
                                    sb.Append(cpuSets[i]);
                                }
                                state.CpuSetIdsDisplay = sb.ToString();
                            }
                            else
                            {
                                state.CpuSetIdsDisplay = "0 (Все ядра)";
                            }
                        }
                    }
                    catch
                    {
                        state.CpuSetIdsDisplay = "Недоступно";
                    }
                    finally
                    {
                        CloseHandle(hProc);
                    }
                }

                state.StatusSummary = string.Format("Game Mode: Активен для {0} (WNF={1})", state.ActiveGameProcessName, powerLow);
            }
            else
            {
                state.IsGameRunning = false;
                state.ActiveGameProcessName = "Нет активных игр";
                state.CpuSetIdsDisplay = "—";
                state.StatusSummary = (powerLow == 3)
                    ? "Game Mode: Профиль WNF активен (фоновый)"
                    : "Game Mode: Ожидание игры (WNF=1)";
            }

            return state;
        }
    }
}
