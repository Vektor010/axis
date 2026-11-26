using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        public double CpuClockGhz { get; set; }
        public double CpuPerfPercent { get; set; }
        public string CpuClockDisplay { get; set; }
        public bool IsCpuTempAvailable { get; set; }
        public double CpuTempC { get; set; }
        public string CpuTempDisplay { get; set; }
        public double GpuLoadPercent { get; set; }
        public bool IsGpuLoadAvailable { get; set; }
        public string GpuLoadDisplay { get; set; }
        public string GpuDetailDisplay { get; set; }
        public double RamUsedGb { get; set; }
        public double RamFreeGb { get; set; }
        public double RamTotalGb { get; set; }
        public double RamLoadPercent { get; set; }
        public double DiskActivityPercent { get; set; }
        public string DiskActivityDisplay { get; set; }
        public string DiskSpaceDisplay { get; set; }
        public double NetworkRxKbPerSec { get; set; }
        public double NetworkTxKbPerSec { get; set; }
        public string NetworkRateDisplay { get; set; }
        public string NetworkDetailDisplay { get; set; }
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
            this.CpuClockGhz = 0.0;
            this.CpuPerfPercent = 100.0;
            this.CpuClockDisplay = "—";
            this.IsCpuTempAvailable = false;
            this.CpuTempC = 0.0;
            this.CpuTempDisplay = "";
            this.GpuLoadPercent = 0.0;
            this.IsGpuLoadAvailable = false;
            this.GpuLoadDisplay = "Недоступно";
            this.GpuDetailDisplay = "Опрос видеокарты...";
            this.RamUsedGb = 0.0;
            this.RamFreeGb = 0.0;
            this.RamTotalGb = 0.0;
            this.RamLoadPercent = 0.0;
            this.DiskActivityPercent = 0.0;
            this.DiskActivityDisplay = "Недоступно";
            this.DiskSpaceDisplay = "Нет данных";
            this.NetworkRxKbPerSec = 0.0;
            this.NetworkTxKbPerSec = 0.0;
            this.NetworkRateDisplay = "↓ 0 КБ/с  ↑ 0 КБ/с";
            this.NetworkDetailDisplay = "Активный канал";
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

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSOR_POWER_INFORMATION
        {
            public uint Number;
            public uint MaxMhz;
            public uint CurrentMhz;
            public uint MhzLimit;
            public uint MaxIdleState;
            public uint CurrentIdleState;
        }

        [DllImport("powrprof.dll")]
        private static extern uint CallNtPowerInformation(
            int InformationLevel,
            IntPtr InputBuffer,
            uint InputBufferLength,
            IntPtr OutputBuffer,
            uint OutputBufferLength);

        private long _prevIdleTime;
        private long _prevKernelTime;
        private long _prevUserTime;
        private bool _hasPrevTimes;

        private long _prevNetBytesRx;
        private long _prevNetBytesTx;
        private DateTime _prevNetSampleUtc = DateTime.MinValue;
        private string _cachedNetAdapterName = "";

        private volatile string _cachedGateway = null;
        private DateTime _gatewayCacheUtc = DateTime.MinValue;
        private long _cachedPingMs = -1;
        private volatile string _cachedPingDisplay = "Нет данных";
        private DateTime _pingCacheUtc = DateTime.MinValue;
        private int _pingInFlight = 0;

        private bool _acpiTempProbed = false;
        private bool _acpiTempSupported = false;
        private bool _lhmTempProbed = false;
        private string _lhmNamespace = null;

        private bool _perfCountersInitialized = false;
        private PerformanceCounter _cpuUtilityCounter = null;
        private PerformanceCounter _cpuPerfCounter = null;
        private PerformanceCounter _cpuFreqCounter = null;
        private PerformanceCounter _diskCounter = null;
        private PerformanceCounter _processCounter = null;
        private bool _cpuCountersFailed = false;
        private bool _diskCounterFailed = false;
        private bool _processCounterFailed = false;
        private uint _cachedMaxCpuMhz = 0;
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

        public SystemMetrics CollectInstantStartupSnapshot(string customPingHost = null)
        {
            SystemMetrics m = new SystemMetrics();
            long wIdle, wKernel, wUser;
            if (!this._hasPrevTimes && GetSystemTimes(out wIdle, out wKernel, out wUser))
            {
                this._prevIdleTime = wIdle;
                this._prevKernelTime = wKernel;
                this._prevUserTime = wUser;
                this._hasPrevTimes = true;
            }
            long wRx, wTx;
            string wAdapter;
            if (this._prevNetSampleUtc == DateTime.MinValue && TrySumActiveNetworkBytes(out wRx, out wTx, out wAdapter))
            {
                this._prevNetBytesRx = wRx;
                this._prevNetBytesTx = wTx;
                this._cachedNetAdapterName = wAdapter;
                this._prevNetSampleUtc = DateTime.UtcNow;
            }

            uint baseMhz = this.QueryNativeCpuMaxMhz();
            if (baseMhz > 0)
            {
                m.CpuClockGhz = baseMhz / 1000.0;
                m.CpuClockDisplay = string.Format("{0:F2} ГГц", m.CpuClockGhz);
            }
            this.PopulateMemoryMetrics(m);
            this.TriggerBackgroundPingIfDue(customPingHost);
            m.PingMs = Interlocked.Read(ref this._cachedPingMs);
            m.PingDisplay = this._cachedPingDisplay;
            this.PopulateGpuMetrics(m);
            double cpuTempC;
            if (TryReadAfterburnerCpuTemp(out cpuTempC))
            {
                m.IsCpuTempAvailable = true;
                m.CpuTempC = cpuTempC;
                m.CpuTempDisplay = string.Format("{0:F0}°C", cpuTempC);
            }
            m.DiskActivityDisplay = "0%";
            m.DiskSpaceDisplay = this.QuerySystemDriveUsage();
            if (!string.IsNullOrEmpty(this._cachedNetAdapterName)) m.NetworkDetailDisplay = this._cachedNetAdapterName;
            PopulateUptime(m);
            m.LastUpdatedDisplay = DateTime.Now.ToString("HH:mm:ss");
            return m;
        }

        private void PopulateMemoryMetrics(SystemMetrics m)
        {
            try
            {
                MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
                mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                if (GlobalMemoryStatusEx(ref mem))
                {
                    double totalGb = (double)mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    double availGb = (double)mem.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    m.RamTotalGb = totalGb;
                    m.RamFreeGb = availGb;
                    m.RamUsedGb = Math.Max(0.0, totalGb - availGb);
                    m.RamLoadPercent = (double)mem.dwMemoryLoad;
                }
            }
            catch { }
        }

        private void PopulateGpuMetrics(SystemMetrics m)
        {
            try
            {
                if (this._tempSource == null)
                {
                    this._tempSource = new NvmlTemperatureSource();
                }

                NvmlTemperatureSource nvml = this._tempSource as NvmlTemperatureSource;
                GpuTelemetrySnapshot gpuSnap;
                if (nvml != null && nvml.TryGetGpuTelemetry(out gpuSnap) && gpuSnap.IsAvailable)
                {
                    m.TempDisplay = string.Format("{0}°C", gpuSnap.TemperatureC);
                    m.TempStatus = "GPU (NVIDIA NVML)";
                    m.IsGpuLoadAvailable = true;
                    m.GpuLoadPercent = gpuSnap.GpuLoadPercent;
                    m.GpuLoadDisplay = string.Format("{0}%", gpuSnap.GpuLoadPercent);

                    List<string> detailParts = new List<string>();
                    if (gpuSnap.CoreClockMhz > 0 && gpuSnap.MemClockMhz > 0)
                    {
                        detailParts.Add(string.Format("{0}/{1} МГц", gpuSnap.CoreClockMhz, gpuSnap.MemClockMhz));
                    }
                    if (gpuSnap.PowerWatts > 0.5)
                    {
                        detailParts.Add(string.Format("{0:F0} Вт", gpuSnap.PowerWatts));
                    }
                    if (gpuSnap.VramTotalGb > 0.5)
                    {
                        detailParts.Add(string.Format("{0:F1}/{1:F0} ГБ", gpuSnap.VramUsedGb, gpuSnap.VramTotalGb));
                    }
                    m.GpuDetailDisplay = detailParts.Count > 0 ? string.Join(" • ", detailParts.ToArray()) : "NVIDIA NVML";
                }
                else
                {
                    GpuTemperatureResult tempRes = this._tempSource.GetGpuTemperature();
                    if (tempRes.IsAvailable)
                    {
                        m.TempDisplay = string.Format("{0}°C", tempRes.TemperatureC);
                        m.TempStatus = "GPU (NVIDIA NVML)";
                        m.GpuDetailDisplay = "NVIDIA NVML";
                    }
                    else
                    {
                        m.TempDisplay = "Нет данных";
                        m.TempStatus = "GPU не обнаружена";
                        m.GpuDetailDisplay = tempRes.ErrorMessage;
                    }
                }
            }
            catch
            {
                m.TempDisplay = "Нет данных";
                m.TempStatus = "Датчик недоступен";
                m.GpuDetailDisplay = "Нет данных NVML";
            }
        }

        private static void PopulateUptime(SystemMetrics m)
        {
            try
            {
                TimeSpan up = TimeSpan.FromMilliseconds(GetTickCount64());
                m.UptimeDisplay = up.Days > 0
                    ? string.Format("{0}д {1:D2}:{2:D2}:{3:D2}", up.Days, up.Hours, up.Minutes, up.Seconds)
                    : string.Format("{0:D2}:{1:D2}:{2:D2}", up.Hours, up.Minutes, up.Seconds);
            }
            catch { }
        }

        public SystemMetrics CollectLiveMetrics(string customPingHost = null)
        {
            SystemMetrics m = new SystemMetrics();

            // 0. Однократная быстрая инициализация аппаратных счетчиков PDH (~60 мс при первом запуске, далее 0 мс/тик)
            bool needWarmup = !this._perfCountersInitialized ||
                              !this._hasPrevTimes ||
                              this._prevNetSampleUtc == DateTime.MinValue;
            if (needWarmup)
            {
                this.EnsurePerfCountersInitialized();
                long wIdle, wKernel, wUser;
                if (!this._hasPrevTimes && GetSystemTimes(out wIdle, out wKernel, out wUser))
                {
                    this._prevIdleTime = wIdle;
                    this._prevKernelTime = wKernel;
                    this._prevUserTime = wUser;
                    this._hasPrevTimes = true;
                }
                long wRx, wTx;
                string wAdapter;
                if (this._prevNetSampleUtc == DateTime.MinValue && TrySumActiveNetworkBytes(out wRx, out wTx, out wAdapter))
                {
                    this._prevNetBytesRx = wRx;
                    this._prevNetBytesTx = wTx;
                    this._cachedNetAdapterName = wAdapter;
                    this._prevNetSampleUtc = DateTime.UtcNow;
                }
                Thread.Sleep(110);
            }

            // 1. Загрузка ЦП (формула Диспетчера задач Windows: % Processor Utility) + запасной путь GetSystemTimes
            double cpuPct = -1.0;
            if (!this._cpuCountersFailed && this._cpuUtilityCounter != null)
            {
                try
                {
                    double util = this._cpuUtilityCounter.NextValue();
                    if (!double.IsNaN(util) && !double.IsInfinity(util))
                    {
                        cpuPct = Math.Max(0.0, Math.Min(100.0, util));
                    }
                }
                catch
                {
                    this._cpuCountersFailed = true;
                }
            }

            try
            {
                long idle, kernel, user;
                if (GetSystemTimes(out idle, out kernel, out user))
                {
                    if (cpuPct < 0.0 && this._hasPrevTimes)
                    {
                        long sysDiff = (kernel - this._prevKernelTime) + (user - this._prevUserTime);
                        long idleDiff = idle - this._prevIdleTime;
                        if (sysDiff > 0)
                        {
                            double cpu = (1.0 - (double)idleDiff / (double)sysDiff) * 100.0;
                            cpuPct = Math.Max(0.0, Math.Min(100.0, cpu));
                        }
                    }
                    this._prevIdleTime = idle;
                    this._prevKernelTime = kernel;
                    this._prevUserTime = user;
                    this._hasPrevTimes = true;
                }
            }
            catch { }
            m.CpuLoadPercent = cpuPct >= 0.0 ? cpuPct : 0.0;

            // 1.5 Реальная эффективная частота ЦП и Turbo Boost (% Processor Performance + CallNtPowerInformation fallback)
            double perfPct = 100.0;
            double baseMhz = this._cachedMaxCpuMhz;
            if (!this._cpuCountersFailed && this._cpuPerfCounter != null && this._cpuFreqCounter != null)
            {
                try
                {
                    double pVal = this._cpuPerfCounter.NextValue();
                    double fVal = this._cpuFreqCounter.NextValue();
                    if (fVal > 200.0 && pVal > 5.0)
                    {
                        perfPct = pVal;
                        baseMhz = fVal;
                    }
                }
                catch { }
            }
            if (baseMhz <= 0)
            {
                baseMhz = this.QueryNativeCpuMaxMhz();
            }
            if (baseMhz > 0)
            {
                double effMhz = baseMhz * (perfPct / 100.0);
                m.CpuPerfPercent = perfPct;
                m.CpuClockGhz = effMhz / 1000.0;
                m.CpuClockDisplay = string.Format("{0:F2} ГГц", m.CpuClockGhz);
            }

            // 2. Память через GlobalMemoryStatusEx
            this.PopulateMemoryMetrics(m);

            // 3. Неблокирующий фоновый Ping (обновляется в фоне раз в ~2.5 сек, не тормозит 500мс графики!)
            this.TriggerBackgroundPingIfDue(customPingHost);
            m.PingMs = Interlocked.Read(ref this._cachedPingMs);
            m.PingDisplay = this._cachedPingDisplay;

            // 4. Полная телеметрия GPU через NVML (температура, нагрузка, частоты, питание Вт, VRAM)
            this.PopulateGpuMetrics(m);

            // 5. Температура ЦП (каскад: MSI Afterburner SharedMem -> LHM/OHM WMI -> ACPI; без ошибок при отсутствии)
            double cpuTempC;
            if (this.TryQueryCpuTempSmart(out cpuTempC))
            {
                m.IsCpuTempAvailable = true;
                m.CpuTempC = cpuTempC;
                m.CpuTempDisplay = string.Format("{0:F0}°C", cpuTempC);
            }
            else
            {
                m.IsCpuTempAvailable = false;
                m.CpuTempDisplay = m.CpuClockDisplay;
            }

            // 6. Активность и занятость дисков
            m.DiskActivityDisplay = this.QueryDiskActivitySafe(m);
            m.DiskSpaceDisplay = this.QuerySystemDriveUsage();

            // 7. Сетевая активность (RX / TX КБ/с)
            this.QueryNetworkThroughput(m);

            // 8. Аптайм ОС и число активных процессов (через 0.02мс PDH без создания Process[] объектов)
            PopulateUptime(m);
            m.ProcessCount = this.QueryProcessCountFast();
            m.LastUpdatedDisplay = DateTime.Now.ToString("HH:mm:ss");
            return m;
        }

        private void EnsurePerfCountersInitialized()
        {
            if (this._perfCountersInitialized) return;
            this._perfCountersInitialized = true;

            try
            {
                this._cpuUtilityCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total", true);
                this._cpuPerfCounter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total", true);
                this._cpuFreqCounter = new PerformanceCounter("Processor Information", "Processor Frequency", "_Total", true);
                this._cpuUtilityCounter.NextValue();
                this._cpuPerfCounter.NextValue();
                this._cpuFreqCounter.NextValue();
            }
            catch
            {
                this._cpuCountersFailed = true;
            }

            if (this._diskCounter == null && !this._diskCounterFailed)
            {
                try
                {
                    this._diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total", true);
                    this._diskCounter.NextValue();
                }
                catch { this._diskCounterFailed = true; }
            }

            if (this._processCounter == null && !this._processCounterFailed)
            {
                try
                {
                    this._processCounter = new PerformanceCounter("System", "Processes", true);
                    this._processCounter.NextValue();
                }
                catch { this._processCounterFailed = true; }
            }
        }

        private uint QueryNativeCpuMaxMhz()
        {
            if (this._cachedMaxCpuMhz > 0) return this._cachedMaxCpuMhz;
            IntPtr buf = IntPtr.Zero;
            try
            {
                int sz = Marshal.SizeOf(typeof(PROCESSOR_POWER_INFORMATION));
                buf = Marshal.AllocHGlobal(sz * Math.Max(1, Environment.ProcessorCount));
                if (CallNtPowerInformation(11, IntPtr.Zero, 0, buf, (uint)(sz * Environment.ProcessorCount)) == 0)
                {
                    PROCESSOR_POWER_INFORMATION info = (PROCESSOR_POWER_INFORMATION)Marshal.PtrToStructure(buf, typeof(PROCESSOR_POWER_INFORMATION));
                    if (info.MaxMhz > 0) this._cachedMaxCpuMhz = info.MaxMhz;
                }
            }
            catch { }
            finally
            {
                if (buf != IntPtr.Zero) Marshal.FreeHGlobal(buf);
            }
            return this._cachedMaxCpuMhz;
        }

        private void TriggerBackgroundPingIfDue(string customPingHost)
        {
            if (string.Equals(customPingHost, "Нет данных", StringComparison.OrdinalIgnoreCase))
            {
                Interlocked.Exchange(ref this._cachedPingMs, -1);
                this._cachedPingDisplay = "Нет данных";
                return;
            }
            if ((DateTime.UtcNow - this._pingCacheUtc).TotalSeconds < 2.5) return;
            if (Interlocked.CompareExchange(ref this._pingInFlight, 1, 0) != 0) return;

            this._pingCacheUtc = DateTime.UtcNow;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string hostToPing = customPingHost;
                    bool isAutoGateway = false;
                    if (string.IsNullOrEmpty(hostToPing))
                    {
                        if (string.IsNullOrEmpty(this._cachedGateway) || (DateTime.UtcNow - this._gatewayCacheUtc).TotalSeconds > 30.0)
                        {
                            this._cachedGateway = this.GetDefaultGatewayAddress();
                            this._gatewayCacheUtc = DateTime.UtcNow;
                        }
                        hostToPing = this._cachedGateway;
                        isAutoGateway = true;
                    }

                    if (!string.IsNullOrEmpty(hostToPing) && hostToPing != "Нет данных")
                    {
                        using (Ping pinger = new Ping())
                        {
                            PingReply reply = null;
                            try { reply = pinger.Send(hostToPing, 250); } catch { }
                            if ((reply == null || reply.Status != IPStatus.Success) && isAutoGateway)
                            {
                                try { reply = pinger.Send("1.1.1.1", 250); } catch { }
                            }

                            if (reply != null && reply.Status == IPStatus.Success)
                            {
                                Interlocked.Exchange(ref this._cachedPingMs, reply.RoundtripTime);
                                this._cachedPingDisplay = reply.RoundtripTime == 0 ? "< 1 мс" : string.Format("{0} мс", reply.RoundtripTime);
                            }
                            else
                            {
                                Interlocked.Exchange(ref this._cachedPingMs, -1);
                                this._cachedPingDisplay = "Нет ответа";
                            }
                        }
                    }
                    else
                    {
                        Interlocked.Exchange(ref this._cachedPingMs, -1);
                        this._cachedPingDisplay = "Нет данных";
                    }
                }
                catch
                {
                    Interlocked.Exchange(ref this._cachedPingMs, -1);
                    this._cachedPingDisplay = "Нет данных";
                }
                finally
                {
                    Interlocked.Exchange(ref this._pingInFlight, 0);
                }
            });
        }

        private int QueryProcessCountFast()
        {
            if (!this._processCounterFailed && this._processCounter != null)
            {
                try
                {
                    int val = (int)Math.Round(this._processCounter.NextValue());
                    if (val > 0) return val;
                }
                catch { this._processCounterFailed = true; }
            }
            try
            {
                Process[] procs = Process.GetProcesses();
                int count = procs.Length;
                for (int i = 0; i < procs.Length; i++)
                {
                    try { procs[i].Dispose(); } catch { }
                }
                return count;
            }
            catch { return 0; }
        }

        private bool TryQueryCpuTempSmart(out double tempC)
        {
            tempC = 0.0;

            // 1. MSI Afterburner Shared Memory (< 0.05 мс, без драйверов и WMI)
            if (TryReadAfterburnerCpuTemp(out tempC))
            {
                return true;
            }

            // 2. LibreHardwareMonitor / OpenHardwareMonitor WMI (только если уже запущены у пользователя)
            if (!this._lhmTempProbed)
            {
                this._lhmTempProbed = true;
                string[] nsList = new[] { @"root\LibreHardwareMonitor", @"root\OpenHardwareMonitor" };
                for (int n = 0; n < nsList.Length; n++)
                {
                    if (TryReadLhmCpuTemp(nsList[n], out tempC))
                    {
                        this._lhmNamespace = nsList[n];
                        return true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(this._lhmNamespace))
            {
                if (TryReadLhmCpuTemp(this._lhmNamespace, out tempC)) return true;
            }

            // 3. ACPI ThermalZone WMI (проверяется 1 раз и кэшируется при отсутствии на десктопной плате)
            if (this._acpiTempProbed && !this._acpiTempSupported) return false;
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
                this._acpiTempProbed = true;
                if (maxC > 0)
                {
                    this._acpiTempSupported = true;
                    tempC = maxC;
                    return true;
                }
                this._acpiTempSupported = false;
            }
            catch
            {
                this._acpiTempProbed = true;
                this._acpiTempSupported = false;
            }
            return false;
        }

        private static bool TryReadAfterburnerCpuTemp(out double tempC)
        {
            tempC = 0.0;
            try
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("MAHMSharedMemory", MemoryMappedFileRights.Read))
                using (MemoryMappedViewAccessor acc = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
                {
                    byte[] hdr = new byte[24];
                    acc.ReadArray(0, hdr, 0, 24);
                    uint sig = BitConverter.ToUInt32(hdr, 0);
                    uint hdrSize = BitConverter.ToUInt32(hdr, 8);
                    uint numEntries = BitConverter.ToUInt32(hdr, 12);
                    uint entrySize = BitConverter.ToUInt32(hdr, 16);
                    if (sig != 0x4D41484D && sig != 0x4D48414D) return false;
                    if (hdrSize < 24 || entrySize < 1304 || numEntries == 0 || numEntries > 256) return false;

                    byte[] allEntries = new byte[numEntries * entrySize];
                    acc.ReadArray(hdrSize, allEntries, 0, allEntries.Length);

                    double fallbackCoreMax = 0.0;
                    for (int i = 0; i < (int)numEntries; i++)
                    {
                        int off = i * (int)entrySize;
                        int nameLen = 0;
                        while (nameLen < 260 && allEntries[off + nameLen] != 0) nameLen++;
                        string name = Encoding.ASCII.GetString(allEntries, off, nameLen);
                        if (name.IndexOf("CPU", StringComparison.OrdinalIgnoreCase) >= 0 &&
                            name.IndexOf("temperature", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            float val = BitConverter.ToSingle(allEntries, off + 1300);
                            if (val > 5.0f && val < 130.0f)
                            {
                                if (string.Equals(name, "CPU temperature", StringComparison.OrdinalIgnoreCase))
                                {
                                    tempC = val;
                                    return true;
                                }
                                if (val > fallbackCoreMax) fallbackCoreMax = val;
                            }
                        }
                    }
                    if (fallbackCoreMax > 0.0)
                    {
                        tempC = fallbackCoreMax;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool TryReadLhmCpuTemp(string wmiNamespace, out double tempC)
        {
            tempC = 0.0;
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiNamespace, "SELECT Name, Value, SensorType FROM Sensor WHERE SensorType='Temperature'"))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject obj in results)
                    {
                        using (obj)
                        {
                            string name = obj["Name"] != null ? obj["Name"].ToString() : "";
                            if (name.IndexOf("CPU", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                name.IndexOf("Package", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                name.IndexOf("Tctl", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                object raw = obj["Value"];
                                if (raw != null)
                                {
                                    double v = Convert.ToDouble(raw);
                                    if (v > 5.0 && v < 130.0)
                                    {
                                        tempC = v;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
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

        private static bool TrySumActiveNetworkBytes(out long totalRx, out long totalTx, out string activeAdapterName)
        {
            totalRx = 0;
            totalTx = 0;
            activeAdapterName = "";
            bool anyOk = false;
            try
            {
                NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
                for (int i = 0; i < ifaces.Length; i++)
                {
                    NetworkInterface ni = ifaces[i];
                    if (ni.OperationalStatus != OperationalStatus.Up ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(activeAdapterName)) activeAdapterName = ni.Name;
                    try
                    {
                        IPInterfaceStatistics stats = ni.GetIPStatistics();
                        if (stats != null)
                        {
                            totalRx += stats.BytesReceived;
                            totalTx += stats.BytesSent;
                            anyOk = true;
                        }
                    }
                    catch
                    {
                        try
                        {
                            IPv4InterfaceStatistics v4 = ni.GetIPv4Statistics();
                            if (v4 != null)
                            {
                                totalRx += v4.BytesReceived;
                                totalTx += v4.BytesSent;
                                anyOk = true;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
            return anyOk;
        }

        private void QueryNetworkThroughput(SystemMetrics metrics)
        {
            try
            {
                long totalRx, totalTx;
                string adapterName;
                if (!TrySumActiveNetworkBytes(out totalRx, out totalTx, out adapterName))
                {
                    metrics.NetworkRateDisplay = "Недоступно";
                    return;
                }

                if (!string.IsNullOrEmpty(adapterName))
                {
                    this._cachedNetAdapterName = adapterName;
                    metrics.NetworkDetailDisplay = adapterName;
                }
                else if (!string.IsNullOrEmpty(this._cachedNetAdapterName))
                {
                    metrics.NetworkDetailDisplay = this._cachedNetAdapterName;
                }

                DateTime now = DateTime.UtcNow;
                if (this._prevNetSampleUtc != DateTime.MinValue)
                {
                    double sec = (now - this._prevNetSampleUtc).TotalSeconds;
                    if (sec >= 0.1 && sec <= 10.0)
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
            if (this._cpuUtilityCounter != null) { try { this._cpuUtilityCounter.Dispose(); } catch { } this._cpuUtilityCounter = null; }
            if (this._cpuPerfCounter != null) { try { this._cpuPerfCounter.Dispose(); } catch { } this._cpuPerfCounter = null; }
            if (this._cpuFreqCounter != null) { try { this._cpuFreqCounter.Dispose(); } catch { } this._cpuFreqCounter = null; }
            if (this._processCounter != null) { try { this._processCounter.Dispose(); } catch { } this._processCounter = null; }
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
