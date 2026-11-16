using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using WinTweaker.Engine;
using WinTweaker.Models;

class RollbackVerification
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("==================================================================");
        Console.WriteLine("           LIVE ROLLBACK VERIFICATION (E2E TEST)                  ");
        Console.WriteLine("==================================================================");

        string testStatePath = Path.Combine(Path.GetTempPath(), "tweaker_test_state_" + Guid.NewGuid().ToString("N") + ".json");
        string testJournalDir = Path.Combine(Path.GetTempPath(), "tweaker_test_journal_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testJournalDir);
        string testLogPath = Path.Combine(testJournalDir, "journal.log");

        var stateMgr = new StateManager(testStatePath);
        var journalMgr = new JournalManager(testLogPath);
        var regStore = new RealRegistryStore();
        var engine = new TweakEngine(regStore, stateMgr, journalMgr, true, null);

        string testSubKey = @"Software\WinTweakerTestRollback_" + Environment.TickCount;
        RegistryKey hkcu = Registry.CurrentUser.CreateSubKey(testSubKey);
        hkcu.Close();

        int passed = 0;
        int failed = 0;

        try
        {
            // --- СЦЕНАРИЙ 1: Параметра НЕ было в системе (mode = "delete") ---
            // Должен создаться при Apply, а при Revert полностью удалиться из реестра.
            {
                string valName = "NonExistentVal";
                var tweak1 = new TweakItem
                {
                    id = "test-rollback-delete",
                    name_ru = "Тест отката: удаление созданного ключа",
                    category = "system",
                    registry = new List<RegistryActionItem>
                    {
                        new RegistryActionItem
                        {
                            hive = "HKCU",
                            path = testSubKey,
                            name = valName,
                            type = "REG_DWORD",
                            apply = 1,
                            revert = new RevertSpec { mode = "delete" }
                        }
                    }
                };

                // Проверяем, что значения нет
                bool existsBefore = regStore.ValueExists("HKCU", testSubKey, valName);
                
                // Применяем
                bool applyOk = engine.Apply(tweak1);
                object valAfterApply = regStore.GetValue("HKCU", testSubKey, valName);

                // Откатываем
                bool revertOk = engine.Revert(tweak1);
                bool existsAfterRevert = regStore.ValueExists("HKCU", testSubKey, valName);

                bool s1Ok = (!existsBefore) && applyOk && (Convert.ToInt32(valAfterApply) == 1) && revertOk && (!existsAfterRevert);
                if (s1Ok)
                {
                    Console.WriteLine("[PASS] Сценарий 1 (mode=delete): Ключ создан (1) -> при откате полностью удален из реестра!");
                    passed++;
                }
                else
                {
                    Console.WriteLine("[FAIL] Сценарий 1 не прошел!");
                    failed++;
                }
            }

            // --- СЦЕНАРИЙ 2: Параметр существовал со значением 1 (mode = "set", default = 1, apply = 0) ---
            // Например, ShowSleepOption: исходно 1, apply = 0, revert = 1
            {
                string valName = "ExistingValSet";
                regStore.SetValue("HKCU", testSubKey, valName, 1, RegistryValueKind.DWord);

                var tweak2 = new TweakItem
                {
                    id = "test-rollback-set",
                    name_ru = "Тест отката: возврат дефолтного значения 1",
                    category = "power",
                    registry = new List<RegistryActionItem>
                    {
                        new RegistryActionItem
                        {
                            hive = "HKCU",
                            path = testSubKey,
                            name = valName,
                            type = "REG_DWORD",
                            apply = 0,
                            revert = new RevertSpec { mode = "set", value = 1 }
                        }
                    }
                };

                int valInitial = Convert.ToInt32(regStore.GetValue("HKCU", testSubKey, valName));
                bool applyOk = engine.Apply(tweak2);
                int valAfterApply = Convert.ToInt32(regStore.GetValue("HKCU", testSubKey, valName));

                bool revertOk = engine.Revert(tweak2);
                int valAfterRevert = Convert.ToInt32(regStore.GetValue("HKCU", testSubKey, valName));

                bool s2Ok = (valInitial == 1) && applyOk && (valAfterApply == 0) && revertOk && (valAfterRevert == 1);
                if (s2Ok)
                {
                    Console.WriteLine("[PASS] Сценарий 2 (mode=set): Исходно (1) -> Apply (0) -> Revert вернул строго (1)!");
                    passed++;
                }
                else
                {
                    Console.WriteLine("[FAIL] Сценарий 2 не прошел!");
                    failed++;
                }
            }

            // --- СЦЕНАРИЙ 3: Откат из снимка (mode = "snapshot", кастомное значение до применения) ---
            // Пользователь имел нестандартное значение 42, Apply ставит 99, Revert возвращает ровно 42!
            {
                string valName = "CustomUserValue";
                regStore.SetValue("HKCU", testSubKey, valName, 42, RegistryValueKind.DWord);

                var tweak3 = new TweakItem
                {
                    id = "test-rollback-snapshot",
                    name_ru = "Тест отката: возврат кастомного снимка",
                    category = "privacy",
                    registry = new List<RegistryActionItem>
                    {
                        new RegistryActionItem
                        {
                            hive = "HKCU",
                            path = testSubKey,
                            name = valName,
                            type = "REG_DWORD",
                            apply = 99,
                            revert = new RevertSpec { mode = "snapshot" }
                        }
                    }
                };

                int valInitial = Convert.ToInt32(regStore.GetValue("HKCU", testSubKey, valName));
                bool applyOk = engine.Apply(tweak3);
                int valAfterApply = Convert.ToInt32(regStore.GetValue("HKCU", testSubKey, valName));

                bool revertOk = engine.Revert(tweak3);
                int valAfterRevert = Convert.ToInt32(regStore.GetValue("HKCU", testSubKey, valName));

                bool s3Ok = (valInitial == 42) && applyOk && (valAfterApply == 99) && revertOk && (valAfterRevert == 42);
                if (s3Ok)
                {
                    Console.WriteLine("[PASS] Сценарий 3 (mode=snapshot): Исходно (42) -> Apply (99) -> Revert восстановил ровно (42)!");
                    passed++;
                }
                else
                {
                    Console.WriteLine("[FAIL] Сценарий 3 не прошел!");
                    failed++;
                }
            }

            // --- СЦЕНАРИЙ 4: Проверка экспорта .reg бэкапа перед Apply ---
            {
                string[] regFiles = Directory.GetFiles(testJournalDir, "*.reg", SearchOption.AllDirectories);
                bool hasRegBackups = regFiles.Length >= 3;
                if (hasRegBackups)
                {
                    Console.WriteLine(string.Format("[PASS] Сценарий 4 (.reg бэкапы): Автоматически создано {0} файлов .reg в каталоге бэкапов!", regFiles.Length));
                    passed++;
                }
                else
                {
                    Console.WriteLine("[FAIL] Сценарий 4: .reg бэкапы не обнаружены!");
                    failed++;
                }
            }
        }
        finally
        {
            // Очистка тестового ключа
            try { Registry.CurrentUser.DeleteSubKeyTree(testSubKey); } catch { }
            try { if (File.Exists(testStatePath)) File.Delete(testStatePath); } catch { }
            try { if (Directory.Exists(testJournalDir)) Directory.Delete(testJournalDir, true); } catch { }
        }

        Console.WriteLine("==================================================================");
        Console.WriteLine(string.Format("РЕЗУЛЬТАТ: {0} УСПЕШНО, {1} ОШИБОК", passed, failed));
        Console.WriteLine("==================================================================");

        Environment.Exit(failed > 0 ? 1 : 0);
    }
}
