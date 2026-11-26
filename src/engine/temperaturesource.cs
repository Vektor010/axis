using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WinTweaker.Engine
{
    public struct GpuTelemetrySnapshot
    {
        public bool IsAvailable;
        public uint TemperatureC;
        public uint GpuLoadPercent;
        public uint CoreClockMhz;
        public uint MemClockMhz;
        public double PowerWatts;
        public uint FanPercent;
        public double VramUsedGb;
        public double VramTotalGb;
    }

    public struct GpuTemperatureResult
    {
        public bool IsAvailable;
        public uint TemperatureC;
        public string ErrorMessage;

        public static GpuTemperatureResult Success(uint temp)
        {
            return new GpuTemperatureResult
            {
                IsAvailable = true,
                TemperatureC = temp,
                ErrorMessage = ""
            };
        }

        public static GpuTemperatureResult Fail(string error)
        {
            return new GpuTemperatureResult
            {
                IsAvailable = false,
                TemperatureC = 0,
                ErrorMessage = error
            };
        }
    }

    public interface ITemperatureSource : IDisposable
    {
        bool Initialize();
        GpuTemperatureResult GetGpuTemperature();
        void Shutdown();
        string FoundLibraryPath { get; }
    }

    public class NvmlTemperatureSource : ITemperatureSource
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlInitDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlDeviceGetHandleDelegate(uint index, out IntPtr device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlGetTempDelegate(IntPtr device, int sensorType, out uint temp);

        [StructLayout(LayoutKind.Sequential)]
        private struct NvmlUtilization
        {
            public uint gpu;
            public uint memory;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlGetUtilDelegate(IntPtr device, out NvmlUtilization utilization);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlGetClockInfoDelegate(IntPtr device, int clockType, out uint clockMhz);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlGetPowerUsageDelegate(IntPtr device, out uint powerMw);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlGetFanSpeedDelegate(IntPtr device, out uint speedPercent);

        [StructLayout(LayoutKind.Sequential)]
        private struct NvmlMemory
        {
            public ulong total;
            public ulong free;
            public ulong used;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlGetMemoryInfoDelegate(IntPtr device, out NvmlMemory memory);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvmlShutdownDelegate();

        private IntPtr _hMod = IntPtr.Zero;
        private IntPtr _device = IntPtr.Zero;
        private bool _isInitialized = false;
        private string _initError = "";
        private string _foundPath = "";

        private NvmlInitDelegate _initFunc;
        private NvmlDeviceGetHandleDelegate _getHandleFunc;
        private NvmlGetTempDelegate _getTempFunc;
        private NvmlGetUtilDelegate _getUtilFunc;
        private NvmlGetClockInfoDelegate _getClockFunc;
        private NvmlGetPowerUsageDelegate _getPowerFunc;
        private NvmlGetFanSpeedDelegate _getFanFunc;
        private NvmlGetMemoryInfoDelegate _getMemFunc;
        private NvmlShutdownDelegate _shutdownFunc;

        public string FoundLibraryPath
        {
            get { return this._foundPath; }
        }

        public bool Initialize()
        {
            if (this._isInitialized)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(this._initError))
            {
                return false;
            }

            try
            {
                // 1. Попытка загрузить "nvml.dll" из стандартных путей Windows
                string req = "nvml.dll";
                this._hMod = LoadLibrary(req);
                if (this._hMod == IntPtr.Zero)
                {
                    // 2. Попытка загрузить по прямому пути NVSMI
                    req = @"C:\Program Files\NVIDIA Corporation\NVSMI\nvml.dll";
                    this._hMod = LoadLibrary(req);
                }

                if (this._hMod == IntPtr.Zero)
                {
                    int err = Marshal.GetLastWin32Error();
                    this._initError = string.Format("Библиотека nvml.dll не найдена (код {0})", err);
                    return false;
                }

                StringBuilder sb = new StringBuilder(1024);
                GetModuleFileName(this._hMod, sb, sb.Capacity);
                this._foundPath = sb.ToString();

                IntPtr pInit = GetProcAddress(this._hMod, "nvmlInit_v2");
                IntPtr pHandle = GetProcAddress(this._hMod, "nvmlDeviceGetHandleByIndex_v2");
                IntPtr pTemp = GetProcAddress(this._hMod, "nvmlDeviceGetTemperature");
                IntPtr pUtil = GetProcAddress(this._hMod, "nvmlDeviceGetUtilizationRates");
                IntPtr pClock = GetProcAddress(this._hMod, "nvmlDeviceGetClockInfo");
                IntPtr pPower = GetProcAddress(this._hMod, "nvmlDeviceGetPowerUsage");
                IntPtr pFan = GetProcAddress(this._hMod, "nvmlDeviceGetFanSpeed");
                IntPtr pMem = GetProcAddress(this._hMod, "nvmlDeviceGetMemoryInfo");
                IntPtr pShutdown = GetProcAddress(this._hMod, "nvmlShutdown");

                if (pInit == IntPtr.Zero || pHandle == IntPtr.Zero || pTemp == IntPtr.Zero || pShutdown == IntPtr.Zero)
                {
                    this._initError = "Функции NVML отсутствуют в библиотеке";
                    this.CleanupLibrary();
                    return false;
                }

                this._initFunc = (NvmlInitDelegate)Marshal.GetDelegateForFunctionPointer(pInit, typeof(NvmlInitDelegate));
                this._getHandleFunc = (NvmlDeviceGetHandleDelegate)Marshal.GetDelegateForFunctionPointer(pHandle, typeof(NvmlDeviceGetHandleDelegate));
                this._getTempFunc = (NvmlGetTempDelegate)Marshal.GetDelegateForFunctionPointer(pTemp, typeof(NvmlGetTempDelegate));
                this._getUtilFunc = (pUtil != IntPtr.Zero)
                    ? (NvmlGetUtilDelegate)Marshal.GetDelegateForFunctionPointer(pUtil, typeof(NvmlGetUtilDelegate))
                    : null;
                this._getClockFunc = (pClock != IntPtr.Zero)
                    ? (NvmlGetClockInfoDelegate)Marshal.GetDelegateForFunctionPointer(pClock, typeof(NvmlGetClockInfoDelegate))
                    : null;
                this._getPowerFunc = (pPower != IntPtr.Zero)
                    ? (NvmlGetPowerUsageDelegate)Marshal.GetDelegateForFunctionPointer(pPower, typeof(NvmlGetPowerUsageDelegate))
                    : null;
                this._getFanFunc = (pFan != IntPtr.Zero)
                    ? (NvmlGetFanSpeedDelegate)Marshal.GetDelegateForFunctionPointer(pFan, typeof(NvmlGetFanSpeedDelegate))
                    : null;
                this._getMemFunc = (pMem != IntPtr.Zero)
                    ? (NvmlGetMemoryInfoDelegate)Marshal.GetDelegateForFunctionPointer(pMem, typeof(NvmlGetMemoryInfoDelegate))
                    : null;
                this._shutdownFunc = (NvmlShutdownDelegate)Marshal.GetDelegateForFunctionPointer(pShutdown, typeof(NvmlShutdownDelegate));

                int res = this._initFunc();
                if (res != 0)
                {
                    this._initError = string.Format("Ошибка инициализации NVML (код {0})", res);
                    this.CleanupLibrary();
                    return false;
                }

                res = this._getHandleFunc(0, out this._device);
                if (res != 0)
                {
                    this._initError = string.Format("Видеокарта NVIDIA не обнаружена (код {0})", res);
                    if (this._shutdownFunc != null)
                    {
                        try { this._shutdownFunc(); } catch { }
                    }
                    this.CleanupLibrary();
                    return false;
                }

                this._isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                this._initError = string.Format("Исключение при инициализации NVML: {0}", ex.Message);
                this.CleanupLibrary();
                return false;
            }
        }

        public GpuTemperatureResult GetGpuTemperature()
        {
            if (!this._isInitialized)
            {
                if (!this.Initialize())
                {
                    return GpuTemperatureResult.Fail(this._initError);
                }
            }

            try
            {
                uint temp = 0;
                int res = this._getTempFunc(this._device, 0, out temp); // 0 = NVML_TEMPERATURE_GPU
                if (res != 0)
                {
                    return GpuTemperatureResult.Fail(string.Format("Ошибка чтения температуры (код {0})", res));
                }

                return GpuTemperatureResult.Success(temp);
            }
            catch (Exception ex)
            {
                return GpuTemperatureResult.Fail(string.Format("Сбой при вызове nvmlDeviceGetTemperature: {0}", ex.Message));
            }
        }

        public bool TryGetGpuUtilization(out uint gpuPercent)
        {
            gpuPercent = 0;
            if (!this._isInitialized)
            {
                if (!this.Initialize()) return false;
            }
            if (this._getUtilFunc == null || this._device == IntPtr.Zero) return false;
            try
            {
                NvmlUtilization util;
                int res = this._getUtilFunc(this._device, out util);
                if (res == 0 && util.gpu <= 100)
                {
                    gpuPercent = util.gpu;
                    return true;
                }
            }
            catch { }
            return false;
        }

        public bool TryGetGpuTelemetry(out GpuTelemetrySnapshot snap)
        {
            snap = new GpuTelemetrySnapshot();
            if (!this._isInitialized)
            {
                if (!this.Initialize()) return false;
            }
            if (this._device == IntPtr.Zero) return false;

            try
            {
                uint temp = 0;
                if (this._getTempFunc != null && this._getTempFunc(this._device, 0, out temp) == 0)
                {
                    snap.IsAvailable = true;
                    snap.TemperatureC = temp;
                }

                NvmlUtilization util;
                if (this._getUtilFunc != null && this._getUtilFunc(this._device, out util) == 0 && util.gpu <= 100)
                {
                    snap.IsAvailable = true;
                    snap.GpuLoadPercent = util.gpu;
                }

                uint clk = 0;
                if (this._getClockFunc != null)
                {
                    if (this._getClockFunc(this._device, 0, out clk) == 0) snap.CoreClockMhz = clk; // NVML_CLOCK_GRAPHICS
                    if (this._getClockFunc(this._device, 2, out clk) == 0) snap.MemClockMhz = clk;  // NVML_CLOCK_MEM
                }

                uint mw = 0;
                if (this._getPowerFunc != null && this._getPowerFunc(this._device, out mw) == 0 && mw > 0)
                {
                    snap.PowerWatts = mw / 1000.0;
                }

                uint fan = 0;
                if (this._getFanFunc != null && this._getFanFunc(this._device, out fan) == 0 && fan <= 100)
                {
                    snap.FanPercent = fan;
                }

                NvmlMemory mem;
                if (this._getMemFunc != null && this._getMemFunc(this._device, out mem) == 0 && mem.total > 0)
                {
                    double gb = 1024.0 * 1024.0 * 1024.0;
                    snap.VramTotalGb = mem.total / gb;
                    snap.VramUsedGb = mem.used / gb;
                }

                return snap.IsAvailable;
            }
            catch
            {
                return false;
            }
        }

        public void Shutdown()
        {
            if (this._isInitialized)
            {
                try
                {
                    if (this._shutdownFunc != null)
                    {
                        this._shutdownFunc();
                    }
                }
                catch { }
                finally
                {
                    this._isInitialized = false;
                    this._device = IntPtr.Zero;
                }
            }

            this.CleanupLibrary();
        }

        private void CleanupLibrary()
        {
            if (this._hMod != IntPtr.Zero)
            {
                try
                {
                    FreeLibrary(this._hMod);
                }
                catch { }
                finally
                {
                    this._hMod = IntPtr.Zero;
                }
            }
        }

        public void Dispose()
        {
            this.Shutdown();
        }
    }

    public class MockTemperatureSource : ITemperatureSource
    {
        private readonly bool _available;
        private readonly uint _temperatureC;
        private readonly string _errorMessage;
        private readonly string _mockPath;

        public MockTemperatureSource(bool available, uint temperatureC = 0, string errorMessage = "Библиотека nvml.dll не найдена", string mockPath = "")
        {
            this._available = available;
            this._temperatureC = temperatureC;
            this._errorMessage = errorMessage;
            this._mockPath = mockPath;
        }

        public string FoundLibraryPath
        {
            get { return this._mockPath; }
        }

        public bool Initialize()
        {
            return this._available;
        }

        public GpuTemperatureResult GetGpuTemperature()
        {
            if (this._available)
            {
                return GpuTemperatureResult.Success(this._temperatureC);
            }
            return GpuTemperatureResult.Fail(this._errorMessage);
        }

        public void Shutdown()
        {
        }

        public void Dispose()
        {
        }
    }
}
