package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
)

func main() {
	data, _ := ioutil.ReadFile(`C:\Users\User\Documents\win-tweaker\data\tweaks.json`)
	var rawList []map[string]interface{}
	json.Unmarshal(data, &rawList)
	for i, item := range rawList {
		fmt.Printf("%2d: %-45s | %-12s | %s\n", i+1, item["id"], item["category"], item["name_ru"])
	}
}
