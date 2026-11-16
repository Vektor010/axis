using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

class Program
{
    [DllImport("avrt.dll", SetLastError = true)]
    public static extern bool AvQuerySystemResponsiveness(IntPtr AvrtHandle, out uint SystemResponsivenessValue);

    [DllImport("avrt.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr AvSetMmThreadCharacteristicsW(string TaskName, ref uint TaskIndex);

    [DllImport("avrt.dll", SetLastError = true)]
    public static extern bool AvRevertMmThreadCharacteristics(IntPtr AvrtHandle);

    [DllImport("avrt.dll", SetLastError = true)]
    public static extern bool AvSetMmThreadPriority(IntPtr AvrtHandle, int Priority);

    static void Main()
    {
        uint resp = 0;
        bool ok = AvQuerySystemResponsiveness(IntPtr.Zero, out resp);
        Console.WriteLine("AvQuerySystemResponsiveness: ok=" + ok + ", SystemResponsiveness=" + resp + "%");

        string[] tasks = new[] { "Audio", "Pro Audio", "Games", "Playback", "Capture", "Distribution", "Window Manager" };

        foreach (var task in tasks)
        {
            uint taskIndex = 0;
            IntPtr h = AvSetMmThreadCharacteristicsW(task, ref taskIndex);
            int err = Marshal.GetLastWin32Error();
            if (h != IntPtr.Zero)
            {
                bool setPrioOk = AvSetMmThreadPriority(h, 1);
                Console.WriteLine("Task '" + task + "': HANDLE=" + h.ToInt64().ToString("X") + ", TaskIndex=" + taskIndex + ", SetPriority(1)=" + setPrioOk);
                AvRevertMmThreadCharacteristics(h);
            }
            else
            {
                Console.WriteLine("Task '" + task + "': FAILED, Error=" + err);
            }
        }

        // Also inspect registry configuration
        using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"))
        {
            if (key != null)
            {
                object sr = key.GetValue("SystemResponsiveness");
                object nti = key.GetValue("NetworkThrottlingIndex");
                object nlm = key.GetValue("NoLazyMode");
                Console.WriteLine("SystemProfile: SystemResponsiveness=" + (sr ?? "default(20)") + ", NetworkThrottlingIndex=" + (nti ?? "default(10)") + ", NoLazyMode=" + (nlm ?? "0"));
            }
        }
    }
}
