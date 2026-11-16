package main

import (
	"bytes"
	"fmt"
	"io/ioutil"
	"strings"
)

func main() {
	content, err := ioutil.ReadFile("Tests/Program.cs")
	if err != nil {
		panic(err)
	}

	target := "string.Format(\"Все id латиницей, ошибка на={0}\", allIdsLatin ? \"нет\" : failedTweakId));"
	idx := strings.Index(string(content), target)
	if idx == -1 {
		panic("target not found")
	}

	insertion := `

            // 36. D4E_OnDemandNvcplScriptExists: Проверка наличия скрипта scripts/nvcpl.ps1
            string rootScriptDir = Path.GetDirectoryName(Path.GetDirectoryName(tweaksPath));
            string nvcplScriptPath = Path.Combine(rootScriptDir, @"scripts\nvcpl.ps1");
            bool scriptExists = File.Exists(nvcplScriptPath);
            Assert(scriptExists,
                "D4E_OnDemandNvcplScriptExists: Скрипт On-Demand NVCPL nvcpl.ps1 присутствует в scripts/",
                string.Format("Путь={0}, Наличие={1}", nvcplScriptPath, scriptExists));

            // 37. D4E_OnDemandNvcplTweakProperties: Наличие твика nvidia-on-demand-nvcpl
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

            // 38. D4E_OnDemandNvcplSimulatedApplyAndRevert: Проверка применения и отката через TweakEngine
            bool engineSimulationOk = false;
            if (onDemandTweak != null)
            {
                InMemoryRegistryStore memStore = new InMemoryRegistryStore();
                memStore.SetValue("HKLM", @"System\CurrentControlSet\Services\NVDisplay.ContainerLocalSystem", "Start", 2, Microsoft.Win32.RegistryValueKind.DWord);
                StateManager sm = new StateManager(Path.Combine(Path.GetTempPath(), "tweaker_test_ondemand_" + Guid.NewGuid().ToString("N")));
                JournalManager jm = new JournalManager(Path.Combine(Path.GetTempPath(), "tweaker_test_ondemand_j_" + Guid.NewGuid().ToString("N")));
                TweakEngine testEngine = new TweakEngine(memStore, sm, jm, false);

                bool initialStatus = testEngine.Check(onDemandTweak);
                testEngine.Apply(onDemandTweak);
                bool appliedStatus = testEngine.Check(onDemandTweak);
                testEngine.Revert(onDemandTweak);
                bool revertedStatus = testEngine.Check(onDemandTweak);

                engineSimulationOk = (!initialStatus && appliedStatus && !revertedStatus);
            }
            Assert(engineSimulationOk,
                "D4E_OnDemandNvcplSimulatedApplyAndRevert: TweakEngine корректно переключает и откатывает Start (2 -> 4 -> 2)",
                string.Format("SimOk={0}", engineSimulationOk));`

	// Ensure CRLF
	insertion = strings.Replace(insertion, "\r\n", "\n", -1)
	insertion = strings.Replace(insertion, "\n", "\r\n", -1)

	var buf bytes.Buffer
	buf.Write(content[:idx+len(target)])
	buf.WriteString(insertion)
	buf.Write(content[idx+len(target):])

	// Ensure BOM
	res := buf.Bytes()
	if !bytes.HasPrefix(res, []byte{0xEF, 0xBB, 0xBF}) {
		res = append([]byte{0xEF, 0xBB, 0xBF}, res...)
	}

	if err := ioutil.WriteFile("Tests/Program.cs", res, 0644); err != nil {
		panic(err)
	}

	fmt.Println("Tests/Program.cs updated with BOM and CRLF")
}
