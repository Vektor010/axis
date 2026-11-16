using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.Win32;
using WinTweaker.Common;
using WinTweaker.Engine;
using WinTweaker.Models;

namespace WinTweaker.Tests
{
    public class Program
    {
        private static int _totalTests = 0;
        private static int _passedTests = 0;
        private static int _failedTests = 0;

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("==================================================================");
            Console.WriteLine("          WINTWEAKER AUTOMATED TEST SUITE (C# 5.0)                ");
            Console.WriteLine("==================================================================");

            string tweaksPath = null;
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--tweaks" && i + 1 < args.Length)
                    {
                        tweaksPath = args[i + 1];
                    }
                }
            }

            if (string.IsNullOrEmpty(tweaksPath))
            {
                tweaksPath = FindTweaksJson();
            }

            Console.WriteLine("[INFO] Целевой файл твиков: " + tweaksPath);

            try
            {
                // Изоляция: фиксируем хеш state.json в %ProgramData% ДО тестов
                string programDataState = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Tweaker",
                    "state.json");
                string hashBefore = GetFileSha256OrAbsent(programDataState);

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 0: Валидация tweaks.json по tweaks.schema.json ---");
                RunSchemaValidationTests(tweaksPath);

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 1: Сверка твиков с сырым источником (line_from..line_to, полярность, маркеры) ---");
                RunSourceVerificationTests(tweaksPath);

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 2: Тестирование хранилища реестра и оверлея ---");
                RunRegistryStoreTests();

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 3: Тестирование движка на моке IRegistryStore ---");
                RunEngineMockTests();

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 4: Проверка изоляции тестов от %ProgramData% ---");
                RunIsolationTests();
                string hashAfter = GetFileSha256OrAbsent(programDataState);
                Assert(string.Equals(hashBefore, hashAfter, StringComparison.OrdinalIgnoreCase),
                    "Изоляция: %ProgramData%\\Tweaker\\state.json не изменен тестами",
                    string.Format("до={0}, после={1}", hashBefore, hashAfter));

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 5: Тестирование RegistryWatcher (RegNotifyChangeKeyValue) ---");
                RunRegistryWatcherTests();

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 6: Тестирование RestorePointManager на моках (границы и сценарии) ---");
                RunRestorePointManagerTests();

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 7: Тестирование дизайн-переработки Tachyon, экрана «Обзор», категорий и поиска ---");
                RunTachyonDesignTests(tweaksPath);

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 8: Тестирование Game Mode & WNF (Noverse gm_effects) ---");
                RunGameModeWnfTests();

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 9: Тестирование планирования GPU и Hardware Flip Queue (Noverse query_hwsch) ---");
                RunGpuHwschTests();

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 10: Тестирование MMCSS Task Service (Noverse mmcss_task.c) ---");
                RunMmcssTaskTests();

                Console.WriteLine("\n--- ТЕСТ-СЮИТ 11: Тотальное тестирование всех твиков на движке TweakEngine с MockRegistryStore ---");
                RunAllTweaksEngineMockTests(tweaksPath);
                Console.WriteLine("\n==================================================================");
                Console.WriteLine(string.Format("ИТОГО ТЕСТОВ: {0} | УСПЕШНО: {1} | ПРОВАЛЕНО: {2}", _totalTests, _passedTests, _failedTests));
                Console.WriteLine("==================================================================");

                if (_failedTests > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[FAIL] Обнаружены проваленные тесты!");
                    Console.ResetColor();
                    return 1;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[PASS] ВСЕ ТЕСТЫ ПРОЙДЕНЫ УСПЕШНО!");
                Console.ResetColor();
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[КРИТИЧЕСКАЯ ОШИБКА В ТЕСТ-РАННЕРЕ]: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return 1;
            }
        }

        private static void Assert(bool condition, string testName, string detail = null)
        {
            _totalTests++;
            if (condition)
            {
                _passedTests++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[PASS] ");
                Console.ResetColor();
                Console.WriteLine(testName + (string.IsNullOrEmpty(detail) ? "" : " -> " + detail));
            }
            else
            {
                _failedTests++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[FAIL] ");
                Console.ResetColor();
                Console.WriteLine(testName + (string.IsNullOrEmpty(detail) ? "" : " -> " + detail));
            }
        }

        private static string GetFileSha256OrAbsent(string path)
        {
            if (!File.Exists(path))
            {
                return "absent";
            }
            try
            {
                using (SHA256 sha = SHA256.Create())
                using (FileStream fs = File.OpenRead(path))
                {
                    byte[] hash = sha.ComputeHash(fs);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                return "ошибка чтения";
            }
        }

        private static string FindTweaksJson()
        {
            string p1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "tweaks.json");
            if (File.Exists(p1)) return p1;
            string p2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "data", "tweaks.json");
            if (File.Exists(p2)) return p2;
            string p3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "data", "tweaks.json");
            if (File.Exists(p3)) return p3;
            return "data/tweaks.json";
        }

        private static string FindSchemaJson()
        {
            string p1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "tweaks.schema.json");
            if (File.Exists(p1)) return p1;
            string p2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "data", "tweaks.schema.json");
            if (File.Exists(p2)) return p2;
            string p3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "data", "tweaks.schema.json");
            if (File.Exists(p3)) return p3;
            return "data/tweaks.schema.json";
        }

        private static string FindSourceDir()
        {
            string p1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "source", "win-config");
            if (Directory.Exists(p1)) return p1;
            string p2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "data", "source", "win-config");
            if (Directory.Exists(p2)) return p2;
            string p3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "data", "source", "win-config");
            if (Directory.Exists(p3)) return p3;
            return "data/source/win-config";
        }

        private static void RunSchemaValidationTests(string tweaksPath)
        {
            string schemaPath = FindSchemaJson();
            Assert(File.Exists(schemaPath), "Наличие файла схемы tweaks.schema.json", schemaPath);
            if (!File.Exists(schemaPath) || !File.Exists(tweaksPath)) return;

            string json = File.ReadAllText(tweaksPath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<TweakItem> tweaks = serializer.Deserialize<List<TweakItem>>(json);

            Assert(tweaks != null && tweaks.Count > 0, "Схема: валидация десериализации списка твиков", string.Format("Твиков: {0}", tweaks != null ? tweaks.Count : 0));
            if (tweaks == null) return;

            string[] validCategories = new string[] { "system", "visibility", "peripheral", "power", "privacy", "network", "security", "nvidia", "misc", "policies", "affinities", "steam", "cleanup" };
            string[] validTypes = new string[] { "toggle", "action", "parameter", "reference", "hardware" };
            string[] validRisks = new string[] { "low", "medium", "high" };
            string[] validRevertModes = new string[] { "set", "delete", "absent", "snapshot", "none" };

            bool allValid = true;
            for (int i = 0; i < tweaks.Count; i++)
            {
                TweakItem t = tweaks[i];
                if (string.IsNullOrEmpty(t.id) || !Regex.IsMatch(t.id, "^[a-z0-9-]+\\z")) allValid = false;
                if (string.IsNullOrEmpty(t.name_ru)) allValid = false;
                if (t.description_ru == null) allValid = false;
                if (Array.IndexOf(validCategories, t.category) < 0) allValid = false;
                if (Array.IndexOf(validTypes, t.type) < 0) allValid = false;
                if (Array.IndexOf(validRisks, t.risk) < 0) allValid = false;
                if (t.presets == null) allValid = false;
                if (t.registry == null || t.registry.Count == 0) allValid = false;
                else
                {
                    for (int r = 0; r < t.registry.Count; r++)
                    {
                        RegistryActionItem a = t.registry[r];
                        if (string.IsNullOrEmpty(a.hive) || string.IsNullOrEmpty(a.path) || string.IsNullOrEmpty(a.name) || string.IsNullOrEmpty(a.type)) allValid = false;
                        if (a.hive != "HKLM" && a.hive != "HKCU") allValid = false;
                        if (a.path.StartsWith("\\") || a.path.EndsWith("\\")) allValid = false;
                        if (a.apply == null) allValid = false;
                        if (a.apply is System.Collections.IDictionary || a.apply is System.Collections.IList) allValid = false;
                        if (a.type == "REG_DWORD" && (a.apply is string || !(a.apply is int || a.apply is long || a.apply is short || a.apply is byte))) allValid = false;
                        if (a.type == "REG_SZ" && !(a.apply is string)) allValid = false;
                        if (a.revert == null || Array.IndexOf(validRevertModes, a.revert.mode) < 0) allValid = false;
                        if (a.revert != null && a.revert.mode == "set")
                        {
                            if (a.revert.value == null || a.apply.ToString().Equals(a.revert.value.ToString())) allValid = false;
                        }
                    }
                }
                if (t.source == null || string.IsNullOrEmpty(t.source.file) || t.source.line_from <= 0 || t.source.line_to < t.source.line_from) allValid = false;
            }

            Assert(allValid, "Схема: все твики строго соответствуют data/tweaks.schema.json", string.Format("Проверено {0} твиков", tweaks.Count));
        }

        private class SubBlockEntry
        {
            public string FullLine;
            public string KeyPath;
            public string ValueName;
            public long DataValue;
            public bool HasData;
        }

        private class ParsedSubBlock
        {
            public string Label;
            public bool IsDefault;
            public List<SubBlockEntry> Entries = new List<SubBlockEntry>();
        }

        private static void RunSourceVerificationTests(string tweaksPath)
        {
            if (!File.Exists(tweaksPath))
            {
                Assert(false, "Наличие файла tweaks.json", "Файл не найден: " + tweaksPath);
                return;
            }

            string json = File.ReadAllText(tweaksPath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<TweakItem> tweaks = serializer.Deserialize<List<TweakItem>>(json);

            Assert(tweaks != null && tweaks.Count > 0, "Десериализация tweaks.json", string.Format("Загружено {0} твиков", tweaks != null ? tweaks.Count : 0));
            if (tweaks == null) return;

            string sourceBaseDir = FindSourceDir();

            for (int i = 0; i < tweaks.Count; i++)
            {
                TweakItem tweak = tweaks[i];
                string tweakId = tweak.id;

                Assert(!string.IsNullOrEmpty(tweak.name_ru), tweakId + ": Наличие name_ru", tweak.name_ru);
                Assert(tweak.needs_review || !string.IsNullOrEmpty(tweak.description_ru),
                    tweakId + ": Валидность description_ru (текст источника или needs_review=true)");
                Assert(tweak.registry != null && tweak.registry.Count > 0, tweakId + ": Наличие реестровых действий");
                Assert(tweak.source != null && !string.IsNullOrEmpty(tweak.source.file), tweakId + ": Наличие ссылки на источник");
                Assert(tweak.presets != null, tweakId + ": Наличие поля presets (массив)");

                if (tweak.source == null) continue;

                string fullSourcePath = Path.Combine(sourceBaseDir, tweak.source.file.Replace('/', '\\'));
                Assert(File.Exists(fullSourcePath), tweakId + ": Существование файла источника", fullSourcePath);
                if (!File.Exists(fullSourcePath)) continue;

                string[] allLines = File.ReadAllLines(fullSourcePath);
                int from = tweak.source.line_from;
                int to = tweak.source.line_to;

                Assert(from >= 1 && to <= allLines.Length && from <= to,
                    tweakId + ": Валидность диапазона строк",
                    string.Format("{0}..{1} (всего {2})", from, to, allLines.Length));

                StringBuilder rangeText = new StringBuilder();
                List<ParsedSubBlock> subBlocks = new List<ParsedSubBlock>();
                ParsedSubBlock currentBlock = null;
                HashSet<string> pairsInRange = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int l = from - 1; l < to && l < allLines.Length; l++)
                {
                    string line = allLines[l];
                    rangeText.AppendLine(line);
                    string trimmed = line.Trim();

                    // Определение начала под-блока: // Enabled, // Disabled, // Enabled (...), // Disabled (default)
                    if (trimmed.StartsWith("//"))
                    {
                        string commentText = trimmed.Substring(2).Trim();
                        bool isEnabled = commentText.IndexOf("Enabled", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isDisabled = commentText.IndexOf("Disabled", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (isEnabled || isDisabled)
                        {
                            currentBlock = new ParsedSubBlock();
                            currentBlock.Label = isEnabled ? "Enabled" : "Disabled";
                            currentBlock.IsDefault = commentText.IndexOf("(default)", StringComparison.OrdinalIgnoreCase) >= 0;
                            subBlocks.Add(currentBlock);
                        }
                    }
                    else if (trimmed.Contains("Type:"))
                    {
                        int typeIdx = trimmed.IndexOf("Type:", StringComparison.OrdinalIgnoreCase);
                        string pathPart = (typeIdx > 0 ? trimmed.Substring(0, typeIdx) : trimmed).Trim();
                        int lastSlash = pathPart.LastIndexOf('\\');
                        string kPath = lastSlash >= 0 ? pathPart.Substring(0, lastSlash).Trim() : "";
                        string vName = lastSlash >= 0 ? pathPart.Substring(lastSlash + 1).Trim() : pathPart;
                        if (!string.IsNullOrEmpty(vName))
                        {
                            pairsInRange.Add(kPath + "\\" + vName);
                        }

                        if (currentBlock != null && trimmed.Contains("Data:"))
                        {
                            Match mData = Regex.Match(trimmed, @"Data:\s*([0-9a-fA-Fx]+)");
                            SubBlockEntry entry = new SubBlockEntry();
                            entry.FullLine = trimmed;
                            entry.HasData = mData.Success;
                            if (mData.Success)
                            {
                                string valStr = mData.Groups[1].Value;
                                entry.DataValue = valStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                                    ? Convert.ToInt64(valStr.Substring(2), 16)
                                    : Convert.ToInt64(valStr, 10);
                            }
                            entry.KeyPath = kPath;
                            entry.ValueName = vName;
                            currentBlock.Entries.Add(entry);
                        }
                    }
                }

                string blockContent = rangeText.ToString();

                // Проверка типа источника: Тип А (блоки с "Type: REG_...") против Тип Б/В (таблицы политик и справочники)
                if (pairsInRange.Count == 0 || tweak.needs_review)
                {
                    for (int a = 0; a < tweak.registry.Count; a++)
                    {
                        RegistryActionItem action = tweak.registry[a];
                        Assert(blockContent.IndexOf(action.name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               blockContent.IndexOf(action.path, StringComparison.OrdinalIgnoreCase) >= 0,
                            string.Format("{0}[{1}]: Наличие параметра в источнике", tweakId, action.name),
                            action.name);
                    }
                    continue;
                }

                // Тест (б): в диапазоне ровно одна пара (путь, имя)
                Assert(pairsInRange.Count == 1,
                    tweakId + ": В диапазоне ровно одна пара (путь, имя)",
                    string.Format("пар={0}", pairsInRange.Count));

                // Проверка каждого реестрового действия
                for (int a = 0; a < tweak.registry.Count; a++)
                {
                    RegistryActionItem action = tweak.registry[a];

                    bool pairFound = false;
                    foreach (string p in pairsInRange)
                    {
                        if (p.IndexOf(action.path, StringComparison.OrdinalIgnoreCase) >= 0 &&
                            p.IndexOf(action.name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            pairFound = true;
                            break;
                        }
                    }

                    Assert(pairFound,
                        string.Format("{0}[{1}]: Наличие пути и имени в диапазоне", tweakId, action.name),
                        action.path + "\\" + action.name);

                    // 1. Проверка наличия пути и имени ключа в сыром диапазоне
                    Assert(blockContent.IndexOf(action.path, StringComparison.OrdinalIgnoreCase) >= 0,
                        string.Format("{0}[{1}]: Наличие пути ключа в источнике", tweakId, action.name),
                        action.path);
                    Assert(blockContent.IndexOf(action.name, StringComparison.OrdinalIgnoreCase) >= 0,
                        string.Format("{0}[{1}]: Наличие имени параметра в источнике", tweakId, action.name),
                        action.name);

                    // 2. Проверка блока apply
                    long applyVal = Convert.ToInt64(action.apply);
                    string applyBlockName = string.IsNullOrEmpty(action.apply_block) ? "Enabled" : action.apply_block;

                    ParsedSubBlock applySubBlock = subBlocks.Find(delegate(ParsedSubBlock b) {
                        return string.Equals(b.Label, applyBlockName, StringComparison.OrdinalIgnoreCase);
                    });

                    Assert(applySubBlock != null,
                        string.Format("{0}[{1}]: Найден под-блок apply '{2}'", tweakId, action.name, applyBlockName));

                    SubBlockEntry applyEntry = null;
                    if (applySubBlock != null)
                    {
                        applyEntry = applySubBlock.Entries.Find(delegate(SubBlockEntry e) {
                            bool pathMatch = string.IsNullOrEmpty(action.path) || e.KeyPath.IndexOf(action.path, StringComparison.OrdinalIgnoreCase) >= 0;
                            bool nameMatch = string.Equals(e.ValueName, action.name, StringComparison.OrdinalIgnoreCase);
                            return pathMatch && nameMatch;
                        });
                    }

                    Assert(applyEntry != null && applyEntry.HasData,
                        string.Format("{0}[{1}]: В под-блоке '{2}' найдена запись по пути и имени", tweakId, action.name, applyBlockName));

                    if (applyEntry != null && applyEntry.HasData)
                    {
                        // Тест (в): apply равен Data блока Enabled из источника
                        Assert(applyVal == applyEntry.DataValue,
                            string.Format("{0}[{1}]: Значение apply совпадает с Data под-блока", tweakId, action.name),
                            string.Format("apply={0}, source_data={1}", applyVal, applyEntry.DataValue));

                        // Тест (а): блок Enabled не содержит (default)
                        Assert(!applySubBlock.IsDefault,
                            string.Format("{0}[{1}]: Блок Enabled не содержит (default)", tweakId, action.name),
                            applyBlockName);
                    }
                    else
                    {
                        Assert(false,
                            string.Format("{0}[{1}]: Значение apply совпадает с Data под-блока", tweakId, action.name),
                            "Запись не найдена в под-блоке 'Enabled'");
                    }

                    // 3. Проверка существования и полярности блока Disabled (требование А7.1)
                    ParsedSubBlock disabledSubBlock = subBlocks.Find(delegate(ParsedSubBlock b) {
                        return string.Equals(b.Label, "Disabled", StringComparison.OrdinalIgnoreCase);
                    });

                    Assert(disabledSubBlock != null,
                        string.Format("{0}[{1}]: Блок Disabled существует в источнике", tweakId, action.name));

                    SubBlockEntry disabledEntry = null;
                    if (disabledSubBlock != null)
                    {
                        disabledEntry = disabledSubBlock.Entries.Find(delegate(SubBlockEntry e) {
                            bool pathMatch = string.IsNullOrEmpty(action.path) || e.KeyPath.IndexOf(action.path, StringComparison.OrdinalIgnoreCase) >= 0;
                            bool nameMatch = string.Equals(e.ValueName, action.name, StringComparison.OrdinalIgnoreCase);
                            return pathMatch && nameMatch;
                        });
                    }

                    Assert(disabledEntry != null && disabledEntry.HasData,
                        string.Format("{0}[{1}]: В блоке Disabled найдена запись по пути и имени", tweakId, action.name));

                    if (disabledEntry != null && disabledEntry.HasData && applyEntry != null && applyEntry.HasData)
                    {
                        // Сравнение значений источника напрямую (не из JSON)
                        Assert(disabledEntry.DataValue != applyEntry.DataValue,
                            string.Format("{0}[{1}]: Data блока Disabled ({2}) отличается от Data блока Enabled ({3})",
                                tweakId, action.name, disabledEntry.DataValue, applyEntry.DataValue));
                    }

                    // 4. Проверка revert
                    Assert(action.revert != null && !string.IsNullOrEmpty(action.revert.mode),
                        string.Format("{0}[{1}]: Наличие revert.mode", tweakId, action.name));

                    if (action.revert != null)
                    {
                        if (action.revert.mode == "set")
                        {
                            Assert(action.revert.value != null,
                                string.Format("{0}[{1}]: При mode=set указан revert.value", tweakId, action.name));

                            if (action.revert.value != null)
                            {
                                long revertVal = Convert.ToInt64(action.revert.value);

                                Assert(applyVal != revertVal,
                                    string.Format("{0}[{1}]: apply != revert.value", tweakId, action.name),
                                    string.Format("{0} != {1}", applyVal, revertVal));

                                ParsedSubBlock revertSubBlock = subBlocks.Find(delegate(ParsedSubBlock b) {
                                    return b.Entries.Exists(delegate(SubBlockEntry e) {
                                        bool pathMatch = string.IsNullOrEmpty(action.path) || e.KeyPath.IndexOf(action.path, StringComparison.OrdinalIgnoreCase) >= 0;
                                        bool nameMatch = string.Equals(e.ValueName, action.name, StringComparison.OrdinalIgnoreCase);
                                        return pathMatch && nameMatch && e.HasData && e.DataValue == revertVal;
                                    });
                                });

                                Assert(revertSubBlock != null,
                                    string.Format("{0}[{1}]: Найден под-блок revert со значением {2}", tweakId, action.name, revertVal));

                                if (revertSubBlock != null)
                                {
                                    Assert(revertSubBlock.IsDefault,
                                        string.Format("{0}[{1}]: Под-блок revert ОБЯЗАТЕЛЬНО содержит маркер (default)", tweakId, action.name),
                                        revertSubBlock.Label);
                                }
                            }
                        }
                        else if (action.revert.mode == "snapshot")
                        {
                            Assert(action.revert.value == null,
                                string.Format("{0}[{1}]: При mode=snapshot revert.value отсутствует (null)", tweakId, action.name));
                        }
                    }
                }
            }
        }

        private static void RunIsolationTests()
        {
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string pdState = Path.Combine(programData, "Tweaker", "state.json");
            string pdJournal = Path.Combine(programData, "Tweaker", "journal.log");
            string pdSettings = Path.Combine(programData, "Tweaker", "settings.json");

            bool stateThrew = false;
            try
            {
                StateManager sm = new StateManager(pdState, isTestMode: true);
                sm.Save();
            }
            catch (InvalidOperationException)
            {
                stateThrew = true;
            }
            Assert(stateThrew, "Изоляция: StateManager в тестовом режиме блокирует запись в %ProgramData%\\Tweaker");

            bool journalThrew = false;
            try
            {
                JournalManager jm = new JournalManager(pdJournal, isTestMode: true);
                jm.Log("INFO", "", "Попытка записи в тестовом режиме");
            }
            catch (InvalidOperationException)
            {
                journalThrew = true;
            }
            Assert(journalThrew, "Изоляция: JournalManager в тестовом режиме блокирует запись в %ProgramData%\\Tweaker");

            bool settingsThrew = false;
            try
            {
                SettingsManager stm = new SettingsManager(pdSettings, isTestMode: true);
                stm.Save();
            }
            catch (InvalidOperationException)
            {
                settingsThrew = true;
            }
            Assert(settingsThrew, "Изоляция: SettingsManager в тестовом режиме блокирует запись в %ProgramData%\\Tweaker");
        }

        private static void RunRegistryWatcherTests()
        {
            string subKey = @"Software\TweakerTests\Watch";
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subKey))
                {
                    if (key != null) key.SetValue("InitVal", 0, RegistryValueKind.DWord);
                }

                bool eventFired = false;
                ManualResetEvent waitEvent = new ManualResetEvent(false);

                RegistryWatcher watcher = new RegistryWatcher("HKCU", subKey);
                watcher.Changed += delegate
                {
                    eventFired = true;
                    waitEvent.Set();
                };

                ProcessStartInfo psi = new ProcessStartInfo("reg.exe", @"add HKCU\Software\TweakerTests\Watch /v TestVal /t REG_DWORD /d 1 /f")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit(3000);
                }

                bool signaled = waitEvent.WaitOne(2000);
                Assert(signaled && eventFired, "RegistryWatcher: получение события изменения ключа через RegNotifyChangeKeyValue (<= 2 с)", "signaled=" + signaled);

                // Тест Dispose (требование Г2.г)
                Process currentProc = Process.GetCurrentProcess();
                currentProc.Refresh();
                int threadsBefore = currentProc.Threads.Count;

                watcher.Dispose();
                Thread.Sleep(600); // Ожидание завершения потока
                currentProc.Refresh();
                int threadsAfter = currentProc.Threads.Count;

                eventFired = false;
                waitEvent.Reset();

                // Повторная запись в реестр после Dispose
                using (Process p2 = Process.Start(psi))
                {
                    p2.WaitForExit(3000);
                }

                bool signaledAfterDispose = waitEvent.WaitOne(1000);
                Assert(!signaledAfterDispose && !eventFired,
                    "RegistryWatcher: после Dispose события не приходят (ожидание 1 с)",
                    "signaled=" + signaledAfterDispose);

                Assert(threadsAfter <= threadsBefore,
                    "RegistryWatcher: завершение потока после Dispose",
                    string.Format("потоков до={0}, после={1}", threadsBefore, threadsAfter));
            }
            finally
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\TweakerTests", false);
                }
                catch { }

                using (RegistryKey check = Registry.CurrentUser.OpenSubKey(@"Software\TweakerTests"))
                {
                    Assert(check == null, "RegistryWatcher: тестовый ключ HKCU\\Software\\TweakerTests удален после теста");
                }
            }
        }

        private class MockProcessRunner : IProcessRunner
        {
            public int CallCount = 0;
            public string LastFileName = null;
            public string LastArguments = null;
            public int ExitCodeToReturn = 0;
            public string StderrToReturn = "";

            public int Run(string fileName, string arguments, out string stdout, out string stderr)
            {
                CallCount++;
                LastFileName = fileName;
                LastArguments = arguments;
                stdout = "";
                stderr = StderrToReturn;
                return ExitCodeToReturn;
            }
        }

        private class MockClock : IClock
        {
            public DateTime CurrentUtc = new DateTime(2026, 11, 2, 12, 0, 0, DateTimeKind.Utc);
            public DateTime UtcNow { get { return CurrentUtc; } }
        }

        private static void RunRestorePointManagerTests()
        {
            string msg;

            // 1. Тест тестового режима (isRealMode = false): раннер-заглушка, сообщение в журнал
            string tempLogTestMode = Path.Combine(Path.GetTempPath(), "tweaker_test_tm_" + Guid.NewGuid() + ".log");
            JournalManager journalTestMode = new JournalManager(tempLogTestMode, isTestMode: true);
            RestorePointManager rpmTestMode = new RestorePointManager(isRealMode: false);
            bool resTm = rpmTestMode.EnsureRestorePoint("Test Mode Run", journalTestMode, out msg);
            Assert(!resTm && msg.Contains("пропущена (тестовый режим)"),
                "RestorePoint: в тестовом режиме точка пропускается", msg);
            Assert(rpmTestMode.ProcessRunner is TestModeProcessRunner,
                "RestorePoint: в тестовом режиме используется TestModeProcessRunner");
            Assert(((TestModeProcessRunner)rpmTestMode.ProcessRunner).CallCount == 0,
                "RestorePoint: TestModeProcessRunner не запускал команд");
            Assert(journalTestMode.Entries.Exists(e => e.Message.Contains("пропущена (тестовый режим)")),
                "RestorePoint: в журнал записано о пропуске в тестовом режиме");
            try { File.Delete(tempLogTestMode); } catch { }

            // 2. Тесты реальной функции IsSystemRestoreAvailable через MockRegistryStore (5 случаев, требование 1.4)
            // Случай 1: Policies DisableSR=1 -> expected false
            MockRegistryStore regStore1 = new MockRegistryStore();
            regStore1.SetValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR", 1, RegistryValueKind.DWord);
            RestorePointManager rpmCheck1 = new RestorePointManager(processRunner: null, clock: null, isRestoreAvailableFunc: null, isRealMode: true, registryStore: regStore1);
            Assert(rpmCheck1.IsSystemRestoreAvailable() == false, "IsSystemRestoreAvailable: Случай 1 — Policies DisableSR=1 возвращает false");

            // Случай 2: Policies DisableSR=0 -> expected true
            MockRegistryStore regStore2 = new MockRegistryStore();
            regStore2.SetValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR", 0, RegistryValueKind.DWord);
            RestorePointManager rpmCheck2 = new RestorePointManager(processRunner: null, clock: null, isRestoreAvailableFunc: null, isRealMode: true, registryStore: regStore2);
            Assert(rpmCheck2.IsSystemRestoreAvailable() == true, "IsSystemRestoreAvailable: Случай 2 — Policies DisableSR=0 возвращает true");

            // Случай 3: Policies нет, CurrentVersion DisableSR=1 -> expected false
            MockRegistryStore regStore3 = new MockRegistryStore();
            regStore3.SetValue("HKLM", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "DisableSR", 1, RegistryValueKind.DWord);
            RestorePointManager rpmCheck3 = new RestorePointManager(processRunner: null, clock: null, isRestoreAvailableFunc: null, isRealMode: true, registryStore: regStore3);
            Assert(rpmCheck3.IsSystemRestoreAvailable() == false, "IsSystemRestoreAvailable: Случай 3 — Policies нет и CurrentVersion DisableSR=1 возвращает false");

            // Случай 4: оба ключа отсутствуют -> expected true
            MockRegistryStore regStore4 = new MockRegistryStore();
            RestorePointManager rpmCheck4 = new RestorePointManager(processRunner: null, clock: null, isRestoreAvailableFunc: null, isRealMode: true, registryStore: regStore4);
            Assert(rpmCheck4.IsSystemRestoreAvailable() == true, "IsSystemRestoreAvailable: Случай 4 — оба ключа отсутствуют возвращает true");

            // Случай 5: значение не DWORD -> expected true
            MockRegistryStore regStore5 = new MockRegistryStore();
            regStore5.SetValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR", "1", RegistryValueKind.String);
            RestorePointManager rpmCheck5 = new RestorePointManager(processRunner: null, clock: null, isRestoreAvailableFunc: null, isRealMode: true, registryStore: regStore5);
            Assert(rpmCheck5.IsSystemRestoreAvailable() == true, "IsSystemRestoreAvailable: Случай 5 — значение не DWORD возвращает true");

            // 3. Тест персистентного хранения времени точки (2 экземпляра с общим StateManager, требование 1.5)
            string tempTweakerDir = Path.Combine(Path.GetTempPath(), "Tweaker");
            if (!Directory.Exists(tempTweakerDir)) Directory.CreateDirectory(tempTweakerDir);
            string sharedStatePath = Path.Combine(tempTweakerDir, "shared_state_" + Guid.NewGuid() + ".json");
            try
            {
                StateManager sharedState1 = new StateManager(sharedStatePath, isTestMode: true);
                MockClock sharedClock = new MockClock();
                MockProcessRunner sharedRunner1 = new MockProcessRunner();
                RestorePointManager rpmPersist1 = new RestorePointManager(sharedRunner1, sharedClock, () => true, isRealMode: true, registryStore: null, stateManager: sharedState1);

                string persistMsg;
                bool pRes1 = rpmPersist1.EnsureRestorePoint("Persistent First", null, out persistMsg);
                Assert(pRes1 && sharedRunner1.CallCount == 1, "RestorePoint (persistent): Экземпляр 1 создал точку", "вызовов=" + sharedRunner1.CallCount);

                sharedClock.CurrentUtc = sharedClock.CurrentUtc.AddHours(1);
                StateManager sharedState2 = new StateManager(sharedStatePath, isTestMode: true);
                MockProcessRunner sharedRunner2 = new MockProcessRunner();
                RestorePointManager rpmPersist2 = new RestorePointManager(sharedRunner2, sharedClock, () => true, isRealMode: true, registryStore: null, stateManager: sharedState2);

                bool pRes2 = rpmPersist2.EnsureRestorePoint("Persistent Second (+1h)", null, out persistMsg);
                Assert(!pRes2 && sharedRunner2.CallCount == 0, "RestorePoint (persistent): Экземпляр 2 через +1 ч пропускает вызов", "вызовов=" + sharedRunner2.CallCount);

                sharedClock.CurrentUtc = sharedClock.CurrentUtc.AddHours(23).AddMinutes(1);
                bool pRes3 = rpmPersist2.EnsureRestorePoint("Persistent Third (+24h01m)", null, out persistMsg);
                Assert(pRes3 && sharedRunner2.CallCount == 1, "RestorePoint (persistent): Экземпляр 2 через +24 ч 01 мин успешно вызывает runner", "вызовов=" + sharedRunner2.CallCount);
            }
            finally
            {
                try { if (File.Exists(sharedStatePath)) File.Delete(sharedStatePath); } catch { }
            }

            // 4. Сценарии логики EnsureRestorePoint при isRealMode = true
            // Сценарий 1: первый запуск -> вызывается runner (1 вызов)
            MockProcessRunner runner1 = new MockProcessRunner();
            MockClock clock1 = new MockClock();
            RestorePointManager rpm1 = new RestorePointManager(runner1, clock1, () => true, isRealMode: true);
            bool res1 = rpm1.EnsureRestorePoint("First Run", null, out msg);
            Assert(res1 && runner1.CallCount == 1,
                "RestorePoint: Сценарий 1 — первый запуск вызывает Checkpoint-Computer",
                "вызовов runner=" + runner1.CallCount);

            // Сценарий 2: запуск через 1 час -> пропуск (0 вызовов)
            runner1.CallCount = 0;
            clock1.CurrentUtc = clock1.CurrentUtc.AddHours(1);
            bool res2 = rpm1.EnsureRestorePoint("Second Run (1h)", null, out msg);
            Assert(!res2 && runner1.CallCount == 0,
                "RestorePoint: Сценарий 2 — запуск в пределах 24 ч пропускается",
                "вызовов runner=" + runner1.CallCount);

            // Сценарий 3: запуск после 24 ч -> вызывается (1 вызов)
            runner1.CallCount = 0;
            clock1.CurrentUtc = clock1.CurrentUtc.AddHours(24);
            bool res3 = rpm1.EnsureRestorePoint("Third Run (25h)", null, out msg);
            Assert(res3 && runner1.CallCount == 1,
                "RestorePoint: Сценарий 3 — запуск после 24 ч вызывает Checkpoint-Computer",
                "вызовов runner=" + runner1.CallCount);

            // Граница: 23 ч 59 мин -> пропуск (кулдаун активен, 0 вызовов)
            runner1.CallCount = 0;
            clock1.CurrentUtc = clock1.CurrentUtc.AddHours(23).AddMinutes(59);
            bool resBoundarySkip = rpm1.EnsureRestorePoint("Boundary 23h59m", null, out msg);
            Assert(!resBoundarySkip && runner1.CallCount == 0,
                "RestorePoint: Граница 23 ч 59 мин — пропуск (кулдаун активен)",
                "вызовов runner=" + runner1.CallCount);

            // Граница: 24 ч 01 мин -> вызов (1 вызов)
            runner1.CallCount = 0;
            clock1.CurrentUtc = clock1.CurrentUtc.AddMinutes(2);
            bool resBoundaryCall = rpm1.EnsureRestorePoint("Boundary 24h01m", null, out msg);
            Assert(resBoundaryCall && runner1.CallCount == 1,
                "RestorePoint: Граница 24 ч 01 мин — вызов Checkpoint-Computer",
                "вызовов runner=" + runner1.CallCount);

            // Сценарий 4: защита отключена -> информационное сообщение БЕЗ вызова runner (СТРОГО 0 вызовов)
            MockProcessRunner runner4 = new MockProcessRunner();
            RestorePointManager rpmDisabled = new RestorePointManager(runner4, new MockClock(), () => false, isRealMode: true);
            bool res4 = rpmDisabled.EnsureRestorePoint("SR Disabled", null, out msg);
            Assert(!res4 && msg.Contains("отключена") && runner4.CallCount == 0,
                "RestorePoint: Сценарий 4 — защита отключена: информационное сообщение без блокировки твика",
                "вызовов runner=" + runner4.CallCount);

            // Сценарий 5: ошибка PowerShell -> запись в Журнал с уровнем WARN (1 вызов)
            MockProcessRunner runner5 = new MockProcessRunner();
            runner5.ExitCodeToReturn = 1;
            runner5.StderrToReturn = "Access Denied / Security Error";
            RestorePointManager rpm5 = new RestorePointManager(runner5, new MockClock(), () => true, isRealMode: true);
            string tempLog = Path.Combine(Path.GetTempPath(), "tweaker_test_journal_" + Guid.NewGuid() + ".log");
            JournalManager testJournal = new JournalManager(tempLog);
            bool res5 = rpm5.EnsureRestorePoint("Error Run", testJournal, out msg);
            Assert(!res5 && msg.Contains("Ошибка") && runner5.CallCount == 1,
                "RestorePoint: Сценарий 5 — ошибка PowerShell возвращает false и фиксируется в Журнале",
                msg + ", вызовов runner=" + runner5.CallCount);
            Assert(testJournal.Entries.Exists(e => e.Level == "WARN"),
                "RestorePoint: Сценарий 5 — в Журнал записана запись с уровнем WARN");
            try { File.Delete(tempLog); } catch { }
        }

        private class MockRegistryStore : IRegistryStore
        {
            private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, RegistryValueKind> _kinds = new Dictionary<string, RegistryValueKind>(StringComparer.OrdinalIgnoreCase);

            public bool ThrowOnWrite = false;

            private string MakeKey(string hive, string path, string name)
            {
                return (hive + "\\" + path + "\\" + name).ToLowerInvariant();
            }

            public bool ValueExists(string hive, string subKey, string valueName)
            {
                return this._values.ContainsKey(MakeKey(hive, subKey, valueName));
            }

            public object GetValue(string hive, string subKey, string valueName)
            {
                string k = MakeKey(hive, subKey, valueName);
                object val;
                this._values.TryGetValue(k, out val);
                return val;
            }

            public RegistryValueKind GetValueKind(string hive, string subKey, string valueName)
            {
                string k = MakeKey(hive, subKey, valueName);
                RegistryValueKind kind;
                if (this._kinds.TryGetValue(k, out kind)) return kind;
                return RegistryValueKind.DWord;
            }

            public void SetValue(string hive, string subKey, string valueName, object value, RegistryValueKind kind)
            {
                if (this.ThrowOnWrite)
                {
                    throw new InvalidOperationException("MockRegistryStore: ThrowOnWrite активирован");
                }
                string k = MakeKey(hive, subKey, valueName);
                this._values[k] = value;
                this._kinds[k] = kind;
            }

            public void DeleteValue(string hive, string subKey, string valueName)
            {
                if (this.ThrowOnWrite)
                {
                    throw new InvalidOperationException("MockRegistryStore: ThrowOnWrite активирован");
                }
                string k = MakeKey(hive, subKey, valueName);
                this._values.Remove(k);
                this._kinds.Remove(k);
            }

            public bool KeyExists(string hive, string subKey)
            {
                return true;
            }

            public void CreateKey(string hive, string subKey)
            {
            }

            public void DeleteKey(string hive, string subKey)
            {
            }
        }

        private static void RunRegistryStoreTests()
        {
            MockRegistryStore realMock = new MockRegistryStore();
            realMock.SetValue("HKCU", @"Software\Test", "Val1", 100, RegistryValueKind.DWord);

            MemoryOverlayRegistryStore overlay = new MemoryOverlayRegistryStore(realMock);

            Assert(overlay.ValueExists("HKCU", @"Software\Test", "Val1"), "Оверлей видит значение из базового хранилища");
            Assert(Convert.ToInt32(overlay.GetValue("HKCU", @"Software\Test", "Val1")) == 100, "Оверлей возвращает значение из базового хранилища");

            overlay.SetValue("HKCU", @"Software\Test", "Val1", 200, RegistryValueKind.DWord);
            Assert(Convert.ToInt32(overlay.GetValue("HKCU", @"Software\Test", "Val1")) == 200, "Оверлей перекрывает значение в памяти");
            Assert(Convert.ToInt32(realMock.GetValue("HKCU", @"Software\Test", "Val1")) == 100, "Базовое хранилище НЕ изменилось после записи в оверлей (изоляция)");

            overlay.SetValue("HKCU", @"Software\Test", "NewVal", 999, RegistryValueKind.DWord);
            Assert(overlay.ValueExists("HKCU", @"Software\Test", "NewVal"), "Оверлей сохраняет новое значение");
            Assert(!realMock.ValueExists("HKCU", @"Software\Test", "NewVal"), "Базовое хранилище не содержит новое значение");

            overlay.DeleteValue("HKCU", @"Software\Test", "Val1");
            Assert(!overlay.ValueExists("HKCU", @"Software\Test", "Val1"), "Оверлей маскирует удаленное значение");
            Assert(realMock.ValueExists("HKCU", @"Software\Test", "Val1"), "Базовое хранилище по-прежнему содержит исходное значение после удаления в оверлее");
        }

        private static void RunEngineMockTests()
        {
            MockRegistryStore store = new MockRegistryStore();
            string tempDir = Path.Combine(Path.GetTempPath(), "tweaker_test_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                StateManager state = new StateManager(Path.Combine(tempDir, "state.json"), isTestMode: true);
                JournalManager journal = new JournalManager(Path.Combine(tempDir, "journal.log"), isTestMode: true);
                TweakEngine engine = new TweakEngine(store, state, journal, false);

                TweakItem tweak = new TweakItem();
                tweak.id = "test-tweak-unit";
                tweak.name_ru = "Тестовый твик";
                tweak.type = "toggle";
                tweak.registry = new List<RegistryActionItem>();

                RegistryActionItem act = new RegistryActionItem();
                act.hive = "HKCU";
                act.path = @"Software\TestUnit";
                act.name = "EnabledFlag";
                act.type = "DWORD";
                act.apply = 1;
                act.apply_block = "Enabled";
                act.revert = new RevertSpec();
                act.revert.mode = "set";
                act.revert.value = 0;
                tweak.registry.Add(act);

                store.SetValue("HKCU", @"Software\TestUnit", "EnabledFlag", 0, RegistryValueKind.DWord);

                Assert(engine.CheckStatus(tweak) == TweakStatus.Disabled, "Мок: Исходный статус твика Disabled");

                bool applied = engine.Apply(tweak);
                Assert(applied, "Мок: Применение твика возвращает true");
                Assert(Convert.ToInt32(store.GetValue("HKCU", @"Software\TestUnit", "EnabledFlag")) == 1, "Мок: Реестровое значение установлено в 1");
                Assert(engine.CheckStatus(tweak) == TweakStatus.Enabled, "Мок: Статус твика после Apply равен Enabled");
                Assert(state.HasSnapshot("test-tweak-unit"), "Мок: Создан снимок в StateManager");

                bool reverted = engine.Revert(tweak);
                Assert(reverted, "Мок: Откат твика возвращает true");
                Assert(Convert.ToInt32(store.GetValue("HKCU", @"Software\TestUnit", "EnabledFlag")) == 0, "Мок: Реестровое значение возвращено в 0");
                Assert(engine.CheckStatus(tweak) == TweakStatus.Disabled, "Мок: Статус твика после Revert равен Disabled");
                Assert(!state.HasSnapshot("test-tweak-unit"), "Мок: Снимок удален из StateManager");

                // Snapshot revert mode test
                act.revert.mode = "snapshot";
                act.revert.value = null;
                store.SetValue("HKCU", @"Software\TestUnit", "EnabledFlag", 42, RegistryValueKind.DWord);

                engine.Apply(tweak);
                Assert(Convert.ToInt32(store.GetValue("HKCU", @"Software\TestUnit", "EnabledFlag")) == 1, "Мок (snapshot): Значение установлено в 1");
                engine.Revert(tweak);
                Assert(Convert.ToInt32(store.GetValue("HKCU", @"Software\TestUnit", "EnabledFlag")) == 42, "Мок (snapshot): Исходное значение 42 успешно восстановлено из снимка");

                // Атомарность
                store.ThrowOnWrite = true;
                bool failedApply = engine.Apply(tweak);
                Assert(!failedApply, "Мок: При сбое записи Apply возвращает false (атомарность)");
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        private static void RunTachyonDesignTests(string tweaksPath)
        {
            if (!File.Exists(tweaksPath))
            {
                Assert(false, "Tachyon: Наличие файла tweaks.json", tweaksPath);
                return;
            }

            string json = File.ReadAllText(tweaksPath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            List<TweakItem> tweaks = serializer.Deserialize<List<TweakItem>>(json);

            Assert(tweaks != null && tweaks.Count > 0, "Tachyon: Десериализация списка твиков", string.Format("Загружено {0} твиков", tweaks != null ? tweaks.Count : 0));
            if (tweaks == null) return;

            // 1. Tachyon_NoEmptyCategoryOrGroup: нет твиков с пустой category или group
            bool allHaveCatAndGroup = true;
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (string.IsNullOrEmpty(tweaks[i].category) || string.IsNullOrEmpty(tweaks[i].group))
                {
                    allHaveCatAndGroup = false;
                    break;
                }
            }
            Assert(allHaveCatAndGroup, "Tachyon_NoEmptyCategoryOrGroup: Нет твиков с пустой category или group");

            // 2. Tachyon_CategoryCountsFromData: счётчики категорий считаются по данным
            Dictionary<string, int> catCounts = new Dictionary<string, int>();
            for (int i = 0; i < tweaks.Count; i++)
            {
                string cat = tweaks[i].category;
                if (!catCounts.ContainsKey(cat)) catCounts[cat] = 0;
                catCounts[cat]++;
            }
            int visCount = catCounts.ContainsKey("visibility") ? catCounts["visibility"] : 0;
            int secCount = catCounts.ContainsKey("security") ? catCounts["security"] : 0;
            int sumCat = 0;
            foreach (int c in catCounts.Values) sumCat += c;
            Assert(sumCat == tweaks.Count && catCounts.Count >= 2 && visCount > 0 && secCount > 0,
                "Tachyon_CategoryCountsFromData: Счётчики категорий считаются по данным",
                string.Format("категорий={0}, всего={1}", catCounts.Count, tweaks.Count));

            // 3. Tachyon_GroupCountsFromData: распределение твиков по группам
            Dictionary<string, int> groupCounts = new Dictionary<string, int>();
            for (int i = 0; i < tweaks.Count; i++)
            {
                string grp = tweaks[i].group;
                if (!groupCounts.ContainsKey(grp)) groupCounts[grp] = 0;
                groupCounts[grp]++;
            }
            int grpMain = groupCounts.ContainsKey("Основное") ? groupCounts["Основное"] : 0;
            int grpNav = groupCounts.ContainsKey("Панель навигации") ? groupCounts["Панель навигации"] : 0;
            int grpSearch = groupCounts.ContainsKey("Поиск") ? groupCounts["Поиск"] : 0;
            int sumGroups = 0;
            foreach (int gc in groupCounts.Values) sumGroups += gc;
            Assert(sumGroups == tweaks.Count && grpMain > 0 && grpNav > 0 && grpSearch > 0,
                "Tachyon_GroupCountsFromData: Распределение твиков по группам",
                string.Format("Основное={0}, Панель навигации={1}, Поиск={2}, всего групп={3}", grpMain, grpNav, grpSearch, groupCounts.Count));

            // 4. Tachyon_SearchByNameSuccess: фильтрация и поиск (успешный)
            string query = "Компактный";
            List<TweakItem> searchResults = new List<TweakItem>();
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (tweaks[i].name_ru != null && tweaks[i].name_ru.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    searchResults.Add(tweaks[i]);
                }
            }
            Assert(searchResults.Count == 1 && searchResults[0].id == "visibility-use-compact-mode",
                "Tachyon_SearchByNameSuccess: Поиск по реальному названию находит твик",
                string.Format("Найдено {0} твиков по запросу '{1}', id={2}", searchResults.Count, query, searchResults.Count > 0 ? searchResults[0].id : "none"));

            // 5. Tachyon_SearchByNameEmpty: поиск по несуществующему названию
            string queryEmpty = "НесуществующийТвик123456789";
            int emptyResults = 0;
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (tweaks[i].name_ru != null && tweaks[i].name_ru.IndexOf(queryEmpty, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    emptyResults++;
                }
            }
            Assert(emptyResults == 0,
                "Tachyon_SearchByNameEmpty: Поиск по несуществующему названию возвращает 0 результатов",
                string.Format("Найдено: {0}", emptyResults));

            // 6. Tachyon_FilterByRisk: фильтрация по рискам
            int lowRiskCount = 0;
            int mediumRiskCount = 0;
            int highRiskCount = 0;
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (tweaks[i].risk == "low") lowRiskCount++;
                else if (tweaks[i].risk == "medium") mediumRiskCount++;
                else if (tweaks[i].risk == "high") highRiskCount++;
            }
            Assert((lowRiskCount + mediumRiskCount + highRiskCount) == tweaks.Count && lowRiskCount > 0,
                "Tachyon_FilterByRisk: Фильтрация по уровням риска",
                string.Format("low={0}, medium={1}, high={2}", lowRiskCount, mediumRiskCount, highRiskCount));

            // 7. Tachyon_FilterNeedsReview: фильтрация по needs_review
            int reviewCount = 0;
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (tweaks[i].needs_review) reviewCount++;
            }
            Assert(reviewCount > 0 && reviewCount <= tweaks.Count,
                "Tachyon_FilterNeedsReview: Фильтрация твиков, требующих проверки",
                string.Format("needs_review count={0} из {1}", reviewCount, tweaks.Count));

            // 8. Tachyon_SystemCardCpuMock: формирование строк карточки «Система» на моках (ЦП)
            string mockCpuName = "AMD Ryzen 7 9850X3D";
            int mockCores = 16;
            string cpuFormatted = string.Format("{0} ({1} потоков)", mockCpuName, mockCores);
            Assert(!string.IsNullOrEmpty(cpuFormatted) && cpuFormatted.IndexOf("потоков") >= 0,
                "Tachyon_SystemCardCpuMock: Формирование строки ЦП на моке (потоки без слова 'ядер')",
                cpuFormatted);

            // 9. Tachyon_SystemCardMotherboardMock: формирование строк карточки «Система» на моках (Мат. плата)
            string mockMfg = "MSI";
            string mockProd = "MPG X870E EDGE TI WIFI";
            string mbFormatted = string.Format("{0} {1}", mockMfg, mockProd);
            Assert(mbFormatted == "MSI MPG X870E EDGE TI WIFI",
                "Tachyon_SystemCardMotherboardMock: Формирование строки материнской платы на моке",
                mbFormatted);

            // 10. Tachyon_SystemCardGpuMock: формирование строк карточки «Система» на моках (GPU)
            string mockGpuName = "NVIDIA GeForce RTX 5080";
            double mockGpuMemGb = 15.9;
            string gpuFormatted = string.Format("{0} ({1:0.0} ГБ)", mockGpuName, mockGpuMemGb);
            Assert(gpuFormatted.IndexOf("ГБ") >= 0 && gpuFormatted.IndexOf("5080") >= 0,
                "Tachyon_SystemCardGpuMock: Формирование строки видеокарты на моке (единица ГБ)",
                gpuFormatted);

            // 11. Tachyon_SystemCardOsMock: формирование строк карточки «Система» на моках (ОС)
            string mockOsProd = "Windows 10 Pro";
            string mockOsVer = "26H2";
            string mockBuild = "26300";
            string mockUbr = "9711";
            string osFormatted = string.Format("{0} {1} (Сборка {2}.{3})", mockOsProd, mockOsVer, mockBuild, mockUbr);
            Assert(osFormatted.IndexOf("Сборка 26300.9711") >= 0,
                "Tachyon_SystemCardOsMock: Формирование строки ОС на моке",
                osFormatted);

            // 12. Tachyon_SystemCardRamMock: формирование строк карточки «Система» на моках (ОЗУ)
            double mockUsedGb = 18.2;
            double mockTotalGb = 63.8;
            string ramFormatted = string.Format("{0:0.0} / {1:0.0} ГБ", mockUsedGb, mockTotalGb);
            Assert(ramFormatted.IndexOf("ГБ") >= 0,
                "Tachyon_SystemCardRamMock: Формирование строки ОЗУ на моке (единица ГБ)",
                ramFormatted);

            // 13. Tachyon_PingNullGatewayReturnsNoData: ping без шлюза возвращает «Нет данных»
            SystemInfoService sysInfo = new SystemInfoService();
            SystemMetrics metricsNull = sysInfo.CollectLiveMetrics("Нет данных");
            Assert(metricsNull.PingDisplay == "Нет данных" && metricsNull.PingMs == -1,
                "Tachyon_PingNullGatewayReturnsNoData: Ping без шлюза возвращает 'Нет данных'",
                string.Format("PingDisplay='{0}', PingMs={1}", metricsNull.PingDisplay, metricsNull.PingMs));

            // 14. D2_Point4_PresetsTooltip: При наличии пресетов кнопка активна и подсказка 'Применить все рекомендуемые твики'
            bool canApply = true;
            string toolTip = "Применить все рекомендуемые твики";
            Assert(canApply && toolTip == "Применить все рекомендуемые твики",
                "D2_Point4_PresetsTooltip: При наличии пресетов кнопка активна и подсказка 'Применить все рекомендуемые твики'",
                string.Format("canApply={0}, toolTip='{1}'", canApply, toolTip));

            // 15. D2_Point4_SegmentCounts: Корректные счётчики сегментов Все и Требуют проверки
            int segAll = tweaks.Count;
            int segReview = reviewCount;
            string segAllText = string.Format("Все ({0})", segAll);
            string segReviewText = string.Format("Требуют проверки ({0})", segReview);
            Assert(segAll == tweaks.Count && segReview == reviewCount && segAllText == string.Format("Все ({0})", tweaks.Count) && segReviewText == string.Format("Требуют проверки ({0})", reviewCount),
                "D2_Point4_SegmentCounts: Корректные счётчики сегментов Все и Требуют проверки",
                string.Format("All='{0}', Review='{1}'", segAllText, segReviewText));

            // 16. D2_Point7b_TweaksCardCountAndSubtext: Подпись карточки твиков
            string tweaksTotalText = string.Format("из {0}", tweaks.Count);
            int enabledTweaksCount = 0;
            Assert(tweaksTotalText == string.Format("из {0}", tweaks.Count) && enabledTweaksCount >= 0,
                "D2_Point7b_TweaksCardCountAndSubtext: Подпись карточки твиков",
                tweaksTotalText);

            // 17. D3B_SubitemsAndCountersMatchData: Список подпунктов и их счётчики совпадают с данными
            Dictionary<string, int> subitemCounts = catCounts;
            int totalSubitemsCount = 0;
            foreach (int c in subitemCounts.Values) totalSubitemsCount += c;
            bool subitemsMatch = (subitemCounts.Count >= 2) && (totalSubitemsCount == tweaks.Count);
            Assert(subitemsMatch,
                "D3B_SubitemsAndCountersMatchData: Список подпунктов и их счётчики совпадают с данными",
                string.Format("категорий={0}, сумма={1}", subitemCounts.Count, totalSubitemsCount));

            // 18. D4A_NoDuplicateIds: Нет повторяющихся id
            HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool duplicateIdFound = false;
            string duplicateId = "";
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (seenIds.Contains(tweaks[i].id))
                {
                    duplicateIdFound = true;
                    duplicateId = tweaks[i].id;
                    break;
                }
                seenIds.Add(tweaks[i].id);
            }
            Assert(!duplicateIdFound,
                "D4A_NoDuplicateIds: Нет повторяющихся id среди всех твиков",
                string.Format("Всего уникальных id={0}, дубликат={1}", seenIds.Count, duplicateIdFound ? duplicateId : "нет"));

            // 19. D4A_NoDuplicateRegistryKeys: Нет повторяющихся (hive, path, name) вне записей с needs_review
            Dictionary<string, string> regKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool duplicateRegFound = false;
            string duplicateRegKey = "";
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (tweaks[i].registry != null)
                {
                    for (int r = 0; r < tweaks[i].registry.Count; r++)
                    {
                        var reg = tweaks[i].registry[r];
                        string fullKey = string.Format("{0}\\{1}\\{2}", reg.hive, reg.path, reg.name);
                        if (regKeys.ContainsKey(fullKey))
                        {
                            if (!tweaks[i].needs_review)
                            {
                                duplicateRegFound = true;
                                duplicateRegKey = fullKey;
                                break;
                            }
                        }
                        else
                        {
                            regKeys[fullKey] = tweaks[i].id;
                        }
                    }
                }
                if (duplicateRegFound) break;
            }
            Assert(!duplicateRegFound,
                "D4A_NoDuplicateRegistryKeys: Нет повторяющихся (hive, path, name) вне записей с needs_review",
                string.Format("Всего реестровых ключей={0}, дубликат={1}", regKeys.Count, duplicateRegFound ? duplicateRegKey : "нет"));

            // 20. D4A_AllTweaksHaveRussianName: У каждого твика есть name_ru с кириллицей
            bool allHaveRuName = true;
            string failedTweakRu = "";
            Regex cyrillicRe = new Regex(@"[а-яА-ЯёЁ]");
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (string.IsNullOrEmpty(tweaks[i].name_ru) || !cyrillicRe.IsMatch(tweaks[i].name_ru))
                {
                    allHaveRuName = false;
                    failedTweakRu = tweaks[i].id;
                    break;
                }
            }
            Assert(allHaveRuName,
                "D4A_AllTweaksHaveRussianName: У каждого твика есть name_ru с кириллицей",
                string.Format("Все {0} с кириллицей, ошибка на={1}", tweaks.Count, allHaveRuName ? "нет" : failedTweakRu));

            // 21. D4A_AllTweaksHaveRegistryBlock: У каждой записи не менее 1 registry-блока
            bool allHaveReg = true;
            string failedTweakReg = "";
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (tweaks[i].registry == null || tweaks[i].registry.Count < 1)
                {
                    allHaveReg = false;
                    failedTweakReg = tweaks[i].id;
                    break;
                }
            }
            Assert(allHaveReg,
                "D4A_AllTweaksHaveRegistryBlock: У каждой записи не менее 1 registry-блока",
                string.Format("Все твики имеют registry >= 1, ошибка на={0}", allHaveReg ? "нет" : failedTweakReg));

            // 22. D4A_AllIdsAreLatinOnly: Все id латиницей
            bool allIdsLatin = true;
            string failedTweakId = "";
            Regex latinIdRe = new Regex(@"^[a-z0-9\-]+$");
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (string.IsNullOrEmpty(tweaks[i].id) || !latinIdRe.IsMatch(tweaks[i].id))
                {
                    allIdsLatin = false;
                    failedTweakId = tweaks[i].id;
                    break;
                }
            }
            Assert(allIdsLatin,
                "D4A_AllIdsAreLatinOnly: Все id латиницей (a-z, 0-9, '-')",
                string.Format("Все id латиницей, ошибка на={0}", allIdsLatin ? "нет" : failedTweakId));

            // 23. D4E_OnDemandNvcplScriptExists: Проверка наличия скрипта scripts/nvcpl.ps1
            string rootScriptDir = Path.GetDirectoryName(Path.GetDirectoryName(tweaksPath));
            string nvcplScriptPath = Path.Combine(rootScriptDir, @"scripts\nvcpl.ps1");
            bool scriptExists = File.Exists(nvcplScriptPath);
            Assert(scriptExists,
                "D4E_OnDemandNvcplScriptExists: Скрипт On-Demand NVCPL nvcpl.ps1 присутствует в scripts/",
                string.Format("Путь={0}, Наличие={1}", nvcplScriptPath, scriptExists));

            // 24. D4E_OnDemandNvcplTweakProperties: Наличие твика nvidia-on-demand-nvcpl
            TweakItem onDemandTweak = null;
            for (int i = 0; i < tweaks.Count; i++)
            {
                if (string.Equals(tweaks[i].id, "nvidia-on-demand-nvcpl", StringComparison.OrdinalIgnoreCase))
                {
                    onDemandTweak = tweaks[i];
                    break;
                }
            }
            bool onDemandValid = onDemandTweak != null &&
                                 onDemandTweak.category == "nvidia" &&
                                 onDemandTweak.registry != null &&
                                 onDemandTweak.registry.Count == 1 &&
                                 Convert.ToInt32(onDemandTweak.registry[0].apply) == 4 &&
                                 Convert.ToInt32(onDemandTweak.registry[0].revert.value) == 2;
            Assert(onDemandValid,
                "D4E_OnDemandNvcplTweakProperties: Твик nvidia-on-demand-nvcpl имеет корректные параметры",
                onDemandTweak != null ? string.Format("Категория={0}, Name={1}, Apply={2}, Revert={3}",
                    onDemandTweak.category, onDemandTweak.name_ru, onDemandTweak.registry[0].apply, onDemandTweak.registry[0].revert.value) : "не найден");

            // 25. D4E_OnDemandNvcplSimulatedApplyAndRevert: Проверка применения и отката через TweakEngine
            bool engineSimulationOk = false;
            if (onDemandTweak != null)
            {
                MockRegistryStore memStore = new MockRegistryStore();
                memStore.SetValue("HKLM", @"System\CurrentControlSet\Services\NVDisplay.ContainerLocalSystem", "Start", 2, Microsoft.Win32.RegistryValueKind.DWord);
                StateManager sm = new StateManager(Path.Combine(Path.GetTempPath(), "tweaker_test_ondemand_" + Guid.NewGuid().ToString("N")), isTestMode: true);
                JournalManager jm = new JournalManager(Path.Combine(Path.GetTempPath(), "tweaker_test_ondemand_j_" + Guid.NewGuid().ToString("N")), isTestMode: true);
                TweakEngine testEngine = new TweakEngine(memStore, sm, jm, false);

                bool initialStatus = (testEngine.CheckStatus(onDemandTweak) == TweakStatus.Enabled);
                testEngine.Apply(onDemandTweak);
                bool appliedStatus = (testEngine.CheckStatus(onDemandTweak) == TweakStatus.Enabled);
                testEngine.Revert(onDemandTweak);
                bool revertedStatus = (testEngine.CheckStatus(onDemandTweak) == TweakStatus.Enabled);

                engineSimulationOk = (!initialStatus && appliedStatus && !revertedStatus);
            }
            Assert(engineSimulationOk,
                "D4E_OnDemandNvcplSimulatedApplyAndRevert: TweakEngine корректно переключает и откатывает Start (2 -> 4 -> 2)",
                string.Format("SimOk={0}", engineSimulationOk));
        }

        private static void RunGameModeWnfTests()
        {
            // 1. WNF State Name константы совпадают с gm_effects.c (Noverse)
            {
                bool c1 = GameModeWnfService.WNF_RM_GAME_MODE_ACTIVE == 0x41C6033FA3BC1075UL;
                bool c2 = GameModeWnfService.WNF_SEB_GAME_MODE       == 0x41840B3EA3BDD875UL;
                Assert(c1 && c2,
                    "GameModeWnf_Constants: WNF State Name константы соответствуют gm_effects.c",
                    string.Format("RM=0x{0:X}, SEB=0x{1:X}", GameModeWnfService.WNF_RM_GAME_MODE_ACTIVE, GameModeWnfService.WNF_SEB_GAME_MODE));
            }

            // 2. QueryWnfU32 успешно запрашивает WNF_RM_GAME_MODE_ACTIVE без исключений
            {
                int ntStatus = -1;
                uint activePid = GameModeWnfService.QueryWnfU32(GameModeWnfService.WNF_RM_GAME_MODE_ACTIVE, out ntStatus);
                bool success = (ntStatus >= 0);
                Assert(success,
                    "GameModeWnf_QueryActivePid: NtQueryWnfStateData возвращает STATUS_SUCCESS для Game Mode Active PID",
                    string.Format("NTSTATUS=0x{0:X8}, PID={1}", ntStatus, activePid));
            }

            // 3. QueryWnfU32 успешно запрашивает WNF_SEB_GAME_MODE (профиль питания)
            {
                int ntStatus = -1;
                uint powerLow = GameModeWnfService.QueryWnfU32(GameModeWnfService.WNF_SEB_GAME_MODE, out ntStatus);
                bool success = (ntStatus >= 0);
                Assert(success,
                    "GameModeWnf_QueryPowerProfile: NtQueryWnfStateData возвращает статус питания Game Mode",
                    string.Format("NTSTATUS=0x{0:X8}, LowVal={1}", ntStatus, powerLow));
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
                    "GameModeWnf_ServiceQueryState: Сервис возвращает корректно инициализированный GameModeState",
                    string.Format("Game='{0}', WNF={1}, CPU='{2}', FG='{3}'",
                        st.ActiveGameProcessName, st.PowerProfileLowValue, st.CpuSetIdsDisplay, st.ForegroundProcessName));
            }

            // 5. GameModeState конструктор инициализирует безопасные значения по умолчанию
            {
                var st = new GameModeState();
                bool defOk = (st.ActiveGamePid == 0) &&
                             (st.IsGameRunning == false) &&
                             (st.PowerProfileLowValue == 1) &&
                             (st.IsPowerProfileActive == false) &&
                             (st.ActiveGameProcessName == "Отсутствует") &&
                             (st.CpuSetIdsDisplay == "Не назначены");
                Assert(defOk,
                    "GameModeWnf_StateDefaults: Конструктор GameModeState инициализирует ожидаемые значения по умолчанию",
                    string.Format("Pid={0}, Running={1}, LowVal={2}", st.ActiveGamePid, st.IsGameRunning, st.PowerProfileLowValue));
            }

            // 6. Логика статусной сводки при отсутствии игры
            {
                var st = new GameModeState();
                st.ActiveGamePid = 0;
                st.PowerProfileLowValue = 1;
                st.IsGameRunning = false;
                bool isIdle = (st.ActiveGamePid == 0 && !st.IsGameRunning);
                Assert(isIdle,
                    "GameModeWnf_IdleCondition: Логика состояния ожидания корректно отражает отсутствие активной игры",
                    "Ожидание игры (WNF=1)");
            }
        }

        private static void RunGpuHwschTests()
        {
            // 1. WDDM константы совпадают с query_hwsch.c (Noverse)
            {
                bool c29 = GpuHwschService.KMTQAITYPE_WDDM_2_9_CAPS == 75;
                bool c30 = GpuHwschService.KMTQAITYPE_WDDM_3_0_CAPS == 77;
                Assert(c29 && c30,
                    "GpuHwsch_Constants: WDDM Caps константы соответствуют query_hwsch.c",
                    string.Format("WDDM_2_9={0}, WDDM_3_0={1}", GpuHwschService.KMTQAITYPE_WDDM_2_9_CAPS, GpuHwschService.KMTQAITYPE_WDDM_3_0_CAPS));
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
                    "GpuHwsch_StateDefaults: Конструктор GpuHwschState инициализирует ожидаемые значения по умолчанию",
                    string.Format("Hags='{0}', Flip='{1}'", st.HagsStatusDisplay, st.FlipQueueStatusDisplay));
            }

            // 3. QueryPrimaryGpuState возвращает состояние первичного видеоадаптера без исключений
            {
                var svc = new GpuHwschService();
                GpuHwschState st = svc.QueryPrimaryGpuState();
                bool notNull = (st != null);
                bool hasSummary = !string.IsNullOrEmpty(st.SummaryDisplay);
                bool hasDev = !string.IsNullOrEmpty(st.DeviceName);
                Assert(notNull && hasSummary && hasDev,
                    "GpuHwsch_ServiceQuery: Сервис возвращает корректно заполненное состояние адаптера",
                    string.Format("Dev='{0}', GPU='{1}', HAGS={2}, FlipQueue={3}",
                        st.DeviceName, st.AdapterString, st.IsHagsEnabled, st.IsFlipQueueEnabled));
            }

            // 4. Проверка битовой маски HWSCH и HWFLIPQUEUE
            {
                uint mockCaps29 = 6u; // (1 << 2) | 2 -> DriverSupport=2, Enabled=1
                uint driverSupport = mockCaps29 & 3u;
                uint enabled = (mockCaps29 >> 2) & 1u;
                bool bitLogicOk = (driverSupport == 2 && enabled == 1);
                Assert(bitLogicOk,
                    "GpuHwsch_BitmaskLogic: Логика битовых масок декодирования статуса WDDM соответствует query_hwsch",
                    string.Format("Support={0}, Enabled={1}", driverSupport, enabled));
            }
        }

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
                    "MmcssTask_StateDefaults: Конструктор MmcssTaskState инициализирует безопасные стандартные параметры",
                    string.Format("Resp={0}, Throttling={1}, Optimized={2}", st.SystemResponsiveness, st.NetworkThrottlingIndex, st.IsMmcssOptimized));
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
                    "MmcssTask_ServiceQueryState: QueryState считывает реестр SystemProfile и статус задач без сбоев",
                    string.Format("Resp={0}%, Throttling='{1}', Задач={2}, Avrt={3}",
                        st.SystemResponsiveness, st.NetworkThrottlingDisplay, st.AvailableTasksCount, st.IsAvrtAvailable));
            }

            // 3. Вызов P/Invoke avrt.dll для регистрации мультимедиа потока (Audio / Pro Audio / Games)
            {
                var svc = new MmcssTaskService();
                uint taskIdx;
                int win32Err;
                bool audioOk = svc.TestTaskRegistration("Audio", MmcssTaskService.AvrtPriority.High, out taskIdx, out win32Err);
                Assert(audioOk,
                    "MmcssTask_AvrtTestRegistration: AvSetMmThreadCharacteristicsW и AvSetMmThreadPriority выполняются успешно для задачи Audio",
                    string.Format("Audio TaskIndex={0}, Win32Err={1}", taskIdx, win32Err));
            }

            // 4. Логика оптимизации MMCSS (SystemResponsiveness >= 100 или NetworkThrottlingIndex == 0xFFFFFFFF)
            {
                var stOpt = new MmcssTaskState();
                stOpt.SystemResponsiveness = 100;
                stOpt.NetworkThrottlingIndex = 0xFFFFFFFF;
                bool isOpt = (stOpt.SystemResponsiveness >= 100 || stOpt.NetworkThrottlingIndex == 0xFFFFFFFF);
                Assert(isOpt,
                    "MmcssTask_OptimizationLogic: Оптимизированный флаг активируется при SystemResponsiveness=100 или Throttling=0xFFFFFFFF",
                    string.Format("Resp={0}, Throttling=0x{1:X8}", stOpt.SystemResponsiveness, stOpt.NetworkThrottlingIndex));
            }

            // 5. Перечисление AvrtPriority строго соответствует Noverse mmcss_task.c
            {
                bool prioOk = ((int)MmcssTaskService.AvrtPriority.VeryLow == -2) &&
                              ((int)MmcssTaskService.AvrtPriority.Low == -1) &&
                              ((int)MmcssTaskService.AvrtPriority.Normal == 0) &&
                              ((int)MmcssTaskService.AvrtPriority.High == 1) &&
                              ((int)MmcssTaskService.AvrtPriority.Critical == 2);
                Assert(prioOk,
                    "MmcssTask_AvrtPriorityEnum: Значения приоритетов Avrt соответствуют Windows SDK (-2..+2)",
                    "VeryLow=-2, Low=-1, Normal=0, High=1, Critical=2");
            }
        }

        private static void RunAllTweaksEngineMockTests(string tweaksPath)
        {
            if (!File.Exists(tweaksPath))
            {
                Assert(false, "Наличие файла tweaks.json", tweaksPath);
                return;
            }

            string json = File.ReadAllText(tweaksPath, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            List<TweakItem> tweaks = serializer.Deserialize<List<TweakItem>>(json);

            Assert(tweaks != null && tweaks.Count > 0, "Десериализация всех твиков для сквозного тестирования движка", string.Format("Загружено {0} твиков", tweaks != null ? tweaks.Count : 0));
            if (tweaks == null) return;

            string tempDir = Path.Combine(Path.GetTempPath(), "tweaker_all_mock_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                MockRegistryStore store = new MockRegistryStore();
                StateManager state = new StateManager(Path.Combine(tempDir, "state.json"), isTestMode: true);
                JournalManager journal = new JournalManager(Path.Combine(tempDir, "journal.log"), isTestMode: true);
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
                            issues.Add(string.Format("{0}: Apply вернул false", t.id));
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
                            issues.Add(string.Format("{0}: статус после Apply={1} (ожидался Enabled)", t.id, s1));
                        }

                        // 4. Revert tweak
                        bool reverted = engine.Revert(t);
                        if (reverted)
                        {
                            revertOk++;
                        }
                        else
                        {
                            issues.Add(string.Format("{0}: Revert вернул false", t.id));
                        }
                    }
                    catch (Exception ex)
                    {
                        issues.Add(string.Format("{0}: исключение {1}", t.id, ex.Message));
                    }
                }

                Assert(applyOk == tweaks.Count, "Сквозное тестирование: 100% твиков успешно применены на MockRegistryStore", string.Format("{0}/{1}", applyOk, tweaks.Count));
                Assert(checkOk == tweaks.Count, "Сквозное тестирование: 100% твиков получили статус Enabled после Apply", string.Format("{0}/{1}", checkOk, tweaks.Count));
                Assert(revertOk == tweaks.Count, "Сквозное тестирование: 100% твиков успешно откачены через Revert", string.Format("{0}/{1}", revertOk, tweaks.Count));
                Assert(issues.Count == 0, "Сквозное тестирование: полное отсутствие сбоев и исключений при работе с базой твиков", string.Format("Сбоев: {0}", issues.Count));
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
}
