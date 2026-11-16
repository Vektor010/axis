using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public double RamUsedGb { get; set; }
        public double RamTotalGb { get; set; }
        public double RamLoadPercent { get; set; }
        public long PingMs { get; set; }
        public string PingDisplay { get; set; }
        public string TempDisplay { get; set; }
        public string TempStatus { get; set; }

        public SystemMetrics()
        {
            this.CpuLoadPercent = 0.0;
            this.RamUsedGb = 0.0;
            this.RamTotalGb = 0.0;
            this.RamLoadPercent = 0.0;
            this.PingMs = -1;
            this.PingDisplay = "Нет данных";
            this.TempDisplay = "Нет данных";
            this.TempStatus = "Датчик недоступен без драйвера";
        }
    }

    public class SystemInfoService
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

        private long _prevIdleTime;
        private long _prevKernelTime;
        private long _prevUserTime;
        private bool _hasPrevTimes;

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

            // 4. Температура GPU через NVML
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
                    m.TempStatus = "GPU (NVIDIA)";
                }
                else
                {
                    m.TempDisplay = "Нет данных";
                    m.TempStatus = "Датчик недоступен";
                }
            }
            catch
            {
                m.TempDisplay = "Нет данных";
                m.TempStatus = "Датчик недоступен";
            }

            return m;
        }
    }
}
