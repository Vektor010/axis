package main

import (
	"fmt"
	"io/ioutil"
	"os"
	"strings"
)

func replaceInFile(fpath, oldStr, newStr string) error {
	data, err := ioutil.ReadFile(fpath)
	if err != nil {
		return err
	}
	content := string(data)
	// normalize target search by trying with whatever CRLF/LF is in file
	if !strings.Contains(content, oldStr) {
		// try replacing LF with CRLF
		oldStrCRLF := strings.ReplaceAll(oldStr, "\r\n", "\n")
		oldStrCRLF = strings.ReplaceAll(oldStrCRLF, "\n", "\r\n")
		if strings.Contains(content, oldStrCRLF) {
			oldStr = oldStrCRLF
			newStr = strings.ReplaceAll(newStr, "\r\n", "\n")
			newStr = strings.ReplaceAll(newStr, "\n", "\r\n")
		} else {
			// try LF only
			oldStrLF := strings.ReplaceAll(oldStr, "\r\n", "\n")
			if strings.Contains(content, oldStrLF) {
				oldStr = oldStrLF
				newStr = strings.ReplaceAll(newStr, "\r\n", "\n")
			} else {
				return fmt.Errorf("target string not found in %s", fpath)
			}
		}
	}
	content = strings.Replace(content, oldStr, newStr, 1)
	return ioutil.WriteFile(fpath, []byte(content), 0644)
}

func main() {
	if len(os.Args) < 4 {
		fmt.Println("Usage: go run updater.go <file> <oldFile> <newFile>")
		os.Exit(1)
	}
	fpath := os.Args[1]
	oldBytes, err := ioutil.ReadFile(os.Args[2])
	if err != nil {
		fmt.Printf("Error reading oldFile: %v\n", err)
		os.Exit(1)
	}
	newBytes, err := ioutil.ReadFile(os.Args[3])
	if err != nil {
		fmt.Printf("Error reading newFile: %v\n", err)
		os.Exit(1)
	}
	err = replaceInFile(fpath, string(oldBytes), string(newBytes))
	if err != nil {
		fmt.Printf("Error: %v\n", err)
		os.Exit(1)
	}
	fmt.Printf("Successfully updated %s\n", fpath)
}
