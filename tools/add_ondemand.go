package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
)

type RevertSpec struct {
	Mode  string      `json:"mode"`
	Value interface{} `json:"value,omitempty"`
}

type RegistryAction struct {
	Hive       string      `json:"hive"`
	Path       string      `json:"path"`
	Name       string      `json:"name"`
	Type       string      `json:"type"`
	Apply      interface{} `json:"apply"`
	ApplyBlock string      `json:"apply_block"`
	Revert     RevertSpec  `json:"revert"`
}

type SourceRef struct {
	File     string `json:"file"`
	LineFrom int    `json:"line_from"`
	LineTo   int    `json:"line_to"`
}

type TweakItem struct {
	ID            string           `json:"id"`
	NameRu        string           `json:"name_ru"`
	DescriptionRu string           `json:"description_ru"`
	Category      string           `json:"category"`
	Group         string           `json:"group"`
	Type          string           `json:"type"`
	Risk          string           `json:"risk"`
	Presets       []string         `json:"presets"`
	Registry      []RegistryAction `json:"registry"`
	Source        SourceRef        `json:"source"`
	MinBuild      *int             `json:"min_build"`
	NeedsReview   bool             `json:"needs_review"`
}

func main() {
	raw, err := ioutil.ReadFile("data/tweaks.json")
	if err != nil {
		panic(err)
	}

	var tweaks []TweakItem
	if err := json.Unmarshal(raw, &tweaks); err != nil {
		panic(err)
	}

	for _, t := range tweaks {
		if t.ID == "nvidia-on-demand-nvcpl" {
			fmt.Println("Already present")
			return
		}
	}

	newTweak := TweakItem{
		ID:            "nvidia-on-demand-nvcpl",
		NameRu:        "Запуск Панели NVIDIA по требованию (On-Demand NVCPL)",
		DescriptionRu: "Отключает постоянную фоновую службу NVDisplay.ContainerLocalSystem. Создает лончер On-Demand (скрипт nvcpl.ps1 от Noverse): служба запускается только при открытии панели управления и выгружается сразу после ее закрытия.",
		Category:      "nvidia",
		Group:         "Службы и телеметрия",
		Type:          "toggle",
		Risk:          "low",
		Presets:       []string{},
		Registry: []RegistryAction{
			{
				Hive:       "HKLM",
				Path:       "System\\CurrentControlSet\\Services\\NVDisplay.ContainerLocalSystem",
				Name:       "Start",
				Type:       "REG_DWORD",
				Apply:      4,
				ApplyBlock: "Enabled",
				Revert: RevertSpec{
					Mode:  "set",
					Value: 2,
				},
			},
		},
		Source: SourceRef{
			File:     "nvidia/desc.md",
			LineFrom: 1188,
			LineTo:   1192,
		},
		MinBuild:    nil,
		NeedsReview: false,
	}

	tweaks = append(tweaks, newTweak)

	out, err := json.MarshalIndent(tweaks, "", "  ")
	if err != nil {
		panic(err)
	}

	if err := ioutil.WriteFile("data/tweaks.json", out, 0644); err != nil {
		panic(err)
	}

	fmt.Printf("Successfully added nvidia-on-demand-nvcpl with empty presets. Total tweaks: %d\n", len(tweaks))
}
