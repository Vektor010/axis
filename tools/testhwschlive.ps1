$csharpCode = @"
using System;
using System.Runtime.InteropServices;

public class HwschChecker
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
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern int D3DKMTOpenAdapterFromHdc(ref D3DKMT_OPENADAPTERFROMHDC open);

    [DllImport("gdi32.dll")]
    public static extern int D3DKMTQueryAdapterInfo(ref D3DKMT_QUERYADAPTERINFO query);

    [DllImport("gdi32.dll")]
    public static extern int D3DKMTCloseAdapter(ref D3DKMT_CLOSEADAPTER close);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool EnumDisplayDevicesW(string lpDevice, uint iDevNum, ref DISPLAY_DEVICEW lpDisplayDevice, uint dwFlags);

    const int KMTQAITYPE_WDDM_2_9_CAPS = 75;
    const int KMTQAITYPE_WDDM_3_0_CAPS = 77;
    const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;

    public static void Run()
    {
        var dd = new DISPLAY_DEVICEW();
        dd.cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICEW));

        for (uint i = 0; EnumDisplayDevicesW(null, i, ref dd, 0); i++)
        {
            if ((dd.StateFlags & DISPLAY_DEVICE_ACTIVE) != 0)
            {
                QueryDevice(dd.DeviceName, dd.DeviceString);
            }
            dd = new DISPLAY_DEVICEW();
            dd.cb = (uint)Marshal.SizeOf(typeof(DISPLAY_DEVICEW));
        }
    }

    static void QueryDevice(string devName, string devString)
    {
        Console.WriteLine("Display: " + devName + " (" + devString + ")");
        IntPtr hdc = CreateDCW("DISPLAY", devName, null, IntPtr.Zero);
        if (hdc == IntPtr.Zero)
        {
            Console.WriteLine("  Failed CreateDC");
            return;
        }

        try
        {
            var open = new D3DKMT_OPENADAPTERFROMHDC();
            open.hDc = hdc;
            int status = D3DKMTOpenAdapterFromHdc(ref open);
            if (status >= 0)
            {
                Console.WriteLine("  Adapter handle: " + open.hAdapter + ", LUID: " + open.AdapterLuid.HighPart + ":" + open.AdapterLuid.LowPart);

                // WDDM 2.9 (HWSCH / HAGS)
                IntPtr pCaps29 = Marshal.AllocHGlobal(4);
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
                    uint caps29 = (uint)Marshal.ReadInt32(pCaps29);
                    uint driverSupport = caps29 & 3u;
                    uint enabled = (caps29 >> 2) & 1u;
                    Console.WriteLine("  WDDM 2.9 HWSCH (HAGS): DriverSupport=" + driverSupport + ", Enabled=" + (enabled == 1 ? "ДА (Active)" : "НЕТ (Disabled)"));
                }
                else
                {
                    Console.WriteLine(string.Format("  WDDM 2.9 Query NTSTATUS=0x{0:X8}", st29));
                }
                Marshal.FreeHGlobal(pCaps29);

                // WDDM 3.0 (HWFLIPQUEUE)
                IntPtr pCaps30 = Marshal.AllocHGlobal(4);
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
                    uint caps30 = (uint)Marshal.ReadInt32(pCaps30);
                    uint driverSupport = caps30 & 3u;
                    uint enabled = (caps30 >> 2) & 1u;
                    uint displayable = (caps30 >> 3) & 1u;
                    Console.WriteLine("  WDDM 3.0 HWFLIPQUEUE: DriverSupport=" + driverSupport + ", Enabled=" + (enabled == 1 ? "ДА (Active)" : "НЕТ (Disabled)") + ", DisplayableSupported=" + displayable);
                }
                else
                {
                    Console.WriteLine(string.Format("  WDDM 3.0 Query NTSTATUS=0x{0:X8}", st30));
                }
                Marshal.FreeHGlobal(pCaps30);

                var close = new D3DKMT_CLOSEADAPTER { hAdapter = open.hAdapter };
                D3DKMTCloseAdapter(ref close);
            }
            else
            {
                Console.WriteLine(string.Format("  D3DKMTOpenAdapterFromHdc NTSTATUS=0x{0:X8}", status));
            }
        }
        finally
        {
            DeleteDC(hdc);
        }
    }
}
"@

Add-Type -TypeDefinition $csharpCode -Language CSharp
[HwschChecker]::Run()
