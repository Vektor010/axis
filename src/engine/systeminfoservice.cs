using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace WinTweaker.Engine
{
    public class SystemSpecs
    {
        public string Cpu { get; set; }
        public string Motherboard { get; set; }
        public string Gpu { get; set; }
        public string TotalRam { get; set; }
        public string Disks { get; set; }
        public List<string> DisksList { get; set; }
        public string Os { get; set; }
        public string Gateway { get; set; }

        public SystemSpecs()
        {
            this.Cpu = "Нет данных";
            this.Motherboard = "Нет данных";
            this.Gpu = "Нет данных";
            this.TotalRam = "Нет данных";
            this.Disks = "Нет данных";
            this.DisksList = new List<string>();
            this.Os = "Нет данных";
            this.Gateway = "Нет данных";
        }
    }

    public class SystemMetrics
    {
        public double CpuLoadPercent { get; set; }
        public string CpuTempDisplay { get; set; }
        public double GpuLoadPercent { get; set; }
        public bool IsGpuLoadAvailable { get; set; }
        public string GpuLoadDisplay { get; set; }
        public double RamUsedGb { get; set; }
        public double RamTotalGb { get; set; }
        public double RamLoadPercent { get; set; }
        public double DiskActivityPercent { get; set; }
        public string DiskActivityDisplay { get; set; }
        public string DiskSpaceDisplay { get; set; }
        public double NetworkRxKbPerSec { get; set; }
        public double NetworkTxKbPerSec { get; set; }
        public string NetworkRateDisplay { get; set; }
        public string UptimeDisplay { get; set; }
        public int ProcessCount { get; set; }
        public long PingMs { get; set; }
        public string PingDisplay { get; set; }
        public string TempDisplay { get; set; }
        public string TempStatus { get; set; }
        public string LastUpdatedDisplay { get; set; }

        public SystemMetrics()
        {
            this.CpuLoadPercent = 0.0;
            this.CpuTempDisplay = "Датчик недоступен";
            this.GpuLoadPercent = 0.0;
            this.IsGpuLoadAvailable = false;
            this.GpuLoadDisplay = "Недоступно";
            this.RamUsedGb = 0.0;
            this.RamTotalGb = 0.0;
            this.RamLoadPercent = 0.0;
            this.DiskActivityPercent = 0.0;
            this.DiskActivityDisplay = "Недоступно";
            this.DiskSpaceDisplay = "Нет данных";
            this.NetworkRxKbPerSec = 0.0;
            this.NetworkTxKbPerSec = 0.0;
            this.NetworkRateDisplay = "0 КБ/с";
            this.UptimeDisplay = "—";
            this.ProcessCount = 0;
            this.PingMs = -1;
            this.PingDisplay = "Нет данных";
            this.TempDisplay = "Нет данных";
            this.TempStatus = "Датчик недоступен";
            this.LastUpdatedDisplay = DateTime.Now.ToString("HH:mm:ss");
        }
    }

    public class SystemInfoService : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetSystemTimes(out long lpIdleTime, out long lpKernelTime, out long lpUserTime);

        [DllImport("kernel32.dll")]
        private static extern ulong GetTickCount64();

        private long _prevIdleTime;
        private long _prevKernelTime;
        private long _prevUserTime;
        private bool _hasPrevTimes;

        private long _prevNetBytesRx;
        private long _prevNetBytesTx;
        private DateTime _prevNetSampleUtc = DateTime.MinValue;

        private bool _cpuTempProbed = false;
        private bool _cpuTempSupported = false;
        private PerformanceCounter _diskCounter = null;
        private bool _diskCounterFailed = false;
        private bool _isDisposed = false;

        private SystemSpecs _cachedSpecs;
        private ITemperatureSource _tempSource;

        public SystemInfoService(ITemperatureSource tempSource = null)
        {
            this._hasPrevTimes = false;
            this._tempSource = tempSource;
        }

        public SystemSpecs GetOrCollectSpecs()
        {
            if (this._cachedSpecs != null)
            {
                return this._cachedSpecs;
            }

            SystemSpecs specs = new SystemSpecs();

            // 1. Процессор
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    if (key != null)
                    {
                        object nameVal = key.GetValue("ProcessorNameString");
                        if (nameVal != null)
                        {
                            string cpuName = nameVal.ToString().Trim();
                            cpuName = System.Text.RegularExpressions.Regex.Replace(cpuName, @"\s+\d+-Core\s+Processor", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                            specs.Cpu = string.Format("{0} ({1} потоков)", cpuName, Environment.ProcessorCount);
                        }
                    }
                }
            }
            catch
            {
                specs.Cpu = "Нет данных";
            }

            // 2. Материнская плата
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS"))
                {
                    if (key != null)
                    {
                        object mfg = key.GetValue("BaseBoardManufacturer");
                        object prod = key.GetValue("BaseBoardProduct");
                        string mfgStr = mfg != null ? mfg.ToString().Trim() : "";
                        string prodStr = prod != null ? prod.ToString().Trim() : "";
                        string combined = (mfgStr + " " + prodStr).Trim();
                        specs.Motherboard = string.IsNullOrEmpty(combined) ? "Нет данных" : combined;
                    }
                }
            }
            catch
            {
                specs.Motherboard = "Нет данных";
            }

            // 3. Видеокарта
            try
            {
                List<string> gpuList = new List<string>();
                string gpuClassKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
                using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(gpuClassKey))
                {
                    if (baseKey != null)
                    {
                        string[] subKeyNames = baseKey.GetSubKeyNames();
                        for (int i = 0; i < subKeyNames.Length; i++)
                        {
                            string subName = subKeyNames[i];
                            if (subName.Length == 4)
                            {
                                using (RegistryKey subKey = baseKey.OpenSubKey(subName))
                                {
                                    if (subKey != null)
                                    {
                                        object descVal = subKey.GetValue("DriverDesc");
                                        if (descVal != null && !string.IsNullOrEmpty(descVal.ToString().Trim()))
                                        {
                                            string desc = descVal.ToString().Trim();
                                            object memVal = subKey.GetValue("HardwareInformation.qwMemorySize");
                                            if (memVal == null)
                                            {
                                                memVal = subKey.GetValue("HardwareInformation.MemorySize");
                                            }

                                            string memStr = "";
                                            if (memVal != null)
                                            {
                                                try
                                                {
                                                    long bytes = Convert.ToInt64(memVal);
                                                    double gb = (double)bytes / (1024.0 * 1024.0 * 1024.0);
                                                    if (gb > 0)
                                                    {
                                                        memStr = string.Format(" ({0:F1} ГБ)", gb);
                                                    }
                                                }
                                                catch { }
                                            }
                                            gpuList.Add(desc + memStr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (gpuList.Count > 0)
                {
                    specs.Gpu = string.Join("; ", gpuList.ToArray());
                }
                else
                {
                    specs.Gpu = "Нет данных";
                }
            }
            catch
            {
                specs.Gpu = "Нет данных";
            }

            // 4. Память
            try
            {
                MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
                mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (GlobalMemoryStatusEx(ref mem))
                {
                    double totalGb = (double)mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    specs.TotalRam = string.Format("{0:F1} ГБ", totalGb);
                }
            }
            catch
            {
                specs.TotalRam = "Нет данных";
            }

            // 5. Диски
            try
            {
                List<string> diskList = new List<string>();
                DriveInfo[] drives = DriveInfo.GetDrives();
                for (int i = 0; i < drives.Length; i++)
                {
                    DriveInfo d = drives[i];
                    if (d.DriveType == DriveType.Fixed && d.IsReady)
                    {
                        double freeGb = (double)d.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                        double totalGb = (double)d.TotalSize / (1024.0 * 1024.0 * 1024.0);
                        string name = d.Name.TrimEnd('\\', ':');
                                                string diskStr = string.Format("{0}: {1:F0} ГБ свободно из {2:F0} ГБ", name, freeGb, totalGb);
                        diskList.Add(diskStr);
                        specs.DisksList.Add(diskStr);
                    }
                }

                if (diskList.Count > 0)
                {
                    specs.Disks = string.Join(", ", diskList.ToArray());
                }
                else
                {
                    specs.Disks = "Нет данных";
                }
            }
            catch
            {
                specs.Disks = "Нет данных";
            }

            // 6. ОС
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        object prod = key.GetValue("ProductName");
                        object disp = key.GetValue("DisplayVersion");
                        object build = key.GetValue("CurrentBuild");
                        object ubr = key.GetValue("UBR");

                        string pStr = prod != null ? prod.ToString().Trim() : "Windows";
                        string dStr = disp != null ? disp.ToString().Trim() : "";
                        string bStr = build != null ? build.ToString().Trim() : "";
                        string uStr = ubr != null ? ubr.ToString().Trim() : "";

                        int buildNum = 0;
                        if (int.TryParse(bStr, out buildNum) && buildNum >= 22000)
                        {
                            if (pStr.StartsWith("Windows 10"))
                            {
                                pStr = "Windows 11" + pStr.Substring("Windows 10".Length);
                            }
                        }

                        StringBuilder sb = new StringBuilder();
                        sb.Append(pStr);
                        if (!string.IsNullOrEmpty(dStr))
                        {
                            sb.Append(" ").Append(dStr);
                        }
                        if (!string.IsNullOrEmpty(bStr))
                        {
                            sb.Append(" (Сборка ").Append(bStr);
                            if (!string.IsNullOrEmpty(uStr))
                            {
                                sb.Append(".").Append(uStr);
                            }
                            sb.Append(")");
                        }
                        specs.Os = sb.ToString();
                    }
                }
            }
            catch
            {
                specs.Os = "Нет данных";
            }

            // 7. Шлюз
            try
            {
                specs.Gateway = this.GetDefaultGatewayAddress();
            }
            catch
            {
                specs.Gateway = "Нет данных";
            }

            this._cachedSpecs = specs;
            return specs;
        }

        public string GetDefaultGatewayAddress()
        {
            try
            {
                NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
                for (int i = 0; i < ifaces.Length; i++)
                {
                    NetworkInterface ni = ifaces[i];
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        GatewayIPAddressInformationCollection gateways = ni.GetIPProperties().GatewayAddresses;
                        for (int g = 0; g < gateways.Count; g++)
                        {
                            if (gateways[g].Address != null)
                            {
                                string addr = gateways[g].Address.ToString();
                                if (!string.IsNullOrEmpty(addr) && addr != "0.0.0.0" && !addr.StartsWith("127."))
                                {
                                    return addr;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return "Нет данных";
        }

        public SystemMetrics CollectLiveMetrics(string customPingHost = null)
        {
            SystemMetrics m = new SystemMetrics();

            // 1. Нагрузка ЦП через GetSystemTimes
            try
            {
                long idle, kernel, user;
                if (GetSystemTimes(out idle, out kernel, out user))
                {
                    if (this._hasPrevTimes)
                    {
                        long sysDiff = (kernel - this._prevKernelTime) + (user - this._prevUserTime);
                        long idleDiff = idle - this._prevIdleTime;
                        if (sysDiff > 0)
                        {
                            double cpu = (1.0 - (double)idleDiff / (double)sysDiff) * 100.0;
                            m.CpuLoadPercent = Math.Max(0.0, Math.Min(100.0, cpu));
                        }
                    }
                    this._prevIdleTime = idle;
                    this._prevKernelTime = kernel;
                    this._prevUserTime = user;
                    this._hasPrevTimes = true;
                }
            }
            catch
            {
                m.CpuLoadPercent = 0.0;
            }

            // 2. Память через GlobalMemoryStatusEx
            try
            {
                MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
                mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (GlobalMemoryStatusEx(ref mem))
                {
                    double totalGb = (double)mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    double availGb = (double)mem.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    m.RamTotalGb = totalGb;
                    m.RamUsedGb = Math.Max(0.0, totalGb - availGb);
                    m.RamLoadPercent = (double)mem.dwMemoryLoad;
                }
            }
            catch { }

            // 3. Задержка Ping
            string hostToPing = customPingHost;
            bool isAutoGateway = false;
            if (string.IsNullOrEmpty(hostToPing))
            {
                hostToPing = this.GetDefaultGatewayAddress();
                isAutoGateway = true;
            }

            if (!string.IsNullOrEmpty(hostToPing) && hostToPing != "Нет данных")
            {
                try
                {
                    using (Ping pinger = new Ping())
                    {
                        PingReply reply = null;
                        try { reply = pinger.Send(hostToPing, 1000); } catch { }

                        if ((reply == null || reply.Status != IPStatus.Success) && isAutoGateway)
                        {
                            // Fallback to fast public DNS resolver if gateway drops ICMP
                            try { reply = pinger.Send("1.1.1.1", 800); } catch { }
                            if (reply == null || reply.Status != IPStatus.Success)
                            {
                                try { reply = pinger.Send("8.8.8.8", 800); } catch { }
                            }
                        }

                        if (reply != null && reply.Status == IPStatus.Success)
                        {
                            m.PingMs = reply.RoundtripTime;
                            m.PingDisplay = reply.RoundtripTime == 0 ? "< 1 мс" : string.Format("{0} мс", reply.RoundtripTime);
                        }
                        else
                        {
                            m.PingMs = -1;
                            m.PingDisplay = "Нет данных";
                        }
                    }
                }
                catch
                {
                    m.PingMs = -1;
                    m.PingDisplay = "Нет данных";
                }
            }
            else
            {
                m.PingMs = -1;
                m.PingDisplay = "Нет данных";
            }

            // 4. Температура GPU и загрузка GPU через NVML (если установлен драйвер NVIDIA)
            try
            {
                if (this._tempSource == null)
                {
                    this._tempSource = new NvmlTemperatureSource();
                }
                GpuTemperatureResult tempRes = this._tempSource.GetGpuTemperature();
                if (tempRes.IsAvailable)
                {
                    m.TempDisplay = string.Format("{0}°C", tempRes.TemperatureC);
                    m.TempStatus = "GPU (NVIDIA NVML)";
                }
                else
                {
                    m.TempDisplay = "Нет данных";
                    m.TempStatus = "Датчик недоступен";
                }

                NvmlTemperatureSource nvml = this._tempSource as NvmlTemperatureSource;
                uint gpuUtil;
                if (nvml != null && nvml.TryGetGpuUtilization(out gpuUtil))
                {
                    m.IsGpuLoadAvailable = true;
                    m.GpuLoadPercent = gpuUtil;
                    m.GpuLoadDisplay = string.Format("{0}%", gpuUtil);
                }
            }
            catch
            {
                m.TempDisplay = "Нет данных";
                m.TempStatus = "Датчик недоступен";
            }

            // 5. Температура ЦП через ACPI ThermalZone (честный отказ при отсутствии датчика)
            m.CpuTempDisplay = this.QueryCpuThermalZoneSafe();

            // 6. Активность и занятость дисков
            m.DiskActivityDisplay = this.QueryDiskActivitySafe(m);
            m.DiskSpaceDisplay = this.QuerySystemDriveUsage();

            // 7. Сетевая активность (RX / TX КБ/с)
            this.QueryNetworkThroughput(m);

            // 8. Аптайм ОС и число активных процессов (без утечки Win32 handles)
            try
            {
                TimeSpan up = TimeSpan.FromMilliseconds(GetTickCount64());
                m.UptimeDisplay = up.Days > 0
                    ? string.Format("{0}д {1:D2}:{2:D2}:{3:D2}", up.Days, up.Hours, up.Minutes, up.Seconds)
                    : string.Format("{0:D2}:{1:D2}:{2:D2}", up.Hours, up.Minutes, up.Seconds);
            }
            catch { }

            try
            {
                Process[] procs = Process.GetProcesses();
                m.ProcessCount = procs.Length;
                for (int i = 0; i < procs.Length; i++)
                {
                    try { procs[i].Dispose(); } catch { }
                }
            }
            catch { m.ProcessCount = 0; }

            m.LastUpdatedDisplay = DateTime.Now.ToString("HH:mm:ss");
            return m;
        }

        private string QueryCpuThermalZoneSafe()
        {
            if (this._cpuTempProbed && !this._cpuTempSupported) return "Датчик недоступен";
            try
            {
                double maxC = -1.0;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature"))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject obj in results)
                    {
                        using (obj)
                        {
                            object raw = obj["CurrentTemperature"];
                            if (raw != null)
                            {
                                double celsius = (Convert.ToDouble(raw) / 10.0) - 273.15;
                                if (celsius > 5.0 && celsius < 130.0 && celsius > maxC)
                                {
                                    maxC = celsius;
                                }
                            }
                        }
                    }
                }
                this._cpuTempProbed = true;
                if (maxC > 0)
                {
                    this._cpuTempSupported = true;
                    return string.Format("{0:F0}°C", maxC);
                }
                this._cpuTempSupported = false;
            }
            catch
            {
                this._cpuTempProbed = true;
                this._cpuTempSupported = false;
            }
            return "Датчик недоступен";
        }

        private string QueryDiskActivitySafe(SystemMetrics metrics)
        {
            if (this._diskCounterFailed) return "Недоступно";
            try
            {
                if (this._diskCounter == null)
                {
                    this._diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total", true);
                    this._diskCounter.NextValue();
                    return "Сбор...";
                }
                double val = Math.Max(0.0, Math.Min(100.0, this._diskCounter.NextValue()));
                metrics.DiskActivityPercent = val;
                return string.Format("{0:F0}%", val);
            }
            catch
            {
                this._diskCounterFailed = true;
                if (this._diskCounter != null)
                {
                    try { this._diskCounter.Dispose(); } catch { }
                    this._diskCounter = null;
                }
                return "Недоступно";
            }
        }

        private string QuerySystemDriveUsage()
        {
            try
            {
                string sysRoot = Path.GetPathRoot(Environment.SystemDirectory);
                DriveInfo d = new DriveInfo(sysRoot);
                if (d.IsReady && d.TotalSize > 0)
                {
                    double usedGb = (double)(d.TotalSize - d.TotalFreeSpace) / (1024.0 * 1024.0 * 1024.0);
                    double totalGb = (double)d.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    double pct = usedGb * 100.0 / totalGb;
                    return string.Format("{0} {1:F0}% ({2:F0}/{3:F0} ГБ)", d.Name.TrimEnd('\\'), pct, usedGb, totalGb);
                }
            }
            catch { }
            return "Нет данных";
        }

        private void QueryNetworkThroughput(SystemMetrics metrics)
        {
            try
            {
                long totalRx = 0;
                long totalTx = 0;
                NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
                for (int i = 0; i < ifaces.Length; i++)
                {
                    NetworkInterface ni = ifaces[i];
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
                        totalRx += stats.BytesReceived;
                        totalTx += stats.BytesSent;
                    }
                }

                DateTime now = DateTime.UtcNow;
                if (this._prevNetSampleUtc != DateTime.MinValue)
                {
                    double sec = (now - this._prevNetSampleUtc).TotalSeconds;
                    if (sec >= 0.25)
                    {
                        double rxKb = Math.Max(0.0, (totalRx - this._prevNetBytesRx) / 1024.0 / sec);
                        double txKb = Math.Max(0.0, (totalTx - this._prevNetBytesTx) / 1024.0 / sec);
                        metrics.NetworkRxKbPerSec = rxKb;
                        metrics.NetworkTxKbPerSec = txKb;
                        metrics.NetworkRateDisplay = string.Format("↓ {0}  ↑ {1}", FormatRate(rxKb), FormatRate(txKb));
                    }
                }
                this._prevNetBytesRx = totalRx;
                this._prevNetBytesTx = totalTx;
                this._prevNetSampleUtc = now;
            }
            catch
            {
                metrics.NetworkRateDisplay = "Недоступно";
            }
        }

        private static string FormatRate(double kbPerSec)
        {
            if (kbPerSec >= 1024.0) return string.Format("{0:F1} МБ/с", kbPerSec / 1024.0);
            return string.Format("{0:F0} КБ/с", kbPerSec);
        }

        public void Dispose()
        {
            if (this._isDisposed) return;
            this._isDisposed = true;
            if (this._diskCounter != null)
            {
                try { this._diskCounter.Dispose(); } catch { }
                this._diskCounter = null;
            }
            if (this._tempSource != null)
            {
                try { this._tempSource.Dispose(); } catch { }
                this._tempSource = null;
            }
        }
    }
}
