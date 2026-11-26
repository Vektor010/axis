using System;
using System.Runtime.InteropServices;
using WinTweaker.Engine;

namespace WinTweaker.Common
{
    public static class NativeMethods
    {
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int DWMWA_MICA_EFFECT = 1029;
        public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        public const int DWMWA_BORDER_COLOR = 34;
        public const int DWMWA_CAPTION_COLOR = 35;
        public const int DWMWA_TEXT_COLOR = 36;

        public const int DWMWCP_ROUND = 2;
        public const int DWMSBT_MAINWINDOW = 2; // Mica

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public const int REG_NOTIFY_CHANGE_NAME = 1;
        public const int REG_NOTIFY_CHANGE_ATTRIBUTES = 2;
        public const int REG_NOTIFY_CHANGE_LAST_SET = 4;
        public const int REG_NOTIFY_CHANGE_SECURITY = 8;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegNotifyChangeKeyValue(
            IntPtr hKey,
            bool bWatchSubtree,
            int dwNotifyFilter,
            IntPtr hEvent,
            bool fAsynchronous);

        public static void ApplyModernWindowStyles(IntPtr hwnd, bool isDark, JournalManager journal = null, bool disableMica = false)
        {
            try
            {
                OperatingSystem os = Environment.OSVersion;
                int build = os.Version.Build;

                // Dark mode for title bar
                int darkValue = isDark ? 1 : 0;
                int attr = (build >= 18985) ? DWMWA_USE_IMMERSIVE_DARK_MODE : DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                DwmSetWindowAttribute(hwnd, attr, ref darkValue, sizeof(int));
                if (journal != null && isDark)
                {
                    journal.Log("INFO", "", "Заголовок окна: применен темный режим (DWMWA_USE_IMMERSIVE_DARK_MODE)");
                }

                // Windows 11 rounded corners (Build 22000+)
                if (build >= 22000)
                {
                    int cornerVal = DWMWCP_ROUND;
                    DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerVal, sizeof(int));

                    // Windows 11 22H2+ Mica backdrop (Build 22621+)
                    if (build >= 22621 && !disableMica)
                    {
                        int backdropType = DWMSBT_MAINWINDOW;
                        DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
                        if (journal != null)
                        {
                            journal.Log("INFO", "", "Mica: применено (билд " + build + ")");
                        }
                    }
                    else
                    {
                        // Win11 21H2 legacy mica
                        int mica = 1;
                        DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref mica, sizeof(int));
                        if (journal != null)
                        {
                            journal.Log("INFO", "", "Mica: применено (билд " + build + ")");
                        }
                    }
                }
                else
                {
                    if (journal != null)
                    {
                        journal.Log("INFO", "", "Mica: не поддерживается (билд " + build + ")");
                    }
                }
            }
            catch
            {
                // Soft fallback on Windows 10
            }
        }

        public static void ApplyWindowFrameColors(IntPtr hwnd, bool isDark)
        {
            if (hwnd == IntPtr.Zero) return;
            try
            {
                int build = Environment.OSVersion.Version.Build;
                if (build < 22000) return;

                // Color format for DWMWA_CAPTION_COLOR is COLORREF: 0x00BBGGRR (R | (G << 8) | (B << 16))
                // Dark Theme: WindowBg is #0B0B12 (R: 0x0B, G: 0x0B, B: 0x12) -> 0x00120B0B
                // Border: #242434 (R: 0x24, G: 0x24, B: 0x34) -> 0x00342424
                // Text: #E8E8F2 (R: 0xE8, G: 0xE8, B: 0xF2) -> 0x00F2E8E8
                //
                // Light Theme: WindowBg is #F3F3F8 (R: 0xF3, G: 0xF3, B: 0xF8) -> 0x00F8F3F3
                // Border: #E1E1EC (R: 0xE1, G: 0xE1, B: 0xEC) -> 0x00ECE1E1
                // Text: #1B1B28 (R: 0x1B, G: 0x1B, B: 0x28) -> 0x00281B1B
                int captionColor = isDark ? (0x0B | (0x0B << 8) | (0x12 << 16)) : (0xF3 | (0xF3 << 8) | (0xF8 << 16));
                int borderColor = isDark ? (0x24 | (0x24 << 8) | (0x34 << 16)) : (0xE1 | (0xE1 << 8) | (0xEC << 16));
                int textColor = isDark ? (0xE8 | (0xE8 << 8) | (0xF2 << 16)) : (0x1B | (0x1B << 8) | (0x28 << 16));

                DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));
            }
            catch { }
        }

        private const int HWND_BROADCAST = 0xFFFF;
        private const int WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;
        private const uint SPI_SETMOUSE = 0x0004;
        private const uint SPI_SETANIMATION = 0x0049;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        [StructLayout(LayoutKind.Sequential)]
        private struct ANIMATIONINFO
        {
            public uint cbSize;
            public int iMinAnimate;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            int Msg,
            IntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            IntPtr pvParam,
            uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            int[] pvParam,
            uint fWinIni);

        public static void BroadcastSettingChange(string area)
        {
            try
            {
                IntPtr res;
                SendMessageTimeout(new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, IntPtr.Zero, area, SMTO_ABORTIFHUNG, 500, out res);
            }
            catch { }
        }

        public static void ApplyLiveOsRefresh(string hive, string path, string name, object writtenValue)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                string lowPath = path.ToLowerInvariant();
                if (lowPath == @"control panel\mouse" &&
                    (string.Equals(name, "MouseSpeed", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(name, "MouseThreshold1", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(name, "MouseThreshold2", StringComparison.OrdinalIgnoreCase)))
                {
                    int[] mouseParams = new int[3];
                    mouseParams[0] = ReadHkcuInt(@"Control Panel\Mouse", "MouseThreshold1", 0);
                    mouseParams[1] = ReadHkcuInt(@"Control Panel\Mouse", "MouseThreshold2", 0);
                    mouseParams[2] = ReadHkcuInt(@"Control Panel\Mouse", "MouseSpeed", 0);
                    SystemParametersInfo(SPI_SETMOUSE, 0, mouseParams, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                }
                else if (lowPath == @"control panel\desktop\windowmetrics" &&
                         string.Equals(name, "MinAnimate", StringComparison.OrdinalIgnoreCase))
                {
                    int val = 0;
                    if (writtenValue != null) int.TryParse(writtenValue.ToString(), out val);
                    ANIMATIONINFO ai = new ANIMATIONINFO();
                    ai.cbSize = (uint)Marshal.SizeOf(typeof(ANIMATIONINFO));
                    ai.iMinAnimate = (val != 0) ? 1 : 0;
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ANIMATIONINFO)));
                    try
                    {
                        Marshal.StructureToPtr(ai, ptr, false);
                        SystemParametersInfo(SPI_SETANIMATION, ai.cbSize, ptr, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
                else if (lowPath.StartsWith(@"control panel\desktop") ||
                         lowPath.Contains(@"explorer\visualeffects") ||
                         lowPath.Contains(@"explorer\advanced"))
                {
                    BroadcastSettingChange("Environment");
                    BroadcastSettingChange("WindowMetrics");
                }
            }
            catch { }
        }

        private static int ReadHkcuInt(string subkey, string name, int fallback)
        {
            try
            {
                using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey, false))
                {
                    if (k != null)
                    {
                        object v = k.GetValue(name);
                        int parsed;
                        if (v != null && int.TryParse(v.ToString(), out parsed)) return parsed;
                    }
                }
            }
            catch { }
            return fallback;
        }
    }
}
