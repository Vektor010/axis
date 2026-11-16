package main

import (
	"fmt"
	"io/ioutil"
	"strings"
)

func main() {
	b, _ := ioutil.ReadFile(`C:\Users\User\Documents\win-tweaker\MainWindow.xaml`)
	lines := strings.Split(string(b), "\n")
	for i := 1210; i < 1270 && i < len(lines); i++ {
		fmt.Printf("%d: %q\n", i+1, lines[i])
	}
}
