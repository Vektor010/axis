package main

import (
	"bufio"
	"fmt"
	"os"
	"strings"
)

func search(fpath, pattern string) {
	f, err := os.Open(fpath)
	if err != nil {
		fmt.Printf("Error: %v\n", err)
		return
	}
	defer f.Close()
	scanner := bufio.NewScanner(f)
	lineNum := 0
	for scanner.Scan() {
		lineNum++
		line := scanner.Text()
		if strings.Contains(strings.ToLower(line), strings.ToLower(pattern)) {
			fmt.Printf("%s:%d %s\n", fpath, lineNum, line)
		}
	}
}

func main() {
	search(`C:\Users\User\Documents\win-tweaker\data\source\win-config\security\desc.md`, "EnableVirtualizationBasedSecurity")
	search(`C:\Users\User\Documents\win-tweaker\data\source\win-config\security\desc.md`, "TdrDelay")
	search(`C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md`, "DODownloadMode")
	search(`C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md`, "DisablePagingExecutive")
}
