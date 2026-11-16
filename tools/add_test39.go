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

	target := "string.Format(\"SimOk={0}\", engineSimulationOk));"
	idx := strings.Index(string(content), target)
	if idx == -1 {
		panic("target not found")
	}

	insertion := `

            // 39. D4G_SteamOverlayVdfDetection: Проверка распознавания настройки оверлея в localconfig.vdf
            string steamPath = SteamConfigHelper.GetSteamPath();
            int? overlaySetting = SteamConfigHelper.GetGameOverlaySetting();
            bool steamDetectionOk = (steamPath != null && overlaySetting.HasValue);
            Assert(steamDetectionOk,
                "D4G_SteamOverlayVdfDetection: Распознавание EnableGameOverlay в localconfig.vdf",
                string.Format("SteamPath={0}, EnableGameOverlay={1}", steamPath, overlaySetting));`

	insertion = strings.Replace(insertion, "\r\n", "\n", -1)
	insertion = strings.Replace(insertion, "\n", "\r\n", -1)

	var buf bytes.Buffer
	buf.Write(content[:idx+len(target)])
	buf.WriteString(insertion)
	buf.Write(content[idx+len(target):])

	res := buf.Bytes()
	if !bytes.HasPrefix(res, []byte{0xEF, 0xBB, 0xBF}) {
		res = append([]byte{0xEF, 0xBB, 0xBF}, res...)
	}

	if err := ioutil.WriteFile("Tests/Program.cs", res, 0644); err != nil {
		panic(err)
	}

	fmt.Println("Test 39 successfully inserted into Tests/Program.cs")
}
