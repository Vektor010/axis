Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public class WnfChecker
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

    public static uint QueryU32(ulong stateName, out int status)
    {
        uint changeStamp = 0;
        uint bufferSize = 8;
        IntPtr buf = Marshal.AllocHGlobal(8);
        try
        {
            status = NtQueryWnfStateData(ref stateName, IntPtr.Zero, IntPtr.Zero, out changeStamp, buf, ref bufferSize);
            if (status >= 0 && bufferSize >= 4)
            {
                return (uint)Marshal.ReadInt32(buf);
            }
            return 0;
        }
        finally
        {
            Marshal.FreeHGlobal(buf);
        }
    }
}
"@

$status1 = 0
$pidVal = [WnfChecker]::QueryU32([WnfChecker]::WNF_RM_GAME_MODE_ACTIVE, [ref]$status1)
Write-Host "WNF_RM_GAME_MODE_ACTIVE PID: $pidVal (NTSTATUS: 0x$($status1.ToString('X8')))"

$status2 = 0
$pwrVal = [WnfChecker]::QueryU32([WnfChecker]::WNF_SEB_GAME_MODE, [ref]$status2)
Write-Host "WNF_SEB_GAME_MODE PowerLow: $pwrVal (NTSTATUS: 0x$($status2.ToString('X8')))"
