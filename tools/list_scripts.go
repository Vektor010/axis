package main

import (
	"bufio"
	"fmt"
	"os"
	"strings"
)

func main() {
	files := []string{
		`C:\Users\User\Documents\оптимизация\scripts\Security-Tweaks.ps1`,
		`C:\Users\User\Documents\оптимизация\scripts\System-Tweaks.ps1`,
		`C:\Users\User\Documents\оптимизация\scripts\Visibility-Tweaks.ps1`,
		`C:\Users\User\Documents\оптимизация\scripts\Privacy-Tweaks.ps1`,
		`C:\Users\User\Documents\оптимизация\scripts\Network-Tweaks.ps1`,
	}

	for _, fpath := range files {
		f, err := os.Open(fpath)
		if err != nil {
			fmt.Printf("Error opening %s: %v\n", fpath, err)
			continue
		}
		fmt.Printf("\n=== FILE: %s ===\n", fpath)
		scanner := bufio.NewScanner(f)
		for scanner.Scan() {
			line := scanner.Text()
			if strings.HasPrefix(line, "# Название:") || strings.HasPrefix(line, "Write-Host \"[") {
				fmt.Println(line)
			}
		}
		f.Close()
	}
}
