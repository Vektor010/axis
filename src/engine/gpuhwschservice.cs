using System;
using System.Runtime.InteropServices;

namespace WinTweaker.Engine
{
    public class GpuHwschState
    {
        public string DeviceName { get; set; }
        public string AdapterString { get; set; }
        public uint AdapterLuidLow { get; set; }
        public int AdapterLuidHigh { get; set; }
        public bool IsWddm29Supported { get; set; }
        public bool IsHagsEnabled { get; set; }
        public uint HagsDriverSupportState { get; set; }
        public string HagsStatusDisplay { get; set; }
        public bool IsWddm30Supported { get; set; }
        public bool IsFlipQueueEnabled { get; set; }
        public uint FlipQueueDriverSupportState { get; set; }
        public bool IsDisplayableSupported { get; set; }
        public string FlipQueueStatusDisplay { get; set; }
        public string SummaryDisplay { get; set; }
        public bool HasActiveGpu { get; set; }

        public GpuHwschState()
        {
            this.DeviceName = "DISPLAY1";
            this.AdapterString = "Видеокарта";
            this.AdapterLuidLow = 0;
            this.AdapterLuidHigh = 0;
            this.IsWddm29Supported = false;
            this.IsHagsEnabled = false;
            this.HagsDriverSupportState = 0;
            this.HagsStatusDisplay = "Не поддерживается";
            this.IsWddm30Supported = false;
            this.IsFlipQueueEnabled = false;
            this.FlipQueueDriverSupportState = 0;
            this.IsDisplayableSupported = false;
            this.FlipQueueStatusDisplay = "Не поддерживается";
            this.SummaryDisplay = "HAGS: Нет данных";
            this.HasActiveGpu = false;
        }
    }

    public class GpuHwschService
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct D3DKMT_OPENADAPTERFROMHDC
        {
            public IntPtr hDc;
            public uint hAdapter;
            public LUID AdapterLuid;
            public uint VidPnSourceId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct D3DKMT_QUERYADAPTERINFO
        {
            public uint hAdapter;
            public int Type;
            public IntPtr pPrivateDriverData;
            public uint PrivateDriverDataSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct D3DKMT_CLOSEADAPTER
        {
            public uint hAdapter;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAY_DEVICEW
        {
            public uint cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateDCW(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTOpenAdapterFromHdc(ref D3DKMT_OPENADAPTERFROMHDC open);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTQueryAdapterInfo(ref D3DKMT_QUERYADAPTERINFO query);

        [DllImport("gdi32.dll")]
        public static extern int D3DKMTCloseAdapter(ref D3DKMT_CLOSEADAPTER close);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayDevicesW(string lpDevice, uint iDevNum, ref DISPLAY_DEVICEW lpDisplayDevice, uint dwFlags);

        public const int KMTQAITYPE_WDDM_2_9_CAPS = 75;
        public const int KMTQAITYPE_WDDM_3_0_CAPS = 77;
        private const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;

        public GpuHwschState QueryPrimaryGpuState()
        {
            var state = new GpuHwschState();

            try
            {
                var dd = new DISPLAY_DEVICEW();
                dd.cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICEW));

                for (uint i = 0; EnumDisplayDevicesW(null, i, ref dd, 0); i++)
                {
                    if ((dd.StateFlags & DISPLAY_DEVICE_ACTIVE) != 0)
                    {
                        state.DeviceName = dd.DeviceName ?? "DISPLAY1";
                        state.AdapterString = dd.DeviceString ?? "GPU";
                        state.HasActiveGpu = true;

                        QueryAdapter(state);
                        break;
                    }

                    dd = new DISPLAY_DEVICEW();
                    dd.cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICEW));
                }
            }
            catch
            {
                // Fallback gracefully on non-desktop or headless environments
            }

            return state;
        }

        private static void QueryAdapter(GpuHwschState state)
        {
            IntPtr hdc = CreateDCW("DISPLAY", state.DeviceName, null, IntPtr.Zero);
            if (hdc == IntPtr.Zero) return;

            try
            {
                var open = new D3DKMT_OPENADAPTERFROMHDC();
                open.hDc = hdc;

                int status = D3DKMTOpenAdapterFromHdc(ref open);
                if (status >= 0)
                {
                    state.AdapterLuidLow = open.AdapterLuid.LowPart;
                    state.AdapterLuidHigh = open.AdapterLuid.HighPart;

                    // 1. WDDM 2.9 (HWSCH / HAGS)
                    IntPtr pCaps29 = Marshal.AllocHGlobal(4);
                    try
                    {
                        Marshal.WriteInt32(pCaps29, 0);
                        var q29 = new D3DKMT_QUERYADAPTERINFO
                        {
                            hAdapter = open.hAdapter,
                            Type = KMTQAITYPE_WDDM_2_9_CAPS,
                            pPrivateDriverData = pCaps29,
                            PrivateDriverDataSize = 4
                        };

                        int st29 = D3DKMTQueryAdapterInfo(ref q29);
                        if (st29 >= 0)
                        {
                            state.IsWddm29Supported = true;
                            uint caps29 = (uint)Marshal.ReadInt32(pCaps29);
                            state.HagsDriverSupportState = caps29 & 3u;
                            state.IsHagsEnabled = ((caps29 >> 2) & 1u) == 1;
                            state.HagsStatusDisplay = state.IsHagsEnabled ? "Активен (WDDM 2.9)" : "Отключен в системе";
                        }
                        else
                        {
                            state.HagsStatusDisplay = "Драйвер не поддерживает WDDM 2.9";
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pCaps29);
                    }

                    // 2. WDDM 3.0 (HWFLIPQUEUE)
                    IntPtr pCaps30 = Marshal.AllocHGlobal(4);
                    try
                    {
                        Marshal.WriteInt32(pCaps30, 0);
                        var q30 = new D3DKMT_QUERYADAPTERINFO
                        {
                            hAdapter = open.hAdapter,
                            Type = KMTQAITYPE_WDDM_3_0_CAPS,
                            pPrivateDriverData = pCaps30,
                            PrivateDriverDataSize = 4
                        };

                        int st30 = D3DKMTQueryAdapterInfo(ref q30);
                        if (st30 >= 0)
                        {
                            state.IsWddm30Supported = true;
                            uint caps30 = (uint)Marshal.ReadInt32(pCaps30);
                            state.FlipQueueDriverSupportState = caps30 & 3u;
                            state.IsFlipQueueEnabled = ((caps30 >> 2) & 1u) == 1;
                            state.IsDisplayableSupported = ((caps30 >> 3) & 1u) == 1;
                            state.FlipQueueStatusDisplay = state.IsFlipQueueEnabled ? "Активна (WDDM 3.0)" : "Отключена в системе";
                        }
                        else
                        {
                            state.FlipQueueStatusDisplay = "Драйвер не поддерживает WDDM 3.0";
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pCaps30);
                    }

                    state.SummaryDisplay = string.Format("HAGS: {0} | FlipQueue: {1}",
                        state.IsHagsEnabled ? "Вкл" : "Выкл",
                        state.IsFlipQueueEnabled ? "Вкл" : "Выкл");

                    var close = new D3DKMT_CLOSEADAPTER { hAdapter = open.hAdapter };
                    D3DKMTCloseAdapter(ref close);
                }
            }
            catch
            {
                // Safe error swallow
            }
            finally
            {
                DeleteDC(hdc);
            }
        }
    }
}
