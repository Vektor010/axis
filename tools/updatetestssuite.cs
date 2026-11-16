using System;
using System.IO;
using System.Text;

namespace UpdateTests
{
    class Program
    {
        static void Main(string[] args)
        {
            string programCsPath = @"C:\Users\User\Documents\win-tweaker\Tests\Program.cs";
            string text = File.ReadAllText(programCsPath, Encoding.UTF8);

            // 1. Ужесточение RunSchemaValidationTests
            string oldLoop = @"for (int r = 0; r < t.registry.Count; r++)
                    {
                        RegistryActionItem a = t.registry[r];
                        if (string.IsNullOrEmpty(a.hive) || string.IsNullOrEmpty(a.path) || string.IsNullOrEmpty(a.name) || string.IsNullOrEmpty(a.type)) allValid = false;
                        if (a.revert == null || Array.IndexOf(validRevertModes, a.revert.mode) < 0) allValid = false;
                    }";

            string newLoop = @"for (int r = 0; r < t.registry.Count; r++)
                    {
                        RegistryActionItem a = t.registry[r];
                        if (string.IsNullOrEmpty(a.hive) || string.IsNullOrEmpty(a.path) || string.IsNullOrEmpty(a.name) || string.IsNullOrEmpty(a.type)) allValid = false;
                        if (a.hive != ""HKLM"" && a.hive != ""HKCU"") allValid = false;
                        if (a.path.StartsWith(""\\"") || a.path.EndsWith(""\\"")) allValid = false;
                        if (a.apply == null) allValid = false;
                        if (a.apply is System.Collections.IDictionary || a.apply is System.Collections.IList) allValid = false;
                        if (a.type == ""REG_DWORD"" && (a.apply is string || !(a.apply is int || a.apply is long || a.apply is short || a.apply is byte))) allValid = false;
                        if (a.type == ""REG_SZ"" && !(a.apply is string)) allValid = false;
                        if (a.revert == null || Array.IndexOf(validRevertModes, a.revert.mode) < 0) allValid = false;
                        if (a.revert != null && a.revert.mode == ""set"")
                        {
                            if (a.revert.value == null || a.apply.ToString().Equals(a.revert.value.ToString())) allValid = false;
                        }
                    }";

            // Нормализация переводов строк для поиска
            string normText = text.Replace("\r\n", "\n");
            string normOldLoop = oldLoop.Replace("\r\n", "\n");
            string normNewLoop = newLoop.Replace("\r\n", "\n");

            if (!normText.Contains(normOldLoop))
            {
                Console.WriteLine("ОШИБКА: oldLoop не найден в Program.cs!");
                return;
            }

            normText = normText.Replace(normOldLoop, normNewLoop);
            Console.WriteLine("RunSchemaValidationTests успешно обновлен!");

            // 2. Добавление вызова ТЕСТ-СЮИТ 11 в Main
            string oldMainCall = @"Console.WriteLine(""\n--- ТЕСТ-СЮИТ 10: Тестирование MMCSS Task Service (Noverse mmcss_task.c) ---"");
                RunMmcssTaskTests();";

            string newMainCall = @"Console.WriteLine(""\n--- ТЕСТ-СЮИТ 10: Тестирование MMCSS Task Service (Noverse mmcss_task.c) ---"");
                RunMmcssTaskTests();

                Console.WriteLine(""\n--- ТЕСТ-СЮИТ 11: Тотальное тестирование всех твиков на движке TweakEngine с MockRegistryStore ---"");
                RunAllTweaksEngineMockTests(tweaksPath);";

            string normOldMainCall = oldMainCall.Replace("\r\n", "\n");
            string normNewMainCall = newMainCall.Replace("\r\n", "\n");

            if (!normText.Contains(normOldMainCall))
            {
                Console.WriteLine("ОШИБКА: oldMainCall не найден в Program.cs!");
                return;
            }

            normText = normText.Replace(normOldMainCall, normNewMainCall);
            Console.WriteLine("Вызов ТЕСТ-СЮИТ 11 успешно добавлен в Main!");

            // 3. Добавление метода RunAllTweaksEngineMockTests
            string newMethod = @"
        private static void RunAllTweaksEngineMockTests(string tweaksPath)
        {
            if (!File.Exists(tweaksPath))
            {
                Assert(false, ""Наличие файла tweaks.json"", tweaksPath);
                return;
            }

            string json = File.ReadAllText(tweaksPath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            List<TweakItem> tweaks = serializer.Deserialize<List<TweakItem>>(json);

            Assert(tweaks != null && tweaks.Count > 0, ""Десериализация всех твиков для сквозного тестирования движка"", string.Format(""Загружено {0} твиков"", tweaks != null ? tweaks.Count : 0));
            if (tweaks == null) return;

            string tempDir = Path.Combine(Path.GetTempPath(), ""tweaker_all_mock_"" + Guid.NewGuid().ToString(""N""));
            Directory.CreateDirectory(tempDir);

            try
            {
                MockRegistryStore store = new MockRegistryStore();
                StateManager state = new StateManager(Path.Combine(tempDir, ""state.json""), isTestMode: true);
                JournalManager journal = new JournalManager(Path.Combine(tempDir, ""journal.log""), isTestMode: true);
                TweakEngine engine = new TweakEngine(store, state, journal, false);

                int applyOk = 0;
                int checkOk = 0;
                int revertOk = 0;
                List<string> issues = new List<string>();

                for (int i = 0; i < tweaks.Count; i++)
                {
                    TweakItem t = tweaks[i];
                    int checkBuild = t.min_build.HasValue ? t.min_build.Value : 22631;

                    try
                    {
                        // 1. Initial check (no exceptions)
                        TweakStatus s0 = engine.CheckStatus(t, checkBuild);

                        // 2. Apply tweak
                        bool applied = engine.Apply(t);
                        if (!applied)
                        {
                            issues.Add(string.Format(""{0}: Apply вернул false"", t.id));
                            continue;
                        }
                        applyOk++;

                        // 3. Post-apply check: status must be Enabled
                        TweakStatus s1 = engine.CheckStatus(t, checkBuild);
                        if (s1 == TweakStatus.Enabled)
                        {
                            checkOk++;
                        }
                        else
                        {
                            issues.Add(string.Format(""{0}: статус после Apply={1} (ожидался Enabled)"", t.id, s1));
                        }

                        // 4. Revert tweak
                        bool reverted = engine.Revert(t);
                        if (reverted)
                        {
                            revertOk++;
                        }
                        else
                        {
                            issues.Add(string.Format(""{0}: Revert вернул false"", t.id));
                        }
                    }
                    catch (Exception ex)
                    {
                        issues.Add(string.Format(""{0}: исключение {1}"", t.id, ex.Message));
                    }
                }

                Assert(applyOk == tweaks.Count, ""Сквозное тестирование: 100% твиков успешно применены на MockRegistryStore"", string.Format(""{0}/{1}"", applyOk, tweaks.Count));
                Assert(checkOk == tweaks.Count, ""Сквозное тестирование: 100% твиков получили статус Enabled после Apply"", string.Format(""{0}/{1}"", checkOk, tweaks.Count));
                Assert(revertOk == tweaks.Count, ""Сквозное тестирование: 100% твиков успешно откачены через Revert"", string.Format(""{0}/{1}"", revertOk, tweaks.Count));
                Assert(issues.Count == 0, ""Сквозное тестирование: полное отсутствие сбоев и исключений при работе с базой твиков"", string.Format(""Сбоев: {0}"", issues.Count));
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch { }
            }
        }
";

            // Вставляем новый метод перед закрывающей скобкой класса Program
            int lastBrace = normText.LastIndexOf('}');
            int classBrace = normText.LastIndexOf('}', lastBrace - 1);
            normText = normText.Substring(0, classBrace) + newMethod.Replace("\r\n", "\n") + normText.Substring(classBrace);

            // Возвращаем \r\n
            string finalText = normText.Replace("\n", "\r\n");
            File.WriteAllText(programCsPath, finalText, new UTF8Encoding(false));
            Console.WriteLine("Файл Program.cs успешно обновлен и сохранен!");
        }
    }
}
