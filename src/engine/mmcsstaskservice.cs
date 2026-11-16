using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WinTweaker.Engine
{
    public class MmcssTaskState
    {
        public uint SystemResponsiveness { get; set; }
        public string SystemResponsivenessDisplay { get; set; }
        public uint NetworkThrottlingIndex { get; set; }
        public string NetworkThrottlingDisplay { get; set; }
        public uint NoLazyMode { get; set; }
        public string LazyModeDisplay { get; set; }
        public uint SchedulerTimerResolution { get; set; }
        public uint SchedulerPeriod { get; set; }
        public bool IsAvrtAvailable { get; set; }
        public uint AudioTaskIndex { get; set; }
        public uint GamesTaskIndex { get; set; }
        public uint ProAudioTaskIndex { get; set; }
        public int AvailableTasksCount { get; set; }
        public string RegisteredTasksList { get; set; }
        public bool IsMmcssOptimized { get; set; }
        public string StatusSummary { get; set; }

        public MmcssTaskState()
        {
            this.SystemResponsiveness = 20;
            this.SystemResponsivenessDisplay = "20% (Стандартное)";
            this.NetworkThrottlingIndex = 10;
            this.NetworkThrottlingDisplay = "10 NBL / такт";
            this.NoLazyMode = 0;
            this.LazyModeDisplay = "Энергосбережение вкл.";
            this.SchedulerTimerResolution = 10000;
            this.SchedulerPeriod = 100000;
            this.IsAvrtAvailable = false;
            this.AudioTaskIndex = 0;
            this.GamesTaskIndex = 0;
            this.ProAudioTaskIndex = 0;
            this.AvailableTasksCount = 0;
            this.RegisteredTasksList = "Audio, Games, Pro Audio";
            this.IsMmcssOptimized = false;
            this.StatusSummary = "MMCSS: Инициализация...";
        }
    }

    public class MmcssTaskService
    {
        public enum AvrtPriority
        {
            VeryLow = -2,
            Low = -1,
            Normal = 0,
            High = 1,
            Critical = 2
        }

        [DllImport("avrt.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr AvSetMmThreadCharacteristicsW(string TaskName, ref uint TaskIndex);

        [DllImport("avrt.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AvRevertMmThreadCharacteristics(IntPtr AvrtHandle);

        [DllImport("avrt.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AvSetMmThreadPriority(IntPtr AvrtHandle, AvrtPriority Priority);

        [DllImport("avrt.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AvQuerySystemResponsiveness(IntPtr AvrtHandle, out uint SystemResponsivenessValue);

        private const string SystemProfileKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
        private const string TasksKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks";

        public MmcssTaskState QueryState()
        {
            MmcssTaskState state = new MmcssTaskState();

            // 1. Чтение параметров SystemProfile из реестра (Noverse desc.md:1200..1211)
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(SystemProfileKey))
                {
                    if (key != null)
                    {
                        object srObj = key.GetValue("SystemResponsiveness");
                        if (srObj is int || srObj is uint)
                        {
                            state.SystemResponsiveness = Convert.ToUInt32(srObj);
                        }
                        else
                        {
                            state.SystemResponsiveness = 20; // Default Windows
                        }

                        object ntiObj = key.GetValue("NetworkThrottlingIndex");
                        if (ntiObj is int || ntiObj is uint)
                        {
                            state.NetworkThrottlingIndex = Convert.ToUInt32(ntiObj);
                        }
                        else
                        {
                            state.NetworkThrottlingIndex = 10; // Default Windows
                        }

                        object nlmObj = key.GetValue("NoLazyMode");
                        if (nlmObj is int || nlmObj is uint)
                        {
                            state.NoLazyMode = Convert.ToUInt32(nlmObj);
                        }

                        object strObj = key.GetValue("SchedulerTimerResolution");
                        if (strObj is int || strObj is uint)
                        {
                            state.SchedulerTimerResolution = Convert.ToUInt32(strObj);
                        }

                        object spObj = key.GetValue("SchedulerPeriod");
                        if (spObj is int || spObj is uint)
                        {
                            state.SchedulerPeriod = Convert.ToUInt32(spObj);
                        }
                    }
                }
            }
            catch
            {
                // Fallback к значениям по умолчанию
                state.SystemResponsiveness = 20;
                state.NetworkThrottlingIndex = 10;
            }

            // Форматирование SystemResponsiveness (Noverse: 100 отключает резервирование CPU для фона, отдавая 100% играм)
            if (state.SystemResponsiveness >= 100)
            {
                state.SystemResponsivenessDisplay = "100% (Игровой режим: фоновые задачи не резервируют CPU)";
            }
            else if (state.SystemResponsiveness <= 10)
            {
                state.SystemResponsivenessDisplay = string.Format("{0}% (Приоритет мультимедиа: резерв 10%)", state.SystemResponsiveness);
            }
            else
            {
                state.SystemResponsivenessDisplay = string.Format("{0}% (Стандартное: резерв {0}% CPU для фона)", state.SystemResponsiveness);
            }

            // Форматирование NetworkThrottlingIndex (Noverse: 0xFFFFFFFF = NDIS_INDICATE_ALL_NBLS, троттлинг выключен)
            if (state.NetworkThrottlingIndex == 0xFFFFFFFF || state.NetworkThrottlingIndex >= 71)
            {
                state.NetworkThrottlingDisplay = "Отключен (0xFFFFFFFF — максимум NDIS)";
            }
            else
            {
                state.NetworkThrottlingDisplay = string.Format("{0} пакетов/такт (Ограничение NDIS)", state.NetworkThrottlingIndex);
            }

            // Форматирование NoLazyMode
            state.LazyModeDisplay = state.NoLazyMode == 1 ? "Высокая точность (NoLazyMode=1)" : "Энергосберегающий режим (0)";

            // 2. Чтение списка доступных задач из Tasks (Audio, Pro Audio, Games, etc.)
            try
            {
                using (RegistryKey tasksKey = Registry.LocalMachine.OpenSubKey(TasksKey))
                {
                    if (tasksKey != null)
                    {
                        string[] subKeyNames = tasksKey.GetSubKeyNames();
                        state.AvailableTasksCount = subKeyNames.Length;
                        state.RegisteredTasksList = string.Join(", ", subKeyNames);
                    }
                }
            }
            catch
            {
                state.AvailableTasksCount = 0;
            }

            // 3. Тест вызовов avrt.dll (AvSetMmThreadCharacteristicsW и AvSetMmThreadPriority по коду mmcss_task.c)
            bool avrtSuccess = false;
            try
            {
                uint audioIdx = 0;
                IntPtr hAudio = AvSetMmThreadCharacteristicsW("Audio", ref audioIdx);
                if (hAudio != IntPtr.Zero)
                {
                    state.AudioTaskIndex = audioIdx;
                    AvSetMmThreadPriority(hAudio, AvrtPriority.High);
                    AvRevertMmThreadCharacteristics(hAudio);
                    avrtSuccess = true;
                }

                uint gamesIdx = 0;
                IntPtr hGames = AvSetMmThreadCharacteristicsW("Games", ref gamesIdx);
                if (hGames != IntPtr.Zero)
                {
                    state.GamesTaskIndex = gamesIdx;
                    AvSetMmThreadPriority(hGames, AvrtPriority.High);
                    AvRevertMmThreadCharacteristics(hGames);
                    avrtSuccess = true;
                }

                uint proAudioIdx = 0;
                IntPtr hProAudio = AvSetMmThreadCharacteristicsW("Pro Audio", ref proAudioIdx);
                if (hProAudio != IntPtr.Zero)
                {
                    state.ProAudioTaskIndex = proAudioIdx;
                    AvRevertMmThreadCharacteristics(hProAudio);
                    avrtSuccess = true;
                }
            }
            catch
            {
                avrtSuccess = false;
            }

            state.IsAvrtAvailable = avrtSuccess;

            // Оптимизированность: Responsiveness >= 100 или Throttling == 0xFFFFFFFF
            state.IsMmcssOptimized = state.SystemResponsiveness >= 100 || state.NetworkThrottlingIndex == 0xFFFFFFFF;

            if (state.IsMmcssOptimized)
            {
                state.StatusSummary = "MMCSS: Оптимизирован для киберспорта и минимального инпут-лага";
            }
            else
            {
                state.StatusSummary = "MMCSS: Стандартные параметры Windows (возможны задержки)";
            }

            return state;
        }

        public bool TestTaskRegistration(string taskName, AvrtPriority priority, out uint taskIndex, out int win32Error)
        {
            taskIndex = 0;
            win32Error = 0;
            try
            {
                IntPtr h = AvSetMmThreadCharacteristicsW(taskName, ref taskIndex);
                if (h == IntPtr.Zero)
                {
                    win32Error = Marshal.GetLastWin32Error();
                    return false;
                }

                bool setPrioOk = AvSetMmThreadPriority(h, priority);
                if (!setPrioOk)
                {
                    win32Error = Marshal.GetLastWin32Error();
                }

                AvRevertMmThreadCharacteristics(h);
                return setPrioOk;
            }
            catch (Exception ex)
            {
                win32Error = Marshal.GetHRForException(ex);
                return false;
            }
        }
    }
}
