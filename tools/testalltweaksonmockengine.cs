using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.Win32;
using WinTweaker.Engine;
using WinTweaker.Models;

namespace MockEngineTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string tweaksPath = @"C:\Users\User\Documents\win-tweaker\data\tweaks.json";
            string raw = File.ReadAllText(tweaksPath, Encoding.UTF8);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var tweaks = serializer.Deserialize<List<TweakItem>>(raw);

            Console.WriteLine("Загружено твиков для сквозного тестирования: " + tweaks.Count);

            string tempDir = Path.Combine(Path.GetTempPath(), "tweaker_mock_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            int applySuccessCount = 0;
            int applyFailCount = 0;
            int checkAppliedCount = 0;
            int revertSuccessCount = 0;

            List<string> failures = new List<string>();

            try
            {
                var store = new MockRegistryStore();
                var stateManager = new StateManager(Path.Combine(tempDir, "state.json"), isTestMode: true);
                var journalManager = new JournalManager(Path.Combine(tempDir, "journal.log"), isTestMode: true);
                var engine = new TweakEngine(store, stateManager, journalManager, false);

                for (int i = 0; i < tweaks.Count; i++)
                {
                    var tweak = tweaks[i];

                    int checkBuild = tweak.min_build.HasValue ? tweak.min_build.Value : 22631;

                    // 1. Check initial status (должен выполниться без исключений)
                    try
                    {
                        var st0 = engine.CheckStatus(tweak, checkBuild);
                    }
                    catch (Exception ex)
                    {
                        failures.Add(string.Format("{0}: CheckStatus (initial) exception: {1}", tweak.id, ex.Message));
                        continue;
                    }

                    // 2. Apply tweak
                    bool applied = false;
                    try
                    {
                        applied = engine.Apply(tweak);
                    }
                    catch (Exception ex)
                    {
                        failures.Add(string.Format("{0}: Apply exception: {1}", tweak.id, ex.Message));
                        continue;
                    }

                    if (!applied)
                    {
                        applyFailCount++;
                        failures.Add(string.Format("{0}: Apply вернул false", tweak.id));
                        continue;
                    }
                    applySuccessCount++;

                    // 3. Check status after apply - должен быть Enabled
                    try
                    {
                        var st1 = engine.CheckStatus(tweak, checkBuild);
                        if (st1 == TweakStatus.Enabled)
                        {
                            checkAppliedCount++;
                        }
                        else
                        {
                            failures.Add(string.Format("{0}: после Apply статус={1} (ожидалось Enabled)", tweak.id, st1));
                        }
                    }
                    catch (Exception ex)
                    {
                        failures.Add(string.Format("{0}: CheckStatus (after apply) exception: {1}", tweak.id, ex.Message));
                    }

                    // 4. Revert tweak
                    bool reverted = false;
                    try
                    {
                        reverted = engine.Revert(tweak);
                    }
                    catch (Exception ex)
                    {
                        failures.Add(string.Format("{0}: Revert exception: {1}", tweak.id, ex.Message));
                        continue;
                    }

                    if (reverted)
                    {
                        revertSuccessCount++;
                    }
                    else
                    {
                        failures.Add(string.Format("{0}: Revert вернул false", tweak.id));
                    }
                }

                Console.WriteLine("\n=== ИТОГИ ТЕСТИРОВАНИЯ НА МОК-ДВИЖКЕ ===");
                Console.WriteLine("Всего проверено твиков: " + tweaks.Count);
                Console.WriteLine("Успешно применены (Apply == true): " + applySuccessCount);
                Console.WriteLine("Статус подтвержден (CheckStatus == Enabled): " + checkAppliedCount);
                Console.WriteLine("Успешно откачены (Revert == true): " + revertSuccessCount);
                Console.WriteLine("Всего сбоев / несоответствий: " + failures.Count);

                if (failures.Count > 0)
                {
                    Console.WriteLine("\n--- ПЕРВЫЕ 20 СБОЕВ ---");
                    for (int f = 0; f < Math.Min(20, failures.Count); f++)
                    {
                        Console.WriteLine(" [FAIL] " + failures[f]);
                    }
                }
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
    }

    public class MockRegistryStore : IRegistryStore
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RegistryValueKind> _kinds = new Dictionary<string, RegistryValueKind>(StringComparer.OrdinalIgnoreCase);

        private string MakeKey(string hive, string path, string name)
        {
            return (hive ?? "") + "\\" + (path ?? "") + "!" + (name ?? "");
        }

        public bool KeyExists(string hive, string subkey)
        {
            return true;
        }

        public bool ValueExists(string hive, string subkey, string valueName)
        {
            return _values.ContainsKey(MakeKey(hive, subkey, valueName));
        }

        public object GetValue(string hive, string subkey, string valueName)
        {
            string key = MakeKey(hive, subkey, valueName);
            object val;
            _values.TryGetValue(key, out val);
            return val;
        }

        public RegistryValueKind GetValueKind(string hive, string subkey, string valueName)
        {
            string key = MakeKey(hive, subkey, valueName);
            RegistryValueKind kind;
            if (_kinds.TryGetValue(key, out kind)) return kind;
            return RegistryValueKind.Unknown;
        }

        public void SetValue(string hive, string subkey, string valueName, object value, RegistryValueKind kind)
        {
            string key = MakeKey(hive, subkey, valueName);
            _values[key] = value;
            _kinds[key] = kind;
        }

        public void DeleteValue(string hive, string subkey, string valueName)
        {
            string key = MakeKey(hive, subkey, valueName);
            _values.Remove(key);
            _kinds.Remove(key);
        }

        public void CreateKey(string hive, string subkey)
        {
        }
    }
}
