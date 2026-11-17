using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace WinTweaker.Common
{
    public static class WindowPlacementHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        private const int MONITORINFOF_PRIMARY = 0x00000001;

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        public static List<Rect> GetScreenWorkAreas()
        {
            List<Rect> list = new List<Rect>();
            try
            {
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate(IntPtr hMon, IntPtr hdc, ref RECT rc, IntPtr data)
                {
                    MONITORINFOEX mi = new MONITORINFOEX();
                    mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                    if (GetMonitorInfo(hMon, ref mi))
                    {
                        double w = mi.rcWork.Right - mi.rcWork.Left;
                        double h = mi.rcWork.Bottom - mi.rcWork.Top;
                        if (w > 0 && h > 0)
                        {
                            list.Add(new Rect(mi.rcWork.Left, mi.rcWork.Top, w, h));
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch
            {
            }

            if (list.Count == 0)
            {
                list.Add(new Rect(0, 0, 1920, 1080));
            }
            return list;
        }

        public static Rect GetPrimaryWorkArea()
        {
            Rect primary = Rect.Empty;
            try
            {
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate(IntPtr hMon, IntPtr hdc, ref RECT rc, IntPtr data)
                {
                    MONITORINFOEX mi = new MONITORINFOEX();
                    mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                    if (GetMonitorInfo(hMon, ref mi))
                    {
                        if ((mi.dwFlags & MONITORINFOF_PRIMARY) != 0)
                        {
                            double w = mi.rcWork.Right - mi.rcWork.Left;
                            double h = mi.rcWork.Bottom - mi.rcWork.Top;
                            if (w > 0 && h > 0)
                            {
                                primary = new Rect(mi.rcWork.Left, mi.rcWork.Top, w, h);
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch
            {
            }

            if (primary.IsEmpty || primary.Width <= 0 || primary.Height <= 0)
            {
                List<Rect> all = GetScreenWorkAreas();
                primary = all.Count > 0 ? all[0] : new Rect(0, 0, 1920, 1080);
            }
            return primary;
        }

        public static bool IsCompletelyOutsideScreens(Rect windowRect, IEnumerable<Rect> screenWorkAreas)
        {
            if (screenWorkAreas == null)
            {
                return true;
            }

            foreach (Rect screen in screenWorkAreas)
            {
                if (screen.Width <= 0 || screen.Height <= 0)
                {
                    continue;
                }

                Rect intersection = Rect.Intersect(windowRect, screen);
                if (!intersection.IsEmpty && intersection.Width > 0 && intersection.Height > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static Rect FindBestScreen(Rect windowRect, IEnumerable<Rect> screenWorkAreas)
        {
            Rect bestScreen = Rect.Empty;
            double maxArea = -1.0;

            if (screenWorkAreas != null)
            {
                foreach (Rect screen in screenWorkAreas)
                {
                    if (bestScreen.IsEmpty)
                    {
                        bestScreen = screen;
                    }

                    Rect intersection = Rect.Intersect(windowRect, screen);
                    if (!intersection.IsEmpty && intersection.Width > 0 && intersection.Height > 0)
                    {
                        double area = intersection.Width * intersection.Height;
                        if (area > maxArea)
                        {
                            maxArea = area;
                            bestScreen = screen;
                        }
                    }
                }
            }

            if (bestScreen.IsEmpty || bestScreen.Width <= 0 || bestScreen.Height <= 0)
            {
                bestScreen = GetPrimaryWorkArea();
            }

            return bestScreen;
        }
    }
}
