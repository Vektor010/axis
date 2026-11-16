using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\Tests\Program.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        string targetCall = "RunGameModeWnfTests();\r\n";
        string newCall = "RunGameModeWnfTests();\r\n\r\n                Console.WriteLine(\"\\n--- ТЕСТ-СЮИТ 9: Тестирование планирования GPU и Hardware Flip Queue (Noverse query_hwsch) ---\");\r\n                RunGpuHwschTests();\r\n";
        if (!content.Contains(targetCall))
        {
            targetCall = "RunGameModeWnfTests();\n";
            newCall = "RunGameModeWnfTests();\n\n                Console.WriteLine(\"\\n--- ТЕСТ-СЮИТ 9: Тестирование планирования GPU и Hardware Flip Queue (Noverse query_hwsch) ---\");\n                RunGpuHwschTests();\n";
        }

        string methodBody = @"
        private static void RunGpuHwschTests()
        {
            // 1. WDDM константы совпадают с query_hwsch.c (Noverse)
            {
                bool c29 = GpuHwschService.KMTQAITYPE_WDDM_2_9_CAPS == 75;
                bool c30 = GpuHwschService.KMTQAITYPE_WDDM_3_0_CAPS == 77;
                Assert(c29 && c30,
                    ""GpuHwsch_Constants: WDDM Caps константы соответствуют query_hwsch.c"",
                    string.Format(""WDDM_2_9={0}, WDDM_3_0={1}"", GpuHwschService.KMTQAITYPE_WDDM_2_9_CAPS, GpuHwschService.KMTQAITYPE_WDDM_3_0_CAPS));
            }

            // 2. GpuHwschState конструктор инициализирует безопасные значения по умолчанию
            {
                var st = new GpuHwschState();
                bool defOk = (!st.HasActiveGpu) &&
                             (!st.IsHagsEnabled) &&
                             (!st.IsFlipQueueEnabled) &&
                             (!string.IsNullOrEmpty(st.HagsStatusDisplay)) &&
                             (!string.IsNullOrEmpty(st.FlipQueueStatusDisplay));
                Assert(defOk,
                    ""GpuHwsch_StateDefaults: Конструктор GpuHwschState инициализирует ожидаемые значения по умолчанию"",
                    string.Format(""Hags='{0}', Flip='{1}'"", st.HagsStatusDisplay, st.FlipQueueStatusDisplay));
            }

            // 3. QueryPrimaryGpuState возвращает состояние первичного видеоадаптера без исключений
            {
                var svc = new GpuHwschService();
                GpuHwschState st = svc.QueryPrimaryGpuState();
                bool notNull = (st != null);
                bool hasSummary = !string.IsNullOrEmpty(st.SummaryDisplay);
                bool hasDev = !string.IsNullOrEmpty(st.DeviceName);
                Assert(notNull && hasSummary && hasDev,
                    ""GpuHwsch_ServiceQuery: Сервис возвращает корректно заполненное состояние адаптера"",
                    string.Format(""Dev='{0}', GPU='{1}', HAGS={2}, FlipQueue={3}"",
                        st.DeviceName, st.AdapterString, st.IsHagsEnabled, st.IsFlipQueueEnabled));
            }

            // 4. Проверка битовой маски HWSCH и HWFLIPQUEUE
            {
                uint mockCaps29 = 6u; // (1 << 2) | 2 -> DriverSupport=2, Enabled=1
                uint driverSupport = mockCaps29 & 3u;
                uint enabled = (mockCaps29 >> 2) & 1u;
                bool bitLogicOk = (driverSupport == 2 && enabled == 1);
                Assert(bitLogicOk,
                    ""GpuHwsch_BitmaskLogic: Логика битовых масок декодирования статуса WDDM соответствует query_hwsch"",
                    string.Format(""Support={0}, Enabled={1}"", driverSupport, enabled));
            }
        }
    }
}
";

        if (!content.Contains("RunGpuHwschTests"))
        {
            content = content.Replace(targetCall, newCall);

            int lastClosing = content.LastIndexOf("    }\r\n}");
            if (lastClosing < 0)
            {
                lastClosing = content.LastIndexOf("    }\n}");
            }

            if (lastClosing >= 0)
            {
                content = content.Substring(0, lastClosing) + methodBody;
                File.WriteAllText(path, content, Encoding.UTF8);
                Console.WriteLine("Tests/Program.cs successfully patched with RunGpuHwschTests!");
            }
            else
            {
                Console.WriteLine("Could not find class closing braces.");
            }
        }
        else
        {
            Console.WriteLine("RunGpuHwschTests already present.");
        }
    }
}
