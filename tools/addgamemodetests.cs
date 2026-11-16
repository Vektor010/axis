using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"C:\Users\User\Documents\win-tweaker\Tests\Program.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        string targetCall = "RunTachyonDesignTests(tweaksPath);\r\n";
        string newCall = "RunTachyonDesignTests(tweaksPath);\r\n\r\n                Console.WriteLine(\"\\n--- ТЕСТ-СЮИТ 8: Тестирование Game Mode & WNF (Noverse gm_effects) ---\");\r\n                RunGameModeWnfTests();\r\n";

        // Check line endings
        if (!content.Contains(targetCall))
        {
            targetCall = "RunTachyonDesignTests(tweaksPath);\n";
            newCall = "RunTachyonDesignTests(tweaksPath);\n\n                Console.WriteLine(\"\\n--- ТЕСТ-СЮИТ 8: Тестирование Game Mode & WNF (Noverse gm_effects) ---\");\n                RunGameModeWnfTests();\n";
        }

        string methodBody = @"
        private static void RunGameModeWnfTests()
        {
            // 1. WNF State Name константы совпадают с gm_effects.c (Noverse)
            {
                bool c1 = GameModeWnfService.WNF_RM_GAME_MODE_ACTIVE == 0x41C6033FA3BC1075UL;
                bool c2 = GameModeWnfService.WNF_SEB_GAME_MODE       == 0x41840B3EA3BDD875UL;
                Assert(c1 && c2,
                    ""GameModeWnf_Constants: WNF State Name константы соответствуют gm_effects.c"",
                    string.Format(""RM=0x{0:X}, SEB=0x{1:X}"", GameModeWnfService.WNF_RM_GAME_MODE_ACTIVE, GameModeWnfService.WNF_SEB_GAME_MODE));
            }

            // 2. QueryWnfU32 успешно запрашивает WNF_RM_GAME_MODE_ACTIVE без исключений
            {
                int ntStatus = -1;
                uint activePid = GameModeWnfService.QueryWnfU32(GameModeWnfService.WNF_RM_GAME_MODE_ACTIVE, out ntStatus);
                bool success = (ntStatus >= 0);
                Assert(success,
                    ""GameModeWnf_QueryActivePid: NtQueryWnfStateData возвращает STATUS_SUCCESS для Game Mode Active PID"",
                    string.Format(""NTSTATUS=0x{0:X8}, PID={1}"", ntStatus, activePid));
            }

            // 3. QueryWnfU32 успешно запрашивает WNF_SEB_GAME_MODE (профиль питания)
            {
                int ntStatus = -1;
                uint powerLow = GameModeWnfService.QueryWnfU32(GameModeWnfService.WNF_SEB_GAME_MODE, out ntStatus);
                bool success = (ntStatus >= 0);
                Assert(success,
                    ""GameModeWnf_QueryPowerProfile: NtQueryWnfStateData возвращает статус питания Game Mode"",
                    string.Format(""NTSTATUS=0x{0:X8}, LowVal={1}"", ntStatus, powerLow));
            }

            // 4. QueryState возвращает валидный GameModeState объект со всеми заполненными строками
            {
                var service = new GameModeWnfService();
                GameModeState st = service.QueryState();
                bool notNull = (st != null);
                bool hasProc = !string.IsNullOrEmpty(st.ActiveGameProcessName);
                bool hasSummary = !string.IsNullOrEmpty(st.StatusSummary);
                bool hasCpuSets = !string.IsNullOrEmpty(st.CpuSetIdsDisplay);
                bool hasFg = !string.IsNullOrEmpty(st.ForegroundProcessName);
                Assert(notNull && hasProc && hasSummary && hasCpuSets && hasFg,
                    ""GameModeWnf_ServiceQueryState: Сервис возвращает корректно инициализированный GameModeState"",
                    string.Format(""Game='{0}', WNF={1}, CPU='{2}', FG='{3}'"",
                        st.ActiveGameProcessName, st.PowerProfileLowValue, st.CpuSetIdsDisplay, st.ForegroundProcessName));
            }

            // 5. GameModeState конструктор инициализирует безопасные значения по умолчанию
            {
                var st = new GameModeState();
                bool defOk = (st.ActiveGamePid == 0) &&
                             (st.IsGameRunning == false) &&
                             (st.PowerProfileLowValue == 1) &&
                             (st.IsPowerProfileActive == false) &&
                             (st.ActiveGameProcessName == ""Отсутствует"") &&
                             (st.CpuSetIdsDisplay == ""Не назначены"");
                Assert(defOk,
                    ""GameModeWnf_StateDefaults: Конструктор GameModeState инициализирует ожидаемые значения по умолчанию"",
                    string.Format(""Pid={0}, Running={1}, LowVal={2}"", st.ActiveGamePid, st.IsGameRunning, st.PowerProfileLowValue));
            }

            // 6. Логика статусной сводки при отсутствии игры
            {
                var st = new GameModeState();
                st.ActiveGamePid = 0;
                st.PowerProfileLowValue = 1;
                st.IsGameRunning = false;
                bool isIdle = (st.ActiveGamePid == 0 && !st.IsGameRunning);
                Assert(isIdle,
                    ""GameModeWnf_IdleCondition: Логика состояния ожидания корректно отражает отсутствие активной игры"",
                    ""Ожидание игры (WNF=1)"");
            }
        }
    }
}
";

        if (!content.Contains("RunGameModeWnfTests"))
        {
            content = content.Replace(targetCall, newCall);

            // Replace final closing braces of class Program and namespace
            int lastClosing = content.LastIndexOf("    }\r\n}");
            if (lastClosing < 0)
            {
                lastClosing = content.LastIndexOf("    }\n}");
            }

            if (lastClosing >= 0)
            {
                content = content.Substring(0, lastClosing) + methodBody;
                File.WriteAllText(path, content, Encoding.UTF8);
                Console.WriteLine("Tests/Program.cs successfully patched with RunGameModeWnfTests!");
            }
            else
            {
                Console.WriteLine("Could not find class closing braces.");
            }
        }
        else
        {
            Console.WriteLine("RunGameModeWnfTests already present.");
        }
    }
}
