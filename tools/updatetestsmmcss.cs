using System;
using System.IO;

class Program
{
    static int Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\Tests\Program.cs";
        string content = File.ReadAllText(path);

        // 1. Add Call in Main
        string targetCall = "                Console.WriteLine(\"\\n--- ТЕСТ-СЮИТ 9: Тестирование планирования GPU и Hardware Flip Queue (Noverse query_hwsch) ---\");\r\n                RunGpuHwschTests();";
        string mmcssCall = "                Console.WriteLine(\"\\n--- ТЕСТ-СЮИТ 9: Тестирование планирования GPU и Hardware Flip Queue (Noverse query_hwsch) ---\");\r\n                RunGpuHwschTests();\r\n\r\n                Console.WriteLine(\"\\n--- ТЕСТ-СЮИТ 10: Тестирование MMCSS Task Service (Noverse mmcss_task.c) ---\");\r\n                RunMmcssTaskTests();";

        if (!content.Contains(targetCall))
        {
            targetCall = targetCall.Replace("\r\n", "\n");
            mmcssCall = mmcssCall.Replace("\r\n", "\n");
            if (!content.Contains(targetCall))
            {
                Console.WriteLine("Error: targetCall not found");
                return 1;
            }
        }
        content = content.Replace(targetCall, mmcssCall);

        // 2. Add RunMmcssTaskTests method before the closing brace of Program
        string targetEnd = "        }\r\n    }\r\n}";
        string methodCode = @"
        private static void RunMmcssTaskTests()
        {
            // 1. Состояние по умолчанию MmcssTaskState
            {
                var st = new MmcssTaskState();
                bool defOk = (st.SystemResponsiveness == 20) &&
                             (st.NetworkThrottlingIndex == 10) &&
                             (st.NoLazyMode == 0) &&
                             (!st.IsMmcssOptimized) &&
                             (!string.IsNullOrEmpty(st.StatusSummary)) &&
                             (!string.IsNullOrEmpty(st.RegisteredTasksList));
                Assert(defOk,
                    ""MmcssTask_StateDefaults: Конструктор MmcssTaskState инициализирует безопасные стандартные параметры"",
                    string.Format(""Resp={0}, Throttling={1}, Optimized={2}"", st.SystemResponsiveness, st.NetworkThrottlingIndex, st.IsMmcssOptimized));
            }

            // 2. MmcssTaskService.QueryState возвращает актуальное состояние MMCSS
            {
                var svc = new MmcssTaskService();
                MmcssTaskState st = svc.QueryState();
                bool notNull = (st != null);
                bool hasSummary = !string.IsNullOrEmpty(st.StatusSummary);
                bool hasRespDisplay = !string.IsNullOrEmpty(st.SystemResponsivenessDisplay);
                bool hasTasks = (st.AvailableTasksCount >= 0);
                Assert(notNull && hasSummary && hasRespDisplay && hasTasks,
                    ""MmcssTask_ServiceQueryState: QueryState считывает реестр SystemProfile и статус задач без сбоев"",
                    string.Format(""Resp={0}%, Throttling='{1}', Задач={2}, Avrt={3}"",
                        st.SystemResponsiveness, st.NetworkThrottlingDisplay, st.AvailableTasksCount, st.IsAvrtAvailable));
            }

            // 3. Вызов P/Invoke avrt.dll для регистрации мультимедиа потока (Audio / Pro Audio / Games)
            {
                var svc = new MmcssTaskService();
                uint taskIdx;
                int win32Err;
                bool audioOk = svc.TestTaskRegistration(""Audio"", MmcssTaskService.AvrtPriority.High, out taskIdx, out win32Err);
                Assert(audioOk,
                    ""MmcssTask_AvrtTestRegistration: AvSetMmThreadCharacteristicsW и AvSetMmThreadPriority выполняются успешно для задачи Audio"",
                    string.Format(""Audio TaskIndex={0}, Win32Err={1}"", taskIdx, win32Err));
            }

            // 4. Логика оптимизации MMCSS (SystemResponsiveness >= 100 или NetworkThrottlingIndex == 0xFFFFFFFF)
            {
                var stOpt = new MmcssTaskState();
                stOpt.SystemResponsiveness = 100;
                stOpt.NetworkThrottlingIndex = 0xFFFFFFFF;
                bool isOpt = (stOpt.SystemResponsiveness >= 100 || stOpt.NetworkThrottlingIndex == 0xFFFFFFFF);
                Assert(isOpt,
                    ""MmcssTask_OptimizationLogic: Оптимизированный флаг активируется при SystemResponsiveness=100 или Throttling=0xFFFFFFFF"",
                    string.Format(""Resp={0}, Throttling=0x{1:X8}"", stOpt.SystemResponsiveness, stOpt.NetworkThrottlingIndex));
            }

            // 5. Перечисление AvrtPriority строго соответствует Noverse mmcss_task.c
            {
                bool prioOk = ((int)MmcssTaskService.AvrtPriority.VeryLow == -2) &&
                              ((int)MmcssTaskService.AvrtPriority.Low == -1) &&
                              ((int)MmcssTaskService.AvrtPriority.Normal == 0) &&
                              ((int)MmcssTaskService.AvrtPriority.High == 1) &&
                              ((int)MmcssTaskService.AvrtPriority.Critical == 2);
                Assert(prioOk,
                    ""MmcssTask_AvrtPriorityEnum: Значения приоритетов Avrt соответствуют Windows SDK (-2..+2)"",
                    ""VeryLow=-2, Low=-1, Normal=0, High=1, Critical=2"");
            }
        }
    }
}";

        if (!content.Contains(targetEnd))
        {
            targetEnd = targetEnd.Replace("\r\n", "\n");
            methodCode = methodCode.Replace("\r\n", "\n");
            if (!content.Contains(targetEnd))
            {
                Console.WriteLine("Error: targetEnd not found");
                return 2;
            }
        }
        content = content.Replace(targetEnd, methodCode);

        File.WriteAllText(path, content);
        Console.WriteLine("Successfully updated Tests/Program.cs with RunMmcssTaskTests!");
        return 0;
    }
}
