package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
)

func main() {
	fpath := `C:\Users\User\Documents\win-tweaker\data\tweaks.json`
	data, err := ioutil.ReadFile(fpath)
	if err != nil {
		panic(err)
	}

	var rawList []map[string]interface{}
	err = json.Unmarshal(data, &rawList)
	if err != nil {
		panic(err)
	}

	recommendedIDs := map[string]bool{
		// 5 UI Animation tweaks
		"visibility-dwm-disallow-animations":         true,
		"visibility-disable-min-animate":             true,
		"visibility-taskbar-animations":              true,
		"visibility-turn-off-spi-animations":         true,
		"visibility-visual-effects-best-performance": true,
		// Safe visibility / explorer
		"visibility-show-cloud-files-in-quick-access": true,
		"visibility-hide-file-ext":                    true,
		// NVIDIA performance & latency
		"nvidia-hardware-accelerated-gpu-scheduling": true,
		"nvidia-low-latency-prerender-limit":         true,
		"nvidia-powermizer-maximum-performance":      true,
		"nvidia-disable-telemetry-service":           true,
		"nvidia-physx-force-gpu":                     true,
		"nvidia-disable-gfe-overlay":                 true,
		"nvidia-disable-dynamic-pstate":              true,
		"nvidia-noverse-minimal-profile":             true,
		"nvidia-on-demand-nvcpl":                     true,
		// Steam latency
		"steam-disable-game-overlay":               true,
		"steam-disable-gpu-accel-webviews":         true,
		"steam-disable-smooth-scroll":              true,
		"steam-disable-dwrite-font-smoothing":      true,
		"steam-disable-downloads-during-gameplay":  true,
		// Security / Core
		"security-dynamic-lock": true,
	}

	assigned := 0
	for _, item := range rawList {
		id, _ := item["id"].(string)
		if recommendedIDs[id] {
			item["presets"] = []string{"recommended"}
			assigned++
		}
	}

	outData, err := json.MarshalIndent(rawList, "", "  ")
	if err != nil {
		panic(err)
	}

	err = ioutil.WriteFile(fpath, outData, 0644)
	if err != nil {
		panic(err)
	}

	fmt.Printf("Successfully assigned recommended preset to %d tweaks in %s\n", assigned, fpath)
}
