package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
)

func main() {
	fpath := `C:\Users\User\Documents\win-tweaker\data\tweaks.json`
	data, _ := ioutil.ReadFile(fpath)

	var rawList []map[string]interface{}
	json.Unmarshal(data, &rawList)

	for _, item := range rawList {
		id, _ := item["id"].(string)
		src, _ := item["source"].(map[string]interface{})
		if id == "security-vbs-hvci-disable" {
			src["file"] = "security/desc.md"
			src["line_from"] = 986
			src["line_to"] = 1030
		} else if id == "security-tdr-delay" {
			src["file"] = "security/desc.md"
			src["line_from"] = 1177
			src["line_to"] = 1230
		} else if id == "system-delivery-optimization-p2p" {
			src["file"] = "security/desc.md"
			src["line_from"] = 1069
			src["line_to"] = 1097
		} else if id == "system-disable-paging-executive" {
			src["file"] = "system/desc.md"
			src["line_from"] = 3540
			src["line_to"] = 3560
		}
	}

	out, _ := json.MarshalIndent(rawList, "", "  ")
	ioutil.WriteFile(fpath, out, 0644)
	fmt.Println("Fixed source ranges in data/tweaks.json")
}
